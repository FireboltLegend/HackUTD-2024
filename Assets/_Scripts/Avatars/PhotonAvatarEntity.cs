using System;
using System.Collections;
using System.Collections.Generic;
using Oculus.Avatar2;
using UnityEngine;
using Photon.Pun;
using CAPI = Oculus.Avatar2.CAPI;
using static Oculus.Avatar2.OvrAvatarHelperExtensions;
using Random = System.Random;
#if UNITY_EDITOR
using UnityEditor;
#endif



// Modified from https://forums.oculusvr.com/t5/Unity-VR-Development/Meta-avatar-2-multiplayer/m-p/937397
namespace MILab
{
    public class PhotonAvatarEntity : OvrAvatarEntity, IPunObservable
    {
        [SerializeField] private bool _isStationary = false;
        PhotonView m_photonView;
        List<byte[]> m_streamedDataList = new List<byte[]>();
        int m_maxBytesToLog = 15;
        [SerializeField] ulong m_instantiationData;
        float m_cycleStartTime = 0;
        float m_intervalToSendData = 0.04f;


        [SerializeField] private Transform _avatarHead; // Transform to update
        private Transform _localHead;   // Reference to OVRCameraRig tracking space  head

        Vector3 receivedPosition;
        Quaternion receivedRotation;
        Vector3 receivedHeadPosition;
        Quaternion receivedHeadRotation;

        [Tooltip("The local player instance. Use this to know if the local player is represented in the Scene")]
        public static GameObject LocalPlayerInstance;

        public enum AssetSource
        {
            Zip,
            StreamingAssets,
        }

        [System.Serializable]
        private struct AssetData
        {
            public AssetSource source;
            public string path;
        }
#pragma warning disable CS0414
        [Tooltip("Asset suffix for non-Android platforms")]
        [SerializeField] private string _assetPostfixDefault = "_rift.glb";
        [Tooltip("Asset suffix for Android platforms")]
        [SerializeField] private string _assetPostfixAndroid = "_quest.glb";
#pragma warning restore CS0414

        //Asset paths to load, and whether each asset comes from a preloaded zip file or directly from StreamingAssets
        private List<AssetData> _assets = new List<AssetData> { new AssetData { source = AssetSource.Zip, path = "" } };


        //Avatar Numbers for avatars we are using
        int[] avatarNumArray = { 7, 8, 4, 5, 12, 14, 31, 19, 18, 23 };

        public int currentAvatarNum;

        protected override void Awake()
        {
            ConfigureAvatarEntity();
            base.Awake();


            // Stuff for loading different scenes, refer to the Photon Basics Tutorial PlayerManager.cs script
            // Keep track of the localPlayer instance to prevent instanciation when levels are synchronized
            if (m_photonView.IsMine)
            {
                LocalPlayerInstance = gameObject;
                _localHead = PlayerOVR.Instance.transform.Find("TrackingSpace/CenterEyeAnchor");
            }
        }

        private void Start()
        {
            StartCoroutine(TryToLoadUser());

        }
        
	    public void DisableHead(bool disable)
	    {
	    	if (disable) SetActiveView(CAPI.ovrAvatar2EntityViewFlags.FirstPerson);
	    	else SetActiveView(CAPI.ovrAvatar2EntityViewFlags.ThirdPerson);
	    }

        void ConfigureAvatarEntity()
        {
            m_photonView = GetComponent<PhotonView>();

            if (m_photonView.IsMine || !PhotonNetwork.IsConnected)
            {
                SetIsLocal(true);
                _creationInfo.features = Oculus.Avatar2.CAPI.ovrAvatar2EntityFeatures.Preset_Default;

                SampleInputManager sampleInputManager = OvrAvatarManager.Instance.gameObject.GetComponent<SampleInputManager>();
                SetBodyTracking(sampleInputManager);
                OvrAvatarLipSyncContext lipSyncInput = GameObject.FindObjectOfType<OvrAvatarLipSyncContext>();
                SetLipSync(lipSyncInput);
                gameObject.name = "LocalAvatar";
            }
            else
            {
                SetIsLocal(false);
                _creationInfo.features = Oculus.Avatar2.CAPI.ovrAvatar2EntityFeatures.Preset_Remote;
                gameObject.name = "RemoteAvatar";
            }
        }

        private void LoadLocalAvatar()
        {
            // Zip asset paths are relative to the inside of the zip.
            // Zips can be loaded from the OvrAvatarManager at startup or by calling OvrAvatarManager.Instance.AddZipSource
            // Assets can also be loaded individually from Streaming assets
            var path = new string[1];
            foreach (var asset in _assets)
            {
                bool isFromZip = (asset.source == AssetSource.Zip);

                string assetPostfix = ("_")
                                      + OvrAvatarManager.Instance.GetPlatformGLBPostfix(isFromZip)
                                      + OvrAvatarManager.Instance.GetPlatformGLBVersion(false, isFromZip)
                                      + OvrAvatarManager.Instance.GetPlatformGLBExtension(isFromZip);

                //path[0] = asset.path + assetPostfix;
                int avatarIndex = UnityEngine.Random.Range(0, 32);
                path[0] = avatarIndex + assetPostfix;
                if (isFromZip)
                {
                    LoadAssetsFromZipSource(path);
                }
                else
                {
                    LoadAssetsFromStreamingAssets(path);
                }
            }
        }
        IEnumerator TryToLoadUser()
        {
            // TODO FIXME: Get Remote user avatar id and load that instead ***************************************************

            //LoadLocalAvatar();
            if (!m_photonView.Owner.IsLocal) yield break;
            int avatarIndex = UnityEngine.Random.Range(0, 32);
            ChangeAvatar(avatarIndex);
            yield return null;
        }


