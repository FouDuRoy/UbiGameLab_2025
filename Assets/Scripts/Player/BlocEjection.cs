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
    [SerializeField] float energyLoss = 0.8f;
    [SerializeField] float upEjectionMax =1f;
    [SerializeField] float ejectionFactor=1f;
    [SerializeField] float maxAngle=5f;
    private GridSystem gridSystem;
    private PlayerObjects playerObjects;



    private void Start()
    {
        gridSystem = gameObject.transform.root.GetComponent<GridSystem>();
        playerObjects = FindObjectOfType<PlayerObjects>();
        upEjectionMax = Mathf.Clamp01(upEjectionMax);
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
             
            if (ownerHitter != ownerHitted && ownerHitter.Contains("Player") && ownerHitted.Contains("Player") && !ownerHitter.Contains("projectile"))
            {
                Vector3 relativeVelocity = collision.relativeVelocity;
                if (relativeVelocity.magnitude > velocityTreshold)
                {
                    // Calculate normal average
                    hitted.transform.root.GetComponent<PlayerObjects>().addRigidBody(hitted);
                    gridSystem.DetachBlock(hitted);
                    Vector3 ejectionVeolcity = relativeVelocity*energyLoss;
                    float ejectionMag = ejectionVeolcity.magnitude;
                    Vector3 hittedVelocity = (ejectionVeolcity.normalized*(1-randomHeightFactor)+Vector3.up*randomHeightFactor)*ejectionMag;
                    hitted.GetComponent<Rigidbody>().velocity = hittedVelocity*ejectionFactor;
                    StartCoroutine(blockNeutral(hitted));
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
             
            if (ownerHitter != ownerHitted  && ownerHitted.Contains("Player") && ownerHitter.Contains("projectile"))
            {
                Vector3 relativeVelocity = collision.relativeVelocity;
                if (relativeVelocity.magnitude > velocityTreshold)
                {

                    hitted.transform.root.GetComponent<PlayerObjects>().addRigidBody(hitted);
                    gridSystem.DetachBlock(hitted);
                    hitted.GetComponent<Bloc>().owner = "projectile";
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
        }
    }

    IEnumerator blockNeutral(GameObject block)
    {
        yield return new WaitForSeconds(3f);
        if (block != null)
        {
            block.GetComponent<Bloc>().setOwner("Neutral");
        }
    }
}
