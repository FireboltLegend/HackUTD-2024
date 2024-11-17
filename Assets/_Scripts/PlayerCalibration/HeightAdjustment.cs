using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.UI;

public class HeightAdjustment : MonoBehaviour
{
    private GameObject currentPlayer; //tracking space

    [SerializeField]
    private GameObject pinchSlider;

    private float sliderStartingValue = 0f;

    private float startingPlayerHeight;

    private void Start()
    {
        CheckPlayer();

        startingPlayerHeight = currentPlayer.transform.position.y; //getthe startHeight
    }

    // Update is called once per frame
    void Update()
    {
        if (pinchSlider.activeSelf)
        {
            currentPlayer.transform.position = new Vector3(
                currentPlayer.transform.position.x,
                startingPlayerHeight + pinchSlider.GetComponent<PinchSlider>().SliderValue - sliderStartingValue,
                currentPlayer.transform.position.z
                );
        }
    }

    void CheckPlayer()
    {
        if (currentPlayer == null)
            currentPlayer = GameObject.Find("TrackingSpace"); //CustomMRTK-Quest_OVRCameraRig
    }
}
