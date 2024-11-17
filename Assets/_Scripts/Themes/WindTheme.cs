using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class WindTheme : MonoBehaviour
{
    [SerializeField] private GameObject _particleParent;
    
    [Header("Post Processing")]
    [SerializeField] private Volume _themeVolume;
    
    [Header("Debug")]
    [SerializeField, ReadOnly] private bool _themeActive;
    public bool ThemeActive => _themeActive;
    public Volume ThemeVolume => _themeVolume;
    
    [Button]
    public void ToggleTheme()
    {
        _themeActive = !_themeActive;
        _particleParent.SetActive(_themeActive);
    }
}
