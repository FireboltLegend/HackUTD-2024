using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Photon.Pun;
using TMPro;

namespace MILab
{

    public class PeripheralPositionUpdater : MonoBehaviour
    {

        [SerializeField, ReadOnly] string PeripheralName = "Keyboard";

        public class MyData
        {
            public int IsDynamicUpdateEnabled;
            public string Position;
            public string Rotation;
        }

        //-----------GPT------------------
        public int maxListSize = 1;
        public int precision = 2; // Adjust the precision as needed.

        [SerializeField] private List<TransformData> transformList = new List<TransformData>();


        [System.Serializable]
        public struct TransformData
        {
            public Vector3 position;
            public Vector3 rotation;
        }

        [SerializeField, ReadOnly] GameObject mostPopularCenterEye;
        [SerializeField, ReadOnly] Vector3 mostPopularCenterEyePosition;
        [SerializeField, ReadOnly] Vector3 mostPopularCenterEyeRotation;

        //-----------GPT-END----------------

        //For database
        private bool callingAPI = false;
        private bool callingAPIPut = true;

        //for bottle object
        
        [SerializeField, ReadOnly] string objectDetectedMaxCoord;
        [SerializeField, ReadOnly] string objectDetectedMinCoord;


        private Vector3 dynamic_pos;
        private Quaternion dynamic_rot;

        //public float monitorHeight = 1.1f;
        private bool isInGoodView = false;

        //HMD gameobject
        public GameObject centerEye;
        GameObject playerCamera;
        public Vector3 oldPosition; //= new Vector3(0, 0, 0);
        public Vector3 oldRotation;

        [Header("ZERO frame comparison")]
        Vector3 zeroFramePosition; //= new Vector3(0, 0, 0);
        Vector3 zeroFrameRotation;
        [SerializeField, ReadOnly] Vector3 diffDistanceFromZero;
        [SerializeField, ReadOnly] Vector3 diffRotationFromZero;

        //[SerializeField]
        // float rotationAngle = 91.04f;

        [Header("Calibration")]
        [SerializeField] int objectZone;
        [SerializeField] ObjectSTManager objectSTManager;
        [SerializeField] GameObject objCoordinate;
        [SerializeField, ReadOnly] Vector3 currentObjRotation;
        [SerializeField] GameObject resetButton;
        [SerializeField] float monitorHeightOffset = 0.705f;//0.717f;//0.7370126f; //0.705f; //0.2595f; // adding this height offset to lower monitor due to monitor model center
        [SerializeField] float factorHeadTilt = 1;
        [SerializeField] int signHeadTilt = 1;

        [SerializeField] float lidarOffset = 0.12f;
        [SerializeField] GameObject lidarCamera;
        [SerializeField, ReadOnly] GameObject lidarAnchor;

        [SerializeField, ReadOnly] string objectDetectedCoordString;
        String[] objectDetectedCoord;

        [SerializeField, ReadOnly] float objectAngle;

        bool isCalibrating;

        String[] networkPos;
        String[] networkRot;
        [SerializeField, ReadOnly] bool isDetectable;
        [SerializeField, ReadOnly] int isDynamicUpdateEnabled;
        int newIsDynamicUpdateEnabled;

        [Header("Allowed Movemement Area for Calibration")]
        [SerializeField] float minX;
        [SerializeField] float maxX;
        [SerializeField] float minZ;
        [SerializeField] float maxZ;

        [Header("Calibration Threshold")]
        [SerializeField] float movementThreshold = 0.1f; // Adjustable threshold for head movement
        Quaternion previousRotation;

        // CALIBRATION UI VARIABLES
        public TextMeshPro calibrationMsg;
        public float countdownDuration = 1f;
        [SerializeField] GameObject calibrationUI;
        [SerializeField, ReadOnly] float distanceFromUser;
        [SerializeField] PassthroughCtrl passthroughCtrl;

        // CALIBRATION MODE (DIFFFERENT FROM DYNAMIC MODE)
        public bool isCalibrationModeEnabled;
        bool isOneShotNotDynamic;
        Vector3 previousMonitorPosition;
        Vector3 currentMonitorPosition;
        float monitorPosThres = 0.05f;

