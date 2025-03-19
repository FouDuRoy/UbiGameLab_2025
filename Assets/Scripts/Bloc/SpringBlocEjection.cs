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
    [SerializeField] float upEjectionMax = 1f;
    [SerializeField] float ejectionFactor = 1f;
    [SerializeField] float maxAngle = 5f;
    [SerializeField] float springBreakForce = 500f;
    [SerializeField] float range = 4;
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
        GameObject hitted = collision.GetContact(0).thisCollider.gameObject;
        Bloc hitterComponent = hitter.GetComponent<Bloc>();
        Bloc hittedComponent = hitted.GetComponent<Bloc>();
        float randomHeightFactor = Random.Range(0, upEjectionMax);

        checkCollisionBetweenPlayerAndBlock(collision, hitter, hitted, hitterComponent, hittedComponent);
        if (hitterComponent != null && hittedComponent != null)
        {

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
                mainCubeRb = transform.GetComponent<Rigidbody>();
                moveType = transform.root.GetComponent<PlayerMouvement>().moveType;

                Vector3 relativeVelocity = collision.relativeVelocity;
                Rigidbody hitterRB = hitter.transform.parent.GetComponent<Rigidbody>();
                Rigidbody hittedRB = hitted.transform.parent.GetComponent<Rigidbody>();
                float hitterVelocity = hitterRB.velocity.magnitude + hitterRB.angularVelocity.magnitude * (hitter.transform.position - hitterRB.position).magnitude;
                float hittedVelocityMag = hittedRB.velocity.magnitude + hittedRB.angularVelocity.magnitude* (hitted.transform.position - hittedRB.position).magnitude;
                Debug.Log(hitted.name + "velocity" + hitterVelocity);
                if (hitterVelocity > velocityTresholdMelee && hitterVelocity > hittedVelocityMag)
                {
                    foreach (var v in gridSystem.grid)
                    {
                        Joint[] joints = v.Value.GetComponents<Joint>();
                        foreach(Joint joint in joints)
                        {
                            if ((joint.transform.position - hitted.transform.position).magnitude < range)
                            {
                                joint.breakTorque = springBreakForce;

                            }
                        }
                    }
                    StartCoroutine(resetTorque(gridSystem));
                }
            }
            if (playerHittedByownMelee && areOwnedByPlayers)
            {
                gridSystem = gameObject.transform.root.GetComponent<GridSystem>();
                playerObjects = FindObjectOfType<PlayerObjects>();
                upEjectionMax = Mathf.Clamp01(upEjectionMax);
                mainCubeRb = transform.GetComponent<Rigidbody>();
                moveType = transform.root.GetComponent<PlayerMouvement>().moveType;

                Vector3 relativeVelocity = collision.relativeVelocity;
                float hitterVelocity = hitter.GetComponent<Rigidbody>().velocity.magnitude;
                float hittedVelocityMag = hitted.transform.parent.GetComponent<Rigidbody>().velocity.magnitude;
                if (hitterVelocity > velocityTresholdMelee && hitterVelocity > hittedVelocityMag)
                {
                    // Calculate normal average

                    gridSystem.DetachBlock(hitted);
                    hittedComponent.state = BlocState.melee;
                    Vector3 ejectionVeolcity = relativeVelocity * energyLoss;
                    float ejectionMag = ejectionVeolcity.magnitude;
                    Vector3 hittedVelocity = (ejectionVeolcity.normalized * (1 - randomHeightFactor) + Vector3.up * randomHeightFactor) * ejectionMag;
                    hitted.GetComponent<Rigidbody>().velocity = hittedVelocity * ejectionFactor;
                    //mainCubeRb.AddForce(ejectionVeolcity,ForceMode.VelocityChange);
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
            bool playerHittedByOwenBlock = ownerHitter == ownerHitted && stateHitted == BlocState.structure && stateHitter == BlocState.detached;
            if ((playerHittedByOwenBlock) && areOwnedByPlayers)
            {
                gridSystem = gameObject.transform.root.GetComponent<GridSystem>();
                playerObjects = FindObjectOfType<PlayerObjects>();
                upEjectionMax = Mathf.Clamp01(upEjectionMax);
                mainCubeRb = transform.GetComponent<Rigidbody>();
                moveType = transform.root.GetComponent<PlayerMouvement>().moveType;
                Vector3 relativeVelocity = collision.relativeVelocity;
                if (relativeVelocity.magnitude > velocityTreshold)
                {

                    gridSystem.DetachBlock(hitted);
                    hittedComponent.state = BlocState.detached;
                    Vector3 ejectionVeolcity = relativeVelocity * energyLoss;
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
                Vector3 relativeVelocity = collision.relativeVelocity;
                if (relativeVelocity.magnitude > velocityTreshold)
                {
                    gridSystem.DetachBlock(hitted);
                    hittedComponent.state = BlocState.detached;
                    Vector3 ejectionVeolcity = relativeVelocity * energyLoss;
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
        Debug.Log(breakForce);
        gridSystem = gameObject.transform.root.GetComponent<GridSystem>();
        gridSystem.DetachBlock(this.gameObject);
        this.GetComponent<Bloc>().state = BlocState.detached;
    }
    IEnumerator blockNeutral(GameObject block)
    {
        yield return new WaitForSeconds(3f);
        if (block != null)
        {
            block.GetComponent<Bloc>().setOwner("Neutral");

            block.GetComponent<Bloc>().state = BlocState.none;
        }
    }
    IEnumerator resetTorque(GridSystem grid)
    {
        yield return new WaitForSeconds(3f);

        foreach (var v in gridSystem.grid)
        {
            Joint[] joints = v.Value.GetComponents<Joint>();
            foreach (Joint joint in joints)
            {
                joint.breakTorque = 1000000000f;
            }
        }
    }
}
