using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class SynchroGolem : MonoBehaviour
{
    // Start is called before the first frame update
    Quaternion initialRotation;
    public bool lockRotation = false;
    Transform physicBody;
    void Start()
    {
        physicBody = transform.parent.GetComponent<PlayerObjects>().cubeRb.transform; 
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (lockRotation)
        {
            transform.rotation = initialRotation;
        }
    }

    public void setLockRotation(bool lockRotation)
    {
        if(lockRotation)
        {
            initialRotation = transform.rotation;
        }
        this.lockRotation = lockRotation;
    }
}