        //private PhotonView photonView;

        [SerializeField] float shiftThreshold = 0f; // negative moves left, positive moves right

        [SerializeField, ReadOnly] int stabilizedFrames = 0;
        int triggerStabilizedFrameNum = 20;

        [Header("DEBUG")]
        
        [SerializeField, ReadOnly] Vector3 differenceThres;
        [SerializeField, ReadOnly] Vector3 diffRotation;
        [SerializeField, ReadOnly] float differenceThresValue = 0.03f;
        [SerializeField] float maxThresValue = 0.25f;
        [SerializeField, ReadOnly] float differenceRotThresValue = 0.05f;
        [SerializeField, ReadOnly] float monitorDepthOffset = 0.05f;
        bool dynamicCalibrationMode = false;

        float oldMonitorDistanceFromUser = 0f;


        void CheckPlayer()
        {
            if (mostPopularCenterEye == null)
                mostPopularCenterEye = GameObject.Find("CenterEyeAnchor"); //GameObject.Find("CenterEyeAnchor");
            if (playerCamera == null)
                playerCamera = GameObject.Find("TrackingSpace"); //CustomMRTK-Quest_OVRCameraRig
            if (centerEye == null)
                centerEye = GameObject.Find("CenterEyeAnchor");
            if (lidarAnchor == null)
                lidarAnchor = GameObject.Find("LiDARAnchor");
        }

        public void setCallingApiActive(bool setValue)
        {
            callingAPI = setValue;
        }

        // Coroutine to make GET request
        IEnumerator GetObjectData()
        {
            using (UnityWebRequest request = UnityWebRequest.Get(String.Format("http://172.16.136.143:8080/Object_detection/"+PeripheralName)))
            {
                callingAPI = true;
                //yield return new WaitForSeconds(2.0f);

                yield return request.Send();
                while (!request.isDone)
                    yield return null;
                //Debug.Log(request.result);

                byte[] result = request.downloadHandler.data;
                string ObjectJSON = System.Text.Encoding.Default.GetString(result);
                ObjectData info = JsonUtility.FromJson<ObjectData>(ObjectJSON);

                //--Data we got from database--
                //info."updateKeyName"
               
                objectDetectedCoordString = info.Coordinate;
                objectDetectedCoord = objectDetectedCoordString.Split(",");

                objectAngle = info.Angle;
                //Debug.LogFormat("<color=cyan>Keyboard angle: " + objectAngle + "</color>");

                //Debug.Log(objectDetectedMaxCoord);
                //Debug.Log(objectDetectedMinCoord);
                //-----------------------------

                isDetectable = info.IsDetected;
                //isDynamicUpdateEnabled = info.IsDynamicUpdateEnabled;
                //Debug.LogFormat("<color=cyan>" + info.IsDynamicUpdateEnabled + "</color>");
                //Debug.LogFormat("<color=green>" + info.LeftCoordinate + "</color>");
                //Debug.LogFormat("<color=green>" + info.RightCoordinate + "</color>");
                //Debug.LogFormat("<color=green>" + float.Parse(leftCoordinateString[2]) * (float)Math.Sin(0.349066f) + "</color>");
                //Debug.LogFormat("<color=blue>" + isDetectable + "</color>");
                UpdateObjectCoordinatesAndAngle();
                //yield return new WaitForSeconds(1.0f);

                // Update Pos and Rot from Network
                if (isDynamicUpdateEnabled == 0)
                {
                    networkPos = info.Position.Split(",");
                    networkRot = info.Rotation.Split(",");
                    //transform.rotation = new Quaternion(float.Parse(networkRot[0]), float.Parse(networkRot[1]), float.Parse(networkRot[2]), float.Parse(networkRot[3]));
                    //transform.position = new Vector3(float.Parse(networkPos[0]), float.Parse(networkPos[1]), float.Parse(networkPos[2]));
                }
                else
                {
                    //transform.rotation = dynamic_rot;
                    //transform.position = dynamic_pos;
                }
                

                

                // !! FOR TESTING ONLY - REMOVE/COMMENT FOR DEMOS !!
                if (isDetectable && isDynamicUpdateEnabled==1)
                    UpdatePositionAndPose();
                // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!

                callingAPI = false;

            } //end of request

        } // end of GetObjectData

