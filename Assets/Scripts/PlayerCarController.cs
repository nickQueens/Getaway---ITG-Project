using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCarController : MonoBehaviour
{

    private float forwardInput;
    private float horizontalInput;

    private float forwardAcceleration = 1;
    private float forwardSpeed = 0;
    private float dragCoefficient = 0.04f;
    private float minimumSpeed = 0.5f;

    private float turnSpeed = 50;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        forwardInput = Input.GetAxis("Vertical");
        float drag = forwardSpeed * -dragCoefficient;
        float acceleration = forwardInput * forwardAcceleration;
        forwardSpeed += acceleration + drag;
        transform.Translate(Vector3.forward * forwardSpeed * Time.deltaTime);

        if (forwardSpeed != 0)
        {
            // This feels unnatural when playing
            // ToDo: Use physics to add sideways force to the front of the car to turn
            horizontalInput = Input.GetAxis("Horizontal");
            transform.Rotate(Vector3.up, turnSpeed * horizontalInput * Time.deltaTime);

            if ((Mathf.Abs(forwardSpeed) < minimumSpeed) && (forwardInput == 0))
            {
                forwardSpeed = 0;
            }
        }

        Debug.Log("Car speed: " + forwardSpeed);
    }

}
