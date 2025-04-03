using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Dash : MonoBehaviour
{
    [SerializeField] private float dashForce = 80f;
    [SerializeField] private float dashDuration = .15f;
    [SerializeField] private float cooldown=1f;
    [SerializeField] private float dashStopTime = 0.05f;
    [SerializeField] private float magneticRecoveryTime = 0.25f;

    private PlayerInfo playerInfo;
    private float lastDashTime;
    private bool isDashing = false;

    // Start is called before the first frame update
    void Start()
    {
        playerInfo = GetComponentInParent<PlayerInfo>();
        lastDashTime = -cooldown;
    }

    // Update is called once per frame
    public void TryToDash(Rigidbody playerRb, Transform playerGolem, PlayerMouvement playerMouvement)
    {
        if (Time.time > lastDashTime+cooldown)
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
        playerRb.gameObject.layer = 0;
        playerRb.AddForce(playerGolem.transform.forward * dashForce, ForceMode.VelocityChange);
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
        playerRb.gameObject.layer = 3;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (isDashing)
        {
            Bloc blocHit = collision.gameObject.GetComponent<Bloc>();

            if (blocHit != null)
            {
                GridSystem grid = blocHit.GetComponentInParent<GridSystem>();

                if (blocHit.state == BlocState.structure && grid!=null)
                {
                    grid.DetachBlock(blocHit.gameObject);
                }
            }
        }
        
    }
}
