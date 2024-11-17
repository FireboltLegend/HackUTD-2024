using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Photon.Pun;

namespace MILab
{
    // This script represents the hand menu seen by the local  user
    public class HealthMenuHand : MonoBehaviour, IPunObservable
    {
        // Handles the networked health data
        [SerializeField] private HealthChannelSO _healthChannelSO;
        [SerializeField] private bool _isDisplayingData = false;
        [SerializeField, ReadOnly] private int _dataIndex = 0;
        [SerializeField] private string _pulsoidToken = "2f1c6484-a7ee-4a7e-99eb-94df7322e1d7";
        private PhotonView _photonView;


        private void Awake()
        {
            _photonView = GetComponent<PhotonView>();
        }

        private void OnValidate()
        {
            if (_photonView is not null)
            {
                _photonView.RPC(nameof(RPC_ChangePulsoidToken), RpcTarget.AllBuffered, _pulsoidToken);
            }
        }

        [Button(Mode = ButtonMode.InPlayMode)]
        public void ActivateDataDisplay(int index)
        {
            Debug.Log("Activate Data Display: " + index);
            if (_isDisplayingData && index == _dataIndex)
            {
                _isDisplayingData = false;
            }
            else
            {
                _isDisplayingData = true;
            }
            _dataIndex = index;
            _photonView.RPC(nameof(RPC_ActivateData), RpcTarget.AllBuffered, index, _isDisplayingData);
        }


        [PunRPC]
        private void RPC_ActivateData(int index, bool display)
        {
            Debug.Log("RPC Response Activate Data: " + index + " " + display);
            _healthChannelSO.RaiseHideAll();

            // Call the display func
            if (display)
            {
                if (index == 0)
                {
                    _healthChannelSO.RaiseShowHealth();
                }
                else if (index == 1)
                {
                    _healthChannelSO.RaiseShowFitness();
                }
                else if (index == 2)
                {
                    _healthChannelSO.RaiseShowSleep();
                }
            }
        }

        [PunRPC]
        private void RPC_ChangePulsoidToken(string token)
        {
            _pulsoidToken = token;
            _healthChannelSO.PulsoidToken = token;
        }

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting && _photonView.IsMine)
            {
                // Send position and rotation
                stream.SendNext(_isDisplayingData);
                stream.SendNext(_dataIndex);
            }
            else
            {
                // Receive the position and rotation 
                _isDisplayingData = (bool)stream.ReceiveNext();
                _dataIndex = (int)stream.ReceiveNext();
            }
        }
    }
}
