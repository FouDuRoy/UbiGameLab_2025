using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.ProBuilder;

public class GridSystem : MonoBehaviour
{
    private Dictionary<Vector3Int, GameObject> grid = new Dictionary<Vector3Int, GameObject>();
    public GameObject kernel; // Le noyau du système, point (0,0,0)

    private void Start()
    {
        grid.Add(new Vector3Int(0, 0, 0), kernel);
    }

    public void AttachBlock(GameObject blocToAttach, GameObject attachedBloc, Vector3 closestFace)
    {
        Vector3Int fixedVector = new Vector3Int(Mathf.RoundToInt(closestFace.x), Mathf.RoundToInt(closestFace.y), Mathf.RoundToInt(closestFace.z));
        if (attachedBloc.name == "MainBody")
        {
            grid.Add(fixedVector, blocToAttach);
            Debug.Log(fixedVector.ToString());
        }
        else if (grid.ContainsValue(attachedBloc))
        {
            Vector3Int newGridPos = grid.FirstOrDefault(x => x.Value == attachedBloc).Key + fixedVector;
            grid.Add(newGridPos, blocToAttach);
            Debug.Log(newGridPos.ToString());
        }
    }
    public void DetachBlock(GameObject bloc)
    {
        Vector3Int detachedGridPos = grid.FirstOrDefault(x => x.Value == bloc).Key;
        grid.Remove(detachedGridPos);
        CheckAndDetachDisconnectedBlocks();
    }

    public void CheckAndDetachDisconnectedBlocks()
    {
        HashSet<Vector3Int> safeBlocks = new HashSet<Vector3Int>();
        HashSet<Vector3Int> visited = new HashSet<Vector3Int>();
        List<GameObject> detachedBlocks = new List<GameObject>();

        foreach (var kvp in grid)
        {
            if (!visited.Contains(kvp.Key))
            {
                List<Vector3Int> currentBranch = new List<Vector3Int>();
                if (IsBranchConnectedToKernel(kvp.Key, visited, currentBranch))
                {
                    safeBlocks.UnionWith(currentBranch);
                }
                else
                {
                    foreach (Vector3Int pos in currentBranch)
                    {
                        detachedBlocks.Add(grid[pos]);
                    }
                }
            }
        }

        foreach (GameObject bloc in detachedBlocks)
        {
            Vector3Int detachedGridPos = grid.FirstOrDefault(x => x.Value == bloc).Key;
            Debug.Log(bloc.name + " " + detachedBlocks);
            grid.Remove(bloc.GetComponent<Bloc>().GetGridPosition());
            bloc.transform.parent = null;
            bloc.GetComponent<Rigidbody>().isKinematic = false;
        }
    }

    private bool IsBranchConnectedToKernel(Vector3Int start, HashSet<Vector3Int> visited, List<Vector3Int> currentBranch)
    {
        if (!grid.ContainsKey(start) || visited.Contains(start)) return false;

        Queue<Vector3Int> toCheck = new Queue<Vector3Int>();
        HashSet<Vector3Int> localVisited = new HashSet<Vector3Int>();
        toCheck.Enqueue(start);
        bool isConnectedToKernel = false;

        while (toCheck.Count > 0)
        {
            Vector3Int current = toCheck.Dequeue();
            if (visited.Contains(current)) continue;

            visited.Add(current);
            currentBranch.Add(current);
            localVisited.Add(current);

            if (current == Vector3Int.zero) // Si on trouve le noyau
            {
                isConnectedToKernel = true;
            }

            foreach (Vector3Int neighbor in GetNeighbors(current))
            {
                if (grid.ContainsKey(neighbor) && !localVisited.Contains(neighbor))
                {
                    toCheck.Enqueue(neighbor);
                }
            }
        }

        return isConnectedToKernel;
    }


    private List<Vector3Int> GetNeighbors(Vector3Int position)
    {
        List<Vector3Int> neighbors = new List<Vector3Int>
        {
            position + Vector3Int.right,
            position + Vector3Int.left,
            position + Vector3Int.up,
            position + Vector3Int.down,
            position + new Vector3Int(0, 0, 1),
            position + new Vector3Int(0, 0, -1)
        };
        return neighbors;
    }
}
