using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LiDARAnchor : MonoBehaviour
{
    GameObject centerEye;

    // Start is called before the first frame update
    void Start()
    {
        if (centerEye == null)
            centerEye = GameObject.Find("CenterEyeAnchor");
    }

    private void Update()
    {
        if (centerEye == null)
            centerEye = GameObject.Find("CenterEyeAnchor");

        else if (!transform.parent.name.Contains("CenterEyeAnchor"))
        {
            transform.SetParent(centerEye.transform);
        }
    }


}
