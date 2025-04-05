using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventsManager : MonoBehaviour
{
    [SerializeField] private List<GameObject> eventsToSummon = new List<GameObject>();
    [SerializeField] private float delayBeforeFirstEvent;
    [SerializeField] private float timeBetweenEvents;
    [SerializeField] private float chancesForEachSpawnpointToSpawnAPrefab;
    [SerializeField] private GameObject spawnBeacon;
    [SerializeField] private float beaconMaxIntensity = 600f;
    [SerializeField] private float beaconDurationBeforePrefabSpawn = 3f;

    private List<GameObject> spawnpoints = new List<GameObject>();
    private float nextEventTime;
    private GameObject selectedEvent;

    void Start()
    {
        foreach (Transform child in transform)
        {
            spawnpoints.Add(child.gameObject);
        }
        foreach(GameObject eventToSummon in eventsToSummon)
        {
            eventToSummon.SetActive(false);
        }
        nextEventTime = Time.time + delayBeforeFirstEvent;
    }

    void Update()
    {
        if (eventsToSummon.Count>0 && Time.time >= nextEventTime) 
        {
            nextEventTime = Time.time + timeBetweenEvents;

            foreach (GameObject spawnpoint in spawnpoints) // On parcourt tous les points de spawn, et on tire aléatoirement ceux qui spawnent une structure
            {
                if(Random.Range(0f, 1f)<= chancesForEachSpawnpointToSpawnAPrefab)
                {
                    selectedEvent=eventsToSummon[Random.Range(0, eventsToSummon.Count)];

                    StartCoroutine(SummonEvent(selectedEvent, spawnpoint.transform.position, spawnpoint.transform.rotation));
                    eventsToSummon.Remove(selectedEvent);
                }
            }
        }
    }

    private IEnumerator SummonEvent(GameObject eventToSummon, Vector3 loc, Quaternion rot)
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

        print(eventToSummon);
        eventToSummon.transform.position = loc;
        eventToSummon.transform.rotation = rot;
        eventToSummon.SetActive(true);
    }
}
