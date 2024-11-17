using UnityEngine;

// Source: https://stackoverflow.com/questions/62467088/oculus-quest-real-world-alignment
// https://twitter.com/IRCSS/status/1231523329183559681/photo/1
// The original code had functionality for scaling the scene which I removed because it messed with positioning

namespace MILab
{
    public class AlignPlayer : MonoBehaviour
    {
        [Tooltip("Drag in the OVR rig hand used to click the buttons here")]
        public Transform HandTransform;
        [Tooltip("Drag in the Pivot A here")]
        public Transform PivotATransform;
        [Tooltip("Drag in the Pivot B here")]
        public Transform PivotBTransform;

        public AligmentState alignmentState = AligmentState.None;

        private Vector3 resetPosition;
        private OVRManager ovrManager;

        public enum AligmentState
        {
            None,
            PivotOneSet,
            PivotTwoSet,
            PivotThreeSet,
        }

        private void Awake()
        {
            resetPosition = gameObject.transform.position;
            ovrManager = GetComponent<OVRManager>();
        }

        void Update()
        {
            switch (alignmentState)
            {
                case AligmentState.None:
                    if (!OVRInput.GetDown(OVRInput.Button.One))
                        return;

                    // Move Player so that hand is at Pivot A
                    adjustPosition();

                    alignmentState = AligmentState.PivotOneSet;
                    break;



                case AligmentState.PivotOneSet:
                    if (OVRInput.GetDown(OVRInput.Button.Two))
                    {
                        // Reset
                        ResetTransform();
                    }

                    if (!OVRInput.GetDown(OVRInput.Button.One))
                        return;

                    // Rotate player so they face forward
                    Vector3 pivotAtoRealB = HandTransform.position - PivotATransform.position;
                    Vector3 pivotAtoVirtualB = PivotBTransform.position - PivotATransform.position;

                    float turnAngle = Vector3.SignedAngle(pivotAtoRealB, pivotAtoVirtualB, Vector3.up);
                    //transform.Rotate(0f, turnAngle, 0f, Space.Self);
                    transform.RotateAround(PivotATransform.position, Vector3.up, turnAngle);

                    alignmentState = AligmentState.PivotTwoSet;
                    break;



                case AligmentState.PivotTwoSet:
                    if (OVRInput.GetDown(OVRInput.Button.Two))
                    {
                        // Reset
                        ResetTransform();
                    }
                    if (!OVRInput.GetDown(OVRInput.Button.One))
                        return;

                    // Move Player again so that the hand is at PivotA
                    adjustPosition();

                    alignmentState = AligmentState.PivotThreeSet;
                    break;



                case AligmentState.PivotThreeSet:
                    if (OVRInput.GetDown(OVRInput.Button.Two))
                    {
                        ResetTransform();
                    }
                    break;
            }
        }

        void ResetTransform()
        {
            // Reset
            transform.localScale = new Vector3(1, 1, 1);
            transform.rotation = Quaternion.identity;
            transform.position = resetPosition;
            alignmentState = AligmentState.None;
        }

        void adjustPosition()
        {
            Vector3 handOffset = HandTransform.position - transform.position;
            Vector3 newPosition = PivotATransform.position - handOffset;

            // Do not adjust Y position if using floor level tracking
            if (ovrManager.trackingOriginType == OVRManager.TrackingOrigin.FloorLevel)
            {
                newPosition.y = resetPosition.y;
            }
            transform.position = newPosition;
        }
    }
}