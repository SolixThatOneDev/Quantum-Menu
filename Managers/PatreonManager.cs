/*
 * Quantum Menu  Managers/PatreonManager.cs
 * A community driven mod menu for Gorilla Tag with over 1000+ mods
 *
 * Copyright (C) 2026  Quantum Software
 * https://github.com/Quantum/Quantum-Menu
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */

using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using Quantum.Classes.Menu;
using Quantum.Extensions;
using Quantum.Menu;
using Quantum.Mods;
using Quantum.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using static Quantum.Utilities.AssetUtilities;
using static Quantum.Utilities.RigUtilities;

namespace Quantum.Managers
{
    public class PatreonManager : MonoBehaviour
    {
        public static PatreonManager instance = null;

        public void Awake()
        {
            instance = this;
            PhotonNetwork.NetworkingClient.EventReceived += EventReceived;
        }

        public readonly Dictionary<string, PatreonMembership> PatreonMembers = new Dictionary<string, PatreonMembership>();
        public readonly struct PatreonMembership
        {
            public readonly string TierName;
            public readonly string IconURL;

            public PatreonMembership(string tierName, string iconURL)
            {
                TierName = tierName;
                IconURL = iconURL;
            }
        }

        private Material iconMaterial;
        private readonly Dictionary<VRRig, GameObject> iconPool = new Dictionary<VRRig, GameObject>();
        private readonly Dictionary<VRRig, GameObject> menuPool = new Dictionary<VRRig, GameObject>();
        private static readonly List<Player> excludedIndicators = new List<Player>();

        public static KeyValuePair<NetPlayer, PatreonMembership>[] GetAllMembersInRoom()
        {
            return !NetworkSystem.Instance.InRoom
                ? Array.Empty<KeyValuePair<NetPlayer, PatreonMembership>>()
                : NetworkSystem.Instance.PlayerListOthers
                .Where(player => instance.PatreonMembers.ContainsKey(player.UserId))
                .Select(player => new KeyValuePair<NetPlayer, PatreonMembership>(player, instance.PatreonMembers[player.UserId]))
                .ToArray();
        }

        public static bool IsPlayerPatreonMember(NetPlayer player) =>
            instance.PatreonMembers.ContainsKey(player.UserId);

        public static Color GetTierColor(string color) // Hard coded slop my beloved
        {
            return color switch
            {
                "Donor" => new Color32(196, 201, 200, 255),
                "Supporter" => new Color32(241, 196, 15, 255),
                "Basic Tracker" => new Color32(189, 221, 244, 255),
                "Ultimate Tracker" => new Color32(170, 184, 194, 255),

                "Owner" => new Color32(108, 190, 127, 255),
                "Co-Owner" => new Color32(73, 143, 214, 255),
                "Console Owner" => new Color32(189, 96, 231, 255),
                "Menu Developer" => new Color32(212, 132, 61, 255),
                "Admin" => new Color32(255, 110, 118, 255),
                "Staff Manager" => new Color32(102, 241, 180, 255),
                "Moderator" => new Color32(88, 101, 242, 255),
                "Community Helper" => new Color32(253, 215, 101, 255),

                "Boyfriend" => new Color32(244, 171, 186, 255),

                _ => Color.white,
            };
        }

