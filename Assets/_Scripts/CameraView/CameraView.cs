using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace MILab
{
    public class CameraView : MonoBehaviour
    {
        [SerializeField] GameObject titleBar;
        [SerializeField] GameObject cameraViewContent;

        bool isSendingCaptureData = false;

        public void ShowCameraView()
        {
            titleBar.SetActive(true);
            cameraViewContent.SetActive(true);
        }

        public void HideCameraView()
        {
            titleBar.SetActive(false);
            cameraViewContent.SetActive(false);
        }

        [Button]
        public void Capture()
        {
            if (!isSendingCaptureData)
                StartCoroutine(PutCamViewData("on"));
        }

        public void UpdateCameraZoom(float zoomValue)
        {
            //StartCoroutine(PutZoomData(zoomValue));
        }

        
        // Coroutine to make PUT request
        IEnumerator PutCamViewData(string switchStatus)
        {
            //isSendingCaptureData = true;

            String body = "";
            body = "{\"switch_status\":\"" + switchStatus + "\"}";
            byte[] body_data = System.Text.Encoding.UTF8.GetBytes(body);
            using (UnityWebRequest req = UnityWebRequest.Put("http://172.16.136.143:8080/Smart_Switch/CameraView", body_data))

            {
                req.SetRequestHeader("Content-Type", "application/json");
                yield return req.Send();
                while (!req.isDone)
                    yield return null;
                byte[] result = req.downloadHandler.data;
                //Debug.Log(System.Text.Encoding.Default.GetString(result));
                //string lampJSON = System.Text.Encoding.Default.GetString(result);
                //LampData info = JsonUtility.FromJson<LampData>(lampJSON);
            }
            Debug.LogFormat("<color=yellow> Sent Pinch Gesture detected event! </color>");
            //yield return new WaitForSeconds(1);
            //isSendingCaptureData = false;
        }

        // Coroutine to make PUT request
        IEnumerator PutZoomData(float zoomValue)
        {
            String body = "";
            body = "{\"zoom_value\":\"" + zoomValue + "\"}";
            byte[] body_data = System.Text.Encoding.UTF8.GetBytes(body);
            using (UnityWebRequest req = UnityWebRequest.Put("http://172.16.136.143:8080/Smart_Switch/CameraView", body_data))

            {
                req.SetRequestHeader("Content-Type", "application/json");
                yield return req.Send();
                while (!req.isDone)
                    yield return null;
                byte[] result = req.downloadHandler.data;
                //Debug.Log(System.Text.Encoding.Default.GetString(result));
                //string lampJSON = System.Text.Encoding.Default.GetString(result);
                //LampData info = JsonUtility.FromJson<LampData>(lampJSON);
            }
        }
    }
}