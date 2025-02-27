using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerBloc : MonoBehaviour
{
    private Bloc cubeComponent;

    void Start()
    {
        cubeComponent = GetComponent<Bloc>(); // Cache component for performance
    }

    private void OnCollisionEnter(Collision collision)

    {
        Collider other = collision.collider;
    
        if (cubeComponent != null)
        {
            PlayerInfo playerInfo = collision.gameObject.GetComponent<PlayerInfo>();

            if (other.GetComponent<Bloc>() != null)
            {
                if (other.GetComponent<Bloc>().owner != "Neutral" && other.GetComponent<Bloc>().owner != "" &&
                   cubeComponent.owner != "Neutral" && cubeComponent.owner != "")
                {
                    if (cubeComponent.owner != other.GetComponent<Bloc>().owner)
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

