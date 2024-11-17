using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HingeJointReset : MonoBehaviour
{
    [SerializeField] private HingeJoint _joint;
    [SerializeField] private Rigidbody _rb;

    private Quaternion _initialRot;

    private void OnValidate()
    {
        if (_joint == null) _joint = GetComponent<HingeJoint>();
        if (_rb == null) _rb = GetComponent<Rigidbody>();
    }

    private void Awake()
    {
        _initialRot = _joint.transform.localRotation;
    }

    private void OnEnable()
    {
        ResetHinge();
    }

    [Button]
    private void ResetHinge()
    {
        _joint.axis = _joint.axis;
        _rb.velocity = Vector3.zero;
        _joint.transform.localRotation = _initialRot;
    }
}
