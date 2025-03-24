using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GridSystem : MonoBehaviour
{
    [SerializeField] bool checkGrid = false;
    [SerializeField] Material playerMat;

    public Dictionary<Vector3Int, GameObject> grid = new Dictionary<Vector3Int, GameObject>();
    public GameObject kernel; // Le noyau du syst�me, point (0,0,0)
    public PlayerObjects playerObj;
    public Quaternion kernelRotI;
    public float cubeSize = 1.2f;
    List<Material> materials = new List<Material>();
    HapticFeedbackController feedback;
    private void Start()
    {
        kernel = transform.GetComponent<PlayerObjects>().cubeRb.gameObject;
        grid.Add(new Vector3Int(0, 0, 0), kernel);
        playerObj = transform.GetComponent<PlayerObjects>();

        feedback = GetComponent<HapticFeedbackController>();
        materials.Add(playerMat);

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
        Vector3Int fixedVector = new Vector3Int(Mathf.RoundToInt((closestFace.x / cubeSize))
            , Mathf.RoundToInt((closestFace.y / cubeSize)), Mathf.RoundToInt((closestFace.z / cubeSize)));
        blocToAttach.GetComponent<Bloc>().ownerTranform = this.transform;
        if (grid.ContainsValue(attachedBloc))
        {
            grid.Add(fixedVector, blocToAttach);
            playerObj.weight += blocToAttach.GetComponent<Bloc>().weight;
            blocToAttach.GetComponent<Bloc>().setOwner(transform.root.gameObject.name);
            blocToAttach.gameObject.GetComponent<MeshRenderer>().SetMaterials(materials);
        }

        //Déclenche un feedback à chaque bloc qui s'attache
        if (feedback != null)
        {
            feedback.BlocAttachedVibration();
        }
    }
    public void AttachGrid(GridSystem gridToAttach, GameObject attachedBloc, GameObject blocToAttach, Vector3 closestFace)
    {
        Vector3Int systemACoordinates = new Vector3Int(Mathf.RoundToInt((closestFace.x / cubeSize))
            , Mathf.RoundToInt((closestFace.y / cubeSize)), Mathf.RoundToInt((closestFace.z / cubeSize)));

        Vector3 blocToattachPositionB = gridToAttach.getPositionOfObject(blocToAttach);
        Vector3Int systemBCoordinates = new Vector3Int(Mathf.RoundToInt((blocToattachPositionB.x / cubeSize))
            , Mathf.RoundToInt((blocToattachPositionB.y / cubeSize)), Mathf.RoundToInt((blocToattachPositionB.z / cubeSize)));
        Vector3Int changeOfCoordinatesBtoA = systemACoordinates - systemBCoordinates;

        foreach (var v in gridToAttach.grid)
        {
            if (!grid.ContainsValue(v.Value))
            {
                Vector3 blocAttachToA = kernel.transform.InverseTransformPoint(gridToAttach.kernel.transform
          .TransformPoint(gridToAttach.getPositionOfObject(v.Value)));
                Vector3Int intCord = new Vector3Int(Mathf.RoundToInt((blocAttachToA.x / cubeSize))
                    , Mathf.RoundToInt((blocAttachToA.y / cubeSize)), Mathf.RoundToInt((blocAttachToA.z / cubeSize)));

                grid.Add(intCord, v.Value);
                v.Value.GetComponent<Bloc>().setOwner(transform.root.gameObject.name);
                v.Value.GetComponent<Bloc>().state = BlocState.structure;
                v.Value.GetComponent<MeshRenderer>().SetMaterials(materials);
            }
        
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



            List<Vector3Int> keys = new List<Vector3Int>();
            foreach (var gridBloc in grid)
            {
                if (!IsBlockConnected(gridBloc.Value))
                {

                    playerObj.removeCube(gridBloc.Value);
                    keys.Add(gridBloc.Key);
                    gridBloc.Value.GetComponent<Bloc>().state = BlocState.detached;
                    neighbors = GetNeighbors(gridBloc.Key);
                    playerObj.weight -= gridBloc.Value.GetComponent<Bloc>().weight;
                    gridBloc.Value.GetComponent<Rigidbody>().AddForce(
                        (gridBloc.Value.transform.position - kernel.transform.position).normalized * 10f, ForceMode.VelocityChange);
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

    public void clearGrid()
    {
        grid.Clear();
        grid.Add(new Vector3Int(0, 0, 0), kernel);
        playerObj.weight = 1;
    }
    public Vector3 tranformToVector3(Vector3Int position)
    {
        return new Vector3(position.x * cubeSize, position.y * cubeSize, position.z * cubeSize);
    }

    public List<Vector3> getAvailableNeighbours(GameObject cube)
    {
        Vector3Int positionCube = grid.FirstOrDefault(x => x.Value == cube).Key;
        if (positionCube == null)
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

    public Vector3[] ClosestNeighbourPosition(GameObject cube, Vector3 position)
    {
        Vector3Int positionCube = grid.FirstOrDefault(x => x.Value == cube).Key;
        Vector3 cubePositionToKernel = tranformToVector3(positionCube);

        if (positionCube == null)
        {
            return null;
        }
        else
        {
            Transform cubeTransform = cube.transform;
            if (cube.transform.Find("Orientation") != null)
            {
                cubeTransform = cube.transform.Find("Orientation");
            }
            //Look for closest position relative to current position and rotation
            float scaleFactor = cubeTransform.lossyScale.x;
            Vector3[] directions = new Vector3[] {cubeTransform.forward*cubeSize*scaleFactor, -cubeTransform.forward * cubeSize*scaleFactor
                , cubeTransform.up * cubeSize*scaleFactor,-cubeTransform.up*cubeSize*scaleFactor
                , cubeTransform.right * cubeSize*scaleFactor,-cubeTransform.right*cubeSize*scaleFactor };
            List<Vector3> directionsList = directions.ToList();
            directionsList.Sort((x, y) =>
            {
                float distanceX = (cubeTransform.position + x - position).magnitude;
                float distanceY = (cubeTransform.position + y - position).magnitude;
                return distanceX.CompareTo(distanceY);
            });

            int i = 0;
            bool foundPoint = false;
            Vector3 positionRelativeToKernel = Vector3.zero;
            Vector3 facePositionWorld = Vector3.zero;
            do
            {
                positionRelativeToKernel = cubePositionToKernel + cubeTransform.InverseTransformDirection(directionsList[i]).normalized * cubeSize;
                foundPoint = !containsKey(positionRelativeToKernel);
                if (foundPoint)
                {
                    facePositionWorld = directionsList[i] + cubeTransform.position;
                }
                i++;
            } while (!foundPoint && i < directionsList.Count);

            return new Vector3[] { facePositionWorld, positionRelativeToKernel };

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


    public Vector3 FindMaxDimensions()
    {
        float maxX = grid.Keys.Max(x => x.x);
        float minX = grid.Keys.Min(x => x.x);
        float maxY = grid.Keys.Max(x => x.y);
        return new Vector3(maxX, minX, maxY);
    }
    public void coneEjectRest(float ejectionSpeed, float rightDriftProportion)
    {
        List<Vector3Int> keys = new List<Vector3Int>();
        foreach (var gridBloc in grid)
        {
            if (!IsBlockConnected(gridBloc.Value))
            {
                GameObject cube = gridBloc.Value;
                playerObj.removeCube(gridBloc.Value);
                keys.Add(gridBloc.Key);
                gridBloc.Value.GetComponent<Bloc>().state = BlocState.projectile;
                playerObj.weight -= gridBloc.Value.GetComponent<Bloc>().weight;

                Transform golem = kernel.transform.Find("GolemBuilt");
                float rightDrift = golem.InverseTransformPoint(cube.transform.position).x;
                cube.GetComponent<Rigidbody>().interpolation = RigidbodyInterpolation.Interpolate;
                cube.GetComponent<Rigidbody>().AddForce((golem.forward + golem.right * rightDrift * rightDriftProportion) * ejectionSpeed, ForceMode.VelocityChange);

            }

        }
        foreach (Vector3Int key in keys)
        {
            grid.Remove(key);
        }
    }

}
