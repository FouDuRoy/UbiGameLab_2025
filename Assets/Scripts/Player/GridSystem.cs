using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridSystem : MonoBehaviour
{
    private Dictionary<Vector3Int, GameObject> grid = new Dictionary<Vector3Int, GameObject>();
    public GameObject kernel; // Le noyau du système, point (0,0,0)

    public void AttachBlock(GameObject block)
    {
        Vector3Int gridPos = GetGridPosition(block.transform.position);
        if (!grid.ContainsKey(gridPos))
        {
            grid[gridPos] = block;
            block.GetComponent<Bloc>().SetGridPosition(gridPos); // Stocker la position relative dans le bloc
        }
    }

    public void DetachBlock(GameObject block)
    {
        Vector3Int gridPos = block.GetComponent<Bloc>().GetGridPosition();
        if (grid.ContainsKey(gridPos))
        {
            grid.Remove(gridPos);
        }

        // Vérifier les blocs déconnectés du noyau après détachement
        List<GameObject> detachedBlocks = GetDisconnectedBlocks();
        foreach (GameObject detachedBlock in detachedBlocks)
        {
            DetachBlock(detachedBlock); // Détacher récursivement
            detachedBlock.GetComponent<Rigidbody>().isKinematic = false; // Permet la physique
        }
    }

    private List<GameObject> GetDisconnectedBlocks()
    {
        List<GameObject> disconnectedBlocks = new List<GameObject>();
        HashSet<Vector3Int> visited = new HashSet<Vector3Int>();
        Queue<Vector3Int> toCheck = new Queue<Vector3Int>();

        // Ajouter la position du noyau (0,0,0) dans la liste de vérification
        Vector3Int kernelPos = Vector3Int.zero;
        if (grid.ContainsKey(kernelPos))
        {
            toCheck.Enqueue(kernelPos);
        }

        // Parcours en largeur pour marquer tous les blocs connectés
        while (toCheck.Count > 0)
        {
            Vector3Int currentPos = toCheck.Dequeue();
            visited.Add(currentPos);

            // Vérifier les voisins adjacents
            foreach (Vector3Int neighbor in GetNeighbors(currentPos))
            {
                if (grid.ContainsKey(neighbor) && !visited.Contains(neighbor))
                {
                    toCheck.Enqueue(neighbor);
                }
            }
        }

        // Identifier les blocs non connectés
        foreach (var kvp in grid)
        {
            if (!visited.Contains(kvp.Key))
            {
                disconnectedBlocks.Add(kvp.Value);
            }
        }

        return disconnectedBlocks;
    }

    private Vector3Int GetGridPosition(Vector3 position)
    {
        return new Vector3Int(
            Mathf.RoundToInt(position.x),
            Mathf.RoundToInt(position.y),
            Mathf.RoundToInt(position.z)
        );
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
