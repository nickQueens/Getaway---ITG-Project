using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class TrafficSpawner : MonoBehaviour
{
    [SerializeField] public List<GameObject> trafficPrefabs;
    [SerializeField] private GameObject policeCarPrefab;
    public int carsToSpawn;

    private List<Waypoint> availableWaypoints;

    public void Start()
    {
        NetworkManager.Singleton.OnServerStarted += SpawnTrafficStart;
    }

    private void SpawnTrafficStart()
    {
        NetworkManager.Singleton.OnServerStarted -= SpawnTrafficStart;

        StartCoroutine(SpawnTraffic());
    }

    IEnumerator SpawnTraffic()
    {
        availableWaypoints = new List<Waypoint>(transform.GetComponentsInChildren<Waypoint>());
        int count = 0;
        while (count < carsToSpawn)
        {
            if (availableWaypoints.Count > 0)
            {
                SpawnCar();
            }

            yield return new WaitForEndOfFrame();
            count++;
        }

        // Gaurantees a police car will be spawned if provided
        if (policeCarPrefab != null)
        {
            SpawnPoliceCar(availableWaypoints);
        }
    }

    private void SpawnCar()
    {
        int waypointIndex = Random.Range(0, availableWaypoints.Count);
        Transform childWaypoint = availableWaypoints[waypointIndex].transform;

        availableWaypoints.RemoveAt(waypointIndex);

        int trafficPrefabIndex = Random.Range(0, trafficPrefabs.Count);

        NetworkObject obj = NetworkObjectPool.Singleton.GetNetworkObject(trafficPrefabs[trafficPrefabIndex], childWaypoint.position, childWaypoint.rotation);
        obj.GetComponentInChildren<AIInput>().isTraffic = true;
        obj.GetComponentInChildren<AIInput>().isInPursuit = false;
        obj.GetComponentInChildren<AIInput>().currentWaypoint = childWaypoint.GetComponent<Waypoint>();
        if (!obj.IsSpawned) { obj.Spawn(true); }
    }

    private void SpawnPoliceCar(List<Waypoint> availableWaypoints)
    {
        int waypointIndex = Random.Range(0, availableWaypoints.Count);
        Transform childWaypoint = availableWaypoints[waypointIndex].transform;

        availableWaypoints.RemoveAt(waypointIndex);

        NetworkObject obj = NetworkObjectPool.Singleton.GetNetworkObject(policeCarPrefab, childWaypoint.position, childWaypoint.rotation);
        obj.GetComponentInChildren<AIInput>().isTraffic = true;
        obj.GetComponentInChildren<AIInput>().isInPursuit = false;
        obj.GetComponentInChildren<AIInput>().currentWaypoint = childWaypoint.GetComponent<Waypoint>();
        if (!obj.IsSpawned) { obj.Spawn(true); }
    }

}
