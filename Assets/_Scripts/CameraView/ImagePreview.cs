using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System;


namespace MILab
{
    public class ImagePreview : MonoBehaviour
    {
        [SerializeField, ReadOnly] string imageUrl; //= "https://remotely-glorious-gelding.ngrok-free.app/uploads/captured_image_20240227_164841.jpg";
        [SerializeField] Renderer imageRenderer;
        [SerializeField] Texture2D loadingTexture;
        [SerializeField] float loadImageDelay = 2.0f;
        Texture2D prevTexture;
        string latestImageName;
        string prevImageName = "";

        void Start()
        {
            //imageRenderer = GetComponent<Renderer>();

            // Start the image loading process
            //StartCoroutine(LoadImage());
        }

        [Button]
        public void LoadImagePreview()
        {
            StartCoroutine(LoadImage());
        }

        IEnumerator LoadImage()
        {
            if (imageRenderer != null)
            {
                imageRenderer.material.mainTexture = loadingTexture;
                imageRenderer.material.SetColor("_BaseColor", Color.white);
                imageRenderer.material.SetColor("_EmissionColor", Color.white);
            }

            yield return new WaitForSeconds(loadImageDelay);

            using (UnityWebRequest req = UnityWebRequest.Get(String.Format("http://172.16.136.143:8080/Smart_Switch/CameraView")))
            {

                yield return req.Send();
                while (!req.isDone)
                    yield return null;
                byte[] result = req.downloadHandler.data;
                string switchJSON = System.Text.Encoding.Default.GetString(result);
                SwitchData info = JsonUtility.FromJson<SwitchData>(switchJSON);

                latestImageName = info.switch_ip;
                //Debug.Log(info.switch_name);
                //Debug.Log(info.switch_status);


            }

            if (latestImageName != prevImageName)
            {
                
                
                imageUrl = "https://remotely-glorious-gelding.ngrok-free.app/uploads_vr/" + latestImageName;
                Debug.LogFormat("<color=cyan>" + imageUrl + "</color>");

                // Create a UnityWebRequest object with the image URL
                UnityWebRequest www = UnityWebRequestTexture.GetTexture(imageUrl);

                // Wait for the download to complete
                yield return www.SendWebRequest();

                // Check for errors
                if (www.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError("Failed to load image: " + www.error);
                }
                else
                {
                    // Get the downloaded texture
                    Texture2D texture = DownloadHandlerTexture.GetContent(www);

                    // Apply the texture to the object's material
                    if (imageRenderer != null)
                    {
                        imageRenderer.material.mainTexture = texture;

                        // Example: Adjust shader properties
                        // Adjust the base map color to white
                        imageRenderer.material.SetColor("_BaseColor", Color.white);

                        //imageRenderer.material.SetFloat("_Metallic", 0.0f);
                        //imageRenderer.material.SetFloat("_Smoothness", 0.5f);
                        imageRenderer.material.SetColor("_EmissionColor", Color.white);
                        //imageRenderer.material.SetColor("_BaseMap", Color.white);
                        prevTexture = texture;
                    }
                }

                prevImageName = latestImageName;
            }
            else if (prevTexture != null)
            {
                if(imageRenderer != null)
                {
                    imageRenderer.material.mainTexture = prevTexture;
                    imageRenderer.material.SetColor("_BaseColor", Color.white);
                    imageRenderer.material.SetColor("_EmissionColor", Color.white);
                }
            }

            
        }
    }
}