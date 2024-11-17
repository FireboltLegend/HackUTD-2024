using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class ThemeController : MonoBehaviour
{
    [Header("Post Processing")]
    [SerializeField] private Volume _defaultVolume;
    [SerializeField] private float _blendSpeed = 1;
    
    [Header("Themes")]
    [SerializeField] private AquariumTheme _aquarium;
    [SerializeField] private WiFiTheme _wifi;
    [SerializeField] private WindTheme _wind;

    private Volume _activeVolume;
    private Coroutine _activateVolumeRoutine;

    public bool AquariumActive => _aquarium.ThemeActive;
    public bool WifiActive => _wifi.ThemeActive;
    public bool WindActive => _wind.ThemeActive;
    
    [Button]
    public void ToggleAquarium()
    {
        _aquarium.ToggleTheme();
        Blend(_aquarium.ThemeVolume, _aquarium.ThemeActive);
    }

    [Button]
    public void ToggleWifi()
    {
        _wifi.ToggleTheme();
    }

    [Button]
    public void ToggleWind()
    {
        _wind.ToggleTheme();
        Blend(_wind.ThemeVolume, _wind.ThemeActive);
    }

    #region Volume Blending

    private void Start()
    {
        _activeVolume = _defaultVolume;
        _defaultVolume.weight = 1;
        _aquarium.ThemeVolume.weight = 0;
        _wind.ThemeVolume.weight = 0;
    }

    private void Blend(Volume volume, bool active)
    {
        var toActivate = active ? volume : _defaultVolume;
        if (_activeVolume == toActivate) return;
        StartCoroutine(DisableVolume(_activeVolume));
        if (_activateVolumeRoutine != null) StopCoroutine(_activateVolumeRoutine);
        _activateVolumeRoutine = StartCoroutine(ActivateVolume(toActivate));
    }
    
    private IEnumerator DisableVolume(Volume v)
    {
        while (v.weight > 0)
        {
            v.weight -= Time.deltaTime * _blendSpeed;
            yield return null;
        }
        v.weight = 0;
    }

    private IEnumerator ActivateVolume(Volume v)
    {
        _activeVolume = v;
        while (v.weight < 1)
        {
            v.weight += Time.deltaTime * _blendSpeed;
            yield return null;
        }
        v.weight = 1;
        _activateVolumeRoutine = null;
    }
    
    #endregion
}
