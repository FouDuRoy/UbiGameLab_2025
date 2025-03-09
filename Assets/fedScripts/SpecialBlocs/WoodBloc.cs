using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WoodBloc : MonoBehaviour
{
    [Header("Wood Block Properties")]
    public float resistance = 5f;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.relativeVelocity.magnitude > resistance)
        {
            Destroy(gameObject);
        }
    }
}