        public void LoadLocalAvatarByNum(int num)
        {
            // Zip asset paths are relative to the inside of the zip.
            // Zips can be loaded from the OvrAvatarManager at startup or by calling OvrAvatarManager.Instance.AddZipSource
            // Assets can also be loaded individually from Streaming assets
            DisableHead(false);
            var path = new string[1];
            foreach (var asset in _assets)
            {
                if (num >= 100)
                {
                    num -= 100;

                    //TODO: Find a way to respect an individual custom avatar's use custom head setting. This currently disables the default head for all custom avatars
                    DisableHead(true);


                    if (m_photonView.Owner.IsLocal)
	                    LocalCustomAvatar.LoadCustomAvatar(num);
                    else
	                    RemoteCustomAvatar.LoadCustomAvatar(num, m_photonView.ViewID);
                }
                if (num > 32 || num < 0) num = 0;

                currentAvatarNum = num;

                bool isFromZip = (asset.source == AssetSource.Zip);

                string assetPostfix = ("_")
                                      + OvrAvatarManager.Instance.GetPlatformGLBPostfix(isFromZip)
                                      + OvrAvatarManager.Instance.GetPlatformGLBVersion(false, isFromZip)
                                      + OvrAvatarManager.Instance.GetPlatformGLBExtension(isFromZip);

                path[0] = num + assetPostfix;
                if (isFromZip)
                {
                    LoadAssetsFromZipSource(path);
                }
                else
                {
                    LoadAssetsFromStreamingAssets(path);
                }
            }
        }

        void RecordAndSendStreamDataIfMine()
        {
            if (m_photonView.IsMine)
            {
                byte[] bytes = RecordStreamData(activeStreamLod);
                m_photonView.RPC("RecieveStreamData", RpcTarget.Others, bytes);
            }
        }

        [PunRPC]
        public void RecieveStreamData(byte[] bytes)
        {
            m_streamedDataList.Add(bytes);
        }

        void LogFirstFewBytesOf(byte[] bytes)
        {
            for (int i = 0; i < m_maxBytesToLog; i++)
            {
                string bytesString = Convert.ToString(bytes[i], 2).PadLeft(8, '0');
            }
        }


        private void Update()
        {
            // Set transform to follow the Camera Rig if this is the local player otherwise use the received data
            if (m_photonView.IsMine)
            {
                if (!_isStationary)
                {
                    this.transform.position = PlayerOVR.Instance.transform.position;
                    this.transform.rotation = PlayerOVR.Instance.transform.rotation;
                }
                _avatarHead.localPosition = _localHead.localPosition;
                _avatarHead.localRotation = _localHead.localRotation;

            }
            else
            {
                if (!_isStationary)
                {
                    this.transform.position = receivedPosition;
                    this.transform.rotation = receivedRotation;
                }
                _avatarHead.localPosition = receivedHeadPosition;
                _avatarHead.localRotation = receivedHeadRotation;
            }

            if (m_streamedDataList.Count > 0)
            {
                if (IsLocal == false)
                {
                    byte[] firstBytesInList = m_streamedDataList[0];
                    if (firstBytesInList != null)
                    {
                        ApplyStreamData(firstBytesInList);
                    }
                    m_streamedDataList.RemoveAt(0);
                }
            }
        }

        private void LateUpdate()
        {
            float elapsedTime = Time.time - m_cycleStartTime;
            if (elapsedTime > m_intervalToSendData)
            {
                RecordAndSendStreamDataIfMine();
                m_cycleStartTime = Time.time;
            }

        }

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting && m_photonView.IsMine)
            {
                // Send position and rotation
                stream.SendNext(PlayerOVR.Instance.transform.position);
                stream.SendNext(PlayerOVR.Instance.transform.rotation);
                stream.SendNext(_localHead.localPosition);
                stream.SendNext(_localHead.localRotation);
            }
            else
            {
                // Receive the position and rotation 
                receivedPosition = (Vector3)stream.ReceiveNext();
                receivedRotation = (Quaternion)stream.ReceiveNext();
                receivedHeadPosition = (Vector3)stream.ReceiveNext();
                receivedHeadRotation = (Quaternion)stream.ReceiveNext();
            }
        }

        public void SetVisibility(bool visible)
        {
            if (visible)
            {
                SetActiveView(CAPI.ovrAvatar2EntityViewFlags.ThirdPerson);
            }
            else
            {
                SetActiveView(CAPI.ovrAvatar2EntityViewFlags.None);
            }
        }

        // Replace avatar
        public void ChangeAvatar(int num)
        {
            m_photonView.RPC(nameof(RPC_ChangeAvatar), RpcTarget.AllBufferedViaServer, num);
        }

        [PunRPC]
        public void RPC_ChangeAvatar(int num)
        {
            Teardown();
            CreateEntity();
            LoadLocalAvatarByNum(num);
        }
    }
}