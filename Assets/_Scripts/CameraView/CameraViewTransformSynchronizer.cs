using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

namespace MILab
{
    public class CameraViewTransformSynchronizer : MonoBehaviourPun
    {
        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            if (photonView.IsMine)
            {
                photonView.RPC("SyncObjectTransform", RpcTarget.All, transform.position, transform.rotation, transform.localScale);
            }
        }

        [PunRPC]
        void SyncObjectTransform(Vector3 position, Quaternion rotation, Vector3 scale)
        {
            // Update the object's transform for all players
            transform.position = position;
            transform.rotation = rotation;
            transform.localScale = scale;
        }
    }
}