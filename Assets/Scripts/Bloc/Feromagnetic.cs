using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class Feromagnetic : MonoBehaviour
{
        private const float timeBeforeActiveMagnet = 0;

        private const float maxSpeed = 3f;

        private const float maxDistanceBeforeStop = 1f;

    // Settings Magnetisme

    [SerializeField] float passiveRadius = 5.0f;

        [SerializeField] float activeRadius = 5f;

        [SerializeField] float charge = 5.0f;

        [SerializeField] float error = 0.05f;

        [SerializeField] bool interpolates;

        [SerializeField] float timeLerpingTransform = 0.2f;

        [SerializeField] float spacingBetweenCubes = 0.1f;

        [SerializeField] float moveTime = 0.1f;

        [SerializeField] int maxForceAttraction = 30;

        [SerializeField] float lerpingDistance;

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

        Dictionary<GameObject, List<Vector3>> storedFaces = new Dictionary<GameObject, List<Vector3>>();

        Quaternion endRotationRelativeToAttractedCube;

        Quaternion startRotationRelativeToAttractedCube;

        List<Quaternion> quaternions;

        Vector3[] directionsList;

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

        directionsList = new Vector3[] { new Vector3(cubeSize, 0, 0), new Vector3(-cubeSize, 0, 0), new Vector3(0, 0, cubeSize)
    ,   new Vector3(0, 0, -cubeSize),new Vector3(0,cubeSize,0),new Vector3(0,-cubeSize,0) };
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
                GridSystem grid =  mag.transform.root.GetComponent<GridSystem>();
                if (grid == null || grid.getAvailableNeighbours(mag.gameObject).Count==0)
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

            Vector3[] list =playerAtractedTo.GetComponent<GridSystem>().ClosestNeighbourPosition(cubeAttractedToTransform.gameObject, transform.position);
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
        if (lerping && (errorP > error || errorR > 5) && (distance < lerpingDistance && timer < 1))
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
            Vector3 angularVelocity = axis * (angle * Mathf.Deg2Rad) * rotationSpeed*2f;
            Debug.Log("relativeRoataion" + absoluteEndRotation.eulerAngles + "relative" + endRotationRelativeToAttractedCube.eulerAngles);
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
            errorR = Quaternion.Angle(absoluteEndRotation,cubeRB.rotation);
            Debug.Log("errorP" + errorP + "errorR" + errorR+"Cube"+transform.name);
        }
        else if (!(errorP > error || errorR > 5))
        {
            //Set location and velocity
            cubeRB.velocity = Vector3.zero;
            transform.localPosition = endPositionRelativeToAttractedCube;
            transform.rotation = cubeAttractedToTransform.rotation;
            AttachCube();
        }
        else
        {
            ResetObject();
        }
    }

        private void ResetObject()
    {
        lerping = false;
        timer = 0;
        cubeAttractedToTransform = null;
        playerAtractedTo = null;
        t = 0;
        relativePositionToMainCube = Vector3.zero;
        closestFaceRelativeToWorld = Vector3.zero;
        closestFaceRelativeToMainCube = Vector3.zero;
        foreach (var cube in storedFaces)
        {
            List<Vector3> faces = cube.Key.GetComponent<Faces>().faces;
            foreach (Vector3 face in cube.Value)
            {
                faces.Add(face);
            }
        }
        storedFaces = new Dictionary<GameObject, List<Vector3>>();
        transform.parent = this.transform.root.parent;
        Start();
    }

        private void AttachCube()
    {
        //Attach magnetic field
        gameObject.AddComponent<SphereCollider>();
        gameObject.GetComponent<SphereCollider>().transform.position = transform.position;
        gameObject.GetComponent<SphereCollider>().radius = activeRadius;
        gameObject.GetComponent<SphereCollider>().isTrigger = true;

        Debug.Log("attach"); 
        this.GetComponent<Bloc>().setOwner(transform.root.gameObject.name);
        transform.root.GetComponent<PlayerObjects>().cubes.Add(gameObject);
        if (springType == SpringType.Free || springType == SpringType.Limited)
        {
            cubeRB.mass = 1;
            cubeRB.interpolation = RigidbodyInterpolation.Interpolate;
            this.transform.parent = cubeAttractedToTransform.root.GetComponent<PlayerObjects>().cubeRb.transform;
            playerAtractedTo.GetComponent<GridSystem>().AttachBlock(gameObject, cubeAttractedToTransform.gameObject,closestFaceRelativeToMainCube);

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
                ConfigurableJoint[] joints = this.GetComponents<ConfigurableJoint>();
                ConfigurableJoint joint = joints[i];

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
                Vector3 positionCenter = playerMainCube.InverseTransformPoint(transform.position);
                float xPosition = Mathf.Abs(positionCenter.x);
                float yPosition = Mathf.Abs(positionCenter.y);
                float zPosition = Mathf.Abs(positionCenter.z);

                if (xPosition > yPosition && xPosition > zPosition)
                {
                    joint.yMotion = ConfigurableJointMotion.Locked;
                    joint.xMotion = ConfigurableJointMotion.Locked;
                    joint.zMotion = ConfigurableJointMotion.Locked;
                }
                else if (yPosition > xPosition && yPosition > zPosition)
                {
                    joint.yMotion = ConfigurableJointMotion.Locked;
                    joint.xMotion = ConfigurableJointMotion.Locked;
                    joint.zMotion = ConfigurableJointMotion.Locked;
                }
                else if (zPosition > yPosition && zPosition > xPosition)
                {
                    joint.yMotion = ConfigurableJointMotion.Locked;
                    joint.xMotion = ConfigurableJointMotion.Locked;
                    joint.zMotion = ConfigurableJointMotion.Locked;
                }
                else
                {
                    joint.yMotion = ConfigurableJointMotion.Locked;
                    joint.xMotion = ConfigurableJointMotion.Locked;
                    joint.zMotion = ConfigurableJointMotion.Locked;
                }

                SoftJointLimit limitX = new SoftJointLimit();
                limitX.limit = xLimit;
                joint.linearLimit = limitX;

                joint.enableCollision = true;
                joint.connectedBody = toConnectTo.GetComponent<Rigidbody>();
                joint.anchor = Vector3.zero;
                joint.autoConfigureConnectedAnchor = false;

                Vector3 positionBeforeCorrection = toConnectTo.transform.InverseTransformPoint(transform.position);
                //Vector3 correction = Correct(positionBeforeCorrection);
                //correction = cubePositionRelativeToMainCube - positionCheck;
                joint.connectedAnchor = positionBeforeCorrection;
                if (springType == SpringType.Free)
                {
                    joint.angularYMotion = ConfigurableJointMotion.Free;
                    joint.angularXMotion = ConfigurableJointMotion.Free;
                    joint.angularZMotion = ConfigurableJointMotion.Free;
                    joint.xMotion = ConfigurableJointMotion.Free;
                    joint.yMotion = ConfigurableJointMotion.Free;
                    joint.zMotion = ConfigurableJointMotion.Free;
                    joint.projectionMode = JointProjectionMode.None;

                }
                i++;

            

        }
    }

        private void attach1()
    {

        Transform mainCube = cubeAttractedToTransform.parent.GetComponent<PlayerObjects>().cubeRb.transform;
        Dictionary<Vector3, GameObject> cubeGrid = cubeAttractedToTransform.parent.GetComponent<PlayerObjects>().cubesHash;
        int i = 0;
        var attractedCubePositionRelativeToMainBody = cubeGrid.FirstOrDefault(x => x.Value == cubeAttractedToTransform).Key;
        foreach (Vector3 direction in directionsList)
        {
            //Supposing all same orientation
            Vector3 cubePositionRelativeToMainCube = attractedCubePositionRelativeToMainBody + endPositionRelativeToAttractedCube;
            Vector3 positionCheck = cubePositionRelativeToMainCube + direction;
            foreach (var v in cubeGrid)
            {
                if (v.Key == positionCheck)
                {
                    if (v.Value == cubeAttractedToTransform.gameObject)
                    {
                        GameObject toConnectTo = v.Value;
                        this.AddComponent<ConfigurableJoint>();
                        ConfigurableJoint[] joints = this.GetComponents<ConfigurableJoint>();
                        ConfigurableJoint joint = joints[i];

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

                        //joint.projectionMode = JointProjectionMode.PositionAndRotation;
                        joint.projectionAngle = 0;
                        joint.projectionDistance = 0f;

                        joint.angularYMotion = ConfigurableJointMotion.Limited;
                        joint.angularXMotion = ConfigurableJointMotion.Locked;
                        joint.angularZMotion = ConfigurableJointMotion.Locked;

                        SoftJointLimit limitAy = new SoftJointLimit();
                        limitAy.limit = AngleLimit;// angleLimit
                        joint.angularYLimit = limitAy;
                        Vector3 positionCenter = transform.parent.GetComponent<PlayerObjects>().player.transform.InverseTransformPoint(transform.position);
                        float xPosition = Mathf.Abs(positionCenter.x);
                        float yPosition = Mathf.Abs(positionCenter.y);
                        float zPosition = Mathf.Abs(positionCenter.z);

                        if (xPosition > yPosition && xPosition > zPosition)
                        {
                            joint.yMotion = ConfigurableJointMotion.Locked;
                            joint.xMotion = ConfigurableJointMotion.Locked;
                            joint.zMotion = ConfigurableJointMotion.Locked;
                        }
                        else if (yPosition > xPosition && yPosition > zPosition)
                        {
                            joint.yMotion = ConfigurableJointMotion.Locked;
                            joint.xMotion = ConfigurableJointMotion.Locked;
                            joint.zMotion = ConfigurableJointMotion.Locked;
                        }
                        else if (zPosition > yPosition && zPosition > xPosition)
                        {
                            joint.yMotion = ConfigurableJointMotion.Locked;
                            joint.xMotion = ConfigurableJointMotion.Locked;
                            joint.zMotion = ConfigurableJointMotion.Locked;
                        }
                        else
                        {
                            joint.yMotion = ConfigurableJointMotion.Locked;
                            joint.xMotion = ConfigurableJointMotion.Locked;
                            joint.zMotion = ConfigurableJointMotion.Locked;
                        }

                        SoftJointLimit limitX = new SoftJointLimit();
                        limitX.limit = xLimit;
                        joint.linearLimit = limitX;

                        joint.enableCollision = true;
                        joint.connectedBody = toConnectTo.GetComponent<Rigidbody>();
                        joint.anchor = Vector3.zero;
                        joint.autoConfigureConnectedAnchor = false;

                        Vector3 positionBeforeCorrection = toConnectTo.transform.InverseTransformPoint(cubeAttractedToTransform.TransformPoint(endPositionRelativeToAttractedCube));
                        //Vector3 correction = Correct(positionBeforeCorrection);
                        //correction = cubePositionRelativeToMainCube - v.Key;
                        joint.connectedAnchor = positionBeforeCorrection;
                        if (springType == SpringType.Free)
                        {
                            joint.angularYMotion = ConfigurableJointMotion.Free;
                            joint.angularXMotion = ConfigurableJointMotion.Free;
                            joint.angularZMotion = ConfigurableJointMotion.Free;
                            joint.xMotion = ConfigurableJointMotion.Free;
                            joint.yMotion = ConfigurableJointMotion.Free;
                            joint.zMotion = ConfigurableJointMotion.Free;
                            joint.projectionMode = JointProjectionMode.None;

                        }
                        i++;
                    }

                }

            }

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
        
        if ((transform.position-closestFaceRelativeToWorld).magnitude > lerpingDistance || !LookPositionGridAvailable())
        {
            Debug.DrawLine(cubeAttractedToTransform.position, closestFaceRelativeToWorld, Color.black, 10f);
            cubeRB.AddForce(CoulombLaw(direction, charge, charge));
        }
        // We start attaching the cube 
        else if (!lerping)
        {
            Debug.DrawLine(cubeAttractedToTransform.position, closestFaceRelativeToWorld, Color.black, 10f);
            //Set speed to zero and change layer to magnetic.
            cubeRB.velocity = Vector3.zero;
            cubeRB.angularVelocity = Vector3.zero;
            //Set parent to attracted cube
            this.transform.parent = cubeAttractedToTransform;
            playerAtractedTo = this.transform.root;

            //Start moving towards final positiond
            lerping = true;
            startPositionRelativeToAttractedCube = transform.localPosition;
            startRotationRelativeToAttractedCube = transform.localRotation;
            endPositionRelativeToAttractedCube = cubeAttractedToTransform.InverseTransformPoint(closestFaceRelativeToWorld);
            endRotationRelativeToAttractedCube = RotationChoice(transform.localRotation);
            Debug.Log("rotation" + endRotationRelativeToAttractedCube);
            //Predict position and rotation to find face to remove
            //RemoveFaces();
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

            Vector3 positionFromMainBody = mainBodyCube.InverseTransformPoint(closestFaceRelativeToWorld);
            avaialble = !grid.containsKey(positionFromMainBody);
        }
        return avaialble;
    }
}
