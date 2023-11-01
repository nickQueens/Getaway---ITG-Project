using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public GameObject followObject;

    void LateUpdate()
    {
        transform.position = new Vector3(followObject.transform.position.x - 5, 20, followObject.transform.position.z - 5);
        transform.LookAt(followObject.transform); 
    }
}
