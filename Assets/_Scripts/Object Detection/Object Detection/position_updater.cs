using System.Collections;
using MILab;
using System.Collections.Generic;
using System.Net;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using OculusSampleFramework;
using System;

//gets the detected coordinates from database and updates the position of detected object in the unity scene
namespace MILab
{
    public class position_updater : MonoBehaviour
    {
        //For database
        private bool callingAPI = false;

        //for bottle object
        public string objectDetectedCoord;
        public string objectDetectedMaxCoord;
        public string objectDetectedMinCoord;

        public string objectPosition;
        public string objectRotation;
        public float offset_x; //0.47f
        public float offset_y; //0.927f
        public float offset_z; //1.31f
        public float min_y; //0.927f
        public float error_threshold; //0.05f


        //The objects we have in the scene
        public GameObject bottleObject;
        public GameObject Current_Object;

        //HMD gameobject
        public GameObject centerEye;
        public Vector3 oldPosition = new Vector3(0, 0, 0);
        private float sign;
        private float old_pos_x;

        void CheckCenterEye()
        {
            if (centerEye == null)
                centerEye = GameObject.Find("CenterEyeAnchor");
        }

        private void Start()
        {
            StartCoroutine(GetObjectData());

            // Find the objects in the scene
            bottleObject = GameObject.Find("BottleGrabbable");
            Current_Object = GameObject.Find("Coke_can");

            //get the HMD headset
            centerEye = GameObject.Find("CenterEyeAnchor");
            oldPosition = new Vector3(0.0f, 0.0f, 0.0f);
            //old_pos_x = 0.05f;

    }

    // Coroutine to make GET request
    IEnumerator GetObjectData()
        {
            using (UnityWebRequest request = UnityWebRequest.Get(String.Format("http://172.16.136.143:8080/Object_detection/Lamp")))
            {

                callingAPI = true;
                
                yield return request.Send();
                while (!request.isDone)
                    yield return null;
                Debug.Log(request.result);

                byte[] result = request.downloadHandler.data;
                string ObjectJSON = System.Text.Encoding.Default.GetString(result);
                ObjectData info = JsonUtility.FromJson<ObjectData>(ObjectJSON);

                //--Data we got from database--
                //info."updateKeyName"
                
                objectDetectedCoord = info.Coordinate;

                
                //Debug.Log(objectDetectedMaxCoord);
                //Debug.Log(objectDetectedMinCoord);
                //-----------------------------

                callingAPI = false;

            } //end of request

        } // end of GetObjectData

