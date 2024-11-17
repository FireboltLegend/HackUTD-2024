using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ObjectEditingEvents : MonoBehaviour
{
    public List<PlaceableObjectController> placables = new List<PlaceableObjectController>();
    public UnityEvent<string> OnHandMenuChange;


    public void UpdatePlacables(string menu)
    {
        foreach(PlaceableObjectController objCntrl in placables)
        {
            objCntrl.HandMenuChangeHandler(menu);
        }
    }

}
