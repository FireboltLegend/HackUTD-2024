using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Bhaptics.SDK2;
using TMPro;

public class HapticInteraction : MonoBehaviour
{
    public TextMeshProUGUI textMeshProUGUI;
    public string hands;

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Interactable"))
            StartHaptics(other.gameObject, textMeshProUGUI);
    }
    private void OnTriggerExit(Collider other)
    {
        if(other.CompareTag("Interactable"))
            StopHaptics();
    }
    private void StartHaptics(GameObject gameObject, TextMeshProUGUI textMeshProUGUI)
    {
        BhapticsLibrary.PlayParam(hands, intensity : 1, duration : 1);
        Debug.Log("No haptic device connected");
    }
    void StopHaptics()
    {
        
    }
}