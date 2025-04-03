using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.ProBuilder;
using UnityEngine.ProBuilder.Shapes;

public class ConnectMagneticStructure : MonoBehaviour
{
    [SerializeField] float passiveRadius = 5.0f;


    [SerializeField] float maxSpeed = 10f;
    [SerializeField] float error = 0.05f;
    [SerializeField] float AngleError = 0.05f;


    [SerializeField] float moveTime = 0.1f;


    [SerializeField] float lerpingDistance;

    [SerializeField] float timeBeforeSwitching = 0.5f;

    [SerializeField] float xLimit = 0f;

    [SerializeField] float AngleLimit = 0f;

    [SerializeField] float mainLinearDrive = 1000f;

    [SerializeField] float mainLinearDamp = 50f;

    [SerializeField] float secondaryLinearDrive = 5000f;

    [SerializeField] float secondaryLinearDamp = 100f;

    [SerializeField] float angularDrive = 5000f;

    [SerializeField] float angularDamp = 100f;
    [SerializeField] bool projection = true;
    [SerializeField] public float springTorqueBreak = 1000f;
    [SerializeField] public float springForceBreak = 1000f;

    LayerMask mask;
    GridSystem playerGrid;
    Rigidbody cubeRB;

    Transform cubeAttractedToTransform;

    Transform playerAtractedTo;
    Transform playerMainCube;

    bool lerping = false;

    Vector3 endPositionRelativeToAttractedCube;

    Vector3 startPositionRelativeToAttractedCube;
    Vector3 closestFaceRelativeToWorld;
    Vector3 closestFaceRelativeToMainCube;

    Vector3 relativePositionToMainCube;
    GameObject closestCube;
    GameObject closestCubeOwn;

    Quaternion endRotationRelativeToAttractedCube;

    Quaternion startRotationRelativeToAttractedCube;

    List<Quaternion> quaternions;

    float errorP = 1;

    float errorR = 1;

    float timer = 0;

    float t = 0;

    Collider[] magnetic = new Collider[1000];
    // Start is called before the first frame update
    void Awake()
    {
        playerGrid = GetComponent<GridSystem>();
        quaternions = createListAngles();
        cubeRB = GetComponent<Rigidbody>();
    }
    void OnEnable()
    {
        ResetObject();
    }
    // Update is called once per frame
    public void ResetObject()
    {   
        foreach (var v in playerGrid.grid)
            {
                v.Value.layer = LayerMask.NameToLayer("magneticStructure");
            }
        lerping = false;
        timer = 0;
        cubeAttractedToTransform = null;
        playerAtractedTo = null;
        t = 0;
        relativePositionToMainCube = Vector3.zero;
        closestFaceRelativeToWorld = Vector3.zero;
        closestFaceRelativeToMainCube = Vector3.zero;
        closestCube = null;
        closestCubeOwn = null;
        errorP = 1;
        errorR = 1;
       
        
        cubeRB.useGravity = true;

        
        transform.parent = this.transform.root.parent;
       
    }
    void FixedUpdate()
    {

        if (!LookPositionGridAvailable() && lerping)
        {
            ResetObject();
        }
        if (cubeAttractedToTransform != null && cubeAttractedToTransform.root.GetComponent<PlayerObjects>() == null)
        {
            ResetObject();
        }
        if(closestCubeOwn != null && !playerGrid.grid.ContainsValue(closestCubeOwn)) {
            ResetObject();
        }
        if (!lerping)
        {
            
            //Trouver la distance la plus petite par rapport � la structucture 
            int i = 0;
            closestCube = null;
            float shortestDistance = 1000f;
            mask = LayerMask.GetMask("magnetic");
            int maxX = playerGrid.grid.Keys.Max(x => x.x);
            int minX = playerGrid.grid.Keys.Min(x => x.x);
            int maxZ = playerGrid.grid.Keys.Max(x => x.z);
            int minZ = playerGrid.grid.Keys.Min(x => x.z);
            int radiusInBlocs = Mathf.Max(Mathf.Abs(maxX), Mathf.Abs(minX), Mathf.Abs(maxZ), Mathf.Abs(minZ));
            float blocSizeWorld = playerGrid.cubeSize * playerGrid.kernel.transform.lossyScale.x;
            float radius = radiusInBlocs * blocSizeWorld;
            cubeRB = this.GetComponent<Rigidbody>();
            int hits = Physics.OverlapSphereNonAlloc(transform.position, radius, magnetic, mask);
            //Remove magnets not available
            for (int j = 0; j < hits; j++)
            {
                GridSystem grid;
                bool hasGrid = magnetic[j].transform.root.TryGetComponent<GridSystem>(out grid);
             
                if ( (magnetic[j].transform.root != null && !magnetic[j].transform.root.name.Contains("Player"))
                || !hasGrid || !grid.hasNeighbours(magnetic[j].gameObject) )
                {
                    magnetic[j] = null;
                }
            }
         
            if(hits > 0)
            {
                foreach (var v in playerGrid.grid)
                {
                    if(playerGrid.hasNeighbours(v.Value)){
                         if (i == 0)
                    {
                        closestCube = CheckClosestMagnet(magnetic, v.Value.transform, hits);
                        closestCubeOwn = v.Value;
                        if (closestCube == null)
                        {
                            shortestDistance = Mathf.Infinity;
                        }
                        else
                        {
                            shortestDistance = Vector3.Distance(closestCube.transform.position, v.Value.transform.position);

                        }
                    }
                    else
                    {
                        GameObject comparedCube = CheckClosestMagnet(magnetic, v.Value.transform, hits);
                        float comparedDistance;

                        if (comparedCube == null)
                        {
                            comparedDistance = Mathf.Infinity;
                        }
                        else
                        {
                            comparedDistance = Vector3.Distance(comparedCube.transform.position, v.Value.transform.position);
                        }

                        if (comparedDistance < shortestDistance)
                        {
                            shortestDistance = comparedDistance;
                            closestCube = comparedCube;
                            closestCubeOwn = v.Value;
                        }
                    }
                    i++;
                    }
                }
            }

            if (closestCube != null)
            {

                CheckClosestFace(closestCube);
                Vector3 direction = cubeAttractedToTransform.position - closestCubeOwn.transform.position;
                relativePositionToMainCube = playerMainCube.InverseTransformPoint(closestCubeOwn.transform.position);
                lerpingMagents(direction, relativePositionToMainCube, closestFaceRelativeToWorld);

            }
        }
        else
        {

            VelocityLerping();

        }


    }

