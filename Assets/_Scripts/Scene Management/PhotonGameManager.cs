using System.Collections;
using System.Collections.Generic;
using Photon.Realtime;
using Photon.Pun;
using UnityEngine;

// This game object manages the Photon connection
// This script requires that an OVRCameraRig be in the scene and have the PlayerOVR script attached

namespace MILab
{
    public class PhotonGameManager : MonoBehaviourPunCallbacks
    {
        [Tooltip("The maximum number of players per room")]
        [SerializeField]
        private byte maxPlayersPerRoom = 8;

        [Tooltip("The name of the Photon room to create or join")]
        [SerializeField]
        private string photonRoomName = "TestRoom";

        [Tooltip("Prefab that represents the player avatars")]
        [SerializeField]
        private GameObject networkedPlayerPrefab;

        public static GameObject localPlayer;

        // Singleton reference
        public static PhotonGameManager Instance { get; private set; }


        // Flag to track connection progress
        private bool isConnecting;

        // This client's version number. Users are separated from each other by gameVersion (which allows you to make breaking changes).
        private string gameVersion = "1";

        private void Awake()
        {
            // If there is an instance, and it's not me, delete myself.
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            Connect();
        }

        // Called from within the OnJoinedRoom callback
        private void SetUpPlayer()
        {
            if (networkedPlayerPrefab == null)
            {
                Debug.LogError("Missing playerPrefab in PhotonGameManager");
            }
            else if (PlayerOVR.Instance == null)
            {
                Debug.LogError("Missing OVR Player Controller in PhotonGameManager");
            }
            else
            {
                Debug.Log("Connected and instantiating player");
                localPlayer = PhotonNetwork.Instantiate(networkedPlayerPrefab.name, Vector3.zero, Quaternion.identity, 0);
                SceneController.Instance.GetComponent<PhotonView>().RPC("RPC_AddAvatar", RpcTarget.AllBuffered, PhotonNetwork.LocalPlayer.ActorNumber, localPlayer.GetComponent<PhotonView>().ViewID);
            }
        }

        private void Connect()
        {
            isConnecting = true;

            // we check if we are connected or not, we join if we are , else we initiate the connection to the server.
            if (PhotonNetwork.IsConnected)
            {
                PhotonNetwork.JoinOrCreateRoom(photonRoomName, new RoomOptions { MaxPlayers = this.maxPlayersPerRoom }, null);
            }
            else
            {
                // #Critical, we must first and foremost connect to Photon Online Server.
                PhotonNetwork.ConnectUsingSettings();
                PhotonNetwork.GameVersion = this.gameVersion;
            }
        }

        #region MonoBehaviourPunCallbacks CallBacks
        // Called after the connection to the master is established and authenticated
        public override void OnConnectedToMaster()
        {
            // we don't want to do anything if we are not attempting to join a room. 
            // this case where isConnecting is false is typically when you lost or quit the game, when this level is loaded, OnConnectedToMaster will be called, in that case
            // we don't want to do anything.
            if (isConnecting)
            {
                Debug.Log("OnConnectedToMaster: Next -> try to Join Room");

                // #Critical: The first we try to do is to join a potential existing room. If there is, good, else, we'll be called back with OnJoinRoomFailed()
                PhotonNetwork.JoinOrCreateRoom(photonRoomName, new RoomOptions { MaxPlayers = this.maxPlayersPerRoom, CleanupCacheOnLeave = false }, null);
            }
        }

        public override void OnJoinRoomFailed(short returnCode, string message)
        {
            Debug.Log("Failed to Join Room");
        }
        
        // Called after disconnecting from the Photon server.
        public override void OnDisconnected(DisconnectCause cause)
        {
            Debug.LogError("Photon Game Manager Disconnected: " + cause);
            isConnecting = false;
        }

        // Called when entering a room (by creating or joining it). Called on all clients (including the Master Client).
        public override void OnJoinedRoom()
        {
            Debug.Log("Green>OnJoinedRoom with " + PhotonNetwork.CurrentRoom.PlayerCount + " Player(s)");
            Debug.Log("OnJoinedRoom() called by PUN. Now this client is in a room.\nFrom here on, your game would be running.");
            SetUpPlayer();

            //SceneController.Instance.AsyncAdditiveSceneLoad(firstSceneName);
            //SceneController.Instance.RPC_LoadScene(PhotonNetwork.LocalPlayer.ActorNumber, firstSceneName);
            //SceneController.Instance.GetComponent<PhotonView>().RPC("RPC_LoadScene", RpcTarget.AllBuffered, PhotonNetwork.LocalPlayer.ActorNumber, firstSceneName);
            SceneController.Instance.LoadInitialScene();
        }
        #endregion

    }

}