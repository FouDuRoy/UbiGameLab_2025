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
            PlayerInfo playerInfo = collision.gameObject.GetComponent<PlayerInfo>();

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
            else if (other.gameObject.name.Contains("Player") && other.gameObject.name != cubeComponent.owner
                && cubeComponent.owner != "" && cubeComponent.owner != "Neutral")
            {
                playerInfo.TakeDamage(cubeComponent.owner); // Send enemy's name
            }
           

        }
    }
}

