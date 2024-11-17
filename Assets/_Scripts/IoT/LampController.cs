using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using OculusSampleFramework;
using System;

// Hotfix: For some reason all PUT requests to API Server are redirected to GET requests
// Workaround for now is to send everything as a GET request with an extra request parameter to specify desired behavior

namespace MILab
{
    #pragma warning disable 0618
    public class LampController : MonoBehaviour
    {
        [SerializeField] private Light lampLight;
        [SerializeField] private Material lampShadeMaterial;
        private bool lightOn = false;
        private bool callingAPI = false;

        void Start()
        {
            // Start by syncing scene lamp to server
            StartCoroutine(GetLampData(CheckLampStatus));
        }

        void Update()
        {
            // Call the API if there isn't a call in progress
            if (!callingAPI)
            {
                StartCoroutine(GetLampData(CheckLampStatus));
            }

            if (Input.GetKeyDown(KeyCode.L))
            {
                lightOn = !lightOn;
                StartCoroutine(PutLampData());
            }

        }

        public void LampButtonStateChanged(InteractableStateArgs obj)
        {
            if (obj.NewInteractableState == InteractableState.ActionState)
            {
                ChangeLampState();
            }
        }
        [Button]
        public void ChangeLampState()
        {
            lightOn = !lightOn;
            StartCoroutine(PutLampData());
        }
        // Coroutine to make PUT request
        IEnumerator PutLampData()
        {
            byte[] myData = System.Text.Encoding.UTF8.GetBytes("{status: " + lightOn + "}");
            Debug.Log("PUT lamp: " + lightOn);
            String body = "";
            if (lightOn==true)
            body = "{\"lamp_status\":" + 1+"}";
            else
            body = "{\"lamp_status\":" + 0 + "}";
            byte[] body_data = System.Text.Encoding.UTF8.GetBytes(body);

            //using (UnityWebRequest req = UnityWebRequest.Put(String.Format("http://172.16.136.165:5000/lamp/1?status={0}", lightOn), myData))
            //using (UnityWebRequest req = UnityWebRequest.Put(String.Format("http://digitaltwin-henrykim.pitunnel.com/lamp/1?status={0}", lightOn), myData))
            using (UnityWebRequest req = UnityWebRequest.Put("http://172.16.136.143:8080/lamp_data/lamp_0", body_data))

            {
                req.SetRequestHeader("Content-Type", "application/json");
                yield return req.Send();
                while (!req.isDone)
                    yield return null;
                byte[] result = req.downloadHandler.data;

                //string lampJSON = System.Text.Encoding.Default.GetString(result);
                //LampData info = JsonUtility.FromJson<LampData>(lampJSON);
            }
        }

        // Coroutine to make GET request
        IEnumerator GetLampData(Action<LampData> onSuccess, float wait_time = 0.1f)
        {
            //using (UnityWebRequest req = UnityWebRequest.Get(String.Format("http://172.16.136.165:5000/lamp/1")))
            //using (UnityWebRequest req = UnityWebRequest.Get(String.Format("http://digitaltwin-henrykim.pitunnel.com/lamp/1")))
            //using (UnityWebRequest req = UnityWebRequest.Get(String.Format("https://op2hg17cxl.execute-api.us-east-2.amazonaws.com/Prod/lamp?Device_name=lamp_0")))
            using (UnityWebRequest req = UnityWebRequest.Get(String.Format("http://172.16.136.143:8080/lamp_data/lamp_0")))
            //using (UnityWebRequest req = UnityWebRequest.Get(String.Format("https://op2hg17cxl.execute-api.us-east-2.amazonaws.com/Prod/lamp?Device_name=lamp_0")))
            {
                callingAPI = true;
                yield return new WaitForSeconds(wait_time);
                yield return req.Send();
                while (!req.isDone)
                    yield return null;
                byte[] result = req.downloadHandler.data;
                string lampJSON = System.Text.Encoding.Default.GetString(result);
                LampData info = JsonUtility.FromJson<LampData>(lampJSON);
                onSuccess(info);
                callingAPI = false;
            }
        }

        void CheckLampStatus(LampData info)
        {
            //Debug.Log("Lamp status: " + info.status);
            lightOn = info.lamp_status;
            lampLight.enabled = lightOn;

            // Make lampshade glow if light is on
            if (lightOn)
            {
                lampShadeMaterial.EnableKeyword("_EMISSION");
            }
            else
            {
                lampShadeMaterial.DisableKeyword("_EMISSION");
            }
        }
    }
    #pragma warning restore 0618
}