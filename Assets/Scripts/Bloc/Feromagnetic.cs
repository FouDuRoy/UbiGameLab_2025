using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;
//Federico Barallobres
public class Feromagnetic : MonoBehaviour
{
    private const float timeBeforeActiveMagnet = 0;
    // Settings Magnetisme

    [SerializeField] float passiveRadius = 5.0f;
    [SerializeField] float activeRadius = 5f;
    [SerializeField] float charge = 5.0f;
    [SerializeField] float maxSpeed = 10f;
    [SerializeField] float maxSpeedVariation = 3f;
    [SerializeField] float error = 0.05f;
    [SerializeField] float AngleError = 0.05f;
    [SerializeField] float moveTime = 0.1f;
    [SerializeField] int maxForceAttraction = 30;
    [SerializeField] float lerpingDistance;
    [SerializeField] float timeBeforeSwitching = 0.5f;
    [SerializeField] float timeBeforeSwitchingVariation = 0.05f;
    [SerializeField] public float springTorqueBreak = 1000f;
    [SerializeField] public float springForceBreak = 1000f;
    [SerializeField] float lerpingMass = 0.001f;
    [SerializeField] float structureMass = 0.01f;
    [SerializeField] float structureDrag = 5f;
    [SerializeField] float structureAngularDrag = 5f;

    //Joint settings

    [SerializeField] SpringType springType;
    [SerializeField] LerpingType lerpingType;
    [SerializeField] float xLimit = 0f;
    [SerializeField] float AngleLimit = 0f;
    [SerializeField] float mainLinearDrive = 1000f;
    [SerializeField] float mainLinearDamp = 50f;
    [SerializeField] float secondaryLinearDrive = 5000f;
    [SerializeField] float secondaryLinearDamp = 100f;
    [SerializeField] float angularDrive = 5000f;
    [SerializeField] float angularDamp = 100f;
    [SerializeField] bool projection = true;

    LayerMask mask;

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


    Quaternion endRotationRelativeToAttractedCube;
    Quaternion startRotationRelativeToAttractedCube;
    List<Quaternion> quaternions;


    float errorP = 1;
    float errorR = 1;
    float timer = 0;
    float t = 0;

    PlayerObjects cubeAtractedToPlayerObjects = null;
    GridSystem playerAttractedToGrid = null;

    Collider[] magnetic = new Collider[1000];
    void Start()
    {
        mask = LayerMask.GetMask("magneticPlayer1") | LayerMask.GetMask("magneticStructure") | LayerMask.GetMask("magneticPlayer2") ;
        // We assume all cubes have same scale
        quaternions = createListAngles();
        timeBeforeSwitching += Random.Range(-timeBeforeSwitchingVariation, timeBeforeSwitchingVariation);
        maxSpeed += Random.Range(-maxSpeedVariation, maxSpeedVariation);

    }

    void OnEnable()
    {
        // When enable again we reset object
        ResetObject();
    }
    public float getPassiveRadius()
    {
        return passiveRadius;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        // On regarde en premier si la position vers laquelle on se dirige est disponible

        if (!LookPositionGridAvailable() && lerping)
        {
            ResetObject();
        }

        //We reset if there is problem with target
        if (cubeAttractedToTransform != null && cubeAtractedToPlayerObjects == null)
        {
            ResetObject();
        }

        // Set the position of the block and look for all magnetic blocs in range
        if (!lerping)
        {
            //Regarde les cubes magentiques dans le range du cube. 
            int hits = Physics.OverlapSphereNonAlloc(transform.position, passiveRadius, magnetic, mask);
            cubeRB = this.GetComponent<Rigidbody>();

            //Remove magnets not available
            for (int i = 0; i < hits; i++)
            {
                GridSystem grid = magnetic[i].transform.root.GetComponent<GridSystem>();
                if ((grid == null || grid.getAvailableNeighbours(magnetic[i].gameObject).Count == 0))
                {
                    magnetic[i] = null;
                }
            }

            //Check for closest magnetic cube
            CheckClosestMagnet(magnetic, hits);

            //If there is a magnet in range execute attraction algo
            if (cubeAttractedToTransform != null && (transform.position - closestFaceRelativeToWorld).magnitude <2)
            {
                Vector3 direction = cubeAttractedToTransform.position - transform.position;
                relativePositionToMainCube = playerMainCube.InverseTransformPoint(transform.position);
                lerpingMagents(direction, relativePositionToMainCube, closestFaceRelativeToWorld);
            }
        }
        else
        {
            if (lerpingType == LerpingType.velocityLerping)
            {
                VelocityLerping();
            }
            else if (lerpingType == LerpingType.transformLerping)
            {
                TransformLerping();
            }
        }
    }

