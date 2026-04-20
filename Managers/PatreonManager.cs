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
                    if (player.IsLocal) continue;
                    VRRig playerRig = player.VRRig();
                    if (playerRig == null) continue;

                    // --- 1. Universal Patreon & Tag Logic ---
                    bool isOwner = false;
                    string userId = player.UserId;
                    string nick = player.GetPlayer()?.NickName ?? "";

                    // Multi-layered Owner Check (ID + Alias + Nickname fallback)
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
                            // Nuclear Fix: For owners, create a BLANK object (No Quad, No Block)
                            if (isOwner)
                            {
                                iconObj = new GameObject("Quantum_OwnerTag");
                            }
                            else
                            {
                                iconObj = GameObject.CreatePrimitive(PrimitiveType.Quad);
                                Destroy(iconObj.GetComponent<Collider>());
                                if (iconMaterial == null) iconMaterial = new Material(Shader.Find("Sprites/Default"));
                                iconObj.GetComponent<Renderer>().material = new Material(iconMaterial);
                                AssetUtilities.LoadTextureFromURL(membership.IconURL, $"Images/Patreon/{player.UserId}.{FileUtilities.GetFileExtension(membership.IconURL)}", iconObj.GetComponent<Renderer>());
                            }

                            // Shared Parenting
                            Transform head = Visuals.GetNameTagTransform(playerRig);
                            iconObj.transform.SetParent(head, false);
                            iconObj.transform.localPosition = new Vector3(0, 0.4f, 0);

                            // Text Creation
                            GameObject txtContainer = new GameObject("Quantum_Nametag");
                            txtContainer.transform.SetParent(iconObj.transform, false);
                            txtContainer.transform.localScale = new Vector3(0.4f, 0.4f, 0.4f);
                            txtContainer.transform.localPosition = new Vector3(0, isOwner ? 0f : 0.25f, 0);
                            
                            TextMeshPro tmp = txtContainer.AddComponent<TextMeshPro>();
                            tmp.fontSize = 4.8f;
                            tmp.alignment = TextAlignmentOptions.Center;
                            tmp.SafeSetFontStyle(Main.activeFontStyle);
                            tmp.SafeSetFont(Main.activeFont);

                            if (isOwner)
                            {
                                tmp.text = "OWNER";
                                tmp.enableVertexGradient = true;
                                tmp.colorGradient = new VertexGradient(Color.black, new Color(0.7f, 0.7f, 0.7f), Color.black, new Color(0.7f, 0.7f, 0.7f));
                                Shader txtShader = Shader.Find("GUI/Text Shader");
                                if (txtShader != null) tmp.fontMaterial.shader = txtShader;
                            }
                            else
                            {
                                tmp.SafeSetText(membership.TierName);
                                tmp.color = GetTierColor(membership.TierName);
                            }

                            iconPool.Add(playerRig, iconObj);
                        }

                        // Billboarding (If not null)
                        if (iconPool.TryGetValue(playerRig, out GameObject liveIcon))
                        {
                            liveIcon.transform.rotation = Camera.main.transform.rotation;
                            liveIcon.transform.Rotate(0f, 180f, 0f);
                        }
                    }

                    // --- 2. Master Overhaul Sync Menu (High-Fidelity Canvas Rebuild) ---
                    try
                    {
                        object menuOpenProp = null;
                        Hashtable props = player.GetCustomProperties();
                        if (props != null) props.TryGetValue("QuantumMenuOpen", out menuOpenProp);
                        
                        bool isMenuOpen = menuOpenProp != null && (bool)menuOpenProp;

                        if (isMenuOpen && playerRig.leftHand.rigTarget != null)
                        {
                            if (!menuPool.ContainsKey(playerRig))
                            {
                                // 1. Ghost Menu Root
                                GameObject syncMenu = new GameObject("Quantum_GhostMenu_Canvas");
                                Transform hand = playerRig.leftHand.rigTarget;
                                syncMenu.transform.SetParent(hand, false);
                                syncMenu.transform.localPosition = Vector3.zero;
                                syncMenu.transform.localRotation = Quaternion.Euler(0, 90, 90);
                                
                                // Normalized scale fix
                                float s = 1f;
                                if (hand.lossyScale.x != 0) s = 1f / hand.lossyScale.x;
                                syncMenu.transform.localScale = new Vector3(s, s, s);

                                // 2. World Space Canvas
                                Canvas canvas = syncMenu.AddComponent<Canvas>();
                                canvas.renderMode = RenderMode.WorldSpace;
                                RectTransform rect = syncMenu.GetComponent<RectTransform>();
                                rect.sizeDelta = new Vector2(0.5f, 0.8f);
                                rect.localScale = new Vector3(0.5f, 0.5f, 0.5f);

                                // 3. Main UI Background (Rectangle)
                                GameObject bgObj = new GameObject("Background");
                                bgObj.transform.SetParent(syncMenu.transform, false);
                                UnityEngine.UI.Image bgImg = bgObj.AddComponent<UnityEngine.UI.Image>();
                                bgImg.color = new Color(0.15f, 0.15f, 0.15f, 1f);
                                bgImg.rectTransform.sizeDelta = new Vector2(0.8f, 1.2f);

                                // 4. Side Panels
                                float panelWidth = 0.15f;
                                GameObject leftP = new GameObject("LeftPanel");
                                leftP.transform.SetParent(syncMenu.transform, false);
                                UnityEngine.UI.Image lImg = leftP.AddComponent<UnityEngine.UI.Image>();
                                lImg.color = new Color(0.12f, 0.12f, 0.12f);
                                lImg.rectTransform.sizeDelta = new Vector2(panelWidth, 1.1f);
                                lImg.rectTransform.localPosition = new Vector3(-0.5f, 0f, 0.01f);

                                GameObject rightP = new GameObject("RightPanel");
                                rightP.transform.SetParent(syncMenu.transform, false);
                                UnityEngine.UI.Image rImg = rightP.AddComponent<UnityEngine.UI.Image>();
                                rImg.color = new Color(0.12f, 0.12f, 0.12f);
                                rImg.rectTransform.sizeDelta = new Vector2(panelWidth, 1.1f);
                                rImg.rectTransform.localPosition = new Vector3(0.5f, 0f, 0.01f);

                                // 5. Top Bar (Disconnect)
                                GameObject topBar = new GameObject("TopBar");
                                topBar.transform.SetParent(syncMenu.transform, false);
                                UnityEngine.UI.Image tImg = topBar.AddComponent<UnityEngine.UI.Image>();
                                tImg.color = new Color(0.12f, 0.12f, 0.12f);
                                tImg.rectTransform.sizeDelta = new Vector2(0.75f, 0.15f);
                                tImg.rectTransform.localPosition = new Vector3(0f, 0.72f, 0.01f);

                                // 6. Text Labels (The "High-Fidelity" touch)
                                GameObject titleObj = new GameObject("Title");
                                titleObj.transform.SetParent(syncMenu.transform, false);
                                titleObj.transform.localPosition = new Vector3(0f, 0.52f, -0.01f);
                                TextMeshPro titleTxt = titleObj.AddComponent<TextMeshPro>();
                                titleTxt.text = "Quantum [1]";
                                titleTxt.fontSize = 0.8f;
                                titleTxt.alignment = TextAlignmentOptions.Center;
                                titleTxt.color = Color.white;
                                titleTxt.SafeSetFont(Main.activeFont);

                                GameObject fpsObj = new GameObject("FPS");
                                fpsObj.transform.SetParent(syncMenu.transform, false);
                                fpsObj.transform.localPosition = new Vector3(0f, 0.45f, -0.01f);
                                TextMeshPro fpsTxt = fpsObj.AddComponent<TextMeshPro>();
                                fpsTxt.text = "FPS: 82";
                                fpsTxt.fontSize = 0.35f;
                                fpsTxt.alignment = TextAlignmentOptions.Center;
                                fpsTxt.color = new Color(0.8f, 0.8f, 0.8f);

                                // 7. 6 Menu Buttons
                                string[] btnLabels = { "Join Discord", "Settings", "Friends", "Players", "Favorite Mods", "Enabled Mods" };
                                for (int i = 0; i < 6; i++)
                                {
                                    GameObject btnObj = new GameObject($"Btn_{i}");
                                    btnObj.transform.SetParent(syncMenu.transform, false);
                                    btnObj.transform.localPosition = new Vector3(0f, 0.3f - (i * 0.14f), -0.01f);
                                    
                                    UnityEngine.UI.Image bImg = btnObj.AddComponent<UnityEngine.UI.Image>();
                                    bImg.color = new Color(0.2f, 0.2f, 0.2f);
                                    bImg.rectTransform.sizeDelta = new Vector2(0.7f, 0.11f);

                                    GameObject t = new GameObject("Label");
                                    t.transform.SetParent(btnObj.transform, false);
                                    TextMeshPro lbl = t.AddComponent<TextMeshPro>();
                                    lbl.text = btnLabels[i];
                                    lbl.fontSize = 0.4f;
                                    lbl.alignment = TextAlignmentOptions.Center;
                                    lbl.color = Color.white;
                                    lbl.SafeSetFont(Main.activeFont);
                                }

                                menuPool.Add(playerRig, syncMenu);
                            }
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



