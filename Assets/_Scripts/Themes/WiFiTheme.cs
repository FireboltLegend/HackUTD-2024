using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class WiFiTheme : MonoBehaviour
{
    [SerializeField] private ScriptableRendererFeature _rendererFeature;
    [SerializeField] private Material _wifiMaterial;
    [SerializeField] private float _range;
    [SerializeField] private Transform _origin;
    
    [Header("Debug")]
    [SerializeField, ReadOnly] private bool _themeActive;
    public bool ThemeActive => _themeActive;
    
    private static readonly int SphereParameters = Shader.PropertyToID("_SphereParameters");

    private void Start()
    {
        if (!_origin) _origin = transform;
        _themeActive = _rendererFeature.isActive;
    }

    private void OnDestroy()
    {
        _wifiMaterial.SetVector(SphereParameters, Vector4.zero);
        _rendererFeature.SetActive(false);
    }

    private void Update()
    {
        if (!_themeActive) return;
        Vector4 p = _origin.position;
        p.w = _range;
        _wifiMaterial.SetVector(SphereParameters, p);
    }

    [Button]
    public void ToggleTheme()
    {
        _themeActive = !_themeActive;
        _rendererFeature.SetActive(_themeActive);
    }

    /*
    [Button]
    private void ConvertAllMaterials()
    {
        var renderers = FindObjectsOfType<MeshRenderer>();
        foreach (var r in renderers)
        {
            r.materials = new Material[] {_wifiMaterial};
            r.material = _wifiMaterial;
        }
    }
    */
}
