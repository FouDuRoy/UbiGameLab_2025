using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrackMouvement : MonoBehaviour
{
    // Start is called before the first frame update
    Transform mesh;
    Transform c1;
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {

        Transform[] tableauSousCubes = gameObject.GetComponentsInChildren<Transform>();
        Vector3 averagePosition = Vector3.zero;
        Quaternion averageRotation = Quaternion.identity;
        foreach (Transform c in tableauSousCubes)
        {
            if (c.name != "Mesh" && this.transform != c)
            {
                averagePosition += c.position;
                averageRotation *= c.rotation;
                if(c.name =="C1")
                    c1 = c;
            }
            else
            {
                mesh = c;
            }
            
        }
        mesh.position = averagePosition/(tableauSousCubes.Length-2);
        mesh.rotation = c1.rotation;
    }
}
