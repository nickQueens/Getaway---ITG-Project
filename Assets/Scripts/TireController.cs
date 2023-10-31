using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TireController : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
        // 3 Forces on each tire
        // Y: Damped spring
        // X: Anti-slipping
        // Z: Acceleration
    }

    private void TireSpringForce(Rigidbody carRigidBody, Transform tireTransform)
    {
        float springStrength = 5;
        float springDamping = 2;
        float suspensionRestDistance = 2;
        RaycastHit tireRay;


        // Damped spring for suspension
        Vector3 springDirection = tireTransform.up;
        Vector3 tireVelocity = carRigidBody.GetPointVelocity(tireTransform.position);

        float offset = suspensionRestDistance - tireRay.distance;
        float yVelocity = Vector3.Dot(springDirection, tireVelocity);
        float yForce = (offset * springStrength) - (yVelocity * springDamping);

        carRigidBody.AddForceAtPosition(springDirection* yForce, tireTransform.position);
    }
}
