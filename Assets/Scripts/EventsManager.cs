using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventsManager : MonoBehaviour
{
    [SerializeField] private List<GameObject> eventsToSummonStructure = new List<GameObject>();
    [SerializeField] private List<GameObject> eventsToSummonPowerUp = new List<GameObject>();
    [SerializeField] private List<GameObject> eventsToSummonOther = new List<GameObject>();
    [SerializeField] private float delayBeforeFirstEvent;
    [SerializeField] private float timeBetweenEvents;
    [SerializeField] private float chancesForEachSpawnpointToSpawnAPrefab;
    [SerializeField] private GameObject spawnBeacon;
    [SerializeField] private float beaconMaxRange = 100f;
    [SerializeField] private float beaconDurationBeforePrefabSpawn = 3f;
    [SerializeField] private Vector3 beaconEndScale = new Vector3(15f, 15f, 15f);
    [SerializeField] private float[] probArraySpawns;
    [SerializeField] private float[] probArrayPools;
    public int maxNumberStructure = 3;
    public int maxNumberPowerUp = 3;
    public int maxNumberOther = 3;
    private List<GameObject> spawnpoints = new List<GameObject>();
    private float nextEventTime;
    private GameObject selectedEvent;
    private SpawnChanceDistribution spawnChancesDist;
    private SpawnChanceDistribution structureTypeDist;
    private int numberOfStructure = 0;
    private int numberOfOther = 0;
    private int numberOfPowerUp = 0;

    void Start()
    {
        foreach (Transform child in transform)
        {
            spawnpoints.Add(child.gameObject);
        }
        nextEventTime = Time.time + delayBeforeFirstEvent;
        spawnChancesDist = new SpawnChanceDistribution(probArraySpawns);
        structureTypeDist = new SpawnChanceDistribution(probArrayPools);
    }

    void Update()
    {
        int totalEventsCount = maxNumberPowerUp + maxNumberOther + maxNumberStructure - numberOfOther - numberOfPowerUp - numberOfStructure;
        if (totalEventsCount > 0 && Time.time >= nextEventTime)
        {
            nextEventTime = Time.time + timeBetweenEvents;
            int numberOfSpawns = spawnChancesDist.sampleDistribution();
            List<GameObject> spawnAvailable = AvailableSpawns();
            numberOfSpawns = Mathf.Min(numberOfSpawns, spawnAvailable.Count);

            for (int i = 0; i < numberOfSpawns; i++) // On parcourt tous les points de spawn, et on tire aléatoirement ceux qui spawnent une structure
            {

                int j = structureTypeDist.sampleDistribution();
                if (j == 0)
                {
                    GameObject item = eventsToSummonStructure[Random.Range(0, eventsToSummonStructure.Count)];
                    StartCoroutine(SummonEvent(item, spawnAvailable[i].transform.position, Quaternion.identity));
                    numberOfStructure++;
                }
                if (j == 1)
                {
                    GameObject item = eventsToSummonPowerUp[Random.Range(0, eventsToSummonPowerUp.Count)];
                    StartCoroutine(SummonEvent(item, spawnAvailable[i].transform.position, Quaternion.identity));
                    numberOfPowerUp++;
                }
                if (j == 2)
                {
                    GameObject item = eventsToSummonOther[Random.Range(0, eventsToSummonOther.Count)];
                    StartCoroutine(SummonEvent(item, spawnAvailable[i].transform.position, Quaternion.identity));
                    numberOfOther++;
                }
                redistribute();
            }
        }
        else if (totalEventsCount <= 0)
        {
            enabled = false;
        }
    }

    public void redistribute()
    {
        if (numberOfStructure >= maxNumberStructure && probArrayPools[0] != 0)
        {
            int nonZero = 0;
            float proba = probArrayPools[0];
            probArrayPools[0] = 0;
            foreach (float prob in probArrayPools)
            {
                if (prob != 0)
                {
                    nonZero++;

                }
            }
            if (nonZero > 0)
            {
                for (int i = 0; i < probArrayPools.Length; i++)
                {
                    if (probArrayPools[i] != 0)
                    {
                        probArrayPools[i] += proba / nonZero;
                    }
                }
            }
            structureTypeDist = new SpawnChanceDistribution(probArrayPools);

        }
        else if (numberOfPowerUp >= maxNumberPowerUp && probArrayPools[1] != 0)
        {
            int nonZero = 0;
            float proba = probArrayPools[1];
            probArrayPools[1] = 0;
            foreach (float prob in probArrayPools)
            {
                if (prob != 0)
                {
                    nonZero++;

                }
            }
            if (nonZero > 0)
            {
                for (int i = 0; i < probArrayPools.Length; i++)
                {
                    if (probArrayPools[i] != 0)
                    {
                        probArrayPools[i] += proba / nonZero;
                    }
                }
            }
            structureTypeDist = new SpawnChanceDistribution(probArrayPools);
        }
        else if (numberOfOther >= maxNumberOther && probArrayPools[2] != 0)
        {
            int nonZero = 0;
            float proba = probArrayPools[2];
            probArrayPools[2] = 0;
            foreach (float prob in probArrayPools)
            {
                if (prob != 0)
                {
                    nonZero++;

                }
            }
            if (nonZero > 0)
            {
                for (int i = 0; i < probArrayPools.Length; i++)
                {
                    if (probArrayPools[i] != 0)
                    {
                        probArrayPools[i] += proba / nonZero;
                    }
                }
            }
            structureTypeDist = new SpawnChanceDistribution(probArrayPools);
        }

    }
    private IEnumerator SummonEvent(GameObject eventToSummon, Vector3 loc, Quaternion rot)
    {
        GameObject beaconInstance = Instantiate(spawnBeacon, loc, rot);
        Rigidbody sphere = beaconInstance.GetComponentInChildren<Rigidbody>();

        Light beaconLight = beaconInstance.GetComponentInChildren<Light>();
        float elapsedTime = 0f;



        while (elapsedTime < beaconDurationBeforePrefabSpawn)
        {
            elapsedTime += Time.deltaTime;
            //beaconLight.range = Mathf.Lerp(3f, 1.25f*eventToSummon.GetComponent<Diametre>().diametre / 2f, elapsedTime / beaconDurationBeforePrefabSpawn);
            beaconInstance.transform.localScale = new Vector3(1, 1, 1) * Mathf.Lerp(0, eventToSummon.GetComponent<Diametre>().diametre / 2f, elapsedTime / beaconDurationBeforePrefabSpawn);
            yield return new WaitForSeconds(Time.deltaTime);
        }


        beaconInstance.SetActive(false);

        yield return new WaitForEndOfFrame();
        eventToSummon.transform.position = beaconInstance.transform.position;
        eventToSummon.transform.rotation = Quaternion.identity;
        eventToSummon.SetActive(true);
        Destroy(beaconInstance);
    }

    private List<GameObject> AvailableSpawns()
    {
        List<GameObject> availableSpawns = new List<GameObject>();
        int layerMask = ~LayerMask.GetMask("ground");
        foreach (GameObject spawn in spawnpoints)
        {
            spawn.layer = LayerMask.NameToLayer("ground");
            if (Physics.OverlapSphere(spawn.transform.position, 2, layerMask).Length == 0)
            {
                availableSpawns.Add(spawn);
            }
            spawn.layer = 0;
        }
        return availableSpawns;
    }

}
