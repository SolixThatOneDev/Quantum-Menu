using ExitGames.Client.Photon;
using GorillaExtensions;
using GorillaLocomotion;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using static Quantum.Menu.Main;

namespace Quantum.Managers
{
    public class GunLib : MonoBehaviour
    {
        public static GunLibData data = new GunLibData();

        public class GunLibData
        {
            public bool IsGripping { get; set; }
            public bool IsTriggered { get; set; }
            public Vector3 HitPos { get; set; }
            public VRRig LockedRig { get; set; }
            public VRRig LastLockedRig { get; set; }
            public bool GunReady { get; set; }
            public Collider Collider { get; set; }

            public GunLibData(bool gripped = false, bool triggered = false, Vector3 hitpos = default, VRRig player = null, VRRig lastPlr = null, bool gunReady = false, Collider cPoint = null)
            {
                IsGripping = gripped;
                IsTriggered = triggered;
                HitPos = hitpos;
                LockedRig = player;
                LastLockedRig = lastPlr;
                GunReady = gunReady;
                Collider = cPoint;
            }
        }

        public void Start()
        {
            InitpObjs();
            Default = backgroundColor.GetColor(0);
            Selected = buttonColors[1].GetColor(0);
        }

        public static void ResetGL()
        {
            pObj?.SetActive(false);
            if (PhotonNetwork.InRoom)
                PhotonNetwork.RaiseEvent(22, null, new RaiseEventOptions { Receivers = ReceiverGroup.All }, SendOptions.SendUnreliable);
        }

        #region ptr declaration & vars
        public static GameObject pObj;
        public static LineRenderer gunLine;
        public static Vector3 determinePos, endPoint;
        public static Material pColor;
        public static readonly Dictionary<int, GameObject> GunPtr = new Dictionary<int, GameObject>();
        public static bool rightGunHand;

        public static Color Default;
        public static Color Selected;
        public static GameObject InitpObjs()
        {
            pObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            Destroy(pObj.GetComponent<Rigidbody>());
            Destroy(pObj.GetComponent<SphereCollider>());
            pObj.transform.localScale = (Vector3.one * .3f) / 2;
            Renderer pR = pObj.GetComponent<Renderer>();
            pR.material.shader = Shader.Find("GUI/Text Shader");
            pR.material.color = Default;
            pColor = pR.material;
            pObj.SetActive(false);
            gunLine = pObj.GetOrAddComponent<LineRenderer>();
            gunLine.material.shader = Shader.Find("GUI/Text Shader");
            gunLine.startWidth = .006f;
            gunLine.useWorldSpace = true;
            gunLine.material.color = Default;
            gunLine.positionCount = 51;
            gunLine.enabled = true;
            return pObj;
        }
        #endregion

        #region colliders
        public static readonly string[] bypassLayers =
        {
            "Gorilla Trigger",
            "Gorilla Boundary",
            "GorillaHand",
            "GorillaObject",
            "Zone",
            "Water",
            "GorillaCosmetics",
            "GorillaParticle",
        };
        public static readonly LayerMask BypassLayers = ~LayerMask.GetMask(bypassLayers);
        #endregion

        #region determine button hand/input
        public static bool DetermineGunHand(bool trigger) => rightGunHand ? (trigger ? rightTriggerPressed : rightGrab) : (trigger ? leftTriggerPressed : leftGrab);
        public static Transform DetermineHand() => rightGunHand ? GorillaLocomotion.GTPlayer.Instance.GetControllerTransform(false) : GorillaLocomotion.GTPlayer.Instance.GetControllerTransform(true);
        #endregion

        public static GunLibData GunInstance(bool lockable = false)
        {
            Vector3 pos = XRSettings.isDeviceActive ? DetermineHand().position - (DetermineHand().up / 4f) : GameObject.Find("Shoulder Camera").GetComponent<Camera>().ScreenPointToRay(UnityInput.mousePosition).origin;
            Vector3 dir = XRSettings.isDeviceActive ? -DetermineHand().up : GameObject.Find("Shoulder Camera").GetComponent<Camera>().ScreenPointToRay(UnityInput.mousePosition).direction;

            data.IsGripping = XRSettings.isDeviceActive ? DetermineGunHand(false) : UnityInput.GetMouseButton(1);
            data.IsTriggered = XRSettings.isDeviceActive ? DetermineGunHand(true) : UnityInput.GetMouseButton(0);

            if (data.IsGripping) //make null ptr pos & null rig (plr left) fallback
            {
                Physics.Raycast(pos, dir, out RaycastHit hit, float.PositiveInfinity, BypassLayers);
                if (lockable)
                {
                    VRRig rig = hit.collider.GetComponentInParent<VRRig>();
                    if (!data.LockedRig)
                    {
                        if (rig && data.IsTriggered)
                            data.LockedRig = rig;
                        determinePos = data.IsTriggered && rig && !rig.isOfflineVRRig ? data.LockedRig.transform.position : hit.point;
                        pColor.color = gunLine.material.color = Default;
                    }
                    else if (data.IsTriggered && data.LockedRig)
                    {
                        data.GunReady = true;
                        determinePos = data.HitPos = data.LockedRig.transform.position;
                        pColor.color = gunLine.material.color = Selected;
                    }
                    else
                    {
                        determinePos = hit.point;
                        data.GunReady = false;
                        data.LastLockedRig = data.LockedRig;
                        data.LockedRig = null;
                        pColor.color = gunLine.material.color = Default;
                    }
                }
                else
                {
                    data.HitPos = hit.point;
                    determinePos = data.HitPos;
                    pColor.color = gunLine.material.color = Default;
                    data.GunReady = data.IsTriggered;
                    data.Collider = hit.collider;
                }
                endPoint = Vector3.Lerp(endPoint, determinePos, Time.deltaTime * 12);
                Vector3 mid = (DetermineHand().position + endPoint) * .5f;
                for (int i = 0; i < gunLine.positionCount; i++)
                {
                    float t = i / (float)(gunLine.positionCount - 1);
                    Vector3 a = Vector3.Lerp(DetermineHand().position, mid, t);
                    Vector3 b = Vector3.Lerp(mid, endPoint, t);
                    gunLine.SetPosition(i, Vector3.Lerp(a, b, t));
                    //may remove l8r?
                }
                pObj.transform.position = gunLine.GetPosition(gunLine.positionCount - 1);
                pObj.SetActive(true);
            }
            else ResetGL();
            return data;
        }
    }
}