        // Coroutine to make PUT request
        IEnumerator PutObjectData()
        {
            //Debug.LogFormat("<color=cyan>" + "Entering PutObjectData" + "</color>");
            //Debug.LogFormat("<color=blue>" + "Entering PutObjectData" + "</color>");
            byte[] myData = System.Text.Encoding.UTF8.GetBytes("{}"); //IsDynamicUpdateEnabled: " + newIsDynamicUpdateEnabled + "
            //Debug.Log("PUT lamp: " + lightOn);
            String body = "";
            //if (lightOn == true)
            //    body = "{\"lamp_status\":" + 1 + "}";
            //else
            //    body = "{\"lamp_status\":" + 0 + "}";
            String pos = transform.position.ToString("F4");
            String rot = transform.rotation.ToString("F4");
            //Debug.LogFormat("<color=cyan>" + pos + "</color>");
            //Debug.LogFormat("<color=pink>" + rot + "</color>");
            /*body = "{\"IsDynamicUpdateEnabled\":" + isDynamicUpdateEnabled + " , " + "\"Position\":" + pos[1..^1] + "," + "\"Rotation\":" + rot[1..^1] + "}";*/

            MyData data = new MyData();

            data.IsDynamicUpdateEnabled = isDynamicUpdateEnabled;
            //if(data.IsDynamicUpdateEnabled == 0)
            //{
                data.Position = pos[1..^1];
                data.Rotation = rot[1..^1];
            //}
            
            string jsonBody = JsonUtility.ToJson(data);
            byte[] body_data = System.Text.Encoding.UTF8.GetBytes(jsonBody);

            //using (UnityWebRequest req = UnityWebRequest.Put(String.Format("http://172.16.136.165:5000/lamp/1?status={0}", lightOn), myData))
            //using (UnityWebRequest req = UnityWebRequest.Put(String.Format("http://digitaltwin-henrykim.pitunnel.com/lamp/1?status={0}", lightOn), myData))
            using (UnityWebRequest req = UnityWebRequest.Put("http://172.16.136.143:8080/Object_detection/"+PeripheralName, body_data))
            {
                callingAPIPut = true;
                //Debug.LogFormat("<color=red>" + "Sending PUT data to server" + "</color>");
                req.SetRequestHeader("Content-Type", "application/json");
                yield return req.Send();
                while (!req.isDone)
                    yield return null;
                byte[] result = req.downloadHandler.data;

                string resText = System.Text.Encoding.Default.GetString(result);
                
                //LampData info = JsonUtility.FromJson<LampData>(lampJSON);
                callingAPIPut = false;
            }
        }

        //[Button]
        public bool ToggleDynamicAndManual()
        {
            if (isDynamicUpdateEnabled == 1)
                isDynamicUpdateEnabled = 0;
            else
                isDynamicUpdateEnabled = 1;
            newIsDynamicUpdateEnabled = isDynamicUpdateEnabled;
            StartCoroutine(PutObjectData());
            
            return (isDynamicUpdateEnabled == 1);
        }

        void UpdateObjectCoordinatesAndAngle()
        {
            CheckPlayer();
            
            objCoordinate.transform.position = lidarAnchor.transform.TransformPoint(new Vector3(float.Parse(objectDetectedCoord[0]),
                -float.Parse(objectDetectedCoord[1])+0.09f,
               float.Parse(objectDetectedCoord[2])));

            //currentObjRotation = mostPopularCenterEye.transform.TransformDirection(new Vector3(0, objectAngle, 0));

        } //end of UpdateLeftAndRightPointCoordinates

        
        IEnumerator CalibrationCountdown()
        {
            //calibrationMsg.enabled = true;
            // display and start the countdown
            isCalibrating = true;
            float remainingTime = countdownDuration;

            while (remainingTime > 0)
            {
                yield return new WaitForSeconds(1f);
                remainingTime -= 1f;
            }
            UpdatePositionAndPose();
            isCalibrating = false;
            //calibrationMsg.enabled = false;
        }

