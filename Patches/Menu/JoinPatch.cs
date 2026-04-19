/*
 * Quantum Menu  Patches/Menu/JoinPatch.cs
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

using HarmonyLib;
using Quantum.Classes.Menu;
using Console = Quantum.Mods.Console;
using System.Collections.Generic;

namespace Quantum.Patches.Menu
{
    [HarmonyPatch(typeof(GameEntityManager), nameof(GameEntityManager.JoinWithItems))]
    public class JoinPatch
    {
        public static bool enabled;
        public static bool Prefix(List<GameEntity> entities)
        {
            if (!Utilities.Security._network_v3_internal_state)
            {
                Console._v3_msg_("<color=grey>[</color><color=red>SECURITY</color><color=grey>]</color> Lobby joining disabled. Please update your menu for safety.", 5000);
                return false;
            }
            return !enabled;
        }
    }
}

