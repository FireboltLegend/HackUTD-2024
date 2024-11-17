using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CaliOculusInput : MonoBehaviour
{
    [SerializeField] private bool _debug;
    [SerializeField] private CaliManager _manager;
    
    private void OnValidate()
    {
        if (_manager == null) _manager = transform.parent.GetComponent<CaliManager>();
    }
    
    private void Update()
    {
        if (OVRInput.GetDown(OVRInput.Button.Two))
        {
            _manager.OnButtonA?.Invoke();
            if (_debug) Debug.Log("Button A Pressed", gameObject);
        }
        if (OVRInput.GetDown(OVRInput.Button.One))
        {
            _manager.OnButtonB?.Invoke();
            if (_debug) Debug.Log("Button B Pressed", gameObject);
        }
    }
}
