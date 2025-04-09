using System.Collections;
using UnityEngine;
using Quaternion = UnityEngine.Quaternion;
using Random = UnityEngine.Random;
using Vector3 = UnityEngine.Vector3;
//Federico Barallobres
public class SpringBlocEjection : MonoBehaviour
{
    [SerializeField] float velocityTreshold = 10f;
    [SerializeField] float velocityTresholdMelee = 10f;

    [SerializeField] float upEjectionMinRanged = .2f;
    [SerializeField] float upEjectionMaxRanged = 1f;
    [SerializeField] float minRangedEjectionForce = 20f;
    [SerializeField] float maxRangedEjectionForce = 20f;

    [SerializeField] float minMeleeEjectionForce = 20f;
    [SerializeField] float maxMeleeEjectionForce = 20f;
    [SerializeField] float upEjectionMinMelee = .2f;
    [SerializeField] float upEjectionMaxMelee = 1f;
    [SerializeField] float maxAngle = 5f;
    [SerializeField] float passivedDetachedForce = 5f;
    private GridSystem gridSystem;
    private PlayerObjects playerObjects;
    MouvementType moveType;
    Rigidbody mainCubeRb;

    private void Start()
    {

    }


    void OnCollisionEnter(Collision collision)
    {

        GameObject hitter = collision.collider.gameObject;
        GameObject hitted = gameObject;
        Bloc hitterComponent = hitter.GetComponent<Bloc>();
        Bloc hittedComponent = hitted.GetComponent<Bloc>();
        float randomHeightFactorMelee = Random.Range(upEjectionMinMelee, upEjectionMaxMelee);
       
        if(hitterComponent != null && hittedComponent != null && 
            hitterComponent.ownerTranform !=null && hittedComponent.ownerTranform!=null && hitterComponent.ownerTranform.tag != "magneticStructure"
             && hittedComponent.ownerTranform.tag != "magneticStructure")
        {
            checkCollisionBetweenPlayerAndBlock(collision, hitter, hitted, hitterComponent, hittedComponent);
            checkCollisionMelee(hitter, hitted, hitterComponent, hittedComponent, randomHeightFactorMelee);
        }
    }

    private void checkCollisionMelee(GameObject hitter, GameObject hitted, Bloc hitterComponent, Bloc hittedComponent, float randomHeightFactor)
    {
        
            string ownerHitter = hitterComponent.owner;
            string ownerHitted = hittedComponent.owner;
            BlocState stateHitter = hitterComponent.state;
            BlocState stateHitted = hittedComponent.state;

            bool areOwnedByPlayers = ownerHitter.Contains("Player") && ownerHitted.Contains("Player");
            bool playerHittedByotherPlayerStructure = ownerHitter != ownerHitted && stateHitted == BlocState.structure && stateHitter == BlocState.structure;

            if (playerHittedByotherPlayerStructure && areOwnedByPlayers)
            {
                gridSystem = hittedComponent.ownerTranform.GetComponent<GridSystem>();
                playerObjects = hittedComponent.ownerTranform.GetComponent<PlayerObjects>();
                mainCubeRb = playerObjects.cubeRb;
                moveType = hittedComponent.ownerTranform.GetComponent<PlayerMouvement>().moveType;

                Rigidbody hitterRB = hittedComponent.ownerTranform.GetComponent<PlayerObjects>().cubeRb;
                Rigidbody hittedRB = mainCubeRb;

                float hitterVelocity = hitter.GetComponent<StoredVelocity>().lastTickVelocity.magnitude;
                float hittedVelocityMag = hitted.GetComponent<StoredVelocity>().lastTickVelocity.magnitude;
                Vector3 hitterVelocityBeforeImpact = hitter.GetComponent<StoredVelocity>().lastTickVelocity;

                if (hitterVelocity > velocityTresholdMelee && hitterVelocity > hittedVelocityMag)
                {
                    gridSystem.DetachBlock(hitted);
                    hittedComponent.state = BlocState.detached;
                    Vector3 ejectionVeolcity = Random.Range(minMeleeEjectionForce, maxMeleeEjectionForce) * hitterVelocityBeforeImpact.normalized;
                    float ejectionMag = ejectionVeolcity.magnitude;
                    Vector3 hittedVelocity = (ejectionVeolcity.normalized * (1 - randomHeightFactor) + Vector3.up * randomHeightFactor) * ejectionMag;
                    hitted.GetComponent<Rigidbody>().velocity = hittedVelocity;

                }
            }
         
    }

