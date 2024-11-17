using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Microsoft.MixedReality.Toolkit.UI.BoundsControl;
using Microsoft.MixedReality.Toolkit.Input;

public class PlaceableSpawner : MonoBehaviour, IPunObservable
{
    private PhotonView _photonView;
    const string _filePath = "Placeable/";
    private void Awake()
    {
        _photonView = GetComponent<PhotonView>();
    }


    public void SpawnObjectEditMode(string obj)
    {
        var newObj = PhotonNetwork.Instantiate(_filePath + obj, Camera.main.transform.position + (Camera.main.transform.forward * .3f), Quaternion.identity, 0);
        newObj.GetComponent<BoundsControl>().enabled = true;
        newObj.GetComponent<Microsoft.MixedReality.Toolkit.UI.ObjectManipulator>().enabled = true;
    }

    public void SpawnObject(string obj)
    {
        var newObj = PhotonNetwork.Instantiate(_filePath + obj, Camera.main.transform.position + (Camera.main.transform.forward * .3f), Quaternion.identity, 0);
    }


    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        // TODO: Synchronize the current state
    }
}
