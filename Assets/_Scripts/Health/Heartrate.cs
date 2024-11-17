using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System;
using TMPro;

namespace MILab
{
    public class Heartrate : MonoBehaviour
    {
        [SerializeField] private HealthChannelSO _healthChannelSO;
        [SerializeField] private TMP_Text _textMeshPro;
        [SerializeField] private AudioSource _audio;
        [SerializeField] private AudioClip _slowBeat;
        [SerializeField] private AudioClip _mediumBeat;
        [SerializeField] private AudioClip _FastBeat;
        [SerializeField, ReadOnly] private int _heartRate = 0;

        enum Speed { SLOW, MED, FAST }
        [SerializeField, ReadOnly] Speed _currentSpeed = Speed.MED;
        [SerializeField] private int _slowToMedThreshold = 75;
        [SerializeField] private int _medToFastThreshold = 90;


        public int HeartRate
        {
            get { return _heartRate; }
        }

        private bool _isCallingAPI = false;

        private void OnEnable()
        {
            _isCallingAPI = false;
        }

        // Update is called once per frame
        void Update()
        {
            // Call the API if there isn't a call in progress
            if (!_isCallingAPI)
            {
                StartCoroutine(GetRequest());
            }
        }


        IEnumerator GetRequest()
        {
            _isCallingAPI = true;
            using (UnityWebRequest webRequest = UnityWebRequest.Get("https://dev.pulsoid.net/api/v1/data/heart_rate/latest?response_mode=text_plain_only_heart_rate"))
            {
                webRequest.SetRequestHeader("Authorization", "Bearer " + _healthChannelSO.PulsoidToken);
                Debug.Log("Using Pulsoid token: " + _healthChannelSO.PulsoidToken);
                // Request and wait for the desired page.
                yield return webRequest.SendWebRequest();
                _heartRate = int.Parse(webRequest.downloadHandler.text);
                _textMeshPro.text = webRequest.downloadHandler.text;
            }
            // Handle the audio 
            if (_currentSpeed == Speed.SLOW)
            {
                if (_heartRate > _slowToMedThreshold)
                {
                    _audio.clip = _mediumBeat;
                    _audio.Play();
                    _currentSpeed = Speed.MED;
                }
            }
            else if (_currentSpeed == Speed.MED)
            {
                if (_heartRate < _slowToMedThreshold)
                {
                    _audio.clip = _slowBeat;
                    _currentSpeed = Speed.SLOW;
                    _audio.Play();
                }
                else if (_heartRate > _medToFastThreshold)
                {
                    _audio.clip = _slowBeat;
                    _currentSpeed = Speed.FAST;
                    _audio.Play();
                }
            }
            else if (_currentSpeed == Speed.FAST)
            {
                if (_heartRate < _medToFastThreshold)
                {
                    _audio.clip = _mediumBeat;
                    _currentSpeed = Speed.MED;
                    _audio.Play();
                }
            }
            _isCallingAPI = false;
        }
    }
}