using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bloc : MonoBehaviour
{
    [SerializeField] float minimalSpeed = 0.5f;
    public string owner;
    public Rigidbody rb;
    //private bool activeMagnetism = true;

    void Start()
    {
       
    }

    void Update()
    {
        
        
        if( owner == "Neutral")
        {
            float speed = this.GetComponent<Rigidbody>().velocity.magnitude;
           
            if (speed < minimalSpeed)
            {
                this.GetComponent<Feromagnetic>().enabled = true;
            }
            
        }
    }
    public void setOwner(string owner)
    {
        this.owner = owner;
    }
  
}
