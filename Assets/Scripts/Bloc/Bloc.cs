using UnityEngine;
//Federico Barallobres
public class Bloc : MonoBehaviour
{
    [SerializeField] float minimalSpeed = 0.5f;
    public float weight;
    public string owner;
    public Rigidbody rb;

    private Vector3Int gridPosition;

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
        if (owner == "Neutral")
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