        public static bool IndicatorsEnabled = true;
        public void Update()
        {
            List<VRRig> toRemoveRigs = new List<VRRig>();

            foreach (var indicator in iconPool.Where(indicator => !IndicatorsEnabled || !indicator.Key.Active() || !IsPlayerPatreonMember(GetPlayerFromVRRig(indicator.Key)) || excludedIndicators.Contains(indicator.Key.GetPhotonPlayer())))
            {
                toRemoveRigs.Add(indicator.Key);
                Destroy(indicator.Value);
            }

            foreach (VRRig rig in toRemoveRigs)
            {
                iconPool.Remove(rig);
                if (menuPool.TryGetValue(rig, out GameObject menu))
                {
                    Destroy(menu);
                    menuPool.Remove(rig);
                }
            }

            if (!IndicatorsEnabled) return;

            if (!NetworkSystem.Instance.InRoom) return;
            var members = GetAllMembersInRoom();
            foreach (var member in members)
            {
                VRRig playerRig = GetVRRigFromPlayer(member.Key);
                if (playerRig == null) continue;
                if (excludedIndicators.Contains(member.Key.GetPlayer())) continue;

                if (!iconPool.TryGetValue(playerRig, out GameObject playerIndicator))
                {
                    playerIndicator = GameObject.CreatePrimitive(PrimitiveType.Quad);
                    Destroy(playerIndicator.GetComponent<Collider>());

                    if (iconMaterial == null)
                    {
                        iconMaterial = new Material(Shader.Find("Sprites/Default"));
                    }

                    playerIndicator.GetComponent<Renderer>().material = new Material(iconMaterial);
                    AssetUtilities.LoadTextureFromURL(member.Value.IconURL, $"Images/Patreon/{member.Key.UserId}.{FileUtilities.GetFileExtension(member.Value.IconURL)}", playerIndicator.GetComponent<Renderer>());
                    playerIndicator.GetComponent<Renderer>().material.color = Color.white;

                    // Parent to the head transform for smooth movement
                    Transform head = Visuals.GetNameTagTransform(playerRig);
                    playerIndicator.transform.SetParent(head, false);
                    playerIndicator.transform.localPosition = new Vector3(0, 0.4f, 0);

                    GameObject go = new GameObject("Quantum_Nametag");
                    go.transform.localScale = new Vector3(0.25f, 0.25f, 0.25f);
                    TextMeshPro textMesh = go.AddComponent<TextMeshPro>();
                    textMesh.fontSize = 4.8f;
                    textMesh.alignment = TextAlignmentOptions.Center;

                    try
                    {
                        if (member.Value.TierName == "Owner")
                        {
                            Renderer rend = playerIndicator.GetComponent<Renderer>();
                            if (rend != null) rend.enabled = false;

                            textMesh.text = "OWNER";
                            textMesh.enableVertexGradient = true;
                            textMesh.colorGradient = new VertexGradient(Color.black, new Color(0.7f, 0.7f, 0.7f), Color.black, new Color(0.7f, 0.7f, 0.7f));
                            
                            Shader targetShader = Shader.Find("GUI/Text Shader");
                            if (targetShader != null)
                                textMesh.fontMaterial.shader = targetShader;
                        }
                        else
                        {
                            textMesh.SafeSetText(member.Value.TierName);
                            textMesh.color = GetTierColor(member.Value.TierName);
                        }
                    }
                    catch (Exception ex)
                    {
                        Quantum.Mods.Console._v3_out_("Failed to setup owner tag: " + ex.Message);
                        textMesh.SafeSetText(member.Value.TierName);
                    }
                    textMesh.SafeSetFontStyle(Main.activeFontStyle);
                    textMesh.SafeSetFont(Main.activeFont);
                    textMesh.transform.localScale = new Vector3(0.4f, 0.4f, 0.4f);
                    textMesh.transform.SetParent(playerIndicator.transform, false);

                    iconPool.Add(playerRig, playerIndicator);
                }

                // Rock-solid billboarding logic (No Tilt)
                if (playerIndicator != null)
                {
                    // Match camera rotation for a perfect 2D billboard effect
                    playerIndicator.transform.rotation = Camera.main.transform.rotation;
                    // Quad correction (Quads face -Z, so rotate 180 on Y if needed, 
                    // but Camera.main.rotation usually points at the camera, so we rotate to face us)
                    playerIndicator.transform.Rotate(0f, 180f, 0f);
                    
                    GameObject nameTag = playerIndicator.transform.Find("Quantum_Nametag")?.gameObject;
                    if (nameTag != null)
                    {
                        nameTag.transform.localPosition = new Vector3(0, 0.25f, 0);
                        nameTag.transform.rotation = Camera.main.transform.rotation;
                    }
                }

                // Synced Menu Visibility Logic
                try 
                {
                    object menuOpenProp;
                    member.Key.GetCustomProperties().TryGetValue("QuantumMenuOpen", out menuOpenProp);
                    bool isMenuOpen = menuOpenProp != null && (bool)menuOpenProp;

                    if (isMenuOpen)
                    {
                        if (!menuPool.ContainsKey(playerRig))
                        {
                            // 1. Base Menu Object (Invisible Container)
                            GameObject syncMenu = new GameObject("Quantum_GhostMenu");
                            syncMenu.transform.SetParent(playerRig.leftHandTransform, false);
                            syncMenu.transform.localPosition = Vector3.zero;
                            syncMenu.transform.localRotation = Quaternion.Euler(0, 90, 90);
                            syncMenu.transform.localScale = new Vector3(0.1f, 0.3f, 0.3825f);
                            
                            // 2. Menu Background (The Main Panel)
                            GameObject background = GameObject.CreatePrimitive(PrimitiveType.Cube);
                            Destroy(background.GetComponent<BoxCollider>());
                            background.transform.SetParent(syncMenu.transform, false);
                            background.transform.localPosition = new Vector3(0.50f, 0f, 0f);
                            background.transform.localScale = new Vector3(0.1f, 1.5f, 1f);
                            background.GetComponent<Renderer>().material.color = new Color(0.1f, 0.1f, 0.1f, 0.95f);

                            // 3. Outlines (Frames)
                            float xSize = 1.01f; float thickness = 0.01f;
                            Vector3[] outlinePos = { new Vector3(0f, 0.5f, 0f), new Vector3(0f, -0.5f, 0f), new Vector3(0f, 0f, 0.5f), new Vector3(0f, 0f, -0.5f) };
                            Vector3[] outlineScale = { new Vector3(xSize, thickness, 1f), new Vector3(xSize, thickness, 1f), new Vector3(xSize, 1.01f, thickness), new Vector3(xSize, 1.01f, thickness) };
                            for (int i = 0; i < 4; i++)
                            {
                                GameObject outline = GameObject.CreatePrimitive(PrimitiveType.Cube);
                                Destroy(outline.GetComponent<BoxCollider>());
                                outline.transform.SetParent(background.transform, false);
                                outline.transform.localPosition = outlinePos[i];
                                outline.transform.localScale = outlineScale[i];
                                outline.GetComponent<Renderer>().material.color = Color.cyan; // Standard Quantum Theme
                            }

                            // 4. Disconnect Button (Top Piece)
                            GameObject dcBtn = GameObject.CreatePrimitive(PrimitiveType.Cube);
                            Destroy(dcBtn.GetComponent<BoxCollider>());
                            dcBtn.transform.SetParent(syncMenu.transform, false);
                            dcBtn.transform.localScale = new Vector3(0.09f, 0.9f, 0.08f);
                            dcBtn.transform.localPosition = new Vector3(0.56f, 0f, 0.43f);
                            dcBtn.GetComponent<Renderer>().material.color = Color.red;

                            // 5. Canvas for Text
                            GameObject canvasObj = new GameObject("Canvas");
                            canvasObj.transform.SetParent(syncMenu.transform, false);
                            Canvas canvas = canvasObj.AddComponent<Canvas>();
                            canvas.renderMode = RenderMode.WorldSpace;
                            canvasObj.transform.localScale = Vector3.one;

                            // 6. Title Text
                            GameObject titleObj = new GameObject("Title");
                            titleObj.transform.SetParent(canvasObj.transform, false);
                            TextMeshPro title = titleObj.AddComponent<TextMeshPro>();
                            title.text = "Quantum Sync";
                            title.fontSize = 0.8f;
                            title.alignment = TextAlignmentOptions.Center;
                            title.transform.localPosition = new Vector3(0.06f, 0f, 0.165f);
                            title.transform.localRotation = Quaternion.Euler(180, 90, 90);

                            // 7. Ghost Buttons (Rendering 6 mod slots)
                            for (int i = 0; i < 6; i++)
                            {
                                float offset = (i + 1) * 0.11f;
                                GameObject btn = GameObject.CreatePrimitive(PrimitiveType.Cube);
                                Destroy(btn.GetComponent<BoxCollider>());
                                btn.transform.SetParent(syncMenu.transform, false);
                                btn.transform.localScale = new Vector3(0.09f, 1.3f, 0.08f);
                                btn.transform.localPosition = new Vector3(0.56f, 0f, 0.28f - offset);
                                btn.GetComponent<Renderer>().material.color = new Color(0.2f, 0.2f, 0.2f);
                            }

                            menuPool.Add(playerRig, syncMenu);
                        }
                    }
                    else if (menuPool.TryGetValue(playerRig, out GameObject existingMenu))
                    {
                        Destroy(existingMenu);
                        menuPool.Remove(playerRig);
                    }
                }
                catch { }
            }
        }

