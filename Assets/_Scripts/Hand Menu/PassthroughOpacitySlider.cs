using Microsoft.MixedReality.Toolkit.UI;
using MILab;
using UnityEngine;

public class PassthroughOpacitySlider : MonoBehaviour
{
    [SerializeField] private float _currentOpacity;
    [SerializeField] private float _maxOpacity = 0.8f;
    [SerializeField] private PinchSlider _slider;
    [SerializeField] private bool _passthroughActive;

    private OVRPassthroughLayer _passthrough;
    private OVRPassthroughLayer Passthrough
    {
        get
        {
            if (_passthrough == null)
            {
                if (PlayerOVR.Instance) _passthrough = PlayerOVR.Instance.GetComponent<OVRPassthroughLayer>();
            }
            return _passthrough;
        }
    }

    private void Start()
    {
        //SetPassthroughOpacity(_currentOpacity, true);
    }

    public void TogglePassthrough()
    {
        _passthroughActive = !_passthroughActive;
        SetPassthroughOpacity(_currentOpacity);
    }
    
    public void SetPassthroughOpacity(SliderEventData eventData)
    {
        SetPassthroughOpacity(eventData.NewValue);
    }

    private void SetPassthroughOpacity(float value, bool updateSlider = false)
    {
        _currentOpacity = value;
        if (!_passthroughActive) value = 0;
        if (Passthrough) Passthrough.textureOpacity = Mathf.Clamp01(value) * _maxOpacity;
        if (updateSlider && _slider) _slider.SliderValue = value;
    }
}
