using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.MixedReality.Toolkit.UI;
using UnityEngine;
using UnityEngine.Rendering;

public class AquariumTheme : MonoBehaviour
{
    [Header("Post Processing")]
    [SerializeField] private Volume _themeVolume;

    [Header("Enable Disable")]
    [SerializeField] private List<GameObject> _objsToDisable;
    [SerializeField] private List<GameObject> _objsToEnable;
    
    [Header("Debug")]
    [SerializeField, ReadOnly] private bool _themeActive;
    public bool ThemeActive => _themeActive;
    public Volume ThemeVolume => _themeVolume;

    [Button]
    public void ToggleTheme()
    {
        if (_themeActive)
        {
            DisableTheme();
        }
        else
        {
            EnableTheme();
        }
    }

    private void EnableTheme()
    {
        _themeActive = true;
        foreach (var obj in _objsToDisable)
        {
            obj.SetActive(false);
        }
        foreach (var obj in _objsToEnable)
        {
            obj.SetActive(true);
        }
    }

    private void DisableTheme()
    {
        _themeActive = false;
        foreach (var obj in _objsToDisable)
        {
            obj.SetActive(true);
        }
        foreach (var obj in _objsToEnable)
        {
            obj.SetActive(false);
        }
    }
}
