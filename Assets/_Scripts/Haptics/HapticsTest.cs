using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Bhaptics.SDK2;

public class HapticsTest : MonoBehaviour
{
    FingerHaptics finger;
    
    
    public void PlayTestHaptics()
    {
        BhapticsLibrary.Play(BhapticsEvent.PUNCH);

        //BhapticsLibrary.PlayMotors(
        //        position: 4, //(int)PositionType.GloveL,
        //        motors: new int[6] { 0, 70, 0, 0, 0, 0 },
        //        durationMillis: 1000
        //        );
    }

    public void TestFingerHaptics()
    {
        if (finger == null)
            finger = GameObject.Find("Hand_IndexTip").GetComponent<FingerHaptics>();


        finger.PlayUIHapticInteraction();
    }

    public void CameraTest()
    {

    }

}
