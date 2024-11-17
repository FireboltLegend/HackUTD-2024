using System.Collections;
using MILab;
using UnityEngine;

public class CalibrationController : MonoBehaviour
{
    [SerializeField] private GameObject _calibrationButton;
    [SerializeField] private GameObject _recalibrationButton;
    [SerializeField] private GameObject _passthroughButton;
    [SerializeField] private GameObject _stopButton;

    [SerializeField] public PlayerPositionOffset _playerPositionOffset;

    //private void Start()
    //{
    //    if (_aquariumActive) _aquariumActive.SetActive(false);
    //    if (_wifiActive) _wifiActive.SetActive(false);
    //    if (_windActive) _windActive.SetActive(false);
    //}

    private void Awake()
    {
        // Assign the reference to the PlayerPositionOffset component
        _playerPositionOffset = FindObjectOfType<PlayerPositionOffset>();
    }

    [Button]
    public void Calibrate()
    {
        if (_playerPositionOffset == null)
            _playerPositionOffset = FindObjectOfType<PlayerPositionOffset>();
        _playerPositionOffset.StartCalibration();
    }

    [Button]
    public void ReCalibrate()
    {
        _playerPositionOffset.ReCalibrate();
    }

    [Button]
    public void TogglePasshrough()
    {
        _playerPositionOffset.TogglePassthrough();
    }

    [Button]
    public void StopCalibration()
    {
        _playerPositionOffset.StopCalibration();
    }
}
