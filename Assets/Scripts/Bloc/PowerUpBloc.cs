using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerUpBloc : MonoBehaviour
{
    public float resistance = 5f;
    public int gigaRepulsionNbBlocs = 10;
    public float gigaRepulsionTimer = 10f;

    public float superDashTime = 10f;
    public float attractionOmniscienteTimer = 10f;
    bool alive = true;
    string ownerName;
    Transform ownerTransform;
    private List<IEnumerator> powerUps = new List<IEnumerator>();

    private void Start()
    {
        powerUps.Add(GigaRepulsion());
        powerUps.Add(SuperDash());
        powerUps.Add(AttractionOmni());
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (alive)
        {
            GameObject cube = gameObject;
            Bloc blocComp = cube.GetComponent<Bloc>();
            if (blocComp == null)
            {
                return;
            }
            ownerName = blocComp.owner;
            if (ownerName.Contains("Player") && collision.collider.tag != "ground")
            {

                if (collision.relativeVelocity.magnitude > resistance)
                {
                    alive = false;
                    Debug.Log("Collided:" + collision.collider + "speed:" + collision.relativeVelocity.magnitude);
                    Bloc myBloc = GetComponent<Bloc>();
                    ownerTransform = myBloc.ownerTranform;
                    if (myBloc.state == BlocState.structure)
                    {
                        ownerTransform.GetComponent<GridSystem>().DetachBlock(gameObject);
                    }
                    // Give power up to owner
                    int powerUpIndex = Random.Range(0, powerUps.Count);
                    Debug.Log(powerUpIndex);
                    DisablePowerBLoc();
                    StartCoroutine(powerUps[powerUpIndex]);
                }
            }
        }
    }



    IEnumerator GigaRepulsion()
    {
        ownerTransform.GetComponent<ConeEjectionAndProjection>().maxBlocs = gigaRepulsionNbBlocs;
        yield return null;
        Destroy(gameObject);

    }
    IEnumerator SuperDash()
    {
        ownerTransform.GetComponent<PlayerObjects>().cubeRb.GetComponent<Dash>().superDash = true;
        yield return null;
        Destroy(gameObject);

    }

    IEnumerator AttractionOmni()
    {
        ownerTransform.GetComponent<ConeEjectionAndProjection>().initialAngle = 360;
        yield return null;
        Destroy(gameObject);

    }
    private void DisablePowerBLoc()
    {
       GetComponent<BoxCollider>().enabled = false;
       GetComponent<Feromagnetic>().enabled = false;
       GetComponent<Bloc>().enabled = false;
       GetComponent<SpringBlocEjection>().enabled = false;
       GetComponent<StoredVelocity>().enabled = false;
        GetComponent<DragAfterImpact>().enabled = false;
    }
}

