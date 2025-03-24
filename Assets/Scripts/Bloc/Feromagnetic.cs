using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
//Federico Barallobres
public class Feromagnetic : MonoBehaviour
{
    private const float timeBeforeActiveMagnet = 0;
    // Settings Magnetisme

    [SerializeField] float passiveRadius = 5.0f;

    [SerializeField] float activeRadius = 5f;

    [SerializeField] float charge = 5.0f;
    [SerializeField] float maxSpeed = 10f;
    [SerializeField] float error = 0.05f;
    [SerializeField] float AngleError = 0.05f;

    [SerializeField] float spacingBetweenCubes = 0.1f;

    [SerializeField] float moveTime = 0.1f;

    [SerializeField] int maxForceAttraction = 30;

    [SerializeField] float lerpingDistance;

    [SerializeField] float timeBeforeSwitching = 0.5f;
    [SerializeField] float timeBeforeSwitchingVariation = 0.05f;
    [SerializeField] float maxSpeedVariation = 3f;
    [SerializeField] public float springTorqueBreak = 1000f;
    [SerializeField] public float springForceBreak = 1000f;

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

    Rigidbody cubeRB;

    Transform cubeAttractedToTransform;

    LayerMask mask;

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


    float cubeSize = 0.5f;

    float errorP = 1;

    float errorR = 1;

    float timer = 0;

    float t = 0;

    void Start()
    {
        mask = LayerMask.GetMask("magnetic");
        // We assume all cubes have same scale
        cubeSize = 1f + spacingBetweenCubes;
        quaternions = createListAngles();
        timeBeforeSwitching += Random.Range(-timeBeforeSwitchingVariation, timeBeforeSwitchingVariation);
        maxSpeed += Random.Range(-maxSpeedVariation, maxSpeedVariation);


    }

