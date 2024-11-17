using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OVR;
using ReadyPlayerMe.Samples.QuickStart;
using System.ComponentModel;

public class Technician : MonoBehaviour
{
    public GameObject technicianPrefab;
    public OVRHand leftHand;
    public OVRCameraRig mainCameraRig;
    public float distanceInFront = 0.05f;
    [SerializeField, ReadOnly(true)] private GameObject rightIndexTip;

    private GameObject technicianInstance;

    void Start()
    {
        if (technicianPrefab != null)
        {
            technicianInstance = Instantiate(technicianPrefab);
            technicianInstance.SetActive(false);
        }

        if (mainCameraRig == null)
        {
            mainCameraRig = FindObjectOfType<OVRCameraRig>();
        }

        rightIndexTip = GameObject.Find("Hand_Index3_CapsuleRigidbody");
    }

    void Update()
    {
        if(rightIndexTip == null)
        {
            rightIndexTip = GameObject.Find("Hand_Index3_CapsuleRigidbody");
        }

        if (Input.GetKeyDown(KeyCode.T) && technicianInstance != null)
        {
            technicianInstance.SetActive(!technicianInstance.activeSelf);
        }

        if (technicianInstance.activeSelf)
        {
            PositionTechnicianInFrontOfLeftHand();
        }
    }

    void PositionTechnicianInFrontOfLeftHand()
    {
        if (leftHand != null && technicianInstance != null && mainCameraRig != null)
        {
            Transform leftHandTransform = leftHand.transform;
            Transform cameraTransform = mainCameraRig.centerEyeAnchor;

            Vector3 leftHandPosition = leftHandTransform.position;
            Quaternion leftHandRotation = leftHandTransform.rotation;


            technicianInstance.transform.position = leftHandPosition + leftHandRotation * Vector3.forward * distanceInFront;

            Vector3 directionToCamera = cameraTransform.position - technicianInstance.transform.position;
            directionToCamera.y = 0;
technicianInstance.transform.rotation = Quaternion.LookRotation(cameraTransform.position - technicianInstance.transform.position);
        }
    }
}
