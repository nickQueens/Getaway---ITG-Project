using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

public class AINavigation : NetworkBehaviour
{
    NavMeshAgent agent;
    private float maxDistanceFromParent = 15;
    [SerializeField] GameObject targetObject;
    [SerializeField] GameObject parentObject;
    private float distanceToPlayer = float.PositiveInfinity;
    public Vector3 directionToTarget;
    private void Initialise()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.enabled = false;
        if (targetObject == null)
        {
            targetObject = GameObject.Find("Player Car");
        }

        if (NavMesh.SamplePosition(transform.position, out NavMeshHit hit, 2.0f, NavMesh.AllAreas))
        {
            transform.position = hit.position;
        }
        else
        {
            Debug.LogError($"NavAgent on {transform.parent.name} couldn't find mesh.");
        }
        agent.enabled = true;
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        Initialise();
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (!IsServer) { return; }
        if (!transform.parent.GetComponent<NetworkObject>().IsSpawned) { return; }
        var allPlayers = GameObject.FindGameObjectsWithTag("Player");

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
        transform.LookAt(targetObject.transform.position);
    }
}
