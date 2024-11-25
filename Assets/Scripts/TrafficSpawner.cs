using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class TrafficSpawner : MonoBehaviour
{
    [SerializeField] public GameObject trafficPrefab;
    public int carsToSpawn;

    public void Start()
    {
        NetworkManager.Singleton.OnServerStarted += SpawnTrafficStart;
    }

    private void SpawnTrafficStart()
    {
        NetworkManager.Singleton.OnServerStarted -= SpawnTrafficStart;

        StartCoroutine(SpawnTraffic());
    }

    private void SpawnCar()
    {
        Transform childWaypoint = transform.GetChild(Random.Range(0, transform.childCount - 1));

        NetworkObject obj = NetworkObjectPool.Singleton.GetNetworkObject(trafficPrefab, childWaypoint.position, childWaypoint.rotation);
        obj.GetComponentInChildren<AIInput>().isTraffic = true;
        obj.GetComponentInChildren<AIInput>().isInPursuit = false;
        obj.GetComponentInChildren<AIInput>().currentWaypoint = childWaypoint.GetComponent<Waypoint>().nextWaypoint;
        if (!obj.IsSpawned) { obj.Spawn(true); }
    }

    IEnumerator SpawnTraffic()
    {
        int count = 0;
        while (count < carsToSpawn)
        {
            SpawnCar();

            yield return new WaitForEndOfFrame();
            count++;
        }
    }

}
