using System;
using System.Collections.Generic;
using Microsoft.MixedReality.Toolkit.UI;
using MILab;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class HandMenuController : MonoBehaviour
{
    [SerializeField, ReadOnly] private int _activeMenu;
    [SerializeField] private List<GameObject> _menus = new List<GameObject>();
    [SerializeField] private TextMeshPro _title;
    [SerializeField] private TextMeshPro _switchMenuText;
    [SerializeField] private List<SceneButton> _sceneButtons = new List<SceneButton>();
    [SerializeField] List<GameObject> _menuItemBackplates = new List<GameObject>();

    [SerializeField] private ObjectEditingEvents editEvents;
    private int NextMenu => (_activeMenu + 1) % _menus.Count;
    
    private void Awake()
    {
        for (int i = 0; i < _menus.Count; i++)
        {
            SetMenuActive(i, true);
        }
    }

    private void OnEnable()
    {
        SceneController.OnSceneLoad += RefreshButtons;
    }
    
    private void OnDisable()
    {
        SceneController.OnSceneLoad -= RefreshButtons;
    }
    
    private void Start()
    {
        RefreshButtons();
        for (int i = 0; i < _menus.Count; i++)
        {
            SetMenuActive(i, i == _activeMenu);
            _menuItemBackplates[i].SetActive(i == _activeMenu);
        }
    }

    [Button]
    public void SwitchMenu()
    {
        SetMenuActive(_activeMenu, false);
        _activeMenu = NextMenu;
        if (_title) _title.text = _menus[_activeMenu].name;
        if (_switchMenuText) _switchMenuText.text = _menus[NextMenu].name;
        SetMenuActive(_activeMenu, true);
        editEvents.OnHandMenuChange.Invoke(_menus[_activeMenu].name);
    }

    public void ActivateMenu(int index)
    {
        SetMenuActive(_activeMenu, false);
        _menuItemBackplates[_activeMenu].SetActive(false);
        _activeMenu = index;
        if (_title) _title.text = _menus[_activeMenu].name;
        if (_switchMenuText) _switchMenuText.text = _menus[NextMenu].name;
        SetMenuActive(_activeMenu, true);
        _menuItemBackplates[_activeMenu].SetActive(true);
        editEvents.OnHandMenuChange.Invoke(_menus[_activeMenu].name);
    }

    private void SetMenuActive(int index, bool active)
    {
        _menus[index].SetActive(active);
    }

    private void RefreshButtons(string _) => RefreshButtons();
    private void RefreshButtons()
    {
        int buttonIndex = 0;
        var scenes = SceneController.Instance.Scenes;
        string current = SceneController.Instance.CurrentScene;
        for (var i = 0; i < scenes.Count; i++)
        {
            var scene = scenes[i];
            if (scene.Valid && !scene.SceneName.Equals(current) && buttonIndex < _sceneButtons.Count)
            {
                _sceneButtons[buttonIndex].EnableAndSetText(i, scene.SceneName);
                buttonIndex++;
            }
        }

        for (; buttonIndex < _sceneButtons.Count; buttonIndex++)
        {
            _sceneButtons[buttonIndex].Disable();
        }
    }
}