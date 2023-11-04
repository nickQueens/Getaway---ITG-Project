using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TireController : MonoBehaviour
{
    // Start is called before the first frame update
    public Component[] wheels;
    private Rigidbody carRigidBody;
    private float accelerationInput;
    private float suspensionRestDistance = 0.6f;

    void Start()
    {
        carRigidBody = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {

        // 3 Forces on each tire
        // Y: Damped spring
        // X: Anti-slipping
        // Z: Acceleration
        accelerationInput = Input.GetAxis("Vertical");

        for (int i = 0; i < wheels.Length; i++)
        {
            float springMaxLength = 1;
            float wheelRadius = 0.3f;
            Ray tireRay = new(wheels[i].transform.position, -wheels[i].transform.up);
            bool rayDidHit = Physics.Raycast(tireRay, out RaycastHit tireRayHit, springMaxLength + wheelRadius);
            //Bring ray out of spring class
            if (rayDidHit)
            {
                Debug.DrawRay(wheels[i].transform.position, -wheels[i].transform.up * tireRayHit.distance);
                float tireFriction = 0.3f;
                TireSpringForce(carRigidBody, wheels[i], tireRayHit);
                TireSteerForce(carRigidBody, wheels[i], tireFriction);
                if (i < 2)
                {
                    TireForwardForce(carRigidBody, wheels[i]);
                }
            }
        }
    }

    private void TireSpringForce(Rigidbody carRigidBody, Component wheel, RaycastHit tireRayHit)
    {
        float springStrength = 15;
        float springDamping = 2;

        // Damped spring for suspension
        Vector3 springDirection = wheel.transform.up;
        Vector3 tireVelocity = carRigidBody.GetPointVelocity(wheel.transform.position);

        float offset = suspensionRestDistance - tireRayHit.distance;
        float yVelocity = Vector3.Dot(springDirection, tireVelocity);
        float yForce = (offset * springStrength) - (yVelocity * springDamping);

        carRigidBody.AddForceAtPosition(springDirection * yForce, wheel.transform.position);
    }

    private void TireSteerForce(Rigidbody carRigidBody, Component wheel, float tireGripFactor)
    {
        float tireMass = 0.05f;
        Vector3 steeringDirection = wheel.transform.right;
        Vector3 tireWorldVelocity = carRigidBody.GetPointVelocity(wheel.transform.position);

        float steeringVelocity = Vector3.Dot(steeringDirection, tireWorldVelocity);
        float desiredVelocityChange = -steeringVelocity * tireGripFactor;
        float desiredAcceleration = desiredVelocityChange / Time.fixedDeltaTime;
        carRigidBody.AddForceAtPosition(desiredAcceleration * tireMass * steeringDirection, wheel.transform.position);
    }

    private void TireForwardForce(Rigidbody carRigidBody, Component wheel)
    {
        Vector3 accelDir = wheel.transform.forward;
        float carSpeed = Vector3.Dot(transform.forward, carRigidBody.velocity);
        float topSpeed = 70;
        float torque = 4;

        if (accelerationInput != 0.0f && carSpeed < topSpeed)
        {
            carRigidBody.AddForceAtPosition(accelDir * torque * accelerationInput, wheel.transform.position);
        }
    }
}