        IEnumerator ShowSyncMessage()
        {
            calibrationMsg.text = "Synchronized";
            calibrationMsg.enabled = true;
            yield return new WaitForSeconds(1.5f);
            calibrationMsg.enabled = false;
        }

        //IEnumerator ShowFailedSyncMessage()
        //{
        //    calibrationMsg.text = "Sync Failed";
        //    calibrationMsg.enabled = true;
        //    yield return new WaitForSeconds(1.5f);
        //    calibrationMsg.enabled = false;
        //}

        //[Button]
        public void StartCalibrationCountdown()
        {
            //StartCoroutine(CalibrationCountdown());
            stabilizedFrames = 0;
            isCalibrating = true;
            UpdatePositionAndPose();
            isCalibrating = false;
            //calibrationMsg.enabled = false;
            isDynamicUpdateEnabled = 0;
            newIsDynamicUpdateEnabled = 0;
            StartCoroutine(PutObjectData());
            //resetButton.SetActive(false);
            isCalibrationModeEnabled = false;
            passthroughCtrl.TogglePassthrough(isCalibrationModeEnabled);
            StartCoroutine(ShowSyncMessage());
        }

        [Button]
        public bool ToggleCalibrationMode()
        {
            CheckPlayer();
            isOneShotNotDynamic = true;
            dynamicCalibrationMode = false;
            isCalibrationModeEnabled = !isCalibrationModeEnabled;
            passthroughCtrl.TogglePassthrough(isCalibrationModeEnabled);
            if (isCalibrationModeEnabled)
            {
                /*isDynamicUpdateEnabled = 1;
                //previousMonitorPosition = centerEye.transform.position + centerEye.transform.forward * (monitorDistanceFromUser - 0.2f);
                StartCoroutine(PutObjectData());*/
                isDynamicUpdateEnabled = 1;
                stabilizedFrames = 0;
            }
            else
            {
                isDynamicUpdateEnabled = 0;
            }
            newIsDynamicUpdateEnabled = isDynamicUpdateEnabled;
            StartCoroutine(PutObjectData());
            return isCalibrationModeEnabled;
        }

        [Button]
        public bool ToggleDynamicCalibrationMode()
        {
            CheckPlayer();
            isOneShotNotDynamic = false;
            //dynamicCalibrationMode = true;
            isCalibrationModeEnabled = !isCalibrationModeEnabled;
            //passthroughCtrl.TogglePassthroughShowRoom(isDynamicUpdateEnabled==0);
            if (isCalibrationModeEnabled)
            {
                /*isDynamicUpdateEnabled = 1;
                //previousMonitorPosition = centerEye.transform.position + centerEye.transform.forward * (monitorDistanceFromUser - 0.2f);
                StartCoroutine(PutObjectData());*/
                dynamicCalibrationMode = true;
                isDynamicUpdateEnabled = 1;
                stabilizedFrames = 0;

                
            }
            else
            {
                isDynamicUpdateEnabled = 0;
                dynamicCalibrationMode = false;
            }
            newIsDynamicUpdateEnabled = isDynamicUpdateEnabled;
            //passthroughCtrl.TogglePassthrough(dynamicCalibrationMode);
            StartCoroutine(PutObjectData());
            return dynamicCalibrationMode;
        }

        public void MoveCalibrationUI()
        {
            //CheckPlayer();
            //calibrationUI.transform.Rotate(0, 180, 0);
            //float monitorDistanceFromUser = (float.Parse(leftCoordinateString[2]) + float.Parse(rightCoordinateString[2])) / 2;
            
           /* currentMonitorPosition = centerEye.transform.position + centerEye.transform.forward * (monitorDistanceFromUser - 0.2f);
            if (Vector3.Distance(previousMonitorPosition, currentMonitorPosition) > monitorPosThres)
            {
                calibrationUI.transform.position = currentMonitorPosition;
            }
            previousMonitorPosition = currentMonitorPosition;*/
            calibrationUI.transform.LookAt(centerEye.transform);
        }

