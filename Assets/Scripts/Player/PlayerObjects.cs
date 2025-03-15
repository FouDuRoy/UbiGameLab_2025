using System.Collections.Generic;
using UnityEngine;
//Federico Barallobres
public class PlayerObjects : MonoBehaviour
{
    [SerializeField] public List<GameObject> cubes = new List<GameObject>();
    [SerializeField] public Dictionary<Vector3, GameObject> cubesHash = new Dictionary<Vector3, GameObject>();
    [SerializeField] public GameObject player;
    [SerializeField] public Rigidbody cubeRb;
    [SerializeField] public GameObject passiveCube;

    protected GridSystem gridSystem;
    public float weight=1;

    void Start()
    {
        gridSystem = FindObjectOfType<GridSystem>();
        cubes.Add(player);
        weight = 1;
    }

    public bool removeCube(GameObject cube)
    {
        cube.transform.parent = transform.parent;
        cubes.Remove(cube);
        cube.layer = 0;
        Destroy(cube.GetComponent<SphereCollider>());

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
