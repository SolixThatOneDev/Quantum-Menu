using Quantum.Classes.Menu;
using Quantum.Managers;
using Quantum.Menu;
using Quantum.Patches;
using Quantum.Patches.Menu;
using System;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Quantum
{
    internal static class Bootstrapper
    {
        private static bool initialized;
        public static bool FirstLaunch;
        public static GameObject Loader;

        internal static void Initialize()
        {
            if (initialized) return;
            initialized = true;

            LogManager.Log("Quantum Menu: Bootstrapper.Initialize() called.");

            FirstLaunch = !Directory.Exists(PluginInfo.BaseDirectory);

            string[] existingDirectories =
            {
                "",
                "/Sounds",
                "/Plugins",
                "/Backups",
                "/Macros",
                "/TTS",
                "/PlayerInfo",
                "/CustomScripts",
                "/Friends",
                "/Friends/Messages",
                "/Achievements"
            };

            foreach (string dir in existingDirectories)
            {
                string target = $"{PluginInfo.BaseDirectory}{dir}";
                if (!Directory.Exists(target))
                    Directory.CreateDirectory(target);
            }

            LogManager.Log("Quantum Menu: Patching early security hooks...");
            try
            {
                PatchHandler.PatchAll(true);
            }
            catch (Exception ex)
            {
                LogManager.LogError($"Quantum Menu: Early patching failed: {ex}");
            }

            if (File.Exists($"{PluginInfo.BaseDirectory}/Quantum_Preferences.txt"))
            {
                LogManager.Log("Quantum Menu: Loading preferences...");
                try
                {
                    if (File.ReadAllLines($"{PluginInfo.BaseDirectory}/Quantum_Preferences.txt")[0]
                        .Split(";;")
                        .Contains("Accept TOS"))
                    {
                        TOSPatches.enabled = true;
                    }
                }
                catch (Exception ex)
                {
                    LogManager.LogError($"Quantum Menu: Preference loading failed: {ex}");
                }
            }

            if (File.Exists($"{PluginInfo.BaseDirectory}/Quantum_DisableTelemetry.txt"))
                ServerData.DisableTelemetry = true;

            LogManager.Log("Quantum Menu: Starting spawn wait coroutine...");
            GameObject bootstrapperObject = new GameObject("Quantum_Bootstrapper");
            bootstrapperObject.AddComponent<CoroutineManager>().StartCoroutine(WaitForPlayer());
            UnityEngine.Object.DontDestroyOnLoad(bootstrapperObject);
        }

        private static IEnumerator WaitForPlayer()
        {
            while (GorillaTagger.Instance == null)
                yield return new WaitForSeconds(0.1f);
            
            LogManager.Log("Quantum Menu: Player spawned! Loading menu...");
            try
            {
                LoadMenu();
            }
            catch (Exception ex)
            {
                LogManager.LogError($"Quantum Menu: FATAL ERROR during LoadMenu: {ex}");
            }
        }

        private static void LoadMenu()
        {
            LogManager.Log("Quantum Menu: LoadMenu() started.");
            PatchHandler.PatchAll();

            Loader = new GameObject("Quantum_Loader");
            CoroutineManager coroutineManager = Loader.AddComponent<CoroutineManager>();
            Loader.AddComponent<NotificationManager>();
            Loader.AddComponent<CustomBoardManager>();
            Loader.AddComponent<UI>();
            Loader.AddComponent<Main>();
            UnityEngine.Object.DontDestroyOnLoad(Loader);

            LogManager.Log("Quantum Menu: Loader components added.");
            coroutineManager.StartCoroutine(PatchIntegrityCheck());
            LogManager.Log("Quantum Menu: Initialization complete!");
        }

        private static IEnumerator PatchIntegrityCheck()
        {
            if (PatchHandler.instance == null)
                yield return null;

            PatchHandler.PatchIntegrityCheck();
        }
    }
}
