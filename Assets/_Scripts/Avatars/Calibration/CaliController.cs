using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CaliController : CaliBase
{
    [SerializeField] private Transform _reset;
    [SerializeField] private Transform _pivotA;
    [SerializeField] private Transform _pivotB;

    [SerializeField, ReadOnly] private int _stage;

    public override void Enable()
    {
        base.Enable();
        _stage = 0;
    }
    
    protected override void OnButtonA()
    {
        switch (_stage)
        {
            case 0:
                TranslateActor();
                _stage = 1;
                break;
            case 1:
                RotateActor();
                _stage = 2;
                break;
            case 2:
                TranslateActor(0.8f);
                _manager.CompleteCalibration();
                break;
        }
    }

    protected override void OnButtonB()
    {
        ResetTransform();
        _stage = 0;
    }

    private void ResetTransform()
    {
        _manager.SetActorTransform(_reset);
    }

    private void TranslateActor(float bias = 1)
    {
        var posOffset = _pivotA.position - RightHand.position;
        posOffset.y = 0;
        OvrRootTransform.position += posOffset * bias;
    }

    private void RotateActor()
    {
        var posA = _pivotA.position;
        var pivotAtoRealB = RightHand.position - posA;
        var pivotAtoVirtualB = _pivotB.position - posA;
        float angleOffset = Vector3.SignedAngle(pivotAtoRealB, pivotAtoVirtualB, Vector3.up);
        OvrRootTransform.RotateAround(posA, Vector3.up, angleOffset);
    }
}