    private void VelocityLerping()
    {
        float distance = (closestFaceRelativeToWorld - closestCubeOwn.transform.position).magnitude;

        if (lerping && (errorP > error || errorR > AngleError) && (distance < lerpingDistance && timer < timeBeforeSwitching))
        {
            //Once its locked 
            timer += Time.fixedDeltaTime;
            t = Mathf.Clamp01(timer / moveTime);
            float rotationSpeed = moveTime / Time.fixedDeltaTime;
            Vector3 absoluteStartP = cubeAttractedToTransform.TransformPoint(startPositionRelativeToAttractedCube);
            Vector3 absoluteEndPosition = cubeAttractedToTransform.TransformPoint(endPositionRelativeToAttractedCube);
            absoluteEndPosition=absoluteEndPosition - (closestCubeOwn.transform.position - transform.position);
            Vector3 desiredVelocity = (absoluteEndPosition - transform.position)/Time.fixedDeltaTime;
            Vector3 relativeVelocity = (cubeAttractedToTransform.GetComponent<Rigidbody>().velocity);

            Vector3 velocity = Vector3.Lerp(relativeVelocity, desiredVelocity, t);
            Quaternion absoluteRoatationStart = cubeAttractedToTransform.rotation * startRotationRelativeToAttractedCube;
            Quaternion absoluteEndRotation = cubeAttractedToTransform.rotation * endRotationRelativeToAttractedCube;
            Quaternion newRotation = Quaternion.Slerp(absoluteRoatationStart, absoluteEndRotation, t);
            Quaternion rotationDelta = newRotation * Quaternion.Inverse(cubeRB.rotation);
            rotationDelta.ToAngleAxis(out float angle, out Vector3 axis);
            axis.Normalize();
            Vector3 angularVelocity = axis * (angle * Mathf.Deg2Rad) * rotationSpeed * 2f;



            if (velocity.magnitude > maxSpeed)
            {
                velocity = velocity.normalized * maxSpeed;
            }
            if (angularVelocity.magnitude > maxSpeed)
            {
                angularVelocity = angularVelocity.normalized * maxSpeed;
            }

            //update velocity and rotation
            cubeRB.velocity = velocity;
            cubeRB.angularVelocity = angularVelocity;
            errorP = Vector3.Distance(absoluteEndPosition, cubeRB.position);
            errorR = Quaternion.Angle(absoluteEndRotation, cubeRB.rotation);
        }
        else if (!(errorP > error || errorR > AngleError))
        {
            //Set location and velocity
            cubeRB.velocity = Vector3.zero;
            Quaternion absoluteEndRotation = cubeAttractedToTransform.rotation * endRotationRelativeToAttractedCube;
            transform.rotation = absoluteEndRotation;
            Vector3 absoluteEndPosition = cubeAttractedToTransform.TransformPoint(endPositionRelativeToAttractedCube);
            absoluteEndPosition=absoluteEndPosition - (closestCubeOwn.transform.position - transform.position);

            transform.position = absoluteEndPosition;
          

            AttachCube();
        }
        else
        {
            ResetObject();
        }
    }
    private void AttachCube()
    {
        //Attach magnetic field
        List<GameObject> childsList = new List<GameObject>();
        for (int i = 0; i < this.transform.childCount; i++)
        {
            if(transform.GetChild(i).GetComponent<Bloc>() != null)
            {
                childsList.Add(transform.GetChild(i).gameObject);
            }
        }
        foreach (var v in playerGrid.grid)
        {
            v.Value.layer = 3;
        }

        //Set magnetic rb
        cubeRB = this.GetComponent<Rigidbody>();
        cubeRB.mass = 0.01f;
        cubeRB.interpolation = RigidbodyInterpolation.Interpolate;
        cubeRB.drag = 5f;
        cubeRB.angularDrag = 5f;

        closestCubeOwn.GetComponent<Bloc>().setOwner(transform.root.gameObject.name);
        GridSystem playerAtrcatedGrid = playerAtractedTo.GetComponent<GridSystem>();
        PlayerObjects playerAtractedObjects = playerAtractedTo.GetComponent<PlayerObjects>();
        playerAtrcatedGrid.AttachGrid(playerGrid,  cubeAttractedToTransform.gameObject, closestCubeOwn, closestFaceRelativeToMainCube);
        Quaternion rotationAmount = Quaternion.Inverse(transform.Find("Orientation").transform.rotation) * cubeAttractedToTransform.Find("Orientation").rotation;
        foreach (var v in playerGrid.grid)
        {
            v.Value.transform.Find("Orientation").rotation *= rotationAmount;
        }
        playerGrid.clearGrid();
       
        foreach (var v in playerAtrcatedGrid.grid)
        {
            Rigidbody currentCubeRb = v.Value.GetComponent<Rigidbody>();
            if(currentCubeRb == null)
            {
                playerAtractedObjects.addRigidBody(v.Value);
                
            }
        }
        foreach (var v in playerAtrcatedGrid.grid)
        {
            
            Rigidbody currentCubeRb = v.Value.GetComponent<Rigidbody>();
           if(playerAtractedObjects.cubeRb != currentCubeRb )
            {
                currentCubeRb.mass = 0.01f;
                currentCubeRb.interpolation = RigidbodyInterpolation.Interpolate;
                currentCubeRb.drag = 5f;
                currentCubeRb.angularDrag = 5f;
                currentCubeRb.useGravity = false;
            }
           if(currentCubeRb.transform.parent == this.transform || currentCubeRb.transform == transform)
            {
                attachJ(v.Value);
                

            }

        }

        //Disable script
        transform.parent = playerAtractedObjects.cubeRb.transform;
        foreach (GameObject child in childsList)
        {
            child.transform.parent = playerAtractedObjects.cubeRb.transform;
        }
        
        this.GetComponent<ConnectMagneticStructure>().enabled = false;
    }

