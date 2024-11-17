using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Outliner : MonoBehaviour
{
    // Start is called before the first frame update
    float timer = 0f;
    private void Update()
    {
        timer += Time.deltaTime;
    }
    void OnTriggerEnter(Collider other)
    {
        if (GetComponent<Outline>().enabled && timer > .3)
        {
            GetComponent<Outline>().enabled = false;
            timer = 0f;
        }
        else if(timer > .3)
        {
            GetComponent<Outline>().enabled = true;
            timer = 0f;
        }
    }
}