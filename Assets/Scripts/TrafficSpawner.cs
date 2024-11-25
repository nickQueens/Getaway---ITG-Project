using Unity.Netcode;
using UnityEngine;

public class TrafficSpawner : MonoBehaviour
{
    [SerializeField] public GameObject trafficPrefab;
    public int carsToSpawn;

    private WaitForSeconds _waitForSeconds = new WaitForSeconds(2f);

    public void Start()
    {
        NetworkManager.Singleton.OnServerStarted += SpawnTrafficStart;
    }

    private void SpawnTrafficStart()
    {
        NetworkManager.Singleton.OnServerStarted -= SpawnTrafficStart;

        for (int i = 0; i < carsToSpawn; i++)
        {
            SpawnTraffic();
        }

    }

    private void SpawnTraffic()
    {
        Transform childWaypoint = transform.GetChild(Random.Range(0, transform.childCount - 1));

        NetworkObject obj = NetworkObjectPool.Singleton.GetNetworkObject(trafficPrefab, childWaypoint.position, childWaypoint.rotation);
        Debug.Log(obj);
        Debug.Log(obj.GetComponentInChildren<AIInput>());
        obj.GetComponentInChildren<AIInput>().isTraffic = true;
        obj.GetComponentInChildren<AIInput>().isInPursuit = false;
        obj.GetComponentInChildren<AIInput>().currentWaypoint = childWaypoint.GetComponent<Waypoint>().nextWaypoint;
        if (!obj.IsSpawned) { obj.Spawn(true); }
    }

    //IEnumerator Spawn()
    //{
    //    int count = 0;
    //    while (count < carsToSpawn)
    //    {
    //        //GameObject obj = NetworkManager.SpawnManager.(trafficPrefab);
    //        //var objNetworkObject = obj.GetComponent<NetworkObject>();
    //        //objNetworkObject.Spawn();
    //        //Debug.Log(objNetworkObject);
    //        //Transform child = transform.GetChild(Random.Range(0, transform.childCount - 1));
    //        //objNetworkObject.GetComponent<AIInput>().currentWaypoint = child.GetComponent<Waypoint>();
    //        //objNetworkObject.transform.position = child.position;

    //        yield return new WaitForEndOfFrame();

    //        count++;
    //    }
    //}

}
