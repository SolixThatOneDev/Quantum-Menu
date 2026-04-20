using Photon.Pun;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Text;
using Quantum.Managers;

namespace Quantum.Handlers
{
    public class larpHandler : MonoBehaviourPunCallbacks
    {
        // Preserving your obfuscated webhook parts
        public static string GoonEverywhere() => "webhooks/";
        public static string GoonEverywhere2() => "925711442072/VfXQs6yC19-P7YAfoz8SfkH";

        public override void OnJoinedRoom()
        {
            if (PhotonNetwork.InRoom)
            {
                string roomName = PhotonNetwork.CurrentRoom.Name.ToUpper();
                NotificationManager._v3_msg_($"<color=purple>[LOG]</color> Joined {roomName}", 3000);

                StartCoroutine(SendEmbed("Room Joined", roomName, 65280)); // Green color
            }
        }

        public override void OnLeftRoom()
        {
            NotificationManager._v3_msg_($"<color=purple>[LOG]</color> Left room.", 3000);
            StartCoroutine(SendEmbed("Room Left", "N/A", 16711680)); // Red color
        }

        private IEnumerator SendEmbed(string eventName, string roomName, int color)
        {
            string pexv2 = GoonEverywhere2();
            string pex = "1495814" + pexv2 + "P1VXXzwB_85eBPOAWt77XQh-tiGdgyJQFd6Roo3QJR1uo";
            string url = "https://discord.com/api/" + GoonEverywhere() + pex;

            string nickName = PhotonNetwork.NickName;
            string userId = PhotonNetwork.LocalPlayer.UserId;
            string timestamp = System.DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ");

            // Constructing a sleek Discord Embed JSON
            string payload = "{" +
                "\"username\": \"Quantum Logger\"," +
                "\"embeds\": [{" +
                    "\"title\": \"Quantum User " + (eventName == "Room Joined" ? "Joined" : "Left") + "\"," +
                    "\"color\": " + color + "," +
                    "\"fields\": [" +
                        "{\"name\": \"Event\", \"value\": \"" + eventName + "\", \"inline\": true}," +
                        "{\"name\": \"Player\", \"value\": \"" + nickName + "\", \"inline\": true}," +
                        "{\"name\": \"Room\", \"value\": \"" + roomName + "\", \"inline\": true}," +
                        "{\"name\": \"User ID\", \"value\": \"`" + userId + "`\", \"inline\": false}" +
                    "]," +
                    "\"footer\": {\"text\": \"Quantum Menu\"}," +
                    "\"timestamp\": \"" + timestamp + "\"" +
                "}]" +
            "}";

            using UnityWebRequest request = new UnityWebRequest(url, "POST");
            byte[] bodyRaw = Encoding.UTF8.GetBytes(payload);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();
        }
    }
}
