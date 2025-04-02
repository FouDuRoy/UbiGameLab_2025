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
    public float timeBeforeExplosionEnabled = 1f;
    private float gameStartTime;
    private bool canExplode = false;
    ParticleSystem ps;
    public GameObject[] players;

    [Header("Gizmos Settings")]
    public float gizmoDuration = 3f; // Temps d'affichage des sphères visuelles
    private float explosionTime = -1f;
    private bool hasExploded = false;
    [SerializeField] bool explode = false;
    bool pushed = false;

    private void Start()
    {
        gameStartTime = Time.time;
        StartCoroutine(EnableExplosionAfterDelay(timeBeforeExplosionEnabled));
        ps = GetComponentInChildren<ParticleSystem>();
    }

    private IEnumerator EnableExplosionAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        canExplode = true;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!canExplode) return;
        if (collision.relativeVelocity.magnitude > resistance)
        {
            Explode();
        }
    }

    void Update()
    {
        if (explode && canExplode)
        {
            Explode();
        }
    }

    public void Explode()
    {
        if (hasExploded) return;
        hasExploded = true;
        explosionTime = Time.time;

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
        pushed = false;

        BoxCollider boxCollider = GetComponent<BoxCollider>();
        if (boxCollider != null)
        {
            boxCollider.enabled = false;
        }
        Transform ownerTransform = this.GetComponent<Bloc>().ownerTranform;
        if (ownerTransform != null) {
            ownerTransform.GetComponent<GridSystem>().DetachBlock(this.gameObject);
        }
        ps.transform.parent = null;
        ps.transform.position = transform.position;
        print(ps);
        ps.Play();
        gameObject.GetComponent<Feromagnetic>().enabled = false;
        gameObject.SetActive(false);
        Destroy(gameObject, gizmoDuration);
    }

    private void HandleExplosionEffect(GameObject bloc)
    {
        if (bloc.CompareTag("wood"))
        {
            Debug.Log(bloc.name);
            Destroy(bloc);
        }
        else if (bloc.CompareTag("explosive"))
        {
            bloc.GetComponent<ExplosiveBloc>().Explode();
        }
        else
        {
            GridSystem grid = bloc.transform.root.GetComponent<GridSystem>();
            if (grid != null)
            {
                if(bloc != grid.kernel)
                {
                    grid.DetachBlock(bloc);
                    bloc.GetComponent<Bloc>().state = BlocState.detached;
                }
               
            }
            Rigidbody targetRb = bloc.GetComponent<Rigidbody>();
            Vector3 forceDirection = (targetRb.transform.position - transform.position).normalized;
            targetRb.AddForce(forceDirection * repulsionForce, ForceMode.VelocityChange);
        }
    }

    private void ApplyRepulsionEffect(Collider col, Vector3 distance)
    {
        PlayerObjects obj = col.transform.root.GetComponent<PlayerObjects>();
        Rigidbody colRigidBody = col.GetComponent<Rigidbody>();
        if (colRigidBody != null )
        {
            if(obj == null)
            {
                colRigidBody.AddForce(distance.normalized * repulsionForce, ForceMode.VelocityChange);
            }
            else if(!pushed)
            {
                obj.cubeRb.AddForceAtPosition(distance.normalized * repulsionForce,col.transform.root.GetComponent<PlayerMouvement>().CalculateCenterMass(), ForceMode.VelocityChange);
                pushed = true;
            }
        }
    }
}
