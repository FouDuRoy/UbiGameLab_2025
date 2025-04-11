using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckCollision : MonoBehaviour
{
    public bool hasCollided = false;
    public GameObject collided;
    public bool checkCollision = false;
    void Start()
    {
        
    }

    // Update is called once per frame
    private void OnCollisionEnter(Collision collision)
    {
        if (checkCollision)
        {
            GameObject collider = collision.collider.gameObject;
            Bloc bloc = GetComponent<Bloc>();
            Transform ownerTransform = bloc.ownerTranform;
            if (bloc != null && collider.tag != "ground" && bloc.state == BlocState.projectile)
            {
                if (ownerTransform.name.Contains("Player"))
                {
                    if (collider.GetComponent<Bloc>() == null)
                    {
                        hasCollided = true;
                        collided = collider;
                    }
                    else if (collider.GetComponent<Bloc>().owner != bloc.owner)
                    {
                        hasCollided = true;
                        collided = collider;
                    }

                }
                else
                {
                    hasCollided = true;
                    collided = collider;
                }
            }
        }
    }
}