        [Button]
        public void UpdatePositionAndPose()
        {
            CheckPlayer();

            if (PeripheralName == "Keyboard")
            {
                // ================================= POSITION ======================================

                // ADJUSTED DEPTH 
                //* factorHeadTilt * signHeadTilt
                //float adjustedDistanceFromUser = (float)(distanceFromUser * Math.Cos(Math.Asin(lateralDistanceFromCenter / distanceFromUser)) + headTiltCorrection * factorHeadTilt * signHeadTilt);
                //float adjustedDistanceFromUser = (float)(distanceFromUser * Math.Cos(Math.Asin(lateralDistanceFromCenter / distanceFromUser)));// - headTiltCorrection;

                lidarCamera.transform.position = new Vector3(
                    mostPopularCenterEye.transform.position.x,
                    (float)(mostPopularCenterEye.transform.position.y), // + lidarOffset * Math.Cos(1.5708 - mostPopularCenterEye.transform.eulerAngles.x * Math.PI / 180)),
                    mostPopularCenterEye.transform.position.z);

                lidarCamera.transform.rotation = mostPopularCenterEye.transform.rotation;

                //CALCULATE POSITION
                Vector3 newPosition = centerEye.transform.TransformPoint(new Vector3(float.Parse(objectDetectedCoord[0]),
                    -float.Parse(objectDetectedCoord[1]),
                   float.Parse(objectDetectedCoord[2])));

                //Vector3 objVector = (rightPoint.transform.position - leftPoint.transform.position).normalized;
                //Vector3 objVectorNormal = new Vector3(objVector.z, 0, objVector.x);
                // CALCULATE POSITION 
                //newPosition = newPosition - objVectorNormal.normalized * (lateralDistanceFromCenter + shiftThreshold); // offset:  - 0.1f
                //transform.position = transform.position + objVector * lateralDistanceFromCenter;

                // ================================= END POSITION ======================================

                // =================================== POSE ========================================

                currentObjRotation = new Vector3(0, centerEye.transform.rotation.eulerAngles.y + objectAngle + 180, 0);

                // ================================= END POSE ======================================


                // TABLE Y-HEIGHT CORRECTION
                differenceThres = oldPosition - newPosition;
                diffRotation = currentObjRotation - oldRotation;
                //Debug.LogFormat("<color=cyan>" + differenceThres + "</color>");
                //Debug.LogFormat("<color=yellow>" + diffRotation + "</color>");
                //Debug.LogFormat("<color=yellow> distance: " + Math.Abs(monitorDistanceFromUser - oldMonitorDistanceFromUser) + "</color>");
                if (Math.Abs(differenceThres.x) > differenceThresValue || Math.Abs(differenceThres.z) > differenceThresValue || Math.Abs(diffRotation.y) % 360 > differenceRotThresValue * 100) //|| Math.Abs(monitorDistanceFromUser - oldMonitorDistanceFromUser) > 0.02f
                {

                    stabilizedFrames = 0;
                }
                else
                {
                    stabilizedFrames += 1;

                }

                Debug.LogFormat("<color=green> In Update script </color>");
                //transform.position = new Vector3(newPosition.x, monitorHeightOffset, newPosition.y); //monitorHeightOffset
                //transform.eulerAngles = new Vector3(0, 180+objectAngle, 0);

                if (!dynamicCalibrationMode && stabilizedFrames >= triggerStabilizedFrameNum && isDetectable) //&& objectZone == objectSTManager.GetCurrentZone()
                {

                    transform.position = new Vector3(newPosition.x, monitorHeightOffset, newPosition.z); //monitorHeightOffset
                    transform.eulerAngles = currentObjRotation;


                }

                else if (dynamicCalibrationMode && stabilizedFrames == triggerStabilizedFrameNum && isDetectable) //&& objectZone == objectSTManager.GetCurrentZone()
                {
                    diffDistanceFromZero = transform.position - newPosition; //zeroFramePosition - newPosition;
                    diffRotationFromZero = transform.rotation.eulerAngles - currentObjRotation; //zeroFrameRotation - currentObjRotation;

                    //if (Math.Abs(diffDistanceFromZero.x) >= differenceThresValue || 
                    //    Math.Abs(diffDistanceFromZero.z) >= differenceThresValue || 
                    //    Math.Abs(diffRotationFromZero.y) % 360 >= differenceRotThresValue * 100 )
                    if (OVRManager.display.velocity.sqrMagnitude < 0.05f &&
                        //centerEye.transform.eulerAngles.x > 35 &&
                        //centerEye.transform.eulerAngles.x < 90 &&
                        (Math.Abs(diffDistanceFromZero.x) >= differenceThresValue ||
                        Math.Abs(diffDistanceFromZero.z) >= differenceThresValue ||
                        Math.Abs(diffDistanceFromZero.x) <= maxThresValue ||
                        Math.Abs(diffDistanceFromZero.z) <= maxThresValue))
                    {
                        transform.position = new Vector3(newPosition.x, monitorHeightOffset, newPosition.z); //monitorHeightOffset
                        transform.eulerAngles = currentObjRotation;

                    }

                    //stabilizedFrames = 0;
                }

                // Set current position as old position
                oldPosition = newPosition;
                // Set current rotation as old rotation
                oldRotation = currentObjRotation;

                oldMonitorDistanceFromUser = this.distanceFromUser;



                if (stabilizedFrames == 0)
                {
                    dynamic_pos = transform.position;
                    dynamic_rot = transform.rotation;

                    // zero frame
                    zeroFramePosition = newPosition;
                    zeroFrameRotation = currentObjRotation;

                    StartCoroutine(PutObjectData());
                }
                //}

            }

            // ==================== MOUSE =================================
            else if (PeripheralName == "Mouse")
            {
                // ================================= POSITION ==========================================


                
                lidarCamera.transform.position = new Vector3(
                    mostPopularCenterEye.transform.position.x,
                    (float)(mostPopularCenterEye.transform.position.y), // + lidarOffset * Math.Cos(1.5708 - mostPopularCenterEye.transform.eulerAngles.x * Math.PI / 180)),
                    mostPopularCenterEye.transform.position.z);

                lidarCamera.transform.rotation = mostPopularCenterEye.transform.rotation;

                //CALCULATE POSITION
                Vector3 newPosition = centerEye.transform.TransformPoint(new Vector3(float.Parse(objectDetectedCoord[0]),
                    -float.Parse(objectDetectedCoord[1]),
                   float.Parse(objectDetectedCoord[2])));

                //Vector3 objVector = (rightPoint.transform.position - leftPoint.transform.position).normalized;
                //Vector3 objVectorNormal = new Vector3(objVector.z, 0, objVector.x);
                // CALCULATE POSITION 
                //newPosition = newPosition - objVectorNormal.normalized * (lateralDistanceFromCenter + shiftThreshold); // offset:  - 0.1f
                //transform.position = transform.position + objVector * lateralDistanceFromCenter;

                // ================================= END POSITION ======================================

                // =================================== POSE ========================================

                currentObjRotation = new Vector3(0, centerEye.transform.rotation.eulerAngles.y + objectAngle + 180, 0);

                // ================================= END POSE ======================================


                // TABLE Y-HEIGHT CORRECTION
                differenceThres = oldPosition - newPosition;
                diffRotation = currentObjRotation - oldRotation;
                //Debug.LogFormat("<color=cyan>" + differenceThres + "</color>");
                //Debug.LogFormat("<color=yellow>" + diffRotation + "</color>");
                //Debug.LogFormat("<color=yellow> distance: " + Math.Abs(monitorDistanceFromUser - oldMonitorDistanceFromUser) + "</color>");
                if (Math.Abs(differenceThres.x) > differenceThresValue || Math.Abs(differenceThres.z) > differenceThresValue || Math.Abs(diffRotation.y) % 360 > differenceRotThresValue * 100) //|| Math.Abs(monitorDistanceFromUser - oldMonitorDistanceFromUser) > 0.02f
                {

                    stabilizedFrames = 0;
                }
                else
                {
                    stabilizedFrames += 1;

                }

                //Debug.LogFormat("<color=green> In Update script </color>");
                //transform.position = new Vector3(newPosition.x, monitorHeightOffset, newPosition.y); //monitorHeightOffset
                //transform.eulerAngles = new Vector3(0, 180+objectAngle, 0);

                if (!dynamicCalibrationMode && stabilizedFrames >= triggerStabilizedFrameNum)
                {

                    transform.position = new Vector3(newPosition.x, monitorHeightOffset, newPosition.z); //monitorHeightOffset
                    transform.eulerAngles = currentObjRotation;


                }

                else if (dynamicCalibrationMode && stabilizedFrames == triggerStabilizedFrameNum)
                {
                    diffDistanceFromZero = transform.position - newPosition; //zeroFramePosition - newPosition;
                    diffRotationFromZero = transform.rotation.eulerAngles - currentObjRotation; //zeroFrameRotation - currentObjRotation;

                    //if (Math.Abs(diffDistanceFromZero.x) >= differenceThresValue || 
                    //    Math.Abs(diffDistanceFromZero.z) >= differenceThresValue || 
                    //    Math.Abs(diffRotationFromZero.y) % 360 >= differenceRotThresValue * 100 )
                    if (OVRManager.display.velocity.sqrMagnitude < 0.05f &&
                        //centerEye.transform.eulerAngles.x > 35 &&
                        //centerEye.transform.eulerAngles.x < 90 &&
                        (Math.Abs(diffDistanceFromZero.x) >= differenceThresValue ||
                        Math.Abs(diffDistanceFromZero.z) >= differenceThresValue ||
                        Math.Abs(diffDistanceFromZero.x) <= maxThresValue ||
                        Math.Abs(diffDistanceFromZero.z) <= maxThresValue))
                    {
                        transform.position = new Vector3(newPosition.x, monitorHeightOffset, newPosition.z); //monitorHeightOffset
                        transform.eulerAngles = currentObjRotation;

                    }

                    //stabilizedFrames = 0;
                }

                // Set current position as old position
                oldPosition = newPosition;
                // Set current rotation as old rotation
                oldRotation = currentObjRotation;

                oldMonitorDistanceFromUser = this.distanceFromUser;



                if (stabilizedFrames == 0)
                {
                    dynamic_pos = transform.position;
                    dynamic_rot = transform.rotation;

                    // zero frame
                    zeroFramePosition = newPosition;
                    zeroFrameRotation = currentObjRotation;

                    StartCoroutine(PutObjectData());
                }
                //}

            }

        } // end new update


