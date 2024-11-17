using UnityEngine;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit;

// Source: https://stackoverflow.com/questions/62467088/oculus-quest-real-world-alignment
// https://twitter.com/IRCSS/status/1231523329183559681/photo/1
// The original code had functionality for scaling the scene which I removed because it messed with positioning

// This script requires that an OVRCameraRig be in the scene and have the PlayerOVR script attached

// https://github.com/microsoft/MixedRealityToolkit-Unity/issues/9765

namespace MILab
{
    internal enum AlignmentState
    {
        None,
        PivotOneSet,
        PivotTwoSet,
        PivotThreeSet,
    }
    
    public class CalibrationWithMRTK : MonoBehaviour, IMixedRealityInputActionHandler
    {
        [SerializeField] private Transform HandTransform;
        [SerializeField] private Transform PivotATransform;
        [SerializeField] private Transform PivotBTransform;
        [SerializeField] private AlignmentState alignmentState = AlignmentState.None;

        // These have to be set up correctly with the names matching those in the custom MRTK input profile
        [SerializeField] private string buttonOneDesc = "ButtonOnePress";
        [SerializeField] private string buttonTwoDesc = "ButtonTwoPress";

        private Vector3 resetPosition;
        private OVRHand rightHand;
        
        private static Transform OvrRootTransform => PlayerOVR.Instance.transform.parent;

        private void Start()
        {
            PivotATransform = transform.Find("PivotA");
            PivotBTransform = transform.Find("PivotB");
            HandTransform = PlayerOVR.Instance.transform.Find("TrackingSpace/RightHandAnchor/RightControllerAnchor");
            rightHand = PlayerOVR.Instance.transform.Find("TrackingSpace/RightHandAnchor/OVRHandPrefab_Right").GetComponent<OVRHand>();
            resetPosition = PlayerOVR.Instance.transform.position;
        }
        
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

        private void ResetTransform()
        {
            OvrRootTransform.localScale = new Vector3(1, 1, 1);
            OvrRootTransform.rotation = Quaternion.identity;
            OvrRootTransform.position = resetPosition;
            alignmentState = AlignmentState.None;
        }

        private void AdjustPosition()
        {
            Vector3 handOffset = HandTransform.position - OvrRootTransform.position;
            Vector3 newPosition = PivotATransform.position - handOffset;

            // Do not adjust Y position if using floor level tracking
            if (PlayerOVR.Instance.GetComponent<OVRManager>().trackingOriginType == OVRManager.TrackingOrigin.FloorLevel)
            {
                newPosition.y = resetPosition.y;
            }
            OvrRootTransform.position = newPosition;
        }

        public void OnActionStarted(BaseInputEventData eventData)
        {
            Debug.Log(eventData.MixedRealityInputAction.Description);

            string eventDescription = eventData.MixedRealityInputAction.Description;

            switch (alignmentState)
            {
                case AlignmentState.None:
                    if (eventDescription.Equals(buttonOneDesc))
                    {
                        // Move Player so that hand is at Pivot A
                        AdjustPosition();

                        alignmentState = AlignmentState.PivotOneSet;
                    }
                    break;



                case AlignmentState.PivotOneSet:
                    if (eventDescription.Equals(buttonTwoDesc))
                    {
                        // Reset
                        ResetTransform();
                    }
                    else if (eventDescription.Equals(buttonOneDesc))
                    {
                        // Rotate player so they face forward
                        Vector3 pivotAtoRealB = HandTransform.position - PivotATransform.position;
                        Vector3 pivotAtoVirtualB = PivotBTransform.position - PivotATransform.position;

                        float turnAngle = Vector3.SignedAngle(pivotAtoRealB, pivotAtoVirtualB, Vector3.up);

                        //PlayerOVR.Instance.transform.RotateAround(PivotATransform.position, Vector3.up, turnAngle);
                        OvrRootTransform.RotateAround(PivotATransform.position, Vector3.up, turnAngle);


                        alignmentState = AlignmentState.PivotTwoSet;
                    }
                    break;



                case AlignmentState.PivotTwoSet:
                    if (eventDescription.Equals(buttonTwoDesc))
                    {
                        // Reset
                        ResetTransform();
                    }
                    else if (eventDescription.Equals(buttonOneDesc))
                    {
                        // Move Player again so that the hand is at PivotA
                        AdjustPosition();

                        alignmentState = AlignmentState.PivotThreeSet;

                        // Hide pivot points
                        PivotATransform.gameObject.SetActive(false);
                        PivotBTransform.gameObject.SetActive(false);
                    }
                    break;



                case AlignmentState.PivotThreeSet:
                    if (eventDescription.Equals(buttonTwoDesc))
                    {
                        // Show the Pivot points
                        PivotATransform.gameObject.SetActive(true);
                        PivotBTransform.gameObject.SetActive(true);
                        ResetTransform();
                    }
                    break;
            }
        }

        public void OnActionEnded(BaseInputEventData eventData)
        {
        }
    }
}