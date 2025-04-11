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
    [SerializeField] private float beaconMaxRange = 100f;
    [SerializeField] private float beaconDurationBeforePrefabSpawn = 3f;
    [SerializeField] private Vector3 beaconEndScale = new Vector3(15f,15f,15f);

    private List<GameObject> spawnpoints = new List<GameObject>();
    private float nextEventTime;
    private GameObject selectedEvent;

    void Start()
    {
        foreach (Transform child in transform)
        {
            spawnpoints.Add(child.gameObject);
        }

        nextEventTime = Time.time + delayBeforeFirstEvent;
    }

    void Update()
    {
        if (eventsToSummon.Count > 0 && Time.time >= nextEventTime)
        {
            nextEventTime = Time.time + timeBetweenEvents;

            foreach (GameObject spawnpoint in spawnpoints) // On parcourt tous les points de spawn, et on tire aléatoirement ceux qui spawnent une structure
            {
                if (Random.Range(0f, 1f) <= chancesForEachSpawnpointToSpawnAPrefab)
                {
                    selectedEvent = eventsToSummon[Random.Range(0, eventsToSummon.Count)];
                    StartCoroutine(SummonEvent(selectedEvent, spawnpoint.transform.position, spawnpoint.transform.rotation));
                    eventsToSummon.Remove(selectedEvent);
                }
            }
        }
        else if (eventsToSummon.Count <= 0)
        {
            enabled = false;
        }
    }

    private IEnumerator SummonEvent(GameObject eventToSummon, Vector3 loc, Quaternion rot)
    {
        GameObject beaconInstance = Instantiate(spawnBeacon, loc, rot);
        Rigidbody sphere = beaconInstance.GetComponentInChildren<Rigidbody>();
        beaconInstance.transform.position += new Vector3(Random.Range(2, 5), 0, Random.Range(2, 5)); // Décale le collider pour éviter de soulever les objets

        Light beaconLight = beaconInstance.GetComponentInChildren<Light>();
        float elapsedTime = 0f;

        if (beaconLight)
        {
            //beaconLight.intensity = 100f;
            beaconLight.range = 0f;

            while (elapsedTime < beaconDurationBeforePrefabSpawn)
            {
                elapsedTime += Time.deltaTime;
                //beaconLight.intensity = Mathf.Lerp(100f, beaconMaxIntensity, elapsedTime / beaconDurationBeforePrefabSpawn);
                beaconLight.range = Mathf.Lerp(0f, eventToSummon.GetComponent<Diametre>().diametre/2f, elapsedTime / beaconDurationBeforePrefabSpawn);
                sphere.GetComponent<SphereCollider>().radius = Mathf.Lerp(0, eventToSummon.GetComponent<Diametre>().diametre / 2f, elapsedTime / beaconDurationBeforePrefabSpawn);
                yield return new WaitForSeconds(Time.deltaTime);
            }
        }

        beaconInstance.SetActive(false);
     
        yield return new WaitForEndOfFrame();
        eventToSummon.transform.position = beaconInstance.transform.position;
        eventToSummon.transform.rotation = Quaternion.identity;
        eventToSummon.SetActive(true);
        Destroy(beaconInstance);
    }
}
