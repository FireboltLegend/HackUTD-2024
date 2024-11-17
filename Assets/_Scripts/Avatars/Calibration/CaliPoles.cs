using System;
using System.Collections;
using System.Collections.Generic;
using MILab;
using UnityEngine;

public class CaliPoles : CaliBase
{
    [SerializeField] private Transform _calibrationOffset;

    // These are able to be moved around by the player to calibrate
    [SerializeField] private Transform _calibrationController01;
    [SerializeField] private Transform _calibrationController02;
    
    // These are the visuals to show the player where their calibration point is currently
    [SerializeField] private Transform _calibrationPole01;
    [SerializeField] private Transform _calibrationPole02;
    
    // These are the virtual positions where the poles should be (After calibration should match the calibration poles)
    [SerializeField] private Transform _virtualPole01;
    [SerializeField] private Transform _virtualPole02;
    
    // A reference to where the player is / should be
    [SerializeField] private Transform _playerRef;

    private const float ControllerHeight = 1.25f;

    private void Start()
    {
        _calibrationOffset.position = Vector3.zero;
        _calibrationOffset.rotation = Quaternion.identity;
        _calibrationController01.position = _virtualPole01.position + Vector3.up * ControllerHeight;
        _calibrationPole01.position = _virtualPole01.position;
        _calibrationController02.position = _virtualPole02.position + Vector3.up * ControllerHeight;
        _calibrationPole02.position = _virtualPole02.position;
    }
    
    private void Update()
    {
        if (!_enabled) return;
        var pos01 = _calibrationController01.position;
        pos01.y = 0;
        _calibrationPole01.position = pos01;
        var pos02 = _calibrationController02.position;
        pos02.y = 0;
        _calibrationPole02.position = pos02;
    }
    
    protected override void OnButtonA()
    {
        Calibrate();
        
        _manager.CompleteCalibration();
    }

    protected override void OnButtonB()
    {
        ResetCalibration();
    }

    [Button]
    private void ResetCalibration()
    {
        _calibrationOffset.position = Vector3.zero;
        _calibrationOffset.rotation = Quaternion.identity;
        var pos = MainCameraTransform.position + MainCameraTransform.forward;
        pos.y = ControllerHeight;
        _calibrationController01.position = pos;
        _calibrationPole01.position = _virtualPole01.position;
        pos += MainCameraTransform.right;
        _calibrationController02.position = pos;
        _calibrationPole02.position = pos + Vector3.down * ControllerHeight;
    }

    [Button]
    private void Calibrate()
    {
        // Reset stored offset
        var pos01 = _calibrationPole01.position;
        var pos02 = _calibrationPole02.position;
        _calibrationOffset.position = Vector3.zero;
        _calibrationOffset.rotation = Quaternion.identity;
        _calibrationPole01.position = pos01;
        _calibrationPole02.position = pos02;

        // Set the player reference;
        _playerRef.position = MainCameraTransform.position;
        _playerRef.rotation = MainCameraTransform.rotation;
        
        // Offset from the calibration pole to the virtual pole
        Vector3 offset = _virtualPole01.localPosition - _calibrationPole01.position;
        offset.y = 0;
        
        // Add offset to the calibration offset
        _calibrationOffset.position = offset;
        
        // Calculate rotation about pole 1 to match pole 2 up with the virtual pole 2
        var virtualRot = Quaternion.LookRotation(_virtualPole02.position - _virtualPole01.position, Vector3.up);
        var calibrationRot = Quaternion.LookRotation(_calibrationPole02.position - _calibrationPole01.position, Vector3.up);
        var diff = virtualRot * Quaternion.Inverse(calibrationRot);
        _calibrationOffset.rotation *= diff;
        
        // Apply offset again
        offset = _calibrationOffset.position + _virtualPole01.localPosition - _calibrationPole01.position;
        offset.y = 0;
        _calibrationOffset.position = offset;
        
        // Reset Calibration Controller Positions
        _calibrationController01.position = _calibrationPole01.position + Vector3.up * ControllerHeight;
        _calibrationController01.rotation = Quaternion.identity;
        _calibrationController02.position = _calibrationPole02.position + Vector3.up * ControllerHeight;
        _calibrationController02.rotation = Quaternion.identity;

        if (OvrRootTransform)
        {
            _manager.SetActorTransform(_playerRef);
        }

        Log("Position Check" + MainCameraTransform.position + " " + _playerRef.position);
        Log("Rotation Check" + MainCameraTransform.eulerAngles + " " + _playerRef.eulerAngles);
    }
}
