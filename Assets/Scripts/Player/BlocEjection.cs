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
        if (collision.gameObject.tag != "explosive")
        {
            GameObject hitter = collision.collider.gameObject;
            GameObject hitted = collision.GetContact(0).thisCollider.gameObject;

            if (hitted.transform.parent == transform)
            {
                Vector3 relativeVelocity = collision.relativeVelocity;
                if (relativeVelocity.magnitude > velocityTreshold)
                {
                    playerObjects.addRigidBody(hitted);
                    playerObjects.removeCube(hitted);

                    hitted.GetComponent<Rigidbody>().velocity = relativeVelocity;
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
