using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerObjects : MonoBehaviour
{
    // Start is called before the first frame update
    public List<GameObject> cubes = new List<GameObject>();
    [SerializeField] public GameObject cubePrefab;
    public Rigidbody cubeRb;
    void Start()
    {
        cubeRb = cubePrefab.GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
