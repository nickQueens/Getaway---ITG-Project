using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Components;
using Unity.VisualScripting;
using UnityEngine;

public class AIInput : NetworkBehaviour
{
    [SerializeField]
    public bool isPolice = false;
    [SerializeField]
    public bool isInPursuit = false;
    [SerializeField]
    public bool isTraffic = true;
    [SerializeField]
    public Waypoint currentWaypoint;

    [ContextMenu("Start pursuit")]
    private void TriggerStartPursuit()
    {
        StartPursuit();
    }

    [ContextMenu("End pursuit")]
    private void TriggerEndPursuit()
    {
        EndPursuit();
    }

    private Transform targetTransform;
    private TireController tireController;
    private new Rigidbody rigidbody;

    private float accelerationInput;
    private float horizontalInput;
    private bool handbrakeOn = false;

    private const float pursuitAcceleration = 0.6f;
    private const float trafficAcceleration = 0.4f;
    private const float reverseAcceleration = -0.4f;
    private const float reverseDistance = 9f;
    private const float turnThreshold = 0.02f;
    private const float pursuitMaxSpeed = 15;
    private const float trafficMaxSpeed = 6f;
    private const float junctionMaxSpeed = 2f;
    private const float pursuitStartDistance = 20f;
    private const float pursuitEndDistance = 100f;

    private GameObject targetPlayer;
    private float distanceToPlayer = float.MaxValue;

    private float currentMaxSpeed;
    private float currentAcceleration;

    private void Initialise()
    {
        tireController = GetComponent<TireController>();
        rigidbody = transform.GetComponent<Rigidbody>();

        targetTransform = isInPursuit ? transform.parent.Find("NavAgent")
            : isTraffic ? currentWaypoint.transform : null;
        currentMaxSpeed = isInPursuit ? pursuitMaxSpeed : trafficMaxSpeed;
        currentAcceleration = isInPursuit ? pursuitAcceleration : trafficAcceleration;
    }
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        Initialise();
    }

    void FixedUpdate()
    {
        if (!NetworkManager.Singleton.IsServer) { return; }

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

        FollowTarget(targetTransform);

        if (isTraffic)
        {
            UpdateWaypoints(currentWaypoint);
        }

        if (isPolice)
        {
            StartOrEndPursuit();
        }
    }

    private void FollowTarget (Transform targetTransform)
    {
        if (targetTransform == null) {
            tireController.SetInputs(0, 0, false);
            return;
        }
        Vector3 targetPosition = targetTransform.position;
        float distanceToTarget = Vector3.Distance(transform.position, targetPosition);
        Vector3 directionToTarget = (targetPosition - transform.position).normalized;
        float dot = Vector3.Dot(transform.forward, directionToTarget);

        int invertSteering = 1;
        // Need to model and control horizontal and acceleration inputs together,
        // For u-turns, reversing, etc.
        if (dot > 0)
        {
            // Target in front
            accelerationInput = currentAcceleration;
        }
        else
        {
            // Target behind
            if (distanceToTarget > reverseDistance)
            {
                accelerationInput = currentAcceleration;
                // If target is behind and not reversing, ie. u-turn, need to make horizontal input full 1 or -1
            }
            else
            {
                accelerationInput = reverseAcceleration;
                invertSteering = -1;
            }
        }
        accelerationInput = rigidbody.velocity.magnitude >= currentMaxSpeed ? 0 : accelerationInput;

        if (Mathf.Abs(dot) < (1 - turnThreshold))
        {
            float angleToDirection = Vector3.SignedAngle(transform.forward, directionToTarget, Vector3.up);
            float speedFactor = Mathf.Clamp01(rigidbody.velocity.magnitude / pursuitMaxSpeed);
            horizontalInput = angleToDirection > 0 ? 1 - (dot * speedFactor) : -(1 - (dot * speedFactor));

            horizontalInput = Mathf.Clamp(horizontalInput, -0.8f, 0.8f);
        }
        else
        {
            horizontalInput = 0;
        }
        horizontalInput *= invertSteering;

        tireController.SetInputs(accelerationInput, horizontalInput, handbrakeOn);
    }

    private void UpdateWaypoints(Waypoint currentWaypoint)
    {
        if (currentWaypoint == null) {  return; }
        float distance = Vector3.Distance(transform.position, currentWaypoint.transform.position);
        if (distance < currentWaypoint.width)
        {
            bool shouldBranch = false;

            if (currentWaypoint.branches != null && currentWaypoint.branches.Count > 0)
            {
                shouldBranch = Random.Range(0f, 1f) <= currentWaypoint.branchingChance;
            }

            if (shouldBranch)
            {
                currentWaypoint = currentWaypoint.branches[Random.Range(0, currentWaypoint.branches.Count - 1)];
            }

            if ((currentWaypoint.nextWaypoint.branches != null && currentWaypoint.nextWaypoint.branches.Count > 0)
                || currentWaypoint.branches != null && currentWaypoint.branches.Count > 0)
            {
                currentMaxSpeed = junctionMaxSpeed;
            } else
            {
                currentMaxSpeed = trafficMaxSpeed;
            }

            this.currentWaypoint = currentWaypoint.nextWaypoint;
            targetTransform = this.currentWaypoint.transform;
        }
    }

    public void StartOrEndPursuit()
    {
        if (!isInPursuit)
        {
            var allPlayers = GameObject.FindGameObjectsWithTag("Player");
            if (allPlayers != null && allPlayers.Length > 0)
            {
                foreach (var player in allPlayers)
                {
                    var d = Vector3.Distance(player.transform.position,transform.position);
                    Debug.Log("Distance to player: " + d);
                    if (d < distanceToPlayer)
                    {
                        targetPlayer = player;
                        distanceToPlayer = d;
                    }
                }
                if (distanceToPlayer < pursuitStartDistance)
                {
                    Debug.Log(allPlayers.Length);
                    Debug.Log(allPlayers);
                    Debug.Log(targetPlayer.name);
                    Debug.Log(distanceToPlayer);
                    StartPursuit();
                }
            }
        }
        else
        {
            distanceToPlayer = Vector3.Distance(targetPlayer.transform.position, transform.position);
            if (distanceToPlayer > pursuitEndDistance)
            {
                EndPursuit();
            }
        }
    }

    public void StartPursuit()
    {
        isInPursuit = true;
        isTraffic = false;

        currentMaxSpeed = pursuitMaxSpeed;
        currentAcceleration = pursuitAcceleration;

        targetTransform = transform.parent.Find("NavAgent");
    }

    public void EndPursuit()
    {
        isInPursuit = false;
        isTraffic = true;

        currentMaxSpeed = trafficMaxSpeed;
        currentAcceleration = trafficAcceleration;

        List<Waypoint> availableWaypoints = new List<Waypoint>(FindObjectsByType<Waypoint>(FindObjectsSortMode.None));
        float distanceToClosestWaypoint = float.MaxValue;
        Waypoint closestWaypoint = null;
        foreach (Waypoint waypoint in availableWaypoints)
        {
            float d = (transform.position - waypoint.transform.position).sqrMagnitude;

            if (d < distanceToClosestWaypoint)
            {
                closestWaypoint = waypoint;
                distanceToPlayer = d;
            }
        }

        targetTransform = closestWaypoint.transform;
    }
}
