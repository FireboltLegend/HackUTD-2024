using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlinkRandom : MonoBehaviour
{
    [SerializeField] private Animator _animator;
    [SerializeField] private Vector2 _blinkTime = new Vector2(3, 6);
    [SerializeField, ReadOnly] private float _blinkTimer;

    private void Start()
    {
        _blinkTimer = Random.Range(_blinkTime.x, _blinkTime.y);
    }

    private void Update()
    {
        _blinkTimer -= Time.deltaTime;
        if (_blinkTimer <= 0)
        {
            Blink();
            _blinkTimer = Random.Range(_blinkTime.x, _blinkTime.y);
        }
    }

    [Button]
    private void Blink()
    {
        if (_animator) _animator.SetTrigger("Blink");
    }
}
