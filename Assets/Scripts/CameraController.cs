using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public GameObject followObject;
    public float cameraHeight;

    void LateUpdate()
    {
        transform.position = new Vector3
        (
            followObject.transform.position.x,
            followObject.transform.position.y + cameraHeight,
            followObject.transform.position.z - 10
        );
        transform.LookAt(followObject.transform); 
    }
}
