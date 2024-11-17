using OculusSampleFramework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;


/* Much simpler compared to the hand interaction buttons from Oculus package.
 * This script mainly detects the collision of the hand with the trigger collider on the button.
 * The button must have a collider that is a trigger.
 * The interacting hand must have a collider and a rigidbody (is kinematic and not affected by gravity)
 * 
 * TODO: Remove the unnecessary stuff from Oculus's button design.
 */
namespace MILab
{
    public class NonPressButtonScript : MonoBehaviour
    {
        public UnityEvent onPress = new UnityEvent();
        public float cooldownTotal = 0.5f;
        float cooldown = 0f;

        bool buttonActive = true;


        // Detect trigger by player
        private void OnTriggerEnter(Collider other)
        {
            Debug.Log("On Collision Enter: " + other.gameObject.name);
            if (buttonActive)
            {
                onPress.Invoke();
                buttonActive = false;
                cooldown = cooldownTotal;
            }
        }
        // Update is called once per frame
        void Update()
        {
            if (cooldown < 0f)
            {
                buttonActive = true;
            }
            else
            {
                cooldown -= Time.deltaTime;
            }
        }

    }

}