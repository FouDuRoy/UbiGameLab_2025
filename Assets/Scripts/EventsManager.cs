using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventsManager : MonoBehaviour
{
    [SerializeField] private List<GameObject> eventsPrefabs = new List<GameObject>();
    [SerializeField] private float delayBeforeFirstEvent;
    [SerializeField] private float timeBetweenEvents;
    [SerializeField] private float chancesForEachSpawnpointToSpawnAPrefab;
    [SerializeField] private GameObject spawnBeacon;
    [SerializeField] private float beaconMaxIntensity = 600f;
    [SerializeField] private float beaconDurationBeforePrefabSpawn = 3f;

    private List<GameObject> spawnpoints;
    private float nextEventTime;

    void Start()
    {
        //On récupère tous les spawnpoints
        spawnpoints = new List<GameObject>();

        foreach (Transform child in transform)
        {
            spawnpoints.Add(child.gameObject);
        }

        nextEventTime = Time.time + delayBeforeFirstEvent;
    }

    void Update()
    {
        if (Time.time >= nextEventTime) 
        {
            nextEventTime = Time.time + timeBetweenEvents;

            foreach (GameObject spawnpoint in spawnpoints) // On parcourt tous les points de spawn, et on tire aléatoirement ceux qui spawnent une structure
            {
                if(Random.Range(0f, 1f)<= chancesForEachSpawnpointToSpawnAPrefab)
                {
                    StartCoroutine(SummonPrefab(eventsPrefabs[Random.Range(0, eventsPrefabs.Count)], spawnpoint.transform.position, spawnpoint.transform.rotation));
                }
            }
        }
    }

    private IEnumerator SummonPrefab(GameObject prefab, Vector3 loc, Quaternion rot)
    {
        GameObject beaconInstance = Instantiate(spawnBeacon, loc, rot);

        Collider beaconCollider = beaconInstance.GetComponent<Collider>();
        if (beaconCollider) Destroy(beaconCollider);

        Light beaconLight = beaconInstance.GetComponentInChildren<Light>();
        float elapsedTime = 0f;

        if (beaconLight)
        {
            beaconLight.intensity = 0f;

            while (elapsedTime < beaconDurationBeforePrefabSpawn)
            {
                elapsedTime += Time.deltaTime;
                beaconLight.intensity = Mathf.Lerp(0f, beaconMaxIntensity, elapsedTime / beaconDurationBeforePrefabSpawn);
                yield return null;
            }
        }

        Destroy(beaconInstance);
        Instantiate(prefab, loc, rot);
    }
}
