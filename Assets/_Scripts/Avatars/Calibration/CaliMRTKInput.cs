using System.Collections;
using System.Collections.Generic;
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Input;
using UnityEngine;

public class CaliMRTKInput : MonoBehaviour, IMixedRealityInputActionHandler
{
    [SerializeField] private bool _debug;
    [SerializeField] private CaliManager _manager;
    [SerializeField] private string _buttonA = "ButtonOnePress";
    [SerializeField] private string _buttonB = "ButtonTwoPress";
    
    private void OnValidate()
    {
        if (_manager == null) _manager = transform.parent.GetComponent<CaliManager>();
    }
    
    private void OnEnable()
    {
        // Instruct Input System that we would like to receive all input events of type
        CoreServices.InputSystem?.RegisterHandler<IMixedRealityInputActionHandler>(this);
    }

    private void OnDisable()
    {
        // This component is being destroyed
        // Instruct the Input System to disregard us for input event handling
        CoreServices.InputSystem?.UnregisterHandler<IMixedRealityInputActionHandler>(this);
    }
    
    public void OnActionStarted(BaseInputEventData eventData)
    {
        string eventDescription = eventData.MixedRealityInputAction.Description;

        if (eventDescription.Equals(_buttonA))
        {
            _manager.OnButtonA?.Invoke();
            if (_debug) Debug.Log("Button A Pressed", gameObject);
        }
        else if (eventDescription.Equals(_buttonB))
        {
            _manager.OnButtonB?.Invoke();
            if (_debug) Debug.Log("Button B Pressed", gameObject);
        }
    }

    public void OnActionEnded(BaseInputEventData eventData)
    {
    }
}