        public const byte PatreonByte = 63;
        public static void EventReceived(EventData data)
        {
            try
            {
                NetPlayer sender = PhotonNetwork.NetworkingClient.CurrentRoom.GetPlayer(data.Sender);
                if (data.Code != PatreonByte || !IsPlayerPatreonMember(sender)) return;
                VRRig senderRig = GetVRRigFromPlayer(sender);
                object[] args = data.CustomData == null ? new object[] { } : (object[])data.CustomData;
                string command = args.Length > 0 ? (string)args[0] : "";

                switch (command)
                {
                    case "indicator":
                        {
                            if (args.Length > 1 && args[1] is bool enabled)
                            {
                                if (enabled)
                                    if (!excludedIndicators.Contains(sender.GetPlayer()))
                                        excludedIndicators.Add(sender.GetPlayer());
                                    else
                                    if (excludedIndicators.Contains(sender.GetPlayer()))
                                        excludedIndicators.Remove(sender.GetPlayer());
                            }
                            break;
                        }
                }
            }
            catch { }
        }

        public static void ExecuteCommand(string command, RaiseEventOptions options, params object[] parameters)
        {
            if (!NetworkSystem.Instance.InRoom)
                return;

            PhotonNetwork.RaiseEvent(PatreonByte,
                new object[] { command }
                    .Concat(parameters)
                    .ToArray(),
            options, SendOptions.SendReliable);
        }

