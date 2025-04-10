using System.Collections;
using System.Linq;
using UnityEngine;

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
    public bool canExplode = false;
    ParticleSystem ps;
    public GameObject[] players;

    [Header("Gizmos Settings")]
    public float gizmoDuration = 3f; // Temps d'affichage des sph�res visuelles
    private float explosionTime = -1f;
    public bool hasExploded = false;
    [SerializeField] bool explode = false;

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
        if (collision.collider.tag == "ground" && GetComponent<Bloc>().state == BlocState.projectile) return;
        if (collision.relativeVelocity.magnitude > resistance)
        {
            Explode();
        }
    }

    void Update()
    {
        if (explode && canExplode && !hasExploded)
        {
            Explode();
        }
    }

    public void Explode()
    {

        hasExploded = true;
        explosionTime = Time.time;

        Collider[] colliders = Physics.OverlapSphere(transform.position, repulsionRange);
        BoxCollider[] affectedObjects = colliders.OfType<BoxCollider>().ToArray();

        foreach (Collider col in affectedObjects)
        {
            Rigidbody rbBloc = col.GetComponent<Rigidbody>();
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

        BoxCollider boxCollider = GetComponent<BoxCollider>();
        if (boxCollider != null)
        {
            boxCollider.enabled = false;
        }
        Transform ownerTransform = this.GetComponent<Bloc>().ownerTranform;
        if (ownerTransform != null)
        {
            ownerTransform.GetComponent<GridSystem>().DetachBlock(this.gameObject);
        }
        ps.transform.parent = null;
        ps.transform.position = transform.position;
        ps.Play();
        gameObject.GetComponent<Feromagnetic>().enabled = false;
        gameObject.SetActive(false);
        Destroy(gameObject, gizmoDuration);
    }

    private void HandleExplosionEffect(GameObject bloc)
    {
        if (bloc.CompareTag("wood"))
        {
            Destroy(bloc);
        }
        else if (bloc.CompareTag("explosive") && bloc != gameObject && !bloc.GetComponent<ExplosiveBloc>().hasExploded)
        {
            bloc.GetComponent<ExplosiveBloc>().Explode();
        }
        else
        {
            GridSystem grid = bloc.transform.root.GetComponent<GridSystem>();
            if (grid != null)
            {
                if (bloc != grid.kernel)
                {
                    grid.DetachBlocSingle(bloc);
                    grid.ejectRest(repulsionForce);
                    bloc.GetComponent<Bloc>().state = BlocState.exploded;

                }
            }
            Rigidbody targetRb = bloc.GetComponent<Rigidbody>();
            if (targetRb != null)
            {
                Vector3 forceDirection = (targetRb.transform.position - transform.position).normalized;
                targetRb.velocity = Vector3.zero;
                targetRb.AddForce(forceDirection * repulsionForce, ForceMode.VelocityChange);
            }
        }
    }

    private void ApplyRepulsionEffect(Collider col, Vector3 distance)
    {
        Rigidbody colRigidBody = col.GetComponent<Rigidbody>();
        if (colRigidBody != null)
        {
            colRigidBody.AddForce(distance.normalized * repulsionForce, ForceMode.VelocityChange);

        }
        else if (col.gameObject.layer == LayerMask.NameToLayer("magneticStructure")
                || col.gameObject.layer == LayerMask.NameToLayer("magneticStructureConnetc"))
        {
            Rigidbody coreRb = col.gameObject.transform.root.GetComponent<Rigidbody>();
            coreRb.AddForce(distance.normalized * repulsionForce, ForceMode.VelocityChange);
        }
    }
}
