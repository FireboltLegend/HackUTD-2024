using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandHaptics : MonoBehaviour
{
    [SerializeField, ReadOnly] GameObject leftThumbFingerTip;
    [SerializeField, ReadOnly] GameObject leftIndexFingerTip;
    [SerializeField, ReadOnly] GameObject leftMiddleFingerTip;
    [SerializeField, ReadOnly] GameObject leftRingFingerTip;
    [SerializeField, ReadOnly] GameObject leftPinkyFingerTip;

    [SerializeField, ReadOnly] GameObject rightThumbFingerTip;
    [SerializeField, ReadOnly] GameObject rightIndexFingerTip;
    [SerializeField, ReadOnly] GameObject rightMiddleFingerTip;
    [SerializeField, ReadOnly] GameObject rightRingFingerTip;
    [SerializeField, ReadOnly] GameObject rightPinkyFingerTip;

    bool leftHandollidersAdded = false;
    bool rightHandollidersAdded = false;

    void CheckNullAndApplyReferences()
    {
        if (leftThumbFingerTip == null)
            leftThumbFingerTip = GameObject.Find("OVRHandPrefab_Left/Bones/Hand_WristRoot/Hand_Thumb0/Hand_Thumb1/Hand_Thumb2/Hand_Thumb3/Hand_ThumbTip");
        if (leftIndexFingerTip == null)
            leftIndexFingerTip = GameObject.Find("OVRHandPrefab_Left/Bones/Hand_WristRoot/Hand_Index1/Hand_Index2/Hand_Index3/Hand_IndexTip");
        if (leftMiddleFingerTip == null)
            leftMiddleFingerTip = GameObject.Find("OVRHandPrefab_Left/Bones/Hand_WristRoot/Hand_Middle1/Hand_Middle2/Hand_Middle3/Hand_MiddleTip");
        if (leftRingFingerTip == null)
            leftRingFingerTip = GameObject.Find("OVRHandPrefab_Left/Bones/Hand_WristRoot/Hand_Ring1/Hand_Ring2/Hand_Ring3/Hand_RingTip");
        if (leftPinkyFingerTip == null)
            leftPinkyFingerTip = GameObject.Find("OVRHandPrefab_Left/Bones/Hand_WristRoot/Hand_Pinky0/Hand_Pinky1/Hand_Pinky2/Hand_Pinky3/Hand_PinkyTip");

        if (rightThumbFingerTip == null)
            rightThumbFingerTip = GameObject.Find("OVRHandPrefab_Right/Bones/Hand_WristRoot/Hand_Thumb0/Hand_Thumb1/Hand_Thumb2/Hand_Thumb3/Hand_ThumbTip");
        if (rightIndexFingerTip == null)
            rightIndexFingerTip = GameObject.Find("OVRHandPrefab_Right/Bones/Hand_WristRoot/Hand_Index1/Hand_Index2/Hand_Index3/Hand_IndexTip");
        if (rightMiddleFingerTip == null)
            rightMiddleFingerTip = GameObject.Find("OVRHandPrefab_Right/Bones/Hand_WristRoot/Hand_Middle1/Hand_Middle2/Hand_Middle3/Hand_MiddleTip");
        if (rightRingFingerTip == null)
            rightRingFingerTip = GameObject.Find("OVRHandPrefab_Right/Bones/Hand_WristRoot/Hand_Ring1/Hand_Ring2/Hand_Ring3/Hand_RingTip");
        if (rightPinkyFingerTip == null)
            rightPinkyFingerTip = GameObject.Find("OVRHandPrefab_Right/Bones/Hand_WristRoot/Hand_Pinky0/Hand_Pinky1/Hand_Pinky2/Hand_Pinky3/Hand_PinkyTip");
    }

    // Hand: Left = 0 , Right = 1
    void AddSphereColliderAndFingerHaptics(GameObject obj, int hand)
    {
        // Add Sphere Collider component to the GameObject
        SphereCollider sphereCollider = obj.AddComponent<SphereCollider>();

        // Set the properties for the Sphere Collider
        sphereCollider.center = new Vector3(0.003f, 0.0f, 0.0f);
        sphereCollider.radius = 0.005f;
        sphereCollider.isTrigger = true;


        FingerHaptics fingerHaptics = obj.AddComponent<FingerHaptics>();
        if (hand == 0)
            fingerHaptics.hand = (int)Bhaptics.SDK2.PositionType.GloveL;
        else
            fingerHaptics.hand = (int)Bhaptics.SDK2.PositionType.GloveR;

    }

    void CheckAndAddColliders()
    {
        if (!leftHandollidersAdded &&
            leftThumbFingerTip != null &&
            leftIndexFingerTip != null &&
            leftMiddleFingerTip != null &&
            leftRingFingerTip != null &&
            leftPinkyFingerTip != null)
        {
            // Add Sphere Collider to each GameObject with the specified properties
            AddSphereColliderAndFingerHaptics(leftThumbFingerTip,0);
            AddSphereColliderAndFingerHaptics(leftIndexFingerTip,0);
            AddSphereColliderAndFingerHaptics(leftMiddleFingerTip,0);
            AddSphereColliderAndFingerHaptics(leftRingFingerTip,0);
            AddSphereColliderAndFingerHaptics(leftPinkyFingerTip,0);

            leftHandollidersAdded = true;
        }

        if (!rightHandollidersAdded &&
            rightThumbFingerTip != null &&
            rightIndexFingerTip != null &&
            rightMiddleFingerTip != null &&
            rightRingFingerTip != null &&
            rightPinkyFingerTip != null)
        {
            AddSphereColliderAndFingerHaptics(rightThumbFingerTip,1);
            AddSphereColliderAndFingerHaptics(rightIndexFingerTip,1);
            AddSphereColliderAndFingerHaptics(rightMiddleFingerTip,1);
            AddSphereColliderAndFingerHaptics(rightRingFingerTip,1);
            AddSphereColliderAndFingerHaptics(rightPinkyFingerTip,1);

            rightHandollidersAdded = true;

        }
    }

    // Start is called before the first frame update
    void Start()
    {
        CheckNullAndApplyReferences();
    }

    // Update is called once per frame
    void Update()
    {
        CheckNullAndApplyReferences();

        CheckAndAddColliders();
    }
}