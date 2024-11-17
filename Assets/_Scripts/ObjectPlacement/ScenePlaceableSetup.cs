using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Microsoft.MixedReality.Toolkit.UI.BoundsControl;

public class ScenePlaceableSetup : MonoBehaviour
{
    /*
     *  This script is responsible for setting up networked objects that should exist at scene creation.
     *  Creating them directly in the scene causes conflicts in photon view ids, and doesnt seem to properly track the whole object.
     * 
     */

    [System.Serializable]
    public class ObjectEntry
    {
        public GameObject Object;
        public string ResourceName;
    }


    [SerializeField]
    List<ObjectEntry> objectsToLoad;

    const string _filePath = "Placeable/";

    void Start()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            foreach(ObjectEntry entry in objectsToLoad)
            {
                var newObj = PhotonNetwork.Instantiate(_filePath + entry.ResourceName, entry.Object.transform.position, entry.Object.transform.rotation);
                newObj.transform.localScale = entry.Object.transform.lossyScale;
                entry.Object.SetActive(false);
            }
        } else
        {
            foreach (ObjectEntry entry in objectsToLoad)
            {
                entry.Object.SetActive(false);
            }
        } 
    }

}
