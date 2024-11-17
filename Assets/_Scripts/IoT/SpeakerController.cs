using System.Collections;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using OculusSampleFramework;
using TMPro;
using System;

// Hotfix: For some reason all PUT requests to API Server are redirected to GET requests
// Workaround for now is to send everything as a GET request with an extra request parameter to specify desired behavior

namespace MILab
{
    [RequireComponent(typeof(AudioSource))]
    #pragma warning disable 0618
    public class SpeakerController : MonoBehaviour
    {
        [SerializeField] private AudioSource audioSource;
        private bool is_playing;
        //private int[] song_ids = new int[] { 0, 1, 2 };
        private string[] songs = new string[] { "hey", "inspire", "piano",  };
        private int cur_index;
        private bool callingAPI = false;
        [SerializeField]
        private TextMeshPro songname;

        [SerializeField]
        private GameObject play_icon;
        [SerializeField]
        private GameObject pause_icon;



        private string audio_string = "Audio/";
        //AudioClip clip;
        string audioPath;

        private float volume;

        void Start()
        {
             audioSource = GetComponent<AudioSource>();
             //clip = Resources.Load<AudioClip>(audioPath);
             //audioSource.clip = clip;
            /* if (File.Exists(audioPath))
             {
                 Debug.Log("File exists at path: " + audioPath);
             }
             else
             {
                 Debug.LogWarning("File does not exist at path: " + audioPath);
             }*/
            // Start by syncing scene speaker to server
            StartCoroutine(GetSpeakerData(CheckSpeakerStatus));
        }

        void Update()
        {
            // Call the API if there isn't a call in progress
            if (!callingAPI)
            {
                StartCoroutine(GetSpeakerData(CheckSpeakerStatus));
            }


        }

        // ToggleBackward plays the previous song.
        [Button]
        public void ToggleBackward()
        {
            cur_index = Math.Abs((cur_index - 1) % songs.Length);
            audioPath = audio_string + songs[cur_index];
            //clip = Resources.Load<AudioClip>(audioPath);

            //audioSource.clip = clip;
            LoadAndAssignAudioClip();
            Debug.Log("Previous");
            StartCoroutine(PutBackward());
            is_playing = true;
            StartCoroutine(PutPlayPause());
        }

        // PutBackward is Previous functionality. 
        IEnumerator PutBackward()
        {
            String body = "";

            body += "{\"song_id\":" + cur_index + "}";
            byte[] body_data = System.Text.Encoding.UTF8.GetBytes(body);

            using (UnityWebRequest req = UnityWebRequest.Put("http://172.16.136.143:8080/Smart_Speaker/sonos_1", body_data))
            {
                req.SetRequestHeader("Content-Type", "application/json");
                yield return req.Send();
                while (!req.isDone)
                    yield return null;
                byte[] result = req.downloadHandler.data;
                //string speakerJSON = System.Text.Encoding.Default.GetString(result);
                //SpeakerData info = JsonUtility.FromJson<SpeakerData>(speakerJSON);
            }
            if (is_playing)
            {
                audioSource.Play();
            }
            else
            {
                audioSource.Pause();
            }

        }

        // ToggleForward plays the next song.
        [Button]
        public void ToggleForward()
        {
            cur_index = (cur_index + 1) % songs.Length;
            audioPath = audio_string + songs[cur_index];
            //clip = Resources.Load<AudioClip>(audioPath);

            //audioSource.clip = clip;
            LoadAndAssignAudioClip();

            Debug.Log("Next");
            StartCoroutine(PutForward());
            is_playing = true;
            StartCoroutine(PutPlayPause());
        }

        // PutForward is Next functionality. 
        IEnumerator PutForward()
        {
            String body = "";

            body += "{\"song_id\":" + cur_index + "}";
            byte[] body_data = System.Text.Encoding.UTF8.GetBytes(body);

            using (UnityWebRequest req = UnityWebRequest.Put("http://172.16.136.143:8080/Smart_Speaker/sonos_1", body_data))
            {
                req.SetRequestHeader("Content-Type", "application/json");
                yield return req.Send();
                while (!req.isDone)
                    yield return null;
                byte[] result = req.downloadHandler.data;
                //string speakerJSON = System.Text.Encoding.Default.GetString(result);
                //SpeakerData info = JsonUtility.FromJson<SpeakerData>(speakerJSON);
            }
            if (is_playing)
            {
                audioSource.Play();
            }
            else
            {
                audioSource.Pause();
            }
        }

