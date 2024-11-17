using UnityEngine;

// Source: https://stackoverflow.com/questions/62467088/oculus-quest-real-world-alignment
// https://twitter.com/IRCSS/status/1231523329183559681/photo/1
// The original code had functionality for scaling the scene which I removed because it messed with positioning

// This script requires that an OVRCameraRig be in the scene and have the PlayerOVR script attached

namespace MILab
{
    public class Calibration : MonoBehaviour
    {
        [SerializeField]
        private Transform HandTransform;

        [SerializeField]
        private Transform PivotATransform;

        [SerializeField]
        private Transform PivotBTransform;

        [SerializeField]
        private AligmentState alignmentState = AligmentState.None;

        private Vector3 resetPosition;
        private OVRHand rightHand;

        public enum AligmentState
        {
            None,
            PivotOneSet,
            PivotTwoSet,
            PivotThreeSet,
        }

        private void Start()
        {
            PivotATransform = transform.Find("PivotA");
            PivotBTransform = transform.Find("PivotB");
            HandTransform = PlayerOVR.Instance.transform.Find("TrackingSpace/RightHandAnchor/RightControllerAnchor");
            rightHand = PlayerOVR.Instance.transform.Find("TrackingSpace/RightHandAnchor/OVRHandPrefab").GetComponent<OVRHand>();
            resetPosition = PlayerOVR.Instance.transform.position;
        }

        void Update()
        {
            switch (alignmentState)
            {
                case AligmentState.None:
                    if (OVRInput.GetDown(OVRInput.Button.One) && !rightHand.GetFingerIsPinching(OVRHand.HandFinger.Index))
                    {
                        // Move Player so that hand is at Pivot A
                        adjustPosition();

                        alignmentState = AligmentState.PivotOneSet;
                    }
                    break;



                case AligmentState.PivotOneSet:
                    if (OVRInput.GetDown(OVRInput.Button.Two))
                    {
                        // Reset
                        ResetTransform();
                    }
                    else if (OVRInput.GetDown(OVRInput.Button.One) && !rightHand.GetFingerIsPinching(OVRHand.HandFinger.Index))
                    {
                        // Rotate player so they face forward
                        Vector3 pivotAtoRealB = HandTransform.position - PivotATransform.position;
                        Vector3 pivotAtoVirtualB = PivotBTransform.position - PivotATransform.position;

                        float turnAngle = Vector3.SignedAngle(pivotAtoRealB, pivotAtoVirtualB, Vector3.up);

                        PlayerOVR.Instance.transform.RotateAround(PivotATransform.position, Vector3.up, turnAngle);

                        alignmentState = AligmentState.PivotTwoSet;
                    }
                    break;



                case AligmentState.PivotTwoSet:
                    if (OVRInput.GetDown(OVRInput.Button.Two))
                    {
                        // Reset
                        ResetTransform();
                    }
                    else if (OVRInput.GetDown(OVRInput.Button.One) && !rightHand.GetFingerIsPinching(OVRHand.HandFinger.Index))
                    {
                        // Move Player again so that the hand is at PivotA
                        adjustPosition();

                        alignmentState = AligmentState.PivotThreeSet;

                        // Hide pivot points
                        PivotATransform.gameObject.SetActive(false);
                        PivotBTransform.gameObject.SetActive(false);
                    }
                    break;



                case AligmentState.PivotThreeSet:
                    if (OVRInput.GetDown(OVRInput.Button.Two))
                    {
                        // Show the Pivot points
                        PivotATransform.gameObject.SetActive(true);
                        PivotBTransform.gameObject.SetActive(true);
                        ResetTransform();
                    }
                    break;
            }
        }

        void ResetTransform()
        {
            // Reset
            PlayerOVR.Instance.transform.localScale = new Vector3(1, 1, 1);
            PlayerOVR.Instance.transform.rotation = Quaternion.identity;
            PlayerOVR.Instance.transform.position = resetPosition;
            alignmentState = AligmentState.None;
        }

        void adjustPosition()
        {
            Vector3 handOffset = HandTransform.position - PlayerOVR.Instance.transform.position;
            Vector3 newPosition = PivotATransform.position - handOffset;

            // Do not adjust Y position if using floor level tracking
            if (PlayerOVR.Instance.GetComponent<OVRManager>().trackingOriginType == OVRManager.TrackingOrigin.FloorLevel)
            {
                newPosition.y = resetPosition.y;
            }
            PlayerOVR.Instance.transform.position = newPosition;
        }
    }
}