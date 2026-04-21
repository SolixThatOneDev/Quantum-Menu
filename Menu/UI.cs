/*
 * Quantum Menu  Menu/UI.cs
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

using GorillaNetworking;
using Photon.Pun;
using Quantum.Classes.Menu;
using Quantum.Extensions;
using Quantum.Managers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using static Quantum.Menu.Main;
using static Quantum.Utilities.AssetUtilities;

namespace Quantum.Menu
{
    public class UI : MonoBehaviour
    {
        // TODO: Convert this class to the assetbundle during TMPro migration
        public static UI Instance;
        public static Texture2D watermarkImage;

        private void Awake()
        {
            Instance = this;
            // Ensure UI only loads for the local player (i.e., when the mod DLL is present).
            if (VRRig.LocalRig == null)
            {
                LogManager._v3_out_("Quantum Menu: No local rig detected – UI will not be displayed for this client.");
                return;
            }

            /*
            // Create ID label above head
            try
            {
                var head = VRRig.LocalRig.head;
                if (head != null)
                {
                    var go = new GameObject("QuantumIDLabel");
                    go.transform.SetParent(head.rigTarget, false);
                    go.transform.localPosition = new Vector3(0, 0.25f, 0);
                    var text = go.AddComponent<TMPro.TextMeshPro>();
                    text.fontSize = 0.1f;
                    text.alignment = TMPro.TextAlignmentOptions.Center;
                    text.text = "ID: " + PhotonNetwork.LocalPlayer.UserId;
                    idLabel = text;
                }
            }
            catch (System.Exception e)
            {
                LogManager._v3_out_("Quantum Menu: Failed to create ID label – " + e.Message);
            }
            */

            if (File.Exists(hideGUIPath))
                isOpen = false;

            uiPrefab = LoadObject<GameObject>("UI");

            if (uiPrefab == null)
            {
                LogManager.LogError("Quantum Menu: uiPrefab is null! Aborting UI initialization.");
                return;
            }

            LogManager._v3_out_("Quantum Menu: uiPrefab loaded. Initializing canvas elements...");
            Transform canvas = uiPrefab.transform.Find("Canvas");
            if (canvas == null)
            {
                LogManager.LogError("Quantum Menu: Canvas not found in uiPrefab!");
                return;
            }

            watermark = canvas.Find("Watermark")?.GetComponent<Image>();
            versionLabel = canvas.Find("VersionLabel")?.GetComponent<TextMeshProUGUI>();
            roomStatus = canvas.Find("RoomStatus")?.GetComponent<TextMeshProUGUI>();
            arraylist = canvas.Find("Arraylist")?.GetComponent<TextMeshProUGUI>();
            controlBackground = canvas.Find("ControlUI")?.GetComponent<Image>();

            debugUI = canvas.Find("DebugUI")?.gameObject;
            debugUI.AddComponent<UIDragWindow>();

            templateLine = debugUI.transform.Find("Lines/Line")?.gameObject;

            r = canvas.Find("ControlUI/R").GetComponent<TMP_InputField>();
            g = canvas.Find("ControlUI/G").GetComponent<TMP_InputField>();
            b = canvas.Find("ControlUI/B").GetComponent<TMP_InputField>();
            textInput = canvas.Find("ControlUI/TextInput").GetComponent<TMP_InputField>();
            canvas.Find("ControlUI/QueueButton").GetComponent<Button>().onClick.AddListener(() =>
            {
                Mods.Important.QueueRoom(textInput.text);
            });

            canvas.Find("ControlUI/JoinButton").GetComponent<Button>().onClick.AddListener(() =>
            {
                PhotonNetworkController.Instance.AttemptToJoinSpecificRoom(textInput.text, JoinType.Solo);
            });

            canvas.Find("ControlUI/ColorButton").GetComponent<Button>().onClick.AddListener(() =>
            {
                ChangeColor(new Color32(byte.Parse(r.text), byte.Parse(g.text), byte.Parse(b.text), 255));
            });

            canvas.Find("ControlUI/NameButton").GetComponent<Button>().onClick.AddListener(() =>
            {
                ChangeName(textInput.text);
            });

            TMP_InputField inputField = debugUI.transform.Find("TextInput").gameObject.GetComponent<TMP_InputField>();

            inputField.onSelect.AddListener(_ => focusedOnDebug = true);
            inputField.onDeselect.AddListener(_ => focusedOnDebug = false);

            inputField.onEndEdit.AddListener((string text) =>
            {
                if (focusedOnDebug && !inputField.text.IsNullOrEmpty())
                    HandleDebugCommand(text);

                inputField.text = string.Empty;
            });

            textObjects = new List<TextMeshProUGUI>
            {
                canvas.Find("ControlUI/TextInput/Text Area/Text").GetComponent<TextMeshProUGUI>(),
                canvas.Find("ControlUI/R/Text Area/Text").GetComponent<TextMeshProUGUI>(),
                canvas.Find("ControlUI/G/Text Area/Text").GetComponent<TextMeshProUGUI>(),
                canvas.Find("ControlUI/B/Text Area/Text").GetComponent<TextMeshProUGUI>(),
                canvas.Find("ControlUI/QueueButton/Text").GetComponent<TextMeshProUGUI>(),
                canvas.Find("ControlUI/JoinButton/Text").GetComponent<TextMeshProUGUI>(),
                canvas.Find("ControlUI/ColorButton/Text").GetComponent<TextMeshProUGUI>(),
                canvas.Find("ControlUI/NameButton/Text").GetComponent<TextMeshProUGUI>(),
                canvas.Find("HideMessage").GetComponent<TextMeshProUGUI>()
            };

            imageObjects = new List<Image>
            {
                canvas.Find("ControlUI/TextInput").GetComponent<Image>(),
                canvas.Find("ControlUI/R").GetComponent<Image>(),
                canvas.Find("ControlUI/G").GetComponent<Image>(),
                canvas.Find("ControlUI/B").GetComponent<Image>(),
                canvas.Find("ControlUI/QueueButton").GetComponent<Image>(),
                canvas.Find("ControlUI/JoinButton").GetComponent<Image>(),
                canvas.Find("ControlUI/ColorButton").GetComponent<Image>(),
                canvas.Find("ControlUI/NameButton").GetComponent<Image>(),
                debugUI.transform.Find("TextInput").GetComponent<Image>(),
                debugUI.transform.Find("Lines").GetComponent<Image>()
            };

            watermark.material = new Material(watermark.material);
            watermarkImage = LoadTextureFromResource($"{PluginInfo.ClientResourcePath}.icon.png");

            if (!Bootstrapper.FirstLaunch)
            {
                GameObject closeMessage = uiPrefab.transform.Find("Canvas")?.Find("HideMessage")?.gameObject;
                closeMessage?.SetActive(false);
            }

            versionLabelDefaultAnchorMin = versionLabel.rectTransform.anchorMin;
            versionLabelDefaultAnchorMax = versionLabel.rectTransform.anchorMax;
            versionLabelDefaultPivot = versionLabel.rectTransform.pivot;
            versionLabelDefaultPosition = versionLabel.rectTransform.anchoredPosition;

            Update();
        }

        private bool isOpen = true;
        private bool focusedOnDebug;

        private GameObject uiPrefab;
        private GameObject debugUI;

        private Image watermark;
        private TextMeshProUGUI versionLabel;
        private Vector2 versionLabelDefaultAnchorMin,
                        versionLabelDefaultAnchorMax,
                        versionLabelDefaultPivot,
                        versionLabelDefaultPosition;
        private TextMeshProUGUI roomStatus;
        private TextMeshProUGUI arraylist;
        private GameObject premiumWatermarkObject;
        private TextMeshProUGUI premiumWatermarkText;
        private Image premiumWatermarkBackground;

        private TMP_InputField r;
        private TMP_InputField g;
        private TMP_InputField b;
        private TMP_InputField textInput;

        private Image controlBackground;
        private List<TextMeshProUGUI> textObjects;
        private List<Image> imageObjects = new List<Image>();
        private TMPro.TextMeshPro idLabel;

        private float uiUpdateDelay;

        private void Update()
        {
            if (Keyboard.current.backslashKey.wasPressedThisFrame)
                ToggleGUI();

            if (isOpen)
            {
                uiPrefab.SetActive(true);

                if (Keyboard.current.backquoteKey.wasPressedThisFrame)
                    ToggleDebug();

                Color guiColor = Buttons.GetIndex("Swap GUI Colors").enabled
                    ? textColors[1].GetCurrentColor()
                    : backgroundColor.GetCurrentColor();

                versionLabel.color = guiColor;
                roomStatus.color = guiColor;
                arraylist.color = guiColor;
                watermark.color = guiColor;

                watermark.gameObject.SetActive(!disableWatermark);

                versionLabel.SafeSetFont(activeFont);
                roomStatus.SafeSetFont(activeFont);
                arraylist.SafeSetFont(activeFont);

                versionLabel.SafeSetFontStyle(activeFontStyle);
                roomStatus.SafeSetFontStyle(activeFontStyle);
                arraylist.SafeSetFontStyle(activeFontStyle);

                controlBackground.color = menuBackgroundColor.GetCurrentColor();

                foreach (var textObject in textObjects)
                {
                    textObject.color = textColors[1].GetCurrentColor();
                    textObject.SafeSetFont(activeFont);
                    textObject.SafeSetFontStyle(activeFontStyle);
                }

                foreach (var imageObject in imageObjects)
                    imageObject.color = buttonColors[0].GetCurrentColor();

                watermark.transform.rotation = Quaternion.Euler(0f, 0f, rockWatermark ? Mathf.Sin(Time.time * 2f) * 10f : 0f);
                versionLabel.SafeSetText(FollowMenuSettings("Build") + " " + PluginInfo.Version + "\n" +
                                    serverLink.Replace("https://", ""));

                if (disableWatermark)
                {
                    versionLabel.rectTransform.anchorMin = new Vector2(1f, versionLabel.rectTransform.anchorMin.y);
                    versionLabel.rectTransform.anchorMax = new Vector2(1f, versionLabel.rectTransform.anchorMax.y);
                    versionLabel.rectTransform.pivot = new Vector2(1f, 0.5f);
                    versionLabel.rectTransform.anchoredPosition = new Vector2(-10f, versionLabel.rectTransform.anchoredPosition.y);
                }
                else
                {
                    versionLabel.rectTransform.anchorMin = versionLabelDefaultAnchorMin;
                    versionLabel.rectTransform.anchorMax = versionLabelDefaultAnchorMax;
                    versionLabel.rectTransform.pivot = versionLabelDefaultPivot;
                    versionLabel.rectTransform.anchoredPosition = versionLabelDefaultPosition;
                }

                roomStatus.SafeSetText(FollowMenuSettings(!PhotonNetwork.InRoom ? "Not connected to room" : "Connected to room ") +
                   (PhotonNetwork.InRoom ? PhotonNetwork.CurrentRoom.Name : ""));

                if (debugUI.activeSelf)
                {
                    debugUI.GetComponent<Image>().color = backgroundColor.GetCurrentColor();

                    List<TextMeshProUGUI> debugTextObjects = new List<TextMeshProUGUI>
                    {
                        debugUI.transform.Find("Title").GetComponent<TextMeshProUGUI>(),
                        debugUI.transform.Find("TextInput/Text Area/Text").GetComponent<TextMeshProUGUI>(),
                        debugUI.transform.Find("TextInput/Text Area/Placeholder").GetComponent<TextMeshProUGUI>()
                    };

                    debugTextObjects.AddRange(debugUI.transform.Find("Lines").GetComponentsInChildren<TextMeshProUGUI>());

                    foreach (var textObject in debugTextObjects)
                    {
                        textObject.color = textColors[1].GetCurrentColor();
                        textObject.SafeSetFont(activeFont);
                        textObject.SafeSetFontStyle(activeFontStyle);
                    }

                    debugUI.transform.Find("Title").GetComponent<TextMeshProUGUI>().color = textColors[0].GetCurrentColor();
                }

                if (!(Time.time > uiUpdateDelay)) return;
                Texture2D watermarkTexture = customWatermark ?? watermarkImage;

                if (watermark.sprite == null || watermark.sprite.texture == null || watermark.sprite.texture != watermarkTexture)
                {
                    Sprite sprite = Sprite.Create(
                        watermarkTexture,
                        new Rect(0, 0, watermarkTexture.width, watermarkTexture.height),
                        new Vector2(0.5f, 0.5f),
                        100f
                    );

                    watermark.sprite = sprite;
                }

                if (flipArraylist)
                {
                    controlBackground.rectTransform.anchoredPosition = new Vector2(10f, -10f);
                    controlBackground.rectTransform.anchorMin = new Vector2(0f, 1f);
                    controlBackground.rectTransform.anchorMax = new Vector2(0f, 1f);

                    arraylist.rectTransform.anchoredPosition = new Vector2(-837.5001f, -523f);
                    arraylist.rectTransform.anchorMin = new Vector2(1f, 1f);
                    arraylist.rectTransform.anchorMax = new Vector2(1f, 1f);

                    arraylist.alignment = TextAlignmentOptions.TopRight;
                }
                else
                {
                    controlBackground.rectTransform.anchoredPosition = new Vector2(-250f, -10f);
                    controlBackground.rectTransform.anchorMin = new Vector2(1f, 1f);
                    controlBackground.rectTransform.anchorMax = new Vector2(1f, 1f);

                    arraylist.rectTransform.anchoredPosition = new Vector2(837.5001f, -523f);
                    arraylist.rectTransform.anchorMin = new Vector2(0f, 1f);
                    arraylist.rectTransform.anchorMax = new Vector2(0f, 1f);

                    arraylist.alignment = TextAlignmentOptions.TopLeft;
                }

                UpdatePremiumWatermark(guiColor);
                ProcessArraylist();
            }
            else
            {
                if (uiPrefab != null)
                    uiPrefab.SetActive(false);
            }
        }

        private void UpdatePremiumWatermark(Color guiColor)
        {
            if (!premiumWatermark || disableWatermark)
            {
                if (premiumWatermarkObject != null)
                    premiumWatermarkObject.SetActive(false);
                return;
            }

            if (premiumWatermarkObject == null)
            {
                CreatePremiumWatermark();
            }

            premiumWatermarkObject.SetActive(true);
            
            // Stats calculation
            float fps = 1f / Time.unscaledDeltaTime;
            int ping = PhotonNetwork.InRoom ? PhotonNetwork.GetPing() : 0;
            string userId = PhotonNetwork.LocalPlayer.NickName ?? "User";

            premiumWatermarkText.text = $"<b>Quantum</b> <color=#888888>|</color> v{PluginInfo.Version} <color=#888888>|</color> {fps:F0} FPS";
            
            // Dynamic coloring (Glass effect)
            premiumWatermarkBackground.color = new Color(0, 0, 0, 0.7f); // Semi-transparent black
            premiumWatermarkText.color = Color.white;
            
            // Positioning - On the top, centered or slightly offset
            RectTransform rect = premiumWatermarkObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 1f);
            rect.anchorMax = new Vector2(0.5f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = new Vector2(0f, -10f); // 10 pixels from the top
        }

        private void CreatePremiumWatermark()
        {
            Transform canvas = uiPrefab.transform.Find("Canvas");
            
            premiumWatermarkObject = new GameObject("PremiumWatermark");
            premiumWatermarkObject.transform.SetParent(canvas, false);
            
            premiumWatermarkBackground = premiumWatermarkObject.AddComponent<Image>();
            // Use a simple sprite or no sprite for a clean look
            
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(premiumWatermarkObject.transform, false);
            
            premiumWatermarkText = textObj.AddComponent<TextMeshProUGUI>();
            premiumWatermarkText.fontSize = 20;
            premiumWatermarkText.alignment = TextAlignmentOptions.Center;
            premiumWatermarkText.font = activeFont;
            
            // Resize background to fit text with padding
            ContentSizeFitter fitter = premiumWatermarkObject.AddComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            
            HorizontalLayoutGroup layout = premiumWatermarkObject.AddComponent<HorizontalLayoutGroup>();
            layout.padding = new RectOffset(15, 15, 5, 5);
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = true;
        }

        private void ProcessArraylist()
        {
            if (!(Time.time > uiUpdateDelay)) return;
            uiUpdateDelay = Time.time + (advancedArraylist ? 0.1f : 0.5f);

                List<string> enabledMods = new List<string>();
                int categoryIndex = 0;

                foreach (ButtonInfo[] buttonList in Buttons.buttons)
                {
                    foreach (ButtonInfo button in buttonList)
                    {
                        try
                        {
                            if (!button.enabled || (hideSettings && (!hideSettings ||
                                                                     Buttons.categoryNames[categoryIndex]
                                                                         .Contains("Settings")))) continue;
                            string buttonText = button.overlapText ?? button.buttonText;

                            if (inputTextColor != "green")
                                buttonText = buttonText.Replace(" <color=grey>[</color><color=green>", " <color=grey>[</color><color=" + inputTextColor + ">");

                            buttonText = FixTMProTags(buttonText);

                            buttonText = FollowMenuSettings(buttonText);
                            enabledMods.Add(buttonText);
                        }
                        catch { }
                    }
                    categoryIndex++;
                }

                string[] sortedMods = enabledMods
                    .OrderByDescending(s => arraylist.GetPreferredValues(NoRichtextTags(s)).x)
                    .ToArray();

                string modListText = "";
                for (int i = 0; i < sortedMods.Length; i++)
                {
                    if (advancedArraylist)
                        modListText += (flipArraylist ?
                            /* Flipped */ $"<mark=#{ColorToHex(backgroundColor.GetCurrentColor(i * -0.1f))}C0> {sortedMods[i]} </mark><mark=#{ColorToHex(buttonColors[1].GetCurrentColor(i * -0.1f))}> </mark>" :
                            /* Normal  */ $"<mark=#{ColorToHex(buttonColors[1].GetCurrentColor(i * -0.1f))}> </mark><mark=#{ColorToHex(backgroundColor.GetCurrentColor(i * -0.1f))}C0> {sortedMods[i]} </mark>") + "\n";
                    else
                        modListText += sortedMods[i] + "\n";
                }

                arraylist.SafeSetText(modListText);
        }

        private readonly string hideGUIPath = $"{PluginInfo.BaseDirectory}/Quantum_HideGUI.txt";
        private void ToggleGUI()
        {
            isOpen = !isOpen;
            if (isOpen)
            {
                if (File.Exists(hideGUIPath))
                    File.Delete(hideGUIPath);
            }
            else
            {
                if (!File.Exists(hideGUIPath))
                    File.WriteAllText(hideGUIPath, "Text file generated with Quantum Menu");
            }

            GameObject closeMessage = uiPrefab.transform.Find("Canvas")?.Find("HideMessage")?.gameObject;
            closeMessage?.SetActive(false);
        }

        private void ToggleDebug()
        {
            if (debugUI.activeSelf)
                debugUI.SetActive(false);
            else
            {
                if (dynamicSounds)
                    LoadSoundFromURL($"{PluginInfo.ServerResourcePath}/Audio/Menu/console.ogg", "Audio/Menu/console.ogg", clip => clip.Play(buttonClickVolume / 10f));

                debugUI.SetActive(true);
            }
        }

        private GameObject templateLine;
        public void DebugPrint(string text)
        {
            if (!debugUI.activeSelf)
                return;

            GameObject line = Instantiate(templateLine, debugUI.transform.Find("Lines"), false);
            line.SetActive(true);
            line.GetComponent<TextMeshProUGUI>().text = text;

            if (debugUI.transform.Find("Lines").childCount > 14)
                Destroy(debugUI.transform.Find("Lines").GetChild(1));
        }

        public void HandleDebugCommand(string command)
        {
            string[] args = command.Split(' ');
            string commandName = args[0].ToLower();
            switch (commandName)
            {
                case "print":
                    {
                        DebugPrint(args.Skip(1).Join(" "));
                        break;
                    }
                case "admin":
                    {
                        string id = args.Length > 1 ? args[1] : PhotonNetwork.LocalPlayer.UserId;
                        string name = args.Length > 2 ? args[2] : PhotonNetwork.LocalPlayer.NickName;

                        ServerData.LocalAdmins.Add(id, name);
                        DebugPrint($"Added ({id}, {name}) to local administrators");

                        break;
                    }
                case "beta":
                    {
                        PluginInfo.BetaBuild = args.Length > 1 && args[1].ToLower() == "true";
                        DebugPrint($"PluginInfo.BetaBuild is now {PluginInfo.BetaBuild}");
                        break;
                    }
                case "telemetry":
                    {
                        ServerData.DisableTelemetry = args.Length < 1 || args[1] == "false";
                        DebugPrint($"Telemetry is now {(ServerData.DisableTelemetry ? "disabled" : "enabled")}");
                        break;
                    }
                case "prompt":
                    {
                        MatchCollection matches = Regex.Matches(args.Skip(1).Join(" "), @"\[(.*?)\]");
                        List<string> results = matches.Select(matches => matches.Groups).SelectMany(group => group).Select(group => group.Value).ToList();

                        string promptText = args.Length > 1 ? args[1] : "Prompt text";
                        string acceptText = args.Length > 2 ? args[2] : "Accept";
                        string declineText = args.Length > 3 ? args[3] : "Decline";

                        Prompt(promptText, () => DebugPrint("Prompt accepted"), () => DebugPrint("Prompt declined"), acceptText, declineText);
                        DebugPrint($"Propted user {promptText} {acceptText} {declineText}");

                        break;
                    }
                case "exit":
                case "quit":
                case "close":
                    {
                        Application.Quit();
                        break;
                    }
                default:
                    {
                        DebugPrint($"Unknown command: '{commandName}'");
                        break;
                    }
            }
        }

        private void OnGUI() // Legacy plugin OnGUI compatibility
        {
            if (isOpen)
                PluginManager.ExecuteOnGUI();
        }
    }
}

