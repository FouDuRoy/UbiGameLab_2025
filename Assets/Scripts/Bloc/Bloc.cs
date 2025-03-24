using System.Buffers;
using System.Collections.Generic;
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

    List<Material> materials = new List<Material>();
    private Vector3Int gridPosition;
    Rigidbody rb;

    private void Start()
    {
        materials.Add(magneticMaterial);
        rb = GetComponent<Rigidbody>();
        this.GetComponent<BoxCollider>().contactOffset = contactOffset;
    }

    public Vector3Int GetGridPosition()
    {
        return gridPosition;
    }

    void Update()
    {
        if (state != BlocState.structure && owner != "Neutral" && this.GetComponent<Feromagnetic>() != null)
        {
            float speed = rb.velocity.magnitude;

            //If speed is small enough we enable script
            if (speed < minimalSpeed && state == BlocState.magnetic)
            {
                this.GetComponent<Feromagnetic>().enabled = true;
                this.GetComponent<MeshRenderer>().SetMaterials(materials);
                owner = "Neutral";
                ownerTranform = null;
            }

        }
        if (rb.velocity.magnitude > maxSpeed)
        { 
            //rb.velocity = rb.velocity.normalized * maxSpeed;
            Debug.Log("break!");
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
}
