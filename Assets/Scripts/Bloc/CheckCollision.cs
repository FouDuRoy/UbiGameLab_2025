using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckCollision : MonoBehaviour
{
    public bool hasCollided = false;
    void Start()
    {
        
    }

    // Update is called once per frame
    private void OnCollisionEnter(Collision collision)
    {
        GameObject collider = collision.collider.gameObject;
        if (collider.tag != "ground")
        {
            hasCollided = true;
        }
    }
}