    private void attachJ(GameObject attachingCube)
    {
        GridSystem cubeGrid = playerAtractedTo.GetComponent<GridSystem>();
        List<Vector3> occupiedSpaces = cubeGrid.getOccupiedNeighbours(attachingCube);
        Rigidbody attachingCubeRB= attachingCube.GetComponent<Rigidbody>();
      

        foreach (Vector3 cubeAttachToPosition in occupiedSpaces)
        {
      
            GameObject toConnectTo = cubeGrid.getObjectAtPosition(cubeAttachToPosition);
            
            attachingCube.AddComponent<ConfigurableJoint>();
            List<ConfigurableJoint> joints = attachingCube.GetComponents<ConfigurableJoint>().ToList();
            joints.RemoveAll(joint => joint.connectedBody != null);
            ConfigurableJoint joint = joints.First();
            float maxForce = 5000000000000000000f;
            if (joint.connectedBody == null)
            {

                JointDrive xDrive = joint.xDrive;
                xDrive.positionSpring = mainLinearDrive;
                xDrive.positionDamper = mainLinearDamp;
                xDrive.maximumForce = maxForce;
                joint.xDrive = xDrive;

                JointDrive yDrive = joint.yDrive;
                yDrive.positionSpring = secondaryLinearDrive;
                yDrive.positionDamper = secondaryLinearDamp;
                yDrive.maximumForce = maxForce;
                joint.yDrive = yDrive;

                JointDrive zDrive = joint.zDrive;
                zDrive.positionSpring = secondaryLinearDrive;
                zDrive.positionDamper = secondaryLinearDamp;
                zDrive.maximumForce = maxForce;
                joint.zDrive = zDrive;

                JointDrive angularXDrive = joint.angularXDrive;
                angularXDrive.positionSpring = mainLinearDrive;
                angularXDrive.positionDamper = mainLinearDamp;
                angularXDrive.maximumForce = maxForce;
                joint.angularXDrive = angularXDrive;

                JointDrive angularYZDrive = joint.angularYZDrive;
                angularYZDrive.positionSpring = mainLinearDrive;
                angularYZDrive.positionDamper = mainLinearDamp;
                angularYZDrive.maximumForce = maxForce;
                joint.angularYZDrive = angularYZDrive;

                JointDrive slerpDrive = joint.slerpDrive;
                slerpDrive.positionSpring = angularDrive;
                slerpDrive.positionDamper = angularDamp;
                slerpDrive.maximumForce = maxForce;
                joint.slerpDrive = slerpDrive;
                joint.rotationDriveMode = RotationDriveMode.Slerp;

                joint.projectionAngle = 0;
                joint.projectionDistance = 0f;

                joint.angularYMotion = ConfigurableJointMotion.Limited;
                joint.angularXMotion = ConfigurableJointMotion.Limited;
                joint.angularZMotion = ConfigurableJointMotion.Limited;

                SoftJointLimit limitAy = new SoftJointLimit();
                limitAy.limit = AngleLimit;// angleLimit
                joint.angularYLimit = limitAy;

                SoftJointLimit limitAx1 = new SoftJointLimit();
                limitAx1.limit = -2;// angleLimit
                joint.lowAngularXLimit = limitAx1;

                SoftJointLimit limitAx = new SoftJointLimit();
                limitAx.limit = 2;// angleLimit
                joint.highAngularXLimit = limitAx;

                SoftJointLimit limitAZ = new SoftJointLimit();
                limitAZ.limit = AngleLimit;// angleLimit
                joint.angularZLimit = limitAZ;

                joint.yMotion = ConfigurableJointMotion.Limited;
                joint.xMotion = ConfigurableJointMotion.Limited;
                joint.zMotion = ConfigurableJointMotion.Limited;


                SoftJointLimit limitX = new SoftJointLimit();
                limitX.limit = xLimit;
                joint.linearLimit = limitX;

                joint.connectedBody = toConnectTo.GetComponent<Rigidbody>();
                joint.anchor = Vector3.zero;
                joint.autoConfigureConnectedAnchor = false;
                joint.axis = -attachingCube.transform.InverseTransformDirection(attachingCube.transform.Find("Orientation").forward);
                joint.secondaryAxis = Vector3.up;
                Vector3 positionBeforeCorrection = toConnectTo.transform.InverseTransformPoint(attachingCube.transform.position);
                if (toConnectTo.transform.Find("Orientation") != null)
                {
                    joint.connectedAnchor = toConnectTo.transform.InverseTransformPoint(toConnectTo.transform.Find("Orientation")
                        .TransformPoint(cubeGrid.getPositionOfObject(attachingCube) - cubeAttachToPosition));

                }
                else
                {
                    joint.connectedAnchor = cubeGrid.getPositionOfObject(attachingCube) - cubeAttachToPosition;
                }
                
         

                joint.breakTorque = springTorqueBreak;
                joint.breakForce = springForceBreak;
                joint.enableCollision = false;
                if (projection)
                {
                    joint.projectionMode = JointProjectionMode.PositionAndRotation;
                    joint.projectionAngle = AngleLimit;
                    joint.projectionDistance = xLimit;
                }

            }
        }
    }
    GameObject CheckClosestMagnet(Collider[] magnetic, Transform cube,int hits)
    {
        GameObject closestCube = null;
        bool first = true;
        float shortDistance = -1;
        if (hits > 0)
        {
            for (int i = 0; i < hits; i++)
            {
                if (magnetic[i] != null)
                {
                    if (first)
                    {

                        closestCube = magnetic[i].gameObject;
                        shortDistance = (closestCube.transform.position - cube.position).sqrMagnitude;
                        first = false;
                    }
                    else
                    {
                        float distance = (magnetic[i].transform.position - cube.position).sqrMagnitude;
                        if (distance < shortDistance)
                        {
                            shortDistance = distance;
                            closestCube = magnetic[i].gameObject;
                        }
                    }
                }
            }
           
        }
        return closestCube;
    }
    void CheckClosestFace(GameObject cube)
    {
        cubeAttractedToTransform = cube.transform;
        playerMainCube = cubeAttractedToTransform.root.GetComponent<PlayerObjects>().player.transform;
        playerAtractedTo = cubeAttractedToTransform.root;

        Vector3[] list = playerAtractedTo.GetComponent<GridSystem>().ClosestNeighbourPosition(cubeAttractedToTransform.gameObject, closestCubeOwn.transform.position);
        closestFaceRelativeToWorld = list[0];
        closestFaceRelativeToMainCube = list[1];

    }

