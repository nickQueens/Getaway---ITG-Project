using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public GameObject followObject;
    public float cameraHeight;

    void LateUpdate()
    {
        transform.position = new Vector3(followObject.transform.position.x - 5, cameraHeight, followObject.transform.position.z - 5);
        transform.LookAt(followObject.transform); 
    }
}
