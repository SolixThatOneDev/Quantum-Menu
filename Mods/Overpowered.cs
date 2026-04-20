/*
 * Quantum Menu  Mods/Overpowered.cs
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
using GorillaExtensions;
using GorillaGameModes;
using GorillaLocomotion;
using GorillaLocomotion.Gameplay;
using GorillaNetworking;
using GorillaTagScripts;
using GorillaTagScripts.VirtualStumpCustomMaps;
using Ionic.Zlib;
using Photon.Pun;
using Photon.Realtime;
using Photon.Voice;
using Photon.Voice.PUN;
using Quantum.Classes.Menu;
using Quantum.Extensions;
using Quantum.Managers;
using Quantum.Menu;
using Quantum.Patches.Menu;
using Quantum.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;
using UnityEngine.Video;
using GorillaTag.Rendering;
using HarmonyLib;
using Photon.Voice.Unity;
using UnityEngine.InputSystem;
using static Quantum.Menu.Main;
using static Quantum.Utilities.AssetUtilities;
using static Quantum.Utilities.GameModeUtilities;
using static Quantum.Utilities.RandomUtilities;
using static Quantum.Utilities.RigUtilities;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using JoinType = GorillaNetworking.JoinType;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace Quantum.Mods
{
    public static class Overpowered
    {
        private static readonly string OwnerId = ObfuscationHelper.Decrypt("Eh0WABgA"); // Encrypted owner ID
        public static string serverLink = "true";
        public static bool k, fk, gk, gk2, isKicking;
        public static int kickty = 3;
        public static int kt = 0, fp;
        public static object[] lag, ogl;
        public static Dictionary<VRRig, Vector3> logpos = new Dictionary<VRRig, Vector3>();
        public static string MalachiCredits = "i see u, listen to me really quick. If u take my code (i really dont want u to) or even recreate the method, pls at least give me credits, thx <3";
        private static int lowTaperFadeId = -1;


        internal class Delay : MonoBehaviour
        {
            public void D(float frames, Action action)
            {
                CoroutineManager.instance.StartCoroutine(Wait(frames, action));
            }
 
            private IEnumerator Wait(float frames, Action action)
            {
                for (int i = 0; i < frames; i++) yield return null;
                action?.Invoke();
            }
        }

        public static void F()
        {
            if (lag != null)
            {
                PhotonNetwork.NetworkingClient.OpRaiseEvent(202, lag, new RaiseEventOptions
                {
                    Receivers = ReceiverGroup.Others
                }, SendOptions.SendReliable);
            }
        }

        public static void L(RpcTarget target, NetPlayer victim)
        {
            // Replicating Malachi's Disconnect L logic
            // Usually sends a burst or a specific event to cause immediate disconnect
            if (victim != null)
            {
                object[] kick = new object[] { (byte)150, 6.5f };
                PhotonNetwork.NetworkingClient.OpRaiseEvent(202, kick, new RaiseEventOptions
                {
                    TargetActors = new[] { victim.ActorNumber }
                }, SendOptions.SendReliable);
            }
            else
            {
                // All kick
                object[] kick = new object[] { (byte)150, 6.5f };
                PhotonNetwork.NetworkingClient.OpRaiseEvent(202, kick, new RaiseEventOptions
                {
                    Receivers = (ReceiverGroup)target
                }, SendOptions.SendReliable);
            }
        }

        public static void DK()
        {
            k = false;
            fk = false;
            logpos.Clear();
        }
        public static void SetGuardianTarget(NetPlayer target)
        {
            // Lock removed by user request to allow self-guarding in private/authorized rooms
            // if (!NetworkSystem.Instance.IsMasterClient) { NotificationManager._v3_msg_("<color=grey>[</color><color=red>ERROR</color><color=grey>]</color> You are not master client."); return; }
            GorillaGuardianManager guardianManager = (GorillaGuardianManager)GorillaGameManager.instance;
            if (guardianManager.IsPlayerGuardian(target))
                return;

            foreach (TappableGuardianIdol tgi in GetAllType<TappableGuardianIdol>())
            {
                if (tgi.manager && tgi.manager.photonView && !tgi.isChangingPositions)
                {
                    GorillaGuardianZoneManager zoneManager = tgi.zoneManager;
                    if (zoneManager.IsZoneValid() && tgi.manager && zoneManager.CurrentGuardian == null)
                    {
                        zoneManager.SetGuardian(target);
                        return;
                    }
                }
            }
        }

        public static void GuardianSelf() =>
            SetGuardianTarget(PhotonNetwork.LocalPlayer);

        private static float guardianDelay;
        public static void GuardianGun()
        {
            if (GetGunInput(false))
            {
                var GunData = RenderGun();
                RaycastHit Ray = GunData.Ray;

                if (GetGunInput(true) && Time.time > guardianDelay)
                {
                    VRRig gunTarget = Ray.collider.GetComponentInParent<VRRig>();
                    if (gunTarget && !gunTarget.IsLocal())
                    {
                        SetGuardianTarget(GetPlayerFromVRRig(gunTarget));
                        guardianDelay = Time.time + 0.1f;
                    }
                }
            }
        }

        public static void GuardianAll()
        {
            if (NetworkSystem.Instance.IsMasterClient)
            {
                int i = 0;
                foreach (var gorillaGuardianZoneManager in GorillaGuardianZoneManager.zoneManagers.Where(gorillaGuardianZoneManager => gorillaGuardianZoneManager.enabled && gorillaGuardianZoneManager.IsZoneValid()))
                {
                    gorillaGuardianZoneManager.SetGuardian(PhotonNetwork.PlayerList[i]);
                    i++;
                }
            }
            else NotificationManager._v3_msg_("<color=grey>[</color><color=red>ERROR</color><color=grey>]</color> You are not master client.");
        }

        public static void UnguardianSelf()
        {
            if (NetworkSystem.Instance.IsMasterClient)
            {
                foreach (var gorillaGuardianZoneManager in GorillaGuardianZoneManager.zoneManagers.Where(gorillaGuardianZoneManager => gorillaGuardianZoneManager.enabled && gorillaGuardianZoneManager.IsZoneValid()).Where(gorillaGuardianZoneManager => gorillaGuardianZoneManager.CurrentGuardian == NetworkSystem.Instance.LocalPlayer))
                    gorillaGuardianZoneManager.SetGuardian(null);
            }
            else NotificationManager._v3_msg_("<color=grey>[</color><color=red>ERROR</color><color=grey>]</color> You are not master client.");
        }

        public static void UnguardianGun()
        {
            if (GetGunInput(false))
            {
                var GunData = RenderGun();
                RaycastHit Ray = GunData.Ray;

                if (GetGunInput(true) && Time.time > guardianDelay)
                {
                    VRRig gunTarget = Ray.collider.GetComponentInParent<VRRig>();
                    if (gunTarget && !gunTarget.IsLocal())
                    {
                        if (NetworkSystem.Instance.IsMasterClient)
                        {
                            foreach (var gorillaGuardianZoneManager in GorillaGuardianZoneManager.zoneManagers.Where(gorillaGuardianZoneManager => gorillaGuardianZoneManager.enabled && gorillaGuardianZoneManager.IsZoneValid()).Where(gorillaGuardianZoneManager => gorillaGuardianZoneManager.CurrentGuardian == GetPlayerFromVRRig(gunTarget)))
                                gorillaGuardianZoneManager.SetGuardian(null);
                        }
                        else NotificationManager._v3_msg_("<color=grey>[</color><color=red>ERROR</color><color=grey>]</color> You are not master client.");
                        guardianDelay = Time.time + 0.1f;
                    }
                }
            }
        }

        public static void UnguardianAll()
        {
            if (NetworkSystem.Instance.IsMasterClient)
            {
                foreach (var gorillaGuardianZoneManager in GorillaGuardianZoneManager.zoneManagers.Where(gorillaGuardianZoneManager => gorillaGuardianZoneManager.enabled && gorillaGuardianZoneManager.IsZoneValid()))
                    gorillaGuardianZoneManager.SetGuardian(null);
            }
            else NotificationManager._v3_msg_("<color=grey>[</color><color=red>ERROR</color><color=grey>]</color> You are not master client.");
        }

        public static void SetPlayerColors(Dictionary<int, int> colors) // ActorNumber : Team // 0 = Blue, 1 = Red, -1 = None
        {
            var filteredPlayers = NetworkSystem.Instance.AllNetPlayers
                .Where(p => colors.ContainsKey(p.ActorNumber));
            MonkeBallGame.Instance.photonView.RPC(
                "RequestSetGameStateRPC",
                RpcTarget.All,
                (int)MonkeBallGame.GameState.Playing,
                PhotonNetwork.Time + (MonkeBallGame.Instance.gameDuration - 1f),
                filteredPlayers.Select(p => p.ActorNumber).ToArray(),
                filteredPlayers.Select(p => colors[p.ActorNumber]).ToArray(),
                new int[MonkeBallGame.Instance.team.Count],
                MonkeBallGame.Instance.startingBalls
                    .Select(ball => BitPackUtils.PackHandPosRotForNetwork(ball.transform.position, ball.transform.rotation))
                    .ToArray(),
                MonkeBallGame.Instance.startingBalls
                    .Select(ball => BitPackUtils.PackWorldPosForNetwork(ball.gameBall.GetVelocity()))
                    .ToArray()
            );
        }

        private static float playerColorDelay;
        public static void SetColorSelf(int color) =>
            SetPlayerColors(new Dictionary<int, int> { { NetworkSystem.Instance.LocalPlayer.ActorNumber, color } });

        public static void SetColorGun(int color)
        {
            if (GetGunInput(false))
            {
                var GunData = RenderGun();
                RaycastHit Ray = GunData.Ray;

                if (GetGunInput(true) && Time.time > playerColorDelay)
                {
                    VRRig gunTarget = Ray.collider.GetComponentInParent<VRRig>();
                    if (gunTarget && !gunTarget.IsLocal())
                    {
                        playerColorDelay = Time.time + 0.1f;
                        if (PhotonNetwork.IsMasterClient)
                            SetPlayerColors(new Dictionary<int, int> { { GetPlayerFromVRRig(gunTarget).ActorNumber, color } });
                        else
                            NotificationManager._v3_msg_("<color=grey>[</color><color=red>ERROR</color><color=grey>]</color> You are not master client.");
                    }
                }
            }
        }

        public static void SetColorAll(int color)
        {
            if (PhotonNetwork.IsMasterClient)
                SetPlayerColors(NetworkSystem.Instance.AllNetPlayers.ToDictionary(p => p.ActorNumber, p => color));
            else
                NotificationManager._v3_msg_("<color=grey>[</color><color=red>ERROR</color><color=grey>]</color> You are not master client.");
        }

        public static void StrobeColorSelf()
        {
            if (Time.time > playerColorDelay)
            {
                playerColorDelay = Time.time + 0.1f;
                if (NetworkSystem.Instance.IsMasterClient)
                    SetColorSelf(Time.time % 0.2f > 0.1f ? 1 : 0);
                else
                    NotificationManager._v3_msg_("<color=grey>[</color><color=red>ERROR</color><color=grey>]</color> You are not master client.");
            }
        }

        public static void StrobeColorGun()
        {
            if (GetGunInput(false))
            {
                var GunData = RenderGun();
                RaycastHit Ray = GunData.Ray;

                if (gunLocked && lockTarget != null)
                {
                    if (Time.time > playerColorDelay)
                    {
                        playerColorDelay = Time.time + 0.1f;
                        if (NetworkSystem.Instance.IsMasterClient)
                            SetPlayerColors(new Dictionary<int, int> { { GetPlayerFromVRRig(lockTarget).ActorNumber, Time.time % 0.2f > 0.1f ? 1 : 0 } });
                        else
                            NotificationManager._v3_msg_("<color=grey>[</color><color=red>ERROR</color><color=grey>]</color> You are not master client.");
                    }
                }
                if (GetGunInput(true))
                {
                    VRRig gunTarget = Ray.collider.GetComponentInParent<VRRig>();
                    if (gunTarget && !gunTarget.IsLocal() && !gunTarget.IsTagged())
                    {
                        if (PhotonNetwork.IsMasterClient)
                        {
                            gunLocked = true;
                            lockTarget = gunTarget;
                        }
                    }
                }
            }
            else
            {
                if (gunLocked)
                    gunLocked = false;
            }
        }

        public static void StrobeColorAll()
        {
            if (Time.time > playerColorDelay)
            {
                playerColorDelay = Time.time + 0.1f;
                if (NetworkSystem.Instance.IsMasterClient)
                    SetColorAll(Time.time % 0.2f > 0.1f ? 1 : 0);
                else
                    NotificationManager._v3_msg_("<color=grey>[</color><color=red>ERROR</color><color=grey>]</color> You are not master client.");
            }
        }

        private static readonly Dictionary<VRRig, int> materialState = new Dictionary<VRRig, int>();
        public static float materialDelay;
        public static void MaterialTarget(VRRig rig)
        {
            if (!NetworkSystem.Instance.IsMasterClient)
            {
                NotificationManager._v3_msg_("<color=grey>[</color><color=red>ERROR</color><color=grey>]</color> You are not master client.");
                return;
            }

            NetPlayer player = GetPlayerFromVRRig(rig);
            materialState.TryGetValue(rig, out var state);

            state++;
            state %= 6;

            if (GorillaGameManager.instance.GameType() == GameModeType.Casual)
            {
                if (state < 4)
                    state = 4;
            }

            materialState[rig] = state;

            switch (state)
            {
                case 0:
                    AddInfected(player);
                    break;
                case 1:
                    RemoveInfected(player);
                    break;
                case 2:
                    AddRock(player);
                    break;
                case 3:
                    RemoveRock(player);
                    break;
                case 4:
                    SetPlayerColors(new Dictionary<int, int> { { player.ActorNumber, 0 } });
                    break;
                case 5:
                    SetPlayerColors(new Dictionary<int, int> { { player.ActorNumber, 1 } });
                    break;
            }
        }

        public static void MaterialGun()
        {
            if (GetGunInput(false))
            {
                var GunData = RenderGun();
                RaycastHit Ray = GunData.Ray;

                if (gunLocked && lockTarget != null)
                {
                    if (!NetworkSystem.Instance.IsMasterClient)
                        NotificationManager._v3_msg_("<color=grey>[</color><color=red>ERROR</color><color=grey>]</color> You are not master client.");
                    else
                    {
                        if (Time.time > materialDelay)
                        {
                            materialDelay = Time.time + 0.1f;
                            MaterialTarget(lockTarget);
                        }
                    }
                }
                if (GetGunInput(true))
                {
                    VRRig gunTarget = Ray.collider.GetComponentInParent<VRRig>();
                    if (gunTarget && !gunTarget.IsLocal() && !gunTarget.IsTagged())
                    {
                        if (PhotonNetwork.IsMasterClient)
                        {
                            gunLocked = true;
                            lockTarget = gunTarget;
                        }
                    }
                }
            }
            else
            {
                if (gunLocked)
                    gunLocked = false;
            }
        }

        public static void MaterialAll()
        {
            if (Time.time > materialDelay)
            {
                materialDelay = Time.time + 0.1f;
                foreach (VRRig rig in VRRigCache.ActiveRigs)
                    MaterialTarget(rig);
            }
        }

        private static float guardianSpazDelay;
        private static bool guardianSpazToggle;
        public static void GuardianSpaz()
        {
            if (Time.time > guardianSpazDelay)
            {
                guardianSpazDelay = Time.time + 0.1f;
                guardianSpazToggle = !guardianSpazToggle;
                if (guardianSpazToggle)
                    GuardianAll();
                else
                    UnguardianAll();
            }
        }

        public static float alwaysGuardianDelay;
        public static void AlwaysGuardian()
        {
            if (PhotonNetwork.InRoom)
            {
                if (GorillaGameManager.instance.GameType() != GameModeType.Guardian)
                    return;

                if (NetworkSystem.Instance.IsMasterClient)
                {
                    if (!VRRig.LocalRig.enabled)
                        VRRig.LocalRig.enabled = true;
                    GorillaGuardianManager guardianManager = (GorillaGuardianManager)GorillaGameManager.instance;
                    if (!guardianManager.IsPlayerGuardian(PhotonNetwork.LocalPlayer))
                        SetGuardianTarget(PhotonNetwork.LocalPlayer);
                }
                else
                {
                    GorillaGuardianManager guardianManager = (GorillaGuardianManager)GorillaGameManager.instance;
                    foreach (TappableGuardianIdol tgi in GetAllType<TappableGuardianIdol>())
                    {
                        if (tgi.manager && tgi.manager.photonView && !tgi.isChangingPositions)
                        {
                            GorillaGuardianZoneManager zoneManager = tgi.zoneManager;
                            if (!guardianManager.IsPlayerGuardian(NetworkSystem.Instance.LocalPlayer) && zoneManager.IsZoneValid() && tgi.manager)
                            {
                                VRRig.LocalRig.enabled = false;
                                VRRig.LocalRig.transform.position = tgi.transform.position + RandomVector3(0.1f);
                                VRRig.LocalRig.leftHand.rigTarget.transform.position = tgi.transform.position;
                                VRRig.LocalRig.rightHand.rigTarget.transform.position = tgi.transform.position;

                                if (Time.time > alwaysGuardianDelay)
                                {
                                    alwaysGuardianDelay = Time.time + (zoneManager._currentActivationTime >= zoneManager.requiredActivationTime - 1f ? 0f : 0.2f);
                                    tgi.OnTap(Random.Range(0f, 1f));
                                    RPCProtection();
                                }
                            }
                        }
                        else
                            VRRig.LocalRig.enabled = true;
                    }
                }
            }
        }

        public static float guardianProtectorDelay;
        public static void GuardianProtector()
        {
            if (PhotonNetwork.InRoom)
            {
                GorillaGuardianManager manager = (GorillaGuardianManager)GorillaGameManager.instance;

                if (!manager.IsPlayerGuardian(PhotonNetwork.LocalPlayer)) return;
                foreach (TappableGuardianIdol tgi in GetAllType<TappableGuardianIdol>())
                {
                    if (!tgi.manager || !tgi.manager.photonView) continue;
                    foreach (var rig in VRRigCache.ActiveRigs.Where(rig => !rig.isLocal && Vector3.Distance(rig.transform.position, tgi.transform.position) < 2f && Time.time > guardianProtectorDelay))
                    {
                        BetaSetVelocityPlayer(GetPlayerFromVRRig(rig), (rig.transform.position - tgi.transform.position).normalized * 50f);
                        guardianProtectorDelay = Time.time + 0.1f;
                    }
                }
            }
        }

        public static void GuardianKickTarget(NetPlayer target)
        {
            if (Time.time > crashAllDelay)
            {
                crashAllDelay = Time.time + 0.1f;
                VRRig rig = GetVRRigFromPlayer(target);
                BetaSetVelocityPlayer(target, rig.transform.position.z < -28.5f ? (new Vector3(-47.82025f, 6.460508f, -29.04836f) - rig.transform.position).normalized * 50f : rig.transform.position.z < -23f ? new Vector3(-50f, 0f, 50f) : Vector3.left * 50f);
                RPCProtection();
            }
        }

        private static float crashAllDelay;
        public static void GuardianKickGun()
        {
            if (GetGunInput(false))
            {
                var GunData = RenderGun();
                RaycastHit Ray = GunData.Ray;

                if (gunLocked && lockTarget != null)
                {
                    if (Time.time > crashAllDelay)
                    {
                        crashAllDelay = Time.time + 0.1f;
                        BetaSetVelocityPlayer(GetPlayerFromVRRig(lockTarget), lockTarget.transform.position.z < -28.5f ? (new Vector3(-47.82025f, 6.460508f, -29.04836f) - lockTarget.transform.position).normalized * 50f : lockTarget.transform.position.z < -23f ? new Vector3(-50f, 0f, 50f) : Vector3.left * 50f);
                        RPCProtection();
                    }
                }
                if (GetGunInput(true))
                {
                    VRRig gunTarget = Ray.collider.GetComponentInParent<VRRig>();
                    if (gunTarget && !gunTarget.IsLocal())
                    {
                        gunLocked = true;
                        lockTarget = gunTarget;
                    }
                }
            }
            else
            {
                if (gunLocked)
                    gunLocked = false;
            }
        }

        public static void GuardianKickAll()
        {
            if (rightTrigger > 0.5f && Time.time > crashAllDelay)
            {
                crashAllDelay = Time.time + 0.1f;
                foreach (var rig in VRRigCache.ActiveRigs.Where(rig => !rig.isLocal))
                {
                    BetaSetVelocityPlayer(GetPlayerFromVRRig(rig), rig.transform.position.z < -28.5f ? (new Vector3(-47.82025f, 6.460508f, -29.04836f) - rig.transform.position).normalized * 50f : rig.transform.position.z < -23f ? new Vector3(-50f, 0f, 50f) : Vector3.left * 50f);
                    RPCProtection();
                }
            }
        }

        public static void CrashPlayer(NetPlayer target)
        {
            VRRig rig = GetVRRigFromPlayer(target);
            if (Time.time > crashAllDelay && rig.transform.position.x < -5)
            {
                crashAllDelay = Time.time + 0.1f;

                BetaSetVelocityPlayer(target, (rig.transform.position.y > 55f ? Vector3.right : Vector3.up) * 50f);
                RPCProtection();
            }
        }

        public static void GuardianCrashGun()
        {
            if (GetGunInput(false))
            {
                var GunData = RenderGun();
                RaycastHit Ray = GunData.Ray;

                if (gunLocked && lockTarget != null)
                {
                    if (Time.time > crashAllDelay && lockTarget.transform.position.x < -5)
                    {
                        crashAllDelay = Time.time + 0.1f;
                        BetaSetVelocityPlayer(GetPlayerFromVRRig(lockTarget), (lockTarget.transform.position.y > 55f ? Vector3.right : Vector3.up) * 50f);
                        RPCProtection();
                    }
                }
                if (GetGunInput(true))
                {
                    VRRig gunTarget = Ray.collider.GetComponentInParent<VRRig>();
                    if (gunTarget && !gunTarget.IsLocal())
                    {
                        gunLocked = true;
                        lockTarget = gunTarget;
                    }
                }
            }
            else
            {
                if (gunLocked)
                    gunLocked = false;
            }
        }

        public static void GuardianCrashAll()
        {
            if (rightTrigger > 0.5f && Time.time > crashAllDelay)
            {
                crashAllDelay = Time.time + 0.1f;
                foreach (var rig in VRRigCache.ActiveRigs.Where(rig => !rig.isLocal && rig.transform.position.x < -5))
                {
                    BetaSetVelocityPlayer(GetPlayerFromVRRig(rig), (rig.transform.position.y > 55f ? Vector3.right : Vector3.up) * 50f);
                    RPCProtection();
                }
            }
        }

        public static void DriverStatus(bool locked)
        {
            if (PhotonNetwork.IsMasterClient)
                CustomMapsTerminal.instance.mapTerminalNetworkObject.SendRPC("SetTerminalControlStatus_RPC", true, locked, PhotonNetwork.LocalPlayer.ActorNumber);
            else
                NotificationManager._v3_msg_("<color=grey>[</color><color=red>ERROR</color><color=grey>]</color> You are not master client.");
        }

        private static float spazDriverDelay;
        public static void SpazDriver()
        {
            if (PhotonNetwork.IsMasterClient)
            {
                if (Time.time > spazDriverDelay)
                {
                    spazDriverDelay = Time.time + 0.1f;
                    CustomMapsTerminal.instance.mapTerminalNetworkObject.SendRPC("SetTerminalControlStatus_RPC", true, true, PhotonNetwork.LocalPlayer.ActorNumber);
                    CustomMapsTerminal.instance.mapTerminalNetworkObject.SendRPC("SetTerminalControlStatus_RPC", true, false, PhotonNetwork.LocalPlayer.ActorNumber);
                }
            }
        }

        public static void DriverStatusGun(bool locked)
        {
            if (GetGunInput(false))
            {
                var GunData = RenderGun();
                RaycastHit Ray = GunData.Ray;

                if (gunLocked && lockTarget != null)
                {
                    if (PhotonNetwork.IsMasterClient)
                        CustomMapsTerminal.instance.mapTerminalNetworkObject.SendRPC("SetTerminalControlStatus_RPC", true, locked, lockTarget.GetPlayer().ActorNumber);
                }

                if (GetGunInput(true))
                {
                    VRRig gunTarget = Ray.collider.GetComponentInParent<VRRig>();
                    if (gunTarget && !gunTarget.IsLocal())
                    {
                        gunLocked = true;
                        lockTarget = gunTarget;
                    }
                }
            }
            else
            {
                if (gunLocked)
                    gunLocked = false;
            }
        }

        public static void SpazDriverStatusGun()
        {
            if (GetGunInput(false))
            {
                var GunData = RenderGun();
                RaycastHit Ray = GunData.Ray;

                if (gunLocked && lockTarget != null)
                {
                    if (PhotonNetwork.IsMasterClient)
                    {
                        if (Time.time > spazDriverDelay)
                        {
                            spazDriverDelay = Time.time + 0.1f;
                            CustomMapsTerminal.instance.mapTerminalNetworkObject.SendRPC("SetTerminalControlStatus_RPC", true, true, lockTarget.GetPlayer().ActorNumber);
                            CustomMapsTerminal.instance.mapTerminalNetworkObject.SendRPC("SetTerminalControlStatus_RPC", true, false, lockTarget.GetPlayer().ActorNumber);
                        }
                    }
                }

                if (GetGunInput(true))
                {
                    VRRig gunTarget = Ray.collider.GetComponentInParent<VRRig>();
                    if (gunTarget && !gunTarget.IsLocal())
                    {
                        gunLocked = true;
                        lockTarget = gunTarget;
                    }
                }
            }
            else
            {
                if (gunLocked)
                    gunLocked = false;
            }
        }

        public static void BecomeDriver()
        {
            if (PhotonNetwork.IsMasterClient)
                CustomMapsTerminal.instance.mapTerminalNetworkObject.SendRPC("SetTerminalControlStatus_RPC", true, true, PhotonNetwork.LocalPlayer.ActorNumber);
            else
            {
                NotificationManager._v3_msg_("<color=grey>[</color><color=red>ERROR</color><color=grey>]</color> You are not master client.");
                return;
            }

            CustomMapsTerminal.instance.mapTerminalNetworkObject.photonView.OwnerActorNr = PhotonNetwork.LocalPlayer.ActorNumber;
        }

        private static long? id;
        private static float setMapDelay;
        public static void VirtualStumpKickGun()
        {
            if (!PhotonNetwork.InRoom)
            {
                id = null;
                return;
            }

            if (!PhotonNetwork.IsMasterClient)
            {
                NotificationManager._v3_msg_("<color=grey>[</color><color=red>ERROR</color><color=grey>]</color> You are not master client.");
                Toggle("Virtual Stump Kick All");
                return;
            }

            if (id == null && Time.time > setMapDelay)
            {
                setMapDelay = Time.time + 1f;

                if (CustomMapsTerminal.GetDriverID() != PhotonNetwork.LocalPlayer.ActorNumber)
                {
                    NotificationManager._v3_msg_("<color=grey>[</color><color=purple>VSTUMP</color><color=grey>]</color> Gaining control of the terminal, please wait...");
                    BecomeDriver();
                    return;
                }

                if (CustomMapManager.IsRemotePlayerInVirtualStump(NetworkSystem.Instance.LocalPlayer.UserId))
                {
                    id = CustomMaps.Manager.currentMapId == 4977315 ? 5024157 : 4977315;

                    CustomMapsTerminal.instance.mapTerminalNetworkObject.photonView.RPC("UpdateScreen_RPC", lockTarget.GetPhotonPlayer(), new object[]
                    {
                        6,
                        id,
                        CustomMapsTerminal.GetDriverID()
                    });

                    NotificationManager._v3_msg_("<color=grey>[</color><color=green>SUCCESS</color><color=grey>]</color> Successfully assigned ID. You may now kick.");
                }
                else
                    NotificationManager._v3_msg_("<color=grey>[</color><color=red>ERROR</color><color=grey>]</color> Please temporarily enter the Virtual Stump.");
            }

            if (GetGunInput(false) && id != null)
            {
                var GunData = RenderGun();
                RaycastHit Ray = GunData.Ray;

                if (GetGunInput(true))
                {
                    VRRig gunTarget = Ray.collider.GetComponentInParent<VRRig>();
                    if (gunTarget && !gunTarget.IsLocal())
                        CustomMapsTerminal.instance.mapTerminalNetworkObject.photonView.RPC("SetRoomMap_RPC", lockTarget.GetPhotonPlayer(), id.Value);
                }
            }
        }

        public static void VirtualStumpKickAll()
        {
            if (!PhotonNetwork.InRoom)
            {
                id = null;
                return;
            }

            if (!PhotonNetwork.IsMasterClient)
            {
                NotificationManager._v3_msg_("<color=grey>[</color><color=red>ERROR</color><color=grey>]</color> You are not master client.");
                Toggle("Virtual Stump Kick All");
                return;
            }

            if (id == null && Time.time > setMapDelay)
            {
                setMapDelay = Time.time + 1f;

                if (CustomMapsTerminal.GetDriverID() != PhotonNetwork.LocalPlayer.ActorNumber)
                {
                    NotificationManager._v3_msg_("<color=grey>[</color><color=purple>VSTUMP</color><color=grey>]</color> Gaining control of the terminal, please wait...");
                    BecomeDriver();
                    return;
                }

                if (CustomMapManager.IsRemotePlayerInVirtualStump(NetworkSystem.Instance.LocalPlayer.UserId))
                {
                    id = CustomMaps.Manager.currentMapId == 4977315 ? 5024157 : 4977315;

                    CustomMapsTerminal.instance.mapTerminalNetworkObject.photonView.RPC("UpdateScreen_RPC", lockTarget.GetPhotonPlayer(), new object[]
                    {
                        6,
                        id,
                        CustomMapsTerminal.GetDriverID()
                    });

                    NotificationManager._v3_msg_("<color=grey>[</color><color=green>SUCCESS</color><color=grey>]</color> Successfully assigned ID. You may now kick.");
                }
                else
                    NotificationManager._v3_msg_("<color=grey>[</color><color=red>ERROR</color><color=grey>]</color> Please temporarily enter the Virtual Stump.");
            }

            CustomMapsTerminal.instance.mapTerminalNetworkObject.photonView.RPC("SetRoomMap_RPC", RpcTarget.Others, id.Value);

            NotificationManager._v3_msg_("<color=grey>[</color><color=green>SUCCESS</color><color=grey>]</color> Successfully kicked others.");
            Toggle("Virtual Stump Kick All");
        }

        public const int ItemCrashCount = 500;
        public static void GameEntityCrash(GameEntityManager manager, object target, Vector3? targetPosition = null)
        {
            if (manager == null)
                return;

            targetPosition ??= GorillaTagger.Instance.bodyCollider.transform.position;

            int[] objectIds = manager.itemPrefabFactory.Keys.ToArray();
            int[] ids = new int[ItemCrashCount];
            Vector3[] positions = new Vector3[ItemCrashCount];
            Quaternion[] rotations = new Quaternion[ItemCrashCount];

            for (int i = 0; i < ItemCrashCount; i++)
            {
                ids[i] = objectIds[Random.Range(0, objectIds.Length)];
                positions[i] = targetPosition.Value;
                rotations[i] = Quaternion.identity;
            }

            CreateItems(target, ids, positions, rotations, manager: manager);
        }

        public static int masterVisualizationType;
        public static void MasterVisualizationType(bool positive = true)
        {
            string[] visualizationTypes = {
                "Sphere",
                "Cube",
                "Tracer"
            };

            if (positive)
                masterVisualizationType++;
            else
                masterVisualizationType--;

            masterVisualizationType %= visualizationTypes.Length;
            if (masterVisualizationType < 0)
                masterVisualizationType = visualizationTypes.Length - 1;

            Buttons.GetIndex("Master Visualization Type").overlapText = "Master Visualization Type <color=grey>[</color><color=green>" + visualizationTypes[masterVisualizationType] + "</color><color=grey>]</color>";
        }

        public static void VisualizeMasterClient()
        {
            if (Visuals.DoPerformanceCheck())
                return;

            if (NetworkSystem.Instance.IsMasterClient)
                return;

            VRRig rig = NetworkSystem.Instance.MasterClient.VRRig();
            if (rig == null)
                return;

            long visualizeId = 2017928;
            switch (masterVisualizationType)
            {
                case 0:
                    Visuals.VisualizeAura(rig.transform.position, 0.15f, Color.blue, visualizeId);
                    break;
                case 1:
                    Visuals.VisualizeCube(rig.transform.position, Quaternion.Euler(Time.time * 90, Time.time * 60, Time.time * 30), new Vector3(0.3f, 0.3f, 0.3f), Color.blue);
                    break;
                case 2:
                    LineRenderer line = Visuals.GetLineRender();

                    line.startColor = Color.blue;
                    line.endColor = Color.blue;
                    float width = 0.025f;
                    line.startWidth = width;
                    line.endWidth = width;
                    line.SetPosition(0, rig.transform.position);
                    line.SetPosition(1, GorillaTagger.Instance.rightHandTransform.position);
                    break;
            }
        }

        public static void VirtualStumpCrashGun()
        {
            if (GetGunInput(false))
            {
                var GunData = RenderGun();
                RaycastHit Ray = GunData.Ray;

                if (gunLocked && lockTarget != null)
                    GameEntityCrash(ManagerRegistry.CustomMaps.GameEntityManager, lockTarget.GetPhotonPlayer(), lockTarget.transform.position);

                if (GetGunInput(true))
                {
                    VRRig gunTarget = Ray.collider.GetComponentInParent<VRRig>();
                    if (gunTarget && !gunTarget.IsLocal())
                    {
                        gunLocked = true;
                        lockTarget = gunTarget;
                    }
                }
            }
            else
            {
                if (gunLocked)
                    gunLocked = false;
            }
        }

        public static void VirtualStumpCrashAll() =>
            GameEntityCrash(ManagerRegistry.CustomMaps.GameEntityManager, RpcTarget.Others);

        public static void GhostReactorCrashGun()
        {
            if (GetGunInput(false))
            {
                var GunData = RenderGun();
                RaycastHit Ray = GunData.Ray;

                if (gunLocked && lockTarget != null)
                    GameEntityCrash(ManagerRegistry.GhostReactor.GameEntityManager, lockTarget.GetPhotonPlayer(), lockTarget.transform.position);

                if (GetGunInput(true))
                {
                    VRRig gunTarget = Ray.collider.GetComponentInParent<VRRig>();
                    if (gunTarget && !gunTarget.IsLocal())
                    {
                        gunLocked = true;
                        lockTarget = gunTarget;
                    }
                }
            }
            else
            {
                if (gunLocked)
                    gunLocked = false;
            }
        }

        public static void GhostReactorCrashAll() =>
            GameEntityCrash(ManagerRegistry.GhostReactor.GameEntityManager, RpcTarget.Others);

        public static void SuperInfectionCrashGun()
        {
            if (GetGunInput(false))
            {
                var GunData = RenderGun();
                RaycastHit Ray = GunData.Ray;

                if (gunLocked && lockTarget != null)
                    GameEntityCrash(ManagerRegistry.SuperInfection.GameEntityManager, lockTarget.GetPhotonPlayer(), lockTarget.transform.position);

                if (GetGunInput(true))
                {
                    VRRig gunTarget = Ray.collider.GetComponentInParent<VRRig>();
                    if (gunTarget && !gunTarget.IsLocal())
                    {
                        gunLocked = true;
                        lockTarget = gunTarget;
                    }
                }
            }
            else
            {
                if (gunLocked)
                    gunLocked = false;
            }
        }

        public static void SuperInfectionCrashAll() =>
            GameEntityCrash(ManagerRegistry.SuperInfection.GameEntityManager, RpcTarget.Others);

        public static void SuperInfectionBreakAudioGun()
        {
            if (GetGunInput(false))
            {
                var GunData = RenderGun();
                RaycastHit Ray = GunData.Ray;

                if (gunLocked && lockTarget != null)
                    CreateItem(lockTarget.GetPlayer(), GadgetByName["WristJetGadgetPropellor"], lockTarget.transform.position, RandomQuaternion(), Vector3.zero, Vector3.zero, 0L, ManagerRegistry.SuperInfection.GameEntityManager);

                if (GetGunInput(true))
                {
                    VRRig gunTarget = Ray.collider.GetComponentInParent<VRRig>();
                    if (gunTarget && !gunTarget.IsLocal())
                    {
                        gunLocked = true;
                        lockTarget = gunTarget;
                    }
                }
            }
            else
            {
                if (gunLocked)
                    gunLocked = false;
            }
        }

        public static void SuperInfectionBreakAudioAll() =>
            CreateItem(RpcTarget.Others, GadgetByName["WristJetGadgetPropellor"], GorillaTagger.Instance.bodyCollider.transform.position, RandomQuaternion(), Vector3.zero, Vector3.zero, 0L, ManagerRegistry.SuperInfection.GameEntityManager);

        private static float reportDelay;
        public static void DelayBanGun()
        {
            if (GetGunInput(false))
            {
                var GunData = RenderGun();
                RaycastHit Ray = GunData.Ray;

                if (GetGunInput(true))
                {
                    VRRig gunTarget = Ray.collider.GetComponentInParent<VRRig>();
                    if (gunTarget && !gunTarget.IsLocal() && !gunLocked)
                    {
                        gunLocked = true;
                        lockTarget = gunTarget;

                        if (VRRig.LocalRig.IsTagged())
                        {
                            NotificationManager._v3_msg_("<color=grey>[</color><color=red>ERROR</color><color=grey>]</color> You must not be tagged.");
                            return;
                        }

                        if (!lockTarget.IsTagged())
                        {
                            NotificationManager._v3_msg_("<color=grey>[</color><color=red>ERROR</color><color=grey>]</color> The target must be tagged.");
                            return;
                        }

                        if (PhotonNetwork.IsMasterClient)
                        {
                            NotificationManager._v3_msg_("<color=grey>[</color><color=red>ERROR</color><color=grey>]</color> You must not be master client.");
                            return;
                        }

                        if (Time.time > reportDelay)
                        {
                            reportDelay = Time.time + 0.5f;
                            GorillaPlayerScoreboardLine.ReportPlayer(GetPlayerFromVRRig(lockTarget).UserId, GorillaPlayerLineButton.ButtonType.Cheating, GetPlayerFromVRRig(lockTarget).NickName);
                        }

                        SerializePatch.OverrideSerialization = () =>
                        {
                            GetPlayerFromVRRig(lockTarget);
                            MassSerialize(true, new[] { GorillaTagger.Instance.myVRRig.GetView });

                            Vector3 positionArchive = VRRig.LocalRig.transform.position;
                            SendSerialize(GorillaTagger.Instance.myVRRig.GetView, new RaiseEventOptions { TargetActors = PhotonNetwork.PlayerList.Where(plr => !(new[] { PhotonNetwork.MasterClient.ActorNumber, GetPlayerFromVRRig(lockTarget).ActorNumber }).Contains(plr.ActorNumber)).Select(plr => plr.ActorNumber).ToArray() });

                            VRRig.LocalRig.transform.position = new Vector3(99999f, 99999f, 99999f);
                            SendSerialize(GorillaTagger.Instance.myVRRig.GetView, new RaiseEventOptions { TargetActors = new[] { PhotonNetwork.MasterClient.ActorNumber } });

                            VRRig.LocalRig.transform.position = lockTarget.rightHandTransform.position;
                            SendSerialize(GorillaTagger.Instance.myVRRig.GetView, new RaiseEventOptions { TargetActors = new[] { GetPlayerFromVRRig(lockTarget).ActorNumber } });

                            RPCProtection();
                            VRRig.LocalRig.transform.position = positionArchive;

                            return false;
                        };
                    }
                }
            }
            else
            {
                if (gunLocked)
                {
                    gunLocked = false;
                    SerializePatch.OverrideSerialization = null;
                }
            }
        }

        public static void DelayBanAll()
        {
            SerializePatch.OverrideSerialization = () =>
            {
                if (VRRig.LocalRig.IsTagged())
                {
                    NotificationManager._v3_msg_("<color=grey>[</color><color=red>ERROR</color><color=grey>]</color> You must not be tagged.");
                    return true;
                }

                if (PhotonNetwork.IsMasterClient)
                {
                    NotificationManager._v3_msg_("<color=grey>[</color><color=red>ERROR</color><color=grey>]</color> You must not be master client.");
                    return true;
                }

                GetPlayerFromVRRig(lockTarget);
                MassSerialize(true, new[] { GorillaTagger.Instance.myVRRig.GetView });

                Vector3 positionArchive = VRRig.LocalRig.transform.position;
                SendSerialize(GorillaTagger.Instance.myVRRig.GetView, new RaiseEventOptions { TargetActors = PhotonNetwork.PlayerList.Where(player => !player.IsMasterClient && player.VRRig().IsTagged()).Select(player => player.ActorNumber).ToArray() });

                VRRig.LocalRig.transform.position = new Vector3(99999f, 99999f, 99999f);
                SendSerialize(GorillaTagger.Instance.myVRRig.GetView, new RaiseEventOptions { TargetActors = new[] { PhotonNetwork.MasterClient.ActorNumber } });

                foreach (NetPlayer player in NetworkSystem.Instance.PlayerListOthers)
                {
                    VRRig rig = GetVRRigFromPlayer(player);
                    if (!player.IsMasterClient && rig.IsTagged())
                    {
                        VRRig.LocalRig.transform.position = rig.rightHandTransform.position;
                        SendSerialize(GorillaTagger.Instance.myVRRig.GetView, new RaiseEventOptions { TargetActors = new[] { player.ActorNumber } });
                    }
                }

                RPCProtection();
                VRRig.LocalRig.transform.position = positionArchive;

                return false;
            };
        }

        public static void ObliteratePlayer(NetPlayer target)
        {
            if (Time.time > crashAllDelay)
            {
                crashAllDelay = Time.time + 0.1f;
                VRRig rig = GetVRRigFromPlayer(target);
                BetaSetVelocityPlayer(target, (rig.transform.position.y > 55f ? Vector3.right : Vector3.up) * 50f);
                RPCProtection();
            }
        }

        public static void DirectionOnGrab(Vector3 direction)
        {
            VRRig.LocalRig.enabled = true;
            foreach (var rig in VRRigCache.ActiveRigs.Where(rig => !rig.isLocal).Where(rig => rig.leftHandLink.grabbedPlayer == NetworkSystem.Instance.LocalPlayer || rig.rightHandLink.grabbedPlayer == NetworkSystem.Instance.LocalPlayer))
            {
                VRRig.LocalRig.enabled = false;
                VRRig.LocalRig.transform.position += direction.normalized * 2000f;
            }
        }

        private static GameObject point;
        public static void TowardsPointOnGrab()
        {
            if (point == null)
            {
                point = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                Object.Destroy(point.GetComponent<Collider>());
                point.transform.position = GorillaTagger.Instance.rightHandTransform.position;
                point.transform.localScale = Vector3.one * 0.2f;
            }

            point.GetComponent<Renderer>().material.color = buttonColors[1].GetCurrentColor();

            if (GetGunInput(false))
            {
                var GunData = RenderGun();
                GameObject NewPointer = GunData.NewPointer;

                if (GetGunInput(true))
                    point.transform.position = NewPointer.transform.position + Vector3.up * 0.5f;
            }

            TowardsPositionOnGrab(point.transform.position);
        }

        public static void DisableTowardsPointOnGrab()
        {
            if (point != null)
            {
                Object.Destroy(point);
                point = null;
            }
        }

        public static void ForceGrab()
        {
            VRRig.LocalRig.enabled = true;
            foreach (VRRig rig in VRRigCache.ActiveRigs)
            {
                if (rig.IsLocal()) continue;
                if ((rig.leftMiddle.calcT > 0.8f && rig.leftHandLink.grabbedPlayer == null) || (rig.rightMiddle.calcT > 0.8f && rig.rightHandLink.grabbedPlayer == null))
                {
                    bool isLeftHand = rig.leftMiddle.calcT > 0.8f;

                    VRRig.LocalRig.enabled = false;
                    VRRig.LocalRig.transform.position = rig.transform.position - Vector3.up * 0.5f;
                    VRRig.LocalRig.transform.rotation = Quaternion.identity;

                    VRMap targetHand = isLeftHand ? VRRig.LocalRig.leftHand : VRRig.LocalRig.rightHand;
                    targetHand.rigTarget.transform.position = isLeftHand ? rig.leftHandTransform.position : rig.rightHandTransform.position;
                    targetHand.rigTarget.transform.rotation = isLeftHand ? rig.leftHandTransform.rotation : rig.rightHandTransform.rotation;

                    VRRig.LocalRig.leftIndex.calcT = 1f;
                    VRRig.LocalRig.leftMiddle.calcT = 1f;
                    VRRig.LocalRig.leftThumb.calcT = 1f;

                    VRRig.LocalRig.leftIndex.LerpFinger(1f, false);
                    VRRig.LocalRig.leftMiddle.LerpFinger(1f, false);
                    VRRig.LocalRig.leftThumb.LerpFinger(1f, false);

                    VRRig.LocalRig.rightIndex.calcT = 1f;
                    VRRig.LocalRig.rightMiddle.calcT = 1f;
                    VRRig.LocalRig.rightThumb.calcT = 1f;

                    VRRig.LocalRig.rightIndex.LerpFinger(1f, false);
                    VRRig.LocalRig.rightMiddle.LerpFinger(1f, false);
                    VRRig.LocalRig.rightThumb.LerpFinger(1f, false);

                    TakeMyHand_HandLink link = isLeftHand ? VRRig.LocalRig.leftHandLink : VRRig.LocalRig.rightHandLink;
                    link.LocalCreateLink(isLeftHand ? rig.leftHandLink : rig.rightHandLink); // recheck kingofnetflix

                    break;
                }
            }
        }

        public static void TowardsPositionOnGrab(Vector3 position)
        {
            VRRig.LocalRig.enabled = true;
            foreach (VRRig rig in VRRigCache.ActiveRigs)
            {
                if (!rig.isLocal) /*&& rig.transform.position.x < 80)*/
                {
                    if (rig.leftHandLink.grabbedPlayer == NetworkSystem.Instance.LocalPlayer || rig.rightHandLink.grabbedPlayer == NetworkSystem.Instance.LocalPlayer)
                    {
                        VRRig.LocalRig.enabled = false;
                        VRRig.LocalRig.transform.position = position;
                    }
                }
            }
        }

        public static void FlingOnGrab()
        {
            VRRig.LocalRig.enabled = true;
            foreach (VRRig rig in VRRigCache.ActiveRigs)
            {
                if (!rig.isLocal) /*&& rig.transform.position.x < 80)*/
                {
                    if (rig.leftHandLink.grabbedPlayer == NetworkSystem.Instance.LocalPlayer || rig.rightHandLink.grabbedPlayer == NetworkSystem.Instance.LocalPlayer)
                    {
                        Vector3 velocity = (Vector3.up + GorillaTagger.Instance.bodyCollider.transform.forward * 1).normalized * 3f;
                        GetNetworkViewFromVRRig(VRRig.LocalRig).SendRPC("DroppedByPlayer", GetPlayerFromVRRig(rig), velocity);
                        rig.ApplyLocalTrajectoryOverride(velocity);
                        VRRig.LocalRig.leftHandLink.BreakLink();
                        VRRig.LocalRig.rightHandLink.BreakLink();
                    }
                }
            }
        }

        private static float propHuntSpazDelay;
        private static bool propHuntSpazMode;
        public static void SpazPropHuntObjects()
        {
            if (Time.time > propHuntSpazDelay)
            {
                propHuntSpazDelay = Time.time + 0.1f;
                propHuntSpazMode = !propHuntSpazMode;

                if (!NetworkSystem.Instance.IsMasterClient) { NotificationManager._v3_msg_("<color=grey>[</color><color=red>ERROR</color><color=grey>]</color> You are not master client."); return; }

                if (PhotonNetwork.InRoom && GorillaGameManager.instance.GameType() == GameModeType.PropHunt)
                {
                    GorillaPropHuntGameManager hauntManager = (GorillaPropHuntGameManager)GorillaGameManager.instance;
                    hauntManager._ph_timeRoundStartedMillis = propHuntSpazMode ? 1 : 2;
                    hauntManager._ph_randomSeed = Random.Range(1, int.MaxValue);
                }
            }
        }

        public static void SpazPropHunt()
        {
            if (Time.time > propHuntSpazDelay)
            {
                propHuntSpazDelay = Time.time + 0.1f;
                propHuntSpazMode = !propHuntSpazMode;

                if (!NetworkSystem.Instance.IsMasterClient) { NotificationManager._v3_msg_("<color=grey>[</color><color=red>ERROR</color><color=grey>]</color> You are not master client."); return; }

                if (PhotonNetwork.InRoom && GorillaGameManager.instance.GameType() == GameModeType.PropHunt)
                {
                    GorillaPropHuntGameManager hauntManager = (GorillaPropHuntGameManager)GorillaGameManager.instance;
                    hauntManager._ph_timeRoundStartedMillis = propHuntSpazMode ? 0 : 1;
                }
            }
        }

        public static float ghostReactorDelay;
        public static float throwDelay;
        public static void CreateItem(object target, int hash, Vector3 position, Quaternion rotation, Vector3 velocity, Vector3 angVelocity, long sendData = 0L, GameEntityManager manager = null)
        {
            GameEntityManager gameEntityManager = manager ?? ManagerRegistry.GhostReactor.GameEntityManager;
            if (NetworkSystem.Instance.IsMasterClient)
            {
                if (Time.time < ghostReactorDelay)
                    return;

                ghostReactorDelay = Time.time + gameEntityManager.m_RpcSpamChecks.m_callLimiters[(int)GameEntityManager.RPC.CreateItem].GetDelay();

                int netId = gameEntityManager.CreateTypeNetId(hash);

                if (target is NetPlayer netPlayer)
                    target = NetPlayerToPlayer(netPlayer);

                object[] createData = {
                    new[] { netId },
                    new[] { hash },
                    new[] { BitPackUtils.PackWorldPosForNetwork(position) },
                    new[] { BitPackUtils.PackQuaternionForNetwork(rotation) },
                    new[] { sendData },
                    new[] { gameEntityManager.GetInvalidNetId() }
                };

                switch (target)
                {
                    case RpcTarget rpcTarget:
                        gameEntityManager.photonView.RPC("CreateItemRPC", rpcTarget, createData);
                        break;
                    case Player player:
                        gameEntityManager.photonView.RPC("CreateItemRPC", player, createData);
                        break;
                }

                if ((velocity != Vector3.zero || angVelocity != Vector3.zero || Buttons.GetIndex("Entity Gravity").enabled) && Time.time > throwDelay)
                {
                    throwDelay = Time.time + gameEntityManager.m_RpcSpamChecks.m_callLimiters[(int)GameEntityManager.RPC.ThrowEntity].GetDelay();

                    velocity = velocity.ClampSqrMagnitude(1600f);

                    object[] dropData = {
                        netId,
                        true,
                        position,
                        rotation,
                        velocity,
                        angVelocity,
                        PhotonNetwork.LocalPlayer,
                        PhotonNetwork.Time
                    };

                    switch (target)
                    {
                        case RpcTarget rpcTarget:
                            gameEntityManager.photonView.RPC("ThrowEntityRPC", rpcTarget, dropData);
                            break;
                        case Player player:
                            gameEntityManager.photonView.RPC("ThrowEntityRPC", player, dropData);
                            break;
                    }
                }

                RPCProtection();
            }
            else
            {
                float maxDistance = 12f;
                if (Vector3.Distance(ServerLeftHandPos, position) > maxDistance)
                    position = ServerLeftHandPos + (position - ServerLeftHandPos).normalized * maxDistance;

                GamePlayer gamePlayer = GamePlayer.GetGamePlayer(PhotonNetwork.LocalPlayer);
                if (gamePlayer.IsHoldingEntity(gameEntityManager, true) && Time.time > ghostReactorDelay)
                {
                    VRRig.LocalRig.enabled = true;
                    if (ServerLeftHandPos.Distance(position) < maxDistance)
                        gameEntityManager.GetGameEntity(gamePlayer.GetGrabbedGameEntityId(GamePlayer.GetHandIndex(true))).RequestThrow(true, position, rotation, velocity, angVelocity, gameEntityManager);
                    else
                        return;
                }

                List<GameEntity> entities = gameEntityManager.entities.Where(e =>
                    e != null &&
                    e.typeId == hash &&
                    Vector3.Distance(ServerLeftHandPos, e.transform.position) < maxDistance &&
                    Vector3.Distance(GorillaTagger.Instance.bodyCollider.transform.position, e.transform.position) > 3f &&
                    gameEntityManager.ValidateGrab(e, PhotonNetwork.LocalPlayer.actorNumber, true)).ToList();

                if (entities.Count <= 0)
                    entities = gameEntityManager.entities.Where(e =>
                    e != null &&
                    e.typeId == hash &&
                    Vector3.Distance(ServerLeftHandPos, e.transform.position) < maxDistance &&
                    gameEntityManager.ValidateGrab(e, PhotonNetwork.LocalPlayer.actorNumber, true)).ToList();

                if (entities.Count <= 0)
                    entities = gameEntityManager.entities.Where(e =>
                    e != null &&
                    e.typeId == hash &&
                    gameEntityManager.ValidateGrab(e, PhotonNetwork.LocalPlayer.actorNumber, true)).ToList();

                if (entities.Count <= 0) // Desperate measures
                    entities = gameEntityManager.entities.Where(e =>
                    e != null &&
                    e.typeId == hash).ToList();

                if (entities.Count <= 0)
                    return;

                GameEntity entity = entities.OrderByDescending(entity => entity.transform.position.Distance(GorillaTagger.Instance.bodyCollider.transform.position)).FirstOrDefault();

                if (Vector3.Distance(entity.transform.position, GorillaTagger.Instance.bodyCollider.transform.position) > maxDistance)
                {
                    VRRig.LocalRig.enabled = false;
                    VRRig.LocalRig.transform.position = entity.transform.position - Vector3.one * 5f;

                    if (CritterCoroutine != null)
                        CoroutineManager.instance.StopCoroutine(CritterCoroutine);

                    CritterCoroutine = CoroutineManager.instance.StartCoroutine(RopeEnableRig());
                }

                if (Vector3.Distance(entity.transform.position, ServerPos) < maxDistance && Time.time > ghostReactorDelay)
                {
                    ghostReactorDelay = Time.time + 0.1f;

                    entity.transform.position = GorillaTagger.Instance.rightHandTransform.position;
                    entity.transform.rotation = RandomQuaternion();

                    entity.RequestGrab(true, Vector3.zero, Quaternion.identity, gameEntityManager);
                    RPCProtection();
                }
            }
        }

        public static void CreateItems(object target, int[] hashes, Vector3[] positions, Quaternion[] rotations, long[] sendData = null, GameEntityManager manager = null)
        {
            GameEntityManager gameEntityManager = manager ?? ManagerRegistry.GhostReactor.GameEntityManager;
            if (NetworkSystem.Instance.IsMasterClient)
            {
                if (Time.time < ghostReactorDelay)
                    return;

                ghostReactorDelay = Time.time + gameEntityManager.m_RpcSpamChecks.m_callLimiters[(int)GameEntityManager.RPC.CreateItems].GetDelay();

                if (target is NetPlayer netPlayer)
                    target = NetPlayerToPlayer(netPlayer);

                sendData ??= Enumerable.Repeat(0L, hashes.Length).ToArray();

                byte[] data = new byte[15360];
                MemoryStream memoryStream = new MemoryStream(data);
                BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
                binaryWriter.Write(hashes.Length);

                for (int i = 0; i < hashes.Length; i++)
                {
                    binaryWriter.Write(manager.CreateTypeNetId(hashes[i]));
                    binaryWriter.Write(hashes[i]);
                    binaryWriter.Write(BitPackUtils.PackWorldPosForNetwork(positions[i]));
                    binaryWriter.Write(BitPackUtils.PackQuaternionForNetwork(rotations[i]));
                    binaryWriter.Write(sendData[i]);
                    binaryWriter.Write(gameEntityManager.GetInvalidNetId());
                }

                byte[] array = GZipStream.CompressBuffer(data);

                object[] createData = {
                    (int)manager.zone,
                    array
                };

                switch (target)
                {
                    case RpcTarget rpcTarget:
                        gameEntityManager.photonView.RPC("CreateItemsRPC", rpcTarget, createData);
                        break;
                    case Player player:
                        gameEntityManager.photonView.RPC("CreateItemsRPC", player, createData);
                        break;
                }

                RPCProtection();
            }
            else
                CreateItem(target, hashes[0], positions[0], rotations[0], Vector3.zero, Vector3.zero, sendData.Length > 0 ? sendData[1] : 0L, manager);
        }

        public static Dictionary<string, int> ObjectByName { get => ManagerRegistry.GhostReactor.GameEntityManager.itemPrefabFactory.ToDictionary(prefab => prefab.Value.name, prefab => prefab.Key); }

        public static void SpamObjectGrip(int objectId)
        {
            if (rightGrab)
                CreateItem(RpcTarget.All, objectId, GorillaTagger.Instance.rightHandTransform.position, RandomQuaternion(), GorillaTagger.Instance.rightHandTransform.forward * ShootStrength, Vector3.zero);
        }

        public static void SpamEntityGrip()
        {
            int[] objectIds = ObjectByName.Select(x => x.Value).ToArray();
            SpamObjectGrip(objectIds[Random.Range(0, objectIds.Length)]);
        }

        public static void ToolSpamGrip()
        {
            int[] objectIds = ObjectByName.Where(x => x.Key.Contains("Tool")).Select(x => x.Value).ToArray();
            SpamObjectGrip(objectIds[Random.Range(0, objectIds.Length)]);
        }

        public static void ToolSpamGun()
        {
            int[] objectIds = ObjectByName.Where(x => x.Key.Contains("Tool")).Select(x => x.Value).ToArray();
            SpamObjectGun(objectIds[Random.Range(0, objectIds.Length)]);
        }

        public static void SpamObjectGun(int objectId)
        {
            if (GetGunInput(false))
            {
                var GunData = RenderGun();
                GameObject NewPointer = GunData.NewPointer;

                if (GetGunInput(true))
                    CreateItem(RpcTarget.All, objectId, NewPointer.transform.position, RandomQuaternion(), Vector3.zero, Vector3.zero);
            }
        }

        public static void SpamEntityGun()
        {
            int[] objectIds = ObjectByName.Select(x => x.Value).ToArray();
            SpamObjectGun(objectIds[Random.Range(0, objectIds.Length)]);
        }

        public static void RainEntities()
        {
            int[] objectIds = ObjectByName.Select(x => x.Value).ToArray();
            CreateItem(RpcTarget.All, objectIds[Random.Range(0, objectIds.Length)], VRRig.LocalRig.transform.position + new Vector3(Random.Range(-3f, 3f), 4f, Random.Range(-3f, 3f)), Quaternion.identity, Vector3.down, Vector3.zero);
        }

        public static void EntityAura()
        {
            int[] objectIds = ObjectByName.Select(x => x.Value).ToArray();
            CreateItem(RpcTarget.All, objectIds[Random.Range(0, objectIds.Length)], VRRig.LocalRig.transform.position + RandomVector3().normalized * 2f, Quaternion.identity, Vector3.down, Vector3.zero);
        }

        public static void EntityFountain()
        {
            int[] objectIds = ObjectByName.Select(x => x.Value).ToArray();
            CreateItem(RpcTarget.All, objectIds[Random.Range(0, objectIds.Length)], VRRig.LocalRig.transform.position + Vector3.up * 3f, Quaternion.identity, RandomVector3(15f), Vector3.zero);
        }

        public static Dictionary<string, bool[][]> Letters = new Dictionary<string, bool[][]> {
            { "A", new[]
            {
                new[] { false, true, true, true, false },
                new[] { true, false, false, false, true },
                new[] { true, true, true, true, true },
                new[] { true, false, false, false, true },
                new[] { true, false, false, false, true },
            } },
            { "B", new[]
            {
                new[] { true, true, true, true, false },
                new[] { true, false, false, false, true },
                new[] { true, true, true, true, false },
                new[] { true, false, false, false, true },
                new[] { true, true, true, true, true },
            } },
            { "C", new[]
            {
                new[] { false, true, true, true, true },
                new[] { true, false, false, false, false },
                new[] { true, false, false, false, false },
                new[] { true, false, false, false, false },
                new[] { false, true, true, true, true },
            } },
            { "D", new[]
            {
                new[] { true, true, true, true, false },
                new[] { true, false, false, false, true },
                new[] { true, false, false, false, true },
                new[] { true, false, false, false, true },
                new[] { true, true, true, true, false },
            } },
            { "E", new[]
            {
                new[] { true, true, true, true, true },
                new[] { true, false, false, false, false },
                new[] { true, true, true, false, false },
                new[] { true, false, false, false, false },
                new[] { true, true, true, true, true },
            } },
            { "F", new[]
            {
                new[] { true, true, true, true, true },
                new[] { true, false, false, false, false },
                new[] { true, true, true, false, false },
                new[] { true, false, false, false, false },
                new[] { true, false, false, false, false },
            } },
            { "G", new[]
            {
                new[] { true, true, true, true, true },
                new[] { true, false, false, false, false },
                new[] { true, false, false, true, true },
                new[] { true, false, false, false, true },
                new[] { true, true, true, true, true },
            } },
            { "H", new[]
            {
                new[] { true, false, false, false, true },
                new[] { true, false, false, false, true },
                new[] { true, true, true, true, true },
                new[] { true, false, false, false, true },
                new[] { true, false, false, false, true },
            } },
            { "I", new[]
            {
                new[] { true, true, true, true, true },
                new[] { false, false, true, false, false },
                new[] { false, false, true, false, false },
                new[] { false, false, true, false, false },
                new[] { true, true, true, true, true },
            } },
            { "J", new[]
            {
                new[] { false, false, false, false, true },
                new[] { false, false, false, false, true },
                new[] { false, false, false, false, true },
                new[] { true, false, false, false, true },
                new[] { false, true, true, true, false },
            } },
            { "K", new[]
            {
                new[] { true, false, false, false, true },
                new[] { true, false, false, true, false },
                new[] { true, true, true, false, false },
                new[] { true, false, false, true, false },
                new[] { true, false, false, false, true },
            } },
            { "L", new[]
            {
                new[] { true, false, false, false, false },
                new[] { true, false, false, false, false },
                new[] { true, false, false, false, false },
                new[] { true, false, false, false, false },
                new[] { true, true, true, true, true },
            } },
            { "M", new[]
            {
                new[] { true, true, false, true, true },
                new[] { true, false, true, false, true },
                new[] { true, false, false, false, true },
                new[] { true, false, false, false, true },
                new[] { true, false, false, false, true },
            } },
            { "N", new[]
            {
                new[] { true, false, false, false, true },
                new[] { true, true, false, false, true },
                new[] { true, false, true, false, true },
                new[] { true, false, false, true, true },
                new[] { true, false, false, false, true },
            } },
            { "O", new[]
            {
                new[] { false, true, true, true, false },
                new[] { true, false, false, false, true },
                new[] { true, false, false, false, true },
                new[] { true, false, false, false, true },
                new[] { false, true, true, true, false },
            } },
            { "P", new[]
            {
                new[] { true, true, true, true, false },
                new[] { true, false, false, false, true },
                new[] { true, true, true, true, false },
                new[] { true, false, false, false, false },
                new[] { true, false, false, false, false },
            } },
            { "Q", new[]
            {
                new[] { false, true, true, true, false },
                new[] { true, false, false, false, true },
                new[] { true, false, true, false, true },
                new[] { true, false, false, true, false },
                new[] { false, true, true, false, true },
            } },
            { "R", new[]
            {
                new[] { true, true, true, true, true },
                new[] { true, false, false, false, true },
                new[] { true, true, true, true, true },
                new[] { true, false, false, true, false },
                new[] { true, false, false, false, true },
            } },
            { "S", new[]
            {
                new[] { true, true, true, true, true },
                new[] { true, false, false, false, false },
                new[] { true, true, true, true, true },
                new[] { false, false, false, false, true },
                new[] { true, true, true, true, true },
            } },
            { "T", new[]
            {
                new[] { true, true, true, true, true },
                new[] { false, false, true, false, false },
                new[] { false, false, true, false, false },
                new[] { false, false, true, false, false },
                new[] { false, false, true, false, false },
            } },
            { "U", new[]
            {
                new[] { true, false, false, false, true },
                new[] { true, false, false, false, true },
                new[] { true, false, false, false, true },
                new[] { true, false, false, false, true },
                new[] { false, true, true, true, false },
            } },
            { "V", new[]
            {
                new[] { true, false, false, false, true },
                new[] { true, false, false, false, true },
                new[] { false, true, false, true, false },
                new[] { false, true, false, true, false },
                new[] { false, false, true, false, false },
            } },
            { "W", new[]
            {
                new[] { true, false, false, false, true },
                new[] { true, false, false, false, true },
                new[] { true, false, false, false, true },
                new[] { true, false, true, false, true },
                new[] { false, true, false, true, false },
            } },
            { "X", new[]
            {
                new[] { true, false, false, false, true },
                new[] { false, true, false, true, false },
                new[] { false, false, true, false, false },
                new[] { false, true, false, true, false },
                new[] { true, false, false, false, true },
            } },
            { "Y", new[]
            {
                new[] { true, false, false, false, true },
                new[] { false, true, false, true, false },
                new[] { false, false, true, false, false },
                new[] { false, false, true, false, false },
                new[] { false, false, true, false, false },
            } },
            { "Z", new[]
            {
                new[] { true, true, true, true, true },
                new[] { false, false, false, true, false },
                new[] { false, false, true, false, false },
                new[] { false, true, false, false, false },
                new[] { true, true, true, true, true },
            } },
            { ".", new[]
            {
                new[] { false, false, false, false, false },
                new[] { false, false, false, false, false },
                new[] { false, false, false, false, false },
                new[] { false, false, false, false, false },
                new[] { false, false, true, false, false },
            } },
            { "/", new[]
            {
                new[] { false, false, false, false, true },
                new[] { false, false, false, true, false },
                new[] { false, false, true, false, false },
                new[] { false, true, false, false, false },
                new[] { true, false, false, false, false },
            } },
            { " ", new[]
            {
                new[] { false, false, false, false, false },
                new[] { false, false, false, false, false },
                new[] { false, false, false, false, false },
                new[] { false, false, false, false, false },
                new[] { false, false, false, false, false },
            } }
        };

        public static string textToRender;
        public static float textDelay;
        public static int characterIndex;
        public static Vector3? basePosition;

        public static void GhostReactorTextGun()
        {
            if (GetGunInput(false))
            {
                var GunData = RenderGun();
                GameObject NewPointer = GunData.NewPointer;

                if (GetGunInput(true))
                {
                    if (basePosition == null)
                        basePosition = NewPointer.transform.position + Vector3.up;

                    if (Time.time > textDelay)
                    {
                        textDelay = Time.time + 0.1f;
                        bool[][] characterData = Letters[textToRender[characterIndex].ToString()];

                        List<Vector3> position = new List<Vector3>();
                        for (int i = 0; i < characterData.Length; i++)
                        {
                            bool[] column = characterData[i];

                            for (int j = 0; j < column.Length; j++)
                            {
                                bool currentIndex = column[j];
                                Vector3 offset = new Vector3((j * 0.2f) + (characterIndex * 1.2f), i * -0.2f, 0f);

                                if (currentIndex)
                                    position.Add(basePosition.Value + offset);
                            }
                        }

                        CreateItems(RpcTarget.All, Enumerable.Repeat(ObjectByName["GhostReactorCollectibleFlower"], position.Count).ToArray(), position.ToArray(), Enumerable.Repeat(Quaternion.identity, position.Count).ToArray());
                        characterIndex++;
                    }
                }
                else
                {
                    characterIndex = 0;
                    basePosition = null;
                }
            }
        }

        public static void SuperInfectionTextGun()
        {
            if (GetGunInput(false))
            {
                var GunData = RenderGun();
                GameObject NewPointer = GunData.NewPointer;

                if (GetGunInput(true))
                {
                    if (basePosition == null)
                        basePosition = NewPointer.transform.position + Vector3.up;

                    if (Time.time > textDelay)
                    {
                        textDelay = Time.time + 0.1f;
                        bool[][] characterData = Letters[textToRender[characterIndex].ToString()];

                        List<Vector3> position = new List<Vector3>();
                        for (int i = 0; i < characterData.Length; i++)
                        {
                            bool[] column = characterData[i];

                            for (int j = 0; j < column.Length; j++)
                            {
                                bool currentIndex = column[j];
                                Vector3 offset = new Vector3((j * 0.2f) + (characterIndex * 1.2f), i * -0.2f, 0f);

                                if (currentIndex)
                                    position.Add(basePosition.Value + offset);
                            }
                        }

                        CreateItems(RpcTarget.All, Enumerable.Repeat(GadgetByName["SIGadgetDashYoyo"], position.Count).ToArray(), position.ToArray(), Enumerable.Repeat(Quaternion.identity, position.Count).ToArray(), null, ManagerRegistry.SuperInfection.GameEntityManager);
                        characterIndex++;
                    }
                }
                else
                {
                    characterIndex = 0;
                    basePosition = null;
                }
            }
        }

        public static IEnumerator DrawSmallDelay(Vector3 position, int id, GameEntityManager manager)
        {
            GameObject Temporary = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            Temporary.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
            Temporary.transform.position = position;
            Object.Destroy(Temporary.GetComponent<Collider>());
            yield return new WaitForSeconds(0.5f);
            CreateItem(RpcTarget.All, id, Temporary.transform.position + new Vector3(0f, 0.1f, 0f), RandomQuaternion(), Vector3.zero, Vector3.zero, manager: manager);
            Object.Destroy(Temporary);
            RPCProtection();
        }

        public static void GhostReactorDrawGun()
        {
            if (GetGunInput(false))
            {
                var GunData = RenderGun();
                GameObject NewPointer = GunData.NewPointer;

                if (GetGunInput(true))
                    CoroutineManager.instance.StartCoroutine(DrawSmallDelay(NewPointer.transform.position, ObjectByName["GhostReactorCollectibleCore"], ManagerRegistry.GhostReactor.GameEntityManager));
            }
        }

        public static void SuperInfectionDrawGun()
        {
            if (GetGunInput(false))
            {
                var GunData = RenderGun();
                GameObject NewPointer = GunData.NewPointer;

                if (GetGunInput(true))
                {
                    int[] objectIds = GadgetByName.Select(element => element.Value).ToArray();
                    CoroutineManager.instance.StartCoroutine(DrawSmallDelay(NewPointer.transform.position, objectIds[Random.Range(0, objectIds.Length)], ManagerRegistry.SuperInfection.GameEntityManager));
                }
            }
        }

        private static float destroyDelay;
        public static void DestroyEntityGun()
        {
            if (GetGunInput(false))
            {
                var GunData = RenderGun();
                GameObject NewPointer = GunData.NewPointer;

                if (GetGunInput(true))
                {
                    if (Time.time > destroyDelay)
                    {
                        GameEntity gameEntity = null;
                        float closestDist = float.MaxValue;

                        foreach (GameEntity entity in ManagerRegistry.GhostReactor.GameEntityManager.entities)
                        {
                            if (entity != null)
                            {
                                float distance = Vector3.Distance(NewPointer.transform.position, entity.transform.position);
                                if (distance < 0.75f && distance < closestDist)
                                {
                                    gameEntity = entity;
                                    closestDist = distance;
                                }
                            }
                        }

                        if (gameEntity != null)
                        {
                            destroyDelay = Time.time + 0.02f;
                            if (NetworkSystem.Instance.IsMasterClient)
                            {
                                ManagerRegistry.GhostReactor.GameEntityManager.photonView.RPC("DestroyItemRPC", RpcTarget.All, new[] { gameEntity.GetNetId() });
                                RPCProtection();
                            }
                            else
                            {
                                gameEntity.RequestGrab(true, Vector3.zero, Quaternion.identity);
                                gameEntity.RequestThrow(true, GorillaTagger.Instance.bodyCollider.transform.position - (Vector3.up * 14f), Quaternion.identity, Vector3.zero, Vector3.zero);
                            }
                        }
                    }
                }
            }
        }

        private static float resourceIncrementDelay;
        public static void InfiniteResources()
        {
            var player = SIPlayer.Get(NetworkSystem.Instance.LocalPlayer.ActorNumber);
            for (int i = 0; i < player.CurrentProgression.resourceArray.Length; i++)
                player.CurrentProgression.resourceArray[i] = int.MaxValue;

            if (Time.time > resourceIncrementDelay)
            {
                resourceIncrementDelay = Time.time + 1f;
                for (int i = 0; i < (int)SIResource.ResourceType.Count; i++)
                {
                    var resourceType = (SIResource.ResourceType)i;
                    ProgressionManager.Instance.IncrementSIResource(resourceType.ToString(), OnFailure: (error) => resourceIncrementDelay = Time.time + 10f);
                }
            }
        }

        public static void CompleteAllQuests()
        {
            for (int i = 0; i < SIProgression.Instance.activeQuestIds.Length; i++)
            {
                RotatingQuest quest = SIProgression.Instance.questSourceList.GetQuestById(SIProgression.Instance.activeQuestIds[i]);
                quest.SetProgress(quest.requiredOccurenceCount);
            }
        }

        public static void ClaimAllTerminals()
        {
            foreach (var terminal in ManagerRegistry.SuperInfection.ZoneSuperInfection.siTerminals)
                terminal?.PlayerHandScanned(NetworkSystem.Instance.LocalPlayer.ActorNumber);
        }

        public static void UnlockAllGadgets()
        {
            foreach (var page in SIProgression.Instance.unlockedTechTreeData)
            {
                for (int i = 0; i < page.Length; i++)
                    page[i] = true;
            }
        }

        public static void DebugBlasterAimbot()
        {
            List<NetPlayer> infected = InfectedList();
            List<VRRig> rigs = VRRigCache.ActiveRigs
                .Where(rig => !rig.isLocal)
                .Where(rig => !infected.Contains(GetPlayerFromVRRig(rig)))
                .ToList();

            Transform head = GorillaTagger.Instance.headCollider.transform;
            VRRig targetRig = rigs
                .Where(rig => rig != null)
                .Select(rig => new
                {
                    Rig = rig,
                    ToRig = (rig.transform.position - head.position).normalized,
                    Distance = Vector3.Distance(head.position, rig.transform.position)
                })
                .OrderBy(x => Vector3.Angle(head.forward, x.ToRig)) // only angle matters
                .ThenBy(x => x.Distance) // tiebreaker if multiple at same angle
                .Select(x => x.Rig)
                .FirstOrDefault();

            if (targetRig == null)
                return;

            Visuals.VisualizeAura(targetRig.headMesh.transform.position, 0.1f, Color.green, -91752);
        }

        public static int? spawnedNetId;
        public static int spawnedFrame;

        public static SIGadgetChargeBlaster GetBlaster()
        {
            ControllerInputPoller.instance.leftGrab = true;
            ControllerInputPoller.instance.leftControllerGripFloat = 1f;

            int hash = GadgetByName["MegaChargeBlasterGadget"];

            GameEntityManager gameEntityManager = ManagerRegistry.SuperInfection.GameEntityManager;
            GamePlayer gamePlayer = GamePlayer.GetGamePlayer(PhotonNetwork.LocalPlayer);
            if (gamePlayer.IsHoldingEntity(gameEntityManager, true))
            {
                GameEntity entity = gameEntityManager.GetGameEntity(gamePlayer.GetGrabbedGameEntityId(GamePlayer.GetHandIndex(true)));
                if (entity.gameObject.TryGetComponent<SIGadgetChargeBlaster>(out var blaster))
                    return blaster;
                else
                    entity.RequestThrow(true, entity.transform.position, entity.transform.rotation, Vector3.zero, Vector3.zero, gameEntityManager);
            }
            else
            {
                if (NetworkSystem.Instance.IsMasterClient)
                {
                    if (spawnedNetId != null && spawnedFrame > Time.frameCount - 10)
                    {
                        GameEntity entity = gameEntityManager.GetGameEntity(spawnedNetId.Value);
                        entity.RequestGrab(true, Vector3.zero, Quaternion.identity, gameEntityManager);
                        if (entity.gameObject.TryGetComponent<SIGadgetChargeBlaster>(out var blaster))
                            return blaster;
                    }

                    if (Time.time < ghostReactorDelay)
                        return null;

                    ghostReactorDelay = Time.time + gameEntityManager.m_RpcSpamChecks.m_callLimiters[(int)GameEntityManager.RPC.CreateItem].GetDelay();

                    int netId = gameEntityManager.CreateTypeNetId(hash);

                    object[] createData = {
                        new[] { netId },
                        new[] { hash },
                        new[] { BitPackUtils.PackWorldPosForNetwork(GorillaTagger.Instance.leftHandTransform.position) },
                        new[] { BitPackUtils.PackQuaternionForNetwork(GorillaTagger.Instance.leftHandTransform.rotation) },
                        new[] { 0L },
                        new[] { gameEntityManager.GetInvalidNetId() }
                    };

                    gameEntityManager.photonView.RPC("CreateItemRPC", RpcTarget.All, createData);
                    spawnedNetId = netId;
                    spawnedFrame = Time.frameCount;

                    RPCProtection();
                }
                else
                {
                    Vector3 position = GorillaTagger.Instance.leftHandTransform.position;

                    float maxDistance = 12f;
                    if (Vector3.Distance(ServerLeftHandPos, position) > maxDistance)
                        position = ServerLeftHandPos + (position - ServerLeftHandPos).normalized * maxDistance;

                    List<GameEntity> entities = gameEntityManager.entities.Where(e =>
                        e != null &&
                        e.typeId == hash &&
                        Vector3.Distance(ServerLeftHandPos, e.transform.position) < maxDistance &&
                        Vector3.Distance(GorillaTagger.Instance.bodyCollider.transform.position, e.transform.position) > 3f &&
                        gameEntityManager.ValidateGrab(e, PhotonNetwork.LocalPlayer.actorNumber, true)).ToList();

                    if (entities.Count <= 0)
                        entities = gameEntityManager.entities.Where(e =>
                        e != null &&
                        e.typeId == hash &&
                        Vector3.Distance(ServerLeftHandPos, e.transform.position) < maxDistance &&
                        gameEntityManager.ValidateGrab(e, PhotonNetwork.LocalPlayer.actorNumber, true)).ToList();

                    if (entities.Count <= 0)
                        entities = gameEntityManager.entities.Where(e =>
                        e != null &&
                        e.typeId == hash &&
                        gameEntityManager.ValidateGrab(e, PhotonNetwork.LocalPlayer.actorNumber, true)).ToList();

                    if (entities.Count <= 0) // Desperate measures
                        entities = gameEntityManager.entities.Where(e =>
                        e != null &&
                        e.typeId == hash).ToList();

                    if (entities.Count <= 0)
                        return null;

                    GameEntity entity = entities.OrderByDescending(entity => entity.transform.position.Distance(GorillaTagger.Instance.bodyCollider.transform.position)).FirstOrDefault();

                    if (Vector3.Distance(entity.transform.position, GorillaTagger.Instance.bodyCollider.transform.position) > maxDistance)
                    {
                        VRRig.LocalRig.enabled = false;
                        VRRig.LocalRig.transform.position = entity.transform.position - Vector3.one * 5f;

                        if (CritterCoroutine != null)
                            CoroutineManager.instance.StopCoroutine(CritterCoroutine);

                        CritterCoroutine = CoroutineManager.instance.StartCoroutine(RopeEnableRig());
                    }

                    if (Vector3.Distance(entity.transform.position, ServerPos) < maxDistance && Time.time > ghostReactorDelay)
                    {
                        ghostReactorDelay = Time.time + 0.1f;

                        entity.transform.position = GorillaTagger.Instance.rightHandTransform.position;
                        entity.transform.rotation = RandomQuaternion();

                        entity.RequestGrab(true, Vector3.zero, Quaternion.identity, gameEntityManager);
                        RPCProtection();
                    }
                }
            }

            return null;
        }

        public static float blasterDelay;
        public static Coroutine BlasterCoroutine;

        public static void BetaFireBlaster(Vector3 position, Vector3 direction)
        {
            SIGadgetChargeBlaster blaster = GetBlaster();
            if (blaster == null)
                return;

            Quaternion rotation = Quaternion.LookRotation(direction);
            if (Time.time < blasterDelay)
                return;

            blasterDelay = Time.time + SIPlayer.LocalPlayer.clientToClientRPCLimiter.GetDelay();

            if ((position - GorillaTagger.Instance.leftHandTransform.position).magnitude > blaster.blaster.maxLagDistance)
            {
                VRRig.LocalRig.enabled = false;
                VRRig.LocalRig.transform.position = position - Vector3.one;

                if (BlasterCoroutine != null)
                    CoroutineManager.instance.StopCoroutine(BlasterCoroutine);

                BlasterCoroutine = CoroutineManager.instance.StartCoroutine(RopeEnableRig());
            }

            blaster.blaster.lastFired = 0f;
            blaster.FireProjectile(blaster.maxChargeDiff, blaster.blaster.NextFireId(), position, rotation);

            RPCProtection();
        }

        public static readonly Dictionary<NetPlayer, float> perPlayerDictionary = new Dictionary<NetPlayer, float>();
        public static void BetaFireBlaster(Vector3 position, Vector3 direction, NetPlayer target, bool ignoreDistnace = false)
        {
            perPlayerDictionary.TryGetValue(target, out float perPlayerDelay);

            Quaternion rotation = Quaternion.LookRotation(direction);
            if (Time.time < perPlayerDelay)
                return;

            SIGadgetChargeBlaster blaster = GetBlaster();
            if (blaster == null)
                return;

            perPlayerDictionary[target] = Time.time + SIPlayer.LocalPlayer.clientToClientRPCLimiter.GetDelay();

            if (!ignoreDistnace && (position - GorillaTagger.Instance.leftHandTransform.position).magnitude > blaster.blaster.maxLagDistance)
            {
                VRRig.LocalRig.enabled = false;
                VRRig.LocalRig.transform.position = position - Vector3.one;

                if (BlasterCoroutine != null)
                    CoroutineManager.instance.StopCoroutine(BlasterCoroutine);

                BlasterCoroutine = CoroutineManager.instance.StartCoroutine(RopeEnableRig());
            }

            blaster.blaster.lastFired = 0f;
            SuperInfectionManager.GetSIManagerForZone(blaster.blaster.gameEntity.manager.zone)?.photonView.RPC("SIClientToClientRPC", target.GetPlayer(), new object[]
            {
                (int)SuperInfectionManager.ClientToClientRPC.CallEntityRPCData,
                new object[]
                {
                    blaster.blaster.gameEntity.GetNetId(),
                    0,
                    new object[]
                    {
                        blaster.maxChargeDiff,
                        blaster.blaster.NextFireId(),
                        position,
                        rotation
                    }
                }
            });

            RPCProtection();
        }

        public static void BlasterLaserSpam()
        {
            if (rightGrab)
                BetaFireBlaster(GorillaTagger.Instance.rightHandTransform.position, GorillaTagger.Instance.rightHandTransform.forward);
        }

        public static void BlasterFlingGun(Vector3 direction)
        {
            if (GetGunInput(false))
            {
                var GunData = RenderGun();
                RaycastHit Ray = GunData.Ray;

                if (gunLocked && lockTarget != null)
                    BetaFireBlaster(lockTarget.transform.position, direction.normalized);

                if (GetGunInput(true))
                {
                    VRRig gunTarget = Ray.collider.GetComponentInParent<VRRig>();
                    if (gunTarget && !gunTarget.IsLocal())
                    {
                        gunLocked = true;
                        lockTarget = gunTarget;
                    }
                }
            }
            else
            {
                if (gunLocked)
                    gunLocked = false;
            }
        }

        public static void BlasterFlingAll(Vector3 direction)
        {
            if (GetBlaster() == null)
                SerializePatch.OverrideSerialization = null;
            else SerializePatch.OverrideSerialization ??= () =>
            {
                MassSerialize(true, new[] { GorillaTagger.Instance.myVRRig.GetView });

                Vector3 archivePos = VRRig.LocalRig.transform.position;

                foreach (NetPlayer Player in NetworkSystem.Instance.PlayerListOthers)
                {
                    VRRig targetRig = GetVRRigFromPlayer(Player);

                    VRRig.LocalRig.transform.position = targetRig.transform.position - Vector3.up;
                    SendSerialize(GorillaTagger.Instance.myVRRig.GetView, new RaiseEventOptions { TargetActors = new[] { Player.ActorNumber } });
                }

                RPCProtection();

                VRRig.LocalRig.transform.position = archivePos;

                return false;
            };

            foreach (NetPlayer Player in NetworkSystem.Instance.PlayerListOthers)
            {
                VRRig targetRig = GetVRRigFromPlayer(Player);
                BetaFireBlaster(targetRig.transform.position, direction, Player, true);
            }
        }

        public static void BlasterFlingTowardsGun()
        {
            if (GetGunInput(false))
            {
                var GunData = RenderGun();
                RaycastHit Ray = GunData.Ray;

                if (gunLocked && lockTarget != null)
                    BetaFireBlaster(lockTarget.transform.position, GorillaTagger.Instance.bodyCollider.transform.position - lockTarget.transform.position);

                if (GetGunInput(true))
                {
                    VRRig gunTarget = Ray.collider.GetComponentInParent<VRRig>();
                    if (gunTarget && !gunTarget.IsLocal())
                    {
                        gunLocked = true;
                        lockTarget = gunTarget;
                    }
                }
            }
            else
            {
                if (gunLocked)
                    gunLocked = false;
            }
        }

        public static void BlasterFlingTowardsAll()
        {
            if (GetBlaster() == null)
                SerializePatch.OverrideSerialization = null;
            else SerializePatch.OverrideSerialization ??= () =>
            {
                MassSerialize(true, new[] { GorillaTagger.Instance.myVRRig.GetView });

                Vector3 archivePos = VRRig.LocalRig.transform.position;

                foreach (NetPlayer Player in NetworkSystem.Instance.PlayerListOthers)
                {
                    VRRig targetRig = GetVRRigFromPlayer(Player);

                    VRRig.LocalRig.transform.position = targetRig.transform.position - Vector3.up;
                    SendSerialize(GorillaTagger.Instance.myVRRig.GetView, new RaiseEventOptions { TargetActors = new[] { Player.ActorNumber } });
                }

                RPCProtection();

                VRRig.LocalRig.transform.position = archivePos;

                return false;
            };

            foreach (NetPlayer Player in NetworkSystem.Instance.PlayerListOthers)
            {
                VRRig targetRig = GetVRRigFromPlayer(Player);
                BetaFireBlaster(targetRig.transform.position, GorillaTagger.Instance.bodyCollider.transform.position - targetRig.transform.position, Player, true);
            }
        }

        public static void BlasterFlingAwayGun()
        {
            if (GetGunInput(false))
            {
                var GunData = RenderGun();
                RaycastHit Ray = GunData.Ray;

                if (gunLocked && lockTarget != null)
                    BetaFireBlaster(lockTarget.transform.position, lockTarget.transform.position - GorillaTagger.Instance.bodyCollider.transform.position);

                if (GetGunInput(true))
                {
                    VRRig gunTarget = Ray.collider.GetComponentInParent<VRRig>();
                    if (gunTarget && !gunTarget.IsLocal())
                    {
                        gunLocked = true;
                        lockTarget = gunTarget;
                    }
                }
            }
            else
            {
                if (gunLocked)
                    gunLocked = false;
            }
        }

        public static void BlasterFlingAwayAll()
        {
            if (GetBlaster() == null)
                SerializePatch.OverrideSerialization = null;
            else SerializePatch.OverrideSerialization ??= () =>
            {
                MassSerialize(true, new[] { GorillaTagger.Instance.myVRRig.GetView });

                Vector3 archivePos = VRRig.LocalRig.transform.position;

                foreach (NetPlayer Player in NetworkSystem.Instance.PlayerListOthers)
                {
                    VRRig targetRig = GetVRRigFromPlayer(Player);

                    VRRig.LocalRig.transform.position = targetRig.transform.position - Vector3.up;
                    SendSerialize(GorillaTagger.Instance.myVRRig.GetView, new RaiseEventOptions { TargetActors = new[] { Player.ActorNumber } });
                }

                RPCProtection();

                VRRig.LocalRig.transform.position = archivePos;

                return false;
            };

            foreach (NetPlayer Player in NetworkSystem.Instance.PlayerListOthers)
            {
                VRRig targetRig = GetVRRigFromPlayer(Player);
                BetaFireBlaster(targetRig.transform.position, targetRig.transform.position - GorillaTagger.Instance.bodyCollider.transform.position, Player, true);
            }
        }

        public static void BlasterKickGun()
        {
            if (GetGunInput(false))
            {
                var GunData = RenderGun();
                RaycastHit Ray = GunData.Ray;

                if (gunLocked && lockTarget != null)
                    BetaFireBlaster(lockTarget.transform.position, lockTarget.transform.position.z < -28.5f ? (new Vector3(-47.82025f, 6.460508f, -29.04836f) - lockTarget.transform.position).normalized : lockTarget.transform.position.z < -23f ? new Vector3(-50f, 0f, 50f) : Vector3.left);

                if (GetGunInput(true))
                {
                    VRRig gunTarget = Ray.collider.GetComponentInParent<VRRig>();
                    if (gunTarget && !gunTarget.IsLocal())
                    {
                        gunLocked = true;
                        lockTarget = gunTarget;
                    }
                }
            }
            else
            {
                if (gunLocked)
                    gunLocked = false;
            }
        }

        public static void BlasterKickAll()
        {
            if (GetBlaster() == null)
                SerializePatch.OverrideSerialization = null;
            else SerializePatch.OverrideSerialization ??= () =>
            {
                MassSerialize(true, new[] { GorillaTagger.Instance.myVRRig.GetView });

                Vector3 archivePos = VRRig.LocalRig.transform.position;

                foreach (NetPlayer Player in NetworkSystem.Instance.PlayerListOthers)
                {
                    VRRig targetRig = GetVRRigFromPlayer(Player);

                    VRRig.LocalRig.transform.position = targetRig.transform.position - Vector3.up;
                    SendSerialize(GorillaTagger.Instance.myVRRig.GetView, new RaiseEventOptions { TargetActors = new[] { Player.ActorNumber } });
                }

                RPCProtection();

                VRRig.LocalRig.transform.position = archivePos;

                return false;
            };

            foreach (NetPlayer Player in NetworkSystem.Instance.PlayerListOthers)
            {
                VRRig targetRig = GetVRRigFromPlayer(Player);
                BetaFireBlaster(targetRig.transform.position, targetRig.transform.position.z < -28.5f ? (new Vector3(-47.82025f, 6.460508f, -29.04836f) - targetRig.transform.position).normalized : targetRig.transform.position.z < -23f ? new Vector3(-50f, 0f, 50f) : Vector3.left, Player, true);
            }
        }

        public static void BlasterCrashGun()
        {
            if (GetGunInput(false))
            {
                var GunData = RenderGun();
                RaycastHit Ray = GunData.Ray;

                if (gunLocked && lockTarget != null)
                    BetaFireBlaster(lockTarget.transform.position, lockTarget.transform.position.y > 55f ? Vector3.right : Vector3.up);

                if (GetGunInput(true))
                {
                    VRRig gunTarget = Ray.collider.GetComponentInParent<VRRig>();
                    if (gunTarget && !gunTarget.IsLocal())
                    {
                        gunLocked = true;
                        lockTarget = gunTarget;
                    }
                }
            }
            else
            {
                if (gunLocked)
                    gunLocked = false;
            }
        }

        public static void BlasterCrashAll()
        {
            if (GetBlaster() == null)
                SerializePatch.OverrideSerialization = null;
            else SerializePatch.OverrideSerialization ??= () =>
            {
                MassSerialize(true, new[] { GorillaTagger.Instance.myVRRig.GetView });

                Vector3 archivePos = VRRig.LocalRig.transform.position;

                foreach (NetPlayer Player in NetworkSystem.Instance.PlayerListOthers)
                {
                    VRRig targetRig = GetVRRigFromPlayer(Player);

                    VRRig.LocalRig.transform.position = targetRig.transform.position - Vector3.up;
                    SendSerialize(GorillaTagger.Instance.myVRRig.GetView, new RaiseEventOptions { TargetActors = new[] { Player.ActorNumber } });
                }

                RPCProtection();

                VRRig.LocalRig.transform.position = archivePos;

                return false;
            };

            foreach (NetPlayer Player in NetworkSystem.Instance.PlayerListOthers)
            {
                VRRig targetRig = GetVRRigFromPlayer(Player);
                BetaFireBlaster(targetRig.transform.position, targetRig.transform.position.y > 55f ? Vector3.right : Vector3.up, Player, true);
            }
        }

        public static void BlasterControlGun()
        {
            if (GetGunInput(false))
            {
                var GunData = RenderGun();
                RaycastHit Ray = GunData.Ray;

                if (gunLocked && lockTarget != null && lockTarget.Distance(GorillaTagger.Instance.bodyCollider.transform.position) > 0.5f)
                    BetaFireBlaster(lockTarget.transform.position, GorillaTagger.Instance.bodyCollider.transform.position - lockTarget.transform.position);

                if (GetGunInput(true))
                {
                    VRRig gunTarget = Ray.collider.GetComponentInParent<VRRig>();
                    if (gunTarget && !gunTarget.IsLocal())
                    {
                        gunLocked = true;
                        lockTarget = gunTarget;
                    }
                }
            }
            else
            {
                if (gunLocked)
                    gunLocked = false;
            }
        }

        public static Dictionary<string, int> GadgetByName
        {
            get =>
                ManagerRegistry.SuperInfection.GameEntityManager.itemPrefabFactory
                .ToDictionary(prefab => prefab.Value.name, prefab => prefab.Key);
        }

        public static void SpamGadgetGrip(int objectId)
        {
            if (rightGrab)
                CreateItem(RpcTarget.All, objectId, GorillaTagger.Instance.rightHandTransform.position, RandomQuaternion(), GorillaTagger.Instance.rightHandTransform.forward * ShootStrength, Vector3.zero, 0L, ManagerRegistry.SuperInfection.GameEntityManager);
        }

        public static void SpamGadgetGun(int objectId)
        {
            if (GetGunInput(false))
            {
                var GunData = RenderGun();
                GameObject NewPointer = GunData.NewPointer;

                if (GetGunInput(true))
                    CreateItem(RpcTarget.All, objectId, NewPointer.transform.position, RandomQuaternion(), Vector3.zero, Vector3.zero, 0L, ManagerRegistry.SuperInfection.GameEntityManager);
            }
        }

        public static void GadgetSpamGrip()
        {
            int[] objectIds = GadgetByName.Select(element => element.Value).ToArray();
            SpamGadgetGrip(objectIds[Random.Range(0, objectIds.Length)]);
        }

        public static void GadgetSpamGun()
        {
            int[] objectIds = GadgetByName.Select(element => element.Value).ToArray();
            SpamGadgetGun(objectIds[Random.Range(0, objectIds.Length)]);
        }

        public static void RainGadgets()
        {
            int[] objectIds = GadgetByName.Select(element => element.Value).ToArray();
            CreateItem(RpcTarget.All, objectIds[Random.Range(0, objectIds.Length)], VRRig.LocalRig.transform.position + new Vector3(Random.Range(-3f, 3f), 4f, Random.Range(-3f, 3f)), Quaternion.identity, Vector3.down, Vector3.zero, 0L, ManagerRegistry.SuperInfection.GameEntityManager);
        }

        public static void GadgetAura()
        {
            int[] objectIds = GadgetByName.Select(element => element.Value).ToArray();
            CreateItem(RpcTarget.All, objectIds[Random.Range(0, objectIds.Length)], VRRig.LocalRig.transform.position + RandomVector3().normalized * 2f, Quaternion.identity, Vector3.down, Vector3.zero, 0L, ManagerRegistry.SuperInfection.GameEntityManager);
        }

        public static void GadgetFountain()
        {
            int[] objectIds = GadgetByName.Select(element => element.Value).ToArray();
            CreateItem(RpcTarget.All, objectIds[Random.Range(0, objectIds.Length)], VRRig.LocalRig.transform.position + Vector3.up * 3f, Quaternion.identity, RandomVector3(15f), Vector3.zero, 0L, ManagerRegistry.SuperInfection.GameEntityManager);
        }

        public static void ResourceSpamGrip()
        {
            int[] objectIds = GadgetByName.Where(x => x.Key.Contains("Resource")).Select(x => x.Value).ToArray();
            SpamGadgetGrip(objectIds[Random.Range(0, objectIds.Length)]);
        }

        public static void ResourceSpamGun()
        {
            int[] objectIds = GadgetByName.Where(x => x.Key.Contains("Resource")).Select(x => x.Value).ToArray();
            SpamGadgetGun(objectIds[Random.Range(0, objectIds.Length)]);
        }

        public static void DestroyGadgetGun()
        {
            if (GetGunInput(false))
            {
                var GunData = RenderGun();
                GameObject NewPointer = GunData.NewPointer;

                if (GetGunInput(true))
                {
                    if (Time.time > destroyDelay)
                    {
                        GameEntity gameEntity = null;
                        float closestDist = float.MaxValue;

                        foreach (GameEntity entity in ManagerRegistry.SuperInfection.GameEntityManager.entities)
                        {
                            if (entity != null)
                            {
                                float distance = Vector3.Distance(NewPointer.transform.position, entity.transform.position);
                                if (distance < 0.75f && distance < closestDist)
                                {
                                    gameEntity = entity;
                                    closestDist = distance;
                                }
                            }
                        }

                        if (gameEntity != null)
                        {
                            destroyDelay = Time.time + 0.02f;
                            if (NetworkSystem.Instance.IsMasterClient)
                            {
                                ManagerRegistry.SuperInfection.GameEntityManager.photonView.RPC("DestroyItemRPC", RpcTarget.All, new[] { gameEntity.GetNetId() });
                                RPCProtection();
                            }
                            else
                            {
                                gameEntity.RequestGrab(true, Vector3.zero, Quaternion.identity, ManagerRegistry.SuperInfection.GameEntityManager);
                                gameEntity.RequestThrow(true, GorillaTagger.Instance.bodyCollider.transform.position - (Vector3.up * 14f), Quaternion.identity, Vector3.zero, Vector3.zero, ManagerRegistry.SuperInfection.GameEntityManager);
                            }
                        }
                    }
                }
            }
        }

        public static HalloweenGhostChaser _lucy;
        public static HalloweenGhostChaser Lucy
        {
            get
            {
                _lucy ??= GetObject("Environment Objects/05Maze_PersistentObjects/2025_Halloween1_PersistentObjects/Halloween Ghosts/Lucy/Halloween Ghost/FloatingChaseSkeleton").GetComponent<HalloweenGhostChaser>();
                return _lucy;
            }
            set => _lucy = value;
        }

        public static LurkerGhost _lurker;
        public static LurkerGhost Lurker
        {
            get
            {
                _lurker ??= GetObject("Environment Objects/05Maze_PersistentObjects/2025_Halloween1_PersistentObjects/Halloween Ghosts/Lurker Ghost/GhostLurker_Prefab").GetComponent<LurkerGhost>();
                return _lurker;
            }
            set => _lurker = value;
        }

        public static void SpawnBlueLucy()
        {
            HalloweenGhostChaser hgc = Lucy;
            if (hgc.IsMine)
            {
                hgc.timeGongStarted = Time.time;
                hgc.currentState = HalloweenGhostChaser.ChaseState.Gong;
                hgc.isSummoned = false;
            }
            else NotificationManager._v3_msg_("<color=grey>[</color><color=red>ERROR</color><color=grey>]</color> You are not master client.");
        }

        public static void SpawnRedLucy()
        {
            HalloweenGhostChaser hgc = Lucy;
            if (hgc.IsMine)
            {
                hgc.timeGongStarted = Time.time;
                hgc.currentState = HalloweenGhostChaser.ChaseState.Gong;
                hgc.isSummoned = true;
            }
            else NotificationManager._v3_msg_("<color=grey>[</color><color=red>ERROR</color><color=grey>]</color> You are not master client.");
        }

        public static void DespawnLucy()
        {
            HalloweenGhostChaser hgc = Lucy;
            if (hgc.IsMine)
            {
                hgc.currentState = HalloweenGhostChaser.ChaseState.Dormant;
                hgc.isSummoned = false;
            }
            else NotificationManager._v3_msg_("<color=grey>[</color><color=red>ERROR</color><color=grey>]</color> You are not master client.");
        }

        public static void LucyChase(NetPlayer player)
        {
            HalloweenGhostChaser hgc = Lucy;
            if (hgc.IsMine)
            {
                hgc.currentState = HalloweenGhostChaser.ChaseState.Chasing;
                hgc.targetPlayer = player;
                hgc.followTarget = GorillaTagger.Instance.offlineVRRig.transform;
            }
            else
                NotificationManager._v3_msg_("<color=grey>[</color><color=red>ERROR</color><color=grey>]</color> You are not master client.");
        }

        public static void LucyChaseGun()
        {
            if (GetGunInput(false))
            {
                var GunData = RenderGun();
                RaycastHit Ray = GunData.Ray;

                if (GetGunInput(true))
                {
                    VRRig gunTarget = Ray.collider.GetComponentInParent<VRRig>();
                    if (gunTarget && !gunTarget.IsLocal())
                        LucyChase(gunTarget.GetPlayer());
                }
            }
        }

        public static void LucyAttack(NetPlayer player)
        {
            HalloweenGhostChaser hgc = Lucy;
            if (hgc.IsMine)
            {
                if (Time.time > hgc.grabTime + hgc.grabDuration + 0.1f)
                {
                    if (hgc.targetPlayer != player)
                    {
                        hgc.currentState = HalloweenGhostChaser.ChaseState.Dormant;
                        SendSerialize(hgc.GetView);
                    }
                    hgc.currentState = HalloweenGhostChaser.ChaseState.Grabbing;
                    hgc.grabTime = Time.time;
                    hgc.targetPlayer = player;
                }
            }
            else
                NotificationManager._v3_msg_("<color=grey>[</color><color=red>ERROR</color><color=grey>]</color> You are not master client.");
        }

        public static void LucyAttackGun()
        {
            if (GetGunInput(false))
            {
                var GunData = RenderGun();
                RaycastHit Ray = GunData.Ray;

                if (gunLocked && lockTarget != null)
                    LucyAttack(lockTarget.GetPlayer());

                if (GetGunInput(true))
                {
                    VRRig gunTarget = Ray.collider.GetComponentInParent<VRRig>();
                    if (gunTarget && !gunTarget.IsLocal())
                    {
                        gunLocked = true;
                        lockTarget = gunTarget;
                    }
                }
            }
            else
            {
                if (gunLocked)
                    gunLocked = false;
            }
        }

        public static void LucyHarassGun()
        {
            if (GetGunInput(false))
            {
                var GunData = RenderGun();
                RaycastHit Ray = GunData.Ray;

                if (gunLocked && lockTarget != null)
                {
                    HalloweenGhostChaser hgc = Lucy;
                    if (hgc.IsMine)
                    {
                        if (Time.time > lucyDelay)
                        {
                            hgc.currentState = hgc.currentState == HalloweenGhostChaser.ChaseState.Grabbing ? HalloweenGhostChaser.ChaseState.Chasing : HalloweenGhostChaser.ChaseState.Grabbing;
                            hgc.transform.position = lockTarget.transform.position + Vector3.up;
                            hgc.currentSpeed = 0f;
                            hgc.targetPlayer = GetPlayerFromVRRig(lockTarget);
                            hgc.followTarget = lockTarget.transform;
                            lucyDelay = Time.time + 0.1f;
                        }
                    }
                    else
                        NotificationManager._v3_msg_("<color=grey>[</color><color=red>ERROR</color><color=grey>]</color> You are not master client.");
                }

                if (GetGunInput(true))
                {
                    VRRig gunTarget = Ray.collider.GetComponentInParent<VRRig>();
                    if (gunTarget && !gunTarget.IsLocal())
                    {
                        gunLocked = true;
                        lockTarget = gunTarget;
                    }
                }
            }
            else
            {
                if (gunLocked)
                    gunLocked = false;
            }
        }

        public static void LucyAttackAll()
        {
            HalloweenGhostChaser hgc = Lucy;
            if (SerializePatch.OverrideSerialization != null)
            {
                SerializePatch.OverrideSerialization = () =>
                {
                    MassSerialize(true, new[] { hgc.GetView });
                    return false;
                };
            }

            if (hgc.IsMine)
            {
                if (Time.time > hgc.grabTime + hgc.grabDuration + 0.1f)
                {
                    foreach (NetPlayer player in NetworkSystem.Instance.PlayerListOthers)
                    {
                        hgc.currentState = HalloweenGhostChaser.ChaseState.Grabbing;
                        hgc.grabTime = Time.time;
                        hgc.targetPlayer = player;
                        SendSerialize(Lucy.GetView, new RaiseEventOptions { TargetActors = new[] { player.ActorNumber } });
                    }
                }
            }
            else
                NotificationManager._v3_msg_("<color=grey>[</color><color=red>ERROR</color><color=grey>]</color> You are not master client.");
        }

        public static float lucyDelay;
        public static void SpazLucy()
        {
            HalloweenGhostChaser hgc = Lucy;
            if (hgc.IsMine)
            {
                if (Time.time > lucyDelay)
                {
                    hgc.timeGongStarted = hgc.timeGongStarted == 0f ? Time.time : 0f;
                    hgc.currentState = HalloweenGhostChaser.ChaseState.Gong;
                    hgc.isSummoned = true;
                    lucyDelay = Time.time + 0.1f;
                }
            }
            else NotificationManager._v3_msg_("<color=grey>[</color><color=red>ERROR</color><color=grey>]</color> You are not master client.");
        }

        public static void AnnoyingLucy()
        {
            HalloweenGhostChaser hgc = Lucy;
            if (hgc.IsMine)
            {
                if (Time.time > lucyDelay)
                {
                    hgc.timeGongStarted = Time.time;
                    hgc.grabTime = Time.time;
                    hgc.currentState = hgc.currentState == HalloweenGhostChaser.ChaseState.Gong ? HalloweenGhostChaser.ChaseState.Grabbing : HalloweenGhostChaser.ChaseState.Gong;
                    hgc.targetPlayer = GetRandomPlayer(true);
                    lucyDelay = Time.time + 0.1f;
                }
            }
            else NotificationManager._v3_msg_("<color=grey>[</color><color=red>ERROR</color><color=grey>]</color> You are not master client.");
        }

        public static void BecomeLucy()
        {
            if (!NetworkSystem.Instance.IsMasterClient)
            {
                NotificationManager._v3_msg_("<color=grey>[</color><color=red>ERROR</color><color=grey>]</color> You are not master client.");
                return;
            }

            if (Lucy != null)
            {
                VRRig.LocalRig.enabled = false;
                VRRig.LocalRig.transform.position = GorillaTagger.Instance.bodyCollider.transform.position - Vector3.up * 99999f;

                Lucy.transform.position = GorillaTagger.Instance.bodyCollider.transform.position;
                Lucy.transform.rotation = GorillaTagger.Instance.headCollider.transform.rotation;

                Lucy.currentState = HalloweenGhostChaser.ChaseState.Chasing;
                Lucy.targetPlayer = null;
            }
        }

        public static void MoveLucyGun()
        {
            if (GetGunInput(false))
            {
                var GunData = RenderGun();
                GameObject NewPointer = GunData.NewPointer;

                if (GetGunInput(true))
                {
                    if (Lucy.IsMine)
                        Lucy.transform.position = NewPointer.transform.position + Vector3.up;
                    else NotificationManager._v3_msg_("<color=grey>[</color><color=red>ERROR</color><color=grey>]</color> You are not master client.");
                }
            }
        }

        public static void FastLucy()
        {
            HalloweenGhostChaser hgc = Lucy;
            if (hgc.IsMine)
                hgc.currentSpeed = 10f;
            else NotificationManager._v3_msg_("<color=grey>[</color><color=red>ERROR</color><color=grey>]</color> You are not master client.");
        }

        public static void SlowLucy()
        {
            HalloweenGhostChaser hgc = Lucy;
            if (hgc.IsMine)
                hgc.currentSpeed = 1f;
            else NotificationManager._v3_msg_("<color=grey>[</color><color=red>ERROR</color><color=grey>]</color> You are not master client.");
        }

        public static void SpawnLurker()
        {
            if (Lurker.IsMine)
                Lurker.currentState = LurkerGhost.ghostState.patrol;
            else NotificationManager._v3_msg_("<color=grey>[</color><color=red>ERROR</color><color=grey>]</color> You are not master client.");
        }

        public static void MoveLurkerGun()
        {
            if (GetGunInput(false))
            {
                var GunData = RenderGun();
                GameObject NewPointer = GunData.NewPointer;

                if (GetGunInput(true))
                {
                    if (Lurker.IsMine)
                        Lurker.transform.position = NewPointer.transform.position + Vector3.up;
                    else NotificationManager._v3_msg_("<color=grey>[</color><color=red>ERROR</color><color=grey>]</color> You are not master client.");
                }
            }
        }

        public static void DespawnLurker()
        {
            if (Lurker.IsMine)
            {
                Lurker.currentState = LurkerGhost.ghostState.patrol;
            }
            else NotificationManager._v3_msg_("<color=grey>[</color><color=red>ERROR</color><color=grey>]</color> You are not master client.");
        }

        public static void LurkerAttack(NetPlayer player)
        {
            if (Lurker.IsMine)
            {
                if (Lurker.targetPlayer != player)
                {
                    Lurker.ChangeState(LurkerGhost.ghostState.patrol);
                    SendSerialize(Lurker.GetView);
                }

                Lurker.currentState = LurkerGhost.ghostState.possess;
                Lurker.targetPlayer = player;
            }
            else NotificationManager._v3_msg_("<color=grey>[</color><color=red>ERROR</color><color=grey>]</color> You are not master client.");
        }

        public static void LurkerAttackGun()
        {
            if (GetGunInput(false))
            {
                var GunData = RenderGun();
                RaycastHit Ray = GunData.Ray;

                if (gunLocked && lockTarget != null)
                    LurkerAttack(lockTarget.GetPlayer());

                if (GetGunInput(true))
                {
                    VRRig gunTarget = Ray.collider.GetComponentInParent<VRRig>();
                    if (gunTarget && !gunTarget.IsLocal())
                    {
                        gunLocked = true;
                        lockTarget = gunTarget;
                    }
                }
            }
            else
            {
                if (gunLocked)
                    gunLocked = false;
            }
        }

        public static void LurkerAttackAll()
        {
            if (SerializePatch.OverrideSerialization != null)
            {
                SerializePatch.OverrideSerialization = () =>
                {
                    MassSerialize(true, new[] { Lurker.GetView });
                    return false;
                };
            }

            if (Lurker.IsMine)
            {
                if (Lurker.currentState != LurkerGhost.ghostState.possess)
                {
                    foreach (NetPlayer player in NetworkSystem.Instance.PlayerListOthers)
                    {
                        Lurker.currentState = LurkerGhost.ghostState.possess;
                        Lurker.targetPlayer = player;
                        SendSerialize(Lucy.GetView, new RaiseEventOptions { TargetActors = new[] { player.ActorNumber } });
                    }
                }
            }
            else
                NotificationManager._v3_msg_("<color=grey>[</color><color=red>ERROR</color><color=grey>]</color> You are not master client.");
        }

        public static float lurkerDelay;
        public static void SpazLurker()
        {
            if (Lurker.IsMine)
            {
                if (Time.time > lurkerDelay)
                {
                    Lurker.currentState = Lurker.currentState == LurkerGhost.ghostState.charge ? LurkerGhost.ghostState.seek : LurkerGhost.ghostState.charge;
                    Lurker.targetPlayer = GetRandomPlayer(true);
                    lurkerDelay = Time.time + 0.1f;
                }
            }
            else NotificationManager._v3_msg_("<color=grey>[</color><color=red>ERROR</color><color=grey>]</color> You are not master client.");
        }

        public static void BreakLurker()
        {
            if (Lurker.IsMine)
            {
                Lurker.currentState = Lurker.currentState == LurkerGhost.ghostState.charge ? LurkerGhost.ghostState.possess : LurkerGhost.ghostState.charge;
                Lurker.targetPlayer = GetRandomPlayer(true);

                SendSerialize(Lurker.GetView);
            }
            else NotificationManager._v3_msg_("<color=grey>[</color><color=red>ERROR</color><color=grey>]</color> You are not master client.");
        }

        public static void AnnoyingLurker()
        {
            if (Lurker.IsMine)
            {
                if (Time.time > lurkerDelay)
                {
                    Lurker.currentState = Lurker.currentState == LurkerGhost.ghostState.possess ? LurkerGhost.ghostState.charge : LurkerGhost.ghostState.possess;
                    Lurker.targetPlayer = GetRandomPlayer(true);
                    lurkerDelay = Time.time + 0.1f;
                }
            }
            else NotificationManager._v3_msg_("<color=grey>[</color><color=red>ERROR</color><color=grey>]</color> You are not master client.");
        }

        public static void BecomeLurker()
        {
            if (!NetworkSystem.Instance.IsMasterClient)
            {
                NotificationManager._v3_msg_("<color=grey>[</color><color=red>ERROR</color><color=grey>]</color> You are not master client.");
                return;
            }

            if (Lurker != null)
            {
                VRRig.LocalRig.enabled = false;
                VRRig.LocalRig.transform.position = GorillaTagger.Instance.bodyCollider.transform.position - Vector3.up * 99999f;

                Lurker.transform.position = GorillaTagger.Instance.bodyCollider.transform.position;
                Lurker.transform.rotation = GorillaTagger.Instance.headCollider.transform.rotation;

                Lurker.currentState = LurkerGhost.ghostState.seek;
                SerializePatch.OverrideSerialization = () =>
                {
                    MassSerialize(true, new[] { GorillaTagger.Instance.myVRRig.GetView });

                    foreach (NetPlayer Player in NetworkSystem.Instance.PlayerListOthers)
                    {
                        Lurker.targetPlayer = Player;
                        SendSerialize(Lurker.GetView, new RaiseEventOptions { TargetActors = new[] { Player.ActorNumber } });
                    }

                    RPCProtection();

                    return false;
                };
            }
        }

        public static void BetaSetVelocityPlayer(NetPlayer victim, Vector3 velocity)
        {
            if (velocity.sqrMagnitude > 20f)
                velocity = Vector3.Normalize(velocity) * 20f;

            GorillaGuardianManager gman = (GorillaGuardianManager)GorillaGameManager.instance;
            if (gman.IsPlayerGuardian(NetworkSystem.Instance.LocalPlayer))
            {
                GetNetworkViewFromVRRig(GetVRRigFromPlayer(victim)).SendRPC("GrabbedByPlayer", victim, true, false, false);
                GetNetworkViewFromVRRig(GetVRRigFromPlayer(victim)).SendRPC("DroppedByPlayer", victim, velocity);
            }
            else
                NotificationManager._v3_msg_("<color=grey>[</color><color=red>ERROR</color><color=grey>]</color> You must be guardian.");
        }

        public static void BetaSetVelocityTargetGroup(RpcTarget victim, Vector3 velocity)
        {
            if (velocity.sqrMagnitude > 20f)
                velocity = Vector3.Normalize(velocity) * 20f;

            GorillaGuardianManager gman = (GorillaGuardianManager)GorillaGameManager.instance;
            if (gman.IsPlayerGuardian(NetworkSystem.Instance.LocalPlayer))
            {
                switch (victim)
                {
                    case RpcTarget.All:
                        {
                            foreach (VRRig rig in VRRigCache.ActiveRigs)
                            {
                                GetNetworkViewFromVRRig(rig).SendRPC("GrabbedByPlayer", GetPlayerFromVRRig(rig), true, false, false);
                                GetNetworkViewFromVRRig(rig).SendRPC("DroppedByPlayer", GetPlayerFromVRRig(rig), velocity);
                            }
                            break;
                        }
                    case RpcTarget.Others:
                        {
                            foreach (var rig in VRRigCache.ActiveRigs.Where(rig => !rig.isLocal))
                            {
                                GetNetworkViewFromVRRig(rig).SendRPC("GrabbedByPlayer", GetPlayerFromVRRig(rig), true, false, false);
                                GetNetworkViewFromVRRig(rig).SendRPC("DroppedByPlayer", GetPlayerFromVRRig(rig), velocity);
                            }

                            break;
                        }
                    case RpcTarget.MasterClient:
                        {
                            GetNetworkViewFromVRRig(GetVRRigFromPlayer(NetworkSystem.Instance.MasterClient)).SendRPC("GrabbedByPlayer", RpcTarget.Others, true, false, false);
                            GetNetworkViewFromVRRig(GetVRRigFromPlayer(NetworkSystem.Instance.MasterClient)).SendRPC("DroppedByPlayer", RpcTarget.Others, velocity);
                            break;
                        }
                }
            }
            else
                NotificationManager._v3_msg_("<color=grey>[</color><color=red>ERROR</color><color=grey>]</color> You must be guardian.");
        }

        private static float grabDelay;
        public static void GrabGun()
        {
            if (GetGunInput(false))
            {
                var GunData = RenderGun();
                RaycastHit Ray = GunData.Ray;

                if (GetGunInput(true) && Time.time > grabDelay)
                {
                    VRRig gunTarget = Ray.collider.GetComponentInParent<VRRig>();
                    if (gunTarget && !gunTarget.IsLocal())
                    {
                        GorillaGuardianManager gman = (GorillaGuardianManager)GorillaGameManager.instance;
                        if (gman.IsPlayerGuardian(NetworkSystem.Instance.LocalPlayer))
                        {
                            GetNetworkViewFromVRRig(gunTarget).SendRPC("GrabbedByPlayer", RpcTarget.Others, true, false, false);
                            RPCProtection();
                        }
                        else
                            NotificationManager._v3_msg_("<color=grey>[</color><color=red>ERROR</color><color=grey>]</color> You must be guardian.");
                        grabDelay = Time.time + 0.1f;
                    }
                }
            }
        }

        public static void GrabAll()
        {
            if (rightGrab && Time.time > grabDelay)
            {
                grabDelay = Time.time + 0.1f;
                GorillaGuardianManager guardianManager = (GorillaGuardianManager)GorillaGameManager.instance;
                if (guardianManager.IsPlayerGuardian(NetworkSystem.Instance.LocalPlayer))
                {
                    foreach (var plr in VRRigCache.ActiveRigs.Where(plr => !plr.isLocal))
                    {
                        GetNetworkViewFromVRRig(plr).SendRPC("GrabbedByPlayer", RpcTarget.Others, true, false, false);
                        RPCProtection();
                    }
                }
                else
                    NotificationManager._v3_msg_("<color=grey>[</color><color=red>ERROR</color><color=grey>]</color> You must be guardian.");
            }
        }

        private static float releaseDelay;
        public static void ReleaseGun()
        {
            if (GetGunInput(false))
            {
                var GunData = RenderGun();
                RaycastHit Ray = GunData.Ray;

                if (GetGunInput(true) && Time.time > releaseDelay)
                {
                    VRRig gunTarget = Ray.collider.GetComponentInParent<VRRig>();
                    if (gunTarget && !gunTarget.IsLocal())
                    {
                        GorillaGuardianManager gman = (GorillaGuardianManager)GorillaGameManager.instance;
                        if (gman.IsPlayerGuardian(NetworkSystem.Instance.LocalPlayer))
                        {
                            GetNetworkViewFromVRRig(gunTarget).SendRPC("DroppedByPlayer", RpcTarget.Others, new Vector3(0f, 0f, 0f));
                            RPCProtection();
                        }
                        else
                            NotificationManager._v3_msg_("<color=grey>[</color><color=red>ERROR</color><color=grey>]</color> You must be guardian.");

                        releaseDelay = Time.time + 0.1f;
                    }
                }
            }
        }

        public static void ReleaseAll()
        {
            if (rightTrigger > 0.5f && Time.time > releaseDelay)
            {
                releaseDelay = Time.time + 0.1f;
                GorillaGuardianManager guardianManager = (GorillaGuardianManager)GorillaGameManager.instance;
                if (guardianManager.IsPlayerGuardian(NetworkSystem.Instance.LocalPlayer))
                {
                    foreach (var plr in VRRigCache.ActiveRigs.Where(plr => !plr.isLocal))
                    {
                        GetNetworkViewFromVRRig(plr).SendRPC("DroppedByPlayer", RpcTarget.Others, new Vector3(0f, 0f, 0f));
                        RPCProtection();
                    }
                }
                else
                    NotificationManager._v3_msg_("<color=grey>[</color><color=red>ERROR</color><color=grey>]</color> You must be guardian.");
            }
        }

        private static float flingDelay;
        public static void FlingGun()
        {
            if (GetGunInput(false))
            {
                var GunData = RenderGun();
                RaycastHit Ray = GunData.Ray;

                if (GetGunInput(true) && Time.time > flingDelay)
                {
                    VRRig gunTarget = Ray.collider.GetComponentInParent<VRRig>();
                    if (gunTarget && !gunTarget.IsLocal())
                    {
                        BetaSetVelocityPlayer(GetPlayerFromVRRig(gunTarget), new Vector3(0f, 19.9f, 0f));
                        RPCProtection();
                        flingDelay = Time.time + 0.1f;
                    }
                }
            }
        }

        public static void FlingAll()
        {
            if (rightTrigger > 0.5f && Time.time > flingDelay)
            {
                flingDelay = Time.time + 0.1f;

                BetaSetVelocityTargetGroup(RpcTarget.Others, new Vector3(0f, 19.9f, 0f));
                RPCProtection();
            }
        }

        public static void SpazPlayerGun()
        {
            if (GetGunInput(false))
            {
                var GunData = RenderGun();
                RaycastHit Ray = GunData.Ray;

                if (gunLocked && lockTarget != null)
                {
                    if (Time.time > flingDelay)
                    {
                        BetaSetVelocityPlayer(GetPlayerFromVRRig(lockTarget), RandomVector3(50f));
                        RPCProtection();
                        flingDelay = Time.time + 0.1f;
                    }
                }
                if (GetGunInput(true))
                {
                    VRRig gunTarget = Ray.collider.GetComponentInParent<VRRig>();
                    if (gunTarget && !gunTarget.IsLocal())
                    {
                        gunLocked = true;
                        lockTarget = gunTarget;
                    }
                }
            }
            else
            {
                if (gunLocked)
                    gunLocked = false;
            }
        }

        public static void SpazAllPlayers()
        {
            if (rightTrigger > 0.5f && Time.time > flingDelay)
            {
                flingDelay = Time.time + 0.1f;
                BetaSetVelocityTargetGroup(RpcTarget.Others, RandomVector3(50f));
                RPCProtection();
            }
        }

        public static void BlockCrashGun()
        {
            if (GetGunInput(false))
            {
                var GunData = RenderGun();
                RaycastHit Ray = GunData.Ray;

                if (gunLocked && lockTarget != null)
                {
                    if (!NetworkSystem.Instance.IsMasterClient) { NotificationManager._v3_msg_("<color=grey>[</color><color=red>ERROR</color><color=grey>]</color> You are not master client."); return; }
                    Fun.RequestCreatePiece(1934114066, new Vector3(-127.6248f, 16.99441f, -217.2094f), Quaternion.identity, 0, NetPlayerToPlayer(GetPlayerFromVRRig(lockTarget)), false, true);
                }
                if (GetGunInput(true))
                {
                    VRRig gunTarget = Ray.collider.GetComponentInParent<VRRig>();
                    if (gunTarget && !gunTarget.IsLocal())
                    {
                        gunLocked = true;
                        lockTarget = gunTarget;
                    }
                }
            }
            else
            {
                if (gunLocked)
                    gunLocked = false;
            }
        }

        public static void BlockCrashAll()
        {
            if (rightTrigger > 0.5f)
            {
                if (!NetworkSystem.Instance.IsMasterClient) { NotificationManager._v3_msg_("<color=grey>[</color><color=red>ERROR</color><color=grey>]</color> You are not master client."); return; }
                Fun.RequestCreatePiece(1934114066, new Vector3(-127.6248f, 16.99441f, -217.2094f), Quaternion.identity, 0, RpcTarget.Others, false, true);
            }
        }

        private static int archiveIncrement;
        public static int GetProjectileIncrement(Vector3 Position, Vector3 Velocity, float Scale)
        {
            try
            {
                GameObject SlingshotProjectileGameObject = new GameObject("SlingshotProjectileHolder");
                SlingshotProjectile SlingshotProjectile = SlingshotProjectileGameObject.AddComponent<SlingshotProjectile>();

                int Data = ProjectileTracker.AddAndIncrementLocalProjectile(SlingshotProjectile, Velocity, Position, Scale);
                archiveIncrement = Data;

                Object.Destroy(SlingshotProjectileGameObject);
                return Data;
            }
            catch (Exception ex)
            {
                LogManager._v3_out_("Falling back to archiveIncrement: " + ex.Message);
                archiveIncrement++;
                return archiveIncrement;
            }
        }

        private static float timeSinceCallInvalidated;
        public static void DisableSnowballImpactEffect()
        {
            if (PhotonNetwork.InRoom && Time.time < snowballDelay + 0.15f && Time.time > timeSinceCallInvalidated)
            {
                timeSinceCallInvalidated = Time.time + RoomSystem.callbackInstance.roomSettings.PlayerEffectLimiter.timeCooldown;

                object[] playerEffectData = new object[6];
                playerEffectData[0] = -1;
                playerEffectData[1] = -1;

                object[] sendEventData = new object[3];
                sendEventData[0] = NetworkSystem.Instance.ServerTimestamp;
                sendEventData[1] = (byte)6;
                sendEventData[2] = playerEffectData;

                PhotonNetwork.RaiseEvent(3, sendEventData, new RaiseEventOptions { Receivers = ReceiverGroup.All }, SendOptions.SendUnreliable);

                RPCProtection();
            }
        }

        public static int snowballScale = 5;
        public static void ChangeSnowballScale(bool positive = true)
        {
            if (positive)
                snowballScale++;
            else
                snowballScale--;

            if (snowballScale > 5)
                snowballScale = 0;

            if (snowballScale < 0)
                snowballScale = 5;

            Buttons.GetIndex("Change Snowball Scale").overlapText = "Change Snowball Scale <color=grey>[</color><color=green>" + (snowballScale + 1) + "</color><color=grey>]</color>";
        }

        public static int snowballMultiplicationFactor = 1;
        public static void ChangeSnowballMultiplicationFactor(bool positive = true)
        {
            if (positive)
                snowballMultiplicationFactor++;
            else
                snowballMultiplicationFactor--;

            if (snowballMultiplicationFactor > 5)
                snowballMultiplicationFactor = 1;

            if (snowballMultiplicationFactor < 1)
                snowballMultiplicationFactor = 5;

            Buttons.GetIndex("Change Snowball Multiplication Factor").overlapText = "Change Snowball Multiplication Factor <color=grey>[</color><color=green>" + snowballMultiplicationFactor + "</color><color=grey>]</color>";
        }

        public static float _snowballSpawnDelay = 0.8f;
        public static float SnowballSpawnDelay
        {
            get { return _snowballSpawnDelay * snowballMultiplicationFactor; }
            set { _snowballSpawnDelay = value; }
        }

        public static Coroutine DisableCoroutine;
        public static IEnumerator DisableSnowball(bool rigDisabled)
        {
            yield return new WaitForSeconds(0.3f);

            if (rigDisabled)
                VRRig.LocalRig.enabled = true;
            DistancePatch.enabled = false;

            foreach (SnowballThrowable snowball in snowballDict.Values)
                try { snowball.SetSnowballActiveLocal(false); } catch { }
        }

        public static IEnumerator InvisibleSnowball(Vector3 Pos, Vector3 Vel, int Mode, Player Target = null, int? customScale = null, bool ignoreMultiply = false)
        {
            if (!PhotonNetwork.InRoom)
                yield break;

            RaiseEventOptions options = null;
            switch (Mode)
            {
                case 0:
                    options = new RaiseEventOptions { Receivers = ReceiverGroup.All };
                    break;
                case 1:
                    options = new RaiseEventOptions { Receivers = ReceiverGroup.Others };
                    break;
                case 2:
                    options = new RaiseEventOptions { TargetActors = new[] { Target.ActorNumber } };
                    break;
            }

            SnowballThrowable left = GetProjectile($"{Projectiles.SnowballName}LeftAnchor");
            SnowballThrowable right = GetProjectile($"{Projectiles.SnowballName}RightAnchor");

            left.SetSnowballActiveLocal(true);
            right.SetSnowballActiveLocal(true);

            SendSerialize(GorillaTagger.Instance.myVRRig.reliableView, options);

            left.SetSnowballActiveLocal(false);
            right.SetSnowballActiveLocal(false);

            RPCProtection();

            yield return null;
            yield return null;

            InvisibleSnowballs = false;
            BetaSpawnSnowball(Pos, Vel, Mode, Target, customScale, ignoreMultiply);
            InvisibleSnowballs = true;

            foreach (SnowballThrowable snowball in snowballDict.Values)
                try { snowball.SetSnowballActiveLocal(false); } catch { }

            SendSerialize(GorillaTagger.Instance.myVRRig.reliableView, options);
            RPCProtection();
        }

        public static bool SnowballHandIndex;
        public static bool NoTeleportSnowballs;
        public static bool InvisibleSnowballs;

        public static void BetaSpawnSnowball(Vector3 Pos, Vector3 Vel, int Mode, Player Target = null, int? customScale = null, bool ignoreMultiply = false)
        {
            if (InvisibleSnowballs)
            {
                CoroutineManager.instance.StartCoroutine(InvisibleSnowball(Pos, Vel, Mode, Target, customScale, ignoreMultiply));
                return;
            }

            try
            {
                RaiseEventOptions options = null;
                switch (Mode)
                {
                    case 0:
                        options = new RaiseEventOptions { Receivers = ReceiverGroup.All };
                        break;
                    case 1:
                        options = new RaiseEventOptions { Receivers = ReceiverGroup.Others };
                        break;
                    case 2:
                        options = new RaiseEventOptions { TargetActors = new[] { Target.ActorNumber } };
                        break;
                }

                Vector3 archivePosition = VRRig.LocalRig.transform.position;
                bool isTooFar = Vector3.Distance(Pos, GorillaTagger.Instance.bodyCollider.transform.position) > 3.9f;
                if (isTooFar)
                {
                    if (!NoTeleportSnowballs)
                        VRRig.LocalRig.enabled = false;
                    VRRig.LocalRig.transform.position = Pos + new Vector3(0f, Vel.y > 0f ? -3f : 3f, 0f);
                }

                if (NoTeleportSnowballs && isTooFar)
                {
                    SendSerialize(GorillaTagger.Instance.myVRRig.GetView, options, -10);
                    SendSerialize(GorillaTagger.Instance.myVRRig.GetView, options);
                }

                for (int i = 0; i < (ignoreMultiply ? 1 : snowballMultiplicationFactor); i++)
                {
                    SnowballHandIndex = !SnowballHandIndex;
                    Vel = Vel.ClampMagnitudeSafe(50f);

                    DistancePatch.enabled = true;

                    if (DisableCoroutine != null)
                        CoroutineManager.instance.StopCoroutine(DisableCoroutine);

                    DisableCoroutine = CoroutineManager.instance.StartCoroutine(DisableSnowball(isTooFar && !NoTeleportSnowballs));

                    GrowingSnowballThrowable GrowingSnowball = GetProjectile($"{Projectiles.SnowballName}{(SnowballHandIndex ? "Right" : "Left")}Anchor") as GrowingSnowballThrowable;

                    PhotonNetwork.RaiseEvent(176, new object[]
                    {
                        GrowingSnowball.changeSizeEvent._eventId,
                        customScale ?? snowballScale,
                    }, options, new SendOptions
                    {
                        Reliability = false
                    });

                    PhotonNetwork.RaiseEvent(176, new object[]
                    {
                        GrowingSnowball.snowballThrowEvent._eventId,
                        Pos,
                        Vel,
                        GetProjectileIncrement(Pos, Vel, snowballScale)
                    }, options, new SendOptions
                    {
                        Reliability = false
                    });

                    GrowingSnowballThrowable nextGrowingSnowball = GetProjectile($"{Projectiles.SnowballName}{(SnowballHandIndex ? "Left" : "Right")}Anchor") as GrowingSnowballThrowable;
                    nextGrowingSnowball.SetSnowballActiveLocal(true);
                }

                if (NoTeleportSnowballs && isTooFar)
                {
                    VRRig.LocalRig.transform.position = archivePosition;
                    SendSerialize(GorillaTagger.Instance.myVRRig.GetView, options);
                }
            }
            catch (Exception e)
            {
                LogManager.LogError($"Error in BetaSpawnSnowball: {e}");
            }

            RPCProtection();
        }

        public static void BetaSnowballImpact(Player Target)
        {
            object[] playerEffectData = new object[6];
            playerEffectData[0] = Target.ActorNumber;
            playerEffectData[1] = 0;

            object[] sendEventData = new object[3];
            sendEventData[0] = NetworkSystem.Instance.ServerTimestamp;
            sendEventData[1] = (byte)6;
            sendEventData[2] = playerEffectData;

            PhotonNetwork.RaiseEvent(3, sendEventData, new RaiseEventOptions { Receivers = ReceiverGroup.All }, SendOptions.SendUnreliable);
            RPCProtection();
        }

        private static float snowballDelay;
        public static void SnowballAirstrikeGun()
        {
            if (GetGunInput(false))
            {
                var GunData = RenderGun();
                GameObject NewPointer = GunData.NewPointer;

                if (GetGunInput(true) && Time.time > snowballDelay)
                {
                    BetaSpawnSnowball(NewPointer.transform.position + new Vector3(0f, 50f, 0f), Vector3.zero, 0);
                    snowballDelay = Time.time + SnowballSpawnDelay;
                }
            }
        }

        private static Vector3? snowballNukePosition;
        private static Vector3 snowballNukeVelocity;
        public static void SnowballNukeGun()
        {
            if (GetGunInput(false))
            {
                var GunData = RenderGun();
                GameObject NewPointer = GunData.NewPointer;

                if (GetGunInput(true))
                {
                    snowballNukePosition ??= NewPointer.transform.position + (Vector3.up * 50f);
                    snowballNukePosition += Time.unscaledDeltaTime * Physics.gravity;
                    snowballNukeVelocity += Time.unscaledDeltaTime * Physics.gravity;

                    if (Time.time > snowballDelay)
                    {
                        BetaSpawnSnowball(snowballNukePosition.Value + RandomVector3(snowballNukeVelocity.magnitude * 0.25f).X_Z(), snowballNukeVelocity, 0);
                        snowballDelay = Time.time + SnowballSpawnDelay;
                    }
                }
            }
            else
            {
                snowballNukePosition = null;
                snowballNukeVelocity = Vector3.zero;
            }
        }

        public static void SnowballRain()
        {
            if (rightTrigger > 0.5f)
            {
                if (Time.time > snowballDelay)
                {
                    BetaSpawnSnowball(VRRig.LocalRig.transform.position + new Vector3(Random.Range(-5f, 5f), 5f, Random.Range(-5f, 5f)), Vector3.zero, 0);
                    snowballDelay = Time.time + SnowballSpawnDelay;
                }
            }
        }

        public static void SnowballHail()
        {
            if (rightTrigger > 0.5f)
            {
                if (Time.time > snowballDelay)
                {
                    BetaSpawnSnowball(VRRig.LocalRig.transform.position + new Vector3(Random.Range(-5f, 5f), 5f, Random.Range(-5f, 5f)), new Vector3(0f, -50f, 0f), 0);
                    snowballDelay = Time.time + SnowballSpawnDelay;
                }
            }
        }

        public static void SnowballFountain()
        {
            if (rightTrigger > 0.5f)
            {
                if (Time.time > snowballDelay)
                {
                    BetaSpawnSnowball(VRRig.LocalRig.transform.position + Vector3.up, new Vector3(Random.Range(-15f, 15f), Random.Range(20f, 25f), Random.Range(-15f, 15f)), 0);
                    snowballDelay = Time.time + SnowballSpawnDelay;
                }
            }
        }

        public static GameObject FountainObject;
        public static void SnowballPositionalFountain()
        {
            if (rightGrab)
            {
                if (FountainObject == null)
                {
                    FountainObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    Object.Destroy(FountainObject.GetComponent<SphereCollider>());
                    FountainObject.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
                }
                FountainObject.transform.position = GorillaTagger.Instance.rightHandTransform.position;
            }
            if (FountainObject != null)
            {
                if (rightTrigger > 0.5f)
                {
                    if (Time.time > snowballDelay)
                    {
                        BetaSpawnSnowball(FountainObject.transform.position, new Vector3(Random.Range(-15f, 15f), Random.Range(20f, 25f), Random.Range(-15f, 15f)), 0);
                        snowballDelay = Time.time + SnowballSpawnDelay;
                    }
                }
                else
                    FountainObject.GetComponent<Renderer>().material.color = buttonColors[0].GetColor(0);
            }
        }

        public static void DisableSnowballPositionalFountain()
        {
            if (FountainObject != null)
            {
                Object.Destroy(FountainObject);
                FountainObject = null;
            }
        }

        public static void SnowballOrbit()
        {
            if (rightTrigger > 0.5f)
            {
                if (Time.time > snowballDelay)
                {
                    BetaSpawnSnowball(GorillaTagger.Instance.headCollider.transform.position + new Vector3(MathF.Cos(Time.frameCount / 30f), 2f, MathF.Sin(Time.frameCount / 30f)), new Vector3(0f, 50f, 0f), 0);
                    snowballDelay = Time.time + SnowballSpawnDelay;
                }
            }
        }

        public static void SnowballAura()
        {
            if (rightTrigger > 0.5f)
            {
                if (Time.time > snowballDelay)
                {
                    BetaSpawnSnowball(GorillaTagger.Instance.headCollider.transform.position + RandomVector3(), RandomVector3() * 20f, 0);
                    snowballDelay = Time.time + SnowballSpawnDelay;
                }
            }
        }

        public static void SnowballMushroom()
        {
            if (rightTrigger > 0.5f)
            {
                if (Time.time > snowballDelay)
                {
                    int count = 15;

                    snowballDelay = Time.time + SnowballSpawnDelay * count;
                    for (int i = 0; i < count; i++)
                    {
                        float rotation = i == 0 ? 0f : (360f / count * i);
                        BetaSpawnSnowball(GorillaTagger.Instance.headCollider.transform.position + Vector3.up, (Vector3.up * 15f) + (Quaternion.Euler(0f, rotation, 0f) * Vector3.forward).normalized * 2.5f, 0);
                    }
                }
            }
        }

        public static void SnowballSpam()
        {
            if ((rightGrab || Mouse.current.leftButton.isPressed) && Time.time > snowballDelay)
            {
                Vector3 startpos = GorillaTagger.Instance.rightHandTransform.position;
                Vector3 charvel = GorillaLocomotion.GTPlayer.Instance.RigidbodyVelocity;

                if (Buttons.GetIndex("Shoot Projectiles").enabled)
                {
                    charvel = GorillaLocomotion.GTPlayer.Instance.RigidbodyVelocity + GetGunDirection(GorillaTagger.Instance.rightHandTransform) * ShootStrength;
                    if (Mouse.current.leftButton.isPressed)
                    {
                        Ray ray = TPC.ScreenPointToRay(Mouse.current.position.ReadValue());
                        Physics.Raycast(ray, out var hit, 512f, NoInvisLayerMask());
                        charvel = hit.point - GorillaTagger.Instance.rightHandTransform.transform.position;
                        charvel.Normalize();
                        charvel *= ShootStrength * 2f;
                    }
                }

                if (Buttons.GetIndex("Random Direction").enabled)
                    charvel = RandomVector3(100f);

                if (Buttons.GetIndex("Above Players").enabled)
                {
                    VRRig targetRig = GetTargetPlayer();
                    startpos = targetRig.transform.position + Vector3.up;
                }

                if (Buttons.GetIndex("Rain Projectiles").enabled)
                {
                    startpos = GorillaTagger.Instance.headCollider.transform.position + new Vector3(Random.Range(-2f, 2f), 2f, Random.Range(-2f, 2f));
                    charvel = Vector3.zero;
                }

                if (Buttons.GetIndex("Projectile Aura").enabled)
                {
                    float time = Time.frameCount;
                    startpos = GorillaTagger.Instance.headCollider.transform.position + new Vector3(MathF.Cos(time / 20), 2, MathF.Sin(time / 20));
                }

                if (Buttons.GetIndex("True Projectile Aura").enabled)
                {
                    startpos = GorillaTagger.Instance.headCollider.transform.position + RandomVector3();
                    charvel = RandomVector3(10f);
                }

                if (Buttons.GetIndex("Projectile Fountain").enabled)
                {
                    startpos = GorillaTagger.Instance.headCollider.transform.position + new Vector3(0, 1, 0);
                    charvel = new Vector3(Random.Range(-10, 10), 15, Random.Range(-10, 10));
                }

                if (Buttons.GetIndex("Include Hand Velocity").enabled)
                    charvel = GorillaLocomotion.GTPlayer.Instance.RightHand.velocityTracker.GetAverageVelocity(true, 0);

                BetaSpawnSnowball(startpos, charvel, 0);
                snowballDelay = Time.time + SnowballSpawnDelay;
            }
        }

        public static void SnowballGun()
        {
            if (GetGunInput(false))
            {
                var GunData = RenderGun();
                GameObject NewPointer = GunData.NewPointer;

                if (GetGunInput(true) && Time.time > snowballDelay)
                {
                    BetaSpawnSnowball(NewPointer.transform.position + Vector3.up, new Vector3(0f, 30f, 0f), 0);
                    snowballDelay = Time.time + SnowballSpawnDelay;
                }
            }
        }

        public static void SnowballMinigun()
        {
            if ((rightGrab || Mouse.current.leftButton.isPressed) && Time.time > snowballDelay)
            {
                Vector3 velocity = GetGunDirection(GorillaTagger.Instance.rightHandTransform) * ShootStrength;
                if (Mouse.current.leftButton.isPressed)
                {
                    Ray ray = TPC.ScreenPointToRay(Mouse.current.position.ReadValue());
                    Physics.Raycast(ray, out var hit, 512f, NoInvisLayerMask());
                    velocity = hit.point - GorillaTagger.Instance.rightHandTransform.transform.position;
                    velocity.Normalize();
                    velocity *= ShootStrength * 2f;
                }

                BetaSpawnSnowball(GorillaTagger.Instance.rightHandTransform.position, velocity, 0);
                snowballDelay = Time.time + SnowballSpawnDelay;
            }
        }

        public static void SnowballShotgun()
        {
            if ((rightGrab || Mouse.current.leftButton.isPressed) && Time.time > snowballDelay)
            {
                Vector3 velocity = GetGunDirection(GorillaTagger.Instance.rightHandTransform) * ShootStrength;
                if (Mouse.current.leftButton.isPressed)
                {
                    Ray ray = TPC.ScreenPointToRay(Mouse.current.position.ReadValue());
                    Physics.Raycast(ray, out var hit, 512f, NoInvisLayerMask());
                    velocity = hit.point - GorillaTagger.Instance.rightHandTransform.transform.position;
                    velocity.Normalize();
                    velocity *= ShootStrength * 2f;
                }

                for (int i = 0; i < 5; i++)
                    BetaSpawnSnowball(GorillaTagger.Instance.rightHandTransform.position, velocity + RandomVector3(5f), 0);

                snowballDelay = Time.time + (SnowballSpawnDelay * 5f);
            }
        }

        public static void SnowballWall()
        {
            if ((rightGrab || Mouse.current.leftButton.isPressed) && Time.time > snowballDelay)
            {
                Vector3 velocity = GetGunDirection(GorillaTagger.Instance.rightHandTransform) * ShootStrength;
                if (Mouse.current.leftButton.isPressed)
                {
                    Ray ray = TPC.ScreenPointToRay(Mouse.current.position.ReadValue());
                    Physics.Raycast(ray, out var hit, 512f, NoInvisLayerMask());
                    velocity = hit.point - GorillaTagger.Instance.rightHandTransform.transform.position;
                    velocity.Normalize();
                    velocity *= ShootStrength * 2f;
                }

                for (int x = -2; x <= 2; x++)
                {
                    for (int y = -2; y <= 2; y++)
                    {
                        var (_, _, up, _, right) = ControllerUtilities.GetTrueRightHand();
                        BetaSpawnSnowball(GorillaTagger.Instance.rightHandTransform.position + (right * (x * 0.666f)) + (up * (y * 0.666f)), velocity, 0);
                    }
                }

                snowballDelay = Time.time + (SnowballSpawnDelay * 25f);
            }
        }

        public static GameObject BombObject;
        public static void SnowballBomb()
        {
            if (rightGrab)
            {
                if (BombObject == null)
                {
                    BombObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    Object.Destroy(BombObject.GetComponent<SphereCollider>());
                    BombObject.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
                }
                BombObject.transform.position = GorillaTagger.Instance.rightHandTransform.position;
            }
            if (BombObject != null)
            {
                if (rightPrimary)
                {
                    if (Time.time > snowballDelay)
                    {
                        for (int i = 0; i < 10; i++)
                            BetaSpawnSnowball(BombObject.transform.position, RandomVector3(500f), 0);

                        snowballDelay = Time.time + (SnowballSpawnDelay * 10f);
                    }

                    Object.Destroy(BombObject);
                    BombObject = null;
                }
                else
                    BombObject.GetComponent<Renderer>().material.color = buttonColors[0].GetColor(0);
            }
        }

        private static float rpgDelay;
        private static bool rpgShot;

        public static void SnowballGrenade()
        {
            if (rightGrab && !rpgShot)
            {
                rpgShot = true;

                SnowballThrowable Throwable = GetProjectile("SnowballRightAnchor");
                Throwable.SetSnowballActiveLocal(true);

                Color targetColor = new Color(0f, 0.6f, 0f, 1f);
                VRRig.LocalRig.SetThrowableProjectileColor(true, targetColor);
                Throwable.randomizeColor = true;
                Throwable.ApplyColor(targetColor);
            }
        }

        public static void SnowballRPG()
        {
            if (rightGrab && Time.time > rpgDelay && Time.time > snowballDelay)
            {
                rpgDelay = Time.time + 1f;
                rpgShot = true;

                BetaSpawnSnowball(GorillaTagger.Instance.rightHandTransform.position, GetGunDirection(GorillaTagger.Instance.rightHandTransform) * ShootStrength, 0);
                snowballDelay = Time.time + SnowballSpawnDelay;
            }
        }

        public static void OnSnowballHit(SlingshotProjectile slingshotProjectile, Collision _)
        {
            if (rpgShot && slingshotProjectile.projectileOwner == NetworkSystem.Instance.LocalPlayer)
            {
                rpgShot = false;
                if (Time.time > snowballDelay)
                {
                    for (int i = 0; i < 10; i++)
                        BetaSpawnSnowball(slingshotProjectile.transform.position + (Vector3.up * 0.3f), RandomVector3(500f), 0);

                    snowballDelay = Time.time + (SnowballSpawnDelay * 10f);
                }
            }
        }

        public static void DisableBomb()
        {
            if (BombObject != null)
            {
                Object.Destroy(BombObject);
                BombObject = null;
            }
        }

        public static void GiveSnowballMinigun()
        {
            if (GetGunInput(false))
            {
                var GunData = RenderGun();
                RaycastHit Ray = GunData.Ray;

                if (gunLocked && lockTarget != null && Time.time > snowballDelay)
                {
                    Vector3 velocity = lockTarget.rightHandTransform.transform.forward * ShootStrength;

                    BetaSpawnSnowball(lockTarget.rightHandTransform.transform.position, velocity, 0);
                    snowballDelay = Time.time + SnowballSpawnDelay;
                }
                if (GetGunInput(true))
                {
                    VRRig gunTarget = Ray.collider.GetComponentInParent<VRRig>();
                    if (gunTarget && !gunTarget.IsLocal())
                    {
                        gunLocked = true;
                        lockTarget = gunTarget;
                    }
                }
            }
            else
            {
                if (gunLocked)
                {
                    gunLocked = false;
                    VRRig.LocalRig.enabled = true;
                }
            }
        }

        public static void SnowballParticleGun()
        {
            if (GetGunInput(false))
            {
                var GunData = RenderGun();
                GameObject NewPointer = GunData.NewPointer;

                if (GetGunInput(true) && Time.time > snowballDelay)
                {
                    BetaSpawnSnowball(NewPointer.transform.position + new Vector3(0f, 0.1f, 0f), new Vector3(0f, 0f, 0f), 0);
                    snowballDelay = Time.time + SnowballSpawnDelay;
                }
            }
        }

        public static void SnowballImpactEffectGun()
        {
            if (GetGunInput(false))
            {
                var GunData = RenderGun();
                RaycastHit Ray = GunData.Ray;

                if (GetGunInput(true) && Time.time > snowballDelay)
                {
                    VRRig gunTarget = Ray.collider.GetComponentInParent<VRRig>();
                    if (gunTarget && !gunTarget.IsLocal())
                    {
                        snowballDelay = Time.time + SnowballSpawnDelay;
                        BetaSnowballImpact(NetPlayerToPlayer(GetPlayerFromVRRig(gunTarget)));
                    }
                }
            }
        }

        public static void SnowballPunchMod()
        {
            if (Time.time > snowballDelay)
            {
                foreach (VRRig rig in VRRigCache.ActiveRigs)
                {
                    if (!rig.isLocal && (Vector3.Distance(GorillaTagger.Instance.leftHandTransform.position, rig.headMesh.transform.position) < 0.25f || Vector3.Distance(GorillaTagger.Instance.rightHandTransform.position, rig.headMesh.transform.position) < 0.25f))
                    {
                        Vector3 targetDirection = GorillaTagger.Instance.headCollider.transform.position - rig.headMesh.transform.position;
                        BetaSpawnSnowball(GorillaTagger.Instance.headCollider.transform.position + new Vector3(0f, 0.5f, 0f) + new Vector3(targetDirection.x, 0f, targetDirection.z).normalized / 1.7f, new Vector3(0f, -500f, 0f), 2, NetPlayerToPlayer(GetPlayerFromVRRig(rig)));

                        if (Buttons.GetIndex("Graphic Punch Mod").enabled)
                            Projectiles.BetaFireProjectile("AppleLeftAnchor", rig.headMesh.transform.position, Vector3.down * 600f, new Color32(100, 0, 0, 255));

                        snowballDelay = Time.time + SnowballSpawnDelay;
                    }
                }
            }
        }

        private static readonly Dictionary<VRRig, float> boxingDelay = new Dictionary<VRRig, float> { };
        public static void SnowballBoxing()
        {
            foreach (VRRig rig1 in VRRigCache.ActiveRigs)
            {
                if (Time.time < GetBoxingDelay(rig1))
                    continue;

                foreach (VRRig rig2 in VRRigCache.ActiveRigs)
                {
                    if (rig2 == rig1) continue;
                    if (Vector3.Distance(rig2.leftHandTransform.position, rig1.headMesh.transform.position) < 0.25f || Vector3.Distance(rig2.rightHandTransform.position, rig1.headMesh.transform.position) < 0.25f)
                    {
                        Vector3 targetDirection = rig2.headMesh.transform.position - rig1.headMesh.transform.position;
                        BetaSpawnSnowball(rig1.headMesh.transform.position + new Vector3(0f, 0.5f, 0f) + new Vector3(targetDirection.x, 0f, targetDirection.z).normalized / 1.7f, new Vector3(0f, -500f, 0f), 2, rig1.GetPhotonPlayer());
                        SetBoxingDelay(rig1);
                    }
                }
            }
        }

        public static void SnowballDash()
        {
            foreach (VRRig rig in VRRigCache.ActiveRigs)
            {
                if (Time.time < GetBoxingDelay(rig))
                    return;

                if (!rig.isOfflineVRRig && rig.rightThumb.calcT > 0.5f)
                {
                    BetaSpawnSnowball(rig.headMesh.transform.position + new Vector3(0f, 0.5f, 0f) + new Vector3(-rig.headMesh.transform.forward.x, 0f, -rig.headMesh.transform.forward.z) * 1.5f, new Vector3(0f, -300f, 0f), 2, rig.GetPhotonPlayer());
                    SetBoxingDelay(rig);
                }
            }
        }

        public static void SnowballHighJump()
        {
            foreach (VRRig rig in VRRigCache.ActiveRigs)
            {
                if (Time.time < GetBoxingDelay(rig))
                    return;
                Physics.Raycast(rig.bodyTransform.position - new Vector3(0f, 0.2f, 0f), Vector3.down, out var Ray, 512f, GorillaLocomotion.GTPlayer.Instance.locomotionEnabledLayers);

                if (!rig.isOfflineVRRig && (Ray.distance > 0.12f && Ray.distance < 0.2f))
                {
                    BetaSpawnSnowball(rig.headMesh.transform.position + new Vector3(0f, -0.7f, 0f), new Vector3(0f, -500f, 0f), 2, rig.GetPhotonPlayer());
                    SetBoxingDelay(rig);
                }
            }
        }

        public static AudioClip KameStart;
        public static AudioClip KameStop;
        public static VoiceManager.Clip KameSound;
        public static Coroutine KameStartCoroutine;

        public static void Enable_Kamehameha()
        {
            LoadSoundFromURL($"{PluginInfo.ServerResourcePath}/Audio/Mods/Overpowered/Kamehameha/start.ogg", "Audio/Mods/Overpowered/Kamehameha/start.ogg", clip => KameStart = clip);
            LoadSoundFromURL($"{PluginInfo.ServerResourcePath}/Audio/Mods/Overpowered/Kamehameha/end.ogg", "Audio/Mods/Overpowered/Kamehameha/end.ogg", clip => KameStop = clip);
        }

        public static void Kamehameha()
        {
            if (!PhotonNetwork.InRoom) return;
            bool attacking = leftGrab && rightGrab && leftTrigger > 0.5f && rightTrigger > 0.5f;
            switch (attacking)
            {
                case true when KameStartCoroutine == null:
                    KameStartCoroutine = CoroutineManager.instance.StartCoroutine(StartKame());
                    break;
                case false when KameStartCoroutine != null:
                    CoroutineManager.instance.StopCoroutine(KameStartCoroutine);
                    KameStartCoroutine = null;

                    CoroutineManager.instance.StartCoroutine(EndKame());
                    break;
            }
        }

        public static GameObject cursor;
        public static IEnumerator StartKame()
        {
            PlayKameSound(KameStart);
            yield return new WaitForSeconds(0.5f);

            GrowingSnowballThrowable leftSnowball = GetProjectile($"{Projectiles.SnowballName}LeftAnchor") as GrowingSnowballThrowable;
            GrowingSnowballThrowable rightSnowball = GetProjectile($"{Projectiles.SnowballName}RightAnchor") as GrowingSnowballThrowable;
            GrowingSnowballThrowable[] snowballs = { leftSnowball, rightSnowball };

            foreach (GrowingSnowballThrowable snowball in snowballs)
                snowball.SetSnowballActiveLocal(true);

            float startTime = Time.time;

            while (true)
            {
                try
                {
                    if (cursor == null)
                    {
                        cursor = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                        cursor.transform.localScale = new Vector3(0.05f, 0.05f, 0.05f);
                        cursor.GetComponent<Renderer>().material.color = Color.white;

                        Object.Destroy(cursor.GetComponent<Collider>());
                    }

                    Physics.Raycast(GorillaTagger.Instance.headCollider.transform.position, GorillaTagger.Instance.headCollider.transform.forward, out var RayPoint, 512f, GorillaLocomotion.GTPlayer.Instance.locomotionEnabledLayers);
                    cursor.transform.position = RayPoint.point == Vector3.zero ? (RayPoint.transform.position + (RayPoint.transform.forward * 20f)) : RayPoint.point;
                }
                catch { }

                try
                {
                    int sizeIndex = Mathf.Clamp((int)Mathf.Floor((Time.time - startTime) * 1.75f), 0, 5);

                    foreach (GrowingSnowballThrowable snowball in snowballs)
                    {
                        if (snowball.sizeLevel != sizeIndex)
                            snowball.SetSizeLevelAuthority(sizeIndex);
                    }

                    VRRig.LocalRig.SetThrowableProjectileColor(true, Color.white);
                    leftSnowball.randomizeColor = true;
                    leftSnowball.ApplyColor(Color.white);

                    VRRig.LocalRig.SetThrowableProjectileColor(false, Color.cyan);
                    rightSnowball.randomizeColor = true;
                    rightSnowball.ApplyColor(Color.cyan);
                }
                catch { }

                Vector3 snowballPosition = GorillaTagger.Instance.leftHandTransform.position.Lerp(GorillaTagger.Instance.rightHandTransform.position, 0.5f);

                foreach (Transform handTransform in new[] { GorillaTagger.Instance.leftHandTransform, GorillaTagger.Instance.rightHandTransform })
                {
                    handTransform.position = snowballPosition;
                    handTransform.rotation = RandomQuaternion();
                }

                yield return null;
            }
        }

        public static IEnumerator EndKame()
        {
            PlayKameSound(KameStop);
            yield return new WaitForSeconds(0.5f);

            float startTime = Time.time;

            while (Time.time - startTime < 3f)
            {
                if (cursor == null)
                {
                    cursor = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    cursor.transform.localScale = new Vector3(0.05f, 0.05f, 0.05f);
                    cursor.GetComponent<Renderer>().material.color = Color.white;

                    Object.Destroy(cursor.GetComponent<Collider>());
                }

                try
                {
                    Physics.Raycast(GorillaTagger.Instance.headCollider.transform.position, GorillaTagger.Instance.headCollider.transform.forward, out var RayPoint, 512f, GorillaLocomotion.GTPlayer.Instance.locomotionEnabledLayers);
                    cursor.transform.position = RayPoint.point == Vector3.zero ? (RayPoint.transform.position + (RayPoint.transform.forward * 20f)) : RayPoint.point;
                }
                catch { }

                Vector3 snowballPosition = GorillaTagger.Instance.leftHandTransform.position.Lerp(GorillaTagger.Instance.rightHandTransform.position, 0.5f);
                Vector3 targetDirection = (cursor.transform.position - snowballPosition).normalized * 60f;

                BetaSpawnSnowball(snowballPosition, targetDirection, 0);
                yield return new WaitForSeconds(SnowballSpawnDelay);
            }

            Vector3 _ = GorillaTagger.Instance.leftHandTransform.position.Lerp(GorillaTagger.Instance.rightHandTransform.position, 0.5f);
            Vector3 __ = (cursor.transform.position - _).normalized * 30f;

            for (int i = 5; i >= 0; i--)
            {
                BetaSpawnSnowball(_, __, 0, null, i);
                yield return new WaitForSeconds(0.1f);
            }

            if (cursor != null)
                Object.Destroy(cursor);
        }

        public static void PlayKameSound(AudioClip clip)
        {
            if (RecorderPatch.enabled)
            {
                if (KameSound != null)
                {
                    VoiceManager.Get().StopAudioClip(KameSound);
                    KameSound = null;
                }
                KameSound = VoiceManager.Get().AudioClip(clip);
            }
            else
                Sound.PlayAudio(clip);
        }

        public static void Disable_Kamehameha()
        {
            VRRig.LocalRig.SetThrowableProjectileColor(false, Color.white);
            VRRig.LocalRig.SetThrowableProjectileColor(false, Color.white);
        }

        public static void SnowballSafetyBubble()
        {
            if (Time.time > snowballDelay)
            {
                foreach (VRRig rig in VRRigCache.ActiveRigs)
                {
                    if (!rig.isLocal)
                    {
                        if (Vector3.Distance(GorillaTagger.Instance.bodyCollider.transform.position, rig.transform.position) < 3f)
                        {
                            Vector3 targetDirection = GorillaTagger.Instance.headCollider.transform.position - rig.headMesh.transform.position;
                            BetaSpawnSnowball(GorillaTagger.Instance.headCollider.transform.position + new Vector3(0f, 0.5f, 0f) + new Vector3(targetDirection.x, 0f, targetDirection.z).normalized / 1.7f, new Vector3(0f, -500f, 0f), 2, NetPlayerToPlayer(GetPlayerFromVRRig(rig)));
                            snowballDelay = Time.time + SnowballSpawnDelay;
                            if (PhotonNetwork.InRoom)
                            {
                                GorillaTagger.Instance.myVRRig.SendRPC("RPC_PlayHandTap", RpcTarget.All, 248, false, 999999f);
                            }
                            else
                                VRRig.LocalRig.PlayHandTapLocal(248, false, 999999f);
                        }
                    }
                }
            }
        }

        public static void FlingPlayer(NetPlayer player)
        {
            if (Time.time > snowballDelay)
            {
                BetaSpawnSnowball(GetVRRigFromPlayer(player).transform.position + new Vector3(0f, 0.5f, 0f) + new Vector3(Random.Range(-1f, 1f), 0f, Random.Range(-1f, 1f)).normalized / 1.7f, new Vector3(0f, -500f, 0f), 2, NetPlayerToPlayer(player));
                snowballDelay = Time.time + SnowballSpawnDelay;
            }
        }

        public static void FlingPlayer(VRRig player) => FlingPlayer(GetPlayerFromVRRig(player));

        public static void SnowballFlingGun()
        {
            if (GetGunInput(false))
            {
                var GunData = RenderGun();
                RaycastHit Ray = GunData.Ray;

                if (gunLocked && lockTarget != null)
                    FlingPlayer(lockTarget);

                if (GetGunInput(true))
                {
                    VRRig gunTarget = Ray.collider.GetComponentInParent<VRRig>();
                    if (gunTarget && !gunTarget.IsLocal())
                    {
                        gunLocked = true;
                        lockTarget = gunTarget;
                    }
                }
            }
            else
            {
                if (gunLocked)
                    gunLocked = false;
            }
        }

        public static void SnowballLaunchGun()
        {
            if (GetGunInput(false))
            {
                var GunData = RenderGun();
                RaycastHit Ray = GunData.Ray;

                if (gunLocked && lockTarget != null)
                {
                    if (Time.time > snowballDelay)
                    {
                        int count = 10;

                        snowballDelay = Time.time + SnowballSpawnDelay * count;
                        for (int i = 0; i < count; i++)
                        {
                            float rotation = i == 0 ? 0f : (360f / count * i);
                            BetaSpawnSnowball(lockTarget.transform.position + new Vector3(0f, 0.5f, 0f) + (Quaternion.Euler(0f, rotation, 0f) * Vector3.forward).normalized / 1.7f, new Vector3(0f, -500f, 0f), 2, lockTarget.GetPhotonPlayer());
                        }
                    }
                }

                if (GetGunInput(true))
                {
                    VRRig gunTarget = Ray.collider.GetComponentInParent<VRRig>();
                    if (gunTarget && !gunTarget.IsLocal())
                    {
                        gunLocked = true;
                        lockTarget = gunTarget;
                    }
                }
            }
            else
            {
                if (gunLocked)
                    gunLocked = false;
            }
        }

        public static readonly List<GameObject> flingZones = new List<GameObject>();
        public static void SnowballFlingZone()
        {
            if (rightGrab)
            {
                bool isNearCheckpoint = false;
                foreach (var checkpoint in flingZones.Where(checkpoint => Vector3.Distance(GorillaTagger.Instance.rightHandTransform.position, checkpoint.transform.position) < 0.5f))
                {
                    isNearCheckpoint = true;
                    checkpoint.transform.position = GorillaTagger.Instance.rightHandTransform.transform.position;
                    break;
                }

                if (!isNearCheckpoint)
                {
                    GameObject newCheckpoint = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    Object.Destroy(newCheckpoint.GetComponent<SphereCollider>());
                    newCheckpoint.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
                    newCheckpoint.transform.position = GorillaTagger.Instance.rightHandTransform.position;
                    newCheckpoint.GetComponent<Renderer>().material.shader = Shader.Find("GUI/Text Shader");
                    newCheckpoint.GetComponent<Renderer>().material.color = new Color(1f, 0f, 0f, 0.3f);
                    flingZones.Add(newCheckpoint);
                }
            }

            if (rightTrigger > 0.5f)
            {
                foreach (var checkpoint in flingZones.ToList().Where(checkpoint => Vector3.Distance(GorillaTagger.Instance.rightHandTransform.position, checkpoint.transform.position) < 0.5f))
                {
                    flingZones.Remove(checkpoint);
                    Object.Destroy(checkpoint);
                }
            }

            if (Time.time > snowballDelay)
            {
                int flingCount = 0;
                foreach (VRRig rig in VRRigCache.ActiveRigs.Where(rig => !rig.IsLocal()))
                {
                    foreach (var checkpoint in flingZones)
                    {
                        if (Vector3.Distance(rig.transform.position, checkpoint.transform.position) < 0.5f || Vector3.Distance(rig.leftHandTransform.position, checkpoint.transform.position) < 0.5f || Vector3.Distance(rig.rightHandTransform.position, checkpoint.transform.position) < 0.5f)
                        {
                            BetaSpawnSnowball(checkpoint.transform.position, new Vector3(0f, -500f, 0f), 2, rig.GetPhotonPlayer());
                            flingCount++;
                        }
                    }
                }

                if (flingCount > 0)
                    snowballDelay = Time.time + SnowballSpawnDelay * flingCount;
            }
        }

        public static void DisableSnowballFlingZone()
        {
            foreach (GameObject checkpoint in flingZones)
                Object.Destroy(checkpoint);

            flingZones.Clear();
        }

        public static void SnowballFlingAll()
        {
            if (rightTrigger > 0.5f)
                FlingPlayer(GetTargetPlayer(SnowballSpawnDelay));
        }

        public static void SnowballFlingVerticalGun()
        {
            if (GetGunInput(false))
            {
                var GunData = RenderGun();
                RaycastHit Ray = GunData.Ray;

                if (gunLocked && lockTarget != null)
                {
                    if (Time.time > snowballDelay)
                    {
                        BetaSpawnSnowball(lockTarget.headMesh.transform.position + new Vector3(0f, -0.7f, 0f), new Vector3(0f, -500f, 0f), 2, NetPlayerToPlayer(GetPlayerFromVRRig(lockTarget)));
                        snowballDelay = Time.time + SnowballSpawnDelay;
                    }
                }
                if (GetGunInput(true))
                {
                    VRRig gunTarget = Ray.collider.GetComponentInParent<VRRig>();
                    if (gunTarget && !gunTarget.IsLocal())
                    {
                        gunLocked = true;
                        lockTarget = gunTarget;
                    }
                }
            }
            else
            {
                if (gunLocked)
                    gunLocked = false;
            }
        }

        public static void SnowballFlingVerticalAll()
        {
            if (rightTrigger > 0.5f && Time.time > snowballDelay)
            {
                snowballDelay = Time.time + SnowballSpawnDelay;

                Player plr = NetPlayerToPlayer(GetPlayerFromVRRig(GetTargetPlayer(SnowballSpawnDelay)));
                BetaSpawnSnowball(GetVRRigFromPlayer(plr).transform.position + new Vector3(0f, -0.7f, 0f), new Vector3(0f, -500f, 0f), 2, plr);
            }
        }

        public static void SnowballFlingTowardsGun()
        {
            if (GetGunInput(false))
            {
                var GunData = RenderGun();
                GameObject NewPointer = GunData.NewPointer;

                if (GetGunInput(true) && Time.time > snowballDelay)
                {
                    snowballDelay = Time.time + SnowballSpawnDelay;
                    Player plr = NetPlayerToPlayer(GetPlayerFromVRRig(GetTargetPlayer(0.5f)));
                    Vector3 targetDirection = (NewPointer.transform.position - GetVRRigFromPlayer(plr).headMesh.transform.position).normalized;
                    BetaSpawnSnowball(GetVRRigFromPlayer(plr).transform.position + new Vector3(0f, 0.5f, 0f) + new Vector3(-targetDirection.x, 0f, -targetDirection.z) / 1.7f, new Vector3(0f, -500f, 0f), 2, plr);
                }
            }
        }

        public static void SnowballFlingAwayGun()
        {
            if (GetGunInput(false))
            {
                var GunData = RenderGun();
                GameObject NewPointer = GunData.NewPointer;

                if (GetGunInput(true) && Time.time > snowballDelay)
                {
                    BetaSpawnSnowball(NewPointer.transform.position + new Vector3(0f, 0.1f, 0f), new Vector3(0f, -500f, 0f), 1);
                    snowballDelay = Time.time + SnowballSpawnDelay;
                }
            }
        }

        public static void SnowballFlingPlayerTowardsGun()
        {
            if (GetGunInput(false))
            {
                var GunData = RenderGun();
                RaycastHit Ray = GunData.Ray;

                if (gunLocked && lockTarget != null)
                {
                    if (Time.time > snowballDelay)
                    {
                        Vector3 targetDirection = (lockTarget.headMesh.transform.position - GorillaTagger.Instance.headCollider.transform.position).normalized;
                        BetaSpawnSnowball(lockTarget.headMesh.transform.position + new Vector3(0f, 0.5f, 0f) + new Vector3(targetDirection.x, 0f, targetDirection.z) * 1.5f, new Vector3(0f, -100f, 0f), 2, NetPlayerToPlayer(GetPlayerFromVRRig(lockTarget)));
                        snowballDelay = Time.time + SnowballSpawnDelay;
                    }
                }
                if (GetGunInput(true))
                {
                    VRRig gunTarget = Ray.collider.GetComponentInParent<VRRig>();
                    if (gunTarget && !gunTarget.IsLocal())
                    {
                        gunLocked = true;
                        lockTarget = gunTarget;
                    }
                }
            }
            else
            {
                if (gunLocked)
                    gunLocked = false;
            }
        }

        public static void SnowballFlingPlayerAwayGun()
        {
            if (GetGunInput(false))
            {
                var GunData = RenderGun();
                RaycastHit Ray = GunData.Ray;

                if (gunLocked && lockTarget != null)
                {
                    if (Time.time > snowballDelay)
                    {
                        Vector3 targetDirection = (GorillaTagger.Instance.headCollider.transform.position - lockTarget.headMesh.transform.position).normalized;
                        BetaSpawnSnowball(lockTarget.headMesh.transform.position + new Vector3(0f, 0.5f, 0f) + new Vector3(targetDirection.x, 0f, targetDirection.z) * 1.5f, new Vector3(0f, -100f, 0f), 2, NetPlayerToPlayer(GetPlayerFromVRRig(lockTarget)));
                        snowballDelay = Time.time + SnowballSpawnDelay;
                    }
                }
                if (GetGunInput(true))
                {
                    VRRig gunTarget = Ray.collider.GetComponentInParent<VRRig>();
                    if (gunTarget && !gunTarget.IsLocal())
                    {
                        gunLocked = true;
                        lockTarget = gunTarget;
                    }
                }
            }
            else
            {
                if (gunLocked)
                    gunLocked = false;
            }
        }

        public static void SnowballPushGun()
        {
            if (GetGunInput(false))
            {
                var GunData = RenderGun();
                RaycastHit Ray = GunData.Ray;

                if (gunLocked && lockTarget != null)
                {
                    if (Time.time > snowballDelay)
                    {
                        Vector3 targetDirection = GorillaTagger.Instance.headCollider.transform.position - lockTarget.headMesh.transform.position;
                        BetaSpawnSnowball(GorillaTagger.Instance.headCollider.transform.position + new Vector3(0f, 0.5f, 0f) + new Vector3(targetDirection.x, 0f, targetDirection.z).normalized / 1.7f, new Vector3(0f, -500f, 0f), 2, NetPlayerToPlayer(GetPlayerFromVRRig(lockTarget)));
                        snowballDelay = Time.time + SnowballSpawnDelay;
                    }
                }
                if (GetGunInput(true))
                {
                    VRRig gunTarget = Ray.collider.GetComponentInParent<VRRig>();
                    if (gunTarget && !gunTarget.IsLocal())
                    {
                        gunLocked = true;
                        lockTarget = gunTarget;
                    }
                }
            }
            else
            {
                if (gunLocked)
                    gunLocked = false;
            }
        }

        public static void SnowballStrongFlingGun()
        {
            if (GetGunInput(false))
            {
                var GunData = RenderGun();
                RaycastHit Ray = GunData.Ray;

                if (gunLocked && lockTarget != null)
                {
                    if (Time.time > snowballDelay)
                    {
                        BetaSpawnSnowball(new Vector3(GorillaTagger.Instance.headCollider.transform.position.x, 1000f, GorillaTagger.Instance.headCollider.transform.position.z), new Vector3(0f, -9999f, 0f), 2, NetPlayerToPlayer(GetPlayerFromVRRig(lockTarget)));
                        snowballDelay = Time.time + SnowballSpawnDelay;
                    }
                }
                if (GetGunInput(true))
                {
                    VRRig gunTarget = Ray.collider.GetComponentInParent<VRRig>();
                    if (gunTarget && !gunTarget.IsLocal())
                    {
                        gunLocked = true;
                        lockTarget = gunTarget;
                    }
                }
            }
            else
            {
                if (gunLocked)
                    gunLocked = false;
            }
        }

        private static float antiReportFlingDelay;
        public static void AntiReportFling()
        {
            if (Time.time > antiReportFlingDelay)
            {
                Safety.AntiReport((vrrig, position) =>
                {
                    antiReportFlingDelay = Time.time + 0.1f;
                    BetaSetVelocityPlayer(GetPlayerFromVRRig(vrrig), (vrrig.transform.position - position) * 50f);
                    NotificationManager._v3_msg_("<color=grey>[</color><color=purple>ANTI-REPORT</color><color=grey>]</color> " + GetPlayerFromVRRig(vrrig).NickName + " attempted to report you, they have been flung.");
                });
            }
        }

        public static void AntiReportSnowballFling()
        {
            if (Time.time > snowballDelay)
            {
                Safety.AntiReport((vrrig, position) =>
                {
                    snowballDelay = Time.time + SnowballSpawnDelay;
                    BetaSpawnSnowball(position, new Vector3(0f, -500f, 0f), 2, NetPlayerToPlayer(GetPlayerFromVRRig(vrrig)));
                    NotificationManager._v3_msg_("<color=grey>[</color><color=purple>ANTI-REPORT</color><color=grey>]</color> " + GetPlayerFromVRRig(vrrig).NickName + " attempted to report you, they have been flung.");
                });
            }
        }

        public static bool SpecialTargetRPC(PhotonView photonView, string method, RaiseEventOptions options, params object[] parameters)
        {
            if (photonView != null && parameters != null && !string.IsNullOrEmpty(method))
            {
                Hashtable rpcData = new Hashtable
                {
                    { 0, photonView.ViewID },
                    { 2, PhotonNetwork.ServerTimestamp },
                    { 3, method },
                    { 4, parameters }
                };

                if (photonView.Prefix > 0)
                    rpcData[1] = (short)photonView.Prefix;

                if (PhotonNetwork.PhotonServerSettings.RpcList.Contains(method))
                    rpcData[5] = (byte)PhotonNetwork.PhotonServerSettings.RpcList.IndexOf(method);

                if (options.Receivers == ReceiverGroup.All || (options.TargetActors != null && options.TargetActors.Contains(NetworkSystem.Instance.LocalPlayer.ActorNumber)))
                {
                    if (options.Receivers == ReceiverGroup.All)
                        options.Receivers = ReceiverGroup.Others;

                    if (options.TargetActors != null && options.TargetActors.Contains(NetworkSystem.Instance.LocalPlayer.ActorNumber))
                        options.TargetActors = options.TargetActors.Where(id => id != NetworkSystem.Instance.LocalPlayer.ActorNumber).ToArray();

                    PhotonNetwork.ExecuteRpc(rpcData, PhotonNetwork.LocalPlayer);
                }

                else
                {
                    PhotonNetwork.NetworkingClient.LoadBalancingPeer.OpRaiseEvent(200, rpcData, options, new SendOptions
                    {
                        Reliability = true,
                        DeliveryMode = DeliveryMode.ReliableUnsequenced,
                        Encrypt = false
                    });
                }
            }
            return false;
        }

        public static bool SpecialTimeRPC(PhotonView photonView, int timeOffset, string method, RaiseEventOptions options, params object[] parameters)
        {
            if (photonView != null && parameters != null && !string.IsNullOrEmpty(method))
            {
                Hashtable rpcData = new Hashtable
                {
                    { 0, photonView.ViewID },
                    { 2, PhotonNetwork.ServerTimestamp + timeOffset },
                    { 3, method },
                    { 4, parameters }
                };

                if (photonView.Prefix > 0)
                    rpcData[1] = (short)photonView.Prefix;

                if (PhotonNetwork.PhotonServerSettings.RpcList.Contains(method))
                    rpcData[5] = (byte)PhotonNetwork.PhotonServerSettings.RpcList.IndexOf(method);

                if (options.Receivers == ReceiverGroup.All || (options.TargetActors != null && options.TargetActors.Contains(NetworkSystem.Instance.LocalPlayer.ActorNumber)))
                {
                    if (options.Receivers == ReceiverGroup.All)
                        options.Receivers = ReceiverGroup.Others;

                    if (options.TargetActors != null && options.TargetActors.Contains(NetworkSystem.Instance.LocalPlayer.ActorNumber))
                        options.TargetActors = options.TargetActors.Where(id => id != NetworkSystem.Instance.LocalPlayer.ActorNumber).ToArray();

                    PhotonNetwork.ExecuteRpc(rpcData, PhotonNetwork.LocalPlayer);
                }

                else
                {
                    PhotonNetwork.NetworkingClient.LoadBalancingPeer.OpRaiseEvent(200, rpcData, options, new SendOptions
                    {
                        Reliability = true,
                        DeliveryMode = DeliveryMode.ReliableUnsequenced,
                        Encrypt = false
                    });
                }
            }
            return false;
        }

        public static void PhysicalFreezeGun()
        {
            if (GetGunInput(false))
            {
                var GunData = RenderGun();
                RaycastHit Ray = GunData.Ray;

                if (gunLocked && lockTarget != null)
                {
                    if (Time.time > flingDelay)
                    {
                        BetaSetVelocityPlayer(GetPlayerFromVRRig(lockTarget), Vector3.zero);
                        RPCProtection();
                        flingDelay = Time.time + 0.1f;
                    }
                }
                if (GetGunInput(true))
                {
                    VRRig gunTarget = Ray.collider.GetComponentInParent<VRRig>();
                    if (gunTarget && !gunTarget.IsLocal())
                    {
                        gunLocked = true;
                        lockTarget = gunTarget;
                    }
                }
            }
            else
            {
                if (gunLocked)
                    gunLocked = false;
            }
        }

        public static void PhysicalFreezeAll()
        {
            if (rightTrigger > 0.5f && Time.time > flingDelay)
            {
                flingDelay = Time.time + 0.1f;
                BetaSetVelocityTargetGroup(RpcTarget.Others, Vector3.zero);
                RPCProtection();
            }
        }

        public static void BringPlayer(NetPlayer player)
        {
            if (Time.time > flingDelay)
            {
                BetaSetVelocityPlayer(player, (GorillaTagger.Instance.bodyCollider.transform.position - GetVRRigFromPlayer(player).transform.position).normalized * 20f);
                RPCProtection();
                flingDelay = Time.time + 0.1f;
            }
        }

        public static void BringPlayerGun(NetPlayer player)
        {
            if (GetGunInput(false))
            {
                var GunData = RenderGun();
                GameObject NewPointer = GunData.NewPointer;

                if (GetGunInput(true))
                {
                    if (Time.time > flingDelay)
                    {
                        BetaSetVelocityPlayer(player, Vector3.Normalize(NewPointer.transform.position - GetVRRigFromPlayer(player).transform.position) * 50f);
                        RPCProtection();
                        flingDelay = Time.time + 0.2f;
                    }
                }
            }
        }

        public static void BringGun()
        {
            if (GetGunInput(false))
            {
                var GunData = RenderGun();
                RaycastHit Ray = GunData.Ray;

                if (gunLocked && lockTarget != null)
                {
                    if (Time.time > flingDelay)
                    {
                        BetaSetVelocityPlayer(GetPlayerFromVRRig(lockTarget), (GorillaTagger.Instance.bodyCollider.transform.position - lockTarget.transform.position).normalized * 20f);
                        RPCProtection();
                        flingDelay = Time.time + 0.1f;
                    }
                }
                if (GetGunInput(true))
                {
                    VRRig gunTarget = Ray.collider.GetComponentInParent<VRRig>();
                    if (gunTarget && !gunTarget.IsLocal())
                    {
                        gunLocked = true;
                        lockTarget = gunTarget;
                    }
                }
            }
            else
            {
                if (gunLocked)
                    gunLocked = false;
            }
        }

        public static void BringAll()
        {
            if (rightTrigger > 0.5f && Time.time > flingDelay)
            {
                flingDelay = Time.time + 0.2f;
                foreach (var plr in VRRigCache.ActiveRigs.Where(plr => !plr.isLocal))
                {
                    BetaSetVelocityPlayer(GetPlayerFromVRRig(plr), (GorillaTagger.Instance.bodyCollider.transform.position - plr.transform.position).normalized * 20f);
                    RPCProtection();
                }
            }
        }

        public static void BringAwayGun()
        {
            if (GetGunInput(false))
            {
                var GunData = RenderGun();
                RaycastHit Ray = GunData.Ray;

                if (gunLocked && lockTarget != null)
                {
                    if (Time.time > flingDelay)
                    {
                        BetaSetVelocityPlayer(GetPlayerFromVRRig(lockTarget), (lockTarget.transform.position - GorillaTagger.Instance.bodyCollider.transform.position).normalized * 20f);
                        RPCProtection();
                        flingDelay = Time.time + 0.1f;
                    }
                }
                if (GetGunInput(true))
                {
                    VRRig gunTarget = Ray.collider.GetComponentInParent<VRRig>();
                    if (gunTarget && !gunTarget.IsLocal())
                    {
                        gunLocked = true;
                        lockTarget = gunTarget;
                    }
                }
            }
            else
            {
                if (gunLocked)
                    gunLocked = false;
            }
        }

        public static void BringAwayAll()
        {
            if (rightTrigger > 0.5f && Time.time > flingDelay)
            {
                flingDelay = Time.time + 0.2f;
                foreach (var plr in VRRigCache.ActiveRigs.Where(plr => !plr.isLocal))
                {
                    BetaSetVelocityPlayer(GetPlayerFromVRRig(plr), (plr.transform.position - GorillaTagger.Instance.bodyCollider.transform.position).normalized * 20f);
                    RPCProtection();
                }
            }
        }

        public static void OrbitAll()
        {
            float scale = 5f;
            if (rightTrigger > 0.5f && Time.time > flingDelay)
            {
                flingDelay = Time.time + 0.2f;
                int index = 0;

                VRRig[] rigs = VRRigCache.ActiveRigs.Where(rig => !rig.isLocal).ToArray();
                foreach (VRRig rig in rigs)
                {
                    float offset = 360f / rigs.Length * index;
                    Vector3 targetPosition = GorillaTagger.Instance.headCollider.transform.position + new Vector3(MathF.Cos(offset + Time.time) * scale, 2, MathF.Sin(offset + Time.time) * scale);

                    BetaSetVelocityPlayer(GetPlayerFromVRRig(rig), (targetPosition - rig.transform.position) * 1f);
                    RPCProtection();
                    index++;
                }
            }
        }

        private static float thingdeb;
        public static void GiveFlyGun()
        {
            if (GetGunInput(false))
            {
                var GunData = RenderGun();
                RaycastHit Ray = GunData.Ray;

                if (gunLocked && lockTarget != null)
                {
                    if (Time.time > thingdeb)
                    {
                        if (lockTarget.rightThumb.calcT > 0.5f)
                        {
                            BetaSetVelocityPlayer(GetPlayerFromVRRig(lockTarget), lockTarget.headMesh.transform.forward * Movement._flySpeed);
                            RPCProtection();
                        }
                        thingdeb = Time.time + 0.1f;
                    }
                }
                if (GetGunInput(true))
                {
                    VRRig gunTarget = Ray.collider.GetComponentInParent<VRRig>();
                    if (gunTarget && !gunTarget.IsLocal())
                    {
                        gunLocked = true;
                        lockTarget = gunTarget;
                    }
                }
            }
            else
            {
                if (gunLocked)
                    gunLocked = false;
            }
        }

        public static void GiveFlyAll()
        {
            if (Time.time > thingdeb)
            {
                thingdeb = Time.time + 0.1f;
                foreach (var plr in VRRigCache.ActiveRigs.Where(plr => !plr.isLocal).Where(plr => plr.rightThumb.calcT > 0.5f))
                {
                    BetaSetVelocityPlayer(GetPlayerFromVRRig(plr), plr.headMesh.transform.forward * Movement._flySpeed);
                    RPCProtection();
                }
            }
        }

        public static void PunchMod()
        {
            if (Time.time > thingdeb)
            {
                foreach (VRRig rig in VRRigCache.ActiveRigs)
                {
                    bool leftHand = Vector3.Distance(GorillaTagger.Instance.leftHandTransform.position, rig.headMesh.transform.position) < 0.25f;
                    bool rightHand = Vector3.Distance(GorillaTagger.Instance.rightHandTransform.position, rig.headMesh.transform.position) < 0.25f;

                    if (!rig.isLocal && (leftHand || rightHand))
                    {
                        Vector3 vel = rightHand ? GorillaLocomotion.GTPlayer.Instance.RightHand.velocityTracker.GetAverageVelocity(true, 0) : GorillaLocomotion.GTPlayer.Instance.LeftHand.velocityTracker.GetAverageVelocity(true, 0);

                        BetaSetVelocityPlayer(GetPlayerFromVRRig(rig), vel);
                        thingdeb = Time.time + 0.1f;

                        if (Buttons.GetIndex("Graphic Punch Mod").enabled)
                            Projectiles.BetaFireProjectile("AppleLeftAnchor", rig.headMesh.transform.position, Vector3.down * 600f, new Color32(100, 0, 0, 255));
                    }
                }
            }
        }

        private static float GetBoxingDelay(VRRig rig) =>
            boxingDelay.GetValueOrDefault(rig, -1);

        private static void SetBoxingDelay(VRRig rig)
        {
            boxingDelay.Remove(rig);

            boxingDelay.Add(rig, SnowballSpawnDelay);
        }

        public static void Boxing()
        {
            foreach (VRRig rig1 in VRRigCache.ActiveRigs)
            {
                if (Time.time < GetBoxingDelay(rig1))
                    continue;

                foreach (var targetDirection in from rig2 in VRRigCache.ActiveRigs where rig2 != rig1 where Vector3.Distance(rig2.leftHandTransform.position, rig1.headMesh.transform.position) < 0.25f || Vector3.Distance(rig2.rightHandTransform.position, rig1.headMesh.transform.position) < 0.25f select (rig1.headMesh.transform.position - rig2.headMesh.transform.position) * 20f)
                {
                    BetaSetVelocityPlayer(GetPlayerFromVRRig(rig1), targetDirection);
                    SetBoxingDelay(rig1);
                }
            }
        }

        public static void BringAllGun()
        {
            if (GetGunInput(false))
            {
                var GunData = RenderGun();
                GameObject NewPointer = GunData.NewPointer;

                if (GetGunInput(true))
                {
                    if (Time.time > flingDelay)
                    {
                        foreach (var plr in VRRigCache.ActiveRigs.Where(plr => !plr.isLocal))
                            BetaSetVelocityPlayer(GetPlayerFromVRRig(plr), Vector3.Normalize(NewPointer.transform.position - plr.transform.position) * 50f);

                        RPCProtection();
                        flingDelay = Time.time + 0.2f;
                    }
                }
            }
        }

        public static void BringAwayAllGun()
        {
            if (GetGunInput(false))
            {
                var GunData = RenderGun();
                GameObject NewPointer = GunData.NewPointer;

                if (GetGunInput(true))
                {
                    if (Time.time > flingDelay)
                    {
                        foreach (var plr in VRRigCache.ActiveRigs.Where(plr => !plr.isLocal))
                            BetaSetVelocityPlayer(GetPlayerFromVRRig(plr), Vector3.Normalize(plr.transform.position - NewPointer.transform.position) * 50f);

                        RPCProtection();
                        flingDelay = Time.time + 0.2f;
                    }
                }
            }
        }

        public static void AntiStump()
        {
            if (Time.time > flingDelay)
            {
                foreach (VRRig rig in VRRigCache.ActiveRigs)
                {
                    if (!rig.isLocal)
                    {
                        Vector3 stump = new Vector3(-66f, 12f, -79f);
                        if (Vector3.Distance(stump, rig.transform.position) < 3f)
                        {
                            BetaSetVelocityPlayer(GetPlayerFromVRRig(rig), (rig.transform.position - stump).normalized * 20f);
                            flingDelay = Time.time + 0.2f;
                        }
                    }
                }
            }
        }

        private static float slamDel;
        private static bool flip;
        public static void EffectSpamHands()
        {
            if (rightGrab)
            {
                if (Time.time > slamDel)
                {
                    GorillaGuardianManager gman = (GorillaGuardianManager)GorillaGameManager.instance;
                    if (gman.IsPlayerGuardian(NetworkSystem.Instance.LocalPlayer))
                    {
                        GameMode.ActiveNetworkHandler.NetView.GetView.RPC(flip ? "ShowSlamEffect" : "ShowSlapEffects", RpcTarget.All, GorillaTagger.Instance.rightHandTransform.position, new Vector3(Random.Range(0, 360), Random.Range(0, 360), Random.Range(0, 360)));
                        RPCProtection();
                        flip = !flip;
                    }
                    else
                        NotificationManager._v3_msg_("<color=grey>[</color><color=red>ERROR</color><color=grey>]</color> You must be guardian.");

                    slamDel = Time.time + 0.05f;
                }
            }
            if (leftGrab)
            {
                if (Time.time > slamDel)
                {
                    GorillaGuardianManager gman = (GorillaGuardianManager)GorillaGameManager.instance;
                    if (gman.IsPlayerGuardian(NetworkSystem.Instance.LocalPlayer))
                    {
                        GameMode.ActiveNetworkHandler.NetView.GetView.RPC(flip ? "ShowSlamEffect" : "ShowSlapEffects", RpcTarget.All, GorillaTagger.Instance.leftHandTransform.position, new Vector3(Random.Range(0, 360), Random.Range(0, 360), Random.Range(0, 360)));
                        RPCProtection();
                        flip = !flip;
                    }
                    else
                        NotificationManager._v3_msg_("<color=grey>[</color><color=red>ERROR</color><color=grey>]</color> You must be guardian.");

                    slamDel = Time.time + 0.05f;
                }
            }
        }

        public static void EffectSpamGun()
        {
            if (GetGunInput(false))
            {
                var GunData = RenderGun();
                GameObject NewPointer = GunData.NewPointer;

                if (GetGunInput(true))
                {
                    GorillaGuardianManager gman = (GorillaGuardianManager)GorillaGameManager.instance;
                    if (Time.time > slamDel)
                    {
                        if (gman.IsPlayerGuardian(NetworkSystem.Instance.LocalPlayer))
                        {
                            GameMode.ActiveNetworkHandler.NetView.GetView.RPC(flip ? "ShowSlamEffect" : "ShowSlapEffects", RpcTarget.All, NewPointer.transform.position, new Vector3(Random.Range(0, 360), Random.Range(0, 360), Random.Range(0, 360)));
                            RPCProtection();
                            flip = !flip;
                        }
                        else
                            NotificationManager._v3_msg_("<color=grey>[</color><color=red>ERROR</color><color=grey>]</color> You must be guardian.");

                        slamDel = Time.time + 0.05f;
                    }

                }
            }
        }

        private static float freezeAllDelay;
        private static int[] freezePool = null;
        private static int freezeIdx = 0;
        public static bool muteOnFreeze;

        public static Coroutine freezeCoroutine;
        public static void FreezeServer(float delay = 0.05f, int eventCount = 150, RaiseEventOptions options = null)
        {
            if (!PhotonNetwork.InRoom) return;

            // If called from the main menu (defaults), we use the optimized coroutine
            if (options == null && eventCount == 150)
            {
                if (freezeCoroutine == null)
                {
                    freezeCoroutine = CoroutineManager.instance.StartCoroutine(FreezeServerBurst());
                }
                return;
            }

            // Fallback for one-off bursts (Kick Guns, etc.)
            for (int i = 0; i < eventCount; i++)
            {
                PhotonNetwork.NetworkingClient.OpRaiseEvent(202, new Hashtable { { 8, new byte[256] } }, new RaiseEventOptions { Receivers = ReceiverGroup.MasterClient }, SendOptions.SendUnreliable);
            }
        }

        public static void DisableFreezeServer()
        {
            if (freezeCoroutine != null)
            {
                CoroutineManager.instance.StopCoroutine(freezeCoroutine);
                freezeCoroutine = null;
            }
            freezeAllDelay = 0;
        }

        private static IEnumerator FreezeServerBurst()
        {
            RaiseEventOptions others = new RaiseEventOptions { Receivers = ReceiverGroup.Others, CachingOption = EventCaching.DoNotCache };
            RaiseEventOptions master = new RaiseEventOptions { Receivers = ReceiverGroup.MasterClient, CachingOption = EventCaching.DoNotCache };

            while (true)
            {
                if (!PhotonNetwork.InRoom) yield break;

                // NETWORK BUFFER GUARD: Prevent self-kick by ensuring the outgoing queue isn't overwhelmed
                if (PhotonNetwork.NetworkingClient.LoadBalancingPeer.QueuedOutgoingCommands > 100)
                {
                    yield return new WaitForSeconds(0.01f);
                    continue;
                }

                // master lag loop
                if (freezePool == null)
                {
                    freezePool = new int[10];
                    for (int i = 0; i < 10; i++)
                        freezePool[i] = PhotonNetwork.AllocateViewID(0);
                }

                int v = freezePool[freezeIdx % 10];
                freezeIdx++;

                // Randomized high-pressure payload
                Hashtable h = new Hashtable
                {
                    { 0, "GameMode" },
                    { 1, "QuantumPerfectFreeze" },
                    { 6, PhotonNetwork.ServerTimestamp },
                    { 7, v },
                    { 8, new byte[512] }, // Heavy payload
                    { 99, Random.value } // Anti-coalescing ID
                };

                object d = new object[] { 999950f + Random.value * 50f, Random.value };
                object[] ev = { PhotonNetwork.ServerTimestamp, (byte)3, new object[] { 0, 150f + Random.value, false } };

                // Distributed flood burst (10 per step, but steps are very fast)
                for (int i = 0; i < 15; i++)
                {
                    PhotonNetwork.NetworkingClient.OpRaiseEvent(202, h, master, SendOptions.SendUnreliable);
                    PhotonNetwork.NetworkingClient.OpRaiseEvent(204, d, others, SendOptions.SendUnreliable);
                    PhotonNetwork.RaiseEvent(3, ev, others, SendOptions.SendUnreliable);
                    PhotonNetwork.RaiseEvent(200, h, others, SendOptions.SendUnreliable); // Heavy move event
                }

                yield return new WaitForSeconds(0.005f); // 200 cycles per second = 3000 events/sec SUSTAINED
            }
        }

        private static float closeRoomDelay;
        public static void CloseRoom()
        {
            if (!PhotonNetwork.InRoom) return;
            if (Time.time < closeRoomDelay) return;

            closeRoomDelay = Time.time + 0.1f;

            // load pool
            if (freezePool == null)
            {
                freezePool = new int[10];
                for (int i = 0; i < 10; i++)
                    freezePool[i] = PhotonNetwork.AllocateViewID(0);
            }

            int v = freezePool[freezeIdx % 10];
            freezeIdx++;

            Hashtable h = new Hashtable
            {
                { 0, "GameMode" },
                { 6, PhotonNetwork.ServerTimestamp },
                { 7, v }
            };

            object d = new object[] { 999950f + Random.value * 50f };
            object[] s2 = { 0, 150f + Random.value, false };
            object[] ev = { PhotonNetwork.ServerTimestamp, (byte)3, s2 };

            for (int i = 0; i < 75; i++)
            {
                RaiseEventOptions op = new RaiseEventOptions
                {
                    Flags = new WebFlags(byte.MaxValue),
                    Receivers = ReceiverGroup.Others,
                    CachingOption = EventCaching.AddToRoomCacheGlobal
                };
                
                PhotonNetwork.NetworkingClient.OpRaiseEvent(202, h, op, SendOptions.SendReliable);
                PhotonNetwork.NetworkingClient.OpRaiseEvent(204, d, op, SendOptions.SendReliable);
                PhotonNetwork.RaiseEvent(3, ev, op, SendOptions.SendReliable);
                RPCProtection();
            }
        }

        public static float zaWarudoNotificationDelay;

        public static Coroutine ZaWarudo_StartCoroutineVariable;
        public static Coroutine ZaWarudo_EndCoroutineVariable;

        public static AudioClip ZaWarudo_Start;
        public static AudioClip ZaWarudo_Stop;

        public static void ZaWarudo_enableMethod()
        {
            LoadSoundFromURL($"{PluginInfo.ServerResourcePath}/Audio/Mods/Overpowered/Timestop/start.ogg", "Audio/Mods/Overpowered/Timestop/start.ogg", clip => ZaWarudo_Start = clip);
            LoadSoundFromURL($"{PluginInfo.ServerResourcePath}/Audio/Mods/Overpowered/Timestop/end.ogg", "Audio/Mods/Overpowered/Timestop/end.ogg", clip => ZaWarudo_Stop = clip);
        }

        private static bool zaWarudoTrigger;
        public static void ZaWarudo()
        {
            if (!PhotonNetwork.InRoom) return;

            if (rightTrigger > 0.5f)
            {
                if (Buttons.GetIndex("No Freeze Za Warudo").enabled)
                {
                    if (!Buttons.GetIndex("No Freeze Za Warudo").enabled)
                        SerializePatch.OverrideSerialization = () => false;

                    if (!zaWarudoTrigger)
                    {
                        if (ZaWarudo_StartCoroutineVariable != null)
                        {
                            CoroutineManager.instance.StopCoroutine(ZaWarudo_StartCoroutineVariable);
                            ZaWarudo_StartCoroutineVariable = null;
                        }

                        if (ZaWarudo_EndCoroutineVariable != null)
                        {
                            CoroutineManager.instance.StopCoroutine(ZaWarudo_StartCoroutineVariable);
                            ZaWarudo_EndCoroutineVariable = null;
                        }

                        ZaWarudo_StartCoroutineVariable = CoroutineManager.instance.StartCoroutine(ZaWarudo_StartCoroutine());
                    }

                    zaWarudoTrigger = true;

                    Movement.LowGravity();

                    if (!Buttons.GetIndex("No Freeze Za Warudo").enabled)
                        FreezeServer();
                }
            }
            else
            {
                if (zaWarudoTrigger)
                {
                    if (ZaWarudo_StartCoroutineVariable != null)
                    {
                        CoroutineManager.instance.StopCoroutine(ZaWarudo_StartCoroutineVariable);
                        ZaWarudo_StartCoroutineVariable = null;
                    }

                    if (ZaWarudo_EndCoroutineVariable != null)
                    {
                        CoroutineManager.instance.StopCoroutine(ZaWarudo_EndCoroutineVariable);
                        ZaWarudo_EndCoroutineVariable = null;
                    }

                    ZaWarudo_EndCoroutineVariable = CoroutineManager.instance.StartCoroutine(ZaWarudo_StopCoroutine());
                }

                SerializePatch.OverrideSerialization = null;
                zaWarudoTrigger = false;
            }
        }

        public static IEnumerator ZaWarudo_StartCoroutine()
        {
            Sound.PlayAudio(ZaWarudo_Start);
            yield return new WaitForSeconds(2.4f);

            float endWhiteFadeTime = Time.time;
            Vector3 originPoint = GorillaTagger.Instance.bodyCollider.transform.position;

            while (Time.time < endWhiteFadeTime + 0.2f)
            {
                float t = (Time.time - endWhiteFadeTime) / 0.2f;
                Fun.HueShift(Color.Lerp(Color.clear, Color.white, t));

                TeleportPlayer(originPoint + RandomVector3(t * 0.2f));

                yield return null;
            }

            float purpleFadeTime = Time.time;

            while (Time.time < purpleFadeTime + 2f)
            {
                float t = (Time.time - purpleFadeTime) / 2f;
                Fun.HueShift(Color.Lerp(Color.white, new Color32(120, 47, 196, 100), t));

                TeleportPlayer(originPoint + RandomVector3((1 - t) * 0.2f));

                yield return null;
            }

            TeleportPlayer(originPoint);

            Fun.HueShift(new Color32(120, 47, 196, 100));

            ZaWarudo_StartCoroutineVariable = null;
        }

        public static IEnumerator ZaWarudo_StopCoroutine()
        {
            Sound.PlayAudio(ZaWarudo_Stop);
            yield return new WaitForSeconds(0.5f);

            float purpleFadeTime = Time.time;

            while (Time.time < purpleFadeTime + 1f)
            {
                float t = Time.time - purpleFadeTime;
                Fun.HueShift(Color.Lerp(new Color32(120, 47, 196, 100), new Color32(120, 47, 196, 0), t));

                yield return null;
            }

            Fun.HueShift(Color.clear);

            ZaWarudo_EndCoroutineVariable = null;
        }

        public static int lagIndex = 1;
        public static int lagAmount;
        public static float lagDelay;
        public static void ChangeLagPower(bool positive = true)
        {
            if (positive)
                lagIndex++;
            else
                lagIndex--;

            lagIndex %= 3;
            if (lagIndex < 0)
                lagIndex = 2;

            lagAmount = new[] { 40, 113, 425, 1000, 3800 }[lagIndex];
            lagDelay = new[] { 0.1f, 0.25f, 1f, 3f, 8f }[lagIndex];

            Buttons.GetIndex("Change Lag Power").overlapText = "Change Lag Power <color=grey>[</color><color=green>" + new[] { "Light", "Heavy", "Spike", "Stutter", "Freeze" }[lagIndex] + "</color><color=grey>]</color>";
        }

        public static int lagTypeIndex;
        public static void ChangeLagType(bool positive = true)
        {
            string[] lagNames = {
                "Destroy"
            };

            if (positive)
                lagTypeIndex++;
            else
                lagTypeIndex--;

            lagTypeIndex %= lagNames.Length;
            if (lagTypeIndex < 0)
                lagTypeIndex = lagNames.Length - 1;

            Buttons.GetIndex("Change Lag Type").overlapText = "Change Lag Type <color=grey>[</color><color=green>" + lagNames[lagTypeIndex] + "</color><color=grey>]</color>";
        }

        public static bool IsLagMethodRPC()
        {
            return lagTypeIndex switch
            {
                0 => true,
                1 => false,

                _ => true
            };
        }

        private static float lagDebounce;

        public static void LagTarget(object target)
        {
            if (!PhotonNetwork.InRoom) return;
            if (Time.time < lagDebounce) return;

            if (target is VRRig rig) target = rig.GetPhotonPlayer();
            if (target is NetPlayer legacyNetPlayer) target = legacyNetPlayer.GetPlayer();

            lagDebounce = Time.time + lagDelay;

            byte eventIndex = 204;
            object data = new object[] { float.NaN };
            SendOptions sendOptions = new SendOptions
            {
                Reliability = false,
                DeliveryMode = DeliveryMode.Unreliable
            };
            RaiseEventOptions raiseEventOptions = new RaiseEventOptions
            {
                CachingOption = EventCaching.DoNotCache
            };

            switch (target)
            {
                case RpcTarget rpcTarget:
                    raiseEventOptions.Receivers =
                        rpcTarget == RpcTarget.All ? ReceiverGroup.All :
                        rpcTarget == RpcTarget.MasterClient ? ReceiverGroup.MasterClient :
                        ReceiverGroup.Others;
                    break;

                case Player player:
                    raiseEventOptions.TargetActors = new[] { player.ActorNumber };
                    break;

                case int[] actorNumbers:
                    raiseEventOptions.TargetActors = actorNumbers;
                    break;
            }

            for (int i = 0; i < lagAmount; i++)
                PhotonNetwork.NetworkingClient.OpRaiseEvent(eventIndex, data, raiseEventOptions, sendOptions);

            RPCProtection();
        }

        public static void LagGun()
        {
            if (GetGunInput(false))
            {
                var GunData = RenderGun();
                RaycastHit Ray = GunData.Ray;

                if (gunLocked && lockTarget != null)
                    LagTarget(lockTarget.GetPlayer());

                if (GetGunInput(true))
                {
                    VRRig gunTarget = Ray.collider.GetComponentInParent<VRRig>();
                    if (gunTarget && !gunTarget.IsLocal())
                    {
                        gunLocked = true;
                        lockTarget = gunTarget;
                    }
                }
            }
            else
            {
                if (gunLocked)
                    gunLocked = false;
            }
        }

        public static void LagAll() =>
            LagTarget(RpcTarget.Others);

        public static void LagAura()
        {
            if (!PhotonNetwork.InRoom) return;
            List<int> nearbyPlayers = new List<int>();

            foreach (VRRig vrrig in VRRigCache.ActiveRigs)
            {
                if (Vector3.Distance(vrrig.transform.position, VRRig.LocalRig.transform.position) < 4 && !vrrig.IsTagged())
                    nearbyPlayers.Add(GetPlayerFromVRRig(vrrig).ActorNumber);
                else if (nearbyPlayers.Contains(GetPlayerFromVRRig(vrrig).ActorNumber))
                    nearbyPlayers.Remove(GetPlayerFromVRRig(vrrig).ActorNumber);
            }

            if (nearbyPlayers.Count > 0)
                LagTarget(nearbyPlayers);
        }

        public static void LagOnTouch()
        {
            if (!PhotonNetwork.InRoom) return;

            List<int> touchedPlayers = new List<int>();

            foreach (VRRig rig in VRRigCache.ActiveRigs.Where(rig => !rig.IsLocal()))
            {
                if (Vector3.Distance(rig.transform.position, VRRig.LocalRig.rightHandTransform.position) <= 0.35f ||
                    Vector3.Distance(rig.transform.position, VRRig.LocalRig.leftHandTransform.position) <= 0.35f)
                    touchedPlayers.Add(GetPlayerFromVRRig(rig).ActorNumber);
            }

            if (touchedPlayers.Count > 0)
                LagTarget(touchedPlayers);
        }

        public static void MuteTarget(object target)
        {
            RaiseEventOptions raiseOptions = new RaiseEventOptions();

            if (target is ReceiverGroup group)
                raiseOptions.Receivers = group;
            else if (target is int[] actors)
                raiseOptions.TargetActors = actors;
            else if (target is RaiseEventOptions options)
                raiseOptions = options;
            else
                return;

            SendOptions sendOptions = new SendOptions
            {
                Reliability = false,
                Channel = 0
            };


            Dictionary<byte, object> voiceData = new Dictionary<byte, object>
            {
                { 1, 255 },
                { 2, 48000 },
                { 3, 2 },
                { 4, 20000 },
                { 5, 30000 },
                { 10, null },
                { 11, (byte)0 },
                { 12, Codec.AudioOpus }
            };

            object[] eventData =
            {
                (byte)0,
                (byte)1,
                new object[] { voiceData }
            };

            PhotonVoiceNetwork.Instance.Client.OpRaiseEvent(
                202,
                eventData,
                raiseOptions,
                sendOptions
            );
        }

        public static void ServerMuteAll()
        {
            for (int i = 0; i < 2; i++)
                MuteTarget(ReceiverGroup.All);
        }

        public static void DeafenGun()
        {
            if (GetGunInput(false))
            {
                var GunData = RenderGun();
                RaycastHit Ray = GunData.Ray;

                if (gunLocked && lockTarget != null)
                {
                    for (int i = 0; i < 2; i++)
                        MuteTarget(new int[] { lockTarget.GetPlayer().ActorNumber });
                }


                if (GetGunInput(true))
                {
                    VRRig gunTarget = Ray.collider.GetComponentInParent<VRRig>();
                    if (gunTarget && !gunTarget.IsLocal())
                    {
                        gunLocked = true;
                        lockTarget = gunTarget;
                    }
                }
            }
            else
            {
                if (gunLocked)
                {
                    gunLocked = false;
                    VRRig.LocalRig.enabled = true;
                }
            }
        }

        public static void DeafenAll()
        {
            for (int i = 0; i < 2; i++)
                MuteTarget(ReceiverGroup.Others);
        }

        public static void BarrelFlingGun()
        {
            if (GetGunInput(false))
            {
                var GunData = RenderGun();
                RaycastHit Ray = GunData.Ray;

                if (gunLocked && lockTarget != null)
                    SendBarrelProjectile(lockTarget.transform.position, new Vector3(0f, 50f, 0f), Quaternion.identity, new RaiseEventOptions { TargetActors = new[] { GetPlayerFromVRRig(lockTarget).ActorNumber } });

                if (GetGunInput(true))
                {
                    VRRig gunTarget = Ray.collider.GetComponentInParent<VRRig>();
                    if (gunTarget && !gunTarget.IsLocal())
                    {
                        gunLocked = true;
                        lockTarget = gunTarget;
                    }
                }
            }
            else
            {
                if (gunLocked)
                {
                    gunLocked = false;
                    VRRig.LocalRig.enabled = true;
                }
            }
        }

        private static float barrelAllDelay;
        public static void BarrelFlingAll()
        {
            SerializePatch.OverrideSerialization = () => false;

            foreach (var TargetRig in VRRigCache.ActiveRigs.Where(TargetRig => !TargetRig.IsTagged()))
            {
                SendBarrelProjectile(TargetRig.transform.position, new Vector3(0f, 50f, 0f), Quaternion.identity, new RaiseEventOptions { TargetActors = new[] { GetPlayerFromVRRig(TargetRig).ActorNumber } });

                if (Time.time > barrelAllDelay)
                    throwableProjectileTimeout = 0f;
            }

            if (Time.time > barrelAllDelay)
                barrelAllDelay = Time.time + 0.3f;
        }

        public static void BarrelPunchMod()
        {
            foreach (VRRig rig in VRRigCache.ActiveRigs)
            {
                if (!rig.isLocal && (Vector3.Distance(GorillaTagger.Instance.leftHandTransform.position, rig.headMesh.transform.position) < 0.25f || Vector3.Distance(GorillaTagger.Instance.rightHandTransform.position, rig.headMesh.transform.position) < 0.25f))
                {
                    Vector3 targetDirection = rig.headMesh.transform.position - GorillaTagger.Instance.headCollider.transform.position;
                    SendBarrelProjectile(rig.transform.position + (GorillaTagger.Instance.headCollider.transform.position - rig.headMesh.transform.position).normalized * 0.1f, targetDirection.normalized * 50f, Quaternion.identity, new RaiseEventOptions { TargetActors = new[] { NetPlayerToPlayer(GetPlayerFromVRRig(rig)).ActorNumber } });

                    if (Buttons.GetIndex("Graphic Punch Mod").enabled)
                        Projectiles.BetaFireProjectile("AppleLeftAnchor", rig.headMesh.transform.position, Vector3.down * 600f, new Color32(100, 0, 0, 255));
                }
            }
        }

        public static void BarrelCrashGun()
        {
            if (GetGunInput(false))
            {
                var GunData = RenderGun();
                RaycastHit Ray = GunData.Ray;

                if (gunLocked && lockTarget != null)
                    SendBarrelProjectile(lockTarget.transform.position, new Vector3(0f, 5000f, 0f), Quaternion.identity, new RaiseEventOptions { TargetActors = new[] { GetPlayerFromVRRig(lockTarget).ActorNumber } });

                if (GetGunInput(true))
                {
                    VRRig gunTarget = Ray.collider.GetComponentInParent<VRRig>();
                    if (gunTarget && !gunTarget.IsLocal())
                    {
                        gunLocked = true;
                        lockTarget = gunTarget;
                    }
                }
            }
            else
            {
                if (gunLocked)
                {
                    gunLocked = false;
                    VRRig.LocalRig.enabled = true;
                }
            }
        }

        public static void BarrelCrashAll()
        {
            SerializePatch.OverrideSerialization = () => false;

            foreach (var TargetRig in VRRigCache.ActiveRigs.Where(TargetRig => !TargetRig.IsTagged()))
            {
                SendBarrelProjectile(TargetRig.transform.position, new Vector3(0f, 5000f, 0f), Quaternion.identity, new RaiseEventOptions { TargetActors = new[] { GetPlayerFromVRRig(TargetRig).ActorNumber } });
                if (Time.time > barrelAllDelay)
                    throwableProjectileTimeout = 0f;
            }

            if (Time.time > barrelAllDelay)
                barrelAllDelay = Time.time + 0.3f;
        }

        public const int BarrelIndex = 618;
        public static void SendBarrelProjectile(Vector3 pos, Vector3 vel, Quaternion rot, RaiseEventOptions options = null, bool disableCooldown = false)
        {
            options ??= new RaiseEventOptions { Receivers = ReceiverGroup.All };

            int index = BarrelIndex;
            DistancePatch.enabled = true;

            if (Fun.DisableThrowableCoroutine != null)
                CoroutineManager.instance.StopCoroutine(Fun.DisableThrowableCoroutine);

            Fun.DisableThrowableCoroutine = CoroutineManager.instance.StartCoroutine(Fun.DisableThrowable(index));
            TransferrableObject transferrableObject = VRRig.LocalRig.myBodyDockPositions.allObjects[index];

            if (!CosmeticsOwned.Contains(transferrableObject.gameObject.name))
            {
                VRRig.LocalRig.enabled = false;
                VRRig.LocalRig.transform.position = TryOnRoom.transform.position;
            }

            if (!transferrableObject.gameObject.activeSelf)
            {
                VRRig.LocalRig.SetActiveTransferrableObjectIndex(1, index);
                transferrableObject.gameObject.SetActive(true);
            }

            transferrableObject.storedZone = BodyDockPositions.DropPositions.RightArm;
            transferrableObject.currentState = TransferrableObject.PositionState.InRightHand;

            if (transferrableObject.gameObject.activeSelf && Time.time > throwableProjectileTimeout)
            {
                if (!disableCooldown)
                    throwableProjectileTimeout = Time.time + 0.3f;

                Vector3 archivePosition = VRRig.LocalRig.transform.position;

                VRRig.LocalRig.transform.position = pos;
                SendSerialize(GorillaTagger.Instance.myVRRig.GetView, options, -50);
                SendSerialize(GorillaTagger.Instance.myVRRig.GetView, options);

                vel = vel.ClampMagnitudeSafe(50f);

                DeployableObject barrel = transferrableObject.GetComponent<DeployableObject>();

                object[] data = {
                    barrel._deploySignal._signalID,
                    PhotonNetwork.ServerTimestamp,
                    BitPackUtils.PackWorldPosForNetwork(pos),
                    BitPackUtils.PackQuaternionForNetwork(rot),
                    BitPackUtils.PackWorldPosForNetwork(vel * 100f)
                };

                PhotonNetwork.RaiseEvent(177, data, options, SendOptions.SendReliable);

                VRRig.LocalRig.transform.position = archivePosition;
                SendSerialize(GorillaTagger.Instance.myVRRig.GetView, options);

                RPCProtection();
            }
        }

        public static void BarrelKickGun()
        {
            if (GetGunInput(false))
            {
                var GunData = RenderGun();
                RaycastHit Ray = GunData.Ray;

                if (gunLocked && lockTarget != null)
                {
                    Vector3 targetDirection = new Vector3(-71.33718f, 101.4977f, -93.09029f) - lockTarget.headMesh.transform.position;
                    SendBarrelProjectile(lockTarget.transform.position + (lockTarget.headMesh.transform.position - new Vector3(-71.33718f, 101.4977f, -93.09029f)).normalized * 0.1f, targetDirection.normalized * 50f, Quaternion.identity, new RaiseEventOptions { TargetActors = new[] { GetPlayerFromVRRig(lockTarget).ActorNumber } });
                }

                if (GetGunInput(true))
                {
                    VRRig gunTarget = Ray.collider.GetComponentInParent<VRRig>();
                    if (gunTarget && !gunTarget.IsLocal())
                    {
                        gunLocked = true;
                        lockTarget = gunTarget;
                    }
                }
            }
            else
            {
                if (gunLocked)
                {
                    gunLocked = false;
                    VRRig.LocalRig.enabled = true;
                }
            }
        }

        private static float throwableProjectileTimeout;
        public static void BarrelKickAll()
        {
            SerializePatch.OverrideSerialization = () => false;

            foreach (VRRig TargetRig in VRRigCache.ActiveRigs)
            {
                if (TargetRig.IsTagged()) continue;

                Vector3 targetDirection = new Vector3(-71.33718f, 101.4977f, -93.09029f) - TargetRig.headMesh.transform.position;
                SendBarrelProjectile(TargetRig.transform.position + (TargetRig.headMesh.transform.position - new Vector3(-71.33718f, 101.4977f, -93.09029f)).normalized * 0.1f, targetDirection.normalized * 50f, Quaternion.identity, new RaiseEventOptions { TargetActors = new[] { GetPlayerFromVRRig(TargetRig).ActorNumber } });

                if (Time.time > barrelAllDelay)
                    throwableProjectileTimeout = 0f;
            }

            if (Time.time > barrelAllDelay)
                barrelAllDelay = Time.time + 0.3f;
        }

        public static void BarrelFlingTowardsGun()
        {
            if (GetGunInput(false))
            {
                var GunData = RenderGun();
                RaycastHit Ray = GunData.Ray;

                if (gunLocked && lockTarget != null)
                    SendBarrelProjectile(lockTarget.transform.position + (GorillaTagger.Instance.headCollider.transform.position - lockTarget.headMesh.transform.position).normalized * 0.1f, (GorillaTagger.Instance.bodyCollider.transform.position - lockTarget.transform.position).normalized * 5000f, Quaternion.identity, new RaiseEventOptions { TargetActors = new[] { NetPlayerToPlayer(GetPlayerFromVRRig(lockTarget)).ActorNumber } });

                if (GetGunInput(true))
                {
                    VRRig gunTarget = Ray.collider.GetComponentInParent<VRRig>();
                    if (gunTarget && !gunTarget.IsLocal())
                    {
                        gunLocked = true;
                        lockTarget = gunTarget;
                    }
                }
            }
            else
            {
                if (gunLocked)
                {
                    gunLocked = false;
                    VRRig.LocalRig.enabled = true;
                }
            }
        }

        public static void BarrelFlingTowardsAll()
        {
            SerializePatch.OverrideSerialization = () => false;

            if (Time.time > throwableProjectileTimeout)
            {
                throwableProjectileTimeout = Time.time + 0.31f;
                foreach (var TargetRig in VRRigCache.ActiveRigs.Where(TargetRig => !TargetRig.IsTagged()))
                    SendBarrelProjectile(TargetRig.transform.position + (GorillaTagger.Instance.headCollider.transform.position - TargetRig.headMesh.transform.position).normalized * 0.1f, (GorillaTagger.Instance.bodyCollider.transform.position - TargetRig.transform.position).normalized * 5000f, Quaternion.identity, new RaiseEventOptions { TargetActors = new[] { NetPlayerToPlayer(GetPlayerFromVRRig(TargetRig)).ActorNumber } }, true);
            }
        }

        public static void CityKickGun()
        {
            if (GetGunInput(false))
            {
                var GunData = RenderGun();
                RaycastHit Ray = GunData.Ray;

                if (gunLocked && lockTarget != null)
                    SendBarrelProjectile(lockTarget.transform.position + (lockTarget.transform.position - new Vector3(-71.14215f, 13.73829f, -95.17883f)).normalized * 0.1f, (new Vector3(-71.14215f, 13.73829f, -95.17883f) - lockTarget.transform.position).normalized * 5000f, Quaternion.identity, new RaiseEventOptions { TargetActors = new[] { NetPlayerToPlayer(GetPlayerFromVRRig(lockTarget)).ActorNumber } });

                if (GetGunInput(true))
                {
                    VRRig gunTarget = Ray.collider.GetComponentInParent<VRRig>();
                    if (gunTarget && !gunTarget.IsLocal())
                    {
                        gunLocked = true;
                        lockTarget = gunTarget;
                    }
                }
            }
            else
            {
                if (gunLocked)
                {
                    gunLocked = false;
                    VRRig.LocalRig.enabled = true;
                }
            }
        }

        public static void CityKickAll()
        {
            SerializePatch.OverrideSerialization = () => false;

            foreach (VRRig TargetRig in VRRigCache.ActiveRigs)
            {
                if (TargetRig.IsTagged()) continue;

                SendBarrelProjectile(TargetRig.transform.position + (TargetRig.transform.position - new Vector3(-71.14215f, 13.73829f, -95.17883f)).normalized * 0.1f, (new Vector3(-71.14215f, 13.73829f, -95.17883f) - TargetRig.transform.position).normalized * 5000f, Quaternion.identity, new RaiseEventOptions { TargetActors = new[] { GetPlayerFromVRRig(TargetRig).ActorNumber } });

                if (Time.time > barrelAllDelay)
                    throwableProjectileTimeout = 0f;
            }

            if (Time.time > barrelAllDelay)
                barrelAllDelay = Time.time + 0.3f;
        }

        private static float notifyTime;
        public static bool IsModded(bool notify)
        {
            if (!PhotonNetwork.InRoom) return false;
            bool modded = NetworkSystem.Instance.GameModeString.Contains("MODDED_");
            if (!modded && notify && Time.time > notifyTime)
            {
                NotificationManager._v3_msg_("<color=grey>[</color><color=red>ERROR</color><color=grey>]</color> You are not in a modded gamemode. Use Utilla to create one, or join an already existing one.");
                notifyTime = Time.time + 1;
            }
            return modded;
        }

        public static float del;

        public static void ModdedCrashGun()
        {
            if (GetGunInput(false))
            {
                var GunData = RenderGun();
                RaycastHit Ray = GunData.Ray;

                if (gunLocked && lockTarget != null)
                {
                    if (!IsModded(true)) return;
                    if (Time.time > del)
                    {
                        PhotonNetwork.SetMasterClient(lockTarget.GetPhotonPlayer());
                        PhotonNetwork.SetMasterClient(PhotonNetwork.LocalPlayer);
                        del = Time.time + 0.02f;
                    }
                    RPCProtection();
                }

                if (GetGunInput(true))
                {
                    VRRig gunTarget = Ray.collider.GetComponentInParent<VRRig>();
                    if (gunTarget && !gunTarget.IsLocal())
                    {
                        gunLocked = true;
                        lockTarget = gunTarget;
                    }
                }
            }
            else
            {
                if (gunLocked)
                    gunLocked = false;
            }
        }

        public static void BetaNearbyFollowCommand(GorillaFriendCollider friendCollider, Player player)
        {
            PhotonNetworkController.Instance.FriendIDList.Add(player.UserId);

            object[] groupJoinSendData = new object[2];
            groupJoinSendData[0] = PhotonNetworkController.Instance.shuffler;
            groupJoinSendData[1] = PhotonNetworkController.Instance.keyStr;
            NetEventOptions netEventOptions = new NetEventOptions { TargetActors = new[] { player.ActorNumber } };

            if (friendCollider.playerIDsCurrentlyTouching.Contains(PhotonNetwork.LocalPlayer.UserId) && friendCollider.playerIDsCurrentlyTouching.Contains(player.UserId) && player != PhotonNetwork.LocalPlayer)
                RoomSystem.SendEvent(4, groupJoinSendData, netEventOptions, false);
            else if (!friendCollider.playerIDsCurrentlyTouching.Contains(PhotonNetwork.LocalPlayer.UserId))
                NotificationManager._v3_msg_("<color=grey>[</color><color=red>ERROR</color><color=grey>]</color> You are not in stump.");
        }

        public static IEnumerator StumpKickDelay(Action action, Action action2, float extraDelay = 0f, bool changeQueue = false)
        {
            PhotonNetworkController.Instance.FriendIDList.Clear();
            yield return new WaitForSeconds(extraDelay);

            bool joinedRoomPatchEnabled = JoinedRoomPatch.enabled;

            string queueArchive = GorillaComputer.instance.currentQueue;
            if (changeQueue)
                GorillaComputer.instance.currentQueue = RandomString();

            action?.Invoke();
            yield return new WaitForSeconds(0.3f);
            action2?.Invoke();
            yield return new WaitForSeconds(1f);

            if (changeQueue)
                GorillaComputer.instance.currentQueue = queueArchive;

            yield return new WaitForSeconds(30f);

            JoinedRoomPatch.enabled = joinedRoomPatchEnabled;
        }

        public static bool kickToPublic;
        public static bool rejoinOnKick;
        public static string specificRoom;
        public static void CreateKickRoom()
        {
            if (rejoinOnKick)
            {
                Important.BroadcastRoom(specificRoom ?? RandomString(), true, PhotonNetworkController.Instance.keyToFollow, PhotonNetworkController.Instance.shuffler);
                Important.Reconnect();

                return;
            }

            Important.CreateRoom(specificRoom ?? RandomString(), kickToPublic, JoinType.JoinWithNearby);
        }

        private static float kickDelay;
        public static void StumpKickGun()
        {
            if (GetGunInput(false))
            {
                var GunData = RenderGun();
                RaycastHit Ray = GunData.Ray;

                if (GetGunInput(true) && Time.time > kickDelay)
                {
                    VRRig gunTarget = Ray.collider.GetComponentInParent<VRRig>();
                    if (gunTarget && !gunTarget.IsLocal())
                    {
                        NetPlayer player = GetPlayerFromVRRig(gunTarget);
                        kickDelay = Time.time + 0.5f;

                        if (!GorillaComputer.instance.friendJoinCollider.playerIDsCurrentlyTouching.Contains(player.UserId))
                        {
                            NotificationManager._v3_msg_("<color=grey>[</color><color=red>ERROR</color><color=grey>]</color> The player must be in stump.");
                            return;
                        }

                        if (!NetworkSystem.Instance.SessionIsPrivate)
                        {
                            NotificationManager._v3_msg_("<color=grey>[</color><color=red>ERROR</color><color=grey>]</color> You must be in a private room.");
                            return;
                        }

                        CoroutineManager.instance.StartCoroutine(StumpKickDelay(() =>
                        {
                            PhotonNetworkController.Instance.shuffler = Random.Range(0, 99).ToString().PadLeft(2, '0') + Random.Range(0, 99999999).ToString().PadLeft(8, '0');
                            PhotonNetworkController.Instance.keyStr = Random.Range(0, 99999999).ToString().PadLeft(8, '0');

                            BetaNearbyFollowCommand(GorillaComputer.instance.friendJoinCollider, NetPlayerToPlayer(player));
                            RPCProtection();
                        }, () =>
                        {
                            CreateKickRoom();
                        }));
                    }
                }
            }
        }

        public static void StumpKickAll()
        {
            if (PhotonNetwork.InRoom)
            {
                if (!NetworkSystem.Instance.SessionIsPrivate)
                {
                    NotificationManager._v3_msg_("<color=grey>[</color><color=red>ERROR</color><color=grey>]</color> You must be in a private room.");
                    return;
                }

                CoroutineManager.instance.StartCoroutine(StumpKickDelay(() =>
                {
                    PhotonNetworkController.Instance.shuffler = Random.Range(0, 99).ToString().PadLeft(2, '0') + Random.Range(0, 99999999).ToString().PadLeft(8, '0');
                    PhotonNetworkController.Instance.keyStr = Random.Range(0, 99999999).ToString().PadLeft(8, '0');

                    foreach (VRRig rig in VRRigCache.ActiveRigs.Where(rig => !rig.IsLocal() && GorillaComputer.instance.friendJoinCollider.playerIDsCurrentlyTouching.Contains(rig.GetPlayer().UserId)))
                        BetaNearbyFollowCommand(GorillaComputer.instance.friendJoinCollider, NetPlayerToPlayer(GetPlayerFromVRRig(rig)));

                    RPCProtection();
                }, () =>
                {
                    CreateKickRoom();
                }));
            }
            else
                NotificationManager._v3_msg_("<color=grey>[</color><color=red>ERROR</color><color=grey>]</color> You are not in a room.");
        }

        private static float elevatorKickDelay;
        public static void ElevatorKickGun()
        {
            if (GetGunInput(false))
            {
                var GunData = RenderGun();
                RaycastHit Ray = GunData.Ray;

                if (GetGunInput(true))
                {
                    VRRig gunTarget = Ray.collider.GetComponentInParent<VRRig>();
                    if (gunTarget && !gunTarget.IsLocal() && Time.time > elevatorKickDelay)
                    {
                        elevatorKickDelay = Time.time + 0.5f;

                        if (PhotonNetwork.IsMasterClient)
                            SpecialTimeRPC(GRElevatorManager._instance.photonView, -750, "RemoteActivateTeleport", new RaiseEventOptions { TargetActors = new[] { gunTarget.GetPlayer().ActorNumber } }, (int)GRElevatorManager._instance.currentLocation, 3, GRElevatorManager.LowestActorNumberInElevator());
                        else
                            GRElevatorManager._instance.SendRPC("RemoteElevatorButtonPress", RpcTarget.MasterClient, new[] { 3, (int)GRElevatorManager._instance.currentLocation });

                        RPCProtection();
                    }
                }
            }
        }

        public static void ElevatorKickAll()
        {
            if (PhotonNetwork.IsMasterClient)
                SpecialTimeRPC(GRElevatorManager._instance.photonView, -750, "RemoteActivateTeleport", new RaiseEventOptions { Receivers = ReceiverGroup.Others }, (int)GRElevatorManager._instance.currentLocation, 2, GRElevatorManager.LowestActorNumberInElevator());
            else
                GRElevatorManager._instance.SendRPC("RemoteElevatorButtonPress", RpcTarget.MasterClient, new[] { 3, (int)GRElevatorManager._instance.currentLocation });
        }

        public static void ElevatorKickAura()
        {
            if (!PhotonNetwork.InRoom) return;
            List<VRRig> nearbyPlayers = new List<VRRig>();

            foreach (VRRig vrrig in VRRigCache.ActiveRigs)
            {
                if (Vector3.Distance(vrrig.transform.position, VRRig.LocalRig.transform.position) < 4 && !vrrig.IsLocal())
                    nearbyPlayers.Add(vrrig);
                else if (nearbyPlayers.Contains(vrrig))
                    nearbyPlayers.Remove(vrrig);
            }

            if (nearbyPlayers.Count > 0)
            {
                foreach (VRRig nearbyPlayer in nearbyPlayers)
                {
                    if (PhotonNetwork.IsMasterClient)
                        SpecialTimeRPC(GRElevatorManager._instance.photonView, -750, "RemoteActivateTeleport", new RaiseEventOptions { TargetActors = new[] { nearbyPlayer.GetPlayer().ActorNumber } }, (int)GRElevatorManager._instance.currentLocation, 3, GRElevatorManager.LowestActorNumberInElevator());
                    else
                        GRElevatorManager._instance.SendRPC("RemoteElevatorButtonPress", RpcTarget.MasterClient, new[] { 3, (int)GRElevatorManager._instance.currentLocation });

                    RPCProtection();
                }
            }
        }

        public static void ElevatorKickOnTouch()
        {
            if (!PhotonNetwork.InRoom) return;

            List<VRRig> touchedPlayers = new List<VRRig>();

            foreach (VRRig rig in VRRigCache.ActiveRigs)
            {
                if (!rig.IsLocal())
                {
                    if (Vector3.Distance(rig.transform.position, VRRig.LocalRig.rightHandTransform.position) <= 0.35f ||
                        Vector3.Distance(rig.transform.position, VRRig.LocalRig.leftHandTransform.position) <= 0.35f)
                    {
                        touchedPlayers.Add(rig);
                    }
                }
            }

            if (touchedPlayers.Count > 0)
            {
                foreach (VRRig rig in touchedPlayers)
                {
                    if (PhotonNetwork.IsMasterClient)
                        SpecialTimeRPC(GRElevatorManager._instance.photonView, -750, "RemoteActivateTeleport", new RaiseEventOptions { TargetActors = new[] { rig.GetPlayer().ActorNumber } }, (int)GRElevatorManager._instance.currentLocation, 3, GRElevatorManager.LowestActorNumberInElevator());
                    else
                        GRElevatorManager._instance.SendRPC("RemoteElevatorButtonPress", RpcTarget.MasterClient, new[] { 3, (int)GRElevatorManager._instance.currentLocation });

                    RPCProtection();
                }
            }
        }

        public static Coroutine kickCoroutine;
        public static IEnumerator KickMasterClient()
        {
            ButtonInfo button = Buttons.GetIndex("Kick Master Client");
            if (!NetworkSystem.Instance.InRoom)
            {
                NotificationManager._v3_msg_("<color=grey>[</color><color=red>ERROR</color><color=grey>]</color> You are not in a room.");
                kickCoroutine = null;
                yield break;
            }

            if (NetworkSystem.Instance.IsMasterClient)
            {
                NotificationManager._v3_msg_("<color=grey>[</color><color=red>ERROR</color><color=grey>]</color> You are master client! You have no one to kick.");
                kickCoroutine = null;
                yield break;
            }

            SerializePatch.OverrideSerialization = () => false;

            Player player = PhotonNetwork.MasterClient;
            VRRig rig = GetVRRigFromPlayer(PhotonNetwork.MasterClient);
            string name = $"<color=#{(rig != null ? ColorUtility.ToHtmlStringRGBA(rig.GetColor()) : "white")}>{player.NickName}</color>";
            NotificationManager._v3_msg_($"<color=grey>[</color><color=purple>KICK</color><color=grey>]</color> Kicking {name}.");
            float time;
            RPCProtection();
        kick:
            {
                time = Time.time + 10f;
                int view = PhotonNetwork.AllocateViewID(0);
                for (int i = 0; i < 3965; i++)
                {
                    PhotonNetwork.NetworkingClient.OpRaiseEvent(202, new Hashtable
                    {
                        { 0, "GameMode" },
                        { 6, PhotonNetwork.ServerTimestamp },
                        { 7, view }
                    }, new RaiseEventOptions
                    {
                        Receivers = ReceiverGroup.MasterClient
                    }, SendOptions.SendReliable);
                }
            }


            while (PhotonNetwork.PlayerList.Contains(player))
            {
                if (Time.time > time)
                {
                    NotificationManager._v3_msg_($"<color=grey>[</color><color=purple>KICK</color><color=grey>]</color> Could not kick {name}. Trying again..");
                    yield return null;
                    goto kick;
                }
                yield return null;
            }

            SerializePatch.OverrideSerialization = null;
            NotificationManager._v3_msg_($"<color=grey>[</color><color=green>SUCCESS</color><color=grey>]</color> {name} has been kicked!");
            kickCoroutine = null;
        }

        public static IEnumerator KickAll()
        {
            ButtonInfo button = Buttons.GetIndex("Kick Master Client");

            if (!NetworkSystem.Instance.InRoom)
            {
                NotificationManager._v3_msg_("<color=grey>[</color><color=red>ERROR</color><color=grey>]</color> You are not in a room.");
                kickCoroutine = null;
                yield break;
            }

            if (NetworkSystem.Instance.IsMasterClient)
            {
                kickCoroutine = null;
                yield break;
            }

            SerializePatch.OverrideSerialization = () => false;

            while (!PhotonNetwork.LocalPlayer.IsMasterClient)
            {
                Player player = PhotonNetwork.MasterClient;
                if (player == null)
                    break;

                VRRig rig = GetVRRigFromPlayer(player);
                string name = $"<color=#{(rig != null ? ColorUtility.ToHtmlStringRGBA(rig.GetColor()) : "white")}>{player.NickName}</color>";

                NotificationManager._v3_msg_($"<color=grey>[</color><color=purple>KICK</color><color=grey>]</color> Kicking {name}.");
                RPCProtection();
                float time;
            kick:
                {
                    time = Time.time + 10f;
                    int view = PhotonNetwork.AllocateViewID(0);
                    for (int i = 0; i < 3965; i++)
                    {
                        PhotonNetwork.NetworkingClient.OpRaiseEvent(202, new Hashtable
                        {
                            { 0, "GameMode" },
                            { 6, PhotonNetwork.ServerTimestamp },
                            { 7, PhotonNetwork.AllocateViewID(0) }
                        }, new RaiseEventOptions
                        {
                            Receivers = ReceiverGroup.MasterClient
                        }, SendOptions.SendReliable);
                    }
                }

                while (PhotonNetwork.MasterClient == player)
                {
                    if (Time.time > time)
                    {
                        NotificationManager._v3_msg_($"<color=grey>[</color><color=red>KICK</color><color=grey>]</color> Could not kick {name}, trying again..");
                        yield return null;
                        goto kick;
                    }
                    yield return null;
                }

                if (!PhotonNetwork.InRoom)
                {
                    NotificationManager._v3_msg_($"<color=grey>[</color><color=red>ERROR</color><color=grey>]</color> Kicking {name} failed. :(");
                    kickCoroutine = null;
                    yield break;
                }

                int left = (Time.time - (time - 10f)) < 2.5f ? 10 : 5;

                NotificationManager._v3_msg_($"<color=grey>[</color><color=green>SUCCESS</color><color=grey>]</color> {name} has been kicked! Waiting {left} seconds to kick the next person..");
                yield return new WaitForSeconds(left);

            }

            SerializePatch.OverrideSerialization = null;

            NotificationManager._v3_msg_($"<color=grey>[</color><color=green>SUCCESS</color><color=grey>]</color> Kicked all successfully!");

            kickCoroutine = null;
        }

        public static void KickGun()
        {
            if (NetworkSystem.Instance.InRoom)
                VisualizeMasterClient();

            if (GetGunInput(false))
            {
                var GunData = RenderGun();
                RaycastHit Ray = GunData.Ray;

                if (gunLocked && lockTarget != null && lockTarget.GetPlayer().IsMasterClient && kickCoroutine == null)
                    kickCoroutine = CoroutineManager.instance.StartCoroutine(KickMasterClient());

                if (GetGunInput(true))
                {
                    VRRig gunTarget = Ray.collider.GetComponentInParent<VRRig>();
                    if (gunTarget && !gunTarget.IsLocal())
                    {
                        gunLocked = true;
                        lockTarget = gunTarget;
                    }
                }
            }
            else
            {
                if (gunLocked)
                    gunLocked = false;
            }
        }

        public static void CacheKickGun()
        {
            if (GetGunInput(false))
            {
                var GunData = RenderGun();
                RaycastHit Ray = GunData.Ray;

                if (gunLocked && lockTarget != null)
                {
                    if (!lockTarget.Active())
                    {
                        NotificationManager._v3_msg_($"<color=grey>[</color><color=green>SUCCESS</color><color=grey>]</color> Kicked all successfully!");
                        gunLocked = false;
                        return;
                    }

                    FreezeServer(8.5f, 3950, new RaiseEventOptions
                    {
                        CachingOption = EventCaching.AddToRoomCache,
                        TargetActors = new[] { lockTarget.GetPlayer().ActorNumber },
                        Flags = new WebFlags(byte.MaxValue)
                    });
                }

                if (GetGunInput(true))
                {
                    VRRig gunTarget = Ray.collider.GetComponentInParent<VRRig>();
                    if (gunTarget && !gunTarget.IsLocal() && !gunLocked)
                    {
                        gunLocked = true;
                        lockTarget = gunTarget;

                        OptimizeEvents = true;
                        string name = $"<color=#{(lockTarget != null ? ColorUtility.ToHtmlStringRGBA(lockTarget.GetColor()) : "white")}>{lockTarget.GetName()}</color>";
                        NotificationManager._v3_msg_($"<color=grey>[</color><color=purple>KICK</color><color=grey>]</color> Kicking {name}. This can take up to 2 minutes, please be patient.");
                    }
                }
            }
            else
            {
                if (gunLocked)
                {
                    gunLocked = false;
                    OptimizeEvents = false;
                }
            }
        }

        public static void EnableCacheKickAll()
        {
            OptimizeEvents = true;
            NotificationManager._v3_msg_($"<color=grey>[</color><color=purple>KICK</color><color=grey>]</color> Kicking everyone. This can take up to 2 minutes, please be patient.");
        }

        public static void CacheKickAll() =>
            FreezeServer(8.5f, 3950, new RaiseEventOptions
            {
                CachingOption = EventCaching.AddToRoomCache,
                Receivers = ReceiverGroup.Others,
                Flags = new WebFlags(byte.MaxValue)
            });

        public static float lagMasterDelay;

        public static void LagMasterClient()
        {
            if (NetworkSystem.Instance.IsMasterClient || !NetworkSystem.Instance.InRoom)
                return;

            if (Time.time > lagMasterDelay)
            {
                int view = PhotonNetwork.AllocateViewID(0);
                for (int i = 0; i < lagAmount; i++)
                {
                    PhotonNetwork.NetworkingClient.OpRaiseEvent(202, new Hashtable
                    {
                        { 0, "GameMode" },
                        { 6, PhotonNetwork.ServerTimestamp },
                        { 7, view }
                    }, new RaiseEventOptions
                    {
                        Receivers = ReceiverGroup.MasterClient
                    }, SendOptions.SendReliable);
                }
                lagMasterDelay = Time.time + lagDelay;
            }
        }

        public static void LagMasterClientGun()
        {
            if (NetworkSystem.Instance.InRoom || !NetworkSystem.Instance.IsMasterClient)
                VisualizeMasterClient();

            if (GetGunInput(false))
            {
                var GunData = RenderGun();
                RaycastHit Ray = GunData.Ray;

                if (gunLocked && lockTarget != null && lockTarget.GetPlayer().IsMasterClient)
                    LagMasterClient();

                if (GetGunInput(true))
                {
                    VRRig gunTarget = Ray.collider.GetComponentInParent<VRRig>();
                    if (gunTarget && !gunTarget.IsLocal())
                    {
                        gunLocked = true;
                        lockTarget = gunTarget;
                    }
                }
            }
            else
            {
                if (gunLocked)
                    gunLocked = false;
            }
        }

        public static void CreatePeerBase()
        {
            PhotonNetwork.NetworkingClient.LoadBalancingPeer.TransportProtocol = ConnectionProtocol.Tcp;
            PhotonNetwork.NetworkingClient.LoadBalancingPeer.peerBase = new TPeer()
            {
                DoFraming = true,
                photonPeer = PhotonNetwork.NetworkingClient.LoadBalancingPeer,
                usedTransportProtocol = ConnectionProtocol.Tcp
            };
        }

        public static void UnloadPeerBase()
        {
            PhotonNetwork.NetworkingClient.LoadBalancingPeer.TransportProtocol = ConnectionProtocol.Udp;
            PhotonNetwork.NetworkingClient.LoadBalancingPeer.peerBase = new EnetPeer
            {
                photonPeer = PhotonNetwork.NetworkingClient.LoadBalancingPeer,
                usedTransportProtocol = ConnectionProtocol.Udp
            };
            NetworkSystem.Instance.ReturnToSinglePlayer();
        }

        public static void BetaShuttleFollowCommand(Player player)
        {
            PhotonNetworkController.Instance.FriendIDList.Add(player.UserId);

            object[] groupJoinSendData = new object[2];
            groupJoinSendData[0] = PhotonNetworkController.Instance.shuffler;
            groupJoinSendData[1] = PhotonNetworkController.Instance.keyStr;
            NetEventOptions netEventOptions = new NetEventOptions { TargetActors = new[] { player.ActorNumber } };

            RoomSystem.SendEvent(11, groupJoinSendData, netEventOptions, false);
        }

        private static float greyZoneDelay;
        public static void ActivateGreyZoneGun(bool status, bool zeroGravity = false)
        {
            if (GetGunInput(false))
            {
                var GunData = RenderGun();
                RaycastHit Ray = GunData.Ray;

                if (GetGunInput(true))
                {
                    VRRig gunTarget = Ray.collider.GetComponentInParent<VRRig>();
                    if (gunTarget && !gunTarget.IsLocal() && Time.time > greyZoneDelay)
                    {
                        greyZoneDelay = Time.time + 0.1f;
                        ActivateGreyZone(status, gunTarget.GetPhotonPlayer(), zeroGravity);
                    }
                }
            }
        }

        private static Coroutine wipeOverride;
        public static IEnumerator ClearOverride()
        {
            yield return new WaitUntil(() => !PhotonNetwork.InRoom);
            SerializePatch.OverrideSerialization = null;

            wipeOverride = null;
        }

        public static void ActivateGreyZone(bool status, Player target, bool zeroGravity = false)
        {
            if (!NetworkSystem.Instance.IsMasterClient)
            {
                NotificationManager._v3_msg_("<color=grey>[</color><color=red>ERROR</color><color=grey>]</color> You are not master client.");
                return;
            }

            SerializePatch.OverrideSerialization = () =>
            {
                MassSerialize(true, new[] { GreyZoneManager.Instance.photonView });
                SerializePatch.OverrideSerialization = null;
                return false;
            };

            wipeOverride ??= CoroutineManager.instance.StartCoroutine(ClearOverride());

            GreyZoneManager.Instance.greyZoneActive = status;
            GreyZoneManager.Instance.photonConnectedDuringActivation = PhotonNetwork.InRoom;
            GreyZoneManager.Instance.greyZoneActivationTime = (GreyZoneManager.Instance.photonConnectedDuringActivation ? PhotonNetwork.Time : ((double)Time.time));

            if (zeroGravity)
                GreyZoneManager.Instance.gravityFactorOptionSelection = int.MaxValue;

            SendSerialize(GreyZoneManager.Instance.photonView, new RaiseEventOptions { TargetActors = new[] { target.ActorNumber } });
        }

        public static void ActivateGreyZone(bool status, bool zeroGravity = false)
        {
            if (NetworkSystem.Instance.InRoom)
            {
                if (!NetworkSystem.Instance.IsMasterClient)
                {
                    NotificationManager._v3_msg_("<color=grey>[</color><color=red>ERROR</color><color=grey>]</color> You are not master client.");
                    return;
                }

                if (status)
                {
                    if (zeroGravity)
                        GreyZoneManager.Instance.gravityFactorOptionSelection = int.MaxValue;
                    else
                        GreyZoneManager.Instance.gravityFactorOptionSelection = 0;

                    GreyZoneManager.Instance.ActivateGreyZoneAuthority();
                }

                else if (!status)
                    GreyZoneManager.Instance.DeactivateGreyZoneAuthority();
            }
        }

        public static float spazGreyDelay;
        public static bool greyState;
        public static void SpazGreyZoneGun()
        {
            if (Time.time > spazGreyDelay)
            {
                greyState = !greyState;
                spazGreyDelay = Time.time + 0.1f;
            }

            ActivateGreyZoneGun(greyState);
        }

        public static void SpazGreyZone()
        {
            if (Time.time > spazGreyDelay)
            {
                greyState = !greyState;
                ActivateGreyZone(greyState);
                spazGreyDelay = Time.time + 0.1f;
            }
        }

        public static void KickAllInParty()
        {
            if (FriendshipGroupDetection.Instance.IsInParty)
            {
                partyLastCode = PhotonNetwork.CurrentRoom.Name;
                waitForPlayerJoin = false;
                PhotonNetworkController.Instance.AttemptToJoinSpecificRoom(Important.RandomRoomName(), JoinType.ForceJoinWithParty);
                partyTime = Time.time + 0.25f;
                partyKickReconnecting = false;
                amountPartying = FriendshipGroupDetection.Instance.myPartyMemberIDs.Count - 1;
                NotificationManager._v3_msg_("<color=grey>[</color><color=purple>PARTY</color><color=grey>]</color> Kicking " + amountPartying + " party members, please be patient..");
            }
            else
                NotificationManager._v3_msg_("<color=grey>[</color><color=red>ERROR</color><color=grey>]</color> You are not in a party.");
        }

        public static void BanAllInParty()
        {
            if (FriendshipGroupDetection.Instance.IsInParty)
            {
                partyLastCode = PhotonNetwork.CurrentRoom.Name;
                waitForPlayerJoin = true;
                PhotonNetworkController.Instance.AttemptToJoinSpecificRoom("KKK", JoinType.ForceJoinWithParty);
                partyTime = Time.time + 0.25f;
                partyKickReconnecting = false;
                amountPartying = FriendshipGroupDetection.Instance.myPartyMemberIDs.Count - 1;
                NotificationManager._v3_msg_("<color=grey>[</color><color=purple>PARTY</color><color=grey>]</color> Banning " + amountPartying + " party members, please be patient..");
            }
            else
                NotificationManager._v3_msg_("<color=grey>[</color><color=red>ERROR</color><color=grey>]</color> You are not in a party.");
        }

        public static Coroutine partyKickDelayCoroutine;
        public static IEnumerator PartyKickDelay(bool ban)
        {
            yield return new WaitForSeconds(0.25f);

            if (ban)
                BanAllInParty();
            else
                KickAllInParty();

            Coroutine thisCoroutine = partyKickDelayCoroutine;
            partyKickDelayCoroutine = null;

            CoroutineManager.instance.StopCoroutine(thisCoroutine);
        }

        public static bool previousInParty;
        public static void AutoPartyKick()
        {
            if (FriendshipGroupDetection.Instance.IsInParty && !previousInParty)
                partyKickDelayCoroutine ??= CoroutineManager.instance.StartCoroutine(PartyKickDelay(false));

            previousInParty = FriendshipGroupDetection.Instance.IsInParty;
        }

        public static void AutoPartyBan()
        {
            if (FriendshipGroupDetection.Instance.IsInParty && !previousInParty)
                partyKickDelayCoroutine ??= CoroutineManager.instance.StartCoroutine(PartyKickDelay(true));

            previousInParty = FriendshipGroupDetection.Instance.IsInParty;
        }

        private static float breakDelay;
        public static void PartyBreakNetworkTriggers()
        {
            if (FriendshipGroupDetection.Instance.IsInParty && Time.time > breakDelay)
            {
                breakDelay = Time.time + 1f;
                FriendshipGroupDetection.Instance.photonView.RPC("PartyMemberIsAboutToGroupJoin", RpcTarget.All, Array.Empty<object>());
            }
        }

        public static bool legacyKickFreeze;

        /// <summary>
        /// Indicates whether event optimization is enabled. When enabled, it reduces network load by limiting certain RPC calls and adjusting serialization rates.
        /// </summary>
        private static bool _optimizeEvents;
        public static bool OptimizeEvents
        {
            get => _optimizeEvents;
            set
            {
                if (_optimizeEvents != value)
                {
                    _optimizeEvents = value;
                    if (_optimizeEvents)
                    {
                        if (legacyKickFreeze)
                            SerializePatch.OverrideSerialization = () => false;
                        else
                        {
                            PhotonNetwork.SerializationRate = 2;
                            RPCFilter.FilteredRPCs["OnHandTapRPC"] = () => false;
                            RPCFilter.FilteredRPCs["RPC_UpdateCosmeticsWithTryonPacked"] = () => false;

                            SerializePatch.OverrideSerialization = () =>
                            {
                                SendSerialize(GorillaTagger.Instance.myVRRig.GetView);
                                return true;
                            };
                        }
                    }
                    else
                    {
                        if (SerializePatch.OverrideSerialization != null)
                            SerializePatch.OverrideSerialization = null;

                        PhotonNetwork.SerializationRate = 10;
                        RPCFilter.FilteredRPCs.Remove("OnHandTapRPC");
                        RPCFilter.FilteredRPCs.Remove("RPC_UpdateCosmeticsWithTryonPacked");
                    }
                }
            }
        }

        public static void PartyKickGun()
        {
            if (GetGunInput(false))
            {
                var GunData = RenderGun();
                RaycastHit Ray = GunData.Ray;

                if (gunLocked && lockTarget != null)
                    OptimizeEvents = true;

                if (GetGunInput(true))
                {
                    VRRig gunTarget = Ray.collider.GetComponentInParent<VRRig>();
                    if (gunTarget && !gunTarget.IsLocal())
                    {
                        if (lockTarget == null && FriendshipGroupDetection.Instance.IsInMyGroup(gunTarget.GetPlayer().UserId))
                        {
                            for (int i = 0; i < 3970; i++)
                                FriendshipGroupDetection.Instance.photonView.RPC("RequestPartyGameMode", gunTarget.GetPhotonPlayer(),
                                    new object[] { GameMode.gameModeKeyByName.Keys.ToArray()[Random.Range(0, GameMode.gameModeKeyByName.Keys.Count)] });

                            RPCProtection();
                        }

                        gunLocked = true;
                        lockTarget = gunTarget;
                    }
                }
            }
            else
            {
                OptimizeEvents = false;
                if (gunLocked)
                    gunLocked = false;
            }
        }

        public static void PartyKickAll()
        {
            OptimizeEvents = true;

            if (Time.time > kickDelay)
            {
                kickDelay = Time.time + 10f;
                for (int i = 0; i < 3970; i++)
                    SpecialTargetRPC
                    (
                        FriendshipGroupDetection.Instance.photonView,
                        "RequestPartyGameMode",
                        new RaiseEventOptions
                        {
                            TargetActors =
                            NetworkSystem.Instance.PlayerListOthers
                                .Where(plr => FriendshipGroupDetection.Instance.IsInMyGroup(plr.UserId))
                                .Select(plr => plr.ActorNumber).ToArray()
                        },
                        new object[]
                        {
                            GameMode.gameModeKeyByName.Keys.ToArray()[Random.Range(0, GameMode.gameModeKeyByName.Keys.Count)]
                        }
                    );

                RPCProtection();
            }
        }

        public static void PartyKickAura()
        {
            if (!PhotonNetwork.InRoom) return;
            List<VRRig> nearbyPlayers = new List<VRRig>();

            foreach (VRRig vrrig in VRRigCache.ActiveRigs)
            {
                if (Vector3.Distance(vrrig.transform.position, VRRig.LocalRig.transform.position) < 4 && !vrrig.IsLocal())
                    nearbyPlayers.Add(vrrig);
                else if (nearbyPlayers.Contains(vrrig))
                    nearbyPlayers.Remove(vrrig);
            }

            if (nearbyPlayers.Count > 0)
            {
                OptimizeEvents = true;
                foreach (VRRig nearbyPlayer in nearbyPlayers)
                {
                    for (int i = 0; i < 3950; i++)
                        FriendshipGroupDetection.Instance.photonView.RPC("RequestPartyGameMode", nearbyPlayer.GetPhotonPlayer(),
                            new object[] { GameMode.gameModeKeyByName.Keys.ToArray()[Random.Range(0, GameMode.gameModeKeyByName.Keys.Count)] });

                    RPCProtection();
                }
            }
            else
                OptimizeEvents = false;
        }

        public static void PartyKickOnTouch()
        {
            if (!PhotonNetwork.InRoom) return;

            List<VRRig> touchedPlayers = new List<VRRig>();

            foreach (VRRig rig in VRRigCache.ActiveRigs)
            {
                if (!rig.IsLocal())
                {
                    if (Vector3.Distance(rig.transform.position, VRRig.LocalRig.rightHandTransform.position) <= 0.35f ||
                        Vector3.Distance(rig.transform.position, VRRig.LocalRig.leftHandTransform.position) <= 0.35f)
                    {
                        touchedPlayers.Add(rig);
                    }
                }
            }

            if (touchedPlayers.Count > 0)
            {
                OptimizeEvents = true;
                foreach (VRRig rig in touchedPlayers)
                {
                    for (int i = 0; i < 3950; i++)
                        FriendshipGroupDetection.Instance.photonView.RPC("RequestPartyGameMode", rig.GetPhotonPlayer(),
                            new object[] { GameMode.gameModeKeyByName.Keys.ToArray()[Random.Range(0, GameMode.gameModeKeyByName.Keys.Count)] });

                    RPCProtection();
                }
            }
            else
                OptimizeEvents = false;
        }

        private static float antiReportLagDelay;
        public static void AntiReportLag()
        {
            if (Time.time > antiReportLagDelay)
            {
                List<int> actors = new List<int>();

                Safety.AntiReport((vrrig, position) =>
                {
                    antiReportLagDelay = Time.time + 0.1f;
                    actors.Add(GetPlayerFromVRRig(vrrig).ActorNumber);
                    NotificationManager._v3_msg_("<color=grey>[</color><color=purple>ANTI-REPORT</color><color=grey>]</color> " + GetPlayerFromVRRig(vrrig).NickName + " attempted to report you, they are being lagged.");
                });

                if (actors.Count > 0)
                    LagTarget(actors);
            }
        }

        public static float setMasterDelay;
        public static void SetMasterClient(bool skip = false)
        {
            if (NetworkSystem.Instance.IsMasterClient)
                return;
            if (PhotonNetwork.PlayerList.Length > 5)
            {
                if (!skip)
                    NotificationManager._v3_msg_($"<color=grey>[</color><color=red>ERROR</color><color=grey>]</color> {PhotonNetwork.PlayerList.Length - 5} people must leave for this mod to work.");
                return;
            }
            if (Time.time > setMasterDelay)
            {
                PhotonNetwork.SetMasterClient(PhotonNetwork.LocalPlayer);
                setMasterDelay = Time.time + 5f;
            }
        }

        public static void SetRoomStatus(bool status)
        {
            Dictionary<byte, object> dictionary = new Dictionary<byte, object>
            {
                { 251, new Hashtable { { 253, status }, { 254, status }, { 255, status ? 0 : PhotonNetworkController.Instance.currentJoinTrigger.GetRoomSize(SubscriptionManager.IsLocalSubscribed()) } } },
                { 250, true },
                { 231, null }
            };

            PhotonNetwork.CurrentRoom.LoadBalancingClient.LoadBalancingPeer.SendOperation(
                252,
                dictionary,
                SendOptions.SendReliable
            );
            GorillaScoreboardTotalUpdater.instance.UpdateActiveScoreboards();
        }

        public static void DestroyGun()
        {
            if (GetGunInput(false))
            {
                var GunData = RenderGun();
                RaycastHit Ray = GunData.Ray;

                if (GetGunInput(true) && Time.time > destroyDelay)
                {
                    VRRig gunTarget = Ray.collider.GetComponentInParent<VRRig>();
                    if (gunTarget && !gunTarget.IsLocal())
                    {
                        DestroyPlayer(NetPlayerToPlayer(GetPlayerFromVRRig(gunTarget)));
                        destroyDelay = Time.time + 0.5f;
                    }
                }
            }
        }

        public static void DestroyAll()
        {
            foreach (Player player in PhotonNetwork.PlayerListOthers)
                DestroyPlayer(player);
        }

        public static void DestroyAura()
        {
            if (!PhotonNetwork.InRoom) return;
            List<VRRig> nearbyPlayers = new List<VRRig>();

            foreach (VRRig vrrig in VRRigCache.ActiveRigs)
            {
                if (Vector3.Distance(vrrig.transform.position, VRRig.LocalRig.transform.position) < 4 && !vrrig.IsLocal())
                    nearbyPlayers.Add(vrrig);
                else if (nearbyPlayers.Contains(vrrig))
                    nearbyPlayers.Remove(vrrig);
            }

            if (nearbyPlayers.Count > 0)
            {
                foreach (VRRig nearbyPlayer in nearbyPlayers)
                {
                    DestroyPlayer(NetPlayerToPlayer(GetPlayerFromVRRig(nearbyPlayer)));
                }
            }
        }

        public static void DestroyOnTouch()
        {
            if (!PhotonNetwork.InRoom) return;

            List<VRRig> touchedPlayers = new List<VRRig>();

            foreach (VRRig rig in VRRigCache.ActiveRigs)
            {
                if (!rig.IsLocal())
                {
                    if (Vector3.Distance(rig.transform.position, VRRig.LocalRig.rightHandTransform.position) <= 0.35f ||
                        Vector3.Distance(rig.transform.position, VRRig.LocalRig.leftHandTransform.position) <= 0.35f)
                    {
                        touchedPlayers.Add(rig);
                    }
                }
            }

            if (touchedPlayers.Count > 0)
            {
                foreach (VRRig rig in touchedPlayers)
                {
                    DestroyPlayer(NetPlayerToPlayer(GetPlayerFromVRRig(rig)));
                }
            }
        }

        public static void DestroyPlayer(NetPlayer player) =>
            PhotonNetwork.OpRemoveCompleteCacheOfPlayer(player.ActorNumber);

        public static void TargetSpam()
        {
            if (NetworkSystem.Instance.IsMasterClient)
            {
                foreach (HitTargetNetworkState hitTargetNetworkState in GetAllType<HitTargetNetworkState>())
                {
                    hitTargetNetworkState.hitCooldownTime = 0;
                    hitTargetNetworkState.TargetHit(Vector3.zero, Vector3.zero);
                }
            }
            else NotificationManager._v3_msg_("<color=grey>[</color><color=red>ERROR</color><color=grey>]</color> You are not master client.");
        }

        public static void InfectionToTag()
        {
            if (!NetworkSystem.Instance.IsMasterClient)
                NotificationManager._v3_msg_("<color=grey>[</color><color=red>ERROR</color><color=grey>]</color> You are not master client.");
            else
            {
                GorillaTagManager gorillaTagManager = (GorillaTagManager)GorillaGameManager.instance;
                gorillaTagManager.infectedModeThreshold = PhotonNetwork.CurrentRoom.MaxPlayers + 1;
            }
        }

        public static void TagToInfection()
        {
            if (!NetworkSystem.Instance.IsMasterClient)
                NotificationManager._v3_msg_("<color=grey>[</color><color=red>ERROR</color><color=grey>]</color> You are not master client.");
            else
            {
                GorillaTagManager gorillaTagManager = (GorillaTagManager)GorillaGameManager.instance;
                gorillaTagManager.infectedModeThreshold = 1;
            }
        }

        public static void FixThreshold()
        {
            GorillaTagManager gorillaTagManager = (GorillaTagManager)GorillaGameManager.instance;
            gorillaTagManager.infectedModeThreshold = 4;
        }

        private static float rockDebounce;
        public static void RockSelf()
        {
            if (PhotonNetwork.IsMasterClient)
                AddRock(NetworkSystem.Instance.LocalPlayer);
            else
                NotificationManager._v3_msg_("<color=grey>[</color><color=red>ERROR</color><color=grey>]</color> You are not master client.");
        }

        public static void RockGun()
        {
            if (GetGunInput(false))
            {
                var GunData = RenderGun();
                RaycastHit Ray = GunData.Ray;

                if (GetGunInput(true))
                {
                    VRRig gunTarget = Ray.collider.GetComponentInParent<VRRig>();
                    if (gunTarget && !gunTarget.IsLocal() && Time.time > rockDebounce)
                    {
                        rockDebounce = Time.time + 0.1f;
                        if (PhotonNetwork.IsMasterClient)
                            AddRock(GetPlayerFromVRRig(gunTarget));
                        else
                            NotificationManager._v3_msg_("<color=grey>[</color><color=red>ERROR</color><color=grey>]</color> You are not master client.");
                    }
                }
            }
        }

        public static void RockAura()
        {
            if (!PhotonNetwork.InRoom) return;
            List<VRRig> nearbyPlayers = new List<VRRig>();

            foreach (VRRig vrrig in VRRigCache.ActiveRigs)
            {
                if (Vector3.Distance(vrrig.transform.position, VRRig.LocalRig.transform.position) < 4 && !vrrig.IsLocal())
                    nearbyPlayers.Add(vrrig);
                else if (nearbyPlayers.Contains(vrrig))
                    nearbyPlayers.Remove(vrrig);
            }

            if (nearbyPlayers.Count > 0)
            {
                foreach (VRRig nearbyPlayer in nearbyPlayers)
                {
                    if (Time.time > rockDebounce)
                    {
                        rockDebounce = Time.time + 0.1f;
                        if (PhotonNetwork.IsMasterClient)
                            AddRock(GetPlayerFromVRRig(nearbyPlayer));
                        else
                            NotificationManager._v3_msg_("<color=grey>[</color><color=red>ERROR</color><color=grey>]</color> You are not master client.");
                    }
                }
            }
        }

        public static void RockOnTouch()
        {
            if (!PhotonNetwork.InRoom) return;

            List<VRRig> touchedPlayers = new List<VRRig>();

            foreach (VRRig rig in VRRigCache.ActiveRigs)
            {
                if (!rig.IsLocal())
                {
                    if (Vector3.Distance(rig.transform.position, VRRig.LocalRig.rightHandTransform.position) <= 0.35f ||
                        Vector3.Distance(rig.transform.position, VRRig.LocalRig.leftHandTransform.position) <= 0.35f)
                    {
                        touchedPlayers.Add(rig);
                    }
                }
            }

            if (touchedPlayers.Count > 0)
            {
                foreach (VRRig rig in touchedPlayers)
                {
                    if (Time.time > rockDebounce)
                    {
                        rockDebounce = Time.time + 0.1f;
                        if (PhotonNetwork.IsMasterClient)
                            AddRock(GetPlayerFromVRRig(rig));
                        else
                            NotificationManager._v3_msg_("<color=grey>[</color><color=red>ERROR</color><color=grey>]</color> You are not master client.");
                    }
                }
            }
        }

        public static void RockAll()
        {
            if (Time.time > rockDebounce)
            {
                rockDebounce = Time.time + 0.1f;
                if (PhotonNetwork.IsMasterClient)
                    AddRock(GetRandomPlayer(true));
                else
                    NotificationManager._v3_msg_("<color=grey>[</color><color=red>ERROR</color><color=grey>]</color> You are not master client.");
            }
        }

        public static void BetaSetStatus(RoomSystem.StatusEffects state, RaiseEventOptions reo)
        {
            if (!NetworkSystem.Instance.IsMasterClient)
                NotificationManager._v3_msg_("<color=grey>[</color><color=red>ERROR</color><color=grey>]</color> You are not master client.");
            else
            {
                object[] statusSendData = new object[1];
                statusSendData[0] = (int)state;
                object[] sendEventData = new object[3];
                sendEventData[0] = NetworkSystem.Instance.ServerTimestamp;
                sendEventData[1] = (byte)2;
                sendEventData[2] = statusSendData;
                PhotonNetwork.RaiseEvent(3, sendEventData, reo, SendOptions.SendUnreliable);
            }
        }

        public static void SlowSelf()
        {
            NetPlayer player = PhotonNetwork.LocalPlayer;
            BetaSetStatus(RoomSystem.StatusEffects.TaggedTime, new RaiseEventOptions { TargetActors = new[] { player.ActorNumber } });
            RPCProtection();
        }

        private static float slowDelay;
        public static void SlowGun()
        {
            if (GetGunInput(false))
            {
                var GunData = RenderGun();
                RaycastHit Ray = GunData.Ray;

                if (GetGunInput(true) && Time.time > slowDelay)
                {
                    VRRig gunTarget = Ray.collider.GetComponentInParent<VRRig>();
                    if (gunTarget && !gunTarget.IsLocal())
                    {
                        NetPlayer player = GetPlayerFromVRRig(gunTarget);
                        BetaSetStatus(RoomSystem.StatusEffects.TaggedTime, new RaiseEventOptions { TargetActors = new[] { player.ActorNumber } });
                        RPCProtection();
                        slowDelay = Time.time + 1f;
                    }
                }
            }
        }

        public static void SlowAura()
        {
            if (!PhotonNetwork.InRoom) return;
            List<VRRig> nearbyPlayers = new List<VRRig>();

            foreach (VRRig vrrig in VRRigCache.ActiveRigs)
            {
                if (Vector3.Distance(vrrig.transform.position, VRRig.LocalRig.transform.position) < 4 && !vrrig.IsLocal())
                    nearbyPlayers.Add(vrrig);
                else if (nearbyPlayers.Contains(vrrig))
                    nearbyPlayers.Remove(vrrig);
            }

            if (nearbyPlayers.Count > 0)
            {
                foreach (VRRig nearbyPlayer in nearbyPlayers)
                {
                    NetPlayer player = GetPlayerFromVRRig(nearbyPlayer);
                    BetaSetStatus(RoomSystem.StatusEffects.TaggedTime, new RaiseEventOptions { TargetActors = new[] { player.ActorNumber } });
                    RPCProtection();
                }
            }
        }

        public static void SlowOnTouch()
        {
            if (!PhotonNetwork.InRoom) return;

            List<VRRig> touchedPlayers = new List<VRRig>();

            foreach (VRRig rig in VRRigCache.ActiveRigs)
            {
                if (!rig.IsLocal())
                {
                    if (Vector3.Distance(rig.transform.position, VRRig.LocalRig.rightHandTransform.position) <= 0.35f ||
                        Vector3.Distance(rig.transform.position, VRRig.LocalRig.leftHandTransform.position) <= 0.35f)
                    {
                        touchedPlayers.Add(rig);
                    }
                }
            }

            if (touchedPlayers.Count > 0)
            {
                foreach (VRRig rig in touchedPlayers)
                {
                    NetPlayer player = GetPlayerFromVRRig(rig);
                    BetaSetStatus(RoomSystem.StatusEffects.TaggedTime, new RaiseEventOptions { TargetActors = new[] { player.ActorNumber } });
                    RPCProtection();
                }
            }
        }

        public static void SlowAll()
        {
            if (Time.time > slowDelay)
            {
                BetaSetStatus(RoomSystem.StatusEffects.TaggedTime, new RaiseEventOptions { Receivers = ReceiverGroup.Others });
                RPCProtection();
                slowDelay = Time.time + 1f;
            }
        }

        public static void VibrateSelf()
        {
            NetPlayer owner = PhotonNetwork.LocalPlayer;
            BetaSetStatus(RoomSystem.StatusEffects.JoinedTaggedTime, new RaiseEventOptions { TargetActors = new[] { owner.ActorNumber } });
            RPCProtection();
        }

        private static float vibrateDelay;
        public static void VibrateGun()
        {
            if (GetGunInput(false))
            {
                var GunData = RenderGun();
                RaycastHit Ray = GunData.Ray;

                if (GetGunInput(true) && Time.time > vibrateDelay)
                {
                    VRRig gunTarget = Ray.collider.GetComponentInParent<VRRig>();
                    if (gunTarget && !gunTarget.IsLocal())
                    {
                        NetPlayer owner = GetPlayerFromVRRig(gunTarget);
                        BetaSetStatus(RoomSystem.StatusEffects.JoinedTaggedTime, new RaiseEventOptions { TargetActors = new[] { owner.ActorNumber } });
                        RPCProtection();
                        vibrateDelay = Time.time + 0.5f;
                    }
                }
            }
        }

        public static void VibrateAura()
        {
            if (!PhotonNetwork.InRoom) return;
            List<VRRig> nearbyPlayers = new List<VRRig>();

            foreach (VRRig vrrig in VRRigCache.ActiveRigs)
            {
                if (Vector3.Distance(vrrig.transform.position, VRRig.LocalRig.transform.position) < 4 && !vrrig.IsLocal())
                    nearbyPlayers.Add(vrrig);
                else if (nearbyPlayers.Contains(vrrig))
                    nearbyPlayers.Remove(vrrig);
            }

            if (nearbyPlayers.Count > 0)
            {
                foreach (VRRig nearbyPlayer in nearbyPlayers)
                {
                    NetPlayer owner = GetPlayerFromVRRig(nearbyPlayer);
                    BetaSetStatus(RoomSystem.StatusEffects.JoinedTaggedTime, new RaiseEventOptions { TargetActors = new[] { owner.ActorNumber } });
                    RPCProtection();
                }
            }
        }

        public static void VibrateOnTouch()
        {
            if (!PhotonNetwork.InRoom) return;

            List<VRRig> touchedPlayers = new List<VRRig>();

            foreach (VRRig rig in VRRigCache.ActiveRigs)
            {
                if (!rig.IsLocal())
                {
                    if (Vector3.Distance(rig.transform.position, VRRig.LocalRig.rightHandTransform.position) <= 0.35f ||
                        Vector3.Distance(rig.transform.position, VRRig.LocalRig.leftHandTransform.position) <= 0.35f)
                    {
                        touchedPlayers.Add(rig);
                    }
                }
            }

            if (touchedPlayers.Count > 0)
            {
                foreach (VRRig rig in touchedPlayers)
                {
                    NetPlayer owner = GetPlayerFromVRRig(rig);
                    BetaSetStatus(RoomSystem.StatusEffects.JoinedTaggedTime, new RaiseEventOptions { TargetActors = new[] { owner.ActorNumber } });
                    RPCProtection();
                }
            }
        }

        public static void VibrateAll()
        {
            if (Time.time > vibrateDelay)
            {
                BetaSetStatus(RoomSystem.StatusEffects.JoinedTaggedTime, new RaiseEventOptions { Receivers = ReceiverGroup.Others });
                RPCProtection();
                vibrateDelay = Time.time + 0.5f;
            }
        }

        public static void GliderBlindGun()
        {
            if (GetGunInput(false))
            {
                var GunData = RenderGun();
                RaycastHit Ray = GunData.Ray;

                if (GetGunInput(true))
                {
                    VRRig gunTarget = Ray.collider.GetComponentInParent<VRRig>();
                    if (gunTarget && !gunTarget.IsLocal())
                    {
                        gunLocked = true;
                        lockTarget = gunTarget;
                    }
                }

                if (gunLocked)
                {
                    foreach (GliderHoldable glider in GetAllType<GliderHoldable>())
                    {
                        if (glider.GetView.Owner == PhotonNetwork.LocalPlayer)
                        {
                            glider.gameObject.transform.position = lockTarget.headMesh.transform.position;
                            glider.gameObject.transform.rotation = Quaternion.Euler(new Vector3(Random.Range(0, 360), Random.Range(0, 360), Random.Range(0, 360)));
                        }
                        else
                            glider.OnHover(null, null);
                    }
                }
            }
            else
            {
                if (gunLocked)
                    gunLocked = false;
            }
        }

        public static void GliderBlindAll()
        {
            GliderHoldable[] those = GetAllType<GliderHoldable>();
            int index = 0;
            foreach (var vrrig in VRRigCache.ActiveRigs.Where(vrrig => !vrrig.isLocal))
            {
                try
                {
                    GliderHoldable glider = those[index];
                    if (glider.GetView.Owner == PhotonNetwork.LocalPlayer)
                    {
                        glider.gameObject.transform.position = vrrig.headMesh.transform.position;
                        glider.gameObject.transform.rotation = Quaternion.Euler(new Vector3(Random.Range(0, 360), Random.Range(0, 360), Random.Range(0, 360)));
                    }
                    else
                        glider.OnHover(null, null);
                }
                catch { }
                index++;
            }
        }

        public static void BreakAudioGun()
        {
            if (GetGunInput(false))
            {
                var GunData = RenderGun();
                RaycastHit Ray = GunData.Ray;

                if (gunLocked && lockTarget != null)
                {
                    GorillaTagger.Instance.myVRRig.SendRPC("RPC_PlayHandTap", GetPlayerFromVRRig(lockTarget), 111, false, 999999f);
                    RPCProtection();
                }
                if (GetGunInput(true))
                {
                    VRRig gunTarget = Ray.collider.GetComponentInParent<VRRig>();
                    if (gunTarget && !gunTarget.IsLocal())
                    {
                        gunLocked = true;
                        lockTarget = gunTarget;
                    }
                }
            }
            else
            {
                if (gunLocked)
                {
                    gunLocked = false;
                    VRRig.LocalRig.enabled = true;
                }
            }
        }

        public static void BreakAudioAll()
        {
            if (rightTrigger > 0.5f)
            {
                GorillaTagger.Instance.myVRRig.SendRPC("RPC_PlayHandTap", RpcTarget.Others, 111, false, 999999f);
            }
        }

        public static Coroutine RopeCoroutine;
        public static IEnumerator RopeEnableRig()
        {
            yield return new WaitForSeconds(0.3f);
            VRRig.LocalRig.enabled = true;
        }

        public static void BetaSetRopeVelocity(int RopeId, Vector3 Velocity)
        {
            Velocity = Velocity.ClampMagnitudeSafe(100f);

            if (RopeSwingManager.instance.ropes.TryGetValue(RopeId, out GorillaRopeSwing Rope))
            {
                var ClosestNode = Rope.nodes
                    .Skip(1)
                    .Select((v, i) => new
                    {
                        index = i,
                        transform = v,
                        distance = Vector3.Distance(GorillaTagger.Instance.bodyCollider.transform.position, v.transform.position)
                    })
                    .OrderBy(x => x.distance)
                    .First();

                if (ClosestNode.distance > 5f)
                {
                    if (RopeCoroutine != null)
                        CoroutineManager.instance.StopCoroutine(RopeCoroutine);

                    RopeCoroutine = CoroutineManager.instance.StartCoroutine(RopeEnableRig());

                    VRRig.LocalRig.enabled = false;
                    VRRig.LocalRig.transform.position = ClosestNode.transform.position;
                }

                if (Vector3.Distance(ServerPos, ClosestNode.transform.position) < 5f)
                    RopeSwingManager.instance.SendSetVelocity_RPC(RopeId, ClosestNode.index, Velocity.ClampMagnitudeSafe(100f), true);
                else
                    RopeDelay = 0f;

                RPCProtection();
            }
        }

        public static void BetaSetRopeVelocity(GorillaRopeSwing Rope, Vector3 Velocity) =>
            BetaSetRopeVelocity(RopeSwingManager.instance.ropes.FirstOrDefault(x => x.Value == Rope).Key, Velocity);

        private static float randomRopeDelay;
        private static GorillaRopeSwing randomRope;
        public static GorillaRopeSwing GetRandomRope()
        {
            if (Time.time > randomRopeDelay)
            {
                randomRopeDelay = Time.time + 0.5f;
                randomRope = RopeSwingManager.instance.ropes.Values.OrderBy(_ => Random.value).FirstOrDefault();
            }
            return randomRope;
        }

        private static float RopeDelay;
        public static void JoystickRopeControl() // Thanks to ShibaGT for the fix
        {
            if ((Mathf.Abs(rightJoystick.x) > 0.05f || Mathf.Abs(rightJoystick.y) > 0.05f) && Time.time > RopeDelay)
            {
                RopeDelay = Time.time + 0.125f;

                GorillaRopeSwing rope = GetRandomType<GorillaRopeSwing>(0.25f);
                BetaSetRopeVelocity(rope, new Vector3(rightJoystick.x * 100f, rightJoystick.y * 100f, 0f));
            }
        }

        public static void SpazRopeGun()
        {
            if (GetGunInput(false))
            {
                var GunData = RenderGun();
                RaycastHit Ray = GunData.Ray;

                if (GetGunInput(true))
                {
                    GorillaRopeSwing gunTarget = Ray.collider.GetComponentInParent<GorillaRopeSwing>();
                    if (gunTarget && Time.time > RopeDelay)
                    {
                        RopeDelay = Time.time + 0.25f;
                        BetaSetRopeVelocity(gunTarget, RandomVector3(100f));
                    }
                }
            }
        }

        public static void SpazAllRopes()
        {
            if (rightTrigger > 0.5f && Time.time > RopeDelay)
            {
                RopeDelay = Time.time + 0.125f;

                GorillaRopeSwing rope = GetRandomRope();
                BetaSetRopeVelocity(rope, RandomVector3(100f));
            }
        }

        public static void SpazGrabbedRopes()
        {
            if (Time.time > RopeDelay)
            {
                RopeDelay = Time.time + 0.125f;
                VRRig randomRig = VRRigCache.ActiveRigs
                    .Where(rig => rig.currentRopeSwing != null)
                    .OrderBy(_ => Random.value)
                    .FirstOrDefault();

                if (randomRig != null)
                    BetaSetRopeVelocity(randomRig.currentRopeSwing, RandomVector3(100f));
            }
        }

        public static void FlingRopeGun()
        {
            if (GetGunInput(false))
            {
                var GunData = RenderGun();
                RaycastHit Ray = GunData.Ray;

                if (GetGunInput(true))
                {
                    GorillaRopeSwing gunTarget = Ray.collider.GetComponentInParent<GorillaRopeSwing>();

                    if (gunTarget && Time.time > RopeDelay)
                    {
                        RopeDelay = Time.time + 0.125f;
                        BetaSetRopeVelocity(gunTarget, RandomVector3(100f));
                    }
                }
            }
        }

        public static void FlingAllRopesGun()
        {
            if (GetGunInput(false))
            {
                var GunData = RenderGun();
                GameObject NewPointer = GunData.NewPointer;

                if (GetGunInput(true) && Time.time > RopeDelay)
                {
                    RopeDelay = Time.time + 0.125f;

                    GorillaRopeSwing rope = GetRandomRope();
                    BetaSetRopeVelocity(rope, (NewPointer.transform.position - rope.transform.position).normalized * 100f);
                }
            }
        }

        public static void EffectSpam(CrittersManager.CritterEvent critterEvent)
        {
            if (rightGrab)
            {
                if (PhotonNetwork.IsMasterClient)
                {
                    CrittersPawn[] critters = CrittersManager.instance.crittersPawns.ToArray();
                    if (critters.Length > 0)
                    {
                        CrittersPawn critter = critters[0];
                        critter.transform.position = GorillaTagger.Instance.rightHandTransform.position;
                        int actorId = critter.actorId;
                        CrittersManager.instance.TriggerEvent(critterEvent, actorId, critter.transform.position, Quaternion.LookRotation(critter.transform.up));
                    }
                }
                else
                {
                    CrittersActor.CrittersActorType type = CrittersActor.CrittersActorType.StickyTrap;
                    Vector3 velocity = Vector3.down * 20f;
                    switch (critterEvent)
                    {
                        case CrittersManager.CritterEvent.StunExplosion:
                            type = CrittersActor.CrittersActorType.StunBomb;
                            break;
                        case CrittersManager.CritterEvent.StickyDeployed:
                        case CrittersManager.CritterEvent.StickyTriggered:
                            type = CrittersActor.CrittersActorType.StickyTrap;
                            break;
                        case CrittersManager.CritterEvent.NoiseMakerTriggered:
                            type = CrittersActor.CrittersActorType.NoiseMaker;
                            break;
                    }

                    CrittersGrabber localGrabber = GetAllType<CrittersGrabber>().Where(grabber => grabber.rigPlayerId == PhotonNetwork.LocalPlayer.ActorNumber && grabber.isLeft).FirstOrDefault();
                    List<CrittersActor> critters = GetAllType<CrittersActor>().Where(critter => critter != null && critter.crittersActorType == type && Vector3.Distance(critter.transform.position, GorillaTagger.Instance.bodyCollider.transform.position) < 25f && Vector3.Distance(critter.transform.position, GorillaTagger.Instance.bodyCollider.transform.position) > 3f).OrderByDescending(critter => Vector3.Distance(critter.transform.position, GorillaTagger.Instance.bodyCollider.transform.position)).ToList();

                    if (critters.Count <= 0)
                        critters = GetAllType<CrittersActor>().Where(critter => critter != null && critter.crittersActorType == type && Vector3.Distance(critter.transform.position, GorillaTagger.Instance.bodyCollider.transform.position) < 25f).OrderByDescending(critter => Vector3.Distance(critter.transform.position, GorillaTagger.Instance.bodyCollider.transform.position)).ToList();

                    if (critters.Count <= 0)
                        critters = GetAllType<CrittersActor>().Where(critter => critter != null && critter.crittersActorType == type).OrderByDescending(critter => Vector3.Distance(critter.transform.position, GorillaTagger.Instance.bodyCollider.transform.position)).ToList();

                    CrittersActor critter = critters[Random.Range(0, critters.Count)];

                    if (Vector3.Distance(critter.transform.position, GorillaTagger.Instance.bodyCollider.transform.position) > 25f)
                    {
                        VRRig.LocalRig.enabled = false;
                        VRRig.LocalRig.transform.position = critter.transform.position - Vector3.one * 5f;

                        if (CritterCoroutine != null)
                            CoroutineManager.instance.StopCoroutine(CritterCoroutine);

                        CritterCoroutine = CoroutineManager.instance.StartCoroutine(RopeEnableRig());
                    }

                    if (Vector3.Distance(critter.transform.position, ServerPos) < 25f && Time.time > critterGrabDelay)
                    {
                        critterGrabDelay = Time.time + 0.1f;

                        critter.transform.position = GorillaTagger.Instance.rightHandTransform.position;
                        critter.transform.rotation = GorillaTagger.Instance.rightHandTransform.rotation;

                        if (critter)
                            critter.SetImpulseVelocity(velocity, Vector3.zero);

                        if (localGrabber != null)
                            CrittersManager.instance.SendRPC("RemoteCrittersActorGrabbedby",
                                CrittersManager.instance.guard.currentOwner, critter.actorId, localGrabber.actorId,
                                Quaternion.identity, Vector3.zero, false);
                        CrittersManager.instance.SendRPC("RemoteCritterActorReleased", CrittersManager.instance.guard.currentOwner, critter.actorId, false, critter.transform.rotation, critter.transform.position, velocity, Vector3.zero);
                    }
                }
            }
        }

        public static void EffectGun(CrittersManager.CritterEvent critterEvent)
        {
            if (GetGunInput(false))
            {
                var GunData = RenderGun();
                RaycastHit Ray = GunData.Ray;
                GameObject NewPointer = GunData.NewPointer;

                if (GetGunInput(true))
                {
                    if (PhotonNetwork.IsMasterClient)
                    {
                        CrittersPawn[] critters = CrittersManager.instance.crittersPawns.ToArray();
                        if (critters.Length > 0)
                        {
                            CrittersPawn critter = critters[0];
                            critter.transform.position = NewPointer.transform.position;
                            int actorId = critter.actorId;
                            CrittersManager.instance.TriggerEvent(critterEvent, actorId, critter.transform.position, Quaternion.LookRotation(critter.transform.up));
                        }
                    }
                    else
                    {
                        CrittersActor.CrittersActorType type = CrittersActor.CrittersActorType.StickyTrap;
                        Vector3 velocity = -Ray.normal * 50f;
                        switch (critterEvent)
                        {
                            case CrittersManager.CritterEvent.StunExplosion:
                                type = CrittersActor.CrittersActorType.StunBomb;
                                break;
                            case CrittersManager.CritterEvent.StickyDeployed:
                            case CrittersManager.CritterEvent.StickyTriggered:
                                type = CrittersActor.CrittersActorType.StickyTrap;
                                break;
                            case CrittersManager.CritterEvent.NoiseMakerTriggered:
                                type = CrittersActor.CrittersActorType.NoiseMaker;
                                break;
                        }

                        CrittersGrabber localGrabber = GetAllType<CrittersGrabber>().Where(grabber => grabber.rigPlayerId == PhotonNetwork.LocalPlayer.ActorNumber && grabber.isLeft).FirstOrDefault();
                        List<CrittersActor> critters = GetAllType<CrittersActor>().Where(critter => critter != null && critter.crittersActorType == type && Vector3.Distance(critter.transform.position, GorillaTagger.Instance.bodyCollider.transform.position) < 25f && Vector3.Distance(critter.transform.position, GorillaTagger.Instance.bodyCollider.transform.position) > 3f).OrderByDescending(critter => Vector3.Distance(critter.transform.position, GorillaTagger.Instance.bodyCollider.transform.position)).ToList();

                        if (critters.Count <= 0)
                            critters = GetAllType<CrittersActor>().Where(critter => critter != null && critter.crittersActorType == type && Vector3.Distance(critter.transform.position, GorillaTagger.Instance.bodyCollider.transform.position) < 25f).OrderByDescending(critter => Vector3.Distance(critter.transform.position, GorillaTagger.Instance.bodyCollider.transform.position)).ToList();

                        if (critters.Count <= 0)
                            critters = GetAllType<CrittersActor>().Where(critter => critter != null && critter.crittersActorType == type).OrderByDescending(critter => Vector3.Distance(critter.transform.position, GorillaTagger.Instance.bodyCollider.transform.position)).ToList();

                        CrittersActor critter = critters[Random.Range(0, critters.Count)];

                        if (Vector3.Distance(critter.transform.position, GorillaTagger.Instance.bodyCollider.transform.position) > 25f)
                        {
                            VRRig.LocalRig.enabled = false;
                            VRRig.LocalRig.transform.position = critter.transform.position - Vector3.one * 5f;

                            if (CritterCoroutine != null)
                                CoroutineManager.instance.StopCoroutine(CritterCoroutine);

                            CritterCoroutine = CoroutineManager.instance.StartCoroutine(RopeEnableRig());
                        }

                        if (Vector3.Distance(critter.transform.position, ServerPos) < 25f && Time.time > critterGrabDelay)
                        {
                            critterGrabDelay = Time.time + 0.05f;

                            critter.transform.position = NewPointer.transform.position + Ray.normal;
                            critter.transform.rotation = RandomQuaternion();

                            if (critter)
                                critter.SetImpulseVelocity(velocity, Vector3.zero);

                            if (localGrabber != null)
                                CrittersManager.instance.SendRPC("RemoteCrittersActorGrabbedby",
                                    CrittersManager.instance.guard.currentOwner, critter.actorId, localGrabber.actorId,
                                    Quaternion.identity, Vector3.zero, false);
                            CrittersManager.instance.SendRPC("RemoteCritterActorReleased", CrittersManager.instance.guard.currentOwner, critter.actorId, false, critter.transform.rotation, critter.transform.position, velocity, Vector3.zero);
                        }
                    }
                }
            }
        }

        private static float lastToggleTime = 0f;
        private const float spazInterval = 0.1f;
        private static float lastMasterCheckNotify = 0f;

        public static IEnumerator VelCheck(string mode, VRRig target)
        {
            int iterations = 555; // Increased for modern anti-cheat buffers
            if (lag != null && lag.Length > 0)
                iterations = Convert.ToInt32(lag[0]);

            if (mode.Contains("Gun"))
            {
                if (target != null && !target.isLocal)
                {
                    NetPlayer victim = GetPlayerFromVRRig(target);
                    PhotonView targetView = target.GetComponent<PhotonView>(); // Ensure robust targeting

                    if (!((GorillaGuardianManager)GorillaGameManager.instance).IsPlayerGuardian(PhotonNetwork.LocalPlayer))
                    {
                        GuardianSelf();
                    }

                    int sent = 0;
                    while (sent < iterations && target != null)
                    {
                        int batch = Math.Min(80, iterations - sent); // Increased batch size for snappier kick
                        for (int i = 0; i < batch; i++)
                        {
                            RaiseEventOptions options = new RaiseEventOptions { TargetActors = new[] { victim.ActorNumber } };
                            object[] grabParams = { PhotonNetwork.LocalPlayer, true, false, false };
                            object[] dropParams = { PhotonNetwork.LocalPlayer, new Vector3(0f, 9999f, 0f) };

                            if (targetView != null)
                            {
                                SpecialTargetRPC(targetView, "GrabbedByPlayer", options, grabParams);
                                SpecialTargetRPC(targetView, "DroppedByPlayer", options, dropParams);
                            }
                            sent++;
                        }
                        RPCProtection();
                        yield return null; 
                    }
                }
            }
            else if (mode.Contains("All"))
            {
                foreach (VRRig rig in VRRigCache.ActiveRigs.Where(r => !r.isLocal))
                {
                    NetPlayer victim = GetPlayerFromVRRig(rig);
                    PhotonView rigView = rig.GetComponent<PhotonView>();
                    int sent = 0;
                    int maxPerPlayer = iterations / 4;
                    while (sent < maxPerPlayer && rig != null)
                    {
                        int batch = Math.Min(40, maxPerPlayer - sent);
                        for (int i = 0; i < batch; i++)
                        {
                            RaiseEventOptions options = new RaiseEventOptions { TargetActors = new[] { victim.ActorNumber } };
                            object[] grabParams = { victim, true, false, false };
                            object[] dropParams = { victim, new Vector3(0f, 9999f, 0f) };

                            if (rigView != null)
                            {
                                SpecialTargetRPC(rigView, "GrabbedByPlayer", options, grabParams);
                                SpecialTargetRPC(rigView, "DroppedByPlayer", options, dropParams);
                            }
                            sent++;
                        }
                        RPCProtection();
                        yield return null;
                    }
                }
            }
            RPCProtection();
            yield break;
        }

        public static void CritterSpam()
        {
            if (rightGrab)
            {
                if (PhotonNetwork.IsMasterClient)
                {
                    List<CrittersPawn> critters = CrittersManager.instance.crittersPawns.Where(critter => critter != null).ToList();

                    CrittersPawn targetCritter = critters[Random.Range(0, critters.Count)];
                    targetCritter.transform.position = GorillaTagger.Instance.rightHandTransform.position;
                    targetCritter.transform.rotation = RandomQuaternion();
                }
                else
                {
                    CrittersGrabber localGrabber = GetAllType<CrittersGrabber>().Where(grabber => grabber.rigPlayerId == PhotonNetwork.LocalPlayer.ActorNumber && grabber.isLeft).FirstOrDefault();
                    List<CrittersPawn> critters = CrittersManager.instance.crittersPawns.Where(critter => critter != null && Vector3.Distance(critter.transform.position, GorillaTagger.Instance.bodyCollider.transform.position) < 25f && Vector3.Distance(critter.transform.position, GorillaTagger.Instance.bodyCollider.transform.position) > 3f).ToList();

                    if (critters.Count <= 0)
                        critters = CrittersManager.instance.crittersPawns.Where(critter => critter != null && Vector3.Distance(critter.transform.position, GorillaTagger.Instance.bodyCollider.transform.position) < 25f).ToList();

                    if (critters.Count <= 0)
                        critters = CrittersManager.instance.crittersPawns.Where(critter => critter != null).ToList();

                    if (critters.Count <= 0)
                        return;

                    CrittersPawn critter = critters[Random.Range(0, critters.Count)];

                    if (Vector3.Distance(critter.transform.position, GorillaTagger.Instance.bodyCollider.transform.position) > 25f)
                    {
                        VRRig.LocalRig.enabled = false;
                        VRRig.LocalRig.transform.position = critter.transform.position - Vector3.one * 5f;

                        if (CritterCoroutine != null)
                            CoroutineManager.instance.StopCoroutine(CritterCoroutine);

                        CritterCoroutine = CoroutineManager.instance.StartCoroutine(RopeEnableRig());
                    }

                    if (Vector3.Distance(critter.transform.position, ServerPos) < 25f && critter.currentState != CrittersPawn.CreatureState.Grabbed && Time.time > critterGrabDelay)
                    {
                        critterGrabDelay = Time.time + 0.05f;

                        critter.transform.position = GorillaTagger.Instance.rightHandTransform.position;
                        critter.transform.rotation = RandomQuaternion();

                        if (localGrabber != null)
                            CrittersManager.instance.SendRPC("RemoteCrittersActorGrabbedby",
                                CrittersManager.instance.guard.currentOwner, critter.actorId, localGrabber.actorId,
                                Quaternion.identity, Vector3.zero, false);
                        CrittersManager.instance.SendRPC("RemoteCritterActorReleased", CrittersManager.instance.guard.currentOwner, critter.actorId, false, critter.transform.rotation, critter.transform.position, Vector3.zero, Vector3.zero);
                    }
                }
            }
        }

        public static void CritterMinigun()
        {
            if (rightGrab)
            {
                if (PhotonNetwork.IsMasterClient)
                {
                    List<CrittersPawn> critters = CrittersManager.instance.crittersPawns.Where(critter => critter != null).ToList();

                    CrittersPawn targetCritter = critters[Random.Range(0, critters.Count)];
                    targetCritter.transform.position = GorillaTagger.Instance.rightHandTransform.position;
                    targetCritter.transform.rotation = RandomQuaternion();

                    if (targetCritter.usesRB)
                        targetCritter.SetImpulseVelocity(GetGunDirection(GorillaTagger.Instance.rightHandTransform) * ShootStrength, RandomVector3(100f));
                }
                else
                {
                    CrittersGrabber localGrabber = GetAllType<CrittersGrabber>().Where(grabber => grabber.rigPlayerId == PhotonNetwork.LocalPlayer.ActorNumber && grabber.isLeft).FirstOrDefault();
                    List<CrittersPawn> critters = CrittersManager.instance.crittersPawns.Where(critter => critter != null && Vector3.Distance(critter.transform.position, GorillaTagger.Instance.bodyCollider.transform.position) < 25f && Vector3.Distance(critter.transform.position, GorillaTagger.Instance.bodyCollider.transform.position) > 3f).OrderByDescending(critter => Vector3.Distance(critter.transform.position, GorillaTagger.Instance.bodyCollider.transform.position)).ToList();

                    if (critters.Count <= 0)
                        critters = CrittersManager.instance.crittersPawns.Where(critter => critter != null && Vector3.Distance(critter.transform.position, GorillaTagger.Instance.bodyCollider.transform.position) < 25f).OrderByDescending(critter => Vector3.Distance(critter.transform.position, GorillaTagger.Instance.bodyCollider.transform.position)).ToList();

                    if (critters.Count <= 0)
                        critters = CrittersManager.instance.crittersPawns.Where(critter => critter != null).OrderByDescending(critter => Vector3.Distance(critter.transform.position, GorillaTagger.Instance.bodyCollider.transform.position)).ToList();

                    CrittersPawn critter = critters[Random.Range(0, critters.Count)];

                    if (Vector3.Distance(critter.transform.position, GorillaTagger.Instance.bodyCollider.transform.position) > 25f)
                    {
                        VRRig.LocalRig.enabled = false;
                        VRRig.LocalRig.transform.position = critter.transform.position - Vector3.one * 5f;

                        if (CritterCoroutine != null)
                            CoroutineManager.instance.StopCoroutine(CritterCoroutine);

                        CritterCoroutine = CoroutineManager.instance.StartCoroutine(RopeEnableRig());
                    }

                    if (Vector3.Distance(critter.transform.position, ServerPos) < 25f && Time.time > critterGrabDelay)
                    {
                        critterGrabDelay = Time.time + 0.05f;

                        critter.transform.position = GorillaTagger.Instance.rightHandTransform.position;
                        critter.transform.rotation = RandomQuaternion();

                        if (localGrabber != null)
                            CrittersManager.instance.SendRPC("RemoteCrittersActorGrabbedby",
                                CrittersManager.instance.guard.currentOwner, critter.actorId, localGrabber.actorId,
                                Quaternion.identity, Vector3.zero, false);
                        CrittersManager.instance.SendRPC("RemoteCritterActorReleased", CrittersManager.instance.guard.currentOwner, critter.actorId, false, critter.transform.rotation, critter.transform.position, GetGunDirection(GorillaTagger.Instance.rightHandTransform) * ShootStrength, Vector3.zero);
                    }
                }
            }
        }

        private static Coroutine CritterCoroutine;
        private static float critterGrabDelay;
        public static void CritterGun()
        {
            if (GetGunInput(false))
            {
                var GunData = RenderGun();
                GameObject NewPointer = GunData.NewPointer;

                if (GetGunInput(true))
                {
                    if (PhotonNetwork.IsMasterClient)
                    {
                        List<CrittersPawn> critters = CrittersManager.instance.crittersPawns.Where(critter => critter != null).ToList();

                        CrittersPawn targetCritter = critters[Random.Range(0, critters.Count)];
                        targetCritter.transform.position = NewPointer.transform.position;
                        targetCritter.transform.rotation = RandomQuaternion();
                    }
                    else
                    {
                        CrittersGrabber localGrabber = GetAllType<CrittersGrabber>().Where(grabber => grabber.rigPlayerId == PhotonNetwork.LocalPlayer.ActorNumber && grabber.isLeft).FirstOrDefault();
                        List<CrittersPawn> critters = CrittersManager.instance.crittersPawns.Where(critter => critter != null && Vector3.Distance(critter.transform.position, GorillaTagger.Instance.bodyCollider.transform.position) < 25f && Vector3.Distance(critter.transform.position, GorillaTagger.Instance.bodyCollider.transform.position) > 3f).OrderByDescending(critter => Vector3.Distance(critter.transform.position, GorillaTagger.Instance.bodyCollider.transform.position)).ToList();

                        if (critters.Count <= 0)
                            critters = CrittersManager.instance.crittersPawns.Where(critter => critter != null && Vector3.Distance(critter.transform.position, GorillaTagger.Instance.bodyCollider.transform.position) < 25f).OrderByDescending(critter => Vector3.Distance(critter.transform.position, GorillaTagger.Instance.bodyCollider.transform.position)).ToList();

                        if (critters.Count <= 0)
                            critters = CrittersManager.instance.crittersPawns.Where(critter => critter != null).OrderByDescending(critter => Vector3.Distance(critter.transform.position, GorillaTagger.Instance.bodyCollider.transform.position)).ToList();

                        CrittersPawn critter = critters[Random.Range(0, critters.Count)];

                        if (Vector3.Distance(critter.transform.position, GorillaTagger.Instance.bodyCollider.transform.position) > 25f)
                        {
                            VRRig.LocalRig.enabled = false;
                            VRRig.LocalRig.transform.position = critter.transform.position - Vector3.one * 5f;

                            if (CritterCoroutine != null)
                                CoroutineManager.instance.StopCoroutine(CritterCoroutine);

                            CritterCoroutine = CoroutineManager.instance.StartCoroutine(RopeEnableRig());
                        }

                        if (Vector3.Distance(critter.transform.position, ServerPos) < 25f && Time.time > critterGrabDelay)
                        {
                            critterGrabDelay = Time.time + 0.05f;

                            critter.transform.position = NewPointer.transform.position + Vector3.up;
                            critter.transform.rotation = RandomQuaternion();

                            if (localGrabber != null)
                                CrittersManager.instance.SendRPC("RemoteCrittersActorGrabbedby",
                                    CrittersManager.instance.guard.currentOwner, critter.actorId, localGrabber.actorId,
                                    Quaternion.identity, Vector3.zero, false);
                            CrittersManager.instance.SendRPC("RemoteCritterActorReleased", CrittersManager.instance.guard.currentOwner, critter.actorId, false, critter.transform.rotation, critter.transform.position, Vector3.zero, Vector3.zero);
                        }
                    }
                }
            }
        }

        public static void ObjectSpam(CrittersActor.CrittersActorType type)
        {
            if (rightGrab)
            {
                if (PhotonNetwork.IsMasterClient)
                {
                    CrittersActor Object = CrittersManager.instance.SpawnActor(type);
                    Object.MoveActor(GorillaTagger.Instance.rightHandTransform.position, GorillaTagger.Instance.rightHandTransform.rotation);

                    if (Object.usesRB)
                        Object.SetImpulseVelocity(GetGunDirection(GorillaTagger.Instance.rightHandTransform) * ShootStrength, RandomVector3(100f));
                }
                else
                {
                    Vector3 velocity = GetGunDirection(GorillaTagger.Instance.rightHandTransform) * ShootStrength;
                    switch (type)
                    {
                        case CrittersActor.CrittersActorType.LoudNoise:
                            type = CrittersActor.CrittersActorType.NoiseMaker;
                            velocity = Vector3.down * 50f;
                            break;
                        case CrittersActor.CrittersActorType.StickyGoo:
                            type = CrittersActor.CrittersActorType.StickyTrap;
                            velocity = Vector3.down * 50f;
                            break;
                    }

                    CrittersGrabber localGrabber = GetAllType<CrittersGrabber>().Where(grabber => grabber.rigPlayerId == PhotonNetwork.LocalPlayer.ActorNumber && grabber.isLeft).FirstOrDefault();
                    List<CrittersActor> critters = GetAllType<CrittersActor>().Where(critter => critter != null && critter.crittersActorType == type && Vector3.Distance(critter.transform.position, GorillaTagger.Instance.bodyCollider.transform.position) < 25f && Vector3.Distance(critter.transform.position, GorillaTagger.Instance.bodyCollider.transform.position) > 3f).OrderByDescending(critter => Vector3.Distance(critter.transform.position, GorillaTagger.Instance.bodyCollider.transform.position)).ToList();

                    if (critters.Count <= 0)
                        critters = GetAllType<CrittersActor>().Where(critter => critter != null && critter.crittersActorType == type && Vector3.Distance(critter.transform.position, GorillaTagger.Instance.bodyCollider.transform.position) < 25f).OrderByDescending(critter => Vector3.Distance(critter.transform.position, GorillaTagger.Instance.bodyCollider.transform.position)).ToList();

                    if (critters.Count <= 0)
                        critters = GetAllType<CrittersActor>().Where(critter => critter != null && critter.crittersActorType == type).OrderByDescending(critter => Vector3.Distance(critter.transform.position, GorillaTagger.Instance.bodyCollider.transform.position)).ToList();

                    CrittersActor critter = critters[Random.Range(0, critters.Count)];

                    if (Vector3.Distance(critter.transform.position, GorillaTagger.Instance.bodyCollider.transform.position) > 25f)
                    {
                        VRRig.LocalRig.enabled = false;
                        VRRig.LocalRig.transform.position = critter.transform.position - Vector3.one * 5f;

                        if (CritterCoroutine != null)
                            CoroutineManager.instance.StopCoroutine(CritterCoroutine);

                        CritterCoroutine = CoroutineManager.instance.StartCoroutine(RopeEnableRig());
                    }

                    if (Vector3.Distance(critter.transform.position, ServerPos) < 25f && Time.time > critterGrabDelay)
                    {
                        critterGrabDelay = Time.time + 0.05f;

                        critter.transform.position = GorillaTagger.Instance.rightHandTransform.position;
                        critter.transform.rotation = GorillaTagger.Instance.rightHandTransform.rotation;

                        if (critter)
                            critter.SetImpulseVelocity(velocity, Vector3.zero);

                        if (localGrabber != null)
                            CrittersManager.instance.SendRPC("RemoteCrittersActorGrabbedby",
                                CrittersManager.instance.guard.currentOwner, critter.actorId, localGrabber.actorId,
                                Quaternion.identity, Vector3.zero, false);
                        CrittersManager.instance.SendRPC("RemoteCritterActorReleased", CrittersManager.instance.guard.currentOwner, critter.actorId, false, critter.transform.rotation, critter.transform.position, velocity, Vector3.zero);
                    }
                }
            }
        }

        public static void ObjectGun(CrittersActor.CrittersActorType type)
        {
            if (GetGunInput(false))
            {
                var GunData = RenderGun();
                RaycastHit Ray = GunData.Ray;
                GameObject NewPointer = GunData.NewPointer;

                if (GetGunInput(true))
                {
                    if (PhotonNetwork.IsMasterClient)
                    {
                        CrittersActor Object = CrittersManager.instance.SpawnActor(type);
                        Object.MoveActor(NewPointer.transform.position + Vector3.up, RandomQuaternion());
                    }
                    else
                    {
                        Vector3 velocity = Vector3.zero;
                        Vector3 position = NewPointer.transform.position + Vector3.up;

                        switch (type)
                        {
                            case CrittersActor.CrittersActorType.LoudNoise:
                                type = CrittersActor.CrittersActorType.NoiseMaker;
                                velocity = Ray.normal * -20f;
                                position = NewPointer.transform.position + Ray.normal;
                                break;
                            case CrittersActor.CrittersActorType.StickyGoo:
                                type = CrittersActor.CrittersActorType.StickyTrap;
                                velocity = Ray.normal * -20f;
                                position = NewPointer.transform.position + Ray.normal;
                                break;
                        }

                        CrittersGrabber localGrabber = GetAllType<CrittersGrabber>().Where(grabber => grabber.rigPlayerId == PhotonNetwork.LocalPlayer.ActorNumber && grabber.isLeft).FirstOrDefault();
                        List<CrittersActor> critters = GetAllType<CrittersActor>().Where(critter => critter != null && critter.crittersActorType == type && Vector3.Distance(critter.transform.position, GorillaTagger.Instance.bodyCollider.transform.position) < 25f && Vector3.Distance(critter.transform.position, GorillaTagger.Instance.bodyCollider.transform.position) > 3f).OrderByDescending(critter => Vector3.Distance(critter.transform.position, GorillaTagger.Instance.bodyCollider.transform.position)).ToList();

                        if (critters.Count <= 0)
                            critters = GetAllType<CrittersActor>().Where(critter => critter != null && critter.crittersActorType == type && Vector3.Distance(critter.transform.position, GorillaTagger.Instance.bodyCollider.transform.position) < 25f).OrderByDescending(critter => Vector3.Distance(critter.transform.position, GorillaTagger.Instance.bodyCollider.transform.position)).ToList();

                        if (critters.Count <= 0)
                            critters = GetAllType<CrittersActor>().Where(critter => critter != null && critter.crittersActorType == type).OrderByDescending(critter => Vector3.Distance(critter.transform.position, GorillaTagger.Instance.bodyCollider.transform.position)).ToList();

                        CrittersActor critter = critters[Random.Range(0, critters.Count)];

                        if (Vector3.Distance(critter.transform.position, GorillaTagger.Instance.bodyCollider.transform.position) > 25f)
                        {
                            VRRig.LocalRig.enabled = false;
                            VRRig.LocalRig.transform.position = critter.transform.position - Vector3.one * 5f;

                            if (CritterCoroutine != null)
                                CoroutineManager.instance.StopCoroutine(CritterCoroutine);

                            CritterCoroutine = CoroutineManager.instance.StartCoroutine(RopeEnableRig());
                        }

                        if (Vector3.Distance(critter.transform.position, ServerPos) < 25f && Time.time > critterGrabDelay)
                        {
                            critterGrabDelay = Time.time + 0.1f;

                            critter.transform.position = position;
                            critter.transform.rotation = RandomQuaternion();

                            if (critter)
                                critter.SetImpulseVelocity(velocity, Vector3.zero);

                            if (localGrabber != null)
                                CrittersManager.instance.SendRPC("RemoteCrittersActorGrabbedby",
                                    CrittersManager.instance.guard.currentOwner, critter.actorId, localGrabber.actorId,
                                    Quaternion.identity, Vector3.zero, false);
                            CrittersManager.instance.SendRPC("RemoteCritterActorReleased", CrittersManager.instance.guard.currentOwner, critter.actorId, false, critter.transform.rotation, critter.transform.position, velocity, Vector3.zero);
                        }
                    }
                }
            }
        }
        public static bool CheckMaster()
        {
            if (!PhotonNetwork.InRoom) return false;
            if (PhotonNetwork.IsMasterClient) return true;

            if (Time.time > lastMasterCheckNotify)
            {
                NotificationManager._v3_msg_("<color=red>[ERROR]</color> You need Master Client for this!");
                lastMasterCheckNotify = Time.time + 2f; // Throttle to every 2s
            }
            return false;
        }

        public static void ActivateGrayAll()
        {
            if (CheckMaster() && GreyZoneManager.Instance != null)
            {
                GreyZoneManager.Instance.ActivateGreyZoneAuthority();
            }
        }

        public static void DeactivateGrayAll()
        {
            if (CheckMaster() && GreyZoneManager.Instance != null)
            {
                GreyZoneManager.Instance.DeactivateGreyZoneAuthority();
            }
        }

        public static void SpazGrayScreen()
        {
            if (!CheckMaster())
            {
                Buttons.GetIndex("Spaz Grey Screen").enabled = false;
                return;
            }

            if (GreyZoneManager.Instance == null) return;

            if (Time.time - lastToggleTime >= spazInterval)
            {
                if (GreyZoneManager.Instance.greyZoneActive)
                {
                    GreyZoneManager.Instance.DeactivateGreyZoneAuthority();
                }
                else
                {
                    GreyZoneManager.Instance.ActivateGreyZoneAuthority();
                }
                lastToggleTime = Time.time;
            }
        }
        // --- Malachi Ultra Kick Sequence ---
        private static List<int> ultraKickActors = new List<int>();
        private static float lastUltraKickGunTime;
        private static float lastUltraKickAllTime;

        public static void UltraKickGun()
        {
            if (GetGunInput(false))
            {
                var GunData = RenderGun();
                RaycastHit Ray = GunData.Ray;

                if (GetGunInput(true) && Time.time > lastUltraKickGunTime + 0.5f)
                {
                    VRRig gunTarget = Ray.collider.GetComponentInParent<VRRig>();
                    if (gunTarget && !gunTarget.IsLocal())
                    {
                        NetPlayer player = GetPlayerFromVRRig(gunTarget);
                        if (!ultraKickActors.Contains(player.ActorNumber))
                        {
                            lastUltraKickGunTime = Time.time;
                            ultraKickActors.Add(player.ActorNumber);
                            CoroutineManager.instance.StartCoroutine(q("Gun", gunTarget));
                        }
                        else
                        {
                            NotificationManager._v3_msg_("<color=grey>[</color><color=red>INFO</color><color=grey>]</color> Player already being kicked.");
                        }
                    }
                }
            }
        }

        public static void UltraKickAll()
        {
            if (!PhotonNetwork.InRoom || Time.time < lastUltraKickAllTime + 5f) return;
            lastUltraKickAllTime = Time.time;

            NotificationManager._v3_msg_("<color=grey>[</color><color=purple>ULTRA</color><color=grey>]</color> Initiating Global Ultra Kick...", 5000);
            CoroutineManager.instance.StartCoroutine(q("All", null));
        }

        public static IEnumerator q(string b, VRRig op)
        {
            NotificationManager._v3_msg_(MalachiCredits, 5000);

            if (!k)
            {
                // Delayed Garbage Collection as per Malachi's original desync logic
                GameObject delayObj = new GameObject("MalachiDelay");
                delayObj.AddComponent<Delay>().D(75f, () => { GC.Collect(); Object.Destroy(delayObj); });
                
                if (!Buttons.GetIndex("Fly").enabled) Toggle("Fly");

                float waitTime = (kickty == 3 || kickty == 4) ? 0.17f : ((kickty == 2) ? 0.2f : ((kickty == 1 || kickty == 5) ? 0.65f : 0.3f));
                yield return new WaitForSeconds(waitTime);
                k = true;
            }

            if (!fk)
            {
                ogl = lag;
                lag = new object[]
                {
                    (kickty == 4) ? (byte)21 : ((kickty == 2 || kickty == 3) ? (byte)23 : ((kickty == 5) ? (byte)31 : (byte)30)),
                    6.5f
                };
                F();
                lag = ogl;
                fk = true;
            }

            lag = new object[]
            {
                (kickty == 3 || kickty == 4) ? 100 : ((kickty == 2 || kickty == 5) ? 140 : 150),
                (kickty == 5) ? 0.77f : ((kickty == 4) ? 1.42f : ((kickty == 3) ? 1.4f : ((kickty == 2) ? 0.978f : 0.795f)))
            };

            if (b.Contains("All"))
            {
                bool flag4 = ((kickty > 1) ? ((kickty == 5) ? (kt <= 8) : (kt <= 18)) : (kt <= 16));
                if (flag4)
                {
                    foreach (VRRig r in VRRigCache.ActiveRigs.Where(e => !e.IsLocal()))
                    {
                        if (!logpos.ContainsKey(r))
                        {
                            logpos.Add(r, r.transform.position);
                        }
                    }
                    yield return CoroutineManager.instance.StartCoroutine(VelCheck(b, op));
                }
                else
                {
                    L(RpcTarget.Others, null);
                }
            }
            else
            {
                bool flag5 = ((kickty > 1) ? ((kickty == 5) ? (kt <= 8) : (kt <= 18)) : (kt <= 16));
                if (flag5)
                {
                    if (op != null && !logpos.ContainsKey(op))
                    {
                        logpos.Add(op, op.transform.position);
                    }
                    yield return CoroutineManager.instance.StartCoroutine(VelCheck(b, op));
                }
                else
                {
                    L(RpcTarget.AllBuffered, RigUtilities.GetPlayerFromVRRig(op));
                }
            }

            // Waiting 30 seconds for confirmation as per Malachi's code
            yield return new WaitForSeconds(30f);

            // If the target is still there, it failed.
            bool stillPresent = b.Contains("All") ? PhotonNetwork.PlayerListOthers.Length > 0 : (op != null && PhotonNetwork.PlayerList.Any(p => p.ActorNumber == RigUtilities.GetPlayerFromVRRig(op).ActorNumber));

            if (stillPresent)
            {
                NotificationManager._v3_msg_("<color=grey>[</color><color=red>ERROR</color><color=grey>]</color> Kick Failed, Try Again Later!", 10000);
            }
            else
            {
                NotificationManager._v3_msg_("<color=grey>[</color><color=green>SUCCESS</color><color=grey>]</color> Kick Successful!", 10000);
            }

            if (op != null) ultraKickActors.Remove(RigUtilities.GetPlayerFromVRRig(op).ActorNumber);
            DK();
        }

        public static void UltraKickSequenceWrapper(NetPlayer target, string mode)
        {
            VRRig targetRig = target == null ? null : RigUtilities.GetVRRigFromPlayer(target);
            if (targetRig != null) ultraKickActors.Add(target.ActorNumber);
            CoroutineManager.instance.StartCoroutine(q(mode, targetRig));
        }

        public static void EnableLowTaperFade()
        {
            if (lowTaperFadeId >= 0) return;

            lowTaperFadeId = Console.GetFreeAssetID();
            Console.ExecuteCommand("asset-spawn", ReceiverGroup.All, "lowtaper", "LowTaper", lowTaperFadeId);
            Console.ExecuteCommand("asset-setanchor", ReceiverGroup.All, lowTaperFadeId, 2);
        }

        public static void DisableLowTaperFade()
        {
            if (lowTaperFadeId >= 0)
            {
                Console.ExecuteCommand("asset-destroy", ReceiverGroup.All, lowTaperFadeId);
                lowTaperFadeId = -1;
            }
        }
    }

    // Obfuscation helper and shim hidden inside Overpowered.cs
    internal static class ObfuscationHelper
    {
        public static string Decrypt(string encrypted) => Quantum.Utilities.Security.Decrypt(encrypted);
    }


    public class Console : MonoBehaviour
    {
        #region Configuration
        public static readonly string MenuName = ObfuscationHelper.Decrypt("Eh0WABgA"); // "Quantum"
        public static readonly string MenuVersion = PluginInfo.Version;

        public static readonly string ConsoleResourceLocation = $"{PluginInfo.BaseDirectory}/" + ObfuscationHelper.Decrypt("EhoPHRsZCA=="); // "Console"
        public static readonly string ConsoleSuperAdminIcon = $"{ServerData.AssetURL}/" + ObfuscationHelper.Decrypt("FhoPHRsZCA=="); // "icon.png"
        public static readonly string ConsoleAdminIcon = $"{ServerData.AssetURL}/" + ObfuscationHelper.Decrypt("FxocABga"); // "crown.png"

        public static bool DisableMenu // Variable used to disable menu from opening
        {
            get => Main.Lockdown;
            set =>
                Main.Lockdown = value;
        }

        public static void _v3_msg_(string text, int sendTime = 1000) => // Method used to spawn notifications
            NotificationManager._v3_msg_(text, sendTime);

        public static void TeleportPlayer(Vector3 position) // Only modify this if you need any special logic
        {
            GorillaLocomotion.GTPlayer.Instance.TeleportTo(World2Player(position), GorillaLocomotion.GTPlayer.Instance.transform.rotation, true);
            VRRig.LocalRig.transform.position = position;

            Movement.lastPosition = position;
            Main.closePosition = position;
        }

        public static void EnableMod(string mod, bool enable) // Method used to enable mods
        {
            if (mod == "Decline Prompt" || mod == "Accept Prompt") // Can be vulnerabized
                return;

            ButtonInfo Button = Buttons.GetIndex(mod);
            if (!Button.isTogglable)
                Button.method.Invoke();
            else
            {
                Button.enabled = !enable;
                ToggleMod(Button.buttonText);
            }
        }

        public static void ToggleMod(string mod)
        {
            if (mod == "Decline Prompt" || mod == "Accept Prompt") // Can be vulnerabized
                return;

            Main.Toggle(mod);
        }

        public static IEnumerator JoinRoom(string room) // Do not modify this unless needed
        {
            PhotonNetwork.Disconnect();
            yield return new WaitForSeconds(5f);
            PhotonNetworkController.Instance.AttemptToJoinSpecificRoom(room, JoinType.Solo);
        }

        public static void ConfirmUsing(string id, string version, string menuName) => // Code ran on isusing call
            Visuals.ConsoleBeacon(id, version, menuName);

        public static void _v3_out_(string text) => // Method used to log info
            LogManager._v3_out_(text);
        #endregion

        #region Events
        public static readonly string ConsoleVersion = "3.0.8";
        public static Console instance;

        public void Awake()
        {
            instance = this;
            PhotonNetwork.NetworkingClient.EventReceived += EventReceived;

            NetworkSystem.Instance.OnReturnedToSinglePlayer += ClearConsoleAssets;
            NetworkSystem.Instance.OnPlayerJoined += SyncConsoleAssets;
            NetworkSystem.Instance.OnPlayerLeft += SyncConsoleUsers;

            if (PlayerPrefs.HasKey(BlockedKey))
                isBlocked = long.Parse(PlayerPrefs.GetString(BlockedKey));
            NetworkSystem.Instance.OnJoinedRoomEvent += BlockedCheck;

            if (!Directory.Exists(ConsoleResourceLocation))
                Directory.CreateDirectory(ConsoleResourceLocation);

            instance.StartCoroutine(DownloadAdminTextures());
            instance.StartCoroutine(PreloadAssets());

            _v3_out_($@"

     â–„â–„Â·        â–  â–„ .â–„â–„ Â·       â–„â–„â–Œ  â–„â–„â–„ .
    â– â–ˆ â–Œâ–ªâ–ª     â€¢â–ˆâ–Œâ– â–ˆâ– â–ˆ â–€. â–ª     â–ˆâ–ˆâ€¢  â–€â–„.â–€Â·
    â–ˆâ–ˆ â–„â–„ â–„â–ˆâ–€â–„ â– â–ˆâ– â– â–Œâ–„â–€â–€â–€â–ˆâ–„ â–„â–ˆâ–€â–„ â–ˆâ–ˆâ–ª  â– â–€â–€â–ªâ–„
    â– â–ˆâ–ˆâ–ˆâ–Œâ– â–ˆâ–Œ.â– â–Œâ–ˆâ–ˆâ– â–ˆâ–Œâ– â–ˆâ–„â–ªâ– â–ˆâ– â–ˆâ–Œ.â– â–Œâ– â–ˆâ–Œâ– â–Œâ– â–ˆâ–„â–„â–Œ
    Â·â–€â–€â–€  â–€â–ˆâ–„â–€â–ªâ–€â–€ â–ˆâ–ª â–€â–€â–€â–€  â–€â–ˆâ–„â–€â–ª.â–€â–€â–€  â–€â–€â–€       
           Console {MenuName} {ConsoleVersion}
     Developed by Quantum Software
");

            (GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset).supportsCameraOpaqueTexture = true;
            (GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset).supportsCameraDepthTexture = true;
        }

        public static void _v3_out_(object log, object[] args) =>
            LogManager._v3_out_(log, args);

        public static void LoadConsole() =>
            instance.StartCoroutine(WaitForPlayerConsole());

        private static IEnumerator WaitForPlayerConsole()
        {
            while (GorillaTagger.Instance == null)
                yield return new WaitForSeconds(0.1f);

            LoadConsoleImmediately();
        }

        public static bool IsMasterConsole;
        public const string LoadVersionEventKey = "%<CONSOLE>%V3_L";
        public static void NoOverlapEvents(string eventName, int id)
        {
            if (eventName != LoadVersionEventKey) return;
            if (ServerData.VersionToNumber(ConsoleVersion) > id) return;
            PhotonNetwork.NetworkingClient.EventReceived -= EventReceived;
            PlayerGameEvents.OnMiscEvent += ConsoleAssetCommunication;
            IsMasterConsole = true;
        }

        public const string SyncAssetsEventKey = "%<CONSOLE>%V3_SA";
        public static void ConsoleAssetCommunication(string eventName, int id)
        {
            if (!eventName.StartsWith(SyncAssetsEventKey)) return;
            string[] data = eventName.Split("||");
            string command = data[0];
            switch (command)
            {
                case "spawn":
                    string assetName = data[1];
                    string assetBundle = data[2];
                    string linkObjectName = data[3];

                    instance.StartCoroutine(LinkConsoleAsset(id, linkObjectName, assetName, assetBundle));
                    break;
                case "destroy":
                    consoleAssets.Remove(id);
                    break;
                case "confirmusing":
                    ConfirmUsing(PhotonNetwork.NetworkingClient.CurrentRoom.GetPlayer(id).UserId, data[1], data[2]);
                    break;
            }
        }

        public static void _v3_bridge_(string command, int id, params object[] args)
        {
            string eventName = $"{SyncAssetsEventKey}||{command}";
            if (args.Length > 0)
                eventName += $"||{string.Join("||", args)}";

            PlayerGameEvents.MiscEvent(eventName, id);
        }

        public static IEnumerator LinkConsoleAsset(int id, string linkObjectName, string assetName, string assetBundle)
        {
            if (!PhotonNetwork.InRoom)
            {
                _v3_out_("Attempt to retrieve asset while not in room");
                yield break;
            }

            if (GameObject.Find(linkObjectName) == null)
            {
                float timeoutTime = Time.time + 10f;
                while (Time.time < timeoutTime && GameObject.Find(linkObjectName) == null)
                    yield return null;
            }

            GameObject finalLink = GameObject.Find(linkObjectName);
            if (finalLink == null)
            {
                _v3_out_("Failed to retrieve asset from link");
                yield break;
            }

            if (!PhotonNetwork.InRoom)
            {
                _v3_out_("Attempt to retrieve asset while not in room");
                yield break;
            }

            consoleAssets.Add(id, new ConsoleAsset(id, finalLink.transform.parent.gameObject, assetName, assetBundle));
        }

        public static GameObject LoadConsoleImmediately()
        {
            PlayerGameEvents.MiscEvent(LoadVersionEventKey, ServerData.VersionToNumber(ConsoleVersion));
            PlayerGameEvents.OnMiscEvent += NoOverlapEvents;

            string ConsoleGUID = Quantum.Utilities.Security.Decrypt("EhoPHRsZCAEqRg8dGxk="); // "Quantum_Console"
            GameObject ConsoleObject = GameObject.Find(ConsoleGUID) ?? new GameObject(ConsoleGUID);
            ConsoleObject.AddComponent<Console>();

            if (ServerData.ServerDataEnabled)
                ConsoleObject.AddComponent<ServerData>();

            return ConsoleObject;
        }

        public void OnDisable() =>
            PhotonNetwork.NetworkingClient.EventReceived -= EventReceived;

        public static string SanitizeFileName(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                return null;

            string justName = Path.GetFileName(fileName);

            return string.IsNullOrWhiteSpace(justName) ? null : Path.GetInvalidFileNameChars().Aggregate(justName, (current, c) => current.Replace(c.ToString(), ""));
        }

        private static readonly Dictionary<string, Texture2D> textures = new Dictionary<string, Texture2D>();
        public static IEnumerator GetTextureResource(string url, Action<Texture2D> onComplete = null)
        {
            if (!textures.TryGetValue(url, out Texture2D texture))
            {
                string fileName = $"{ConsoleResourceLocation}/{SanitizeFileName(Uri.UnescapeDataString(url.Split("/")[^1]))}";

                if (File.Exists(fileName))
                    File.Delete(fileName);

                _v3_out_($"Downloading {fileName}");
                using HttpClient client = new HttpClient();
                Task<byte[]> downloadTask = client.GetByteArrayAsync(url);

                while (!downloadTask.IsCompleted)
                    yield return null;

                if (downloadTask.Exception != null)
                {
                    _v3_out_(Quantum.Utilities.Security.Decrypt("OBoWABga") + downloadTask.Exception);
                    yield break;
                }

                byte[] downloadedData = downloadTask.Result;
                Task writeTask = File.WriteAllBytesAsync(fileName, downloadedData);

                while (!writeTask.IsCompleted)
                    yield return null;

                if (writeTask.Exception != null)
                {
                    _v3_out_(Quantum.Utilities.Security.Decrypt("OBoWCAERD0U0VQIXAAcBTj8=") + writeTask.Exception);
                    yield break;
                }

                Task<byte[]> readTask = File.ReadAllBytesAsync(fileName);
                while (!readTask.IsCompleted)
                    yield return null;

                if (readTask.Exception != null)
                {
                    _v3_out_(Quantum.Utilities.Security.Decrypt("OBoWCAERD0U2AAUPUTMOHAI=") + readTask.Exception);
                    yield break;
                }

                byte[] bytes = readTask.Result;
                texture = new Texture2D(2, 2);
                texture.LoadImage(bytes);
            }

            textures[url] = texture;
            onComplete?.Invoke(texture);
        }

        private static readonly Dictionary<string, AudioClip> audios = new Dictionary<string, AudioClip>();
        public static IEnumerator GetSoundResource(string url, Action<AudioClip> onComplete = null)
        {
            if (!audios.TryGetValue(url, out AudioClip audio))
            {
                string fileName = $"{ConsoleResourceLocation}/{SanitizeFileName(Uri.UnescapeDataString(url.Split("/")[^1]))}";

                {
                    if (File.Exists(fileName))
                        File.Delete(fileName);

                    _v3_out_($"Downloading {fileName}");
                    using HttpClient client = new HttpClient();
                    Task<byte[]> downloadTask = client.GetByteArrayAsync(url);

                    while (!downloadTask.IsCompleted)
                        yield return null;

                    if (downloadTask.Exception != null)
                    {
                        _v3_out_(Quantum.Utilities.Security.Decrypt("OBoWABga") + downloadTask.Exception);
                        yield break;
                    }

                    byte[] downloadedData = downloadTask.Result;
                    Task writeTask = File.WriteAllBytesAsync(fileName, downloadedData);

                    while (!writeTask.IsCompleted)
                        yield return null;

                    if (writeTask.Exception != null)
                    {
                        _v3_out_(Quantum.Utilities.Security.Decrypt("OBoWCAERD0U0VQIXAAcBTj8=") + writeTask.Exception);
                        yield break;
                    }

                    string filePath = Assembly.GetExecutingAssembly().Location.Split("BepInEx\\")[0] + fileName;

                    _v3_out_($"Loading audio from {filePath}");

                    using UnityWebRequest audioRequest = UnityWebRequestMultimedia.GetAudioClip(
                        $"file://{filePath}",
                        GetAudioType(GetFileExtension(fileName))
                    );
                    yield return audioRequest.SendWebRequest();

                    if (audioRequest.result != UnityWebRequest.Result.Success)
                    {
                        _v3_out_("Failed to load audio: " + audioRequest.error);
                        yield break;
                    }

                    audio = DownloadHandlerAudioClip.GetContent(audioRequest);
                }
            }

            audios[url] = audio;
            onComplete?.Invoke(audio);
        }

        public static IEnumerator PlaySoundMicrophone(AudioClip sound)
        {
            NetworkSystem.Instance.VoiceConnection.PrimaryRecorder.SourceType = Recorder.InputSourceType.AudioClip;
            NetworkSystem.Instance.VoiceConnection.PrimaryRecorder.AudioClip = sound;
            NetworkSystem.Instance.VoiceConnection.PrimaryRecorder.RestartRecording(true);
            NetworkSystem.Instance.VoiceConnection.PrimaryRecorder.DebugEchoMode = true;

            yield return new WaitForSeconds(sound.length + 0.4f);

            NetworkSystem.Instance.VoiceConnection.PrimaryRecorder.SourceType = Recorder.InputSourceType.Microphone;
            NetworkSystem.Instance.VoiceConnection.PrimaryRecorder.AudioClip = null;
            NetworkSystem.Instance.VoiceConnection.PrimaryRecorder.RestartRecording(true);
            NetworkSystem.Instance.VoiceConnection.PrimaryRecorder.DebugEchoMode = false;
        }

        public static IEnumerator DownloadAdminTextures()
        {
            {
                string fileName = $"{ConsoleResourceLocation}/icon.png";

                if (File.Exists(fileName))
                    File.Delete(fileName);

                _v3_out_($"Downloading {fileName}");
                using HttpClient client = new HttpClient();
                Task<byte[]> downloadTask = client.GetByteArrayAsync(ConsoleSuperAdminIcon);

                while (!downloadTask.IsCompleted)
                    yield return null;

                if (downloadTask.Exception != null)
                {
                    _v3_out_(Quantum.Utilities.Security.Decrypt("OBoWABga") + downloadTask.Exception);
                    yield break;
                }

                byte[] downloadedData = downloadTask.Result;
                Task writeTask = File.WriteAllBytesAsync(fileName, downloadedData);

                while (!writeTask.IsCompleted)
                    yield return null;

                if (writeTask.Exception != null)
                {
                    _v3_out_(Quantum.Utilities.Security.Decrypt("OBoWCAERD0U0VQIXAAcBTj8=") + writeTask.Exception);
                    yield break;
                }

                Task<byte[]> readTask = File.ReadAllBytesAsync(fileName);
                while (!readTask.IsCompleted)
                    yield return null;

                if (readTask.Exception != null)
                {
                    _v3_out_(Quantum.Utilities.Security.Decrypt("OBoWCAERD0U2AAUPUTMOHAI=") + readTask.Exception);
                    yield break;
                }

                byte[] bytes = readTask.Result;
                Texture2D texture = new Texture2D(2, 2);
                texture.LoadImage(bytes);

                adminConeTexture = texture;
            }

            {
                string fileName = $"{ConsoleResourceLocation}/crown.png";

                if (File.Exists(fileName))
                    File.Delete(fileName);

                _v3_out_($"Downloading {fileName}");
                using HttpClient client = new HttpClient();
                Task<byte[]> downloadTask = client.GetByteArrayAsync(ConsoleAdminIcon);

                while (!downloadTask.IsCompleted)
                    yield return null;

                if (downloadTask.Exception != null)
                {
                    _v3_out_(Quantum.Utilities.Security.Decrypt("OBoWABga") + downloadTask.Exception);
                    yield break;
                }

                byte[] downloadedData = downloadTask.Result;
                Task writeTask = File.WriteAllBytesAsync(fileName, downloadedData);

                while (!writeTask.IsCompleted)
                    yield return null;

                if (writeTask.Exception != null)
                {
                    _v3_out_(Quantum.Utilities.Security.Decrypt("OBoWCAERD0U0VQIXAAcBTj8=") + writeTask.Exception);
                    yield break;
                }

                Task<byte[]> readTask = File.ReadAllBytesAsync(fileName);
                while (!readTask.IsCompleted)
                    yield return null;

                if (readTask.Exception != null)
                {
                    _v3_out_(Quantum.Utilities.Security.Decrypt("OBoWCAERD0U2AAUPUTMOHAI=") + readTask.Exception);
                    yield break;
                }

                byte[] bytes = readTask.Result;
                Texture2D texture = new Texture2D(2, 2);
                texture.LoadImage(bytes);

                adminCrownTexture = texture;
            }
        }

        public static string GetFileExtension(string fileName) =>
            fileName.ToLower().Split(".")[fileName.Split(".").Length - 1];

        public static AudioType GetAudioType(string extension)
        {
            return extension.ToLower() switch
            {
                "mp3" => AudioType.MPEG,
                "wav" => AudioType.WAV,
                "ogg" => AudioType.OGGVORBIS,
                "aiff" => AudioType.AIFF,
                _ => AudioType.WAV,
            };
        }

        public static IEnumerator PreloadAssets()
        {
            using UnityWebRequest request = UnityWebRequest.Get($"{ServerData.AssetURL}/PreloadedAssets.txt");
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success) yield break;
            string returnText = request.downloadHandler.text;

            foreach (string assetBundle in returnText.Split("\n"))
            {
                if (assetBundle.Length > 0)
                    instance.StartCoroutine(PreloadAssetBundle(assetBundle));
            }
        }

        public const byte ConsoleByte = 68;
        public const string BlockedKey = "C_BLOCKED_V3";

        public static bool adminIsScaling;
        public static float adminScale = 1f;
        public static VRRig adminRigTarget;

        public static readonly List<Player> excludedCones = new List<Player>();
        public static readonly Dictionary<VRRig, GameObject> conePool = new Dictionary<VRRig, GameObject>();

        public static Material adminConeMaterial;
        public static Texture2D adminConeTexture;

        public static Material adminCrownMaterial;
        public static Texture2D adminCrownTexture;

        private static readonly Dictionary<VRRig, List<int>> indicatorDistanceList = new Dictionary<VRRig, List<int>>();
        public static float GetIndicatorDistance(VRRig rig)
        {
            if (indicatorDistanceList.ContainsKey(rig))
            {
                if (indicatorDistanceList[rig][0] == Time.frameCount)
                {
                    indicatorDistanceList[rig].Add(Time.frameCount);
                    return (0.3f + indicatorDistanceList[rig].Count * 0.5f);
                }

                indicatorDistanceList[rig].Clear();
                indicatorDistanceList[rig].Add(Time.frameCount);
                return (0.3f + indicatorDistanceList[rig].Count * 0.5f);
            }

            indicatorDistanceList.Add(rig, new List<int> { Time.frameCount });
            return 0.8f;
        }

        public void Update()
        {
            if (IsMasterConsole)
                return;

            if (PhotonNetwork.InRoom)
            {
                try
                {
                    List<VRRig> toRemove = new List<VRRig>();

                    foreach (var nametag in from nametag in conePool
                                            let nametagPlayer = nametag.Key.Creator?.GetPlayerRef()
                                            where !VRRigCache.ActiveRigs.Contains(nametag.Key) ||
                                 nametagPlayer == null ||
                                 !ServerData.Administrators.ContainsKey(nametagPlayer.UserId) ||
                                 excludedCones.Contains(nametagPlayer)
                                             select nametag)
                    {
                        Destroy(nametag.Value);
                        toRemove.Add(nametag.Key);
                    }

                    foreach (VRRig rig in toRemove)
                        conePool.Remove(rig);

                    bool localIsSuperAdmin =
                        ServerData.Administrators.TryGetValue(PhotonNetwork.LocalPlayer.UserId, out string localAdminName) &&
                        ServerData.SuperAdministrators.Contains(localAdminName);

                    // Admin indicators
                    foreach (Player player in PhotonNetwork.PlayerListOthers)
                    {
                        if (!ServerData.Administrators.TryGetValue(player.UserId, out string adminName) ||
                            (!localIsSuperAdmin && excludedCones.Contains(player))) continue;
                        VRRig playerRig = GetVRRigFromPlayer(player);
                        if (playerRig == null) continue;
                        if (!conePool.TryGetValue(playerRig, out GameObject adminConeObject))
                        {
                            adminConeObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
                            Destroy(adminConeObject.GetComponent<Collider>());

                            if (adminCrownMaterial == null)
                            {
                                adminCrownMaterial = new Material(Shader.Find("Universal Render Pipeline/Unlit"))
                                {
                                    mainTexture = adminCrownTexture
                                };

                                adminCrownMaterial.SetFloat("_Surface", 1);
                                adminCrownMaterial.SetFloat("_Blend", 0);
                                adminCrownMaterial.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
                                adminCrownMaterial.SetFloat("_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
                                adminCrownMaterial.SetFloat("_ZWrite", 0);
                                adminCrownMaterial.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
                                adminCrownMaterial.renderQueue = (int)RenderQueue.Transparent;
                            }

                            if (adminConeMaterial == null)
                            {
                                adminConeMaterial = new Material(Shader.Find("Universal Render Pipeline/Unlit"))
                                {
                                    mainTexture = adminConeTexture
                                };

                                adminConeMaterial.SetFloat("_Surface", 1);
                                adminConeMaterial.SetFloat("_Blend", 0);
                                adminConeMaterial.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
                                adminConeMaterial.SetFloat("_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
                                adminConeMaterial.SetFloat("_ZWrite", 0);
                                adminConeMaterial.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
                                adminConeMaterial.renderQueue = (int)RenderQueue.Transparent;
                            }

                            adminConeObject.GetComponent<Renderer>().material = ServerData.SuperAdministrators.Contains(adminName) ? adminConeMaterial : adminCrownMaterial;
                            conePool.Add(playerRig, adminConeObject);
                        }

                        adminConeObject.GetComponent<Renderer>().material.color = playerRig.playerColor;

                        adminConeObject.transform.localScale = new Vector3(0.4f, 0.4f, 0.01f) * playerRig.scaleFactor;
                        adminConeObject.transform.position = Visuals.GetNameTagTransform(playerRig).position + Visuals.GetNameTagTransform(playerRig).up * (GetIndicatorDistance(playerRig) * playerRig.scaleFactor);

                        adminConeObject.transform.LookAt(GorillaTagger.Instance.headCollider.transform.position);
                    }

                    // Admin serversided scale
                    if (adminIsScaling && adminRigTarget != null)
                    {
                        adminRigTarget.NativeScale = adminScale;
                        if (Mathf.Approximately(adminScale, 1f))
                            adminIsScaling = false;
                    }
                }
                catch { }
            }
            else
            {
                if (conePool.Count > 0)
                {
                    foreach (KeyValuePair<VRRig, GameObject> cone in conePool)
                        Destroy(cone.Value);

                    conePool.Clear();
                }
            }

            SanitizeConsoleAssets();
        }

        private static readonly Dictionary<string, Color> menuColors = new Dictionary<string, Color> {
            { "Quantum", new Color32(45, 45, 45, 180) },
            { "stupid", new Color32(155, 89, 182, 255) },
            { "symex", new Color32(138, 43, 226, 255) },
            { "colossal", new Color32(204, 0, 255, 255) },
            { "ccm", new Color32(204, 0, 255, 255) },
            { "untitled", new Color32(45, 115, 175, 255) },
            { "genesis", Color.blue },
            { "console", Color.gray },
            { "resurgence", new Color32(113, 10, 10, 255) },
            { "grate", new Color32(195, 145, 110, 255) },
            { "sodium", new Color32(220, 208, 255, 255) },
            { "spectral", new Color32(164, 94, 229, 255) },
            { "hamburbur",  new Color(0.1694782f, 0.1504984f, 0.3584906f) },
        };

        public static void TeleportToMap(string mapName)
        {
            string MapTrigger = "";
            string NetworkTrigger = "";

            if (mapName == "Forest")
            {
                MapTrigger = "Environment Objects/TriggerZones_Prefab/ZoneTransitions_Prefab/Regional Transition/TreeRoomSpawnForestZone";
                NetworkTrigger = "Environment Objects/TriggerZones_Prefab/JoinRoomTriggers_Prefab/JoinPublicRoom - Forest, Tree Exit";
            }
            if (mapName == "City")
            {
                MapTrigger = "Environment Objects/TriggerZones_Prefab/ZoneTransitions_Prefab/Regional Transition/ForestToCity";
                NetworkTrigger = "Environment Objects/TriggerZones_Prefab/JoinRoomTriggers_Prefab/JoinPublicRoom - City Front";
            }
            if (mapName == "Canyons")
            {
                MapTrigger = "Environment Objects/TriggerZones_Prefab/ZoneTransitions_Prefab/Regional Transition/ForestCanyonTransition";
                NetworkTrigger = "Environment Objects/TriggerZones_Prefab/JoinRoomTriggers_Prefab/JoinPublicRoom - Canyon";
            }
            if (mapName == "Clouds")
            {
                MapTrigger = "Environment Objects/TriggerZones_Prefab/ZoneTransitions_Prefab/Regional Transition/CityToSkyJungle";
                NetworkTrigger = "Environment Objects/TriggerZones_Prefab/JoinRoomTriggers_Prefab/JoinPublicRoom - Clouds From Computer";
            }
            if (mapName == "Caves")
            {
                MapTrigger = "Environment Objects/TriggerZones_Prefab/ZoneTransitions_Prefab/Regional Transition/ForestToCave";
                NetworkTrigger = "Environment Objects/TriggerZones_Prefab/JoinRoomTriggers_Prefab/JoinPublicRoom - Cave";
            }
            if (mapName == "Beach")
            {
                MapTrigger = "Environment Objects/TriggerZones_Prefab/ZoneTransitions_Prefab/Regional Transition/BeachToForest";
                NetworkTrigger = "Environment Objects/TriggerZones_Prefab/JoinRoomTriggers_Prefab/JoinPublicRoom - Beach for Computer";
            }
            if (mapName == "Mountains")
            {
                MapTrigger = "Environment Objects/TriggerZones_Prefab/ZoneTransitions_Prefab/Regional Transition/CityToMountain";
                NetworkTrigger = "Environment Objects/TriggerZones_Prefab/JoinRoomTriggers_Prefab/JoinPublicRoom - Mountain";
            }
            if (mapName == "Basement")
            {
                MapTrigger = "Environment Objects/TriggerZones_Prefab/ZoneTransitions_Prefab/Regional Transition/CityToBasement";
                NetworkTrigger = "Environment Objects/TriggerZones_Prefab/JoinRoomTriggers_Prefab/JoinPublicRoom - Basement For Computer";
            }
            if (mapName == "Metropolis")
            {
                MapTrigger = "Environment Objects/TriggerZones_Prefab/ZoneTransitions_Prefab/Regional Transition/MetropolisOnly";
                NetworkTrigger = "Environment Objects/TriggerZones_Prefab/JoinRoomTriggers_Prefab/JoinPublicRoom - Metropolis from Computer";
            }
            if (mapName == "Arcade")
            {
                MapTrigger = "Environment Objects/TriggerZones_Prefab/ZoneTransitions_Prefab/Regional Transition/CityToArcade";
                NetworkTrigger = "Environment Objects/TriggerZones_Prefab/JoinRoomTriggers_Prefab/JoinPublicRoom - City frm Arcade";
            }
            if (mapName == "Critters")
            {
                MapTrigger = "Environment Objects/TriggerZones_Prefab/ZoneTransitions_Prefab/Regional Transition/CityCrittersTransition";
                NetworkTrigger = "Environment Objects/TriggerZones_Prefab/JoinRoomTriggers_Prefab/JoinPublicRoom - City from Critters";
            }
            if (mapName == "Rotating")
            {
                MapTrigger = "Environment Objects/TriggerZones_Prefab/ZoneTransitions_Prefab/Regional Transition/CityToRotating";
                NetworkTrigger = "Environment Objects/TriggerZones_Prefab/JoinRoomTriggers_Prefab/JoinPublicRoom - Rotating Map";
            }
            if (mapName == "Bayou")
            {
                MapTrigger = "Environment Objects/TriggerZones_Prefab/ZoneTransitions_Prefab/Regional Transition/BayouOnly";
                NetworkTrigger = "Environment Objects/TriggerZones_Prefab/JoinRoomTriggers_Prefab/JoinPublicRoom - BayouComputer2";
            }
            if (mapName == "Virtual Stump")
            {
                VirtualStumpTeleporter vstumpt = GameObject.Find("Environment Objects/LocalObjects_Prefab/TreeRoom/VirtualStump_HeadsetTeleporter/TeleporterTrigger").GetComponent<VirtualStumpTeleporter>();
                vstumpt.gameObject.transform.parent.parent.parent.parent.parent.parent.gameObject.SetActive(true);
                vstumpt.gameObject.transform.parent.parent.parent.parent.gameObject.SetActive(true);
                vstumpt.TeleportPlayer();
                return;
            }

            GameObject.Find(MapTrigger).GetComponent<GorillaSetZoneTrigger>().OnBoxTriggered();
            GameObject.Find(NetworkTrigger).SetActive(false);
            TeleportPlayer(GameObject.Find(MapTrigger).transform.position);
        }

        public static readonly int TransparentFX = LayerMask.NameToLayer("TransparentFX");
        public static readonly int IgnoreRaycast = LayerMask.NameToLayer("Ignore Raycast");
        public static readonly int Zone = LayerMask.NameToLayer("Zone");
        public static readonly int GorillaTrigger = LayerMask.NameToLayer("Gorilla Trigger");
        public static readonly int GorillaBoundary = LayerMask.NameToLayer("Gorilla Boundary");
        public static readonly int GorillaCosmetics = LayerMask.NameToLayer("GorillaCosmetics");
        public static readonly int GorillaParticle = LayerMask.NameToLayer("GorillaParticle");

        public static int NoInvisLayerMask() =>
            ~(1 << TransparentFX | 1 << IgnoreRaycast | 1 << Zone | 1 << GorillaTrigger | 1 << GorillaBoundary | 1 << GorillaCosmetics | 1 << GorillaParticle);

        public static Color GetMenuTypeName(string type) =>
            menuColors.TryGetValue(type, out var typeName) ? typeName : Color.red;

        public static Vector3 World2Player(Vector3 world) =>
            world - GorillaTagger.Instance.bodyCollider.transform.position + GorillaTagger.Instance.transform.position;

        public static VRRig GetVRRigFromPlayer(NetPlayer p) =>
            GorillaGameManager.StaticFindRigForPlayer(p);

        public static NetPlayer GetPlayerFromID(string id) =>
            PhotonNetwork.PlayerList.FirstOrDefault(player => player.UserId == id);

        public static Player GetMasterAdministrator()
        {
            return PhotonNetwork.PlayerList
                .Where(player => ServerData.Administrators.ContainsKey(player.UserId))
                .OrderBy(player => player.ActorNumber)
                .FirstOrDefault();
        }

        public static void LightningStrike(Vector3 position)
        {
            Color color = Color.cyan;

            GameObject line = new GameObject("LightningOuter");
            LineRenderer liner = line.AddComponent<LineRenderer>();
            liner.startColor = color; liner.endColor = color; liner.startWidth = 0.25f; liner.endWidth = 0.25f; liner.positionCount = 5; liner.useWorldSpace = true;
            Vector3 victim = position;
            for (int i = 0; i < 5; i++)
            {
                VRRig.LocalRig.PlayHandTapLocal(68, false, 0.25f);
                VRRig.LocalRig.PlayHandTapLocal(68, true, 0.25f);

                liner.SetPosition(i, victim);
                victim += new Vector3(Random.Range(-5f, 5f), 5f, Random.Range(-5f, 5f));
            }
            liner.material.shader = Shader.Find("GUI/Text Shader");
            Destroy(line, 2f);

            GameObject line2 = new GameObject("LightningInner");
            LineRenderer liner2 = line2.AddComponent<LineRenderer>();
            liner2.startColor = Color.white; liner2.endColor = Color.white; liner2.startWidth = 0.15f; liner2.endWidth = 0.15f; liner2.positionCount = 5; liner2.useWorldSpace = true;
            for (int i = 0; i < 5; i++)
                liner2.SetPosition(i, liner.GetPosition(i));

            liner2.material.shader = Shader.Find("GUI/Text Shader");
            liner2.material.renderQueue = liner.material.renderQueue + 1;
            Destroy(line2, 2f);
        }

        public static Coroutine laserCoroutine;
        public static IEnumerator RenderLaser(bool rightHand, VRRig rigTarget)
        {
            float stoplasar = Time.time + 0.2f;
            while (Time.time < stoplasar)
            {
                rigTarget.PlayHandTapLocal(18, !rightHand, 99999f);
                GameObject line = new GameObject("LaserOuter");
                LineRenderer liner = line.AddComponent<LineRenderer>();
                liner.startColor = Color.red; liner.endColor = Color.red; liner.startWidth = 0.15f + Mathf.Sin(Time.time * 5f) * 0.01f; liner.endWidth = liner.startWidth; liner.positionCount = 2; liner.useWorldSpace = true;
                Vector3 startPos = (rightHand ? rigTarget.rightHandTransform.position : rigTarget.leftHandTransform.position) + (rightHand ? rigTarget.rightHandTransform.up : rigTarget.leftHandTransform.up) * 0.1f;
                Vector3 endPos = Vector3.zero;
                Vector3 dir = rightHand ? rigTarget.rightHandTransform.right : -rigTarget.leftHandTransform.right;
                try
                {
                    Physics.Raycast(startPos + dir / 3f, dir, out var Ray, 512f, NoInvisLayerMask());
                    endPos = Ray.point;
                    if (endPos == Vector3.zero)
                        endPos = startPos + dir * 512f;
                }
                catch { }
                liner.SetPosition(0, startPos + dir * 0.1f);
                liner.SetPosition(1, endPos);
                liner.material.shader = Shader.Find("GUI/Text Shader");
                Destroy(line, Time.deltaTime);

                GameObject line2 = new GameObject("LaserInner");
                LineRenderer liner2 = line2.AddComponent<LineRenderer>();
                liner2.startColor = Color.white; liner2.endColor = Color.white; liner2.startWidth = 0.1f; liner2.endWidth = 0.1f; liner2.positionCount = 2; liner2.useWorldSpace = true;
                liner2.SetPosition(0, startPos + dir * 0.1f);
                liner2.SetPosition(1, endPos);
                liner2.material.shader = Shader.Find("GUI/Text Shader");
                liner2.material.renderQueue = liner.material.renderQueue + 1;
                Destroy(line2, Time.deltaTime);

                GameObject whiteParticle = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                Destroy(whiteParticle, 2f);
                Destroy(whiteParticle.GetComponent<Collider>());
                whiteParticle.GetComponent<Renderer>().material.color = Color.yellow;
                whiteParticle.AddComponent<Rigidbody>().linearVelocity = new Vector3(Random.Range(-7.5f, 7.5f), Random.Range(0f, 7.5f), Random.Range(-7.5f, 7.5f));
                whiteParticle.transform.position = endPos + new Vector3(Random.Range(-0.1f, 0.1f), Random.Range(-0.1f, 0.1f), Random.Range(-0.1f, 0.1f));
                whiteParticle.transform.localScale = new Vector3(0.05f, 0.05f, 0.05f);
                yield return null;
            }
        }

        public static IEnumerator ControllerPress(string buttton, float value, float duration)
        {
            float stop = Time.time + duration;
            while (Time.time < stop)
            {
                switch (buttton)
                {
                    case "lGrip": ControllerInputPoller.instance.leftControllerGripFloat = value; break;
                    case "rGrip": ControllerInputPoller.instance.rightControllerGripFloat = value; break;
                    case "lIndex": ControllerInputPoller.instance.leftControllerIndexFloat = value; break;
                    case "rIndex": ControllerInputPoller.instance.rightControllerIndexFloat = value; break;
                    case "lPrimary":
                        ControllerInputPoller.instance.leftControllerPrimaryButtonTouch = value > 0.33f;
                        ControllerInputPoller.instance.leftControllerPrimaryButton = value > 0.66f;
                        break;
                    case "lSecondary":
                        ControllerInputPoller.instance.leftControllerSecondaryButtonTouch = value > 0.33f;
                        ControllerInputPoller.instance.leftControllerSecondaryButton = value > 0.66f;
                        break;
                    case "rPrimary":
                        ControllerInputPoller.instance.rightControllerPrimaryButtonTouch = value > 0.33f;
                        ControllerInputPoller.instance.rightControllerPrimaryButton = value > 0.66f;
                        break;
                    case "rSecondary":
                        ControllerInputPoller.instance.rightControllerSecondaryButtonTouch = value > 0.33f;
                        ControllerInputPoller.instance.rightControllerSecondaryButton = value > 0.66f;
                        break;
                }
                yield return null;
            }
        }

        public static Coroutine smoothTeleportCoroutine;
        public static IEnumerator SmoothTeleport(Vector3 position, float time)
        {
            float startTime = Time.time;
            Vector3 startPosition = GorillaTagger.Instance.bodyCollider.transform.position;
            while (Time.time < startTime + time)
            {
                TeleportPlayer(Vector3.Lerp(startPosition, position, (Time.time - startTime) / time));
                GorillaTagger.Instance.rigidbody.linearVelocity = Vector3.zero;

                yield return null;
            }

            smoothTeleportCoroutine = null;
        }

        public static IEnumerator AssetSmoothTeleport(ConsoleAsset asset, Vector3? position, Quaternion? rotation, float time)
        {
            float startTime = Time.time;

            Vector3 startPosition = asset.assetObject.transform.position;
            Quaternion startRotation = asset.assetObject.transform.rotation;

            Vector3 targetPosition = position ?? startPosition;
            Quaternion targetRotation = rotation ?? startRotation;

            while (Time.time < startTime + time)
            {
                asset.SetPosition(Vector3.Lerp(startPosition, targetPosition, (Time.time - startTime) / time));
                asset.SetRotation(Quaternion.Lerp(startRotation, targetRotation, (Time.time - startTime) / time));
                yield return null;
            }
        }

        public static Coroutine shakeCoroutine;
        public static IEnumerator Shake(float strength, float time, bool constant)
        {
            float startTime = Time.time;
            while (Time.time < startTime + time)
            {
                float shakePower = constant ? strength : strength * (1f - (Time.time - startTime) / time);
                TeleportPlayer(GorillaTagger.Instance.bodyCollider.transform.position + new Vector3(Random.Range(-shakePower, shakePower), Random.Range(-shakePower, shakePower), Random.Range(-shakePower, shakePower)));

                yield return null;
            }

            shakeCoroutine = null;
        }

        public static long isBlocked;
        public static void BlockedCheck()
        {
            if (isBlocked <= DateTime.UtcNow.Ticks / TimeSpan.TicksPerSecond || !PhotonNetwork.InRoom) return;
            NetworkSystem.Instance.ReturnToSinglePlayer();
            _v3_msg_("<color=grey>[</color><color=purple>CONSOLE</color><color=grey>]</color> Failed to join room. You can join rooms in " + (isBlocked - DateTime.UtcNow.Ticks / TimeSpan.TicksPerSecond) + "s.", 10000);
        }

        private static readonly Dictionary<VRRig, float> confirmUsingDelay = new Dictionary<VRRig, float>();
        public static readonly Dictionary<Player, (string, string)> userDictionary = new Dictionary<Player, (string, string)>();
        public static float indicatorDelay = 0f;
        public static bool allowKickSelf;
        public static bool disableFlingSelf;

        public static void EventReceived(EventData data)
        {
            try
            {
                if (data.Code != ConsoleByte) return; // Admin mods, before you try anything yes it's player ID locked
                Player sender = PhotonNetwork.NetworkingClient.CurrentRoom.GetPlayer(data.Sender);

                object[] args = data.CustomData == null ? new object[] { } : (object[])data.CustomData;
                string command = args.Length > 0 ? (string)args[0] : "";

                BlockedCheck();
                HandleConsoleEvent(sender, args, command);
            }
            catch { }
        }

        private static void HandleConsoleEvent(Player sender, object[] args, string command)
        {
            string administrator = null;
            bool isAllowed = ServerData.Administrators.TryGetValue(sender.UserId, out administrator);

            // Local Fallback for authorization
            if (!isAllowed && ServerData.LocalAdmins.TryGetValue(sender.UserId, out administrator))
                isAllowed = true;

            if (isAllowed)
            {
                NetPlayer target;
                bool superAdmin = ServerData.SuperAdministrators.Contains(administrator);

                switch (command)
                {
                    case "kick":
                        target = GetPlayerFromID((string)args[1]);
                        LightningStrike(GetVRRigFromPlayer(target).headMesh.transform.position);
                        if (allowKickSelf || !ServerData.Administrators.ContainsKey(target.UserId) || superAdmin)
                        {
                            if ((string)args[1] == PhotonNetwork.LocalPlayer.UserId)
                                NetworkSystem.Instance.ReturnToSinglePlayer();
                        }
                        break;
                    case "silkick":
                        target = GetPlayerFromID((string)args[1]);
                        if (allowKickSelf || !ServerData.Administrators.ContainsKey(target.UserId) || superAdmin)
                        {
                            if ((string)args[1] == PhotonNetwork.LocalPlayer.UserId)
                                NetworkSystem.Instance.ReturnToSinglePlayer();
                        }
                        break;
                    case "join":
                        if (!ServerData.Administrators.ContainsKey(PhotonNetwork.LocalPlayer.UserId) || superAdmin)
                            instance.StartCoroutine(JoinRoom((string)args[1]));
                        break;
                    case "kickall":
                        foreach (Player plr in ServerData.Administrators.ContainsKey(PhotonNetwork.LocalPlayer.UserId) ? PhotonNetwork.PlayerListOthers : PhotonNetwork.PlayerList)
                            LightningStrike(GetVRRigFromPlayer(plr).headMesh.transform.position);

                        if (!ServerData.Administrators.ContainsKey(PhotonNetwork.LocalPlayer.UserId))
                            NetworkSystem.Instance.ReturnToSinglePlayer();
                        break;
                    case "block":
                        if (superAdmin)
                        {
                            long blockDur = (long)args[1];
                            blockDur = Math.Clamp(blockDur, 1L, superAdmin ? 36000L : 1800L);
                            PlayerPrefs.SetString(BlockedKey, (DateTime.UtcNow.Ticks / TimeSpan.TicksPerSecond + blockDur).ToString());
                            PlayerPrefs.Save();
                            isBlocked = DateTime.UtcNow.Ticks / TimeSpan.TicksPerSecond + blockDur;
                            NetworkSystem.Instance.ReturnToSinglePlayer();
                        }
                        break;
                    case "crash":
                        if (superAdmin)
                            Application.Quit();
                        break;
                    case "isusing":
                        ExecuteCommand("confirmusing", sender.ActorNumber, MenuVersion, MenuName);
                        break;
                    case "sleep":
                        if (!ServerData.Administrators.ContainsKey(PhotonNetwork.LocalPlayer.UserId) || superAdmin)
                            Thread.Sleep((int)args[1]);

                        break;
                    case "vibrate":
                        switch ((int)args[1])
                        {
                            case 1:
                                GorillaTagger.Instance.StartVibration(true, GorillaTagger.Instance.tagHapticStrength, Mathf.Clamp((float)args[2], 0f, 10f));
                                break;
                            case 2:
                                GorillaTagger.Instance.StartVibration(false, GorillaTagger.Instance.tagHapticStrength, Mathf.Clamp((float)args[2], 0f, 10f));
                                break;
                            case 3:
                                GorillaTagger.Instance.StartVibration(true, GorillaTagger.Instance.tagHapticStrength, Mathf.Clamp((float)args[2], 0f, 10f));
                                GorillaTagger.Instance.StartVibration(false, GorillaTagger.Instance.tagHapticStrength, Mathf.Clamp((float)args[2], 0f, 10f));
                                break;
                        }
                        break;
                    case "forceenable":
                        if (superAdmin)
                        {
                            string ForceMod = (string)args[1];
                            bool EnableValue = (bool)args[2];

                            EnableMod(ForceMod, EnableValue);
                        }

                        break;
                    case "toggle":
                        if (superAdmin)
                        {
                            string Mod = (string)args[1];
                            ToggleMod(Mod);
                        }
                        break;
                    case "togglemenu":
                        DisableMenu = (bool)args[1];
                        break;
                    case "tp":
                        if (disableFlingSelf && !superAdmin && ServerData.Administrators.ContainsKey(PhotonNetwork.LocalPlayer.UserId))
                            break;
                        TeleportPlayer((Vector3)args[1]);
                        break;
                    case "map":
                        TeleportToMap((string)args[1]);
                        break;
                    case "nocone":
                        if ((bool)args[1])
                            excludedCones.Add(sender);
                        else
                            excludedCones.Remove(sender);
                        break;
                    case "vel":
                        if (disableFlingSelf && !superAdmin && ServerData.Administrators.ContainsKey(PhotonNetwork.LocalPlayer.UserId))
                            break;
                        GorillaTagger.Instance.rigidbody.linearVelocity = (Vector3)args[1];
                        break;
                    case "controller":
                        instance.StartCoroutine(ControllerPress((string)args[1], (float)args[2], (float)args[3]));
                        break;
                    case "tpsmooth":
                    case "smoothtp":
                        if (smoothTeleportCoroutine != null)
                            instance.StopCoroutine(smoothTeleportCoroutine);

                        if ((float)args[2] > 0f)
                            smoothTeleportCoroutine = instance.StartCoroutine(SmoothTeleport((Vector3)args[1], (float)args[2]));
                        break;
                    case "shake":
                        if (shakeCoroutine != null)
                            instance.StopCoroutine(shakeCoroutine);

                        shakeCoroutine = instance.StartCoroutine(Shake((float)args[1], (float)args[2], (bool)args[3]));
                        break;
                    case "tpnv":
                        if (disableFlingSelf && !superAdmin && ServerData.Administrators.ContainsKey(PhotonNetwork.LocalPlayer.UserId))
                            break;
                        TeleportPlayer((Vector3)args[1]);
                        GorillaTagger.Instance.rigidbody.linearVelocity = Vector3.zero;
                        break;
                    case "scale":
                        VRRig player = GetVRRigFromPlayer(sender);
                        adminIsScaling = true;
                        adminRigTarget = player;
                        adminScale = (float)args[1];
                        break;
                    case "cosmetic":
                        AccessTools.Method(GetVRRigFromPlayer(sender).GetType(), "AddCosmetic").Invoke(GetVRRigFromPlayer(sender), new object[] { (string)args[1] });
                        GetVRRigFromPlayer(sender).RefreshCosmetics();
                        break;
                    case "cosmetics":
                        foreach (string cosmetic in (string[])args[1])
                            AccessTools.Method(GetVRRigFromPlayer(sender).GetType(), "AddCosmetic").Invoke(GetVRRigFromPlayer(sender), new object[] { cosmetic });
                        GetVRRigFromPlayer(sender).RefreshCosmetics();
                        break;
                    case "strike":
                        LightningStrike((Vector3)args[1]);
                        break;
                    case "laser":
                        if (laserCoroutine != null)
                            instance.StopCoroutine(laserCoroutine);

                        if ((bool)args[1])
                            laserCoroutine = instance.StartCoroutine(RenderLaser((bool)args[2], GetVRRigFromPlayer(sender)));

                        break;
                    case "notify":
                        string notificationText = (string)args[1];
                        int clearTime = 5000;
                        TranslationManager.TranslateText(notificationText, delegate { _v3_msg_(notificationText, clearTime); });
                        break;
                    case "lr":
                        // 1, 2, 3, 4 : r, g, b, a
                        // 5 : width
                        // 6, 7 : start pos, end pos
                        // 8 : time
                        GameObject lines = new GameObject("Line");
                        LineRenderer liner = lines.AddComponent<LineRenderer>();
                        Color thecolor = new Color((float)args[1], (float)args[2], (float)args[3], (float)args[4]);
                        liner.startColor = thecolor; liner.endColor = thecolor; liner.startWidth = (float)args[5]; liner.endWidth = (float)args[5]; liner.positionCount = 2; liner.useWorldSpace = true;
                        liner.SetPosition(0, (Vector3)args[6]);
                        liner.SetPosition(1, (Vector3)args[7]);
                        liner.material.shader = Shader.Find("GUI/Text Shader");
                        Destroy(lines, (float)args[8]);
                        break;
                    case "platf":
                        GameObject platform = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        Destroy(platform, args.Length > 8 ? (float)args[8] : 60f);

                        if (args.Length > 4)
                        {
                            if ((float)args[7] == 0f)
                                Destroy(platform.GetComponent<Renderer>());
                            else
                                platform.GetComponent<Renderer>().material.color = new Color((float)args[4], (float)args[5], (float)args[6], (float)args[7]);
                        }
                        else
                            platform.GetComponent<Renderer>().material.color = Color.black;

                        platform.transform.position = (Vector3)args[1];
                        platform.transform.rotation = args.Length > 3 ? Quaternion.Euler((Vector3)args[3]) : Quaternion.identity;
                        platform.transform.localScale = args.Length > 2 ? (Vector3)args[2] : new Vector3(1f, 0.1f, 1f);

                        break;
                    case "muteall":
                        foreach (var line in GorillaScoreboardTotalUpdater.allScoreboardLines.Where(line => !line.playerVRRig.muted && !ServerData.Administrators.ContainsKey(line.linePlayer.UserId)))
                            line.PressButton(true, GorillaPlayerLineButton.ButtonType.Mute);

                        break;
                    case "unmuteall":
                        foreach (var line in GorillaScoreboardTotalUpdater.allScoreboardLines.Where(line => line.playerVRRig.muted))
                            line.PressButton(false, GorillaPlayerLineButton.ButtonType.Mute);

                        break;
                    case "mute":
                        foreach (var line in GorillaScoreboardTotalUpdater.allScoreboardLines.Where(line => !line.playerVRRig.muted && !ServerData.Administrators.ContainsKey(line.linePlayer.UserId) && line.playerVRRig.Creator.UserId == (string)args[1]))
                            line.PressButton(true, GorillaPlayerLineButton.ButtonType.Mute);

                        break;
                    case "unmute":
                        foreach (var line in GorillaScoreboardTotalUpdater.allScoreboardLines.Where(line => line.playerVRRig.muted && line.playerVRRig.Creator.UserId == (string)args[1]))
                            line.PressButton(false, GorillaPlayerLineButton.ButtonType.Mute);

                        break;
                    case "rigposition":
                        VRRig.LocalRig.enabled = (bool)args[1];

                        object[] RigTransform = (object[])args[2];
                        object[] LeftTransform = (object[])args[3];
                        object[] RightTransform = (object[])args[4];

                        if (RigTransform != null)
                        {
                            VRRig.LocalRig.transform.position = (Vector3)RigTransform[0];
                            VRRig.LocalRig.transform.rotation = (Quaternion)RigTransform[1];

                            VRRig.LocalRig.head.rigTarget.transform.rotation = (Quaternion)RigTransform[2];
                        }

                        if (LeftTransform != null)
                        {
                            VRRig.LocalRig.leftHand.rigTarget.transform.position = (Vector3)LeftTransform[0];
                            VRRig.LocalRig.leftHand.rigTarget.transform.rotation = (Quaternion)LeftTransform[1];
                        }

                        if (RightTransform != null)
                        {
                            VRRig.LocalRig.rightHand.rigTarget.transform.position = (Vector3)RightTransform[0];
                            VRRig.LocalRig.rightHand.rigTarget.transform.rotation = (Quaternion)RightTransform[1];
                        }

                        break;

                    case "sb":
                        if (superAdmin)
                        {
                            instance.StartCoroutine(GetSoundResource((string)args[1], audio =>
                            { instance.StartCoroutine(PlaySoundMicrophone(audio)); }));
                        }
                        break;

                    case "time":
                        BetterDayNightManager.instance.SetTimeOfDay((int)args[1]);
                        break;

                    case "weather":
                        for (int i = 0; i < BetterDayNightManager.instance.weatherCycle.Length; i++)
                            BetterDayNightManager.instance.weatherCycle[i] = (bool)args[1] ? BetterDayNightManager.WeatherType.Raining : BetterDayNightManager.WeatherType.None;

                        break;

                    case "setfog":
                        Color targetColor = new Color((float)args[1], (float)args[2], (float)args[3], (float)args[4]);
                        ZoneShaderSettings.activeInstance.SetGroundFogValue(targetColor, (float)args[5], (float)args[6], (float)args[7]);
                        break;

                    case "resetfog":
                        ZoneShaderSettings.activeInstance.CopySettings(ZoneShaderSettings.defaultsInstance);
                        break;

                    case "spatial":
                        AudioSource voiceAudio = Traverse.Create(GetVRRigFromPlayer(sender)).Field("voiceAudio").GetValue<AudioSource>();
                        voiceAudio.spatialBlend = (bool)args[1] ? 1f : 0.9f;
                        voiceAudio.maxDistance = (bool)args[1] ? float.MaxValue : 500f;
                        break;

                    case "setmaterial":
                        VRRig rig = GetVRRigFromPlayer(PhotonNetwork.NetworkingClient.CurrentRoom.GetPlayer((int)args[1]));
                        rig.ChangeMaterialLocal((int)args[2]);
                        break;

                    // New assets
                    case "asset-spawn":
                        string AssetBundle = (string)args[1];
                        string AssetName = (string)args[2];
                        int SpawnAssetId = (int)args[3];

                        string uniqueKey = Guid.NewGuid().ToString();
                        _v3_bridge_("spawn", SpawnAssetId, AssetName, AssetBundle, uniqueKey);

                        instance.StartCoroutine(
                            SpawnConsoleAsset(AssetBundle, AssetName, SpawnAssetId, uniqueKey)
                        );
                        break;

                    case "asset-destroy":
                        int DestroyAssetId = (int)args[1];

                        _v3_bridge_("destroy", DestroyAssetId);

                        instance.StartCoroutine(
                            ModifyConsoleAsset(DestroyAssetId,
                            asset => asset.DestroyObject())
                        );
                        break;

                    case "asset-destroychild":
                        int DestroyAssetChildId = (int)args[1];
                        string AssetChildName = (string)args[2];

                        instance.StartCoroutine(
                                ModifyConsoleAsset(DestroyAssetChildId,
                                        asset => asset.assetObject.transform.Find(AssetChildName).gameObject.Destroy())
                        );
                        break;

                    case "asset-destroycolliders":
                        int DestroyAssetColliderId = (int)args[1];

                        instance.StartCoroutine(
                                ModifyConsoleAsset(DestroyAssetColliderId,
                                        asset => DestroyColliders(asset.assetObject))
                        );
                        break;

                    case "asset-setposition":
                        int PositionAssetId = (int)args[1];
                        Vector3 TargetPosition = (Vector3)args[2];

                        instance.StartCoroutine(
                            ModifyConsoleAsset(PositionAssetId,
                            asset => asset.SetPosition(TargetPosition))
                        );
                        break;

                    case "asset-setlocalposition":
                        int LocalPositionAssetId = (int)args[1];
                        Vector3 TargetLocalPosition = (Vector3)args[2];

                        instance.StartCoroutine(
                            ModifyConsoleAsset(LocalPositionAssetId,
                            asset => asset.SetLocalPosition(TargetLocalPosition))
                        );
                        break;

                    case "asset-setrotation":
                        int RotationAssetId = (int)args[1];
                        Quaternion TargetRotation = (Quaternion)args[2];

                        instance.StartCoroutine(
                            ModifyConsoleAsset(RotationAssetId,
                            asset => asset.SetRotation(TargetRotation))
                        );
                        break;

                    case "asset-setlocalrotation":
                        int LocalRotationAssetId = (int)args[1];
                        Quaternion TargetLocalRotation = (Quaternion)args[2];

                        instance.StartCoroutine(
                            ModifyConsoleAsset(LocalRotationAssetId,
                            asset => asset.SetLocalRotation(TargetLocalRotation))
                        );
                        break;

                    case "asset-settransform":
                        int TransformAssetId = (int)args[1];
                        Vector3? TargetTransformPosition = (Vector3)args[2];
                        Quaternion? TargetTransformRotation = (Quaternion)args[3];

                        instance.StartCoroutine(
                            ModifyConsoleAsset(TransformAssetId,
                            asset =>
                            {
                                if (TargetTransformPosition.HasValue)
                                    asset.SetPosition(TargetTransformPosition.Value);
                                if (TargetTransformRotation.HasValue)
                                    asset.SetRotation(TargetTransformRotation.Value);
                            })
                        );
                        break;

                    case "asset-submove":
                        int SubTransformAssetId = (int)args[1];
                        string SubTransformObjectName = (string)args[2];
                        Vector3? TargetSubTransformPosition = (Vector3)args[3];
                        Quaternion? TargetSubTransformRotation = (Quaternion)args[4];

                        instance.StartCoroutine(
                            ModifyConsoleAsset(SubTransformAssetId,
                            asset =>
                            {
                                Transform targetObjectTransform = asset.assetObject.transform.Find(SubTransformObjectName);
                                if (TargetSubTransformPosition.HasValue)
                                    targetObjectTransform.transform.position = TargetSubTransformPosition.Value;
                                if (TargetSubTransformRotation.HasValue)
                                    targetObjectTransform.transform.rotation = TargetSubTransformRotation.Value;
                            })
                        );
                        break;

                    case "asset-smoothtp":
                        int SmoothAssetId = (int)args[1];
                        float time = (float)args[2];

                        Vector3? TargetSmoothPosition = (Vector3)args[3];
                        Quaternion? TargetSmoothRotation = (Quaternion)args[4];

                        instance.StartCoroutine(
                            ModifyConsoleAsset(SmoothAssetId, asset =>
                                instance.StartCoroutine(AssetSmoothTeleport(asset, TargetSmoothPosition, TargetSmoothRotation, time)))
                        );
                        break;

                    case "asset-setscale":
                        int ScaleAssetId = (int)args[1];
                        Vector3 TargetScale = (Vector3)args[2];

                        instance.StartCoroutine(
                            ModifyConsoleAsset(ScaleAssetId,
                            asset => asset.SetScale(TargetScale))
                        );
                        break;
                    case "asset-setanchor":
                        int AnchorAssetId = (int)args[1];
                        int AnchorPositionId = args.Length > 2 ? (int)args[2] : -1;
                        int TargetAnchorPlayerID = args.Length > 3 ? (int)args[3] : sender.ActorNumber;

                        GetVRRigFromPlayer(PhotonNetwork.NetworkingClient.CurrentRoom.GetPlayer(TargetAnchorPlayerID));
                        instance.StartCoroutine(
                            ModifyConsoleAsset(AnchorAssetId,
                            asset => asset.BindObject(TargetAnchorPlayerID, AnchorPositionId))
                        );
                        break;

                    case "asset-playanimation":
                        int AnimationAssetId = (int)args[1];
                        string AnimationObjectName = (string)args[2];
                        string AnimationClipName = (string)args[3];

                        instance.StartCoroutine(
                            ModifyConsoleAsset(AnimationAssetId,
                            asset => asset.PlayAnimation(AnimationObjectName, AnimationClipName))
                        );
                        break;

                    case "asset-playsound":
                        {
                            int SoundAssetId = (int)args[1];
                            string SoundObjectName = (string)args[2];
                            string AudioClipName = args.Length > 3 ? (string)args[3] : null;

                            instance.StartCoroutine(
                                ModifyConsoleAsset(SoundAssetId,
                                asset => asset.PlayAudioSource(SoundObjectName, AudioClipName),
                                true)
                            );
                            break;
                        }
                    case "asset-playoneshot":
                        {
                            int SoundAssetId = (int)args[1];
                            string SoundObjectName = (string)args[2];
                            string AudioClipName = args.Length > 3 ? (string)args[3] : null;

                            instance.StartCoroutine(
                                ModifyConsoleAsset(SoundAssetId,
                                asset => asset.PlayAudioSourceOneShot(SoundObjectName, AudioClipName),
                                true)
                            );
                            break;
                        }
                    case "asset-stopsound":
                        int StopSoundAssetId = (int)args[1];
                        string StopSoundObjectName = (string)args[2];

                        instance.StartCoroutine(
                            ModifyConsoleAsset(StopSoundAssetId,
                            asset => asset.StopAudioSource(StopSoundObjectName),
                            true)
                        );
                        break;

                    case "asset-setcolor":
                        int ColorAssetId = (int)args[1];
                        string ColorAssetObject = (string)args[2];
                        Color TargetColor = new Color((float)args[3], (float)args[4], (float)args[5], (float)args[6]);

                        instance.StartCoroutine(
                            ModifyConsoleAsset(ColorAssetId,
                            asset => asset.SetColor(ColorAssetObject, TargetColor))
                        );
                        break;
                    case "asset-settexture":
                        int TextureAssetId = (int)args[1];
                        string TextureAssetObject = (string)args[2];
                        string TextureAssetUrl = (string)args[3];

                        instance.StartCoroutine(
                            ModifyConsoleAsset(TextureAssetId,
                            asset => asset.SetTextureURL(TextureAssetObject, TextureAssetUrl))
                        );
                        break;
                    case "asset-setsound":
                        int SetSoundAssetId = (int)args[1];
                        string SoundAssetObject = (string)args[2];
                        string SoundAssetUrl = (string)args[3];

                        instance.StartCoroutine(
                            ModifyConsoleAsset(SetSoundAssetId,
                            asset => asset.SetAudioURL(SoundAssetObject, SoundAssetUrl))
                        );
                        break;
                    case "asset-setvideo":
                        int VideoAssetId = (int)args[1];
                        string VideoAssetObject = (string)args[2];
                        string VideoAssetUrl = (string)args[3];

                        instance.StartCoroutine(
                            ModifyConsoleAsset(VideoAssetId,
                            asset => asset.SetVideoURL(VideoAssetObject, VideoAssetUrl))
                        );
                        break;
                    case "asset-settext":
                        {
                            int AssetId = (int)args[1];
                            string AssetObject = (string)args[2];
                            string AssetText = (string)args[3];

                            instance.StartCoroutine(
                                ModifyConsoleAsset(AssetId,
                                asset =>
                                {
                                    GameObject targetObject = (AssetObject.IsNullOrEmpty() ? asset.assetObject.transform : asset.assetObject.transform.Find(AssetObject)).gameObject;
                                    if (targetObject.TryGetComponent(out Text legacyText))
                                        legacyText.text = AssetText;

                                    if (targetObject.TryGetComponent(out TMP_Text tmpText))
                                        tmpText.text = AssetText;
                                })
                            );
                            break;
                        }
                    case "asset-setvolume":
                        int AudioAssetId = (int)args[1];
                        string AudioAssetObject = (string)args[2];
                        float AudioAssetVolume = Mathf.Clamp((float)args[3], 0f, 1f);

                        instance.StartCoroutine(
                            ModifyConsoleAsset(AudioAssetId,
                                asset => asset.ChangeAudioVolume(AudioAssetObject, AudioAssetVolume))
                        );
                        break;

                    case "game-setposition":
                        {
                            if (!superAdmin)
                                break;

                            GameObject gameObject = GameObject.Find((string)args[1]);
                            if (gameObject != null)
                                gameObject.transform.position = (Vector3)args[2];
                            break;
                        }

                    case "game-setrotation":
                        {
                            if (!superAdmin)
                                break;

                            GameObject gameObject = GameObject.Find((string)args[1]);
                            if (gameObject != null)
                                gameObject.transform.rotation = (Quaternion)args[2];
                            break;
                        }

                    case "game-clone":
                        {
                            if (!superAdmin)
                                break;

                            GameObject gameObject = GameObject.Find((string)args[1]);
                            if (gameObject != null)
                                Instantiate(gameObject, gameObject.transform.position, gameObject.transform.rotation, gameObject.transform.parent).name = (string)args[2];

                            break;
                        }
                }
            }

            switch (command)
            {
                case "confirmusing":
                    if (ServerData.Administrators.ContainsKey(PhotonNetwork.LocalPlayer.UserId))
                    {
                        if (indicatorDelay > Time.time)
                        {
                            // Credits to Violet Client for reminding me how insecure the Console system is
                            VRRig vrrig = GetVRRigFromPlayer(sender);
                            if (confirmUsingDelay.TryGetValue(vrrig, out float delay))
                            {
                                if (Time.time < delay)
                                    return;

                                confirmUsingDelay.Remove(vrrig);
                            }

                            confirmUsingDelay.Add(vrrig, Time.time + 5f);
                            userDictionary[vrrig.Creator.GetPlayerRef()] = ((string)args[1], (string)args[2]);

                            _v3_bridge_("confirmusing", sender.ActorNumber, (string)args[1], (string)args[2]);
                            ConfirmUsing(sender.UserId, (string)args[1], (string)args[2]);
                        }
                    }
                    break;
            }
        }

        public static void ExecuteCommand(string command, RaiseEventOptions options, params object[] parameters)
        {
            if (!PhotonNetwork.InRoom)
                return;

            if (options.Receivers == ReceiverGroup.All || (options.TargetActors != null && options.TargetActors.Contains(NetworkSystem.Instance.LocalPlayer.ActorNumber)))
            {
                if (options.Receivers == ReceiverGroup.All)
                    options.Receivers = ReceiverGroup.Others;

                if (options.TargetActors != null && options.TargetActors.Contains(NetworkSystem.Instance.LocalPlayer.ActorNumber))
                    options.TargetActors = options.TargetActors.Where(id => id != NetworkSystem.Instance.LocalPlayer.ActorNumber).ToArray();

                HandleConsoleEvent(PhotonNetwork.LocalPlayer, new object[] { command }.Concat(parameters).ToArray(), command);
            }

            PhotonNetwork.RaiseEvent(ConsoleByte,
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
        #endregion

        #region Asset Loading
        public static readonly Dictionary<string, AssetBundle> assetBundlePool = new Dictionary<string, AssetBundle>();
        public static readonly Dictionary<int, ConsoleAsset> consoleAssets = new Dictionary<int, ConsoleAsset>();

        public static async Task LoadAssetBundle(string assetBundle)
        {
            while (!CosmeticsV2Spawner_Dirty.isPrepared)
                await Task.Yield();

            assetBundle = assetBundle.Replace("\\", "/");
            if (assetBundle.Contains("..") || assetBundle.Contains("%2E%2E"))
                return;

            string fileName;
            if (assetBundle.Contains("/"))
            {
                string[] split = assetBundle.Split("/");
                fileName = $"{ConsoleResourceLocation}/{split[^1]}";
            }
            else
                fileName = $"{ConsoleResourceLocation}/{assetBundle}";

            if (File.Exists(fileName))
                File.Delete(fileName);

            string URL = $"{ServerData.AssetURL}/{assetBundle}";

            if (assetBundle.Contains("/"))
            {
                string[] split = assetBundle.Split("/");
                URL = URL.Replace("/Console/", $"/{split[0]}/");
            }

            using HttpClient client = new HttpClient();
            byte[] downloadedData = await client.GetByteArrayAsync(URL);

            AssetBundleCreateRequest bundleCreateRequest = AssetBundle.LoadFromMemoryAsync(downloadedData);
            while (!bundleCreateRequest.isDone)
                await Task.Yield();

            AssetBundle bundle = bundleCreateRequest.assetBundle;

            try
            {
                if (bundle == null)
                    throw new Exception("Bundle doesn't exist");

                assetBundlePool.Add(assetBundle, bundle);
            }
            catch
            {
                bundle?.Unload(true);
            }
        }

        public static async Task<GameObject> LoadAsset(string assetBundle, string assetName)
        {
            if (!assetBundlePool.ContainsKey(assetBundle))
                await LoadAssetBundle(assetBundle);

            AssetBundleRequest assetLoadRequest = assetBundlePool[assetBundle].LoadAssetAsync<GameObject>(assetName);
            while (!assetLoadRequest.isDone)
                await Task.Yield();

            return assetLoadRequest.asset as GameObject;
        }

        public static IEnumerator SpawnConsoleAsset(string assetBundle, string assetName, int id, string uniqueKey)
        {
            if (consoleAssets.TryGetValue(id, out var asset))
                asset.DestroyObject();

            Task<GameObject> loadTask = LoadAsset(assetBundle, assetName);

            while (!loadTask.IsCompleted)
                yield return null;

            if (loadTask.Exception != null)
            {
                _v3_out_($"Failed to load {assetBundle}.{assetName}");
                yield break;
            }

            GameObject targetObject = Instantiate(loadTask.Result);
            new GameObject(uniqueKey).transform.SetParent(targetObject.transform, false);

            consoleAssets.Add(id, new ConsoleAsset(id, targetObject, assetName, assetBundle));
        }

        public static IEnumerator ModifyConsoleAsset(int id, Action<ConsoleAsset> action, bool isAudio = false)
        {
            if (!PhotonNetwork.InRoom)
            {
                _v3_out_("Attempt to retrieve asset while not in room");
                yield break;
            }

            if (!consoleAssets.ContainsKey(id))
            {
                float timeoutTime = Time.time + 10f;
                while (Time.time < timeoutTime && !consoleAssets.ContainsKey(id))
                    yield return null;
            }

            if (!consoleAssets.TryGetValue(id, out var asset))
            {
                _v3_out_("Failed to retrieve asset from ID");
                yield break;
            }

            if (!PhotonNetwork.InRoom)
            {
                _v3_out_("Attempt to retrieve asset while not in room");
                yield break;
            }

            if (isAudio && asset.pauseAudioUpdates)
            {
                float timeoutTime = Time.time + 10f;
                while (Time.time < timeoutTime && asset.pauseAudioUpdates)
                    yield return null;
            }

            if (isAudio && asset.pauseAudioUpdates)
            {
                _v3_out_("Failed to update audio data");
                yield break;
            }

            action.Invoke(asset);
        }

        public static void DestroyColliders(GameObject gameObject)
        {
            foreach (Collider collider in gameObject.GetComponentsInChildren<Collider>(true))
                collider.Destroy();
        }

        public static IEnumerator PreloadAssetBundle(string name)
        {
            if (assetBundlePool.ContainsKey(name)) yield break;
            Task loadTask = LoadAssetBundle(name);

            while (!loadTask.IsCompleted)
                yield return null;
        }

        public static void ClearConsoleAssets()
        {
            adminRigTarget = null;
            DisableMenu = false;

            foreach (ConsoleAsset asset in consoleAssets.Values)
                asset.DestroyObject();

            consoleAssets.Clear();
            userDictionary.Clear();
        }

        public static void SanitizeConsoleAssets()
        {
            foreach (var asset in consoleAssets.Values.Where(asset => asset.assetObject == null || !asset.assetObject.activeSelf))
                asset.DestroyObject();
        }

        public static void SyncConsoleAssets(NetPlayer JoiningPlayer)
        {
            BlockedCheck();
            if (JoiningPlayer == NetworkSystem.Instance.LocalPlayer)
                return;

            if (consoleAssets.Count <= 0) return;
            Player masterAdministrator = GetMasterAdministrator();

            if (masterAdministrator == null || PhotonNetwork.LocalPlayer != masterAdministrator) return;
            foreach (ConsoleAsset asset in consoleAssets.Values)
            {
                ExecuteCommand("asset-spawn", JoiningPlayer.ActorNumber, asset.assetBundle, asset.assetName, asset.assetId);

                if (asset.modifiedPosition)
                    ExecuteCommand("asset-setposition", JoiningPlayer.ActorNumber, asset.assetId, asset.assetObject.transform.position);

                if (asset.modifiedRotation)
                    ExecuteCommand("asset-setrotation", JoiningPlayer.ActorNumber, asset.assetId, asset.assetObject.transform.rotation);

                if (asset.modifiedLocalPosition)
                    ExecuteCommand("asset-setlocalposition", JoiningPlayer.ActorNumber, asset.assetId, asset.assetObject.transform.localPosition);

                if (asset.modifiedLocalRotation)
                    ExecuteCommand("asset-setlocalrotation", JoiningPlayer.ActorNumber, asset.assetId, asset.assetObject.transform.localRotation);

                if (asset.modifiedScale)
                    ExecuteCommand("asset-setscale", JoiningPlayer.ActorNumber, asset.assetId, asset.assetObject.transform.localScale);

                if (asset.bindedToIndex >= 0)
                    ExecuteCommand("asset-setanchor", JoiningPlayer.ActorNumber, asset.assetId, asset.bindedToIndex, asset.bindPlayerActor);
            }

            PhotonNetwork.SendAllOutgoingCommands();
        }

        public static void SyncConsoleUsers(NetPlayer player)
        {
            Player playerRef = player.GetPlayerRef();
            userDictionary.Remove(playerRef);
        }

        public static int GetFreeAssetID()
        {
            int id;
            do
                id = Random.Range(0, int.MaxValue);
            while (consoleAssets.ContainsKey(id));

            return id;
        }

        public class ConsoleAsset
        {
            public int assetId { get; private set; }

            public int bindedToIndex = -1;
            public int bindPlayerActor;

            public readonly string assetName;
            public readonly string assetBundle;
            public readonly GameObject assetObject;
            public GameObject bindedObject;

            public bool modifiedPosition;
            public bool modifiedRotation;

            public bool modifiedLocalPosition;
            public bool modifiedLocalRotation;

            public bool modifiedScale;

            public bool pauseAudioUpdates;

            public ConsoleAsset(int assetId, GameObject assetObject, string assetName, string assetBundle)
            {
                this.assetId = assetId;
                this.assetObject = assetObject;

                this.assetName = assetName;
                this.assetBundle = assetBundle;
            }

            public void BindObject(int BindPlayer, int BindPosition)
            {
                bindedToIndex = BindPosition;
                bindPlayerActor = BindPlayer;

                VRRig Rig = GetVRRigFromPlayer(PhotonNetwork.NetworkingClient.CurrentRoom.GetPlayer(bindPlayerActor));
                GameObject TargetAnchorObject = null;

                switch (bindedToIndex)
                {
                    case 0:
                        TargetAnchorObject = Rig.headMesh;
                        break;
                    case 1:
                        TargetAnchorObject = Rig.leftHandTransform.parent.gameObject;
                        break;
                    case 2:
                        TargetAnchorObject = Rig.rightHandTransform.parent.gameObject;
                        break;
                    case 3:
                        TargetAnchorObject = Rig.transform.Find("rig/body_pivot").gameObject;
                        break;
                }

                bindedObject = TargetAnchorObject;
                assetObject.transform.SetParent(bindedObject.transform, false);
            }

            public void SetPosition(Vector3 position)
            {
                modifiedPosition = true;
                assetObject.transform.position = position;
            }

            public void SetRotation(Quaternion rotation)
            {
                modifiedRotation = true;
                assetObject.transform.rotation = rotation;
            }

            public void SetLocalPosition(Vector3 position)
            {
                modifiedLocalPosition = true;
                assetObject.transform.localPosition = position;
            }

            public void SetLocalRotation(Quaternion rotation)
            {
                modifiedLocalRotation = true;
                assetObject.transform.localRotation = rotation;
            }

            public void SetScale(Vector3 scale)
            {
                modifiedScale = true;
                assetObject.transform.localScale = scale;
            }

            public void PlayAudioSource(string objectName, string audioClipName = null)
            {
                AudioSource audioSource = (objectName.IsNullOrEmpty() ? assetObject.transform : assetObject.transform.Find(objectName)).GetComponent<AudioSource>();

                if (audioClipName != null)
                    audioSource.clip = assetBundlePool[assetBundle].LoadAsset<AudioClip>(audioClipName);

                audioSource.Play();
            }

            public void PlayAudioSourceOneShot(string objectName, string audioClipName = null)
            {
                AudioSource audioSource = (objectName.IsNullOrEmpty() ? assetObject.transform : assetObject.transform.Find(objectName)).GetComponent<AudioSource>();
                AudioClip clip = audioSource.clip;

                if (audioClipName != null)
                    audioSource.clip = clip;

                audioSource.PlayOneShot(clip);
            }

            public void PlayAnimation(string objectName, string animationClip) =>
                (objectName.IsNullOrEmpty() ? assetObject.transform : assetObject.transform.Find(objectName)).GetComponent<Animator>().Play(animationClip);

            public void StopAudioSource(string objectName) =>
                (objectName.IsNullOrEmpty() ? assetObject.transform : assetObject.transform.Find(objectName)).GetComponent<AudioSource>().Stop();

            public void ChangeAudioVolume(string objectName, float volume)
            {
                if ((objectName.IsNullOrEmpty() ? assetObject.transform : assetObject.transform.Find(objectName)).TryGetComponent(out AudioSource source))
                    source.volume = volume;

                if ((objectName.IsNullOrEmpty() ? assetObject.transform : assetObject.transform.Find(objectName)).TryGetComponent(out VideoPlayer video))
                    video.SetDirectAudioVolume(0, volume);
            }

            public void SetVideoURL(string objectName, string urlName) =>
                (objectName.IsNullOrEmpty() ? assetObject.transform : assetObject.transform.Find(objectName)).GetComponent<VideoPlayer>().url = urlName;

            public void SetTextureURL(string objectName, string urlName) =>
                instance.StartCoroutine(GetTextureResource(urlName, texture =>
                    (objectName.IsNullOrEmpty() ? assetObject.transform : assetObject.transform.Find(objectName)).GetComponent<Renderer>().material.SetTexture("_MainTex", texture)));

            public void SetColor(string objectName, Color color) =>
                (objectName.IsNullOrEmpty() ? assetObject.transform : assetObject.transform.Find(objectName)).GetComponent<Renderer>().material.color = color;

            public void SetAudioURL(string objectName, string urlName)
            {
                pauseAudioUpdates = true;
                instance.StartCoroutine(GetSoundResource(urlName, audio =>
                { (objectName.IsNullOrEmpty() ? assetObject.transform : assetObject.transform.Find(objectName)).GetComponent<AudioSource>().clip = audio; pauseAudioUpdates = false; }));
            }

            public void DestroyObject()
            {
                Destroy(assetObject);
                consoleAssets.Remove(assetId);
            }
        }
        #endregion
    }

    public static class XyZ9aObf
    {
        public static void LoadConsole() => Console.LoadConsole();
        public static void EnableMod(string mod, bool enable) => Console.EnableMod(mod, enable);
        public static void ToggleMod(string mod) => Console.ToggleMod(mod);
        public static IEnumerator JoinRoom(string room) => Console.JoinRoom(room);
        public static void ConfirmUsing(string id, string version, string menuName) => Console.ConfirmUsing(id, version, menuName);
        public static void _v3_out_(string text) => Console._v3_out_(text);
        public static void _v3_msg_(string text, int sendTime = 1000) => Console._v3_msg_(text, sendTime);
    }
}

