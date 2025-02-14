using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RandomSpawner : MonoBehaviour
{
    public GameObject objectToSpawn;
    public Transform[] spawnPoints;
    public int numberOfSpawns = 5;
    public float spawnInterval = 15f;

    void Start()
    {
        StartCoroutine(SpawnLoop());
    }

    IEnumerator SpawnLoop()
    {
        while (true)
        {
            SpawnObjects();
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    void SpawnObjects()
    {
        if (spawnPoints.Length < numberOfSpawns)
        {
            Debug.LogError("Pas assez de points de spawn !");
            return;
        }

        List<int> availableIndexes = new List<int>();
        for (int i = 0; i < spawnPoints.Length; i++)
        {
            availableIndexes.Add(i);
        }

        for (int i = 0; i < numberOfSpawns; i++)
        {
            int randomIndex = Random.Range(0, availableIndexes.Count);
            int spawnIndex = availableIndexes[randomIndex];

            Instantiate(objectToSpawn, spawnPoints[spawnIndex].position, Quaternion.identity);

            availableIndexes.RemoveAt(randomIndex);
        }
    }
}
