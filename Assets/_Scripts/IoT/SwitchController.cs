using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace MILab {
    public class SwitchController : MonoBehaviour
    {
        [SerializeField] GameObject indicator;
        [SerializeField] GameObject tvScreen;
        [SerializeField] float waitTimeInSeconds = 0.1f;
        //SpriteRenderer indicatorColor;
        string switchStatus;
        bool callingAPI = false;

        // Start is called before the first frame update
        void Start()
        {
            //indicatorColor = indicator.GetComponent<SpriteRenderer>();
            StartCoroutine(GetSwitchData(CheckSwitchStatus,waitTimeInSeconds));
        }

        // Update is called once per frame
        void Update()
        {
            // Call the API if there isn't a call in progress
            if (!callingAPI)
            {
                StartCoroutine(GetSwitchData(CheckSwitchStatus, waitTimeInSeconds));
            }
        }

        [Button]
        public void ChangeSwitchState()
        {
            //Debug.Log("Attempting Switch Status...");
            if (switchStatus == "on")
            {
                switchStatus = "off";
            }

            else
            {
                switchStatus = "on";
            }
                
            //Debug.Log(switchStatus);
            ChangeIndicatorColorAndTogglePower();
            StartCoroutine(PutSwitchData());
        }

        // Coroutine to make GET request
        IEnumerator GetSwitchData(Action<SwitchData> onSuccess, float waitTimeInSeconds)
        {
            using (UnityWebRequest req = UnityWebRequest.Get(String.Format("http://172.16.136.143:8080/Smart_Switch/MILab_switch")))
            {
                callingAPI = true;
                yield return req.Send();
                while (!req.isDone)
                    yield return null;
                byte[] result = req.downloadHandler.data;
                string switchJSON = System.Text.Encoding.Default.GetString(result);
                SwitchData info = JsonUtility.FromJson<SwitchData>(switchJSON);
                onSuccess(info);
                //Debug.Log(info.switch_name);
                //Debug.Log(info.switch_status);
                yield return new WaitForSeconds(waitTimeInSeconds);
                callingAPI = false;
            }
        }

        // Coroutine to make PUT request
        IEnumerator PutSwitchData()
        {
            String body = "";
            body = "{\"switch_status\":\"" + switchStatus + "\"}";
            byte[] body_data = System.Text.Encoding.UTF8.GetBytes(body);
            using (UnityWebRequest req = UnityWebRequest.Put("http://172.16.136.143:8080/Smart_Switch/MILab_switch", body_data))

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

        void CheckSwitchStatus(SwitchData info)
        {
            switchStatus = info.switch_status;
            //Debug.Log(switchStatus);
            ChangeIndicatorColorAndTogglePower();
        }

        void ChangeIndicatorColorAndTogglePower()
        {
            if (switchStatus == "off")
            {
                tvScreen.SetActive(false);
                indicator.SetActive(false);
            }
                
            else
            {
                tvScreen.SetActive(true);
                indicator.SetActive(true);
            }
        }
    }
}
