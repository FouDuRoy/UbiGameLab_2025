using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomSpawner : MonoBehaviour
{
    public GameObject objectToSpawn; // Object to spawn
    public Transform[] spawnPoints; // Spawn point objects
    private Vector3[] initialPositions; // Store the original positions
    public int numberOfSpawns = 5;
    public float spawnInterval = 15f;
    public float checkRadius = 0.5f; // Adjust the radius to fit your prefab size
    public LayerMask spawnLayerMask; // Assign a specific layer for spawned objects

    void Start()
    {
        // Store the initial positions of all spawn points
        initialPositions = new Vector3[spawnPoints.Length];
        for (int i = 0; i < spawnPoints.Length; i++)
        {
            initialPositions[i] = spawnPoints[i].position;
        }

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
        for (int i = 0; i < initialPositions.Length; i++)
        {
            availableIndexes.Add(i);
        }

        int spawned = 0;
        while (spawned < numberOfSpawns && availableIndexes.Count > 0)
        {
            int randomIndex = Random.Range(0, availableIndexes.Count);
            int spawnIndex = availableIndexes[randomIndex];

            // Check if something is already at the spawn position
            if (!IsPositionOccupied(initialPositions[spawnIndex]))
            {
                Instantiate(objectToSpawn, initialPositions[spawnIndex], Quaternion.identity);
                spawned++; // Only increase if an object was successfully spawned
            }

            availableIndexes.RemoveAt(randomIndex);
        }
    }

    bool IsPositionOccupied(Vector3 position)
    {
        // Check if any colliders are in the area (adjust checkRadius if needed)
        Collider[] colliders = Physics.OverlapSphere(position, checkRadius, spawnLayerMask);
        return colliders.Length > 0; // If there are colliders, the spot is occupied
    }
}
