using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BrowserPanel : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        gameObject.GetComponent<Canvas>().worldCamera = GameObject.FindWithTag("MainCamera").GetComponent<OVRCameraRig>().centerEyeAnchor.gameObject.GetComponent<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        if (gameObject.GetComponent<Canvas>().worldCamera == null)
            gameObject.GetComponent<Canvas>().worldCamera = GameObject.FindWithTag("MainCamera").GetComponent<OVRCameraRig>().centerEyeAnchor.gameObject.GetComponent<Camera>();
    }
}
