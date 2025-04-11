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
            GameObject cubeCollided = gameObject;
            GameObject cubeCollider = collision.gameObject;
            
            Bloc blocCompCollided = cubeCollided.GetComponent<Bloc>();
            Bloc blocCompCollider = cubeCollider.GetComponent<Bloc>();

            if (blocCompCollided == null || blocCompCollider == null)
            {
                return;
            }
            ownerName = blocCompCollided.owner;
            string hitterName = blocCompCollider.owner;

            bool conditionHitted = hitterName.Contains("Player");
            if (conditionHitted)
            {
                if (collision.relativeVelocity.magnitude > resistance)
                {
                    
                        explode(blocCompCollider,blocCompCollided);
                    
                }
            }
        }
    }

    public void explode(Bloc colliderBloc,Bloc collidedBloc)
    {
        alive = false;
        ownerTransform = colliderBloc.ownerTranform;
        Transform blocTransform = collidedBloc.transform;
        if (blocTransform != null)
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
        else
        {
            Destroy(gameObject);
        }
        DisablePowerBLoc();
    }

    IEnumerator GigaRepulsion()
    {
        int oldNumberBlocs = ownerTransform.GetComponent<ConeEjectionAndProjection>().maxBlocs;
        ownerTransform.GetComponent<ConeEjectionAndProjection>().maxBlocs = gigaRepulsionNbBlocs;
        ownerTransform.GetComponent<PlayerObjects>().healthBar.GetComponent<HealthBar>().AddPowerup("GigaRepulsion",gigaRepulsionTimer);
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
        ownerTransform.GetComponent<PlayerObjects>().healthBar.GetComponent<HealthBar>().AddPowerup("GigaRepulsion", gigaRepulsionTimer);
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
        ownerTransform.GetComponent<PlayerObjects>().healthBar.GetComponent<HealthBar>().AddPowerup("GigaRepulsion", gigaRepulsionTimer);
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
        ownerTransform.GetComponent<PlayerObjects>().healthBar.GetComponent<HealthBar>().AddPowerup("GigaRepulsion", gigaRepulsionTimer);
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
       GetComponent<Bloc>().enabled = false;
       GetComponent<StoredVelocity>().enabled = false;
    }
}

