using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowAvatarHead : MonoBehaviour
{
    [SerializeField] private GameObject _prefab;
    [SerializeField, ReadOnly] private Transform _obj;
    
    private void Start()
    {
        _obj = Instantiate(_prefab).transform;
    }

    private void OnDestroy()
    {
        if (_obj) Destroy(_obj.gameObject);
    }

    private void Update()
    {
        _obj.position = transform.position;
    }
}
