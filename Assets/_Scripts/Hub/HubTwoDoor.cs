using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class HubTwoDoor : MonoBehaviour
{
    [SerializeField] private GameObject _swingDoor;
    [SerializeField] private GameObject _staticDoor;
    [SerializeField] private Transform _slider;
    [SerializeField] private Material _defaultSkybox;

    [SerializeField, ReadOnly] private bool _loading;
    [SerializeField] private DigitalTwinScene _scene1;
    [SerializeField, ReadOnly] private bool _loaded1;
    [SerializeField] private DigitalTwinScene _scene2;
    [SerializeField, ReadOnly] private bool _loaded2;

    private void Awake()
    {
        if (_swingDoor) _swingDoor.SetActive(false);
        if (_staticDoor) _staticDoor.SetActive(true);
        SetSlider(0);
    }

    [Button]
    public void ToggleHubDoor1()
    {
        if (_loaded1)
        {
            //UnloadScene1();
        }
        else
        {
            LoadScene1();
        }
    }

    [Button]
    public void ToggleHubDoor2()
    {
        if (_loaded2)
        {
            //UnloadScene2();
        }
        else
        {
            LoadScene2();
        }
    }

    private void LoadScene1()
    {
        if (_loading || !_scene1.Valid) return;
        if (_scene1.Skybox == null) RenderSettings.skybox = _defaultSkybox;
        else RenderSettings.skybox = _scene1.Skybox;
        StartCoroutine(LoadSceneAsync(_scene1));
        _loaded1 = true;
    }

    private void LoadScene2()
    {
        if (_loading || !_scene2.Valid) return;
        if (_scene2.Skybox == null) RenderSettings.skybox = _defaultSkybox;
        else RenderSettings.skybox = _scene2.Skybox;
        StartCoroutine(LoadSceneAsync(_scene2));
        _loaded2 = true;
    }

    private void UnloadScene1()
    {
        if (!_loaded1) return;
        UnloadScene(_scene1);
        _loaded1 = false;
    }

    private void UnloadScene2()
    {
        if (!_loaded2) return;
        UnloadScene(_scene2);
        _loaded2 = false;
    }

    private void UnloadBoth()
    {
        UnloadScene1();
        UnloadScene2();
    }
    
    private void UnloadScene(DigitalTwinScene scene)
    {
        if (_loading) return;
        SceneManager.UnloadSceneAsync(scene.SceneName);
        if (_swingDoor) _swingDoor.SetActive(false);
        if (_staticDoor) _staticDoor.SetActive(true);
        SetSlider(0);
        HubDoor.AnySceneLoaded = false;
        HubDoor.DisableDoors -= UnloadBoth;
    }

    private IEnumerator LoadSceneAsync(DigitalTwinScene scene)
    {
        if (HubDoor.AnySceneLoaded)
        {
            HubDoor.DisableDoors?.Invoke();
        }
        HubDoor.AnySceneLoaded = true;
        _loading = true;
        var async = SceneManager.LoadSceneAsync(scene.SceneName, LoadSceneMode.Additive);
        while (!async.isDone)
        {
            SetSlider(async.progress);
            yield return null;
        }
        if (_swingDoor) _swingDoor.SetActive(true);
        if (_staticDoor) _staticDoor.SetActive(false);
        SetSlider(1);
        _loading = false;
        HubDoor.AnySceneLoaded = true;
        HubDoor.DisableDoors += UnloadBoth;
    }

    private void SetSlider(float value)
    {
        var scale = _slider.localScale;
        scale.x = Mathf.Clamp01(value);
        _slider.localScale = scale;
    }
}
