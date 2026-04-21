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
            // Universal Player Processing Loop (Modders + Patreon)
            foreach (NetPlayer player in NetworkSystem.Instance.AllNetPlayers)
            {
                try
                {
                    // if (player.IsLocal) continue;
                    VRRig playerRig = player.VRRig();
                    if (playerRig == null) continue;

                    // --- 1. Universal Patreon & Tag Logic ---
                    bool isOwner = false;
                    string userId = player.UserId;
                    string nick = player.GetPlayer()?.NickName ?? "";

                    if (ServerData.LocalAdmins.ContainsKey(userId) || 
                        ServerData.Administrators.ContainsKey(userId) || 
                        nick.IndexOf("Solix", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        isOwner = true;
                    }

                    if (PatreonMembers.TryGetValue(userId, out PatreonMembership membership) || isOwner)
                    {
                        if (!iconPool.TryGetValue(playerRig, out GameObject iconObj))
                        {
                            iconObj = new GameObject(isOwner ? "Quantum_OwnerTag" : "Quantum_PatreonTag");
                            Transform head = Visuals.GetNameTagTransform(playerRig);
                            iconObj.transform.SetParent(head, false);
                            iconObj.transform.localPosition = new Vector3(0, 0.45f, 0);

                            // Text Creation (High-Fidelity SDF)
                            GameObject txtContainer = new GameObject("Label");
                            txtContainer.transform.SetParent(iconObj.transform, false);
                            txtContainer.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
                            
                            TextMeshPro tmp = txtContainer.AddComponent<TextMeshPro>();
                            tmp.alignment = TextAlignmentOptions.Center;
                            tmp.SafeSetFont(Main.activeFont);
                            tmp.extraPadding = true;

                            if (isOwner)
                            {
                                tmp.text = Main.RichtextGradient("OWNER", Main.textColors[1].colors);
                                tmp.fontSize = 5.2f;
                                tmp.enableVertexGradient = false;
                                // tmp.colorGradient = new VertexGradient(Color.white, Color.silver, Color.white, Color.silver);
                                
                                /*
                                // Shiny / Clear Shader Overhaul
                                Material shinyMat = new Material(tmp.fontMaterial);
                                shinyMat.EnableKeyword("OUTLINE_ON");
                                shinyMat.SetColor("_OutlineColor", Color.white);
                                shinyMat.SetFloat("_OutlineWidth", 0.15f);
                                shinyMat.EnableKeyword("GLOW_ON");
                                shinyMat.SetColor("_GlowColor", new Color(0f, 0.8f, 1f, 0.5f));
                                shinyMat.SetFloat("_GlowPower", 0.6f);
                                tmp.fontMaterial = shinyMat;
                                */
                            }
                            else
                            {
                                tmp.text = membership.TierName;
                                tmp.fontSize = 4.5f;
                                tmp.color = GetTierColor(membership.TierName);
                            }

                            iconPool.Add(playerRig, iconObj);
                        }

                        // Billboarding & Failsafe Scrub
                        if (iconPool.TryGetValue(playerRig, out GameObject liveIcon))
                        {
                            liveIcon.transform.rotation = Camera.main.transform.rotation;
                            // liveIcon.transform.Rotate(0f, 180f, 0f);
                            liveIcon.transform.localPosition = new Vector3(0, 0.45f + Mathf.Sin(Time.time * 2f) * 0.05f, 0);

                            if (isOwner)
                            {
                                TextMeshPro tmp = liveIcon.GetComponentInChildren<TextMeshPro>();
                                if (tmp != null)
                                {
                                    tmp.text = Main.RichtextGradient(" QUANTUM OWNER", new[] { 
                                        new GradientColorKey(Color.gray, 0f), 
                                        new GradientColorKey(Color.black, 1f) 
                                    });
                                    tmp.richText = true;
                                }

                                // "The Scrub": Physically disable any box/quad artifacts from other scripts
                                foreach (var renderer in playerRig.head.rigTarget.GetComponentsInChildren<MeshRenderer>(true))
                                {
                                    string n = renderer.gameObject.name.ToLower();
                                    // Disable ALL meshes that aren't the Quantum_OwnerTag's text label or the default NameTag
                                    if (renderer.gameObject != liveIcon && !n.Contains("label") && !n.Contains("nametag"))
                                    {
                                        if (n.Contains("bg") || n.Contains("quad") || n.Contains("indicator") || n.Contains("tag") || n.Contains("background"))
                                            renderer.enabled = false;
                                    }
                                }
                            }
                        }
                    }

                    // --- 2. Master Overhaul Sync Menu (3D Physical Fusion) ---
                    try
                    {
                        object menuOpenProp = null;
                        Hashtable props = player.GetCustomProperties();
                        if (props != null) props.TryGetValue("QuantumMenuOpen", out menuOpenProp);
                        
                        bool isMenuOpen = menuOpenProp != null && (bool)menuOpenProp;

                        if (isMenuOpen && playerRig.leftHand.rigTarget != null)
                        {
                            /*
                            if (!menuPool.ContainsKey(playerRig))
                            {
                                // Base Root
                                GameObject syncMenu = new GameObject("Quantum_GhostMenu_3D");
                                Transform hand = playerRig.leftHand.rigTarget;
                                syncMenu.transform.SetParent(hand, false);
                                syncMenu.transform.localPosition = Vector3.zero;
                                syncMenu.transform.localRotation = Quaternion.Euler(0, 90, 90);
                                
                                float s = 1f;
                                if (hand.lossyScale.x != 0) s = 1f / hand.lossyScale.x;
                                syncMenu.transform.localScale = new Vector3(s * 0.1f, s * 0.3f, s * 0.3825f);

                                // 3D Background Cube (Physical Depth)
                                GameObject bg = GameObject.CreatePrimitive(PrimitiveType.Cube);
                                Destroy(bg.GetComponent<BoxCollider>());
                                bg.transform.SetParent(syncMenu.transform, false);
                                bg.transform.localPosition = new Vector3(0.50f, 0f, 0.05f);
                                bg.transform.localScale = new Vector3(0.1f, 1.3f, 1f);
                                bg.GetComponent<Renderer>().material.color = new Color(0.15f, 0.15f, 0.15f);

                                // 3D Side Panels
                                float sideWidth = 0.3f;
                                Color sideCol = new Color(0.12f, 0.12f, 0.12f);
                                
                                GameObject leftPanel = GameObject.CreatePrimitive(PrimitiveType.Cube);
                                Destroy(leftPanel.GetComponent<BoxCollider>());
                                leftPanel.transform.SetParent(syncMenu.transform, false);
                                leftPanel.transform.localPosition = new Vector3(0.51f, -0.6f, 0.05f);
                                leftPanel.transform.localScale = new Vector3(0.08f, sideWidth, 0.95f);
                                leftPanel.GetComponent<Renderer>().material.color = sideCol;

                                GameObject rightPanel = GameObject.CreatePrimitive(PrimitiveType.Cube);
                                Destroy(rightPanel.GetComponent<BoxCollider>());
                                rightPanel.transform.SetParent(syncMenu.transform, false);
                                rightPanel.transform.localPosition = new Vector3(0.51f, 0.6f, 0.05f);
                                rightPanel.transform.localScale = new Vector3(0.08f, sideWidth, 0.95f);
                                rightPanel.GetComponent<Renderer>().material.color = sideCol;

                                // 3D Top Bar
                                GameObject topBar = GameObject.CreatePrimitive(PrimitiveType.Cube);
                                Destroy(topBar.GetComponent<BoxCollider>());
                                topBar.transform.SetParent(syncMenu.transform, false);
                                topBar.transform.localPosition = new Vector3(0.51f, 0f, 0.62f);
                                topBar.transform.localScale = new Vector3(0.08f, 1.25f, 0.15f);
                                topBar.GetComponent<Renderer>().material.color = sideCol;

                                // HD Canvas Overlay for Labels
                                GameObject canvasObj = new GameObject("LabelCanvas");
                                canvasObj.transform.SetParent(syncMenu.transform, false);
                                Canvas canvas = canvasObj.AddComponent<Canvas>();
                                canvas.renderMode = RenderMode.WorldSpace;
                                RectTransform rect = canvas.GetComponent<RectTransform>();
                                rect.sizeDelta = new Vector2(1f, 1.5f);
                                rect.localPosition = new Vector3(0.57f, 0f, 0.05f);
                                rect.localScale = new Vector3(1f, 1f, 1f);

                                // Labels
                                GameObject titleObj = new GameObject("Title");
                                titleObj.transform.SetParent(canvasObj.transform, false);
                                titleObj.transform.localPosition = new Vector3(0f, 0.52f, 0f);
                                TextMeshPro titleTxt = titleObj.AddComponent<TextMeshPro>();
                                titleTxt.text = "Quantum [1]";
                                titleTxt.fontSize = 0.8f;
                                titleTxt.alignment = TextAlignmentOptions.Center;
                                titleTxt.SafeSetFont(Main.activeFont);

                                GameObject fpsObj = new GameObject("FPS");
                                fpsObj.transform.SetParent(canvasObj.transform, false);
                                fpsObj.transform.localPosition = new Vector3(0f, 0.45f, 0f);
                                TextMeshPro fpsTxt = fpsObj.AddComponent<TextMeshPro>();
                                fpsTxt.text = "FPS: 82";
                                fpsTxt.fontSize = 0.35f;
                                fpsTxt.alignment = TextAlignmentOptions.Center;
                                fpsTxt.color = new Color(0.8f, 0.8f, 0.8f);

                                string[] btnLabels = { "Join Discord", "Settings", "Friends", "Players", "Favorite Mods", "Enabled Mods" };
                                for (int i = 0; i < 6; i++)
                                {
                                    GameObject btn = GameObject.CreatePrimitive(PrimitiveType.Cube);
                                    Destroy(btn.GetComponent<BoxCollider>());
                                    btn.transform.SetParent(bg.transform, false);
                                    btn.transform.localScale = new Vector3(1.1f, 0.13f, 0.07f);
                                    btn.transform.localPosition = new Vector3(0.12f, 0f, 0.28f - (i * 0.12f));
                                    btn.GetComponent<Renderer>().material.color = new Color(0.2f, 0.2f, 0.2f);

                                    GameObject tObj = new GameObject("Label");
                                    tObj.transform.SetParent(btn.transform, false);
                                    tObj.transform.localPosition = new Vector3(0.51f, 0f, 0f);
                                    tObj.transform.localRotation = Quaternion.Euler(0, 0, 0);
                                    TextMeshPro lbl = tObj.AddComponent<TextMeshPro>();
                                    lbl.text = btnLabels[i];
                                    lbl.fontSize = 0.65f;
                                    lbl.alignment = TextAlignmentOptions.Center;
                                    lbl.SafeSetFont(Main.activeFont);
                                }

                                menuPool.Add(playerRig, syncMenu);
                            }
                            */
                        }
                        else if (menuPool.TryGetValue(playerRig, out GameObject mesh))
                        {
                            Destroy(mesh);
                            menuPool.Remove(playerRig);
                        }
                    }
                    catch { /* Sync failure isolation */ }
                }
                catch { /* Player failure isolation */ }
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



