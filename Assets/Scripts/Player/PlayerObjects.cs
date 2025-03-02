using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.ProBuilder.Shapes;

public class PlayerObjects : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] public List<GameObject> cubes = new List<GameObject>();
    [SerializeField] public Dictionary<Vector3, GameObject> cubesHash = new Dictionary<Vector3, GameObject>();
    [SerializeField] public GameObject player;
    [SerializeField] public Rigidbody cubeRb;

    [SerializeField] public GameObject passiveCube;
    void Start()
    {

        cubesHash.Add(Vector3.zero, cubeRb.gameObject);
        cubes.Add(player);
    }

    // Update is called once per frame
    void Update()
    {
     
    }
    public bool removeCube(GameObject cube){
        cube.transform.parent = transform.parent;
        cube.GetComponent<Faces>().resetFaces();
        cubes.Remove(cube);
        cube.layer=0;
        foreach(var v in cubesHash){
            if(v.Value == cube){
                return cubesHash.Remove(v.Key);
            }
        }
    return false;        
    }
    public void addRigidBody(GameObject cube){
                cube.AddComponent<Rigidbody>();
                Rigidbody rb = cube.GetComponent<Rigidbody>();
                Rigidbody rb2 = passiveCube.GetComponent<Rigidbody>();
                rb.mass = rb2.mass;
                rb.drag = rb2.drag;
                rb.angularDrag = rb2.angularDrag;
                rb.collisionDetectionMode = rb2.collisionDetectionMode;
                rb.useGravity = rb2.useGravity;
                rb.constraints = rb2.constraints;
    }
}
