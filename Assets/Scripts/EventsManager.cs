using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventsManager : MonoBehaviour
{
    [SerializeField] private List<GameObject> eventsPrefabs = new List<GameObject>();

    void Start()
    {
        //On récupère tous les spawnpoints

        List<GameObject> spawnpoints = new List<GameObject>();

        foreach (Transform child in transform)
        {
            spawnpoints.Add(child.gameObject);
        }

        Debug.Log("Nombre d'enfants : " + spawnpoints.Count);
    }

    void Update()
    {
        
    }
}
