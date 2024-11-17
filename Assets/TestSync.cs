using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class TestSync : MonoBehaviourPun
{
    [SerializeField] GameObject cube;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (photonView.IsMine)
        {
            photonView.RPC("SyncObjectTransform", RpcTarget.All, cube.transform.position, cube.transform.rotation, cube.transform.localScale);
        }
    }

    [PunRPC]
    void SyncObjectTransform(Vector3 position, Quaternion rotation, Vector3 scale)
    {
        // Update the object's transform for all players
        cube.transform.position = position;
        cube.transform.rotation = rotation;
        cube.transform.localScale = scale;
    }

}