        void Update()
        {
            if (!callingAPI)
            {
                StartCoroutine(GetObjectData());
            }


            //
            //print(objectDetectedMaxCoord);
            //print(objectDetectedMinCoord);

            //convert from string to float
            String[] localMax = objectDetectedMaxCoord.Split(",");
            String[] localMin = objectDetectedMinCoord.Split(",");
            String[] objectCoord = objectDetectedCoord.Split(",");

            String[] splitObjectPosition = objectPosition.Split(",");
            String[] splitObjectRotation = objectRotation.Split(",");

            //old_pos_x = float.Parse(objectCoord[0]);

            //Vector3 localMaxVector = new Vector3(float.Parse(localMax[0]), float.Parse(localMax[1]), float.Parse(localMax[2]));
            //Vector3 localMinVector = new Vector3(float.Parse(localMin[0]), float.Parse(localMin[1]), float.Parse(localMin[2]));
            //Vector3 objectVector = new Vector3(Current_Object.transform.localPosition.x, Current_Object.transform.localPosition.y, float.Parse(objectCoord[0])); //z*4 removed
            Vector3 realObjectPosition;

            CheckCenterEye();
            // offset from camera
            /*offset_x = centerEye.transform.position.x;
            offset_y = centerEye.transform.position.y;
            offset_z = centerEye.transform.position.z;*/

            //flip axes
            if (-float.Parse(splitObjectPosition[1]) + offset_y < min_y)
            {
                realObjectPosition = new Vector3(-float.Parse(splitObjectPosition[2]) + offset_x, min_y, float.Parse(splitObjectPosition[0]) + offset_z);
            }
            else
            {
                realObjectPosition = new Vector3(-float.Parse(splitObjectPosition[2]) + offset_x, -float.Parse(splitObjectPosition[1]) + offset_y, float.Parse(splitObjectPosition[0]) + offset_z);
            }
            // global_x = (local_x - center_eye_x)*cos(angle_head) + (local_z - center_eye_z)*sin(angle_head)
            //Debug.Log(splitObjectPosition);/*
            //float angleHead = Vector3.Angle(centerEye.transform.position, new Vector3(1, 0, 0));
            //realObjectPosition = new Vector3((float.Parse(splitObjectPosition[0]) - offset_x)*(float)Math.Cos(angleHead) + (float.Parse(splitObjectPosition[2]) - offset_z) * (float)Math.Sin(angleHead), 
            //                                    float.Parse(splitObjectPosition[1]),//min_y,
            //                                   (float.Parse(splitObjectPosition[0]) - offset_x) * (float)Math.Sin(angleHead) + (float.Parse(splitObjectPosition[2]) - offset_z) * (float)Math.Cos(angleHead)); //360 - centerEye.transform.rotation.y*/

            //looking long wise
            //if (-float.Parse(splitObjectPosition[1]) + offset_y < min_y)
            //{
            //    realObjectPosition = new Vector3(-float.Parse(splitObjectPosition[2]) + offset_x, min_y, float.Parse(splitObjectPosition[0]) + offset_z);
            //}
            //else
            //{
            //    realObjectPosition = new Vector3(-float.Parse(splitObjectPosition[2]) + offset_x, -float.Parse(splitObjectPosition[1]) + offset_y, float.Parse(splitObjectPosition[0]) + offset_z);
            //}

            /*Vector3 realObjectPosition = new Vector3(-float.Parse(splitObjectPosition[2])+offset_x, -float.Parse(splitObjectPosition[1])+offset_y, float.Parse(splitObjectPosition[0])+offset_z); //CHANGED Y AND Z BY FLIPPING THEM/**/
            /* Vector3 realObjectRotation = new Vector3(float.Parse(splitObjectRotation[0]), float.Parse(splitObjectRotation[2]), float.Parse(splitObjectRotation[1]));*/

            //position of middle point of the bounding box (new position)
            /* Vector3 objectPositionFromCamera = (localMaxVector + localMinVector) / 2;*/

            //get the position of the HMD headset (not currently being used)
            /* Vector3 HMD_PositionFromOrigin = centerEye.transform.position;*/

            //add the position of the HMD to the position we got from the detection
            //Vector3 objectPositionFromOrigin = detectionPosition + HMD_PositionFromOrigin; //this will not work need coordinate translation

            //testing
            //Debug.Log("Before IF");
            //Debug.Log("old position" + oldPosition);
            //Debug.Log("new position" + objectVector);

            Debug.Log(old_pos_x);
            Debug.Log(float.Parse(objectCoord[0]));
            //lampObject.transform.localPosition = new Vector3(lampObject.transform.localPosition.x, lampObject.transform.localPosition.y, lampObject.transform.localPosition.z + float.Parse(objectCoord[0]) * 4);

            //if(float.Parse(objectCoord[0])>old_pos_x)
            //{
            //    sign = -1;

            //}
            //else
            //{
            //    sign = 1;
            //}
            //if (float.Parse(objectCoord[0]) < 0)
            //    sign *= -1;

            //old_pos_x = float.Parse(objectCoord[0]);

            //objectVector

            print("outside");
            print(realObjectPosition);

            //if (!oldPosition.Equals(realObjectPosition))
            //{
            //update the position of the object that was detected
            //lampObject.transform.localPosition = new Vector3(lampObject.transform.localPosition.x, lampObject.transform.localPosition.y, lampObject.transform.localPosition.z + (float.Parse(objectCoord[0]) * 4 * sign));
            //oldPosition = objectVector;


            if (Math.Abs(oldPosition[0] - realObjectPosition[0]) > error_threshold || Math.Abs(oldPosition[1] - realObjectPosition[1]) > error_threshold || Math.Abs(oldPosition[2] - realObjectPosition[2]) > error_threshold) //
            {
                print("inside");
                    print(realObjectPosition);

                    Current_Object.transform.localPosition = realObjectPosition;
                    Current_Object.transform.localRotation = new Quaternion(0, 0, 0, 1);
                   oldPosition = realObjectPosition;

            }

            //}



            //distance from headset at R angle


        } //end of update func

    } //end of class

}