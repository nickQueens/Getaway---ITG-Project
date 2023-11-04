using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateWheel : MonoBehaviour
{
    float maxSteerAngle = 22;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        float horizontalInput = Input.GetAxis("Horizontal");

        float steeringAngle = horizontalInput * maxSteerAngle;
        transform.localRotation = Quaternion.Euler(0, steeringAngle, 0);
    }
}
