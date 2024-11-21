using UnityEngine;

public class AIInput : MonoBehaviour
{
    private Transform targetTransform = null;
    private Vector3 targetPosition;
    private TireController tireController;

    private float accelerationInput;
    private float horizontalInput;
    private bool handbrakeOn = false;

    private float pursuitAcceleration = 0.6f;
    private float reverseAcceleration = -0.4f;
    private float reverseDistance = 9f;
    private float turnThreshold = 0.05f;

    private void Awake()
    {
        tireController = GetComponent<TireController>();
        targetTransform = transform.parent.Find("NavAgent");
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
        } else
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

        float angleToDirection = Vector3.SignedAngle(transform.forward, directionToTarget, Vector3.up);
        if (Mathf.Abs(dot) < (1 - turnThreshold))
        {
            if (angleToDirection > 0)
            {
                horizontalInput = 1 - dot;
            }
            else
            {
                horizontalInput = -1f;
            }
        }

        tireController.SetInputs(accelerationInput, horizontalInput, handbrakeOn);
    }
}
