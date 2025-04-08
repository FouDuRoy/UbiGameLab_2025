using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerUpBloc : MonoBehaviour
{
    public float resistance = 5f;

    private void OnCollisionEnter(Collision collision)
    {
        GameObject cube = collision.collider.gameObject;
        Bloc blocComp= cube.GetComponent<Bloc>();
        if (blocComp == null)
        {
            return;
        }
        string owner = blocComp.owner;
        if (owner.Contains("Player"))
        {
            if (collision.relativeVelocity.magnitude > resistance)
            {
                Bloc myBloc = GetComponent<Bloc>();
                if(myBloc.state == BlocState.structure)
                {
                   myBloc.ownerTranform.GetComponent<GridSystem>().DetachBlock(gameObject);
                }
                // Give power up to owner
                Debug.Log(owner + " is super powerfull");
                Destroy(gameObject);
            }
        }
    }
}

