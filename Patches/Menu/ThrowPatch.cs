/*
 * Quantum Menu  Patches/Menu/ThrowPatch.cs
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
using UnityEngine;
using static Quantum.Utilities.RandomUtilities;

namespace Quantum.Patches.Menu
{
    [HarmonyPatch(typeof(GrowingSnowballThrowable), nameof(GrowingSnowballThrowable.PerformSnowballThrowAuthority))]
    public class ThrowPatch
    {
        public static bool enabled;
        public static readonly int extraCount = 5;

        public static bool Prefix(GrowingSnowballThrowable __instance)
        {
            if (enabled)
            {
                enabled = false;

                Vector3 originalLinearVelocity = __instance.velocityEstimator.linearVelocity;
                for (int i = 0; i < extraCount; i++)
                {
                    __instance.velocityEstimator.linearVelocity = originalLinearVelocity + RandomVector3(2f);
                    __instance.PerformSnowballThrowAuthority();
                }

                enabled = true;

                return false;
            }

            return true;
        }
    }
}

