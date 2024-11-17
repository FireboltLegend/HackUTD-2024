using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Photon.Pun;
using TMPro;

namespace MILab
{

    public class NewPositionUpdater : MonoBehaviour
    {


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
        [SerializeField, ReadOnly] string objectDetectedCoord;
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
        [SerializeField] GameObject leftPoint;
        [SerializeField] GameObject rightPoint;
        [SerializeField] GameObject resetButton;
        [SerializeField] float monitorHeightOffset = 0.717f;//0.7370126f; //0.705f; //0.2595f; // adding this height offset to lower monitor due to monitor model center
        [SerializeField] float factorHeadTilt = 1;
        [SerializeField] int signHeadTilt = 1;

        [SerializeField] float lidarOffset = 0.12f;
        [SerializeField] GameObject lidarCamera;

        bool isCalibrating;

        String[] leftCoordinateString;
        String[] rightCoordinateString;
        String[] networkPos;
        String[] networkRot;
        [SerializeField, ReadOnly] bool isDetectable;
        [SerializeField, ReadOnly] int isDynamicUpdateEnabled;
        int newIsDynamicUpdateEnabled;
        
        // CALIBRATION UI VARIABLES
        public TextMeshPro calibrationMsg;
        public float countdownDuration = 1f;
        [SerializeField] GameObject calibrationUI;
        [SerializeField, ReadOnly] float monitorDistanceFromUser;
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
        }

        public void setCallingApiActive(bool setValue)
        {
            callingAPI = setValue;
        }

