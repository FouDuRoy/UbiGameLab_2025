using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class SceneEventSystem : MonoBehaviour
{
    public Vector3[] playerSpawnPositions;

    public void LoadScene(string sceneName)
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.LoadScene(sceneName);
    }


    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Find spawn points in the new scene
        GameObject[] spawnPoints = GameObject.FindGameObjectsWithTag("SpawnPoint");

        // Update spawn positions if needed
        if (spawnPoints.Length > 0)
        {
            Vector3[] newPositions = new Vector3[spawnPoints.Length];
            for (int i = 0; i < spawnPoints.Length; i++)
            {
                newPositions[i] = spawnPoints[i].transform.position;
            }
            playerSpawnPositions = newPositions;
        }

        // Enable and position players
        this.GetComponent<CustomPlayerSpawner>().spawnPlayer(spawnPoints);
        SceneManager.sceneLoaded -= OnSceneLoaded; // Unsubscribe
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}
