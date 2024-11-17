using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouthMovement : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform _mouth;
    [SerializeField] private Transform _lowerLip;
    [SerializeField] private Transform _upperLip;
    [SerializeField] private Transform _mouthRef;
    [SerializeField] private Transform _lowerLipTarget;
    [SerializeField] private Transform _upperLipTarget;

    [Header("Offsets")]
    [SerializeField] private Vector3 _lowerOpenPosOffset;
    [SerializeField] private Vector3 _lowerOpenRotOffset;
    [SerializeField] private Vector3 _lowerClosedPosOffset;
    [SerializeField] private Vector3 _lowerClosedRotOffset;
    [SerializeField] private Vector3 _upperOpenPosOffset;
    [SerializeField] private Vector3 _upperOpenRotOffset;
    [SerializeField] private Vector3 _upperClosedPosOffset;
    [SerializeField] private Vector3 _upperClosedRotOffset;

    [Header("Controls")]
    [SerializeField] private bool _talking;
    [SerializeField] private float _talkSpeed = 1;
    [SerializeField, Range(0, 1)] private float _openClosedDelta;
    [SerializeField] private bool _opening = true;

    public void SetTalking(bool talking)
    {
        if (talking && _talking) return;
        if (talking)
        {
            StartTalking();
        }
        else
        {
            StopTalking();
        }
    }

    [Button(Spacing = 10)]
    public void StartTalking()
    {
        if (_talking) return;
        _talking = true;
        _opening = true;
    }

    [Button]
    public void StopTalking()
    {
        if (!_talking) return;
        _talking = false;
    }
    
    [Button(Spacing = 10)]
    private void OpenMouthImmediate()
    {
        _upperLip.localPosition = _upperOpenPosOffset;
        _upperLip.localRotation = Quaternion.Euler(_upperOpenRotOffset);
        _lowerLip.localPosition = _lowerOpenPosOffset;
        _lowerLip.localRotation = Quaternion.Euler(_lowerOpenRotOffset);
    }
    
    [Button]
    private void CloseMouthImmediate()
    {
        _upperLip.localPosition = _upperClosedPosOffset;
        _upperLip.localRotation = Quaternion.Euler(_upperClosedRotOffset);
        _lowerLip.localPosition = _lowerClosedPosOffset;
        _lowerLip.localRotation = Quaternion.Euler(_lowerClosedRotOffset);
    }
    
    [Button(Spacing = 10)]
    private void SetNewOpenOffset()
    {
        _upperOpenPosOffset = _upperLip.localPosition;
        _upperOpenRotOffset = _upperLip.localRotation.eulerAngles;
        _lowerOpenPosOffset = _lowerLip.localPosition;
        _lowerOpenRotOffset = _lowerLip.localRotation.eulerAngles;
    }

    [Button]
    private void SetNewClosedOffset()
    {
        _upperClosedPosOffset = _upperLip.localPosition;
        _upperClosedRotOffset = _upperLip.localRotation.eulerAngles;
        _lowerClosedPosOffset = _lowerLip.localPosition;
        _lowerClosedRotOffset = _lowerLip.localRotation.eulerAngles;
    }

    private void Update()
    {
        if (_talking)
        {
            if (_opening)
            {
                _openClosedDelta += Time.deltaTime * _talkSpeed;
                if (_openClosedDelta > 1) _opening = false;
            }
            else
            {
                _openClosedDelta -= Time.deltaTime * _talkSpeed;
                if (_openClosedDelta < 0) _opening = true;
            }
        }
        else if (_openClosedDelta > 0)
        {
            _openClosedDelta -= Time.deltaTime * _talkSpeed;
        }
        _openClosedDelta = Mathf.Clamp01(_openClosedDelta);
        SetLipTransforms();
    }

    private void SetLipTransforms()
    {
        _mouthRef.position = _mouth.position;
        _mouthRef.rotation = _mouth.rotation;
        _lowerLipTarget.localPosition = Evaluate(_lowerOpenPosOffset, _lowerClosedPosOffset);
        _lowerLipTarget.localRotation = Quaternion.Euler(Evaluate(_lowerOpenRotOffset, _lowerClosedRotOffset));
        _upperLipTarget.localPosition = Evaluate(_upperOpenPosOffset, _upperClosedPosOffset);
        _upperLipTarget.localRotation = Quaternion.Euler(Evaluate(_upperOpenRotOffset, _upperClosedRotOffset));
    }

    private Vector3 Evaluate(Vector3 open, Vector3 closed) => Vector3.Lerp(open, closed, _openClosedDelta);
}