        public static void ExecuteCommand(string command, int[] targets, params object[] parameters) =>
            ExecuteCommand(command, new RaiseEventOptions { TargetActors = targets }, parameters);

        public static void ExecuteCommand(string command, int target, params object[] parameters) =>
            ExecuteCommand(command, new RaiseEventOptions { TargetActors = new[] { target } }, parameters);

        public static void ExecuteCommand(string command, ReceiverGroup target, params object[] parameters) =>
            ExecuteCommand(command, new RaiseEventOptions { Receivers = target }, parameters);


        #region Patreon Mods
        public static void SetupPatreonMods(string patreonName)
        {
            NotificationManager._v3_msg_($"<color=grey>[</color><color=purple>PATREON</color><color=grey>]</color> Welcome, {patreonName}! Patreon mods have been enabled.", 10000);

            List<ButtonInfo> buttons = Buttons.buttons[Buttons.GetCategory("Main")].ToList();
            buttons.Add(new ButtonInfo { buttonText = "Patreon Mods", method = () => Buttons.CurrentCategoryName = "Patreon Mods", isTogglable = false, toolTip = "Opens the patreon mods." });
            Buttons.buttons[Buttons.GetCategory("Main")] = buttons.ToArray();

            if (Main.dynamicSounds)
            {
                LoadSoundFromURL($"{PluginInfo.ServerResourcePath}/Audio/Menu/patreon.ogg", "Audio/Menu/patreon.ogg", clip =>
                {
                    clip?.Play(Main.buttonClickVolume / 10f);
                });
            }
        }

        public static void ShowIndicator(bool enabled) =>
            ExecuteCommand("indicator", ReceiverGroup.All, enabled);

        private static int lastPlayerCount;
        public static void ConstantHideIndicator()
        {
            if (!PhotonNetwork.InRoom)
                lastPlayerCount = -1;

            if (PhotonNetwork.PlayerList.Length != lastPlayerCount && PhotonNetwork.InRoom)
            {
                ShowIndicator(false);
                lastPlayerCount = PhotonNetwork.PlayerList.Length;
            }
        }
        #endregion
    }
}



