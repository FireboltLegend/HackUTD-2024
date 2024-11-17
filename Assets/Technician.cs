using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Technician : MonoBehaviour
{
    public GameObject technicianPrefab; // The technician GameObject (public for assigning in inspector)
    public OVRHand leftHand; // Reference to the left hand (OVRHand component)
    public float distanceInFront = 0.5f; // How far in front of the hand the technician should appear

    private GameObject technicianInstance; // Holds the instance of the technician

    // Start is called before the first frame update
    void Start()
    {
        // Ensure technician is not active at the start
        if (technicianPrefab != null)
        {
            technicianInstance = Instantiate(technicianPrefab);
            technicianInstance.SetActive(false); // Disable it initially
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Check if T is pressed
        if (Input.GetKeyDown(KeyCode.T))
        {
            if (technicianInstance != null)
            {
                // If technician is already active, toggle it off
                technicianInstance.SetActive(!technicianInstance.activeSelf);

                // Position the technician in front of the left hand when activated
                if (technicianInstance.activeSelf)
                {
                    PositionTechnicianInFrontOfLeftHand();
                }
            }
        }
    }

    // Method to position the technician in front of the left hand
    void PositionTechnicianInFrontOfLeftHand()
    {
        if (leftHand != null && technicianInstance != null)
        {
            // Get the position and rotation of the left hand
            Vector3 leftHandPosition = leftHand.transform.position;
            Quaternion leftHandRotation = leftHand.transform.rotation;

            // Position the technician object in front of the left hand (at a set distance)
            technicianInstance.transform.position = leftHandPosition + leftHandRotation * Vector3.forward * distanceInFront;
            technicianInstance.transform.rotation = leftHandRotation;
        }
    }
}