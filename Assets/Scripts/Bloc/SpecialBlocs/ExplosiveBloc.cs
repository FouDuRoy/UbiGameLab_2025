using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

        Collider[] affectedObjects = Physics.OverlapSphere(transform.position, repulsionRange);
        List<Rigidbody> repulsedBodies = new List<Rigidbody>();

        foreach (Collider col in affectedObjects)
        {
            Bloc bloc = col.GetComponent<Bloc>();
            if (bloc)
            {
                float distance = Vector3.Distance(transform.position, col.transform.position);

                if (distance <= explosionRange)
                {
                    HandleExplosionEffect(bloc);
                }

                if (distance <= repulsionRange)
                {
                    ApplyRepulsionEffect(col, distance, repulsedBodies);
                }
            }
        }

        Destroy(gameObject);
    }

    private void HandleExplosionEffect(Bloc block)
    {
        if (block is WoodBloc)
        {
            Destroy(block.gameObject);
        }
        else
        {
            //Displacement calling
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
}
