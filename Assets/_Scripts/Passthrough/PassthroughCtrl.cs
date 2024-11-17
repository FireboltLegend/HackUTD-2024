using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MILab;

public class PassthroughCtrl : MonoBehaviour
{
    [SerializeField] GameObject roomBaseEmpty;
    [SerializeField] GameObject mirror;
    OVRPassthroughLayer ovrPTLayer;

    [Header("Debug")]
    [SerializeField, ReadOnly] private bool _passthroughActive;
    public bool PTActive => _passthroughActive;

    // Start is called before the first frame update
    void Start()
    {
        ovrPTLayer = PlayerOVR.OvrCameraRig.GetComponent<OVRPassthroughLayer>();
        _passthroughActive = ovrPTLayer.textureOpacity > 0.1f;
    }

    public void TogglePassthrough(bool passthroughActive)
    {
        _passthroughActive = !_passthroughActive; //passthroughActive;
        roomBaseEmpty.SetActive(!_passthroughActive);
        mirror.SetActive(!_passthroughActive);
        if (_passthroughActive) ovrPTLayer.textureOpacity = 0.3f;
        else ovrPTLayer.textureOpacity = 0f;
        //_passthroughActive = passthroughActive;
    }

    [Button]
    public void TogglePassthroughShowRoom(bool passthroughActive)
    {
        _passthroughActive = passthroughActive;
        //roomBaseEmpty.SetActive(!_passthroughActive);
        mirror.SetActive(!_passthroughActive);
        if (_passthroughActive) ovrPTLayer.textureOpacity = 0.3f;
        else ovrPTLayer.textureOpacity = 0f;
        //_passthroughActive = passthroughActive;
    }
}
