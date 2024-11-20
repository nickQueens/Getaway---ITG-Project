using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AIInput : MonoBehaviour
{
    private GameObject AINavAgent = null;
    private TireController tireController;
    private float accelerationInput;
    private float horizontalInput;
    private bool handbrakeOn;

    private void Awake()
    {
        tireController = GetComponent<TireController>();
        AINavAgent = transform.parent.Find("NavAgent").gameObject;
    }

    // Update is called once per frame
    void Update()
    {
        if (!tireController.IsServer) { return; }

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

        tireController.SetInputs(accelerationInput, horizontalInput, handbrakeOn);
    }
}
