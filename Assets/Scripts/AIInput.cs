using Unity.Netcode.Components;
using UnityEngine;

public class AIInput : MonoBehaviour
{
    private Transform targetTransform = null;
    private Vector3 targetPosition;
    private TireController tireController;
    private Rigidbody rigidbody;

    private float accelerationInput;
    private float horizontalInput;
    private bool handbrakeOn = false;

    private const float pursuitAcceleration = 0.6f;
    private const float reverseAcceleration = -0.4f;
    private const float reverseDistance = 9f;
    private const float turnThreshold = 0.05f;
    private const float maxSpeed = 50f;
    private const float speedSteeringReductionFactor = 0.5f;

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
            horizontalInput = angleToDirection > 0 ? 1 - dot : -(1 - dot); 

            horizontalInput = Mathf.Clamp(horizontalInput, -0.8f, 0.8f);
            float speedFactor = Mathf.Clamp01(rigidbody.velocity.magnitude / maxSpeed); // Normalize speed
            horizontalInput *= (1f - speedFactor * speedSteeringReductionFactor);
        }

        tireController.SetInputs(accelerationInput, horizontalInput, handbrakeOn);
    }
}
