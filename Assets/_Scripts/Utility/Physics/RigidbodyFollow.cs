using UnityEngine;

public class RigidbodyFollow : MonoBehaviour
{
    [SerializeField] private Transform _follow;
    [SerializeField] private Rigidbody _rb;
    [SerializeField] private float _force = 10;

    private void OnValidate()
    {
        if (_follow == null) _follow = transform.parent;
        if (_rb == null) _rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        var offset = _follow.position - transform.position;
        offset *= _force;
        _rb.velocity = Vector3.Lerp(_rb.velocity, offset, 0.5f);
    }
}