    void CheckClosestMagnet(Collider[] magnetic, int hits)
    {
        bool first = true;
        if (hits > 0)
        {

            Vector3[] list;
            float shortDistance = -1;
            for (int i = 0; i < hits; i++)
            {
                if (magnetic[i] != null)
                {
                    if (first)
                    {

                        cubeAttractedToTransform = magnetic[i].transform;
                        playerMainCube = cubeAttractedToTransform.root.GetComponent<PlayerObjects>().player.transform;
                        playerAtractedTo = cubeAttractedToTransform.root;

                        list = playerAtractedTo.GetComponent<GridSystem>().ClosestNeighbourPosition(cubeAttractedToTransform.gameObject, transform.position);
                        closestFaceRelativeToWorld = list[0];
                        closestFaceRelativeToMainCube = list[1];
                        shortDistance = (transform.position - closestFaceRelativeToWorld).sqrMagnitude;
                        first = false;
                        
                    }
                    else
                    {
                        Vector3[] list2 = magnetic[i].transform.root.GetComponent<GridSystem>().ClosestNeighbourPosition(magnetic[i].gameObject, transform.position);
                        Vector3 closestFaceOther = list2[0];
                        Vector3 closestFaceOtherMain = list2[1];
                        float distance = (closestFaceOther - transform.position).sqrMagnitude;
                        if (distance < shortDistance)
                        {
                            cubeAttractedToTransform = magnetic[i].transform;
                            playerAtractedTo = cubeAttractedToTransform.root;
                            closestFaceRelativeToWorld = closestFaceOther;
                            shortDistance = distance;
                            closestFaceRelativeToMainCube = closestFaceOtherMain;

                        }
                    }
                }
            }
            if (!first)
            {
                cubeAtractedToPlayerObjects = cubeAttractedToTransform.root.GetComponent<PlayerObjects>();
                playerAttractedToGrid = cubeAttractedToTransform.root.GetComponent<GridSystem>();
            }
        }
    }

    private void TransformLerping()
    {
        if (lerping && timer < moveTime)
        {

            t = timer / moveTime;

            transform.localPosition = Vector3.Lerp(startPositionRelativeToAttractedCube, endPositionRelativeToAttractedCube, t);
            transform.localRotation = Quaternion.Slerp(startRotationRelativeToAttractedCube, endRotationRelativeToAttractedCube, t);
            timer += Time.fixedDeltaTime;
            if (cubeRB.velocity.magnitude > 1)
            {
                cubeRB.velocity = cubeRB.velocity.normalized * 1;
            }
        }
        else if (timer >= moveTime)
        {
            transform.localPosition = endPositionRelativeToAttractedCube;
            transform.localRotation = endRotationRelativeToAttractedCube;
            AttachCube();
        }
    }

