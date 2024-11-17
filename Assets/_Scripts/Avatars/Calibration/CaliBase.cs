using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class CaliBase : MonoBehaviour
{
    [SerializeField] private bool _debug;
    [SerializeField] protected CaliManager _manager;
    [SerializeField, ReadOnly] protected bool _enabled;
    [SerializeField] private List<GameObject> _visuals;

    protected Transform OvrRootTransform => _manager.OvrRootTransform;
    protected Transform OvrTransform => _manager.OvrTransform;
    protected Transform MainCameraTransform => _manager.MainCameraTransform;
    protected Transform RightHand => _manager.RightHand;

    private void OnValidate()
    {
        if (_manager == null) _manager = transform.parent.GetComponent<CaliManager>();
    }

    public virtual void Enable()
    {
        _manager.OnButtonA += OnButtonA;
        _manager.OnButtonB += OnButtonB;

        SetVisualsActive(true);
        _enabled = true;
    }

    public virtual void Disable()
    {
        _manager.OnButtonA -= OnButtonA;
        _manager.OnButtonB -= OnButtonB;

        SetVisualsActive(false);
        _enabled = false;
    }

    private void SetVisualsActive(bool active)
    {
        foreach (var obj in _visuals)
        {
            if (obj) obj.SetActive(active);
        }
    }

    protected abstract void OnButtonA();
    protected abstract void OnButtonB();

    protected void Log(string message)
    {
        if (_debug) Debug.Log(message, gameObject);
    }
}
