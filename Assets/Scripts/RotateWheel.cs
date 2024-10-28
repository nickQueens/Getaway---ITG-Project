using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class RotateWheel : NetworkBehaviour
{
    [SerializeField] private GameObject AINavAgent = null;
    float maxSteerAngle = 22;
    float horizontalInput;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (AINavAgent != null)
        {
            // Get the vector from the AI car to the player car
            Transform parentTransform = GetComponentInParent<Transform>();
            Vector3 toPlayer = AINavAgent.transform.position - parentTransform.position;

            // Normalize the vectors
            toPlayer = toPlayer.normalized;
            Vector3 aiCarForward = parentTransform.forward.normalized;
            Vector3 aiCarLeft = -parentTransform.right.normalized;

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
            Debug.Log(horizontalInput);
        } else if (IsOwner)
        {
            horizontalInput = Input.GetAxis("Horizontal");
        }

        float steeringAngle = horizontalInput * maxSteerAngle;
        transform.localRotation = Quaternion.Euler(0, steeringAngle, 0);
    }
}
