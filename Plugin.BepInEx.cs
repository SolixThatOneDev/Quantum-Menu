/*
 * Quantum Menu  Plugin.BepInEx.cs
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

using BepInEx;
using Quantum.Managers;
using Quantum.Menu;
using System;
using System.ComponentModel;

namespace Quantum
{
    [Description(PluginInfo.Description)]
    [BepInPlugin("org.quantum.gorillatag.quantummenu", "Quantum Menu", "1.1.4")]
    public class PluginBepInEx : BaseUnityPlugin
    {
        static PluginBepInEx()
        {
            // Initial log to confirm the class is even being loaded
            UnityEngine.Debug.Log("[Quantum Menu] Static Constructor executed!");
        }

        public static bool FirstLaunch;

        private void Awake()
        {
            LogManager.SetLogger((level, msg) =>
            {
                switch (level)
                {
                    case Level.Error:
                        Logger.LogError(msg);
                        break;
                    case Level.Warning:
                        Logger.LogWarning(msg);
                        break;
                    case Level.Debug:
                        Logger.LogDebug(msg);
                        break;
                    default:
                        Logger.LogInfo(msg);
                        break;
                }
            });

            LogManager.Log("Quantum Menu: Awake started!");
            try
            {
                Bootstrapper.Initialize();
                LogManager.Log("Quantum Menu: Awake finished successfully!");
            }
            catch (Exception ex)
            {
                LogManager.LogError($"Quantum Menu: CRITICAL ERROR in Awake: {ex}");
            }
        }


        private void OnDestroy() =>
            Main.UnloadMenu();
    }
}