        private void Start()
        {
            StartCoroutine(GetObjectData());
            // Updates monitor Position and pose to  the one used in previous session
            //UpdatePositionAndPose();

            // Find the objects in the scene
            //bottleObject = GameObject.Find("BottleGrabbable");
            //Current_Object = GameObject.Find("Coke_can");

            //get the HMD headset
            //centerEye = GameObject.Find("CenterEyeAnchor");
            //oldPosition = new Vector3(0.0f, 0.0f, 0.0f);
            //old_pos_x = 0.05f;

            //rotationAngle = transform.rotation.y;
            centerEye = GameObject.Find("CenterEyeAnchor");
            playerCamera = GameObject.Find("TrackingSpace");
            lidarAnchor = GameObject.Find("LiDARAnchor");

            oldPosition = transform.position;
            isDetectable = false;
            calibrationMsg.enabled = false;
            //photonView = FindObjectOfType<PhotonView>();
            isDynamicUpdateEnabled = 0;
            newIsDynamicUpdateEnabled = 0;
            StartCoroutine(PutObjectData());
            isCalibrationModeEnabled = false;
            isOneShotNotDynamic = false;

            if (transform.name.Contains("Mouse"))
                PeripheralName = "Mouse";

            else if (transform.name.Contains("Keyboard"))
                PeripheralName = "Keyboard";
            
        }

