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
    public float HyperViteTimer = 10f;
    bool alive = true;
    public bool active = true;
    string ownerName;
    Transform ownerTransform;
    private List<IEnumerator> powerUps = new List<IEnumerator>();

    private void Start()
    {
        powerUps.Add(GigaRepulsion());
        powerUps.Add(SuperDash());
        powerUps.Add(AttractionOmni());
        powerUps.Add(HyperVite());
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (alive && active)
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
                    explode();
                }
            }
        }
    }

    public void explode()
    {
        alive = false;
        Bloc myBloc = GetComponent<Bloc>();
        ownerTransform = myBloc.ownerTranform;
        if (myBloc.state == BlocState.structure)
        {
            ownerTransform.GetComponent<GridSystem>().DetachBlock(gameObject);
        }
        // Give power up to owner
        List<int> playerPowers = ownerTransform.GetComponent<PlayerObjects>().availablePowerUps;
        if(playerPowers.Count > 0)
        {
            int powerUpIndex = Random.Range(0, playerPowers.Count);
            Debug.Log(powerUps[playerPowers[powerUpIndex]]);
            StartCoroutine(powerUps[playerPowers[powerUpIndex]]);
            playerPowers.RemoveAt(powerUpIndex);
        }
        DisablePowerBLoc();
    }

    IEnumerator GigaRepulsion()
    {
        int oldNumberBlocs = ownerTransform.GetComponent<ConeEjectionAndProjection>().maxBlocs;
        ownerTransform.GetComponent<ConeEjectionAndProjection>().maxBlocs = gigaRepulsionNbBlocs;
        float time = 0;
        if (gigaRepulsionTimer == float.PositiveInfinity)
        {
            Destroy(gameObject);
        }
        else
        {
            while (time < gigaRepulsionTimer)
            {
                yield return new WaitForSeconds(Time.deltaTime);
                time += Time.deltaTime;
            }
            ownerTransform.GetComponent<ConeEjectionAndProjection>().maxBlocs = oldNumberBlocs;
            Destroy(gameObject);
        }


    }
    IEnumerator SuperDash()
    {
        ownerTransform.GetComponent<PlayerObjects>().cubeRb.GetComponent<Dash>().superDash = true;
        float time = 0;
        if (superDashTime == float.PositiveInfinity)
        {
            Destroy(gameObject);
        }
        else
        {
            while (time < superDashTime)
            {
                yield return new WaitForSeconds(Time.deltaTime);
                time += Time.deltaTime;
            }
            ownerTransform.GetComponent<PlayerObjects>().cubeRb.GetComponent<Dash>().superDash = false;
            Destroy(gameObject);
        }

    }

    IEnumerator AttractionOmni()
    {
        float oldAngle = ownerTransform.GetComponent<ConeEjectionAndProjection>().initialAngle;
        ownerTransform.GetComponent<ConeEjectionAndProjection>().initialAngle = 360;
        float time = 0;
        if (attractionOmniscienteTimer == float.PositiveInfinity)
        {
            Destroy(gameObject);
        }
        else
        {
            while (time < attractionOmniscienteTimer)
            {
                yield return new WaitForSeconds(Time.deltaTime);
                time += Time.deltaTime;
            }
            ownerTransform.GetComponent<ConeEjectionAndProjection>().initialAngle = oldAngle;
            Destroy(gameObject);
        }
    }
    IEnumerator HyperVite()
    {
        ownerTransform.GetComponent<PlayerMouvement>().moveType = MouvementType.HyperVite;
        float time = 0;
        if(HyperViteTimer == float.PositiveInfinity)
        {
            Destroy(gameObject);
        }
        else
        {
            while (time < HyperViteTimer)
            {
                yield return new WaitForSeconds(Time.deltaTime);
                time += Time.deltaTime;
            }
            ownerTransform.GetComponent<PlayerMouvement>().moveType = MouvementType.Joystick4;
            Destroy(gameObject);
        }
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

