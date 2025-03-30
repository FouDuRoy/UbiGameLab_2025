using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.ProBuilder.Shapes;
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
    [SerializeField] float maxAngle = 5f;
    [SerializeField] float springBreakForce = 500f;
    [SerializeField] float range = 4;
    [SerializeField] float  pushFactor = 30f;
    private GridSystem gridSystem;
    private PlayerObjects playerObjects;
    Rigidbody mainCubeRb;
    MouvementType moveType;



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

        checkCollisionBetweenPlayerAndBlock(collision, hitter, hitted, hitterComponent, hittedComponent);

        if (hitterComponent != null && hittedComponent != null)
        {
            //Debug.Log("hitter veolcity:" + hitter.GetComponent<StoredVelocity>().lastTickVelocity+ "curentTick" +hitter.GetComponent<Rigidbody>().velocity);
            string ownerHitter = hitterComponent.owner;
            string ownerHitted = hittedComponent.owner;
            BlocState stateHitter = hitterComponent.state;
            BlocState stateHitted = hittedComponent.state;

            bool areOwnedByPlayers = ownerHitter.Contains("Player") && ownerHitted.Contains("Player");
            bool playerHittedByownMelee = ownerHitter == ownerHitted && stateHitted == BlocState.structure && stateHitter == BlocState.melee;
            bool playerHittedByotherPlayerStructure = ownerHitter != ownerHitted && stateHitted == BlocState.structure && stateHitter == BlocState.structure;

            if (playerHittedByotherPlayerStructure && areOwnedByPlayers)
            {
                gridSystem = gameObject.transform.root.GetComponent<GridSystem>();
                playerObjects = FindObjectOfType<PlayerObjects>();
                upEjectionMax = Mathf.Clamp01(upEjectionMax);
                mainCubeRb = transform.parent.GetComponent<Rigidbody>();
                moveType = transform.root.GetComponent<PlayerMouvement>().moveType;

                Rigidbody hitterRB = hitter.transform.parent.GetComponent<Rigidbody>();
                Rigidbody hittedRB = hitted.transform.parent.GetComponent<Rigidbody>();

                float hitterVelocity = hitter.GetComponent<StoredVelocity>().lastTickVelocity.magnitude;
                float hittedVelocityMag = hitted.GetComponent<StoredVelocity>().lastTickVelocity.magnitude;
                Vector3 hitterVelocityBeforeImpact = hitter.GetComponent<StoredVelocity>().lastTickVelocity;

                if (hitterVelocity > velocityTresholdMelee && hitterVelocity > hittedVelocityMag)
                {
                    if (false)
                    {
                        foreach (var v in gridSystem.grid)
                        {
                            Joint[] joints = v.Value.GetComponents<Joint>();
                            foreach (Joint joint in joints)
                            {
                                if ((joint.transform.position - hitted.transform.position).magnitude < range)
                                {
                                    joint.breakTorque = springBreakForce;

                                }
                            }
                        }

                    }
                    else
                    {
                        Debug.Log("it good: "+ hitterVelocity+"hitter:"+hitter.name);
                        gridSystem.DetachBlock(hitted);
                        hittedComponent.state = BlocState.detached;
                        Vector3 ejectionVeolcity = hitterVelocityBeforeImpact * energyLoss;
                        float ejectionMag = ejectionVeolcity.magnitude;
                        Vector3 hittedVelocity = (ejectionVeolcity.normalized * (1 - randomHeightFactor) + Vector3.up * randomHeightFactor) * ejectionMag;
                        hitted.GetComponent<Rigidbody>().velocity = hittedVelocity * ejectionFactor;
                    }
              

                }
            }
            if (playerHittedByownMelee && areOwnedByPlayers)
            {
                gridSystem = gameObject.transform.root.GetComponent<GridSystem>();
                playerObjects = FindObjectOfType<PlayerObjects>();
                upEjectionMax = Mathf.Clamp01(upEjectionMax);
                mainCubeRb = transform.GetComponent<Rigidbody>();
                moveType = transform.root.GetComponent<PlayerMouvement>().moveType;

          
                float hitterVelocity = hitter.GetComponent<StoredVelocity>().lastTickVelocity.magnitude;
                float hittedVelocityMag = hitted.GetComponent<StoredVelocity>().lastTickVelocity.magnitude;
                Vector3 hitterVelocityBeforeImpact = hitter.GetComponent<StoredVelocity>().lastTickVelocity;

                if (hitterVelocity > velocityTresholdMelee && hitterVelocity > hittedVelocityMag)
                {
                    // Calculate normal average
                    Debug.Log("its ok ");
                    gridSystem.DetachBlock(hitted);
                    hittedComponent.state = BlocState.melee;
                    Vector3 ejectionVeolcity = hitterVelocityBeforeImpact * energyLoss;
                    float ejectionMag = ejectionVeolcity.magnitude;
                    Vector3 hittedVelocity = (ejectionVeolcity.normalized * (1 - randomHeightFactor) + Vector3.up * randomHeightFactor) * ejectionMag;
                    hitted.GetComponent<Rigidbody>().velocity = hittedVelocity * ejectionFactor;
                    
                }
            }
        }
    }

    private void checkCollisionBetweenPlayerAndBlock(Collision collision, GameObject hitter, GameObject hitted, Bloc hitterComponent, Bloc hittedComponent)
    {
        if (hitterComponent != null && hittedComponent != null)
        {
            string ownerHitter = hitterComponent.owner;
            string ownerHitted = hittedComponent.owner;
            BlocState stateHitter = hitterComponent.state;
            BlocState stateHitted = hittedComponent.state;

            bool areOwnedByPlayers = ownerHitter.Contains("Player") && ownerHitted.Contains("Player");
            bool playerHittedByotherPlayer = ownerHitter != ownerHitted && stateHitted == BlocState.structure && stateHitter == BlocState.projectile;
            bool playerHittedByOwnBlock = ownerHitter == ownerHitted && stateHitted == BlocState.structure && stateHitter == BlocState.detached;

            if ((playerHittedByOwnBlock) && areOwnedByPlayers)
            {
                gridSystem = gameObject.transform.root.GetComponent<GridSystem>();
                playerObjects = FindObjectOfType<PlayerObjects>();
                upEjectionMax = Mathf.Clamp01(upEjectionMax);
                mainCubeRb = transform.GetComponent<Rigidbody>();
                moveType = transform.root.GetComponent<PlayerMouvement>().moveType;

              
                Vector3 hitterVelocityBeforeImpact = hitter.GetComponent<Rigidbody>().velocity; 
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
                    hitter.GetComponent<Rigidbody>().velocity = randomDeviation * (-ejectionVeolcity);
                }
            }

            if ((playerHittedByotherPlayer) && areOwnedByPlayers)
            {

                gridSystem = gameObject.transform.root.GetComponent<GridSystem>();
                playerObjects = FindObjectOfType<PlayerObjects>();
                mainCubeRb = transform.GetComponent<Rigidbody>();
                moveType = transform.root.GetComponent<PlayerMouvement>().moveType;

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
                    hitter.GetComponent<Rigidbody>().velocity = randomDeviation * (-ejectionVeolcity);
                }
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
        
       // mainCubeRb.AddForce(-cubeRB.velocity.normalized * pushFactor, ForceMode.VelocityChange);
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
