using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class PlayerObjects : MonoBehaviour
{
    [SerializeField] public List<GameObject> cubes = new List<GameObject>();
    [SerializeField] public Dictionary<Vector3, GameObject> cubesHash = new Dictionary<Vector3, GameObject>();
    [SerializeField] public GameObject player;
    [SerializeField] public Rigidbody cubeRb;
    [SerializeField] public GameObject passiveCube;
    public GridSystem gridSystem;

    void Start()
    {
        gridSystem = FindObjectOfType<GridSystem>();
        cubesHash.Add(Vector3.zero, cubeRb.gameObject);
        cubes.Add(player);
    }

    public bool removeCube(GameObject cube)
    {
        cube.transform.parent = transform.parent;
        cube.GetComponent<Faces>().resetFaces();
        cubes.Remove(cube);
        cube.layer = 0;
        Destroy(cube.GetComponent<SphereCollider>());
        gridSystem.DetachBlock(cube); // Supprime de la grille

        foreach (var v in cubesHash)
        {
            if (v.Value == cube)
            {
                return cubesHash.Remove(v.Key);
            }
        }
        return false;
    }

    public void addRigidBody(GameObject cube)
    {
        if (!cube.TryGetComponent<Rigidbody>(out _))
        {
            Rigidbody rb = cube.AddComponent<Rigidbody>();
            Rigidbody rb2 = passiveCube.GetComponent<Rigidbody>();

            rb.mass = rb2.mass;
            rb.drag = rb2.drag;
            rb.angularDrag = rb2.angularDrag;
            rb.collisionDetectionMode = rb2.collisionDetectionMode;
            rb.useGravity = rb2.useGravity;
            rb.constraints = rb2.constraints;
        }
    }
}
