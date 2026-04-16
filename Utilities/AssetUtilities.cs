/*
 * Quantum Menu  Utilities/AssetUtilities.cs
 * A community driven mod menu for Gorilla Tag with over 1000+ mods
 *
 * Copyright (C) 2026  Quantum Software
 * https://github.com/Quantum/Quantum-Menu
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
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

using Quantum.Managers;
using Quantum.Menu;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using UnityEngine;
using Object = UnityEngine.Object;
using UnityEngine.Networking;
using static Quantum.Utilities.FileUtilities;

namespace Quantum.Utilities
{
    public class AssetUtilities
    {
        private static AssetBundle assetBundle;
        private static void LoadAssetBundle()
        {
            try
            {
                string[] resourceNames = Assembly.GetExecutingAssembly().GetManifestResourceNames();
                LogManager._v3_out_($"Quantum Menu: Searching for asset bundle in {resourceNames.Length} resources...");

                // Look for a resource that ends with "quantummenu" (case-insensitive)
                string targetName = resourceNames.FirstOrDefault(n => n.EndsWith("quantummenu", StringComparison.OrdinalIgnoreCase));

                if (string.IsNullOrEmpty(targetName))
                {
                    // Secondary search for anything containing "quantum" and "bundle" or just "menu" as a fallback
                    targetName = resourceNames.FirstOrDefault(n => n.ToLower().Contains("quantum") && n.ToLower().Contains("bundle"))
                                ?? resourceNames.FirstOrDefault(n => n.ToLower().Contains("menu"));
                }

                if (!string.IsNullOrEmpty(targetName))
                {
                    LogManager._v3_out_($"Quantum Menu: Loading asset bundle from {targetName}...");
                    using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(targetName))
                    {
                        if (stream != null)
                        {
                            assetBundle = AssetBundle.LoadFromStream(stream);
                            if (assetBundle != null)
                            {
                                LogManager._v3_out_("Quantum Menu: Asset bundle loaded successfully!");
                            }
                            else
                            {
                                LogManager._v3_out_("Quantum Menu: Failed to load asset bundle from stream (is it a valid bundle?)");
                            }
                        }
                    }
                }
                else
                {
                    LogManager._v3_out_("Quantum Menu: Could not find asset bundle in manifest resources.");
                }
            }
            catch (Exception ex)
            {
                LogManager._v3_out_($"Quantum Menu: Critical error loading asset bundle: {ex.Message}");
            }
        }

        public static AssetBundle Bundle
        {
            get
            {
                if (assetBundle == null)
                {
                    LoadAssetBundle();
                }
                return assetBundle;
            }
        }

        #region Loading Items

        public static T LoadAsset<T>(string name) where T : Object
        {
            if (Bundle != null)
            {
                return Bundle.LoadAsset<T>(name);
            }
            return null;
        }

        public static GameObject LoadAsset(string name)
        {
            return LoadAsset<GameObject>(name);
        }

        public static T LoadObject<T>(string name) where T : Object
        {
            return LoadAsset<T>(name);
        }

        public static void LoadSoundFromURL(string url, string fileName, Action<AudioClip> callback = null)
        {
            if (!PluginInfo.UseRemoteResources && !File.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, PluginInfo.BaseDirectory, fileName)))
            {
                LogManager._v3_out_($"Quantum Menu: Skipping remote load for {fileName} (UseRemoteResources is false and file is missing locally).");
                return;
            }

            if (CoroutineManager.instance != null)
                CoroutineManager.instance.StartCoroutine(LoadAudioClip(url, fileName, callback));
        }

        public static Texture2D LoadTextureFromURL(string url, string fileName)
        {
            // Simple synchronous-looking wrapper or placeholder
            // For now, return a placeholder and we can improve it if needed
            return Texture2D.whiteTexture; 
        }

        public static Texture2D LoadTextureFromFile(string filePath)
        {
            try
            {
                if (File.Exists(filePath))
                {
                    byte[] data = File.ReadAllBytes(filePath);
                    Texture2D texture = new Texture2D(2, 2);
                    if (texture.LoadImage(data))
                        return texture;
                }
            }
            catch (Exception ex)
            {
                LogManager._v3_out_($"Quantum Menu: Failed to load texture from file {filePath}! Reason: {ex.Message}");
            }
            return Texture2D.whiteTexture;
        }

        public static void LoadSoundFromFile(string filePath, Action<AudioClip> callback)
        {
            if (CoroutineManager.instance != null)
                CoroutineManager.instance.StartCoroutine(LoadAudioClip(filePath, Path.GetFileName(filePath), callback));
        }

        public static Texture2D LoadTextureFromResource(string resourcePath)
        {
            try
            {
                using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourcePath))
                {
                    if (stream != null)
                    {
                        byte[] data = new byte[stream.Length];
                        stream.Read(data, 0, data.Length);
                        
                        Texture2D texture = new Texture2D(2, 2);
                        if (texture.LoadImage(data))
                        {
                            return texture;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogManager._v3_out_($"Quantum Menu: Failed to load texture from resource {resourcePath}! Reason: {ex.Message}");
            }
            return Texture2D.whiteTexture;
        }

        public static IEnumerator LoadAudioClip(string url, string fileName, Action<AudioClip> callback)
        {
            string directory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Quantum", "Sounds");
            string filePath = Path.Combine(directory, fileName);

            if (!File.Exists(filePath))
            {
                LogManager._v3_out_($"Quantum Menu: Pulling audio clip {fileName} from {url}...");
                var handler = new DownloadHandlerAudioClip(url, GetAudioType(GetFileExtension(fileName)));

                using UnityWebRequest request = new UnityWebRequest(url, UnityWebRequest.kHttpVerbGET, handler, null);

                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    byte[] data = request.downloadHandler.data;
                    if (!Directory.Exists(directory))
                        Directory.CreateDirectory(directory);

                    File.WriteAllBytes(filePath, data);
                    callback?.Invoke(handler.audioClip);

                    LogManager._v3_out_($"Quantum Menu: Pulled {fileName} successfully!");
                }
                else
                {
                    LogManager._v3_out_($"Quantum Menu: Failed to pull {fileName}! Reason: {request.error}");
                }
            }
            else
            {
                LogManager._v3_out_($"Quantum Menu: Loading local audio clip {fileName}...");
                var handler = new DownloadHandlerAudioClip(filePath, GetAudioType(GetFileExtension(fileName)));

                using UnityWebRequest request = new UnityWebRequest(filePath, UnityWebRequest.kHttpVerbGET, handler, null);

                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    callback?.Invoke(handler.audioClip);
                }
                else
                {
                    LogManager._v3_out_($"Quantum Menu: Failed to load local {fileName}! Reason: {request.error}");
                }
            }
        }

        public static IEnumerator LoadAssetBundle(string resourcePath, string fileName, Action<AssetBundle> callback)
        {
            string directory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Quantum", "Bundles");
            string filePath = Path.Combine(directory, fileName);

            bool shouldDownload = !File.Exists(filePath);

            if (!shouldDownload)
            {
                LogManager._v3_out_($"Quantum Menu: Checking for updates for {fileName}...");

                using UnityWebRequest request = UnityWebRequest.Get(resourcePath);
                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    byte[] remoteData = request.downloadHandler.data;
                    byte[] localData = File.ReadAllBytes(filePath);

                    using var sha = System.Security.Cryptography.SHA256.Create();
                    byte[] remoteHash = sha.ComputeHash(remoteData);
                    byte[] localHash = sha.ComputeHash(localData);

                    if (remoteHash != localHash)
                    {
                        LogManager._v3_out_($"Quantum Menu: Update found for {fileName}!");
                        shouldDownload = true;
                    }
                }
            }

            if (shouldDownload)
            {
                LogManager._v3_out_($"Quantum Menu: Downloading {fileName} from {resourcePath}...");
                using UnityWebRequest request = UnityWebRequest.Get(resourcePath);
                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    byte[] data = request.downloadHandler.data;
                    if (!Directory.Exists(directory))
                        Directory.CreateDirectory(directory);

                    File.WriteAllBytes(filePath, data);
                    callback?.Invoke(AssetBundle.LoadFromMemory(data));

                    LogManager._v3_out_($"Quantum Menu: Downloaded {fileName} successfully!");
                }
                else
                {
                    LogManager._v3_out_($"Quantum Menu: Failed to download {fileName}! Reason: {request.error}");
                }
            }
            else
            {
                LogManager._v3_out_($"Quantum Menu: Loading local asset bundle {fileName}...");
                callback?.Invoke(AssetBundle.LoadFromFile(filePath));
            }
        }
        #endregion
    }
}

