using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using Unity.Mathematics;
using UnityEngine;
using Quaternion = UnityEngine.Quaternion;
using Random = UnityEngine.Random;
using Vector3 = UnityEngine.Vector3;
//Federico Barallobres
public class BlocEjection : MonoBehaviour
{
    [SerializeField] float velocityTreshold = 10f;
    [SerializeField] float velocityTresholdMelee = 10f;
    [SerializeField] float energyLoss = 0.8f;
    [SerializeField] float upEjectionMax =1f;
    [SerializeField] float ejectionFactor=1f;
    [SerializeField] float maxAngle=5f;
    private GridSystem gridSystem;
    private PlayerObjects playerObjects;
    Rigidbody mainCubeRb;



    private void Start()
    {
        gridSystem = gameObject.transform.root.GetComponent<GridSystem>();
        playerObjects = FindObjectOfType<PlayerObjects>();
        upEjectionMax = Mathf.Clamp01(upEjectionMax);
        mainCubeRb = transform.GetComponent<Rigidbody>();
    }

 
    void OnCollisionEnter(Collision collision)
    {

        GameObject hitter = collision.collider.gameObject;
        GameObject hitted = collision.GetContact(0).thisCollider.gameObject;
        Bloc hitterComponent = hitter.GetComponent<Bloc>();
        Bloc hittedComponent = hitted.GetComponent<Bloc>();
        float randomHeightFactor= Random.Range(0,upEjectionMax);

        checkCollisionBetweenPlayerAndBlock(collision, hitter, hitted, hitterComponent, hittedComponent);

        if (hitterComponent != null && hittedComponent != null)
        {

            string ownerHitter = hitterComponent.owner;
            string ownerHitted = hittedComponent.owner;
            BlocState stateHitter = hitterComponent.state;
            BlocState stateHitted = hittedComponent.state;

            bool playerHittedByotherPlayerStructure = ownerHitter != ownerHitted && stateHitted == BlocState.structure && stateHitter == BlocState.structure;
            bool areOwnedByPlayers = ownerHitter.Contains("Player") && ownerHitted.Contains("Player");
        
            if (playerHittedByotherPlayerStructure && areOwnedByPlayers)
            {
                Vector3 relativeVelocity = collision.relativeVelocity;
                float hitterVelocity = hitter.transform.parent.GetComponent<Rigidbody>().velocity.magnitude;
                float hittedVelocityMag = hitted.transform.parent.GetComponent<Rigidbody>().velocity.magnitude;
                if (hitterVelocity > velocityTresholdMelee && hitterVelocity> hittedVelocityMag)
                {
                    // Calculate normal average
                    hitted.transform.root.GetComponent<PlayerObjects>().addRigidBody(hitted);
                    gridSystem.DetachBlock(hitted);
                    hittedComponent.state = BlocState.melee;
                    Vector3 ejectionVeolcity = relativeVelocity*energyLoss;
                    float ejectionMag = ejectionVeolcity.magnitude;
                    Vector3 hittedVelocity = (ejectionVeolcity.normalized*(1-randomHeightFactor)+Vector3.up*randomHeightFactor)*ejectionMag;
                    hitted.GetComponent<Rigidbody>().velocity = hittedVelocity*ejectionFactor;
                    StartCoroutine(blockNeutral(hitted));
                    mainCubeRb.AddForce(ejectionVeolcity,ForceMode.VelocityChange);
                }
            }
            bool playerHittedByownMelee = ownerHitter == ownerHitted && stateHitted == BlocState.structure && stateHitter == BlocState.melee;
            if (playerHittedByownMelee && areOwnedByPlayers)
            {
                Vector3 relativeVelocity = collision.relativeVelocity;
                float hitterVelocity = hitter.GetComponent<Rigidbody>().velocity.magnitude;
                float hittedVelocityMag = hitted.transform.parent.GetComponent<Rigidbody>().velocity.magnitude;
                if (hitterVelocity > velocityTresholdMelee && hitterVelocity > hittedVelocityMag)
                {
                    // Calculate normal average
                    hitted.transform.root.GetComponent<PlayerObjects>().addRigidBody(hitted);
                    gridSystem.DetachBlock(hitted);
                    hittedComponent.state = BlocState.melee;
                    Vector3 ejectionVeolcity = relativeVelocity * energyLoss;
                    float ejectionMag = ejectionVeolcity.magnitude;
                    Vector3 hittedVelocity = (ejectionVeolcity.normalized * (1 - randomHeightFactor) + Vector3.up * randomHeightFactor) * ejectionMag;
                    hitted.GetComponent<Rigidbody>().velocity = hittedVelocity * ejectionFactor;
                    StartCoroutine(blockNeutral(hitted));
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
            BlocState stateHitter =hitterComponent.state;
            BlocState stateHitted = hittedComponent.state;
            bool areOwnedByPlayers = ownerHitter.Contains("Player") && ownerHitted.Contains("Player");
            bool playerHittedByotherPlayer = ownerHitter != ownerHitted && stateHitted == BlocState.structure && stateHitter == BlocState.projectile;
            bool playerHittedByOwenBlock = ownerHitter == ownerHitted && stateHitted == BlocState.structure && stateHitter == BlocState.detached;
            if (( playerHittedByOwenBlock)&& areOwnedByPlayers)
            {
                Vector3 relativeVelocity = collision.relativeVelocity;
                if (relativeVelocity.magnitude > velocityTreshold)
                {

                    hitted.transform.root.GetComponent<PlayerObjects>().addRigidBody(hitted);
                    gridSystem.DetachBlock(hitted);
                    hittedComponent.state = BlocState.detached;
                    Vector3 ejectionVeolcity = relativeVelocity*energyLoss;
                    float ejectionMag = ejectionVeolcity.magnitude;
                    float randomHeightFactor= Random.Range(0,upEjectionMax);

                    Vector3 hittedVelocity = (ejectionVeolcity.normalized*(1-randomHeightFactor)+Vector3.up*randomHeightFactor)*ejectionMag;
                    Quaternion randomDeviation = Quaternion.AngleAxis(Random.Range(-maxAngle,maxAngle),Vector3.up);
                    hitted.GetComponent<Rigidbody>().velocity = hittedVelocity*ejectionFactor;
                    hitter.GetComponent<Rigidbody>().velocity = randomDeviation*(-ejectionVeolcity);
                    StartCoroutine(blockNeutral(hitted));
                }
            }
            if ((playerHittedByotherPlayer) && areOwnedByPlayers)
            {
                Vector3 relativeVelocity = collision.relativeVelocity;
                if (relativeVelocity.magnitude > velocityTreshold)
                {

                    hitted.transform.root.GetComponent<PlayerObjects>().addRigidBody(hitted);
                    gridSystem.DetachBlock(hitted);
                    hittedComponent.state = BlocState.detached;
                    Vector3 ejectionVeolcity = relativeVelocity * energyLoss;
                    float ejectionMag = ejectionVeolcity.magnitude;
                    float randomHeightFactor = Random.Range(0, upEjectionMax);

                    Vector3 hittedVelocity = (ejectionVeolcity.normalized*(1-randomHeightFactor)+Vector3.up*randomHeightFactor)*ejectionMag;
                    Quaternion randomDeviation = Quaternion.AngleAxis(Random.Range(-maxAngle, maxAngle), Vector3.up);
                    hitted.GetComponent<Rigidbody>().velocity = hittedVelocity * ejectionFactor;
                    hitter.GetComponent<Rigidbody>().velocity = randomDeviation * (-ejectionVeolcity);
                    StartCoroutine(blockNeutral(hitted));
                }
            }
        }
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
}
