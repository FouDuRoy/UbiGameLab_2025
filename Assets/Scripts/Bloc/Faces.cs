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
        resetFaces();
    }

    // Update is called once per frame

    public Vector3 removeClosestFace(Vector3 face)
    {
        Vector3 closest = Vector3.zero; 
       for (int i = 0; i < faces.Count; i++) 
        
       {
            float distance = Vector3.Distance(faces[i], face);
            if (distance< 0.01f)
            {
                closest = faces[i];
                faces.Remove(faces[i]);
                return closest;
                
            }
        }
        return Vector3.zero;
    }
    public void resetFaces(){
         float cubeSize = 1f + spacingBetweenCubes;
        Vector3[] face ={ new Vector3(cubeSize, 0, 0), new Vector3(-cubeSize, 0, 0) , 
        new Vector3(0, 0, cubeSize),new Vector3(0, 0, -cubeSize), new Vector3(0, cubeSize, 0), new Vector3(0, -cubeSize, 0)};
        faces = face.ToList();
    }
}
