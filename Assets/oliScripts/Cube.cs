using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cube : MonoBehaviour
{
    [SerializeField] float minimalSpeed = 0.5f;
    public string owner;
    private Rigidbody rb;
    //private bool activeMagnetism = true;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        
        float speed = rb.velocity.magnitude;
        if(speed < minimalSpeed && owner == "Neutral")
        {
            this.GetComponent<Feromagnetic>().enabled = true;
        }
    }
    public void setOwner(string owner)
    {
        this.owner = owner;
    }
}
