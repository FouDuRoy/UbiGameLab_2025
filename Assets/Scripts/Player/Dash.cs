using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dash : MonoBehaviour
{
    [SerializeField] private float dashForce=50f;
    [SerializeField] private float cooldown=2f;

    private float lastDashTime;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    public void DoDash(Rigidbody playerRb, Transform playerGolem)
    {
        if (Time.time > lastDashTime+cooldown)
        {
            playerRb.AddForce(playerGolem.transform.forward * dashForce, ForceMode.VelocityChange);
            lastDashTime = Time.time;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        print("cacz");
    }
}
