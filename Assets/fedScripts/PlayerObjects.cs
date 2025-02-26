using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ProBuilder.Shapes;

public class PlayerObjects : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] public List<GameObject> cubes = new List<GameObject>();
    [SerializeField] public Dictionary<Vector3, GameObject> cubesHash = new Dictionary<Vector3, GameObject>();
    [SerializeField] public GameObject player;
    [SerializeField] public Rigidbody cubeRb;
    void Start()
    {
        
        cubesHash.Add(Vector3.zero, cubeRb.gameObject);
    }

    // Update is called once per frame
    void Update()
    {
       
    }
}
