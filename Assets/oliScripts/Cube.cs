using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cube : MonoBehaviour
{
    public GameObject owner;
    private Rigidbody rb;
    private bool activeMagnetism = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        float speed = rb.velocity.magnitude;
        if(speed < 0.5f && !activeMagnetism)
        {
            activeMagnetism = true;
            this.GetComponent<Feromagnetic>().enabled = true;
        }
    }
}
