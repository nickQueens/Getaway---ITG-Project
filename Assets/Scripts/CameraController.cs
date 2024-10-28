using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform followTransform;
    public float cameraHeight;

    void LateUpdate()
    {
        transform.position = new Vector3
        (
            followTransform.position.x,
            followTransform.position.y + cameraHeight,
            followTransform.position.z - 10
        );
        transform.LookAt(followTransform); 
    }
}
