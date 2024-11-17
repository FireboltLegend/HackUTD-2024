using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Microsoft.MixedReality.Toolkit.Input;

namespace MILab
{

    public class Router : MonoBehaviour, IPunObservable, IMixedRealityPointerHandler
    {

        private Vector3 _pos;
        private Quaternion _rot;
        private Vector3 _localScale;
        private PhotonView _photonView;

        private void Awake()
        {
            _photonView = GetComponent<PhotonView>();
        }

        // Start is called before the first frame update
        void Start()
        {
            _pos = transform.position;
            _rot = transform.rotation;
            _localScale = transform.localScale;
        }

        // Update is called once per frame
        void Update()
        {
            if (_photonView.IsMine) { }
            else
            {
                transform.position = _pos;
                transform.rotation = _rot;
                transform.localScale = _localScale;
            }
        }

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
            {
                stream.SendNext(transform.position);
                stream.SendNext(transform.rotation);
                stream.SendNext(transform.localScale);
            }
            else
            {
                _pos = (Vector3)stream.ReceiveNext();
                _rot = (Quaternion)stream.ReceiveNext();
                _localScale = (Vector3)stream.ReceiveNext();
            }
        }

        public void OnPointerDown(MixedRealityPointerEventData eventData)
        {
            _photonView.TransferOwnership(PhotonNetwork.LocalPlayer);
        }

        public void OnPointerDragged(MixedRealityPointerEventData eventData)
        {
        }

        public void OnPointerUp(MixedRealityPointerEventData eventData)
        {
        }

        public void OnPointerClicked(MixedRealityPointerEventData eventData)
        {
            _photonView.TransferOwnership(PhotonNetwork.LocalPlayer);
        }
    }
}
