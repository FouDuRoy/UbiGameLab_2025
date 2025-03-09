using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlocEjection : MonoBehaviour
{
    [SerializeField] float velocityTreshold = 10f;
    [SerializeField] float energyLoss = 0.8f;
    private GridSystem gridSystem;
    private PlayerObjects playerObjects;


    private void Start()
    {
        gridSystem = gameObject.GetComponent<GridSystem>();
        playerObjects = FindObjectOfType<PlayerObjects>();
    }

 
    void OnCollisionEnter(Collision collision)
    {

        GameObject hitter = collision.collider.gameObject;
        GameObject hitted = collision.GetContact(0).thisCollider.gameObject;
        Bloc hitterComponent = hitter.GetComponent<Bloc>();
        Bloc hittedComponent = hitted.GetComponent<Bloc>();

        checkCollisionBetweenPlayerAndBlock(collision, hitter, hitted, hitterComponent, hittedComponent);
        if (hitterComponent != null && hittedComponent != null)
        {
            string ownerHitter = hitterComponent.owner;
            string ownerHitted = hittedComponent.owner;
             
            if (ownerHitter != ownerHitted && ownerHitter.Contains("Player") && ownerHitted.Contains("Player") && !ownerHitter.Contains("projectile"))
            {
                Vector3 relativeVelocity = collision.relativeVelocity;
                if (relativeVelocity.magnitude > velocityTreshold)
                {
                    // Calculate normal average
                  

                    hitted.transform.root.GetComponent<PlayerObjects>().addRigidBody(hitted);
                    gridSystem.DetachBlock(hitted);
                    Vector3 ejectionVeolcity = relativeVelocity*energyLoss;
                    hitted.GetComponent<Rigidbody>().velocity = ejectionVeolcity;
                    StartCoroutine(blockNeutral(hitted));
                }
            }
        }
    }

    private void checkCollisionBetweenPlayerAndBlock(Collision collision, GameObject hitter, GameObject hitted, Bloc hitterComponent, Bloc hittedComponent)
    {
        if (hitterComponent != null && hittedComponent != null)
        {
            string ownerHitter = hitterComponent.owner;
            string ownerHitted = hittedComponent.owner;
             
            if (ownerHitter != ownerHitted  && ownerHitted.Contains("Player") && ownerHitter.Contains("projectile"))
            {
                Vector3 relativeVelocity = collision.relativeVelocity;
                if (relativeVelocity.magnitude > velocityTreshold)
                {
                    
                   
                    Vector3 ejectionVeolcity =relativeVelocity*energyLoss;
                    hitted.transform.root.GetComponent<PlayerObjects>().addRigidBody(hitted);
                    gridSystem.DetachBlock(hitted);
                    hitted.GetComponent<Bloc>().owner = "projectile";
                    hitted.GetComponent<Rigidbody>().velocity = ejectionVeolcity;
                    hitter.GetComponent<Rigidbody>().velocity = -ejectionVeolcity;
                    StartCoroutine(blockNeutral(hitted));
                }
            }
        }
    }

    IEnumerator blockNeutral(GameObject block)
    {
        yield return new WaitForSeconds(3f);
        if (block != null)
        {
            block.GetComponent<Bloc>().setOwner("Neutral");
        }
    }
}
