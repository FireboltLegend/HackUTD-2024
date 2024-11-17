using System;
using MILab;
using UnityEngine;
using Random = UnityEngine.Random;

public class CaliManager : MonoBehaviour
{
    [SerializeField] private bool _floorLevelTracking;
    [SerializeField, ReadOnly] private bool _calibrationActive;
    
    [Header("Initial Position")]
    [SerializeField] private bool _movePlayerOnStart;
    [SerializeField] private Vector3 _playerStartOffset;
    [SerializeField] private float _playerStartRange;
    [SerializeField] private BoxCollider _roomBoundaryCheck;
    
    [Header("OVR Rig")]
    [SerializeField, ReadOnly] private Transform _ovrRootTransform;
    [SerializeField, ReadOnly] private Transform _ovrTransform;
    [SerializeField, ReadOnly] private Transform _mainCameraTransform;
    [SerializeField, ReadOnly] private Transform _rightHand;
    
    [Header("Calibration Modes")]
    [SerializeField] private bool _calibrateByControllerOnStart;
    [SerializeField] private CaliController _controller;
    [SerializeField] private CaliHeadset _headset;
    [SerializeField] private CaliPoles _poles;
    [SerializeField] private CaliFloorLevel _floorLevel;

    public bool ControllerCalibration => _controller != null && _controller.isActiveAndEnabled;
    public bool HeadsetCalibration => _headset != null && _headset.isActiveAndEnabled;
    public bool PolesCalibration => _poles != null && _poles.isActiveAndEnabled;
    public bool FloorLevelCalibration => _floorLevel != null && _floorLevel.isActiveAndEnabled;
    
    public Action OnButtonA = delegate { };
    public Action OnButtonB = delegate { };
    public Action OnComplete = delegate { };

    #region Properties
        
    public Transform OvrRootTransform
    {
        get
        {
            if (_ovrRootTransform == null) _ovrRootTransform = PlayerOVR.Instance.transform.parent;
            return _ovrRootTransform;
        }
    }
    
    public Transform OvrTransform
    {
        get
        {
            if (_ovrTransform == null) _ovrTransform = PlayerOVR.Instance.transform;
            return _ovrTransform;
        }
    }
    
    public Transform MainCameraTransform
    {
        get
        {
            if (_mainCameraTransform == null)
            {
                try
                {
                    _mainCameraTransform = OVRManager.instance.GetComponent<OVRCameraRig>().centerEyeAnchor;
                }
                catch
                {
                    Debug.LogWarning("No Main Camera (OVRCameraRig) Detected", gameObject);
                    return transform;
                }
            }
            return _mainCameraTransform;
        }
    }

    private const string RightHandLocation = "TrackingSpace/RightHandAnchor/OVRHandPrefab_Right";
    public Transform RightHand
    {
        get
        {
            if (_rightHand == null) _rightHand = OvrTransform.Find(RightHandLocation);
            return _rightHand;
        }
    }
    
    #endregion

    #region Unity Functions

    private void Start()
    {
        if (_roomBoundaryCheck) _roomBoundaryCheck.enabled = false;
        if (_movePlayerOnStart && !HubDoorController.HubActive) SetPlayerInitialPosition();
        
        if (_calibrateByControllerOnStart)
        {
            StartControllerCalibration();
        }
        else
        {
            DisableAll();
        }
    }
    
    private void OnEnable()
    {
        OnButtonB += ButtonBPressed;
    }

    private void OnDisable()
    {
        OnButtonB -= ButtonBPressed;
    }

    private void OnValidate()
    {
        if (_controller == null) _controller = GetComponentInChildren<CaliController>();
        if (_headset == null) _headset = GetComponentInChildren<CaliHeadset>();
        if (_poles == null) _poles = GetComponentInChildren<CaliPoles>();
        if (_floorLevel == null) _floorLevel = GetComponentInChildren<CaliFloorLevel>();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.TransformPoint(_playerStartOffset), _playerStartRange);
    }

    #endregion

    private void SetPlayerInitialPosition()
    {
        // If the Player is within the Room Boundary Check
        if (_roomBoundaryCheck && _roomBoundaryCheck.bounds.Contains(MainCameraTransform.position))
            return;
        
        var offset = transform.TransformPoint(_playerStartOffset) - MainCameraTransform.position;
        offset += Random.insideUnitSphere * _playerStartRange;
        offset.y = 0;
        OvrRootTransform.position += offset;
    }

    private void ButtonBPressed()
    {
        if (!_calibrationActive)
        {
            StartControllerCalibration();
            OnButtonB?.Invoke();
        }
    }

    public void StartControllerCalibration()
    {
        if (!ControllerCalibration) return;
        StartCalibration();
        _controller.Enable();
    }

    public void StartHeadsetCalibration()
    {
        if (!ControllerCalibration) return;
        StartCalibration();
        _headset.Enable();
    }

    public void StartPolesCalibration()
    {
        if (!HeadsetCalibration) return;
        StartCalibration();
        _poles.Enable();
    }

    public void StartFloorLevelCalibration()
    {
        if (_floorLevelTracking) return;
        if (!FloorLevelCalibration) return;
        StartCalibration();
        _floorLevel.Enable();
    }

    private void StartCalibration()
    {
        if (_calibrationActive) DisableAll();
        _calibrationActive = true;
    }

    public void CompleteCalibration()
    {
        if (!_calibrationActive) return;
        _calibrationActive = false;
        OnComplete?.Invoke();
        DisableAll();
    }

    private void DisableAll()
    {
        _controller.Disable();
        _headset.Disable();
        _poles.Disable();
        _floorLevel.Disable();
    }

    public void SetActorTransform(Transform refTransform)
    {
        CalibrateTransforms(OvrRootTransform, MainCameraTransform, refTransform, _floorLevelTracking);
        
        //var rotationOffset = refTransform.rotation * Quaternion.Inverse(MainCameraTransform.rotation);
        //OvrRootTransform.rotation = rotationOffset * OvrRootTransform.rotation;

        // Attempt to correct for the camera's additional rotation
        //var yOffset = refTransform.rotation.eulerAngles.y + (refTransform.rotation.eulerAngles.y - MainCameraTransform.rotation.eulerAngles.y);
        //OvrRootTransform.rotation = Quaternion.Euler(new Vector3(0.0f, yOffset, 0.0f));
    }

    public static void CalibrateTransforms(Transform toCalibrate, Transform start, Transform goal, bool adjustHeight = false)
    {
        AdjustPosition();
        AdjustRotation();
        AdjustPosition();

        void AdjustPosition()
        {
            var offset = goal.position - start.position;
            if (!adjustHeight) offset.y = 0;
            toCalibrate.position += offset;
        }

        void AdjustRotation()
        {
            var rotationOffset = goal.eulerAngles.y - start.eulerAngles.y;
            toCalibrate.eulerAngles += new Vector3(0, rotationOffset, 0);
        }
    }
}
