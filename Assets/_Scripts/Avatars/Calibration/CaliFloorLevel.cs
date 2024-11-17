using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CaliFloorLevel : CaliBase
{
    [SerializeField] private Vector3 _handOffset;
    [SerializeField] private float _heightOffset;
    
    private void Update()
    {
        if (_enabled)
        {
            UpdateHeight();
        }
    }
    
    protected override void OnButtonA()
    {
        UpdateHeight();
        _manager.CompleteCalibration();
    }

    protected override void OnButtonB()
    {
        _heightOffset = RightHand.position.y - OvrRootTransform.position.y;
    }

    private void UpdateHeight()
    {
        var height = RightHand.position.y + _heightOffset;
        var pos = OvrRootTransform.position;
        pos.y = height;
        OvrRootTransform.position = pos;
    }
}
