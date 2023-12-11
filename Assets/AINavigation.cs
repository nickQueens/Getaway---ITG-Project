using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AINavigation : MonoBehaviour
{
    NavMeshAgent agent;
    private float maxDistanceFromParent = 50;
    [SerializeField] GameObject targetObject;
    [SerializeField] GameObject parentObject;
    public Vector3 directionToTarget;
    // Start is called before the first frame update
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (Vector3.Distance(transform.position, parentObject.transform.position) < maxDistanceFromParent)
        {
            agent.SetDestination(targetObject.transform.position);
        } else
        {
            agent.SetDestination(parentObject.transform.position);
        }
        directionToTarget = (targetObject.transform.position - transform.position).normalized;
        Debug.DrawLine(transform.position, directionToTarget);
        transform.LookAt(targetObject.transform.position);
    }
}
