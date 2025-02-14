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
        if(cubeComponent != null)
            if(other.GetComponent<Cube>() != null)
                if (cubeComponent.owner != null && other.GetComponent<Cube>().owner != null && cubeComponent.owner != other.GetComponent<Cube>().owner)
                {
                    Destroy(other.gameObject);
                    Destroy(this);

                }
    }
}

