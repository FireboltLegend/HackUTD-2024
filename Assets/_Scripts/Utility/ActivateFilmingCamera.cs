using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MILab
{
    public class ActivateFilmingCamera : MonoBehaviour
    {
        public GameObject filmingCamera;
        public GameObject vrMirrorRenderer;
        public SingleMirrorRenderer singleMirrorRenderer;
        public Transform[] filmPoints;


        private void Start()
        {
            //if (Application.isEditor)
            if (!OVRManager.isHmdPresent)
            {
                // Deactivate VR stuff
                PlayerOVR.Instance.transform.position -= new Vector3(0, 10, 0);
                PlayerOVR.Instance.gameObject.SetActive(false);
                if (vrMirrorRenderer) vrMirrorRenderer.SetActive(false);


                for (int i = 0; i < filmPoints.Length; i++)
                {
                    Instantiate(filmingCamera, filmPoints[i]);
                }

                SwitchCamera(1);


            }
        }

        private void Update()
        {
            // Hard coded for 3 cameras
            if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1))
            {
                SwitchCamera(1);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2))
            {
                SwitchCamera(2);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3))
            {
                SwitchCamera(3);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha4) || Input.GetKeyDown(KeyCode.Keypad4))
            {
                SwitchCamera(4);
            }
        }

        // Switches to specified camera and deactivates the others
        private void SwitchCamera(int camNum)
        {
            // Deactivate all cameras
            for (int i = 0; i < filmPoints.Length; i++)
            {
                filmPoints[i].gameObject.SetActive(false);
            }

            // Activate the only camera that should be on
            filmPoints[camNum - 1].gameObject.SetActive(true);
            var filming = filmPoints[camNum - 1].GetChild(0).GetComponent<Camera>();
            if (singleMirrorRenderer)
            {
                singleMirrorRenderer.gameObject.SetActive(true);
                singleMirrorRenderer._sourceCamOverride = filming;
            }
        }
    }
}