        // PutPlayPause toggles play/pause functionality of audio.
        [Button]
        public void TogglePlayPause()
        {
            is_playing = !is_playing;
            if (is_playing)
            {
                audioSource.Play();
                
            }
            else
            {
                audioSource.Pause();
               
            }
            StartCoroutine(PutPlayPause());
        }

        IEnumerator PutPlayPause()
        {
            String body = "";
            if (is_playing == false) { 
                body = "{\"is_playing\":" + 0 + "}";
            }
            else { 
                body = "{\"is_playing\":" + 1 + "}";
            }
            byte[] body_data = System.Text.Encoding.UTF8.GetBytes(body);

            using (UnityWebRequest req = UnityWebRequest.Put("http://172.16.136.143:8080/Smart_Speaker/sonos_1", body_data))
            {
                req.SetRequestHeader("Content-Type", "application/json");
                yield return req.Send();
                while (!req.isDone)
                    yield return null;
                byte[] result = req.downloadHandler.data;
                // string speakerJSON = System.Text.Encoding.Default.GetString(result);
                // SpeakerData info = JsonUtility.FromJson<SpeakerData>(speakerJSON);
            }

            
        }

        // IncVolume increases volume by 10.
        [Button]
        public void IncVolume()
        {
            Debug.Log("Volume increased");
            StartCoroutine(PutVolumeData(10));
        }

        // DecVolume decreases volume by 10.
        [Button]
        public void DecVolume()
        {
            Debug.Log("Volume decreased");
            StartCoroutine(PutVolumeData(-10));
        }

        // PutVolumeData either increases or decreases the volume, dependent on input.
        IEnumerator PutVolumeData(float change)
        {
            volume += change;

            // construct body
            String body = "";
            body = "{\"volume\":" + volume + "}";

            byte[] body_data = System.Text.Encoding.UTF8.GetBytes(body);

            using (UnityWebRequest req = UnityWebRequest.Put("http://172.16.136.143:8080/Smart_Speaker/sonos_1", body_data))
            {
                req.SetRequestHeader("Content-Type", "application/json");
                yield return req.Send();
                while (!req.isDone)
                    yield return null;
                byte[] result = req.downloadHandler.data;
                //string speakerJSON = System.Text.Encoding.Default.GetString(result);
                //SpeakerData info = JsonUtility.FromJson<SpeakerData>(speakerJSON);
            }
            audioSource.volume = volume/100;
        }

        // Coroutine to make GET request that updates speaker data.
        IEnumerator GetSpeakerData(Action<SpeakerData> onSuccess, float wait_time = 0.1f)
        {
            using (UnityWebRequest req = UnityWebRequest.Get(String.Format("http://172.16.136.143:8080/Smart_Speaker/sonos_1")))
            {
                callingAPI = true;
                yield return new WaitForSeconds(wait_time);
                yield return req.Send();
                while (!req.isDone)
                    yield return null;
                byte[] result = req.downloadHandler.data;
                string speakerJSON = System.Text.Encoding.Default.GetString(result);
                SpeakerData info = JsonUtility.FromJson<SpeakerData>(speakerJSON);
                volume = info.volume;
                onSuccess(info);
                callingAPI = false;
            }
        }

        // Coroutine to check the speaker's playing status
        void CheckSpeakerStatus(SpeakerData info)
        {
            is_playing = info.is_playing;
            if(is_playing)
            {
                play_icon.SetActive(false);
                pause_icon.SetActive(true);
            }
            if (!is_playing)
            {
                play_icon.SetActive(true);
                pause_icon.SetActive(false);
            }
            volume = info.volume;
            cur_index = info.song_id;
            audioPath =  audio_string + songs[cur_index];
            LoadAndAssignAudioClip();
            Debug.Log(songs[cur_index]);
            songname.text = songs[cur_index];
            audioSource.volume = volume / 100;
            if (is_playing && !audioSource.isPlaying)
            {
                audioSource.Play();
                
            }

            if (!is_playing && audioSource.isPlaying)
            {
                audioSource.Pause();
            }


        }

        void LoadAndAssignAudioClip()
        {
            AudioClip newAudioClip = Resources.Load<AudioClip>(audioPath);
            audioSource.clip = newAudioClip;
            
           
            
        }

    }
    #pragma warning restore 0618
}