using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TireController : NetworkBehaviour
{
    // Start is called before the first frame update
    [SerializeField] private GameObject AINavAgent = null;
    public Component[] wheels;
    private Rigidbody carRigidBody;
    private float accelerationInput = 0;
    private float horizontalInput = 0;
    private bool handbrakeOn = false;
    private float suspensionRestDistance = 0.6f;
    private bool wheelsOnGround = false;
    private float rotationSpeed = 90;
    private float maxSteerAngle = 22; 
    public override void OnNetworkSpawn()
    {
        Debug.Log("Car spawned!");
        carRigidBody = GetComponent<Rigidbody>();

        if (AINavAgent == null && IsOwner)
        {
            Debug.Log("Not AI");
            GameObject.Find("FollowCamera").GetComponent<CameraController>().followTransform = transform;
        }
        
    }

    private void Update()
    {
        if (IsServer && Input.GetKeyDown(KeyCode.R))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }

    [ServerRpc]
    private void MovePlayerServerRpc(float accelerationInput, float horizontalInput, bool handbrakeOn)
    {
        MovePlayer(accelerationInput, horizontalInput, handbrakeOn);
    }

    private void MovePlayer(float accelerationInput, float horizontalInput, bool handbrakeOn)
    {
        wheelsOnGround = false;
        for (int i = 0; i < wheels.Length; i++)
        {
            float tireFriction = 0.8f;
            if (handbrakeOn)
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
                    TireForwardForce(accelerationInput, carRigidBody, wheels[i]);

                    float steeringAngle = horizontalInput * maxSteerAngle;
                    wheels[i].transform.localRotation = Quaternion.Euler(0, steeringAngle, 0);
                }
                wheelsOnGround = true;
            }
        }

        if (!wheelsOnGround && AINavAgent == null)
        {
            Vector3 rotation = new Vector3(
                accelerationInput * rotationSpeed * Time.deltaTime,
                horizontalInput * rotationSpeed * Time.deltaTime,
                0
            );
            transform.Rotate(rotation);
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (!IsOwner || (!Application.isFocused && AINavAgent == null)) { return; }

        // Get Input
        if (AINavAgent == null)
        {
            accelerationInput = Input.GetAxis("Vertical");
            horizontalInput = Input.GetAxis("Horizontal");
            handbrakeOn = Input.GetKey(KeyCode.Space);
        } else if (IsServer)
        {
            // AI Input
            // Acceleration
            Vector3 forward = transform.TransformDirection(Vector3.forward);
            Vector3 toOther = (AINavAgent.transform.position - transform.position).normalized;
            float dotProduct = Vector3.Dot(forward, toOther);
            accelerationInput = Mathf.Clamp(dotProduct, -0.2f, 0.6f);

            // Horizontal
            // Get the vector from the AI car to the player car
            Vector3 toPlayer = AINavAgent.transform.position - transform.position;

            // Normalize the vectors
            toPlayer = toPlayer.normalized;
            Vector3 aiCarForward = transform.forward.normalized;
            Vector3 aiCarLeft = -transform.right.normalized;

            // Take the dot product of the vectors
            float dotForward = Vector3.Dot(toPlayer, aiCarForward);
            float dotLeft = Vector3.Dot(toPlayer, aiCarLeft);

            // Return a value between -1 and 1 based on the dot product
            if (dotForward > 0) // Player car is in front of AI car
            {
                horizontalInput = dotLeft; // Return the dot product with the left vector
            }
            else // Player car is behind AI car
            {
                horizontalInput = -dotLeft; // Return the negative dot product with the left vector
            }
        }

        MovePlayerServerRpc(accelerationInput, horizontalInput, handbrakeOn);
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

    private void TireForwardForce(float accelerationInput, Rigidbody carRigidBody, Component wheel)
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
