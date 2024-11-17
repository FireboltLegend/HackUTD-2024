using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;

namespace MILab
{
    public class TempHumPressSensorController : MonoBehaviour
    {
        [SerializeField] TextMeshPro temperatureText;
        [SerializeField] TextMeshPro humidityText;
        [SerializeField] TextMeshPro pressureText;
        [SerializeField] float waitTimeInSeconds = 10;
        bool toCheckForSensorUpdate = true;

        // Start is called before the first frame update
        void Start()
        {
            StartCoroutine(CheckSensorUpdates(waitTimeInSeconds));
        }

        // Update is called once per frame
        void Update()
        {
            if (toCheckForSensorUpdate)
            {
                StartCoroutine(CheckSensorUpdates(waitTimeInSeconds));
            }
        }

        IEnumerator CheckSensorUpdates(float waitTimeInSeconds)
        {
            using (UnityWebRequest req = UnityWebRequest.Get(String.Format("http://172.16.136.143:8080/Sensor_THP/SensorPush")))
            {
                toCheckForSensorUpdate = false;
                yield return req.Send();
                while (!req.isDone)
                {
                    yield return null;
                }
                byte[] result = req.downloadHandler.data;
                string sensorTPHJSON = System.Text.Encoding.Default.GetString(result);
                SensorTPHData info = JsonUtility.FromJson<SensorTPHData>(sensorTPHJSON);
                temperatureText.GetComponent<TextMeshPro>().text = info.Temperature + "°C";
                humidityText.GetComponent<TextMeshPro>().text = info.Humidity + "%";
                pressureText.GetComponent<TextMeshPro>().text = (float.Parse(info.Pressure) / 101325.0f).ToString("F2") + "atm";
                yield return new WaitForSeconds(waitTimeInSeconds);
                toCheckForSensorUpdate = true;
            }
        }
    }
}

