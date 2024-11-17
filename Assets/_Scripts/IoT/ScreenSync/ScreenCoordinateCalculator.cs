using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Input;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using MILab;
using TMPro;
using System;

namespace MILab
{
    public class ScreenCoordinateCalculator : MonoBehaviour, IMixedRealityPointerHandler
    {
        [SerializeField]
        public GameObject quadObject;
        public InputSourceType sourceType = InputSourceType.Hand;
        private Transform quadTransform;

        private bool callingAPI = false;

        private float cursor_x, cursor_y;
        private bool isDown;
        private float offset = 0.5f;

        public void OnPointerClicked(MixedRealityPointerEventData eventData)
        {

        }

        public void OnPointerDown(MixedRealityPointerEventData eventData)
        {
            if (eventData.InputSource.SourceType == sourceType)
            {
                var result = eventData.Pointer.Result;
                if (result != null)
                {
                    isDown = true;
                    Vector3 hitPoint = result.Details.Point;
                    Vector3 clickRelative = quadTransform.InverseTransformPoint(hitPoint);
                    cursor_x = clickRelative.x + offset;
                    cursor_y = clickRelative.y + offset;
                    Debug.LogFormat("<color=blue> Cursor Position Drag: (" + cursor_x + ", " + cursor_y + ") </color>");
                    StartCoroutine(PutMouseDown());
                }
            }
        }

        public void OnPointerDragged(MixedRealityPointerEventData eventData)
        {

        }

        public void OnPointerUp(MixedRealityPointerEventData eventData)
        {
            if (eventData.InputSource.SourceType == sourceType)
            {
                var result = eventData.Pointer.Result;
                if (result != null)
                {
                    isDown = false;
                    Vector3 hitPoint = result.Details.Point;
                    Vector3 clickRelative = quadTransform.InverseTransformPoint(hitPoint);
                    cursor_x = clickRelative.x + offset;
                    cursor_y = clickRelative.y + offset;
                    Debug.LogFormat("<color=blue> Cursor Position Up: (" + cursor_x + ", " + cursor_y + ") </color>");
                    StartCoroutine(PutMouseDown());
                }
            }
        }

        // PutMouseDown will update the state of the pointer in MongoDB.
        IEnumerator PutMouseDown()
        {
            string body = "{\"isDown\":" + (isDown ? 1 : 0) + ",\"x\":" + cursor_x + ",\"y\":" + cursor_y + "}";

            byte[] body_data = System.Text.Encoding.UTF8.GetBytes(body);

            using (UnityWebRequest req = UnityWebRequest.Put("http://172.16.136.143:8080/screen_sync/screen_0", body_data))
            {
                req.SetRequestHeader("Content-Type", "application/json");
                yield return req.Send();
                while (!req.isDone)
                    yield return null;
                byte[] result = req.downloadHandler.data;
            }
        }

        private void Start()
        {
            quadTransform = quadObject.transform;
            StartCoroutine(GetScreenData(CheckScreenStatus));
        }

        // Coroutine to check the screen's status
        void CheckScreenStatus(ScreenData info)
        {
            cursor_x = info.x;
            cursor_y = info.y;
            isDown = info.isDown;
        }

        IEnumerator GetScreenData(Action<ScreenData> onSuccess, float wait_time = 0.005f)
        {
            using (UnityWebRequest req = UnityWebRequest.Get(String.Format("http://172.16.136.143:8080/screen_sync/screen_0")))
            {
                callingAPI = true;
                yield return new WaitForSeconds(wait_time);
                yield return req.Send();
                while (!req.isDone)
                    yield return null;
                byte[] result = req.downloadHandler.data;
                string speakerJSON = System.Text.Encoding.Default.GetString(result);
                ScreenData info = JsonUtility.FromJson<ScreenData>(speakerJSON);
                cursor_x = info.x;
                cursor_y = info.y;
                isDown = info.isDown;
                onSuccess(info);
                callingAPI = false;
            }
        }

        private void Update()
        {
            // Call the API if there isn't a call in progress
            if (!callingAPI)
            {
                StartCoroutine(GetScreenData(CheckScreenStatus));
            }
        }
    }   
    #pragma warning restore 0618
}
