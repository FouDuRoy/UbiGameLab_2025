using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

public class ExplosiveBloc : MonoBehaviour
{
    [Header("Explosion Properties")]
    public float resistance = 10f;
    public float explosionRange = 5f;
    public float repulsionRange = 8f;
    public float repulsionForce = 50f;
    public float repulsionDistanceFactor = 1.2f;

    private bool hasExploded = false;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.relativeVelocity.magnitude > resistance)
        {
            Explode();
        }
    }

    public void Explode()
    {
        if (hasExploded) return;
        hasExploded = true;

        Collider[] colliders = Physics.OverlapSphere(transform.position, repulsionRange);
        BoxCollider[] affectedObjects = colliders.OfType<BoxCollider>().ToArray();
        List<Rigidbody> repulsedBodies = new List<Rigidbody>();
        foreach (Collider col in affectedObjects)
        {
            Bloc bloc = col.GetComponent<Bloc>();
            if (bloc)
            {
                float distance = Vector3.Distance(transform.position, col.transform.position);

                if (distance <= explosionRange)
                {
                    HandleExplosionEffect(col.gameObject);
                }

                else if (distance <= repulsionRange)
                {
                    ApplyRepulsionEffect(col, distance, repulsedBodies);
                }
            }
        }

        Destroy(gameObject);
    }

    private void HandleExplosionEffect(GameObject bloc)
    {
        if (bloc.CompareTag("wood"))
        {
            Debug.Log("Boom");
            Destroy(bloc);
        }
        else
        {
            PlayerObjects player = bloc.GetComponentInParent<PlayerObjects>();
            if(player != null)
            {
                player.addRigidBody(bloc);
                player.removeCube(bloc);
            }
            Rigidbody targetRb = bloc.GetComponent<Rigidbody>();
            Vector3 forceDirection = (targetRb.transform.position - transform.position).normalized;

            // Appliquer la force dans la direction inverse
            targetRb.AddForce(forceDirection * repulsionForce, ForceMode.Impulse);

            StartCoroutine(blockNeutral(bloc));
        }
    }

    private void ApplyRepulsionEffect(Collider col, float distance, List<Rigidbody> repulsedBodies)
    {
        Rigidbody rb = col.attachedRigidbody;
        if (rb != null && !repulsedBodies.Contains(rb))
        {
            Vector3 direction = (col.transform.position - transform.position).normalized;
            float force = repulsionForce / (distance * repulsionDistanceFactor);
            rb.AddForce(direction * force, ForceMode.Impulse);
            repulsedBodies.Add(rb);
        }
    }

    IEnumerator blockNeutral(GameObject block)
    {
        if (block != null)
        {
            block.GetComponent<Bloc>().setOwner("Neutral");
        }
        yield return null;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(1, 0, 0, 0.3f); // Rouge transparent
        Gizmos.DrawSphere(transform.position, explosionRange);

        Gizmos.color = new Color(0, 0, 1, 0.3f); // Bleu transparent
        Gizmos.DrawSphere(transform.position, repulsionRange);
    }
}
