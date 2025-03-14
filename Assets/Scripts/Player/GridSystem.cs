using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class GridSystem : MonoBehaviour
{
    protected Dictionary<Vector3Int, GameObject> grid = new Dictionary<Vector3Int,  GameObject>();
    public GameObject kernel; // Le noyau du syst�me, point (0,0,0)
    public PlayerObjects playerObj;
    public Quaternion kernelRotI;
    public float cubeSize = 1.2f;
    [SerializeField] bool checkGrid = false;

    private void Start()
    {
        kernel = transform.GetComponent<PlayerObjects>().cubeRb.gameObject;
        grid.Add(new Vector3Int(0, 0, 0), kernel);
        playerObj = transform.GetComponent<PlayerObjects>();
    }

    void Update()
    {
        if (checkGrid)
        {
            foreach (var v in grid)
            {
                Debug.Log(v.Key);
            }
            checkGrid = false;
        }
    }

    public void AttachBlock(GameObject blocToAttach, GameObject attachedBloc, Vector3 closestFace)
    {

        Vector3Int fixedVector = new Vector3Int(Mathf.RoundToInt( (closestFace.x/ cubeSize))
            , Mathf.RoundToInt((closestFace.y / cubeSize)), Mathf.RoundToInt((closestFace.z / cubeSize)));
        if (grid.ContainsValue(attachedBloc))
        {
            grid.Add(fixedVector, blocToAttach);
            playerObj.weight += blocToAttach.GetComponent<Bloc>().weight;
        }
        else if (grid.ContainsValue(attachedBloc))
        {
            Vector3Int newGridPos = grid.FirstOrDefault(x => x.Value == attachedBloc).Key + fixedVector;
            playerObj.weight += blocToAttach.GetComponent<Bloc>().weight;
            grid.Add(newGridPos, blocToAttach);
        }
    }
    public void DetachBlock(GameObject bloc)
    {
        Vector3Int detachedGridPos = grid.FirstOrDefault(x => x.Value == bloc).Key;
        if (grid.ContainsKey(detachedGridPos) && grid[detachedGridPos] == bloc)
        {
            playerObj.removeCube(grid[detachedGridPos]);
            grid.Remove(detachedGridPos);
            playerObj.weight -= bloc.GetComponent<Bloc>().weight;
            List<Vector3Int> neighbors = GetNeighbors(detachedGridPos);
            //Ajouter les faces aux voisins

            foreach (Vector3Int voisin in neighbors)
            {

                if (grid.ContainsKey(voisin))
                {
                    Vector3 faceToAdd = (detachedGridPos - voisin);
                    faceToAdd = faceToAdd * cubeSize;
                }

            }
            List<Vector3Int> keys = new List<Vector3Int>();
            foreach (var gridBloc in grid)
            {
                if (!IsBlockConnected(gridBloc.Value))
                {
                    playerObj.addRigidBody(gridBloc.Value);
                    playerObj.removeCube(gridBloc.Value);
                    keys.Add(gridBloc.Key);
                    gridBloc.Value.GetComponent<Bloc>().owner = "projectile";
                    StartCoroutine(blockNeutral(gridBloc.Value));
                    neighbors = GetNeighbors(gridBloc.Key);
                    playerObj.weight -= gridBloc.Value.GetComponent<Bloc>().weight;
                    //Ajouter les faces aux voisins

                    foreach (Vector3Int voisin in neighbors){
                          if(grid.ContainsKey(voisin)){
                            Vector3 faceToAdd = (gridBloc.Key - voisin);
                            faceToAdd = faceToAdd*cubeSize;
                            grid[voisin].GetComponent<Faces>().addFace(faceToAdd);
                        }
                    }
                }

            }
            foreach (Vector3Int key in keys)
            {
                grid.Remove(key);
            }
        }

    }
    public void DetachBlocSingle(GameObject bloc)
    {
        Vector3Int detachedGridPos = grid.FirstOrDefault(x => x.Value == bloc).Key;
        if (grid.ContainsKey(detachedGridPos) && grid[detachedGridPos] == bloc)
        {
            playerObj.removeCube(grid[detachedGridPos]);
            grid.Remove(detachedGridPos);
            playerObj.weight -= bloc.GetComponent<Bloc>().weight;
            List<Vector3Int> neighbors = GetNeighbors(detachedGridPos);
            //Ajouter les faces aux voisins

            foreach (Vector3Int voisin in neighbors)
            {

                if (grid.ContainsKey(voisin))
                {
                    Vector3 faceToAdd = (detachedGridPos - voisin);
                    faceToAdd = faceToAdd * cubeSize;
                    grid[voisin].GetComponent<Faces>().addFace(faceToAdd);
                }

            }
        }
    }
    public void detachDisconnectedBlocks()
    {
        List<Vector3Int> keys = new List<Vector3Int>();
        List<Vector3Int> neighbors = new List<Vector3Int>();
        foreach (var gridBloc in grid)
        {
            if (!IsBlockConnected(gridBloc.Value))
            {
                neighbors = GetNeighbors(gridBloc.Key);
                playerObj.addRigidBody(gridBloc.Value);
                playerObj.removeCube(gridBloc.Value);
                keys.Add(gridBloc.Key);
                gridBloc.Value.GetComponent<Bloc>().owner = "projectile";
                StartCoroutine(blockNeutral(gridBloc.Value));
                neighbors = GetNeighbors(gridBloc.Key);
                playerObj.weight -= gridBloc.Value.GetComponent<Bloc>().weight;
                //Ajouter les faces aux voisins

                foreach (Vector3Int voisin in neighbors)
                {
                    if (grid.ContainsKey(voisin))
                    {
                        Vector3 faceToAdd = (gridBloc.Key - voisin);
                        faceToAdd = faceToAdd * cubeSize;
                        grid[voisin].GetComponent<Faces>().addFace(faceToAdd);
                    }
                }
            }

        }
        foreach (Vector3Int key in keys)
        {
            grid.Remove(key);
        }
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


    protected List<Vector3Int> GetNeighbors(Vector3Int position)
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

    protected bool IsBlockConnected(GameObject bloc)
    {
        if (!grid.ContainsValue(bloc)) return false;
        Vector3Int blocGridPos = grid.FirstOrDefault(x => x.Value == bloc).Key;
        HashSet<Vector3Int> visited = new HashSet<Vector3Int>();
        Queue<Vector3Int> toVisit = new Queue<Vector3Int>();
        toVisit.Enqueue(blocGridPos);

        while (toVisit.Count > 0)
        {
            Vector3Int current = toVisit.Dequeue();
            if (current == Vector3Int.zero) return true; // Le noyau est atteint
            visited.Add(current);

            foreach (Vector3Int neighbor in GetNeighbors(current))
            {
                if (grid.ContainsKey(neighbor) && !visited.Contains(neighbor))
                {
                    toVisit.Enqueue(neighbor);
                }
            }
        }
        return false; // Aucun chemin trouvé vers le noyau
    }

    public Vector3 getPositionOfObject(GameObject cube)
    {
        Vector3Int blocGridPos = grid.FirstOrDefault(x => x.Value == cube).Key;
        return new Vector3(blocGridPos.x * cubeSize, blocGridPos.y * cubeSize, blocGridPos.z * cubeSize);

    }
    public bool containsKey(Vector3 position)
    {
        Vector3Int key = new Vector3Int(Mathf.RoundToInt(position.x / cubeSize), Mathf.RoundToInt(position.y / cubeSize), Mathf.RoundToInt(position.z / cubeSize));
        return grid.ContainsKey(key);

    }
    public GameObject getObjectAtPosition(Vector3 position)
    {
        Vector3Int key = new Vector3Int(Mathf.RoundToInt(position.x / cubeSize), Mathf.RoundToInt(position.y / cubeSize), Mathf.RoundToInt(position.z / cubeSize));
        return grid[key];

    }

    public void clearGrid(){
        grid.Clear();
        grid.Add(new Vector3Int(0,0,0),kernel);
        playerObj.weight = 1;
    }
    public Vector3 tranformToVector3(Vector3Int position)
    {
        return new Vector3(position.x * cubeSize, position.y * cubeSize, position.z * cubeSize);
    }

    public List<Vector3>  getAvailableNeighbours(GameObject cube)
    {
        Vector3Int positionCube = grid.FirstOrDefault(x => x.Value == cube).Key;
        if(positionCube == null)
        {
            return null;
        }
        else
        {
            List<Vector3> availableSpaces = new List<Vector3>();
            List<Vector3Int> positions = GetNeighbors(positionCube);

            foreach (Vector3Int position in positions)
            {
                
                if (!grid.ContainsKey(position))
                {
                    availableSpaces.Add(tranformToVector3(position));
                }
            }

            return availableSpaces;
        }
    }

    public Vector3[] ClosestNeighbourPosition(GameObject cube,Vector3 position)
    {
        Vector3Int positionCube = grid.FirstOrDefault(x => x.Value == cube).Key;
        Vector3 cubePositionToKernel = tranformToVector3(positionCube);
       
        if (positionCube == null)
        {
            return null;
        }
        else
        {

            //Look for closest position relative to current position and rotation
            float scaleFactor = cube.transform.lossyScale.x;
            Vector3[] directions = new Vector3[] {cube.transform.forward*cubeSize*scaleFactor, -cube.transform.forward * cubeSize*scaleFactor
                , cube.transform.up * cubeSize*scaleFactor,-cube.transform.up*cubeSize*scaleFactor
                , cube.transform.right * cubeSize*scaleFactor,-cube.transform.right*cubeSize*scaleFactor };
            List<Vector3> directionsList = directions.ToList();
            directionsList.Sort((x,y) =>
            {
                float distanceX = (cube.transform.position + x - position).magnitude;
                float distanceY = (cube.transform.position + y - position).magnitude;
                return distanceX.CompareTo(distanceY);
            }); 

            int i = 0;
            bool foundPoint = false;
            Vector3 positionRelativeToKernel = Vector3.zero;
            Vector3 facePositionWorld = Vector3.zero;
            do
            {
                positionRelativeToKernel = cubePositionToKernel+ cube.transform.InverseTransformDirection(directionsList[i]).normalized*cubeSize;
                foundPoint = !containsKey(positionRelativeToKernel);
                if (foundPoint)
                {
                    facePositionWorld = directionsList[i]+cube.transform.position;
                }
                i++;
            }while(!foundPoint && i < directionsList.Count);
            
            return new Vector3[] {facePositionWorld,positionRelativeToKernel};

        }
    }
    
    public List<Vector3> getOccupiedNeighbours(GameObject cube)
    {
        Vector3Int positionCube = grid.FirstOrDefault(x => x.Value == cube).Key;
        if (positionCube == null)
        {
            return null;
        }
        else
        {
            List<Vector3> occupiedSpaces = new List<Vector3>();
            List<Vector3Int> positions = GetNeighbors(positionCube);

            foreach (Vector3Int position in positions)
            {
                if (grid.ContainsKey(position))
                {
                    
                    occupiedSpaces.Add(tranformToVector3(position));
                }
            }

            return occupiedSpaces;
        }
    }
    IEnumerator blockNeutral(GameObject block)
    {
        yield return new WaitForSeconds(3f);
        if (block != null)
        {
            block.GetComponent<Bloc>().setOwner("Neutral");
        }
    }
}
