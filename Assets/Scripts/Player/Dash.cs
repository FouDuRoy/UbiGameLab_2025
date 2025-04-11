using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.ProBuilder.Shapes;

public class Dash : MonoBehaviour
{
    [SerializeField] private float dashForce = 80f;
    [SerializeField] private float superDashForce = 120f;
    [SerializeField] private float dashDuration = .15f;
    [SerializeField] private float cooldown=1f;
    [SerializeField] private float dashStopTime = 0.05f;
    [SerializeField] private float magneticRecoveryTime = 0.25f;

    public bool superDash = false;
    public bool tutoDash=false;
    public bool canDash = true;

    private PlayerInfo playerInfo;
    private float lastDashTime;
    private bool isDashing = false;
    private List<GameObject> blocsToDestroy = new List<GameObject>();
    // Start is called before the first frame update
    void Start()
    {
        playerInfo = GetComponentInParent<PlayerInfo>();
        lastDashTime = -cooldown;
    }

    // Update is called once per frame
    public void TryToDash(Rigidbody playerRb, Transform playerGolem, PlayerMouvement playerMouvement)
    {
        if (Time.time > lastDashTime+cooldown && canDash)
        {
            StartCoroutine(dashCoroutine(playerRb, playerGolem, playerInfo));
            playerMouvement.ThrowCubes();
            lastDashTime = Time.time;
        }
    }

    private IEnumerator dashCoroutine(Rigidbody playerRb, Transform playerGolem, PlayerInfo playerInfo)
    {
        float OG_drag = playerRb.drag;
        float OG_angularDrag= playerRb.angularDrag;
        int oldLayer = playerRb.gameObject.layer;
        playerRb.gameObject.layer = 0;

        if (superDash) { playerRb.AddForce(playerGolem.transform.forward * superDashForce, ForceMode.VelocityChange); }
        else { playerRb.AddForce(playerGolem.transform.forward * dashForce, ForceMode.VelocityChange); }

        playerRb.angularDrag = 50000f;
        isDashing=true;
        playerInfo.invun = true;

        yield return new WaitForSeconds(dashDuration);
        float timeDash = 0;
        while (timeDash<1) {
            playerRb.velocity = Vector3.Lerp(playerRb.velocity, playerRb.velocity.normalized, timeDash);
            timeDash += Time.fixedDeltaTime / dashStopTime;
            yield return new WaitForFixedUpdate();
        }
        playerRb.drag=OG_drag;
        playerRb.angularDrag=OG_angularDrag;
       
        isDashing = false;
        if(!playerInfo.recovering)
            playerInfo.invun = false;

        yield return new WaitForSeconds(magneticRecoveryTime);
        playerRb.gameObject.layer = oldLayer;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (isDashing)
        {
            
            Bloc blocHit = collision.collider.gameObject.GetComponent<Bloc>();
            if (blocHit != null && blocHit.ownerTranform!= null)
            {
                Transform blocOwner = blocHit.ownerTranform;
                GridSystem grid = blocOwner.GetComponent<GridSystem>();

                if (blocHit.state == BlocState.structure && grid!=null)
                {
                    if (superDash || tutoDash)
                    {
                        foreach(var item in grid.grid)
                        {
                            GameObject bloc = item.Value;
                            blocsToDestroy.Add(bloc);
                            bloc.GetComponent<Bloc>().state = BlocState.projectile;

                         
                        }
                        if (tutoDash)
                        {
                            
                            blocsToDestroy.ForEach(bloc => { 
                                grid.DetachBlocSingle(bloc);
                                bloc.GetComponent<Rigidbody>().AddForce((bloc.transform.position-GetComponent<Rigidbody>().position).normalized*150f,ForceMode.VelocityChange);
                                if(bloc.tag == "magneticCube")
                                {
                                    Destroy(bloc.gameObject.GetComponentInChildren<TutoUI>().gameObject);
                                    bloc.GetComponent<BoxCollider>().enabled = false;
                                    bloc.transform.Find("SM_MagnetCube_02_centered").gameObject.SetActive(false);
                                    StartCoroutine(WaitToDestroy(4f, bloc));
                                }
                                
                            });
                            blocsToDestroy.Clear();
                            //tutoDash = false;
                        }
                        else
                        {
                            blocsToDestroy.ForEach(bloc => grid.DetachBlocSingle(bloc));
                            blocsToDestroy.Clear();
                        }
                  
                    }
                    else
                    {
                        grid.DetachBlock(blocHit.gameObject);
                        blocHit.state = BlocState.detached;
                    }                    
                }
            }
           
        }
        
    }
    IEnumerator WaitToDestroy(float time,GameObject cube)
    {
        
        yield return new WaitForSeconds(time);
        Destroy(cube);

    }

        
}
