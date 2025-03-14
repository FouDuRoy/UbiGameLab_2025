using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//Federico Barallobres
public class TriggerBloc : MonoBehaviour
{
    public float cubeShockResistance;
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
            PlayerInfo playerInfo = collision.gameObject.transform.root.GetComponent<PlayerInfo>();

            float magnitude = collision.relativeVelocity.magnitude;
            if (other.GetComponent<Bloc>() != null)
            {
                if (other.GetComponent<Bloc>().owner != "Neutral" && other.GetComponent<Bloc>().owner != "" &&
                   cubeComponent.owner != "Neutral" && cubeComponent.owner != "")
                {
                    if (cubeComponent.owner != other.GetComponent<Bloc>().owner)
                    {
                        

                        if(other.transform.root.name.Contains("Player")){
                            other.transform.root.GetComponent<PlayerObjects>().cubes.Remove(gameObject);
                        }
                        if(transform.root.name.Contains("Player")){
                           transform.root.GetComponent<PlayerObjects>().cubes.Remove(gameObject);
                        }
                    
                        Destroy(other.gameObject);
                        Destroy(this.gameObject);
                        
                    }

                }
            }
            else if (other.transform.root.name.Contains("Player") && other.gameObject.name != cubeComponent.owner
                && cubeComponent.owner != "" && cubeComponent.owner != "Neutral" && magnitude >= cubeShockResistance)
            {
                
                playerInfo.TakeDamage(cubeComponent.owner); // Send enemy's name
            }
            else if(magnitude < cubeShockResistance)
            {
                Debug.Log("Magnitude less than shock resistance");
            }
           

        }
    }
}

