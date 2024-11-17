using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class HubDoor : MonoBehaviour
{
    public static Action DisableDoors = delegate { };
    public static bool AnySceneLoaded;
    
    [SerializeField] private DigitalTwinScene _scene;
    [SerializeField] private GameObject _swingDoor;
    [SerializeField] private GameObject _staticDoor;
    [SerializeField] private Transform _slider;
    [SerializeField] private Material _defaultSkybox;
    [SerializeField, ReadOnly] private bool _loading;
    [SerializeField, ReadOnly] private bool _loaded;

    public bool Valid => _scene.Valid;
    public string SceneName => _scene.SceneName;

    private void Awake()
    {
        _swingDoor.SetActive(false);
        _staticDoor.SetActive(true);
        SetSlider(0);
    }

    [Button]
    public void ToggleHubDoor()
    {
        if (_loaded)
        {
            //UnloadScene();
        }
        else
        {
            LoadScene();
        }
    }

    [Button]
    private void LoadScene()
    {
        if (_loading || !_scene.Valid) return;
        if (_scene.Skybox == null) RenderSettings.skybox = _defaultSkybox;
        else RenderSettings.skybox = _scene.Skybox;
        StartCoroutine(LoadSceneAsync(_scene));
    }
    
    [Button]
    private void UnloadScene()
    {
        if (_loading) return;
        SceneManager.UnloadSceneAsync(_scene.SceneName);
        _swingDoor.SetActive(false);
        _staticDoor.SetActive(true);
        SetSlider(0);
        AnySceneLoaded = false;
        _loaded = false;
        DisableDoors -= UnloadScene;
    }

    private IEnumerator LoadSceneAsync(DigitalTwinScene scene)
    {
        if (AnySceneLoaded)
        {
            DisableDoors?.Invoke();
        }
        AnySceneLoaded = true;
        _loading = true;
        var async = SceneManager.LoadSceneAsync(scene.SceneName, LoadSceneMode.Additive);
        while (!async.isDone)
        {
            SetSlider(async.progress);
            yield return null;
        }
        _swingDoor.SetActive(true);
        _staticDoor.SetActive(false);
        SetSlider(1);
        _loading = false;
        AnySceneLoaded = true;
        _loaded = true;
        DisableDoors += UnloadScene;
    }

    private void SetSlider(float value)
    {
        var scale = _slider.localScale;
        scale.x = Mathf.Clamp01(value);
        _slider.localScale = scale;
    }
}
