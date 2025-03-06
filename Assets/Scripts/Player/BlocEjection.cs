using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlocEjection : MonoBehaviour
{
    [SerializeField] float velocityTreshold = 10f;

    void OnCollisionEnter(Collision collision)
    {
        
        GameObject hitter= collision.collider.gameObject;
        GameObject hitted = collision.GetContact(0).thisCollider.gameObject;
        foreach(ContactPoint p in collision.contacts){
            Debug.Log(p.thisCollider.gameObject);
        }
        if(hitted.transform.parent==transform){
            Vector3 relativeVelocity = collision.relativeVelocity;
            if(relativeVelocity.magnitude> velocityTreshold){
                hitted.transform.root.GetComponent<PlayerObjects>().addRigidBody(hitted);
                hitted.transform.root.GetComponent<PlayerObjects>().removeCube(hitted);
                hitted.GetComponent<Rigidbody>().velocity = relativeVelocity;
                hitter.GetComponent<Rigidbody>().velocity = -relativeVelocity;
                StartCoroutine(blockNeutral(hitted));
         }
        }
        
        
    }
    IEnumerator blockNeutral(GameObject block)
    {

        yield return new WaitForSeconds(3f);
        if(block !=null)
        {
            block.GetComponent<Bloc>().setOwner("Neutral");
        }
       
    }
}
