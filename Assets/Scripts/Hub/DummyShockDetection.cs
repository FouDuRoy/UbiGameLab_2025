using System.Collections;
using UnityEngine;

public class DummyShockDetection : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] float victoryConditionSpeedRange = 10f;
    [SerializeField] float victoryConditionSpeedMelee = 15f;
    [SerializeField] GameObject Ennemy;
    [SerializeField] private TutoUI dashTutoUI;
    [SerializeField] private TutoUI cacTutoUI;
    [SerializeField] private TutoUI shootTutoUI;
    [SerializeField] private int neededShootHits = 2;
    [SerializeField] private int neededCacHits = 2;

    private Rigidbody rb;
    private GridSystem grid;
    private int OG_gridSize;
    private int nShootHits = 0;
    private int nCacHits = 0;
    private bool dashCompleted = false;
    private bool shootCompleted = false;
    bool canTrigger = true;
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        grid= rb.GetComponentInParent<GridSystem>();
        OG_gridSize=grid.grid.Count;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if(grid.grid.Count < OG_gridSize)
        {
            dashCompleted = true;
            dashTutoUI.NextTuto();
        }
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (canTrigger)
        {
            GameObject hitter = collision.collider.gameObject;
            Bloc hitterComponent = hitter.GetComponent<Bloc>();

            if (hitterComponent != null)
            {
                string ownerHitter = hitterComponent.owner;
                string ownerHitted = transform.parent.name;
                BlocState stateHitter = hitterComponent.state;
                bool projectileFromOtherPlayer = stateHitter == BlocState.projectile && (ownerHitter != ownerHitted);

                if (projectileFromOtherPlayer)
                {
                    Vector3 hitterVelocity = hitter.GetComponent<StoredVelocity>().lastTickVelocity;
                    Vector3 projectileVelocity = hitterVelocity;

                    if (projectileVelocity.magnitude > victoryConditionSpeedRange)
                    {
                        DoRotation();

                        if (shootTutoUI.isActiveAndEnabled)
                        {
                            nShootHits++;
                            print(nShootHits);
                            shootTutoUI.SetTutoCount(nShootHits, neededShootHits);
                            if (nShootHits >= neededShootHits)
                            {
                                shootCompleted = true;
                                shootTutoUI.NextTuto();
                            }
                        }

                    }
                }
                bool meleeFromOtherPlayer = stateHitter == BlocState.structure && (ownerHitter != ownerHitted);
                if (meleeFromOtherPlayer)
                {
                    Vector3 hitterVelocity = hitter.GetComponent<StoredVelocity>().lastTickVelocity;
                    Vector3 projectileVelocity = hitterVelocity;

                    if (projectileVelocity.magnitude > victoryConditionSpeedMelee)
                    {
                        DoRotation();

                        if (cacTutoUI.isActiveAndEnabled &&  shootCompleted)
                        {
                            nCacHits++;
                            cacTutoUI.SetTutoCount(nCacHits, neededCacHits);
                            if (nCacHits >= neededCacHits)
                            {
                                cacTutoUI.NextTuto();
                            }
                        }
                    }
                }
            }
        }

    }

    private void DoRotation()
    {
        rb.constraints = RigidbodyConstraints.FreezeRotationZ | RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezePosition;
        rb.AddTorque(0, 100f, 0, ForceMode.VelocityChange);
        StartCoroutine(waitSomeTImeAndReset());

    }
    IEnumerator waitSomeTImeAndReset()
    {
        yield return new WaitForSeconds(2f);
        rb.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePosition;
        canTrigger = true;
    }
}
