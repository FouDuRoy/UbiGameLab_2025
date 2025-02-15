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

    private void OnCollisionEnter(Collision collision)

    {
        Collider other = collision.collider;
    
        if (cubeComponent != null)
        {
            if (other.GetComponent<Cube>() != null)
            {
            
                if (other.GetComponent<Cube>().owner != "Neutral" && other.GetComponent<Cube>().owner != "" &&
                   cubeComponent.owner != "Neutral" && cubeComponent.owner != "")
                {
                    if (cubeComponent.owner != other.GetComponent<Cube>().owner)
                    {
                        Destroy(other.gameObject);
                        Destroy(this.gameObject);
                    }

                }
            }
            else if (other.gameObject.name.Contains("Joueur") && other.gameObject.name != cubeComponent.owner
                && cubeComponent.owner != "" && cubeComponent.owner != "Neutral")
            {
                Debug.Log(other.gameObject.name);
                Debug.Log(cubeComponent.owner);
                Debug.Log("Game Over");
            }
           

        }
    }
}

