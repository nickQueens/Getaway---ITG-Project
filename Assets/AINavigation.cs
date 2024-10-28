using System.Collections;
using System.Collections.Generic;
using Unity.Burst.Intrinsics;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using static UnityEditor.PlayerSettings;

public class AINavigation : NetworkBehaviour
{
    NavMeshAgent agent;
    private float maxDistanceFromParent = 50;
    [SerializeField] GameObject targetObject;
    [SerializeField] GameObject parentObject;
    private float distanceToPlayer = float.PositiveInfinity;
    public Vector3 directionToTarget;
    // Start is called before the first frame update
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        if (targetObject == null)
        {
            targetObject = GameObject.Find("Player Car");
        }
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (!IsServer) { return; }
        var allPlayers = GameObject.FindGameObjectsWithTag("Player");
        Debug.Log(allPlayers);

        foreach (var player in allPlayers)
        {
            var d = (player.transform.position - transform.position).sqrMagnitude;
            if (d < distanceToPlayer)
            {
                targetObject = player;
                distanceToPlayer = d;
            }
        }

        if (targetObject == null) { return; }
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
