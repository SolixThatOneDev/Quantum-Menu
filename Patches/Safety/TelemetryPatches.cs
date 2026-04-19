/*
 * Quantum Menu  Patches/Safety/TelemetryPatches.cs
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
using JetBrains.Annotations;
using Liv.Lck.Telemetry;
using PlayFab;
using PlayFab.EventsModels;
using System.Collections.Generic;
using static Quantum.Patches.PatchHandler;

namespace Quantum.Patches.Safety
{
    // Gorilla Tag's one weakness -- tracking data to get players banned. This is how they did it over the years.
    public class TelemetryPatches
    {
        public static bool enabled = true;

        [PatchOnAwake]
        [HarmonyPatch(typeof(GorillaTelemetry), nameof(GorillaTelemetry.EnqueueTelemetryEvent))]
        public class EnqueueTelemetryEvent
        {
            private static bool Prefix(string eventName, object content, [CanBeNull] string[] customTags = null) =>
                !enabled;
        }

/*
        [PatchOnAwake]
        [HarmonyPatch(typeof(GorillaTelemetry), nameof(GorillaTelemetry.EnqueueTelemetryEventPlayFab))]
        public class EnqueueTelemetryEventPlayFab
        {
            private static bool Prefix(EventContents eventContent) =>
                !enabled;
        }
*/

/*
        [PatchOnAwake]
        [HarmonyPatch(typeof(GorillaTelemetry), nameof(GorillaTelemetry.FlushPlayFabTelemetry))]
        public class FlushPlayFabTelemetry
        {
            private static bool Prefix() =>
                !enabled;
        }
*/

        [PatchOnAwake]
        [HarmonyPatch(typeof(GorillaTelemetry), nameof(GorillaTelemetry.FlushMothershipTelemetry))]
        public class FlushMothershipTelemetry
        {
            private static bool Prefix() =>
                !enabled;
        }

        [PatchOnAwake]
        [HarmonyPatch(typeof(LckTelemetryClient), nameof(LckTelemetryClient.SendTelemetry))]
        public class SendTelemetry
        {
            private static bool Prefix(LckTelemetryEvent lckTelemetryEvent) =>
                !enabled;
        }

        [PatchOnAwake]
        [HarmonyPatch(typeof(PlayFabEventsAPI), nameof(PlayFabEventsAPI.WriteTelemetryEvents))]
        public class WriteTelemetryEvents
        {
            private static bool Prefix(WriteEventsRequest request, System.Action<WriteEventsResponse> resultCallback, System.Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null) =>
                !enabled;
        }
    }
}

