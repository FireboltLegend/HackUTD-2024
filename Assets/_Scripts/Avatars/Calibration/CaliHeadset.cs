using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CaliHeadset : CaliBase
{
    [SerializeField] private Transform _reset;
    
    protected override void OnButtonA()
    {
        ResetTransform();
        _manager.CompleteCalibration();
    }

    protected override void OnButtonB()
    {
        ResetTransform();
        _manager.CompleteCalibration();
    }
    

    private void ResetTransform()
    {
        _manager.SetActorTransform(_reset);
    }
}
