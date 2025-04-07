using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DragAfterImpact : MonoBehaviour
{
    public bool ejected = false;
    public bool colided = false;
    public float dragAfterImpact = 10f;
    Rigidbody cubeRb;
    // Start is called before the first frame update
    void Start()
    {
        cubeRb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        if(cubeRb == null)
        {
            cubeRb = GetComponent<Rigidbody>();
        }
        if (colided && GetComponent<Bloc>().state == BlocState.projectile)
        {
            cubeRb.drag = dragAfterImpact;
            colided = false;
        }
    }
    private void OnCollisionEnter(Collision collision)
    {
        Bloc hitted = collision.gameObject.GetComponent<Bloc>();
        Transform owner = null;
        if (hitted != null)
        {
             owner = hitted.ownerTranform;
        }
        if (owner != null)
        {
            if(owner.name.Contains("Player") && this.GetComponent<Bloc>().owner!= hitted.owner && ejected)
            {
                colided = true;
                ejected = false;
            }
        }
    }
}
