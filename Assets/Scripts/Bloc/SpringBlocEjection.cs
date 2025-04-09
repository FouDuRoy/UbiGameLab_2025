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
    [SerializeField] float energyLoss = 0.8f;
    [SerializeField] float upEjectionMin = .2f;
    [SerializeField] float upEjectionMax = 1f;
    [SerializeField] float ejectionFactor = 1f;
    [SerializeField] float ejectedFactor = 0.75f;
    [SerializeField] float maxAngle = 5f;
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
        float randomHeightFactor = Random.Range(upEjectionMin, upEjectionMax);
       
        if(hitterComponent != null && hittedComponent != null && 
            hitterComponent.ownerTranform !=null && hittedComponent.ownerTranform!=null && hitterComponent.ownerTranform.tag != "magneticStructure"
             && hittedComponent.ownerTranform.tag != "magneticStructure")
        {
            checkCollisionBetweenPlayerAndBlock(collision, hitter, hitted, hitterComponent, hittedComponent);
            checkCollisionMelee(hitter, hitted, hitterComponent, hittedComponent, randomHeightFactor);
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
                upEjectionMax = Mathf.Clamp01(upEjectionMax);
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
                    Vector3 ejectionVeolcity = hitterVelocityBeforeImpact * energyLoss;
                    float ejectionMag = ejectionVeolcity.magnitude;
                    Vector3 hittedVelocity = (ejectionVeolcity.normalized * (1 - randomHeightFactor) + Vector3.up * randomHeightFactor) * ejectionMag;
                    hitted.GetComponent<Rigidbody>().velocity = hittedVelocity * ejectionFactor;

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
                upEjectionMax = Mathf.Clamp01(upEjectionMax);
                mainCubeRb = playerObjects.cubeRb;
                moveType = hittedComponent.ownerTranform.GetComponent<PlayerMouvement>().moveType;

                Vector3 hitterVelocityBeforeImpact = hitter.GetComponent<StoredVelocity>().lastTickVelocity;
                if (hitterVelocityBeforeImpact.magnitude > velocityTreshold)
                {
                    gridSystem.DetachBlock(hitted);
                    hittedComponent.state = BlocState.detached;
                    Vector3 ejectionVeolcity = hitterVelocityBeforeImpact * energyLoss;
                    float ejectionMag = ejectionVeolcity.magnitude;
                    float randomHeightFactor = Random.Range(0, upEjectionMax);

                    Vector3 hittedVelocity = (ejectionVeolcity.normalized * (1 - randomHeightFactor) + Vector3.up * randomHeightFactor) * ejectionMag;
                    Quaternion randomDeviation = Quaternion.AngleAxis(Random.Range(-maxAngle, maxAngle), Vector3.up);
                    hitted.GetComponent<Rigidbody>().velocity = hittedVelocity * ejectionFactor;
                    hitter.GetComponent<Rigidbody>().velocity = randomDeviation * (-ejectionVeolcity* ejectedFactor);
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
