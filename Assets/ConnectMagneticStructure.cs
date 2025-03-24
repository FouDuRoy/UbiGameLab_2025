using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ConnectMagneticStructure : MonoBehaviour
{
    GridSystem playerGrid;
    // Start is called before the first frame update
    void Start()
    {
        playerGrid = GetComponent<GridSystem>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        List<Collider> magneticStructure= new List<Collider>();
        foreach (var v in playerGrid.grid) { 
           // List<Collider> magneticStructureBloc = Physics.OverlapSphere(transform.position, mask).ToList<Collider>();
           
        }
    }
}
