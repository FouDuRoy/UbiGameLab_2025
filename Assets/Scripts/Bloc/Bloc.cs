using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
//Federico Barallobres
public class Bloc : MonoBehaviour
{
    [SerializeField] float minimalSpeed = 0.5f;
    [SerializeField] Material magneticMaterial;
    [SerializeField] float maxSpeed = 1000f;
    public float contactOffset = 0.1f;
    public float weight;
    public string owner; 
    public BlocState state;
    public Transform ownerTranform;

    public GameObject objectToChangeMesh;
    MeshRenderer meshToChange; 
    private Vector3Int gridPosition;
    public Rigidbody rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        this.GetComponent<BoxCollider>().contactOffset = contactOffset;
        if(objectToChangeMesh != null){
         meshToChange = objectToChangeMesh.GetComponent<MeshRenderer>();
        }
    }

    public Vector3Int GetGridPosition()
    {
        return gridPosition;
    }

    void Update()
    {
      
        if (state != BlocState.structure && owner != "Neutral" )
        {
            float speed = rb.velocity.magnitude;
            //If speed is small enough we enable script
            if (speed < minimalSpeed && state == BlocState.magnetic )
            {
                if( this.GetComponent<Feromagnetic>() != null)
                {
                    this.GetComponent<Feromagnetic>().enabled = true;
                  
                }
                if(tag!="explosive"){
                    changeMeshMaterial(magneticMaterial);
                }
                owner = "Neutral";
                ownerTranform = null;
            }
            
            
        }
        

    }
    void FixedUpdate()
    {
        if(rb != null)
        {
            rb.maxLinearVelocity = maxSpeed;

        }
        
    }
    public void setOwner(string owner)
    {
        this.owner = owner;
    }
    public void setState(BlocState state)
    {
        this.state = state;
    }

    public void changeMeshMaterial(Material mat){
        meshToChange.material = mat;
    }
     public void changeMeshMaterialColor(Color color){
        meshToChange.material.color = color;
    }
}
