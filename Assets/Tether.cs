using MILab;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tether : MonoBehaviour
{
    [SerializeField] float refObjYPos;
    [SerializeField] US1Manager us1Man;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void tether_to_table()
    {
        gameObject.transform.localRotation = Quaternion.Euler(0, transform.localRotation.eulerAngles.y, 0);
        //gameObject.transform.position = new Vector3(gameObject.transform.position.x,0,gameObject.transform.position.z);
        if (Mathf.Abs(transform.localPosition.y - refObjYPos) < 0.05f)
        {
            transform.localPosition = new Vector3(
                gameObject.transform.localPosition.x,
                refObjYPos, 
                gameObject.transform.localPosition.z);
        }
    }
}
