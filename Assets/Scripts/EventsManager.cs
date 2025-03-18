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

    private List<GameObject> spawnpoints;
    private float nextEventTime;
    private bool prefabSpawned;

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

            prefabSpawned = false;

            foreach (GameObject spawnpoint in spawnpoints)
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
        print("caca");
        // Instancier le beacon sans collision
        GameObject beaconInstance = Instantiate(spawnBeacon, loc, rot);

        // Supprimer son collider s'il en a un (au cas où)
        Collider beaconCollider = beaconInstance.GetComponent<Collider>();
        if (beaconCollider) Destroy(beaconCollider);

        yield return new WaitForSeconds(2); // Attendre 2 secondes

        // Détruire le beacon
        Destroy(beaconInstance);

        // Instancier le vrai prefab
        Instantiate(prefab, loc, rot);

        yield return null;
    }
}
