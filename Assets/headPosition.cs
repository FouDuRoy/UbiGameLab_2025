using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class headPosition : MonoBehaviour
{
    public GridSystem playerGrid;
    Vector3Int headPos = Vector3Int.up;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (playerGrid.grid.ContainsKey(headPos))
        {
            transform.position += Vector3.up;
            headPos += Vector3Int.up;
        }else if (!playerGrid.grid.ContainsKey(headPos - Vector3Int.up))
        {
            transform.position -= Vector3.up;
            headPos -= Vector3Int.up;
        }
    }
}