        // Coroutine to make GET request
        IEnumerator GetObjectData()
        {
            using (UnityWebRequest request = UnityWebRequest.Get(String.Format("http://172.16.136.143:8080/Object_detection/Monitor")))
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
               
                objectDetectedCoord = info.Coordinate;

                
                //Debug.Log(objectDetectedMaxCoord);
                //Debug.Log(objectDetectedMinCoord);
                //-----------------------------
                leftCoordinateString = info.LeftCoordinate.Split(",");
                rightCoordinateString = info.RightCoordinate.Split(",");
                monitorDistanceFromUser = (float.Parse(leftCoordinateString[2]) + float.Parse(rightCoordinateString[2])) / 2;

                isDetectable = info.IsDetected;
                //isDynamicUpdateEnabled = info.IsDynamicUpdateEnabled;
                //Debug.LogFormat("<color=cyan>" + info.IsDynamicUpdateEnabled + "</color>");
                //Debug.LogFormat("<color=green>" + info.LeftCoordinate + "</color>");
                //Debug.LogFormat("<color=green>" + info.RightCoordinate + "</color>");
                //Debug.LogFormat("<color=green>" + float.Parse(leftCoordinateString[2]) * (float)Math.Sin(0.349066f) + "</color>");
                //Debug.LogFormat("<color=blue>" + isDetectable + "</color>");
                UpdateLeftAndRightPointCoordinates();
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
            byte[] myData = System.Text.Encoding.UTF8.GetBytes("{IsDynamicUpdateEnabled: " + newIsDynamicUpdateEnabled + "}");
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
            using (UnityWebRequest req = UnityWebRequest.Put("http://172.16.136.143:8080/Object_detection/Monitor", body_data))
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

        void UpdateLeftAndRightPointCoordinates()
        {
            CheckPlayer();
            
            // OLD - USES LEFT AND RIGHT COORDINATES GIVEN BY PI, DOESN'T TAKE SIDE HEAD TILT INTO ACCOUNT
            /*leftPoint.transform.position = new Vector3(centerEye.transform.position.x + float.Parse(leftCoordinateString[0]),
                0.715f,
                centerEye.transform.position.z + float.Parse(leftCoordinateString[2]) * (float)Math.Cos(centerEye.transform.eulerAngles.x * Math.PI / 180)); //0.349066f
            rightPoint.transform.position = new Vector3(centerEye.transform.position.x + float.Parse(rightCoordinateString[0]),
                0.715f,
                centerEye.transform.position.z + float.Parse(rightCoordinateString[2]));*/ // * (float)Math.Cos(centerEye.transform.eulerAngles.x * Math.PI / 180)); //0.349066f //WE HAVE TO ADD THE ANGLE OF THE CAMERA!

            // NEW - CALCULATES LEFT AND RIGHT COORDINATES FROM MIDPOINT, TAKES SIDE HEAD TILT INTO ACCOUNT
            
            // Midpoint of the left and right points which will give us rough monitor position from the player
            float midpointY = (float.Parse(leftCoordinateString[1]) + float.Parse(rightCoordinateString[1])) / 2;


            //if ((float)Math.Cos(mostPopularCenterEye.transform.eulerAngles.x * Math.PI / 180) > 0f)
            //midpointY = -midpointY;

            /*leftPoint.transform.position = new Vector3(mostPopularCenterEye.transform.position.x + (float.Parse(leftCoordinateString[0]) / (float)Math.Cos(mostPopularCenterEye.transform.eulerAngles.z * Math.PI / 180)),
                mostPopularCenterEye.transform.position.y - midpointY,
                mostPopularCenterEye.transform.position.z + (float.Parse(leftCoordinateString[2])) * (float)Math.Cos(mostPopularCenterEye.transform.eulerAngles.x * Math.PI / 180)); //0.349066f

            rightPoint.transform.position = new Vector3(mostPopularCenterEye.transform.position.x + (float.Parse(rightCoordinateString[0]) / (float)Math.Cos(mostPopularCenterEye.transform.eulerAngles.z * Math.PI / 180)),
                mostPopularCenterEye.transform.position.y - midpointY,
                mostPopularCenterEye.transform.position.z + (float.Parse(rightCoordinateString[2])) * (float)Math.Cos(mostPopularCenterEye.transform.eulerAngles.x * Math.PI / 180)); //0.349066f*/

            leftPoint.transform.position = centerEye.transform.TransformPoint(new Vector3(float.Parse(leftCoordinateString[0]),
                -float.Parse(leftCoordinateString[1]),
               float.Parse(leftCoordinateString[2]) ));

            rightPoint.transform.position = centerEye.transform.TransformPoint(new Vector3(float.Parse(rightCoordinateString[0]),
                -float.Parse(rightCoordinateString[1]),
               float.Parse(rightCoordinateString[2]) )); //+ 0.02f

            //Debug.LogFormat("<color=yellow>" + leftCoordinateString[0] + "</color>"); 
            //Debug.LogFormat("<color=cyan>" + leftPoint.transform.position + "</color>");
            //Debug.LogFormat("<color=yellow>" + centerEye.transform.eulerAngles.x + "</color>");
        } //end of UpdateLeftAndRightPointCoordinates

        //[Button]
        //void RotateTest()
        //{
        //    if (rotationAngle != transform.rotation.y)
        //        transform.rotation = Quaternion.Euler(0, rotationAngle, 0);
        //    //transform.Rotate(0, 5, 0);
        //} // end of RotateTest

        

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
            resetButton.SetActive(false);
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
            //passthroughCtrl.TogglePassthrough(isCalibrationModeEnabled);
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

        public void UpdatePositionAndPose()
        {
            CheckPlayer();
                        
            // ======================================= POSE ========================================

            /*Vector3 currentObjRotation = transform.localEulerAngles; //transform.eulerAngles;
            // PERFORM LOOK-AT
            Quaternion tempRot = transform.rotation;
            transform.LookAt(mostPopularCenterEye.transform);
            currentObjRotation = transform.localEulerAngles; //transform.eulerAngles;
            transform.rotation = tempRot;
            //currentObjRotation.x = 0;
            //currentObjRotation.z = 0;

            // PERFORM ROTATION
            if (float.Parse(rightCoordinateString[2]) > float.Parse(leftCoordinateString[2]))
            {
                //tempRot = transform.rotation;
                //transform.LookAt(mostPopularCenterEye.transform);
                //Vector3 currentObjRotation2 = currentObjRotation;
                //transform.rotation = tempRot;
                currentObjRotation.x = 0;
                currentObjRotation.z = 0;
                currentObjRotation.y = (float)(currentObjRotation.y - Math.Atan((float.Parse(rightCoordinateString[2]) - float.Parse(leftCoordinateString[2])) / (Math.Abs(float.Parse(rightCoordinateString[0]) - float.Parse(leftCoordinateString[0])))) * 180.0f / Math.PI);
                //currentObjRotation = currentObjRotation;
            }

            else
            {
                //tempRot = transform.rotation;
                //transform.LookAt(mostPopularCenterEye.transform);
                //Vector3 currentObjRotation2 = currentObjRotation;
                //transform.rotation = tempRot;
                currentObjRotation.x = 0;
                currentObjRotation.z = 0;
                currentObjRotation.y = (float)(currentObjRotation.y + Math.Atan((float.Parse(leftCoordinateString[2]) - float.Parse(rightCoordinateString[2])) / (Math.Abs(float.Parse(rightCoordinateString[0]) - float.Parse(leftCoordinateString[0])))) * 180.0f / Math.PI);
                //currentObjRotation = currentObjRotation2;

            }

            // ROTATION CORRECTION
            //float correctionAngle = 0; // Vector3.SignedAngle(transform.position - centerEye.transform.position, centerEye.transform.forward, Vector3.up);

            //currentObjRotation.y += correctionAngle + 180; ////Monitor's backside is its front in the prefab so we need to flip it by 180 degrees
            */

            //----------------------------------------------------------

            // =================================== END POSE ========================================

            // ================================= POSITION ==========================================

            // DEPTH
            float distanceFromUser = (float.Parse(leftCoordinateString[2]) + float.Parse(rightCoordinateString[2])) / 2;

            // DISTANCE THRESHOLD
            //if (distanceFromUser < 0.1f || distanceFromUser > 3.0f)
            //    return;

            // LATERAL
            float lateralDistanceFromCenter = (float.Parse(leftCoordinateString[0]) + float.Parse(rightCoordinateString[0])) / 2;

            float headTiltCorrection = 0;// (float.Parse(leftCoordinateString[1]) + float.Parse(rightCoordinateString[1])) / 2;

            // ADJUSTED DEPTH //* factorHeadTilt * signHeadTilt
            //float adjustedDistanceFromUser = (float)(distanceFromUser * Math.Cos(Math.Asin(lateralDistanceFromCenter / distanceFromUser)) + headTiltCorrection * factorHeadTilt * signHeadTilt);
            float adjustedDistanceFromUser = (float)(distanceFromUser * Math.Cos(Math.Asin(lateralDistanceFromCenter / distanceFromUser)));// - headTiltCorrection;

            lidarCamera.transform.position = new Vector3(
                mostPopularCenterEye.transform.position.x,
                (float)(mostPopularCenterEye.transform.position.y), // + lidarOffset * Math.Cos(1.5708 - mostPopularCenterEye.transform.eulerAngles.x * Math.PI / 180)),
                mostPopularCenterEye.transform.position.z);

            lidarCamera.transform.rotation = mostPopularCenterEye.transform.rotation;

            // Midpoint of the left and right points which will give us rough monitor position from the player
            Vector3 midpoint = (leftPoint.transform.position + rightPoint.transform.position) / 2;

            //Vector3 newPosition = mostPopularCenterEye.transform.position + mostPopularCenterEye.transform.rotation * mostPopularCenterEye.transform.forward * (distanceFromUser + headTiltCorrection);
            //CALCULATE POSITION
            Vector3 newPosition = midpoint;

            //Vector3 objVector = (rightPoint.transform.position - leftPoint.transform.position).normalized;
            //Vector3 objVectorNormal = new Vector3(objVector.z, 0, objVector.x);
            // CALCULATE POSITION 
            //newPosition = newPosition - objVectorNormal.normalized * (lateralDistanceFromCenter + shiftThreshold); // offset:  - 0.1f
            //transform.position = transform.position + objVector * lateralDistanceFromCenter;

            // ================================= END POSITION ======================================

            // =================================== NEW POSE ========================================

            Vector3 leftRightPointsVector = rightPoint.transform.position - leftPoint.transform.position;
            Vector3 currentObjRotation = Quaternion.LookRotation(leftRightPointsVector, Vector3.up).eulerAngles;
            currentObjRotation.y -= 90.0f;

            // ================================= END NEW POSE ======================================


            // TABLE Y-HEIGHT CORRECTION
            differenceThres = oldPosition - newPosition;
            diffRotation = oldRotation - currentObjRotation;
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
                    (Math.Abs(diffDistanceFromZero.x) >= differenceThresValue || 
                    Math.Abs(diffDistanceFromZero.z) >= differenceThresValue || 
                    Math.Abs(diffRotationFromZero.y) % 360 >= differenceRotThresValue * 100) )
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

            oldMonitorDistanceFromUser = monitorDistanceFromUser;



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


        } // end new update


