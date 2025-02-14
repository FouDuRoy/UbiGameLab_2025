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
                if(other.GetComponent<Cube>().owner != "Neutral" &&  other.GetComponent<Cube>().owner != "" && 
                    cubeComponent.owner != "Neutral" && cubeComponent.owner != "")
                {
                    if (cubeComponent.owner != other.GetComponent<Cube>().owner)
                    {
                        Debug.Log("Hit confirmed");
                        //Debug.Log(cubeComponent.owner);
                        //Debug.Log(other.GetComponent<Cube>().owner);
                        //Destroy(other.gameObject);
                        //Destroy(this);
                    }
                }

    }
}

