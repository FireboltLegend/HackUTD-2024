using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Bhaptics.SDK2;
using TMPro;

public class HapticInteraction : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Interactable"))
            StartHaptics();
    }
    private void OnTriggerExit(Collider other)
    {
        if(other.CompareTag("Interactable"))
            StopHaptics();
    }
    private void StartHaptics()
    {
        BhapticsLibrary.PlayParam("hands", intensity : 1, duration : 1);
        Debug.Log("No haptic device connected");
    }
    void StopHaptics()
    {
        BhapticsLibrary.StopAll();
    }
}