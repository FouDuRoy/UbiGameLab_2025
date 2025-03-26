using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Synchro2 : MonoBehaviour
{
    // Start is called before the first frame update
    public bool rotationFixed = true;
    Quaternion oldRotation;
    void Start()
    {
        oldRotation = transform.rotation * Quaternion.Inverse(this.GetComponentInParent<PlayerObjects>().player.transform.rotation);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        transform.position = this.GetComponentInParent<PlayerObjects>().player.transform.position;
        if (rotationFixed)
        {
            transform.rotation = oldRotation*this.GetComponentInParent<PlayerObjects>().player.transform.rotation;
        }
        else
        {
            oldRotation = transform.rotation*Quaternion.Inverse(this.GetComponentInParent<PlayerObjects>().player.transform.rotation);
        }
    }
}
