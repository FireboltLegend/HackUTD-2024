using UnityEngine;

public class FollowTransform : MonoBehaviour
{
    [SerializeField] private Transform _objToFollow;
    [SerializeField] private Transform _idlePos;
    [SerializeField] private bool _useSecondPoint;
    [SerializeField, ShowIf("_useSecondPoint")] private Transform _secondPoint;
    [SerializeField, ShowIf("_useSecondPoint")] private float _secondPointWeight;
    [SerializeField, ShowIf("_useSecondPoint")] private bool _useSecondPointClampPlane;
    [SerializeField, ShowIf("_useSecondPoint")] private Transform _secondPointClampPlane;
    [SerializeField] private bool _maintainOriginalOffset;
    [SerializeField, ShowIf("_maintainOriginalOffset"), ReadOnly] private Vector3 _originalOffset;
    [SerializeField] private bool _lateUpdate;
    [SerializeField] private bool _move;
    [SerializeField, ShowIf("_move")] private float _moveSpeed = 1;
    [SerializeField, ShowIf("_move")] private Vector3 _positionOffset;
    [SerializeField] private bool _rotate;
    [SerializeField, ShowIf("_rotate")] private float _rotateSpeed = 1;
    [SerializeField, ShowIf("_rotate")] private Vector3 _rotationOffset;
    [SerializeField] private bool _lookAtTransform;
    [SerializeField, ShowIf("_lookAtTransform")] private Transform _lookAtTarget;
    [SerializeField, ShowIf("_lookAtTransform")] private float _lookAtTargetWeight = 1;

    private void Start()
    {
        _originalOffset = transform.position - _objToFollow.position;
    }

    private void Update()
    {
        if (!_lateUpdate) Follow();
    }

    private void LateUpdate()
    {
        if (_lateUpdate) Follow();
    }

    private void Follow()
    {
        if (_objToFollow.position == Vector3.zero && _idlePos != null)
        {
            transform.position = Vector3.Lerp(transform.position, _idlePos.position, Time.deltaTime * _moveSpeed);
            transform.rotation = Quaternion.Slerp(transform.rotation, _idlePos.rotation, Time.deltaTime * _rotateSpeed);
        }
        else
        {
            if (_move)
            {
                var goal = _objToFollow.TransformPoint(_positionOffset);
                if (_maintainOriginalOffset) goal += _originalOffset;
                if (_useSecondPoint)
                {
                    var secondPoint = _secondPoint.position;
                    if (_useSecondPointClampPlane)
                    {
                        var plane = new Plane(_secondPointClampPlane.forward, _secondPointClampPlane.position);
                        secondPoint = plane.ClosestPointOnPlane(secondPoint);
                    }
                    goal = Vector3.Lerp(goal, secondPoint, _secondPointWeight);
                }
                transform.position = Vector3.Lerp(transform.position, goal, Time.deltaTime * _moveSpeed);
            }
            if (_rotate)
            {
                var goal = _objToFollow.rotation * Quaternion.Euler(_rotationOffset);
                transform.rotation = Quaternion.Slerp(transform.rotation, goal, Time.deltaTime * _rotateSpeed);
            }
            if (_lookAtTransform)
            {
                var originalRot = transform.rotation;
                transform.LookAt(_lookAtTarget);
                transform.rotation = Quaternion.Slerp(originalRot, transform.rotation, _lookAtTargetWeight);
            }
        }
    }
}
