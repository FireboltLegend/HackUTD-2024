using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class RemoteThemeActivation : MonoBehaviour, IPunObservable
{
    private PhotonView _photonView;
    [SerializeField] private GameObject _aquariumActive;
    [SerializeField] private GameObject _wifiActive;
    [SerializeField] private GameObject _windActive;
    [SerializeField] private GameObject _passthroughBtnBckPlateActive;

    [SerializeField, ReadOnly] private ThemeController _controller;
    [SerializeField, ReadOnly] private PassthroughCtrl _passthroughController;

    private bool Any => Aquarium || Wifi || Wind || Passthrough;
    private bool Aquarium => _controller.AquariumActive;
    private bool Wifi => _controller.WifiActive;
    private bool Wind => _controller.WindActive;
    private bool Passthrough => _passthroughController.PTActive;

    private void Awake()
    {
        _photonView = GetComponent<PhotonView>();
    }

    private void Start()
    {
        PTCheck();
        if (_aquariumActive) _aquariumActive.SetActive(false);
        if (_wifiActive) _wifiActive.SetActive(false);
        if (_windActive) _windActive.SetActive(false);
        if (_passthroughBtnBckPlateActive) _passthroughBtnBckPlateActive.SetActive(false);
    }

    private bool Check()
    {
        if (_controller) return true;
        _controller = FindObjectOfType<ThemeController>();
        return _controller;
    }

    private bool PTCheck()
    {
        Check();
        if (_passthroughController) return true;
        _passthroughController = FindObjectOfType<PassthroughCtrl>();
        return _passthroughController;
    }

    [Button]
    public void ToggleAquarium()
    {
        _photonView.RPC(nameof(RPC_ToggleAquarium), RpcTarget.AllBufferedViaServer);
    }

    [Button]
    public void ToggleWifi()
    {
        _photonView.RPC(nameof(RPC_ToggleWifi), RpcTarget.AllBufferedViaServer);
    }

    [Button]
    public void ToggleWind()
    {
        _photonView.RPC(nameof(RPC_ToggleWind), RpcTarget.AllBufferedViaServer);
    }

    [Button]
    public void TogglePassthrough()
    {
        _photonView.RPC(nameof(RPC_TogglePassthrough), RpcTarget.AllBufferedViaServer);
    }


    #region Pun RPCs
    [PunRPC]
    public void RPC_ToggleAquarium()
    {
        Check();
        if (!Aquarium && Any) return;
        if (Check()) _controller.ToggleAquarium();
        if (_aquariumActive) _aquariumActive.SetActive(Aquarium);
    }

    [PunRPC]
    public void RPC_ToggleWifi()
    {
        Check();
        if (!Wifi && Any) return;
        if (Check()) _controller.ToggleWifi();
        if (_wifiActive) _wifiActive.SetActive(Wifi);
    }

    [PunRPC]
    public void RPC_ToggleWind()
    {
        Check();
        if (!Wind && Any) return;
        if (Check()) _controller.ToggleWind();
        if (_windActive) _windActive.SetActive(Wind);
    }

    [PunRPC]
    public void RPC_TogglePassthrough()
    {
        PTCheck();
        if (!Passthrough && Any) return;
        if (PTCheck()) _passthroughController.TogglePassthrough(Passthrough);
        if (_passthroughBtnBckPlateActive) _passthroughBtnBckPlateActive.SetActive(Passthrough);
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        // TODO: Synchronize the current state
    }
    #endregion
}
