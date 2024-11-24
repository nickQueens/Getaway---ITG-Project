using Unity.Netcode.Components;
using UnityEngine;

public class AIInput : MonoBehaviour
{
    private Transform targetTransform = null;
    private Vector3 targetPosition;
    private TireController tireController;
    private new Rigidbody rigidbody;

    private float accelerationInput;
    private float horizontalInput;
    private bool handbrakeOn = false;

    private const float pursuitAcceleration = 0.6f;
    private const float reverseAcceleration = -0.4f;
    private const float reverseDistance = 9f;
    private const float turnThreshold = 0.02f;
    private const float maxSpeed = 30f;

    private void Awake()
    {
        tireController = GetComponent<TireController>();
        targetTransform = transform.parent.Find("NavAgent");
        rigidbody = transform.GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        if (!tireController.IsServer) { return; }

        targetPosition = targetTransform.position;
        float distanceToTarget = Vector3.Distance(transform.position, targetPosition);


        Vector3 directionToTarget = (targetPosition - transform.position).normalized;
        float dot = Vector3.Dot(transform.forward, directionToTarget);

        if (dot > 0)
        {
            // Target in front
            accelerationInput = pursuitAcceleration;
        }
        else
        {
            // Target behind
            if (distanceToTarget > reverseDistance)
            {
                accelerationInput = pursuitAcceleration;
            }
            else
            {
                accelerationInput = reverseAcceleration;
            }
        }
        accelerationInput = rigidbody.velocity.magnitude >= maxSpeed ? 0 : accelerationInput;

        float angleToDirection = Vector3.SignedAngle(transform.forward, directionToTarget, Vector3.up);
        if (Mathf.Abs(dot) < (1 - turnThreshold))
        {
            float speedFactor = Mathf.Clamp01(rigidbody.velocity.magnitude / maxSpeed);
            horizontalInput = angleToDirection > 0 ? 1 - (dot * speedFactor) : -(1 - (dot * speedFactor)); 

            horizontalInput = Mathf.Clamp(horizontalInput, -0.8f, 0.8f);
        }
        else
        {
            horizontalInput = 0;
        }

        // (High) Add collison avoidance
        // Use sphere/raycast to check for non-target objects ahead
        // If obstacle within certain distance, use arc of raycasts to choose new steering angle
        // Implement raycast cooldown, to allow steering to have effect

        // (Medium) Add logic for if stuck in wall
        // - Check collision
        // - Reverse with no horizontal input for a certain time
        // - Resume normal steering logic
        // - Do opposite if trying to reverse into wall

        // (Low) Add logic for deactivating after high-speed crashes
        // - Check collision
        // - If velocity over a certain threshold, deactivate
        // - Higher threshold for car-on-car collisions

        // The above means that we'll have 3 different movement scenarios, chasing, avoiding obstacles & reversing when stuck


        tireController.SetInputs(accelerationInput, horizontalInput, handbrakeOn);
    }
}
