using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TireController : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] private GameObject AINavAgent = null;
    public Component[] wheels;
    private Rigidbody carRigidBody;
    private float accelerationInput = 0;
    private float suspensionRestDistance = 0.6f;
    private bool wheelsOnGround = false;
    private float rotationSpeed = 90;
    void Start()
    {
        carRigidBody = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {

        // 3 Forces on each tire
        // Y: Damped spring
        // X: Anti-slipping
        // Z: Acceleration
        if (AINavAgent == null)
        {
            accelerationInput = Input.GetAxis("Vertical");
        } else
        {
            Vector3 forward = transform.TransformDirection(Vector3.forward);
            Vector3 toOther = (AINavAgent.transform.position - transform.position).normalized;
            float dotProduct = Vector3.Dot(forward, toOther);
            accelerationInput = Mathf.Clamp(dotProduct, -0.2f, 0.6f);
            //Debug.Log(accelerationInput);
        }
        wheelsOnGround = false;
        for (int i = 0; i < wheels.Length; i++)
        {
            float tireFriction = 0.8f;
            if (Input.GetKey(KeyCode.Space))
            {
                tireFriction = 0.2f;
            }
            float springMaxLength = 1;
            float wheelRadius = 0.3f;
            Ray tireRay = new(wheels[i].transform.position, -wheels[i].transform.up);
            bool rayDidHit = Physics.Raycast(tireRay, out RaycastHit tireRayHit, springMaxLength + wheelRadius);
            //Bring ray out of spring class
            if (rayDidHit)
            {
                Debug.DrawRay(wheels[i].transform.position, -wheels[i].transform.up * tireRayHit.distance);
                TireSpringForce(carRigidBody, wheels[i], tireRayHit);
                TireSteerForce(carRigidBody, wheels[i], tireFriction);
                if (i < 2)
                {
                    TireForwardForce(carRigidBody, wheels[i]);
                }
                wheelsOnGround = true;
            }
        }

        if (!wheelsOnGround)
        {
            Vector3 rotation = new Vector3(
                Input.GetAxis("Vertical") * rotationSpeed * Time.deltaTime,
                Input.GetAxis("Horizontal") * rotationSpeed * Time.deltaTime,
                0
            );
            transform.Rotate(rotation);
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
        Vector3 steeringDirection = wheel.transform.forward;
        Vector3 tireWorldVelocity = carRigidBody.GetPointVelocity(wheel.transform.position);

        float steeringVelocity = Vector3.Dot(steeringDirection, tireWorldVelocity);
        float desiredVelocityChange = -steeringVelocity * tireGripFactor;
        float desiredAcceleration = desiredVelocityChange / Time.fixedDeltaTime;
        carRigidBody.AddForceAtPosition(desiredAcceleration * tireMass * steeringDirection, wheel.transform.position);
    }

    private void TireForwardForce(Rigidbody carRigidBody, Component wheel)
    {
        Vector3 accelDir = wheel.transform.right;
        float carSpeed = Vector3.Dot(transform.forward, carRigidBody.velocity);
        float topSpeed = 70;
        float torque = 6;

        if (accelerationInput != 0.0f && carSpeed < topSpeed)
        {
            Debug.DrawRay(wheel.transform.position, accelDir * torque * accelerationInput);
            carRigidBody.AddForceAtPosition(accelDir * torque * accelerationInput, wheel.transform.position);
        }
    }
}
