using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Bhaptics.SDK2;

public class FingerHaptics : MonoBehaviour
{
    /// <summary>
    /// Left is 0, Right is 1
    /// </summary>
    public int hand;
    
    public void PlayUIHapticInteraction()
    {
        //Thumb
        if (gameObject.name.Contains("Thumb"))
        {
            BhapticsLibrary.PlayMotors(
                position: hand,
                motors: new int[6] { 50, 0, 0, 0, 0, 0 },
                durationMillis: 1000
                );
        }

        //Index
        else if (gameObject.name.Contains("Index"))
        {
            BhapticsLibrary.PlayMotors(
                position: hand,
                motors: new int[6] { 0, 50, 0, 0, 0, 0 },
                durationMillis: 1000
                );
        }

        //Middle
        else if (gameObject.name.Contains("Middle"))
        {
            BhapticsLibrary.PlayMotors(
                position: hand,
                motors: new int[6] { 0, 0, 50, 0, 0, 0 },
                durationMillis: 1000
                );
        }

        //Ring
        else if (gameObject.name.Contains("Ring"))
        {
            BhapticsLibrary.PlayMotors(
                position: hand,
                motors: new int[6] { 0, 0, 0, 50, 0, 0 },
                durationMillis: 1000
                );
        }

        //Pinky
        else if (gameObject.name.Contains("Pinky"))
        {
            BhapticsLibrary.PlayMotors(
                position: hand,
                motors: new int[6] { 0, 0, 0, 0, 50, 0 },
                durationMillis: 1000
                );
        }

    }

    private void OnTriggerStay(Collider other)
    {
        if (other.tag == "UI")
            PlayUIHapticInteraction();
    }

}
