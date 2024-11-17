using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveAlongPath : MonoBehaviour
{
    [SerializeField] private Vector3 _startOffset;
    [SerializeField] private Vector3 _endOffset;
    [SerializeField] private float _yOffsetMag = 5;
    [SerializeField] private AnimationCurve _yOffset = new AnimationCurve(new Keyframe(0, 0.5f), new Keyframe(1, 0.5f));
    [SerializeField] private float _speed = 2;
    [SerializeField] private bool _drawGizmos;
    [SerializeField] private int _gizmoDetails = 1000;
    [SerializeField, ReadOnly] private Vector3 _origin;
    [SerializeField, ReadOnly] private float _delta;

    private void Start()
    {
        _origin = transform.position;
        _drawGizmos = false;
    }

    private void Update()
    {
        _delta += Time.deltaTime * _speed;
        _delta %= 1;
        var offset = Vector3.Lerp(_startOffset, _endOffset, _delta);
        offset.y += (_yOffset.Evaluate(_delta) - 0.5f) * _yOffsetMag;
        transform.position = _origin + offset;
    }

    private void OnDrawGizmosSelected()
    {
        if (!_drawGizmos) return;
        _origin = transform.position;
        Gizmos.color = Color.yellow;
        var start = _startOffset;
        start.y = _yOffset.Evaluate(0) - 0.5f;
        var step = 1f / _gizmoDetails;
        for (float t = 0; t < 1; t += step)
        {
            var offset = Vector3.Lerp(_startOffset, _endOffset, t);
            offset.y += (_yOffset.Evaluate(t) - 0.5f) * _yOffsetMag;
            Gizmos.DrawLine(_origin + start, _origin + offset);
            start = offset;
        }
    }
}