        // This function finds position of 2 points on the monitor from Realsense camera and then
        // moves and aligns the monitor accordingly.
        public void ClassicUpdatePositionAndPose()
        {
            CheckPlayer();
            //GetObjectData();
            //if (!callingAPI)
            //StartCoroutine(GetObjectData());
            //UpdateLeftAndRightPointCoordinates();

            // Midpoint of the left and right points which will give us rough monitor position from the player
            Vector3 midpoint = (leftPoint.transform.position + rightPoint.transform.position) / 2;

            // PHOTON TAKE OWNERSHIP
            //if (!photonView.IsMine)
            //    photonView.TransferOwnership(PhotonNetwork.LocalPlayer);

            //Debug.LogFormat("<color=cyan>" + centerEye.transform.position + "</color>");
            //float correctedMidpointX = (float)((midpoint.x - centerEye.transform.position.x) * Math.Cos(centerEye.transform.rotation.y * Math.PI / 180.0f)) +
            //            (float)((midpoint.z - centerEye.transform.position.z) * Math.Sin(centerEye.transform.rotation.y * Math.PI / 180.0f));
            //float correctedMidpointZ = -(float)((midpoint.x - centerEye.transform.position.x) * Math.Sin(centerEye.transform.rotation.y * Math.PI / 180.0f)) +
            //            (float)((midpoint.z - centerEye.transform.position.z) * Math.Cos(centerEye.transform.rotation.y * Math.PI / 180.0f));
            //float correctedMidpointX = centerEye.transform.position.x + (float)((midpoint.x - centerEye.transform.position.x) * Math.Cos(centerEye.transform.rotation.y * Math.PI / 180.0f)) -
            //            (float)((midpoint.z - centerEye.transform.position.z) * Math.Sin(centerEye.transform.rotation.y * Math.PI / 180.0f));
            //float correctedMidpointZ = centerEye.transform.position.z + (float)((midpoint.x - centerEye.transform.position.x) * Math.Sin(centerEye.transform.rotation.y * Math.PI / 180.0f)) +
            //            (float)((midpoint.z - centerEye.transform.position.z) * Math.Cos(centerEye.transform.rotation.y * Math.PI / 180.0f));
            //transform.position = new Vector3(correctedMidpointX, midpoint.y, correctedMidpointZ);

            //transform.position = Camera.main.transform.TransformPoint(new Vector3(0,0,1));
            //float distanceFromUser = (float.Parse(leftCoordinateString[2]) + float.Parse(rightCoordinateString[2])) / 2;
            //transform.position = new Vector3(transform.position.x, midpoint.y, transform.position.z);

            // ================================= POSITION ==========================================

            // DEPTH
            float distanceFromUser = (float.Parse(leftCoordinateString[2]) + float.Parse(rightCoordinateString[2])) / 2;

            // DISTANCE THRESHOLD
            //if (distanceFromUser < 0.1f || distanceFromUser > 3.0f)
            //    return;

            // LATERAL
            float lateralDistanceFromCenter = (float.Parse(leftCoordinateString[0]) + float.Parse(rightCoordinateString[0])) / 2;

            float headTiltCorrection = 0;// (float.Parse(leftCoordinateString[1]) + float.Parse(rightCoordinateString[1])) / 2;

            //if (mostPopularCenterEye.transform.eulerAngles.x > 45.0f)
             //   return;
            //if (mostPopularCenterEye.transform.eulerAngles.x < 270.0f && mostPopularCenterEye.transform.eulerAngles.x > 0.0f)
            //    headTiltCorrection = (float)(1f * (monitorHeightOffset - midpoint.y) / Math.Tan(mostPopularCenterEye.transform.eulerAngles.x * Math.PI / 180));
            //    headTiltCorrection += (mostPopularCenterEye.transform.eulerAngles.x - 10) * 0.013f;
            //else
            //    headTiltCorrection += (mostPopularCenterEye.transform.eulerAngles.x - 350) * 0.013f;

                // ADJUSTED DEPTH //* factorHeadTilt * signHeadTilt
                //float adjustedDistanceFromUser = (float)(distanceFromUser * Math.Cos(Math.Asin(lateralDistanceFromCenter / distanceFromUser)) + headTiltCorrection * factorHeadTilt * signHeadTilt);
                float adjustedDistanceFromUser = (float)(distanceFromUser * Math.Cos(Math.Asin(lateralDistanceFromCenter / distanceFromUser)));// - headTiltCorrection;
            /*            if ( -0.2f <= headTiltCorrection && headTiltCorrection <= 0.2f)
                        {
                            perfectValue = adjustedDistanceFromUser;
                        }*/

            lidarCamera.transform.position = new Vector3(
                mostPopularCenterEye.transform.position.x,
                (float)(mostPopularCenterEye.transform.position.y), // + lidarOffset * Math.Cos(1.5708 - mostPopularCenterEye.transform.eulerAngles.x * Math.PI / 180)),
                mostPopularCenterEye.transform.position.z);

            lidarCamera.transform.rotation = mostPopularCenterEye.transform.rotation;

            //Vector3 newPosition = mostPopularCenterEye.transform.position + mostPopularCenterEye.transform.forward * distanceFromUser; //adjustedDistanceFromUser; //distanceFromUser; //adjustedDistanceFromUser
            //Vector3 newPosition = lidarCamera.transform.position + lidarCamera.transform.forward * (distanceFromUser + headTiltCorrection); //adjustedDistanceFromUser;
            //Vector3 vec = new Vector3();
            //Vector3 newPosition = mostPopularCenterEye.transform.position + midpoint;

            //newPosition.z = mostPopularCenterEye.transform.position.z + (float)((newPosition.z - mostPopularCenterEye.transform.position.z) * Math.Cos(mostPopularCenterEye.transform.eulerAngles.x * Math.PI / 180)
            //                        - (newPosition.y - mostPopularCenterEye.transform.position.y) * Math.Sin(mostPopularCenterEye.transform.eulerAngles.x * Math.PI / 180));

            //newPosition.y = mostPopularCenterEye.transform.position.y + (float)((newPosition.z - mostPopularCenterEye.transform.position.z) * Math.Sin(mostPopularCenterEye.transform.eulerAngles.x * Math.PI / 180)
            //                        + (newPosition.y - mostPopularCenterEye.transform.position.y) * Math.Cos(mostPopularCenterEye.transform.eulerAngles.x * Math.PI / 180));

            Vector3 newPosition = mostPopularCenterEye.transform.position + mostPopularCenterEye.transform.forward * (distanceFromUser + headTiltCorrection);

            //if (Vector3.Distance(oldPosition, newPosition) < shiftThreshold)
            //    return;
            //transform.position = newPosition;

            //transform.rotation = Quaternion.identity;
            //transform.LookAt(centerEye.transform);
            //Vector3 currentObjRotation = transform.eulerAngles;
            //currentObjRotation.x = 0;
            //currentObjRotation.z = 0;
            //currentObjRotation.y = 90;
            //transform.eulerAngles = currentObjRotation;
            //transform.Rotate(0, 180, 0);


            Vector3 objVector = (rightPoint.transform.position - leftPoint.transform.position).normalized;
            Vector3 objVectorNormal = new Vector3(-objVector.z, 0, objVector.x);
            // CALCULATE POSITION 
            newPosition = newPosition - objVectorNormal.normalized * (lateralDistanceFromCenter + shiftThreshold); // offset:  - 0.1f
            //transform.position = transform.position + objVector * lateralDistanceFromCenter;
            
            // Set current position as old position
            //oldPosition = newPosition;

            //currentObjRotation = transform.eulerAngles;
            //currentObjRotation.x = 0;
            //currentObjRotation.z = 0;
            //currentObjRotation.y = 90;
            //transform.eulerAngles = currentObjRotation;

            // ================================= END POSITION ======================================

            //Vector3 direction = (rightPoint.transform.position - leftPoint.transform.position).normalized;
            //if (centerEye.transform.rotation.y > 90 && centerEye.transform.rotation.y < 270)
            //    transform.rotation = Quaternion.LookRotation(direction);
            //else
            //    transform.rotation = Quaternion.LookRotation(direction);

            // ======================================= POSE ========================================

            Vector3 currentObjRotation = transform.eulerAngles;
            // PERFORM LOOK-AT
            Quaternion tempRot = transform.rotation;
            transform.LookAt(mostPopularCenterEye.transform);
            currentObjRotation = transform.eulerAngles;
            transform.rotation = tempRot;
            currentObjRotation.x = 0;
            currentObjRotation.z = 0;
            //transform.eulerAngles = currentObjRotation;

            
            // PERFORM ROTATION
            if (float.Parse(rightCoordinateString[2]) > float.Parse(leftCoordinateString[2]))
            {
                //Vector3 direction = (rightPoint.transform.position - leftPoint.transform.position).normalized;
                //transform.rotation = Quaternion.LookRotation(direction);
                tempRot = transform.rotation;
                transform.LookAt(mostPopularCenterEye.transform);
                Vector3 currentObjRotation2 = currentObjRotation;
                transform.rotation = tempRot;
                currentObjRotation2.x = 0;
                currentObjRotation2.z = 0;
                //Debug.Log("Right>left");
                //Debug.LogFormat("<color=cyan>" + currentObjRotation.y + "</color>");
                //Debug.LogFormat("<color=yellow>" + float.Parse(leftCoordinateString[2]) + " , " + float.Parse(rightCoordinateString[2]) + " : " + (float.Parse(leftCoordinateString[2]) - float.Parse(rightCoordinateString[2])) + " , " + (Math.Abs(float.Parse(rightCoordinateString[0]) - float.Parse(leftCoordinateString[0]))) + "</color>");
                //Debug.LogFormat("<color=red>" + (Math.Atan((float.Parse(rightCoordinateString[2]) - float.Parse(leftCoordinateString[2])) * (float)Math.Sin(0.349066f) / (Math.Abs(float.Parse(rightCoordinateString[0]) - float.Parse(leftCoordinateString[0]))) ) * 180.0f / Math.PI) + "</color>");
                currentObjRotation2.y = (float)(currentObjRotation2.y - Math.Atan((float.Parse(rightCoordinateString[2]) - float.Parse(leftCoordinateString[2])) / (Math.Abs(float.Parse(rightCoordinateString[0]) - float.Parse(leftCoordinateString[0])))) * 180.0f / Math.PI);
                currentObjRotation = currentObjRotation2;
                //Debug.LogFormat("<color=yellow>" + currentObjRotation2.y + "</color>");
                //Debug.LogFormat("<color=cyan>" + Vector3.Distance(transform.position, centerEye.transform.position) + "</color>");
            }

            else
            {
                //Vector3 direction = (leftPoint.transform.position - rightPoint.transform.position).normalized;
                //transform.rotation = Quaternion.LookRotation(direction);
                tempRot = transform.rotation;
                transform.LookAt(mostPopularCenterEye.transform);
                Vector3 currentObjRotation2 = currentObjRotation;
                transform.rotation = tempRot;
                currentObjRotation2.x = 0;
                currentObjRotation2.z = 0;
                //Debug.Log("Left>right");
                //Debug.LogFormat("<color=cyan>" + currentObjRotation.y + "</color>");
                //Debug.LogFormat("<color=yellow>" + float.Parse(leftCoordinateString[2]) + " , " + float.Parse(rightCoordinateString[2]) + " : " + (float.Parse(leftCoordinateString[2]) - float.Parse(rightCoordinateString[2])) + " , " + (Math.Abs(float.Parse(rightCoordinateString[0]) - float.Parse(leftCoordinateString[0]))) + "</color>");
                //Debug.LogFormat("<color=red>" + (Math.Atan((float.Parse(leftCoordinateString[2]) - float.Parse(rightCoordinateString[2])) * (float)Math.Sin(0.349066f) / (Math.Abs(float.Parse(rightCoordinateString[0]) - float.Parse(leftCoordinateString[0]))) ) * 180.0f / Math.PI) + "</color>");
                currentObjRotation2.y = (float)(currentObjRotation2.y + Math.Atan((float.Parse(leftCoordinateString[2]) - float.Parse(rightCoordinateString[2])) / (Math.Abs(float.Parse(rightCoordinateString[0]) - float.Parse(leftCoordinateString[0])))) * 180.0f / Math.PI);
                currentObjRotation = currentObjRotation2;
                //Debug.LogFormat("<color=yellow>" + currentObjRotation2.y + "</color>");
                //Debug.LogFormat("<color=cyan>" + Vector3.Distance(transform.position, centerEye.transform.position) + "</color>");
            }

            // ROTATION CORRECTION
            float correctionAngle = Vector3.SignedAngle(newPosition - centerEye.transform.position, centerEye.transform.forward, Vector3.up);
            //Debug.LogFormat("<color=yellow>" + correctionAngle + "</color>");
            //transform.Rotate(0, correctionAngle, 0);
            currentObjRotation.y += correctionAngle + 180; ////Monitor's backside is its front in the prefab so we need to flip it by 180 degrees

            //-------------------------------------------------------
            //Making a monitor viewing angle bounds in z and x rotations
            //if (transform.eulerAngles.z <= 180 && transform.eulerAngles.z >= 270 && centerEye.transform.rotation.eulerAngles.x > -50)
            //{
            //    isInGoodView = true;
            //}

            //----------------------------------------------------------

            // =================================== END POSE ========================================

            // TABLE Y-HEIGHT CORRECTION
            //transform.position = new Vector3(transform.position.x, 0.7329732f, transform.position.z); //Normal table
            differenceThres = oldPosition - newPosition;
            diffRotation = oldRotation - currentObjRotation;
            //Debug.LogFormat("<color=cyan>" + differenceThres + "</color>");
            //Debug.LogFormat("<color=yellow>" + diffRotation + "</color>");
            if (Math.Abs(differenceThres.x) > differenceThresValue || Math.Abs(differenceThres.z) > differenceThresValue || Math.Abs(diffRotation.y) % 360 > differenceRotThresValue * 100)
            {
                //if (stabilizedFrames >= triggerStabilizedFrameNum-2) 
                //{ 

                //    if (Math.Abs(differenceThres.x) > differenceThresValue || Math.Abs(differenceThres.z) > differenceThresValue)
                //    {
                //        transform.position = new Vector3(newPosition.x, monitorHeightOffset, newPosition.z); //monitorHeightOffset

                //    }
                //    if (Math.Abs(diffRotation.y) % 360 > differenceRotThresValue * 100)
                //    {
                //        transform.eulerAngles = currentObjRotation;
                //    }
                //}
                stabilizedFrames = 0;
            }
            else
            {
                stabilizedFrames += 1;
                //if (isStabilized)
                //{
                //    isDynamicUpdateEnabled = 0;
                //    //StartCalibrationCountdown(); Since UpdatePositionAndPose is called inside a Coroutine basically stalls the whole program. Call this in Update
                //}
                //else
                //    isStabilized = true;
            }

            if (!dynamicCalibrationMode && stabilizedFrames >= triggerStabilizedFrameNum)
            {
                transform.position = new Vector3(newPosition.x, monitorHeightOffset, newPosition.z); //monitorHeightOffset
                transform.eulerAngles = currentObjRotation;

                //if (Math.Abs(differenceThres.x) > differenceThresValue || Math.Abs(differenceThres.z) > differenceThresValue)
                //{
                //    transform.position = new Vector3(newPosition.x, monitorHeightOffset, newPosition.z); //monitorHeightOffset

                //}
                //if (Math.Abs(diffRotation.y) % 360 > differenceRotThresValue * 100)
                //{
                //    transform.eulerAngles = currentObjRotation;
                //}
            }

            else if (dynamicCalibrationMode && stabilizedFrames == triggerStabilizedFrameNum)
            {
                transform.position = new Vector3(newPosition.x, monitorHeightOffset, newPosition.z); //monitorHeightOffset
                transform.eulerAngles = currentObjRotation;
            }

            // Set current position as old position
            oldPosition = newPosition;
            // Set current rotation as old rotation
            oldRotation = currentObjRotation;


            //transform.Rotate(0, 180, 0); //Monitor's backside is its front in the prefab so we need to flip it
            //transform.rotation = Quaternion.LookRotation(-direction);
            //Debug.LogFormat("<color=cyan>" + (centerEye.transform.rotation.y - 90) + "</color>");
            //Vector3 currentObjRotation = transform.eulerAngles;
            //currentObjRotation.y = currentObjRotation.y + centerEye.transform.eulerAngles.y - 90;
            //transform.eulerAngles = currentObjRotation;
            //differenceThres = dynamic_pos - transform.position;
            //Debug.LogFormat("<color=yellow>" + (differenceThres) + "</color>");
            //if (Math.Abs(differenceThres.x) > differenceThresValue || Math.Abs(differenceThres.z) > differenceThresValue)
            //{
            if (stabilizedFrames==0 || dynamicCalibrationMode)
            {
                dynamic_pos = transform.position;
                dynamic_rot = transform.rotation;
                StartCoroutine(PutObjectData());
            }
            //}
            

        } //end of UpdatePositionAndPose

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

            oldPosition = transform.position;
            isDetectable = false;
            calibrationMsg.enabled = false;
            //photonView = FindObjectOfType<PhotonView>();
            isDynamicUpdateEnabled = 0;
            newIsDynamicUpdateEnabled = 0;
            StartCoroutine(PutObjectData());
            isCalibrationModeEnabled = false;

            isOneShotNotDynamic = false;
        }

        

        private void Update()
        {
            // Get request on first load
            if (!callingAPI && !callingAPIPut && isCalibrationModeEnabled)
                StartCoroutine(GetObjectData());

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
            //Debug.LogFormat("<color=yellow>" + (mostPopularCenterEye.transform.eulerAngles.x%360) + "</color>");

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