        private void Update()
        {

            // Get request on first load
            if (!callingAPI && !callingAPIPut)
                StartCoroutine(GetObjectData());

            //transform.eulerAngles = new Vector3(0, objectAngle, 0);

            Vector3 currentRotation = OVRManager.display.angularVelocity;
            //Debug.LogFormat("<color=magenta>Head Angular Velocity: "+currentRotation.sqrMagnitude+"</color>");

            //if (movementMagnitude < movementThreshold)
            //{
            //    // Head movement is below the threshold, do something here
            //    Debug.LogFormat("<color=magenta>Head movement is below threshold!</color>");
            //}

            // Toggle Reset button visibility if it is getting detected
            //if (isDetectable && !isCalibrating && isCalibrationModeEnabled) //&&isDyanmicUpdatedEnabled==0
            //    resetButton.SetActive(true);
            //else
            //    resetButton.SetActive(false);

            // Move Calibration UI in front of player if calibration mode is active
            if (isCalibrationModeEnabled)
                MoveCalibrationUI();

            if (isOneShotNotDynamic && !dynamicCalibrationMode && stabilizedFrames > triggerStabilizedFrameNum)
                StartCalibrationCountdown();

            //Debug.LogFormat("<color=yellow> player velocity: " + OVRManager.display.velocity + OVRManager.display.velocity.sqrMagnitude + "</color>");
            //if (centerEye.transform.eulerAngles.x > 35.0f &&
                        //centerEye.transform.eulerAngles.x < 90.0f )
            //Debug.LogFormat("<color=cyan>Is angle good? , " + (centerEye.transform.eulerAngles.x > 35 && centerEye.transform.eulerAngles.x < 90) + "</color>");

            //-------------------------------------
            // Create a new TransformData struct to store the current position and rotation.
            TransformData currentData = new TransformData
            {
                position = RoundVector3(centerEye.transform.position, precision),
                rotation = RoundVector3(centerEye.transform.rotation.eulerAngles, precision)
            };
            // Add the new data to the list.
            transformList.Add(currentData);

            // Keep the list size at maxListSize.
            if (transformList.Count > maxListSize)
            {
                transformList.RemoveAt(0);
            }

            CheckPlayer();
            // Get the most popular TransformData.
            TransformData mostPopular = GetMostPopularValue(transformList);
            mostPopularCenterEye.transform.position = mostPopular.position;
            mostPopularCenterEye.transform.rotation = Quaternion.Euler(mostPopular.rotation.x, mostPopular.rotation.y, mostPopular.rotation.z);
            mostPopularCenterEyePosition = mostPopular.position;
            mostPopularCenterEyeRotation = mostPopular.rotation;
            //Debug.Log("last 30 list: " + transformList);
            //Debug.Log("Most popular position: " + mostPopular.position);
            //Debug.Log("Most popular rotation: " + mostPopular.rotation);


            //-------------------------------------


            //Debug.LogFormat("<color=yellow>" + centerEye.transform.rotation.eulerAngles + "</color>");
            //Debug.LogFormat("<color=cyan>" + centerEye.transform.position + "</color>");
            //RotateTest();
            //Debug.Log("debug: "+leftCoordinateString);
            //Debug.Log("debug: "+rightCoordinateString);
            //Debug.Log("debug: "+centerEye);
        }

        Vector3 RoundVector3(Vector3 vector, int decimalPlaces)
        {
            return new Vector3(
                RoundValue(vector.x, decimalPlaces),
                RoundValue(vector.y, decimalPlaces),
                RoundValue(vector.z, decimalPlaces)
            );
        }

        float RoundValue(float value, int decimalPlaces)
        {
            float multiplier = Mathf.Pow(10f, decimalPlaces);
            return Mathf.Round(value * multiplier) / multiplier;
        }

        TransformData GetMostPopularValue(List<TransformData> list)
        {
            Dictionary<TransformData, int> valueCounts = new Dictionary<TransformData, int>();
            foreach (TransformData data in list)
            {
                if (valueCounts.ContainsKey(data))
                {
                    valueCounts[data]++;
                }
                else
                {
                    valueCounts[data] = 1;
                }
            }

            TransformData mostPopular = new TransformData();
            int maxCount = 0;
            foreach (var kvp in valueCounts)
            {
                if (kvp.Value > maxCount)
                {
                    mostPopular = kvp.Key;
                    maxCount = kvp.Value;
                }
            }

            return mostPopular;
        }


    }
}
