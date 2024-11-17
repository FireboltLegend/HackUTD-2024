using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.Networking;
using OculusSampleFramework;
using System;

//Used to receive the name from the python model and then update the position to where the raycast was pointing.
//Not used anymore
//Also does not use sockets anymore, was updated to use AWS database
namespace MILab
{
    public class object_detection_socket : MonoBehaviour
    {
        public float raycastDistance = 20f; // Maximum distance for the raycast

        //for sockets
        Thread thread;
        public int connectionPort = 25001;
        TcpListener server;
        TcpClient client;
        bool running;
        private bool callingAPI = false;
        //for bottle object
        public string object_detected;

        //The objects we have in the scene
        public GameObject bottleObject;
        public GameObject lampObject;
        public GameObject raycastOrigin;

        private LineRenderer lineRenderer;

        private void Start()
        {
            // Start by syncing scene lamp to server

            StartCoroutine(GetObjectData(1f));
            // Find the objects in the scene
            bottleObject = GameObject.Find("BottleGrabbable");
            lampObject = GameObject.Find("Lamp");
            raycastOrigin = GameObject.Find("raycastAnchor");

            // Disable the renderers if no object is detected
            Renderer bottleRenderer = bottleObject.GetComponent<Renderer>();
            if (bottleRenderer != null)
            {
                bottleRenderer.enabled = false;
            }


            // Create a LineRenderer component
            lineRenderer = raycastOrigin.AddComponent<LineRenderer>();
            lineRenderer.startColor = Color.red;
            lineRenderer.endColor = Color.red;
            lineRenderer.startWidth = 0.05f;
            lineRenderer.endWidth = 0.05f;

            // Receive on a separate thread so Unity doesn't freeze waiting for data
            //ThreadStart ts = new ThreadStart(GetData);
            //thread = new Thread(ts);
            //thread.Start();
        }

        void GetData()
        {
            // Create the server
            server = new TcpListener(IPAddress.Any, connectionPort);
            server.Start();

            // Create a client to get the data stream
            client = server.AcceptTcpClient();

            // Start listening
            running = true;
            while (running)
            {
                Connection();
            }
            server.Stop();
        }

        void Connection()
        {
            // Read data from the network stream
            NetworkStream nwStream = client.GetStream();
            byte[] buffer = new byte[client.ReceiveBufferSize];
            int bytesRead = nwStream.Read(buffer, 0, client.ReceiveBufferSize);

            // Decode the bytes into a string
            string dataReceived = Encoding.UTF8.GetString(buffer, 0, bytesRead);

            // Make sure we're not getting an empty string
            dataReceived = dataReceived.Trim();
            if (!string.IsNullOrEmpty(dataReceived))
            {
                // Convert the received string of data to the format we are using
                object_detected = ParseData(dataReceived);
                nwStream.Write(buffer, 0, bytesRead);
            }
        }

        public static string ParseData(string dataString)
        {
            Debug.Log(dataString);
            // Remove the parentheses
            if (dataString.StartsWith("(") && dataString.EndsWith(")"))
            {
                dataString = dataString.Substring(1, dataString.Length - 2);
            }

            return dataString;
        }

        // Coroutine to make GET request
        IEnumerator GetObjectData(float waittime)
        {


            using (UnityWebRequest request = UnityWebRequest.Get(String.Format("https://op2hg17cxl.execute-api.us-east-2.amazonaws.com/Prod/object_detection?Object_name=Current_Object")))
            {

                callingAPI = true;
                yield return new WaitForSeconds(waittime);
                yield return request.Send();
                while (!request.isDone)
                    yield return null;
                Debug.Log(request.result);

                byte[] result = request.downloadHandler.data;
                string ObjectJSON = System.Text.Encoding.Default.GetString(result);
                //ObjectData info = JsonUtility.FromJson<ObjectData>(ObjectJSON);
                //object_detected = info.Detected_Object;
                //Debug.Log(object_detected);
                callingAPI = false;



            }
        }

        void Update()
        {
            if (!callingAPI)
            {
                StartCoroutine(GetObjectData(1.0f));
            }

            if (raycastOrigin == null)
            {
                return; // Exit early if the raycast origin reference is not available
            }

            // Cast a ray from the raycast origin forward
            Ray ray = new Ray(raycastOrigin.transform.position, raycastOrigin.transform.forward);
            RaycastHit hit;

            // Update the line renderer to render the ray
            lineRenderer.SetPosition(0, ray.origin);
            lineRenderer.SetPosition(1, ray.origin + ray.direction * raycastDistance);

            // Check if the ray intersects with any collider
            if (Physics.Raycast(ray, out hit, raycastDistance))
            {
                // Check the tag of the collided object
                if (object_detected == "bottle")
                {
                    // Move the bottle object to the position of the collision
                    bottleObject.transform.position = hit.point;

                    // Offset the bottle position upwards
                    Vector3 newBottlePos = new Vector3(bottleObject.transform.position.x, bottleObject.transform.position.y, bottleObject.transform.position.z);
                    bottleObject.transform.position = newBottlePos;

                    // Enable the bottle renderer
                    Renderer bottleRenderer = bottleObject.GetComponent<Renderer>();
                    if (bottleRenderer != null)
                    {
                        bottleRenderer.enabled = true;
                    }

                    // Reset the object detected
                    object_detected = "";
                }
                else if (object_detected == "lamp")
                {
                    float distancePointToLamp = Vector3.Distance(hit.point, lampObject.transform.position);

                    Debug.Log(distancePointToLamp);

                    if (distancePointToLamp > 0.12)
                    {
                        // Move the lamp object to the position of the collision
                        lampObject.transform.position = hit.point;

                        // Offset the lamp position upwards
                        Vector3 newLampPos = new Vector3(lampObject.transform.position.x, lampObject.transform.position.y, lampObject.transform.position.z);
                        lampObject.transform.position = newLampPos;

                        // Enable the lamp renderer
                        Renderer lampRenderer = lampObject.GetComponent<Renderer>();
                        if (lampRenderer != null)
                        {
                            lampRenderer.enabled = true;
                        }

                        // Reset the object detected
                        object_detected = "";
                    }
                    
                }
            }

        } //end of update func

    } //end of class

}