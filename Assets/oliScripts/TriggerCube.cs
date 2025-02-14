using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerCube : MonoBehaviour
{
    private Cube cubeComponent;

    void Start()
    {
        cubeComponent = GetComponent<Cube>(); // Cache component for performance
    }

    private void OnTriggerEnter(Collider other)
    {

        // Ensure owner is assigned only once and prioritize Player
        if (cubeComponent.owner == null && other.gameObject.CompareTag("Player"))
        {
            cubeComponent.owner = other.gameObject;
            Debug.Log("Owner set to Player: " + other.gameObject.name);
        }
        if(cubeComponent != null)
            if(other.GetComponent<Cube>() != null)
                if (cubeComponent.owner != null && other.GetComponent<Cube>().owner != null && cubeComponent.owner != other.GetComponent<Cube>().owner)
                {
                    Destroy(other.gameObject);
                    Destroy(this);

                }
    }
}

