using System.Collections.Generic;
using MILab;
using UnityEngine;
using UnityEngine.UI;

public class AvatarSelectionController : MonoBehaviour
{
    [SerializeField] private List<Button> _selectionButtons = new List<Button>();
    [SerializeField] private List<Sprite> _avatarSprites = new List<Sprite>();

    [Header("Custom Avatars")]
    [SerializeField] private bool _temocAvatar;
    [SerializeField, ShowIf("_temocAvatar")] private Sprite _temocAvatarSprite;
    [SerializeField] private bool _professorAvatar;
    [SerializeField, ShowIf("_professorAvatar")] private Sprite _professorAvatarSprite;
    
    [Header("Menu Settings")]
    [SerializeField] private bool _useAvatarSprites = true;
    [SerializeField] private Sprite _defaultSprite;
    [SerializeField] private bool _showText;
    [SerializeField] private int _fontSize = 50;

    [SerializeField, ReadOnly] private PhotonAvatarEntity _localAvatar;

    [Button]
    private void UpdateButtons()
    {
        int i = 0;
        SetButton(i, "T", _temocAvatar, _temocAvatarSprite);
        i++;
        SetButton(i, "P", _professorAvatar, _professorAvatarSprite);
        i++;
        int j = 0;
        for (; i < _selectionButtons.Count; i++)
        {
            if (_avatarSprites.Count > j) SetButton(i, (j+1).ToString(), true, _useAvatarSprites ? _avatarSprites[j++] : _defaultSprite);
            else SetButton(i, "", false, null);
        }
    }

    private void SetButton(int index, string title, bool active, Sprite sprite)
    {
        var button = _selectionButtons[index];
        var text = button.GetComponentInChildren<Text>();
        text.gameObject.SetActive(_showText);
        if (_showText)
        {
            text.text = title;
            text.fontSize = _fontSize;
        }
        button.gameObject.SetActive(active);
        var image = button.GetComponent<Image>();
        image.sprite = sprite;
    }

    private void Start()
    {
        HookupButtonEvents();
    }

    private void HookupButtonEvents()
    {
        for (var i = 0; i < _selectionButtons.Count; i++)
        {
            int num = i;
            _selectionButtons[i].onClick.AddListener(() => SelectAvatar(num));
        }
    }

    [Button]
    private static void SelectAvatar(int num)
    {
        PlayerOVR.SelectLocalAvatar(num);
    }
}
