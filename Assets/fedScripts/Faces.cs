using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class Faces : MonoBehaviour
{
    public List<Vector3> faces;
    [SerializeField] float spacingBetweenCubes = 0.2f;
    // Start is called before the first frame update
    void Start()
    {
        float cubeSize = 1f + spacingBetweenCubes;
        Vector3[] face ={ new Vector3(cubeSize, 0, 0), new Vector3(-cubeSize, 0, 0) , new Vector3(0, cubeSize, 0),
            new Vector3(0, -cubeSize, 0), new Vector3(0, 0, cubeSize),new Vector3(0, 0, -cubeSize) };
        faces = face.ToList();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
