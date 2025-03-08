using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlocEjection : MonoBehaviour
{
    [SerializeField] float velocityTreshold = 10f;
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
             
            if (ownerHitter != ownerHitted && ownerHitter.Contains("Player") && ownerHitted.Contains("Player"))
            {
                Vector3 relativeVelocity = collision.relativeVelocity;
                 Debug.Log("hitted:"+relativeVelocity);
                if (relativeVelocity.magnitude > velocityTreshold)
                {
                    Debug.Log(relativeVelocity.magnitude);
                    hitted.transform.root.GetComponent<PlayerObjects>().addRigidBody(hitted);
                    hitted.transform.root.GetComponent<PlayerObjects>().removeCube(hitted);

                    hitted.GetComponent<Rigidbody>().velocity = relativeVelocity*3;
                    hitted.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotation;
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
             
            if (ownerHitter != ownerHitted && ownerHitter.Contains("projectile") && ownerHitted.Contains("Player"))
            {
                Vector3 relativeVelocity = collision.relativeVelocity;
                 Debug.Log("hitted:"+relativeVelocity);
                if (relativeVelocity.magnitude > velocityTreshold)
                {
                    Debug.Log(relativeVelocity.magnitude);
                    hitted.transform.root.GetComponent<PlayerObjects>().addRigidBody(hitted);
                    hitted.transform.root.GetComponent<PlayerObjects>().removeCube(hitted);

                    hitted.GetComponent<Rigidbody>().velocity = relativeVelocity;
                    hitted.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotation;
                    hitter.GetComponent<Rigidbody>().velocity = -relativeVelocity;
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
