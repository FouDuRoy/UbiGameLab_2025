using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerObjects : MonoBehaviour
{
    // Start is called before the first frame update
    public List<GameObject> cubes = new List<GameObject>();
    [SerializeField] public Dictionary<Vector3, GameObject> cubesHash = new Dictionary<Vector3, GameObject>();
    public List<Vector3> staticPositions = new List<Vector3>();
    [SerializeField] public GameObject player;
    public Rigidbody cubeRb;
    void Start()
    {
       
    }

    // Update is called once per frame
    void Update()
    {
     
    }
}