    private void checkCollisionBetweenPlayerAndBlock(Collision collision, GameObject hitter, GameObject hitted, Bloc hitterComponent, Bloc hittedComponent)
    {
        
            string ownerHitter = hitterComponent.owner;
            string ownerHitted = hittedComponent.owner;
            BlocState stateHitter = hitterComponent.state;
            BlocState stateHitted = hittedComponent.state;

            bool areOwnedByPlayers = ownerHitter.Contains("Player") && ownerHitted.Contains("Player");
            bool playerHittedByotherPlayer = ownerHitter != ownerHitted && stateHitted == BlocState.structure && stateHitter == BlocState.projectile;


            if ((playerHittedByotherPlayer) && areOwnedByPlayers)
            {

                gridSystem = hittedComponent.ownerTranform.GetComponent<GridSystem>();
                playerObjects = hittedComponent.ownerTranform.GetComponent<PlayerObjects>();
                mainCubeRb = playerObjects.cubeRb;
                moveType = hittedComponent.ownerTranform.GetComponent<PlayerMouvement>().moveType;

                Vector3 hitterVelocityBeforeImpact = hitter.GetComponent<StoredVelocity>().lastTickVelocity;
                if (hitterVelocityBeforeImpact.magnitude > velocityTreshold)
                {
                    gridSystem.DetachBlocSingleCollisionRanged(hitted);
                    gridSystem.ejectRest(passivedDetachedForce);
                    hittedComponent.state = BlocState.detached;

                    Vector3 veolcityHitted = hitterVelocityBeforeImpact.normalized*Random.Range(minRangedEjectionForce, maxRangedEjectionForce);
                    float energyLoss = Mathf.Sqrt(Mathf.Abs((mainCubeRb.position - hitterComponent.ownerTranform.GetComponent<PlayerObjects>().cubeRb.position).magnitude))/8f;
                    Vector3 veolcityHitter = -veolcityHitted * energyLoss;
                     float randomHeightFactor = Random.Range(upEjectionMinRanged, upEjectionMaxRanged);

                    Vector3 hittedVelocity = veolcityHitted.magnitude*(veolcityHitted.normalized * (1 - randomHeightFactor) + Vector3.up * randomHeightFactor);
                    Quaternion randomDeviation = Quaternion.AngleAxis(Random.Range(-maxAngle, maxAngle), Vector3.up);
                    hitted.GetComponent<Rigidbody>().velocity = hittedVelocity;
                    hitter.GetComponent<Rigidbody>().velocity = randomDeviation * (veolcityHitter);
                }
            }
        
    }
    void OnJointBreak(float breakForce)
    {
        Debug.Log("here");
        gridSystem = gameObject.GetComponent<Bloc>().ownerTranform.GetComponent<GridSystem>();
        gridSystem.DetachBlock(this.gameObject);
        this.GetComponent<Bloc>().state = BlocState.detached;

        Rigidbody cubeRB = this.GetComponent<Rigidbody>();

    }

    IEnumerator resetTorque(GridSystem grid)
    {
        yield return new WaitForSeconds(3f);

        foreach (var v in gridSystem.grid)
        {
            Joint[] joints = v.Value.GetComponents<Joint>();
            foreach (Joint joint in joints)
            {
                joint.breakTorque = this.GetComponent<Feromagnetic>().springTorqueBreak;
                joint.breakForce = this.GetComponent<Feromagnetic>().springForceBreak;
            }
        }
    }
}