    private void VelocityLerping()
    {
        float distance = (closestFaceRelativeToWorld - transform.position).magnitude;
        if (lerping && (errorP > error || errorR > AngleError) && (distance < lerpingDistance && timer < timeBeforeSwitching))
        {
            //Once its locked 
            timer += Time.fixedDeltaTime;
            t = Mathf.Clamp01(timer / moveTime);
            float rotationSpeed = moveTime / Time.fixedDeltaTime;
            Vector3 absoluteStartP = cubeAttractedToTransform.TransformPoint(startPositionRelativeToAttractedCube);
            Vector3 absoluteEndPosition = cubeAttractedToTransform.TransformPoint(endPositionRelativeToAttractedCube);
            Vector3 newPosition = Vector3.Lerp(absoluteStartP, absoluteEndPosition, t);
            Vector3 velocity = (newPosition - cubeRB.position) / Time.fixedDeltaTime;

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
            if (angularVelocity.magnitude > maxSpeed * 10)
            {
                angularVelocity = angularVelocity.normalized * maxSpeed * 10;
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
            Vector3 absoluteEndPosition = cubeAttractedToTransform.TransformPoint(endPositionRelativeToAttractedCube);
            Quaternion absoluteEndRotation = cubeAttractedToTransform.rotation * endRotationRelativeToAttractedCube;
            cubeRB.velocity = Vector3.zero;
            transform.position = absoluteEndPosition;
            transform.rotation = absoluteEndRotation;
            if (cubeAttractedToTransform.Find("Orientation") != null)
            {
                transform.Find("Orientation").transform.rotation = cubeAttractedToTransform.Find("Orientation").rotation;

            }
            else
            {
                transform.Find("Orientation").transform.rotation = cubeAttractedToTransform.rotation;
            }
            AttachCube();
        }
        else if(timer > timeBeforeSwitching)
        {
            cubeRB.AddForce(Vector3.up * 300, ForceMode.VelocityChange);
            timer = 0;
        }else
        {
            ResetObject();
        }
    }
  
    public void ResetObject()
    {
        lerping = false;
        timer = 0;
        playerAttractedToGrid = null;
        if (cubeRB != null && playerAtractedTo != null)
        {

            cubeAtractedToPlayerObjects.resetRb(gameObject);

        }
        cubeAtractedToPlayerObjects = null;
        cubeAttractedToTransform = null;
        playerAtractedTo = null;
        t = 0;
        relativePositionToMainCube = Vector3.zero;
        closestFaceRelativeToWorld = Vector3.zero;
        closestFaceRelativeToMainCube = Vector3.zero;
        errorP = 1;
        errorR = 1;

        transform.parent = this.transform.root.parent;
    }

    private void AttachCube()
    {
        //Attach magnetic field
        
        bool attach = playerAttractedToGrid.AttachBlock(gameObject, cubeAttractedToTransform.gameObject, closestFaceRelativeToMainCube);

        if (attach)
        {
            gameObject.AddComponent<SphereCollider>();
            gameObject.GetComponent<SphereCollider>().transform.position = transform.position;
            gameObject.GetComponent<SphereCollider>().radius = activeRadius;
            gameObject.GetComponent<SphereCollider>().isTrigger = true;
            GetComponent<Bloc>().state = BlocState.structure;
            if ((springType == SpringType.Free || springType == SpringType.Limited) && cubeAttractedToTransform.root.GetComponent<ConnectMagneticStructure>() == null)
            {
                //Set cube rigidbody parameters when in structure
                cubeRB.mass = structureMass;
                cubeRB.interpolation = RigidbodyInterpolation.Interpolate;
                cubeRB.drag = structureDrag;
                cubeRB.angularDrag = structureAngularDrag;
                this.transform.parent = cubeAtractedToPlayerObjects.cubeRb.transform;
                attachJ();
            }
            else
            {
                //It's children of mainCube
                this.transform.parent = cubeAtractedToPlayerObjects.cubeRb.transform;
                if (cubeRB != null)
                {
                    DestroyImmediate(cubeRB);
                }

            }
            //Set to magnetic after some time

            Invoke("setLayer", timeBeforeActiveMagnet);

            //Disable script
            this.GetComponent<Feromagnetic>().enabled = false;
        }
        else
        {
            ResetObject();
        }
    
    }

    private void setLayer()
    {
       
        gameObject.layer = cubeAttractedToTransform.gameObject.layer;
    }

    private void attachJ()
    {

        List<Vector3> occupiedSpaces = playerAttractedToGrid.getOccupiedNeighbours(gameObject);

        int i = 0;
        foreach (Vector3 cubeAttachToPosition in occupiedSpaces)
        {
            GameObject toConnectTo = playerAttractedToGrid.getObjectAtPosition(cubeAttachToPosition);
            this.AddComponent<ConfigurableJoint>();
            List<ConfigurableJoint> joints = this.GetComponents<ConfigurableJoint>().ToList();
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

                joint.axis = -transform.InverseTransformDirection(transform.Find("Orientation").forward);
                joint.secondaryAxis = Vector3.up;
                Vector3 positionBeforeCorrection = toConnectTo.transform.InverseTransformPoint(transform.position);
                if (toConnectTo.transform.Find("Orientation") != null)
                {
                    joint.connectedAnchor = toConnectTo.transform.InverseTransformPoint(toConnectTo.transform.Find("Orientation")
                        .TransformPoint(playerAttractedToGrid.getPositionOfObject(this.gameObject) - cubeAttachToPosition));

                }
                else
                {
                    joint.connectedAnchor = playerAttractedToGrid.getPositionOfObject(this.gameObject) - cubeAttachToPosition;
                }
                // joint.connectedAnchor = positionBeforeCorrection;
                if (springType == SpringType.Free)
                {
                    joint.angularYMotion = ConfigurableJointMotion.Free;
                    joint.angularXMotion = ConfigurableJointMotion.Free;
                    joint.angularZMotion = ConfigurableJointMotion.Free;
                    joint.xMotion = ConfigurableJointMotion.Free;
                    joint.yMotion = ConfigurableJointMotion.Free;
                    joint.zMotion = ConfigurableJointMotion.Free;

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
            i++;
        }
    }

    public Vector3 Correct(Vector3 positionBeforeCorrection)
    {
        float xPos = positionBeforeCorrection.x;
        float yPos = positionBeforeCorrection.y;
        float zPos = positionBeforeCorrection.z;
        Vector3 correction = Vector3.zero;
        if (Mathf.Abs(xPos) > Mathf.Abs(yPos) && Mathf.Abs(xPos) > Mathf.Abs(zPos))
        {
            correction.x = 1.2f * Mathf.Sign(xPos);
            correction.y = 0;
            correction.z = 0;
        }
        else if (Mathf.Abs(yPos) > Mathf.Abs(xPos) && Mathf.Abs(yPos) > Mathf.Abs(zPos))
        {
            correction.y = 1.2f * Mathf.Sign(yPos);
            correction.x = 0;
            correction.z = 0;
        }
        else
        {
            correction.z = 1.2f * Mathf.Sign(zPos);
            correction.y = 0;
            correction.x = 0;
        }

        return correction;
    }



    private void lerpingMagents(Vector3 direction, Vector3 relativeDirection, Vector3 closestFaceRelativeToWorld)
    {
        // If the distance is bigger than lerpingDistance or the position is not available anymore we keep pushing with CoulombLaw

        if ((transform.position - closestFaceRelativeToWorld).magnitude > lerpingDistance || !LookPositionGridAvailable())
        {
            
            cubeRB.AddForce(CoulombLaw(direction, charge, charge));
        }
        // We start attaching the cube 
        else if (!lerping)
        {

            //Set speed to zero and change layer to magnetic.
            cubeRB.velocity = Vector3.zero;
            cubeRB.angularVelocity = Vector3.zero;

            playerAtractedTo = cubeAttractedToTransform.root;

            //Start moving towards final position
            lerping = true;
            startPositionRelativeToAttractedCube = cubeAttractedToTransform.InverseTransformPoint(transform.position);
            startRotationRelativeToAttractedCube = Quaternion.Inverse(cubeAttractedToTransform.rotation) * transform.rotation;
            endPositionRelativeToAttractedCube = cubeAttractedToTransform.InverseTransformPoint(closestFaceRelativeToWorld);
            endRotationRelativeToAttractedCube = RotationChoice(startRotationRelativeToAttractedCube);
            cubeRB.useGravity = false;
            cubeRB.mass = lerpingMass;
        }
    }
    private Vector3 CoulombLaw(Vector3 distance, float charge1, float charge2)
    {

        float normSquaredInverse = 1.0f / Mathf.Pow(distance.magnitude, 2);
        if (normSquaredInverse < maxForceAttraction)
        {
            return charge1 * charge2 * normSquaredInverse * distance.normalized;
        }
        else
        {
            return charge1 * charge2 * maxForceAttraction * distance.normalized;
        }
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

    public bool LookPositionGridAvailable()
    {

        bool avaialble = true;
        if (playerAtractedTo != null)
        {
            Transform mainBodyCube = cubeAtractedToPlayerObjects.cubeRb.transform;

            Vector3 positionFromMainBody;
            if (mainBodyCube.Find("Orientation") != null)
            {
                positionFromMainBody = mainBodyCube.Find("Orientation").InverseTransformPoint(closestFaceRelativeToWorld);
            }
            else
            {
                positionFromMainBody = mainBodyCube.InverseTransformPoint(closestFaceRelativeToWorld);

            }
            avaialble = playerAttractedToGrid.positionAvailable(positionFromMainBody);
        }
        return avaialble;
    }
}
