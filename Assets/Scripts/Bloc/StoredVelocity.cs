using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StoredVelocity : MonoBehaviour
{
    public Vector3 lastTickVelocity;
    Vector3 curentTickVelocity;
    public Rigidbody rb;
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        curentTickVelocity = Vector3.zero;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
     
        if (rb != null)
        {
            lastTickVelocity = curentTickVelocity;
            curentTickVelocity = rb.velocity;

        }

        
    }
}
