using System.Collections;
using System.Collections.Generic;
using MILab;
using TMPro;
using UnityEngine;

public class SceneButton : MonoBehaviour
{
    [SerializeField] private GameObject _container;
    [SerializeField] private TextMeshPro _text;
    [SerializeField, ReadOnly] private int _sceneControllerIndex = -1;

    public void Disable()
    {
        _sceneControllerIndex = -1;
        _container.SetActive(false);
    }

    public void EnableAndSetText(int index, string sceneName)
    {
        _sceneControllerIndex = index;
        _container.SetActive(true);
        _text.text = sceneName;
    }

    [Button]
    public void OnPressButton()
    {
        if (_sceneControllerIndex < 0) return;
        SceneController.Instance.LoadScene(_sceneControllerIndex);
    }
}