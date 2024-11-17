using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon;
using Photon.Pun;

namespace MILab.MetaverseBase
{
    public class Bread : MonoBehaviour//, IPunObservable
    {
        private Vector3 lastPos;
        private Quaternion lastRot;

        PhotonView m_photonView;

        void Awake()
        {
            m_photonView = GetComponent<PhotonView>();
            if(PhotonNetwork.IsMasterClient)
            {
                m_photonView.RPC("ChangeTransform", RpcTarget.AllBuffered, this.transform.position, this.transform.rotation);
            }
            else
            {
                // gameObject.GetComponent<Rigidbody>().useGravity = false;
            }
        }

        void Update()
        {
            if (!(lastPos.Equals(this.transform.position)))
            {
                m_photonView.RPC("ChangeTransform", RpcTarget.AllBuffered, this.transform.position, this.transform.rotation);
                Debug.Log("--------------- Call ChangeTransform RPC");
            }
        }
        // public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        // {
        //     if (stream.IsWriting && PhotonNetwork.IsMasterClient)
        //     {
        //         stream.SendNext(this.transform.position);
        //         stream.SendNext(this.transform.rotation);
        //     }
        //     else
        //     {
        //         this.transform.position = (Vector3)stream.ReceiveNext();
        //         this.transform.rotation = (Quaternion)stream.ReceiveNext();
        //         this.lastPos = this.transform.position;
        //         this.lastRot = this.transform.rotation;

        //         if(!gameObject.GetComponent<Rigidbody>().useGravity)
        //             gameObject.GetComponent<Rigidbody>().useGravity = true;
        //     }
        // }

        [PunRPC]
        public void ChangeTransform(Vector3 pos, Quaternion rot)
        {
            this.transform.position = pos;
            this.transform.rotation = rot;
            this.lastPos = this.transform.position;
            this.lastRot = this.transform.rotation;
            Debug.Log("+++++++++++++++++++ Changed bread postion");

            // if(!gameObject.GetComponent<Rigidbody>().useGravity)
            //     gameObject.GetComponent<Rigidbody>().useGravity = true;
        }
    }
}