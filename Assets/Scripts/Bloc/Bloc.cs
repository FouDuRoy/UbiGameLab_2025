using System.Buffers;
using System.Collections.Generic;
using UnityEngine;
//Federico Barallobres
public class Bloc : MonoBehaviour
{
    [SerializeField] float minimalSpeed = 0.5f;
    [SerializeField] Material magneticMaterial;
    [SerializeField] float maxSpeed = 1000f;
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
    }
    public void SetGridPosition(Vector3Int pos)
    {
        gridPosition = pos;
    }

    public Vector3Int GetGridPosition()
    {
        return gridPosition;
    }

    void Update()
    {
        if (owner == "Neutral" && state == BlocState.nonMagnetic)
        {
            float speed = rb.velocity.magnitude;

            if (speed < minimalSpeed)
            {
                this.GetComponent<Feromagnetic>().enabled = true;
                state = BlocState.magnetic;
                this.GetComponent<MeshRenderer>().SetMaterials(materials);
            }

        }
        if(rb.velocity.magnitude > maxSpeed)
        {
            rb.velocity = rb.velocity.normalized * maxSpeed;
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
