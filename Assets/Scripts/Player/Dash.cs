using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dash : MonoBehaviour
{
    [SerializeField] private float dashForce;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    public void DoDash(Rigidbody playerRb)
    {
        playerRb.AddForce(player dashForce, ForceMode.VelocityChange);
    }
}
