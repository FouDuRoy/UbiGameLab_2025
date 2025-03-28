using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
//Federico Barallobres
public class SynchroGolem : MonoBehaviour
{
    // Start is called before the first frame update
    Quaternion initialRotation;
    Quaternion rot = Quaternion.identity;
    public bool lockRotation = false;
    Transform physicBody;
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (lockRotation)
        {
            transform.rotation = initialRotation*rot;
        }
        else
        {
            rot = Quaternion.identity;
        }
    }

    public void setLockRotation(bool lockRotation)
    {
        if(lockRotation)
        {
            Debug.Log("wow");
            initialRotation = transform.rotation;
        }
        this.lockRotation = lockRotation;
    }
    public void setRotationAdd(Quaternion rot)
    {
      this.rot = rot;
    }
}
