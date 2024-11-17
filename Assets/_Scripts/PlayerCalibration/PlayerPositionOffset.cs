using UnityEngine;
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Input;
using MILab;
using OVRTouchSample;
using System.Collections;

public class PlayerPositionOffset : MonoBehaviour
{
    private Vector3 defaultPosition;
    private Vector3 defaultCalibrationSquarePos;
    private Quaternion defaultCalibrationSquareRot;
    private Vector3 defaultVirtualSquarePos;

    private GameObject parentObject; // The parent GameObject
    private float radius = 0.001f; // Radius of the circle
    private int buttonAPressCount = 0;

    public GameObject pivot1;
    public GameObject pivot2;
    public GameObject virtualPivot1;
    public GameObject virtualPivot2;
    public GameObject calibrationSquare;
    public  GameObject virtualSquare;
    public GameObject MiLabObjects;

    private GameObject player;
    private GameObject fingerSphere;

    private bool buttonCooldown = false;
    private bool passthroughIsOn = false;
    private bool updateRunning;

    public PassthroughCtrl _passthroughController;

    public void StartCalibration()
    {
        //TODO add public object and drag....
        // Find pole1 and pole2 using GameObject.Find
        //pivot1 = GameObject.Find("CalibrationPivot1");
        //pivot2 = GameObject.Find("CalibrationPivot2");
        //virtualPivot1 = GameObject.Find("VirtualPivot1");
        //virtualPivot2 = GameObject.Find("VirtualPivot2");
        //calibrationSquare = GameObject.Find("CalibrationSquare");
        //virtualSquare = GameObject.Find("VirtualSquare");
        //MiLabObjects = GameObject.Find("Room_Layout_02");

        // Find the player object using GameObject.Find
        player = GameObject.Find("MixedRealityPlayspace");

        //Store Default positions
        defaultPosition = player.transform.position;
        defaultCalibrationSquarePos = calibrationSquare.transform.position;
        defaultCalibrationSquareRot = calibrationSquare.transform.rotation;
        defaultVirtualSquarePos = virtualSquare.transform.position;


        parentObject = GameObject.Find("Hand_IndexTip"); // Find the parent GameObject by name
        if (parentObject != null)
        {
            CreateSphere();
        }
        else
        {
            Debug.LogError("Parent GameObject not found.");
        }


        ShowCalibrationObjects();
        MiLabObjects.SetActive(false); //Reverse of others. Room should show

        //start update function
        updateRunning = true;
        
        if (passthroughIsOn == false)
        {
            passthroughIsOn = true;
            _passthroughController.TogglePassthrough(passthroughIsOn);
        }

    }

    public void ReCalibrate()
    {
        buttonAPressCount = 0;
        ShowCalibrationObjects();
        MiLabObjects.SetActive(false); //Reverse of others. Room should show
        ResetAll();
        updateRunning = true;

        if (passthroughIsOn == false)
        {
            passthroughIsOn = true;
            _passthroughController.TogglePassthrough(passthroughIsOn);
        }
    }

    public void TogglePassthrough()
    {
        if (passthroughIsOn == true)
        {
            passthroughIsOn = false;
            _passthroughController.TogglePassthrough(passthroughIsOn);
            MiLabObjects.SetActive(true); //Reverse of others. Room should show
        }
        else if (passthroughIsOn == false)
        {
            passthroughIsOn = true;
            _passthroughController.TogglePassthrough(passthroughIsOn);
            MiLabObjects.SetActive(false); //Reverse of others. Room should show
        }
    }

    //Cancel/Stop calibration
    public void StopCalibration()
    {
        if (updateRunning == true)
        {
            updateRunning = false;
            HideCalibrationObjects();
            MiLabObjects.SetActive(true); //Reverse of others. Room should show
        }

        if (passthroughIsOn == true)
        {
            passthroughIsOn = false;
            _passthroughController.TogglePassthrough(passthroughIsOn);
        }
    }

    private void Update()
    {
        if (updateRunning == true)
        {
            // Check for collision between fingerSphere and CalibrationPivot1
            if (fingerSphere != null && pivot1 != null)
            {
                if (fingerSphere.GetComponent<Collider>().bounds.Intersects(pivot1.GetComponent<Collider>().bounds))
                {
                    if (!buttonCooldown)
                    {
                        buttonAPressCount++;
                        StartCoroutine(ButtonCooldown());

                        if (buttonAPressCount == 1)
                        {
                            print("Update Position");
                            CalculatePosition();
                            PerformRotation();
                        }
                        else if (buttonAPressCount == 2)
                        {
                            print("Performing Rotation");
                            ReAdjustPosition();
                            HideCalibrationObjects();
                            MiLabObjects.SetActive(true); //Reverse of others. Room should show
                            StopCalibration();
                        }
                    }
                }
            }

            // ------uncoment to use controllers------
            // Check for button A press on the Oculus controller
            //bool buttonAPressed = OVRInput.GetDown(OVRInput.Button.One);

            //if (buttonAPressed)
            //{
            //    buttonAPressCount++;

            //    if (buttonAPressCount == 1)
            //    {
            //        print("Update Position");
            //        CalculatePosition();
            //        PerformRotation();
            //    }
            //    else if (buttonAPressCount == 2)
            //    {
            //        print("Performing Rotation");
            //        ReAdjustPosition();
            //    }
            //}

            // Check for button B press on the Oculus controller
            //bool buttonBPressed = OVRInput.GetDown(OVRInput.Button.Two);

            //if (buttonBPressed)
            //{
            //    print("Reset");
            //    buttonAPressCount = 0;
            //    ResetAll();
            //}
        }
    }

