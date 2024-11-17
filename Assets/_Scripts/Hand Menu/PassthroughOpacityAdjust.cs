using MILab;
using UnityEngine;

public class PassthroughOpacityAdjust : MonoBehaviour
{
    [SerializeField] private float _adjustment = 0.1f;
    [SerializeField, ReadOnly] private float _opacity = 1;

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
        //if (Passthrough) _opacity = Passthrough.textureOpacity;
        //else _opacity = 1;
    }
    
    [Button(Mode = ButtonMode.NotInPlayMode)]
    public void Increase()
    {
        _opacity += _adjustment;
        ApplyOpacity();
    }

    [Button(Mode = ButtonMode.NotInPlayMode)]
    public void Decrease()
    {
        _opacity -= _adjustment;
        ApplyOpacity();
    }

    private void ApplyOpacity()
    {
        _opacity = Mathf.Clamp01(_opacity);
        if (Passthrough) Passthrough.textureOpacity = _opacity;
    }
}
