using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit;

namespace MILab
{
    public class CalibrationMRTKPassthrough : MonoBehaviour, IMixedRealityInputActionHandler
    {
        [Header("Calibration")]
        [SerializeField] private bool _calibrationActive;
        [SerializeField] private Transform _overrideRigPosition;
        [SerializeField] private int _calibrationStep = -1;
        [SerializeField] private List<CalibrationPoint> _calibrationPoints;
        
        [Header("Input")]
        [SerializeField] private string _startCalibrationButton = "ButtonOnePress";
        [SerializeField] private string _confirmCalibrationButton = "ButtonTwoPress";
        [SerializeField, ReadOnly] private Transform _rightHand;

        #region Properties

        private Vector3 RigPosition => _overrideRigPosition ? _overrideRigPosition.position : Vector3.zero;
        private Quaternion RigRotation => _overrideRigPosition ? _overrideRigPosition.rotation : Quaternion.identity;
        
        private static Transform _ovrTransform;
        private static Transform OvrTransform
        {
            get
            {
                if (_ovrTransform == null) _ovrTransform = PlayerOVR.Instance.transform;
                return _ovrTransform;
            }
        }
        
        private static Transform _ovrRootTransform;
        private static Transform OvrRootTransform
        {
            get
            {
                if (_ovrRootTransform == null) _ovrRootTransform = PlayerOVR.Instance.transform.parent;
                return _ovrRootTransform;
            }
        }

        private const string RightHandLocation = "TrackingSpace/RightHandAnchor/OVRHandPrefab_Right";
        private Transform RightHand
        {
            get
            {
                if (_rightHand == null) _rightHand = OvrTransform.Find(RightHandLocation);
                return _rightHand;
            }
        }

        private static bool _floorLevelTracking;

        private static bool FloorLevelTracking
        {
            get
            {
                var manager = OvrTransform ? OvrTransform.GetComponent<OVRManager>() : null;
                _floorLevelTracking = manager.trackingOriginType == OVRManager.TrackingOrigin.FloorLevel;
                return false;
            }
        }
        
        #endregion
        
        #region Event Handling

        private void OnEnable()
        {
            // Instruct Input System that we would like to receive all input events of type
            CoreServices.InputSystem?.RegisterHandler<IMixedRealityInputActionHandler>(this);
        }

        private void OnDisable()
        {
            // This component is being destroyed
            // Instruct the Input System to disregard us for input event handling
            CoreServices.InputSystem?.UnregisterHandler<IMixedRealityInputActionHandler>(this);
        }
        
        #endregion

        public void OnActionStarted(BaseInputEventData eventData)
        {
            string eventDescription = eventData.MixedRealityInputAction.Description;

            if (eventDescription.Equals(_startCalibrationButton))
            {
                // Reset Transforms
                OvrRootTransform.position = RigPosition;
                OvrRootTransform.rotation = RigRotation;
                _calibrationStep = 0;
            }
            else if (eventDescription.Equals(_confirmCalibrationButton))
            {
                var calibration = _calibrationPoints[_calibrationStep];
                
                Vector3 handOffset = RightHand.position - OvrRootTransform.position;
                Vector3 calibrationOffset = calibration.Point.position - handOffset;

                if (PlayerOVR.Instance.GetComponent<OVRManager>().trackingOriginType == OVRManager.TrackingOrigin.FloorLevel)
                {
                    calibrationOffset.y = RigPosition.y;
                }
                //OvrRootTransform.position = newPosition;
            }
        }

        public void OnActionEnded(BaseInputEventData eventData)
        {
        }
    }

    [System.Serializable]
    internal class CalibrationPoint
    {
        public Transform Point;
        public bool SetPosition;
        public bool SetRotation;
    }
}