    private IEnumerator ButtonCooldown()
    {
        buttonCooldown = true;
        yield return new WaitForSeconds(1f); // Adjust the cooldown duration as needed
        buttonCooldown = false;
    }

    private void CalculatePosition()
    {
        Vector3 offset = virtualSquare.transform.position - calibrationSquare.transform.position;

        Vector3 handoffset = new Vector3(pivot1.transform.position.x - player.transform.position.x, 0, pivot1.transform.position.z - player.transform.position.z);

        calibrationSquare.transform.position += new Vector3(offset.x, 0, offset.z);

        Vector3 newPosition = new Vector3(pivot1.transform.position.x + handoffset.x, 0, pivot1.transform.position.z + handoffset.z);
        player.transform.position = newPosition;
    }

    private void ReAdjustPosition()
    {
        Vector3 offset = virtualSquare.transform.position - calibrationSquare.transform.position;

        Vector3 handoffset = new Vector3(player.transform.position.x - pivot1.transform.position.x, 0, player.transform.position.z - pivot1.transform.position.z);

        calibrationSquare.transform.position += new Vector3(offset.x, 0, offset.z);

        //Note also added default position.y to fix offset problem
        Vector3 newPosition = new Vector3(pivot1.transform.position.x + handoffset.x, 0, pivot1.transform.position.z + handoffset.z);
        player.transform.position = newPosition;

    }


    private void PerformRotation()
    {
        // Calculate the direction vectors
        Vector3 virtualDirection = virtualPivot1.transform.position - virtualPivot2.transform.position;
        Vector3 realDirection = pivot1.transform.position - pivot2.transform.position;

        // Calculate the angle between the initial and current directions
        float angle = Vector3.SignedAngle(virtualDirection, realDirection, Vector3.up);

        //player.transform.Rotate(0, angle, 0);
        calibrationSquare.transform.Rotate(0, -angle, 0);

        //rotate player
        player.transform.Rotate(Vector3.up, -angle, Space.Self);
    }

    private void ResetAll()
    {
        if (defaultPosition != null)
        {
            // Reset the player position to the default position
            player.transform.position = defaultPosition;

            //Reset cube positions
            calibrationSquare.transform.position = defaultCalibrationSquarePos;
            calibrationSquare.transform.rotation = defaultCalibrationSquareRot;

            virtualSquare.transform.position = defaultVirtualSquarePos;

        }
    }

    private void HideCalibrationObjects()
    {
        pivot1.SetActive(false);
        pivot2.SetActive(false);
        virtualPivot1.SetActive(false);
        virtualPivot2.SetActive(false);
        calibrationSquare.SetActive(false);
        virtualSquare.SetActive(false);
        fingerSphere.SetActive(false);
    }

    private void ShowCalibrationObjects()
    {
        pivot1.SetActive(true);
        pivot2.SetActive(true);
        virtualPivot1.SetActive(true);
        virtualPivot2.SetActive(true);
        calibrationSquare.SetActive(true);
        virtualSquare.SetActive(true);
        fingerSphere.SetActive(true);
    }

    private void CreateSphere()
    {
        fingerSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere); // Create the fingerSphere GameObject
        fingerSphere.transform.parent = parentObject.transform; // Set the parent

        // Set the position and scale of the fingerSphere
        fingerSphere.transform.localPosition = Vector3.zero; // Place the fingerSphere at the parent's origin
        fingerSphere.transform.localScale = Vector3.one * radius * 2f; // Set the scale based on the radius

        // Set the material for rendering
        //Renderer sphereRenderer = fingerSphere.GetComponent<Renderer>();
        //Material sphereMaterial = Resources.Load<Material>("Models/Materials/FingerCollider");
        //sphereRenderer.sharedMaterial = sphereMaterial;

        // Add a fingerSphere collider
        SphereCollider sphereCollider = fingerSphere.AddComponent<SphereCollider>();
        sphereCollider.radius = radius;
    }

} //end of class
