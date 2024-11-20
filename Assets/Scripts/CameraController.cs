using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform followTransform;
    public float cameraHeight;

    void LateUpdate()
    {
        if (!followTransform ) { return; }

        transform.position = new Vector3
        (
            followTransform.position.x,
            followTransform.position.y + cameraHeight,
            followTransform.position.z - 10
        );
        transform.LookAt(followTransform); 
    }
}