    private void lerpingMagents(Vector3 direction, Vector3 relativeDirection, Vector3 closestFaceRelativeToWorld)
    {
        // If the distance is bigger than lerpingDistance or the position is not available anymore we keep pushing with CoulombLaw
        if ((closestCubeOwn.transform.position - closestFaceRelativeToWorld).magnitude < lerpingDistance && LookPositionGridAvailable())
        {
            //closestCubeOwn.transform.parent = cubeAttractedToTransform;
            playerAtractedTo = closestCube.transform.root;

            //Start moving towards final positiond
            lerping = true;
            startPositionRelativeToAttractedCube = cubeAttractedToTransform.InverseTransformPoint(closestCubeOwn.transform.position) - (closestCubeOwn.transform.position - transform.position); ;
            startRotationRelativeToAttractedCube = Quaternion.Inverse(cubeAttractedToTransform.rotation) * transform.rotation;
            endPositionRelativeToAttractedCube = cubeAttractedToTransform.InverseTransformPoint(closestFaceRelativeToWorld);
            endRotationRelativeToAttractedCube = RotationChoice(startRotationRelativeToAttractedCube);
            cubeRB.useGravity = false;
            cubeRB.GetComponent<Rigidbody>().mass = 0.001f;
            foreach (var v in playerGrid.grid)
            {
                v.Value.layer = LayerMask.NameToLayer("magneticStructureConnect");
            }
            Physics.IgnoreLayerCollision(LayerMask.NameToLayer("magneticStructureConnect"),LayerMask.NameToLayer("Wall"));
        }
    }
    public bool LookPositionGridAvailable()
    {

        bool avaialble = true;
        if (playerAtractedTo != null)
        {
            Transform mainBodyCube = playerAtractedTo.GetComponent<PlayerObjects>().cubeRb.transform;
            GridSystem grid = playerAtractedTo.GetComponent<GridSystem>();
            Vector3 positionFromMainBody;
            if (mainBodyCube.Find("Orientation") != null)
            {
                positionFromMainBody = mainBodyCube.Find("Orientation").InverseTransformPoint(closestFaceRelativeToWorld);
            }
            else
            {
                positionFromMainBody = mainBodyCube.InverseTransformPoint(closestFaceRelativeToWorld);

            }
            avaialble = !grid.containsKey(positionFromMainBody);
        }
        return avaialble;
    }

    private Quaternion RotationChoice(Quaternion blocRotation)
    {

        Quaternion direction = quaternions[0];

        foreach (Quaternion dir in quaternions)
        {
            if (Quaternion.Angle(blocRotation, direction) > Quaternion.Angle(blocRotation, dir))
            {
                direction = dir;
            }
        }
        return direction;
    }
    public List<Quaternion> createListAngles()
    {
        List<Quaternion> list = new List<Quaternion>();
        for (int i = 0; i <= 4; i++)
        {
            for (int j = 0; j <= 4; j++)
            {
                for (int k = 0; k <= 4; k++)
                {
                    list.Add(Quaternion.Euler(90 * i, 90 * j, 90 * k));
                }
            }
        }
        return list;
    }
}