    void OnEnable()
    {
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
        if (cubeAttractedToTransform != null && cubeAttractedToTransform.root.GetComponent<PlayerObjects>() == null)
        {
            ResetObject();
        }
        // Set the position of the block and look for all magnetic blocs in range
        if (!lerping)
        {
            //Regarde les cubes magentiques dans le range du cube. 
            List<Collider> magnetic = Physics.OverlapSphere(transform.position, passiveRadius, mask).ToList<Collider>();
            List<Collider> magneticColliderList = magnetic.ToList<Collider>();
            cubeRB = this.GetComponent<Rigidbody>();

            // Enleve les cubes avec aucune face disponible. 
            foreach (Collider mag in magneticColliderList)
            {
                Debug.Log(mag.transform.root);
                GridSystem grid = mag.transform.root.GetComponent<GridSystem>();
                if (grid == null || grid.getAvailableNeighbours(mag.gameObject).Count == 0)
                {
                    magnetic.Remove(mag);
                }
            }

            //Check for closest magnetic cube
            CheckClosestMagnet(magnetic);

            //If there is a magnet in range execute attraction algo
            if (magnetic.Count > 0)
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

    void CheckClosestMagnet(List<Collider> magnetic)
    {
        if (magnetic.Count > 0)
        {

            cubeAttractedToTransform = magnetic[0].transform;
            playerMainCube = cubeAttractedToTransform.root.GetComponent<PlayerObjects>().player.transform;
            playerAtractedTo = cubeAttractedToTransform.root;

            Vector3[] list = playerAtractedTo.GetComponent<GridSystem>().ClosestNeighbourPosition(cubeAttractedToTransform.gameObject, transform.position);
            closestFaceRelativeToWorld = list[0];
            closestFaceRelativeToMainCube = list[1];
            float shortDistance = (transform.position - closestFaceRelativeToWorld).sqrMagnitude;
            foreach (Collider col in magnetic)
            {
                Vector3[] list2 = col.transform.root.GetComponent<GridSystem>().ClosestNeighbourPosition(col.gameObject, transform.position);
                Vector3 closestFaceOther = list2[0];
                Vector3 closestFaceOtherMain = list2[1];
                float distance = (closestFaceOther - transform.position).sqrMagnitude;
                if (distance < shortDistance)
                {
                    cubeAttractedToTransform = col.transform;
                    playerAtractedTo = cubeAttractedToTransform.root;
                    closestFaceRelativeToWorld = closestFaceOther;
                    shortDistance = distance;
                    playerMainCube = cubeAttractedToTransform.root.GetComponent<PlayerObjects>().player.transform;
                    closestFaceRelativeToMainCube = closestFaceOtherMain;



                }
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
        else
        {
            ResetObject();
        }
    }

    public void ResetObject()
    {
        lerping = false;
        timer = 0;
        cubeAttractedToTransform = null;
        playerAtractedTo = null;
        t = 0;
        relativePositionToMainCube = Vector3.zero;
        closestFaceRelativeToWorld = Vector3.zero;
        closestFaceRelativeToMainCube = Vector3.zero;
        errorP = 1;
        errorR = 1;
        if (cubeRB != null)
        {
            cubeRB.useGravity = true;

        }
        transform.parent = this.transform.root.parent;
    }

    private void AttachCube()
    {
        //Attach magnetic field

        gameObject.AddComponent<SphereCollider>();
        gameObject.GetComponent<SphereCollider>().transform.position = transform.position;
        gameObject.GetComponent<SphereCollider>().radius = activeRadius;
        gameObject.GetComponent<SphereCollider>().isTrigger = true;


        if (springType == SpringType.Free || springType == SpringType.Limited)
        {
            cubeRB.mass = 0.01f;
            cubeRB.interpolation = RigidbodyInterpolation.Interpolate;
            cubeRB.drag = 5f;
            cubeRB.angularDrag = 5f;
            this.transform.parent = cubeAttractedToTransform.root.GetComponent<PlayerObjects>().cubeRb.transform;
            playerAtractedTo.GetComponent<GridSystem>().AttachBlock(gameObject, cubeAttractedToTransform.gameObject, closestFaceRelativeToMainCube);
            attachJ();
        }
        else
        {
            //It's children of mainCube
            this.transform.parent = cubeAttractedToTransform.root.GetComponent<PlayerObjects>().cubeRb.transform;
            if (cubeRB != null)
            {
                DestroyImmediate(cubeRB);
            }
            playerAtractedTo.GetComponent<GridSystem>().AttachBlock(gameObject, cubeAttractedToTransform.gameObject, closestFaceRelativeToMainCube);

        }
        //Set to magnetic after some time
        this.GetComponent<Bloc>().state = BlocState.structure;
        Invoke("setLayer", timeBeforeActiveMagnet);

        //Disable script
        this.GetComponent<Feromagnetic>().enabled = false;
    }

    private void setLayer()
    {
        gameObject.layer = 3;
    }

    private void attachJ()
    {

        GridSystem cubeGrid = playerAtractedTo.GetComponent<GridSystem>();
        List<Vector3> occupiedSpaces = cubeGrid.getOccupiedNeighbours(gameObject);

        int i = 0;
        foreach (Vector3 cubeAttachToPosition in occupiedSpaces)
        {
            GameObject toConnectTo = cubeGrid.getObjectAtPosition(cubeAttachToPosition);
            this.AddComponent<ConfigurableJoint>();
            List<ConfigurableJoint> joints = this.GetComponents<ConfigurableJoint>().ToList();
            joints.RemoveAll(joint => joint.connectedBody != null);
            ConfigurableJoint joint = joints.First();

            if (joint.connectedBody == null)
            {
                JointDrive xDrive = joint.xDrive;
                xDrive.positionSpring = mainLinearDrive;
                xDrive.positionDamper = mainLinearDamp;
                joint.xDrive = xDrive;

                JointDrive yDrive = joint.yDrive;
                yDrive.positionSpring = secondaryLinearDrive;
                yDrive.positionDamper = secondaryLinearDamp;
                joint.yDrive = yDrive;

                JointDrive zDrive = joint.zDrive;
                zDrive.positionSpring = secondaryLinearDrive;
                zDrive.positionDamper = secondaryLinearDamp;
                joint.zDrive = zDrive;

                JointDrive angularXDrive = joint.angularXDrive;
                angularXDrive.positionSpring = mainLinearDrive;
                angularXDrive.positionDamper = mainLinearDamp;
                joint.angularXDrive = angularXDrive;

                JointDrive angularYZDrive = joint.angularYZDrive;
                angularYZDrive.positionSpring = mainLinearDrive;
                angularYZDrive.positionDamper = mainLinearDamp;
                joint.angularYZDrive = angularYZDrive;

                JointDrive slerpDrive = joint.slerpDrive;
                slerpDrive.positionSpring = angularDrive;
                slerpDrive.positionDamper = angularDamp;
                joint.slerpDrive = slerpDrive;
                joint.rotationDriveMode = RotationDriveMode.Slerp;

                joint.projectionAngle = 0;
                joint.projectionDistance = 0f;

                joint.angularYMotion = ConfigurableJointMotion.Limited;
                joint.angularXMotion = ConfigurableJointMotion.Locked;
                joint.angularZMotion = ConfigurableJointMotion.Locked;

                SoftJointLimit limitAy = new SoftJointLimit();
                limitAy.limit = AngleLimit;// angleLimit
                joint.angularYLimit = limitAy;

                joint.yMotion = ConfigurableJointMotion.Limited;
                joint.xMotion = ConfigurableJointMotion.Limited;
                joint.zMotion = ConfigurableJointMotion.Limited;


                SoftJointLimit limitX = new SoftJointLimit();
                limitX.limit = xLimit;
                joint.linearLimit = limitX;

                joint.connectedBody = toConnectTo.GetComponent<Rigidbody>();
                joint.anchor = Vector3.zero;
                joint.autoConfigureConnectedAnchor = false;

                Vector3 positionBeforeCorrection = toConnectTo.transform.InverseTransformPoint(transform.position);
                if (toConnectTo.transform.Find("Orientation") != null)
                {
                    joint.connectedAnchor = toConnectTo.transform.InverseTransformPoint(toConnectTo.transform.Find("Orientation")
                        .TransformPoint(cubeGrid.getPositionOfObject(this.gameObject) - cubeAttachToPosition));

                }
                else
                {
                    joint.connectedAnchor = cubeGrid.getPositionOfObject(this.gameObject) - cubeAttachToPosition;
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
            //Set parent to attracted cube
           // this.transform.parent = cubeAttractedToTransform;
            playerAtractedTo = cubeAttractedToTransform.root;

            //Start moving towards final positiond
            lerping = true;
            startPositionRelativeToAttractedCube = cubeAttractedToTransform.InverseTransformPoint(transform.position);
            startRotationRelativeToAttractedCube = Quaternion.Inverse(cubeAttractedToTransform.rotation) * transform.rotation;
            endPositionRelativeToAttractedCube = cubeAttractedToTransform.InverseTransformPoint(closestFaceRelativeToWorld);
            endRotationRelativeToAttractedCube = RotationChoice(startRotationRelativeToAttractedCube);
            cubeRB.useGravity = false;
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
}
