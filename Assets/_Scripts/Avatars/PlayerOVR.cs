using System;
using System.Collections;
using Oculus.Avatar2;
using UnityEngine;

namespace MILab
{
    public class PlayerOVR : MonoBehaviour
    {
        public static PlayerOVR Instance {get; private set; }

        [SerializeField] private bool _isDesktop;
        [SerializeField] private OVRCameraRig _ovrCameraRig;
        [SerializeField] private PhotonAvatarEntity _localAvatar;
        [SerializeField] private OvrAvatarLipSyncContext _lipSyncContext;
        [SerializeField] private OvrAvatarManager _localAvatarManager;
        
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            LocalAvatarManager.GetComponent<SampleInputManager>()._ovrCameraRig = OvrCameraRig;
        }

        private IEnumerator Start()
        {
            if (_isDesktop)
            {
                while (!LocalAvatarEntity)
                {
                    yield return null;
                }
                LocalAvatarEntity.gameObject.SetActive(false);
            }
        }

        [Button]
        public static void SelectLocalAvatar(int num)
        {
	        if (num == 0) num = 100 + LocalCustomAvatar.TemocAvatarNumber;
	        else if (num == 1) num = 100 + LocalCustomAvatar.ProfessorAvatarNumber;
            else num -= 2;
            LocalAvatarEntity.ChangeAvatar(num);
        }

        public static OVRCameraRig OvrCameraRig
        {
            get
            {
                var i = Instance;
                if (i._ovrCameraRig) return i._ovrCameraRig;
                i._ovrCameraRig = i.GetComponent<OVRCameraRig>();
                if (i._ovrCameraRig) return i._ovrCameraRig;
                i._ovrCameraRig = FindObjectOfType<OVRCameraRig>();
                return i._ovrCameraRig;
            }
        }

        public static PhotonAvatarEntity LocalAvatarEntity
        {
            get
            {
                var i = Instance;
                if (i._localAvatar) return i._localAvatar;
                var parent = GameObject.Find("LocalAvatar");
                if (parent) i._localAvatar = parent.GetComponent<PhotonAvatarEntity>();
                return i._localAvatar;
            }
        }

        public static OvrAvatarLipSyncContext LocalAvatarLipSync
        {
            get
            {
                var i = Instance;
                if (i._lipSyncContext) return i._lipSyncContext;
                i._lipSyncContext = FindObjectOfType<OvrAvatarLipSyncContext>();
                return i._lipSyncContext;
            }
        }

        public static OvrAvatarManager LocalAvatarManager
        {
            get
            {
                var i = Instance;
                if (i._localAvatarManager) return i._localAvatarManager;
                i._localAvatarManager = FindObjectOfType<OvrAvatarManager>();
                return i._localAvatarManager;
            }
        }
    }
}