using GorillaLocomotion;
using HarmonyLib;
using Quantum.Managers;
using System;
using System.Collections.Generic;

namespace Quantum.Patches.Menu
{
    [HarmonyPatch("GorillaLocomotion.GTPlayer", "GetSlidePercentage")]
    public class SlidePatch
    {
        public static bool everythingSlippery;
        public static bool minimalSlip;
        public static bool everythingGrippy;

        public static void Postfix(ref float __result)
        {
            if (everythingSlippery)
                __result = 1;

            if (minimalSlip)
                __result = 0.001f;

            if (everythingGrippy)
                __result = 0.000000001f;
        }
    }
}

