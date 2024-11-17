using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Script to make the cube casting the raycast, follow the center eye camera position (the HMD)
//Will not be used anymore
public class followCenterEye : MonoBehaviour
{
    public string cameraName = "CenterEyeAnchor";
    private Transform cameraTransform;

    private void Start()
    {
        // Find the camera with the specified name
        GameObject cameraObject = GameObject.Find(cameraName);

        if (cameraObject != null)
        {
            cameraTransform = cameraObject.transform;
        }
    }

    private void Update()
    {
        if (cameraTransform != null)
        {
            // Set the position and rotation of the object to match the camera
            transform.position = new Vector3(cameraTransform.position.x, cameraTransform.position.y -0.02f, cameraTransform.position.z);
            //transform.rotation = cameraTransform.rotation;

            // Calculate the rotation looking downward
            float downwardAngle = -15f;

            Quaternion targetRotation = Quaternion.LookRotation(cameraTransform.forward, Vector3.down);
            targetRotation *= Quaternion.Euler(downwardAngle, 0f, 0f); // Apply downward angle adjustment

            // Set the rotation of the object
            transform.rotation = targetRotation;
        }
    }
}
