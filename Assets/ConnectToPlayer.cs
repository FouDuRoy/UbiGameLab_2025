using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.ProBuilder.Shapes;
using UnityEngine.UI;

public class ConnectToPlayer : MonoBehaviour
{

    [SerializeField] float passiveRadius = 5.0f;
    LayerMask mask;
    Rigidbody cubeRb;
   public  bool changeFoward;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (changeFoward)
        {
            this.transform.forward = Vector3.left;
        }


    }
}
