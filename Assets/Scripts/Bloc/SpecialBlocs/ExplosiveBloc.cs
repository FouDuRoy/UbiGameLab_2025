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

    [Header("Gizmos Settings")]
    public float gizmoDuration = 3f; // Temps d'affichage des sphères visuelles
    private float explosionTime = -1f;
    public Material explosionMaterial;
    public Material repulsionMaterial;
    private bool hasExploded = false;
    [SerializeField] bool explode = false;

    private GameObject explosionSphere;
    private GameObject repulsionSphere;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.relativeVelocity.magnitude > resistance)
        {
            Explode();
        }
    }

    void Update()
    {
        if (explode)
        {
            Explode();
        }
    }

    public void Explode()
    {
        if (hasExploded) return;
        hasExploded = true;
        explosionTime = Time.time;
        HandleParticles();
        ShowExplosionSpheres();

        Collider[] colliders = Physics.OverlapSphere(transform.position, repulsionRange);
        BoxCollider[] affectedObjects = colliders.OfType<BoxCollider>().ToArray();
        List<Rigidbody> repulsedBodies = new List<Rigidbody>();

        foreach (Collider col in affectedObjects)
        {
            Bloc bloc = col.GetComponent<Bloc>();
            if (bloc)
            {
                float distance = Vector3.Distance(transform.position, col.transform.position);
                Vector3 dist = col.transform.position - transform.position;

                if (distance <= explosionRange)
                {
                    HandleExplosionEffect(col.gameObject);
                }
                else if (distance <= repulsionRange)
                {
                    ApplyRepulsionEffect(col, dist);
                }
            }
        }

        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(false);
        }

        BoxCollider boxCollider = GetComponent<BoxCollider>();
        if (boxCollider != null)
        {
            boxCollider.enabled = false;
        }

        Destroy(gameObject, gizmoDuration);
    }

    private void ShowExplosionSpheres()
    {
        // Créer une sphère rouge pour l'explosion
        explosionSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        explosionSphere.transform.position = transform.position;
        explosionSphere.transform.localScale = Vector3.one * explosionRange * 2;
        explosionSphere.GetComponent<Renderer>().material = explosionMaterial;
        Destroy(explosionSphere, gizmoDuration);

        // Créer une sphère bleue pour la répulsion
        repulsionSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        repulsionSphere.transform.position = transform.position;
        repulsionSphere.transform.localScale = Vector3.one * repulsionRange * 2;
        repulsionSphere.GetComponent<Renderer>().material = repulsionMaterial;
        Destroy(repulsionSphere, gizmoDuration);
    }

    private void HandleParticles()
    {
        ParticleSystem particles = GetComponentInChildren<ParticleSystem>(true);
        if (particles != null)
        {
            particles.gameObject.SetActive(true);
            particles.Play();
            Debug.Log("Boom");
        }
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
            if (player != null)
            {
                player.addRigidBody(bloc);
                player.removeCube(bloc);
            }
            Rigidbody targetRb = bloc.GetComponent<Rigidbody>();
            Vector3 forceDirection = (targetRb.transform.position - transform.position).normalized;
            targetRb.AddForce(forceDirection * repulsionForce, ForceMode.Impulse);
            StartCoroutine(blockNeutral(bloc));
        }
    }

    private void ApplyRepulsionEffect(Collider col, Vector3 distance)
    {
        PlayerObjects obj = col.transform.root.GetComponent<PlayerObjects>();
        Rigidbody colRigidBody = col.GetComponent<Rigidbody>();
        if (obj != null && colRigidBody == null)
        {
            Rigidbody mainBody = obj.cubeRb;
            mainBody.AddForceAtPosition(distance * repulsionForce, col.transform.position);
        }
        else if (colRigidBody != null)
        {
            colRigidBody.AddForce(distance * repulsionForce);
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
}
