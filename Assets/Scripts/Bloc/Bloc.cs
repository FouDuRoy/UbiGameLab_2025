using System.Buffers;
using System.Collections.Generic;
using UnityEngine;
//Federico Barallobres
public class Bloc : MonoBehaviour
{
    [SerializeField] float minimalSpeed = 0.5f;
    [SerializeField] Material magneticMaterial;
    public float weight;
    public string owner; 
    public BlocState state;
    public Transform ownerTranform;

    List<Material> materials = new List<Material>();
    private Vector3Int gridPosition;

    private void Start()
    {
        materials.Add(magneticMaterial);
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
            float speed = this.GetComponent<Rigidbody>().velocity.magnitude;

            if (speed < minimalSpeed)
            {
                this.GetComponent<Feromagnetic>().enabled = true;
                state = BlocState.magnetic;
                this.GetComponent<MeshRenderer>().SetMaterials(materials);
            }

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
