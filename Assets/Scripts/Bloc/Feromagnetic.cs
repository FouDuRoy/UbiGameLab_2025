using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class Feromagnetic : MonoBehaviour
{
        private const float timeBeforeActiveMagnet = 0.01f;

        private const float maxSpeed = 5f;

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

        bool lerping = false;

        Vector3 endPositionRelativeToAttractedCube;

        Vector3 startPositionRelativeToAttractedCube;

        Vector3 closestFace;

        Vector3 relativePositionToAttractedCube;

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
        public float getPassiveRadius()
    {
        return passiveRadius;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        // On regarde en premier si la position vers laquelle on se dirige est disponible

        if (LookPositionGridAvailable() && lerping)
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
                if (mag.GetComponent<Faces>().faces.Count == 0)
                {
                    magnetic.Remove(mag);
                }
            }

            //Check for closest magnetic cube
            CheckClosestMagnet(magnetic);

            //If there is a magnet in range execute attraction algo
            if (magnetic.Count > 0)
            {

                playerAtractedTo = cubeAttractedToTransform.root;
                Vector3 direction = cubeAttractedToTransform.position - transform.position;
                relativePositionToAttractedCube = cubeAttractedToTransform.InverseTransformPoint(transform.position);
                lerpingMagents(direction, relativePositionToAttractedCube, closestFace);
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
            closestFace = CalculateClosestFace(cubeAttractedToTransform.transform.InverseTransformPoint(transform.position), cubeAttractedToTransform.gameObject);
            float shortDistance = (cubeAttractedToTransform.position - transform.position).sqrMagnitude;
            foreach (Collider col in magnetic)
            {
                float distance = (col.transform.position - transform.position).sqrMagnitude;
                if (distance < shortDistance)
                {
                    cubeAttractedToTransform = col.transform;
                    closestFace = CalculateClosestFace(col.transform.InverseTransformPoint(transform.position), col.gameObject);
                    shortDistance = distance;
                    cubeAttractedToTransform = col.transform;
                }
            }

        }
    }

        private void TransformLerping()
    {
        if (lerping && t <= 1)
        {
            transform.localPosition = Vector3.Lerp(startPositionRelativeToAttractedCube, endPositionRelativeToAttractedCube, t);
            transform.localRotation = Quaternion.Slerp(startRotationRelativeToAttractedCube, endRotationRelativeToAttractedCube, t);
            t += Time.fixedDeltaTime / timeLerpingTransform;

        }
        else if (t > 1)
        {
            transform.localPosition = endPositionRelativeToAttractedCube;
            transform.localRotation = endRotationRelativeToAttractedCube;
            transform.parent = cubeAttractedToTransform.parent;
            AttachCube();
        }
    }

        private void VelocityLerping()
    {
        float distance = (cubeAttractedToTransform.position - transform.position).magnitude;
        //if (lerping && (errorP > error || errorR > error) && distance< maxDistanceBeforeStop)
        if (lerping && (errorP > error || errorR > error * 2) && (distance < lerpingDistance && timer < 0.25))
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
            Vector3 angularVelocity = axis * (angle * Mathf.Deg2Rad) * rotationSpeed;

            if (velocity.magnitude > maxSpeed)
            {
                velocity = velocity.normalized * maxSpeed;
            }
            if (angularVelocity.magnitude > 30)
            {
                //  angularVelocity = angularVelocity.normalized * maxSpeed;
            }
            //update velocity and rotation
            cubeRB.velocity = velocity;
            cubeRB.angularVelocity = angularVelocity;
            errorP = Vector3.Distance(absoluteEndPosition, cubeRB.position);
            errorR = (absoluteEndRotation.eulerAngles - cubeRB.rotation.eulerAngles).magnitude;
        }
        else if (!(errorP > error || errorR > error * 2))
        {
            //Set location and velocity
            cubeRB.velocity = Vector3.zero;
            transform.localPosition = endPositionRelativeToAttractedCube;
            transform.localRotation = Quaternion.identity;

            //Set parent
            this.transform.parent = cubeAttractedToTransform.parent;
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
    }

        private void AttachCube()
    {
        //Attach magnetic field
        gameObject.AddComponent<SphereCollider>();
        gameObject.GetComponent<SphereCollider>().transform.position = transform.position;
        gameObject.GetComponent<SphereCollider>().radius = activeRadius;
        gameObject.GetComponent<SphereCollider>().isTrigger = true;

        //Set rigidBody constraints
        cubeRB.mass = 1;
        cubeRB.interpolation = RigidbodyInterpolation.Interpolate;

        if (springType == SpringType.Free || springType == SpringType.Limited)
        {
            attachJ();
        }
        else
        {
            //It's children of mainCube
            this.transform.parent = cubeAttractedToTransform.root.GetComponent<PlayerObjects>().cubeRb.transform;
            DestroyImmediate(cubeRB);
        }

        this.GetComponent<Bloc>().setOwner(transform.root.gameObject.name);
        transform.root.GetComponent<PlayerObjects>().cubes.Add(gameObject);
        Transform mainCube = playerAtractedTo.GetComponent<PlayerObjects>().cubeRb.transform;
        Vector3 positionFromCenter = mainCube.InverseTransformPoint(cubeAttractedToTransform.TransformPoint(closestFace));

        transform.root.GetComponent<PlayerObjects>().cubeRb.gameObject.GetComponent<GridSystem>().AttachBlock(gameObject, cubeAttractedToTransform.gameObject, positionFromCenter);
        //We suppose same orientation of all cubes
        Invoke("setLayer", timeBeforeActiveMagnet);
        //reset
        this.GetComponent<Feromagnetic>().enabled = false;
    }

        private void setLayer()
    {
        lerping = false;
        this.cubeRB = null;
        this.endPositionRelativeToAttractedCube = Vector3.zero;
        this.cubeAttractedToTransform = null;
        this.timer = 0;
        this.t = 0;
        gameObject.layer = 3;
        storedFaces = new Dictionary<GameObject, List<Vector3>>();
    }

        private void attachJ()
    {

        Transform mainCube = cubeAttractedToTransform.parent.GetComponent<PlayerObjects>().cubeRb.transform;
        GridSystem cubeGrid = mainCube.gameObject.GetComponent<GridSystem>();
        int i = 0;
        var attractedCubePositionRelativeToMainBody = cubeGrid.getPositionOfObject(cubeAttractedToTransform.gameObject);
        foreach (Vector3 direction in directionsList)
        {
            //Supposing all same orientation
            Vector3 cubePositionRelativeToMainCube = attractedCubePositionRelativeToMainBody + endPositionRelativeToAttractedCube;
            Vector3 positionCheck = cubePositionRelativeToMainCube + direction;

            if (cubeGrid.containsKey(positionCheck))
            {

                GameObject toConnectTo = cubeGrid.getObjectAtPosition(positionCheck);
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
                Vector3 correction = Correct(positionBeforeCorrection);
                correction = cubePositionRelativeToMainCube - positionCheck;
                joint.connectedAnchor = correction;
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
                        Vector3 correction = Correct(positionBeforeCorrection);
                        correction = cubePositionRelativeToMainCube - v.Key;
                        joint.connectedAnchor = correction;
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

        private void lerpingMagents(Vector3 direction, Vector3 relativeDirection, Vector3 closestFace)
    {

        // If the distance is bigger than lerpingDistance or the position is not available anymore we keep pushing with CoulombLaw
        if ((relativePositionToAttractedCube).magnitude > lerpingDistance || LookPositionGridAvailable())
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
            this.transform.parent = cubeAttractedToTransform;
            playerAtractedTo = this.transform.root;

            //Start moving towards final position
            lerping = true;
            startPositionRelativeToAttractedCube = transform.localPosition;
            startRotationRelativeToAttractedCube = transform.localRotation;
            endPositionRelativeToAttractedCube = closestFace;
            endRotationRelativeToAttractedCube = RotationChoice(transform.localRotation);

            //Predict position and rotation to find face to remove
            RemoveFaces();
        }
    }

        private void RemoveFaces()
    {
        //Calculate position of face to remove in locale cordonates of the cube at end position
        if (playerAtractedTo != null)
        {

            Matrix4x4 projectionM = Matrix4x4.TRS(cubeAttractedToTransform.TransformPoint(endPositionRelativeToAttractedCube),
            cubeAttractedToTransform.rotation, cubeAttractedToTransform.lossyScale);
            Vector4 cubeAtractedToPositionHomo = MathExtension.toHomogeneousCords(cubeAttractedToTransform.position);
            Vector3 faceToRemove = MathExtension.toEuclidianVector(projectionM.inverse * cubeAtractedToPositionHomo);

            //Look if the face is there are other cubes adjacent to our cube
            Transform mainCube = transform.root.GetComponent<PlayerObjects>().cubeRb.transform;
            GridSystem cubeGrid = mainCube.GetComponent<GridSystem>();
            Vector3 attractedCubePositionRelativeToMainBody = cubeGrid.getPositionOfObject(cubeAttractedToTransform.gameObject);
            List<Vector3> myCubeFaceList = new List<Vector3>();

            foreach (Vector3 dir in directionsList)
            {

                // We assume all cubes have same orientation
                Vector3 cubeFinalPositionRelativeToMainBody = attractedCubePositionRelativeToMainBody + endPositionRelativeToAttractedCube;
                Vector3 positionCheked = cubeFinalPositionRelativeToMainBody + dir;

                //Check if there is adjecent cube
                if (cubeGrid.containsKey(positionCheked))
                {
                    cubeAttractedToTransform.TransformPoint(endPositionRelativeToAttractedCube);
                    //Remove adjecent cube face
                    Vector3 cubeAtFace;
                    Vector3 myCubeFace;
                    cubeAtFace = cubeGrid.getObjectAtPosition(positionCheked).GetComponent<Faces>().removeClosestFace((cubeGrid.getObjectAtPosition(positionCheked).transform.InverseTransformPoint(cubeAttractedToTransform.TransformPoint(endPositionRelativeToAttractedCube))));
                    myCubeFace = transform.GetComponent<Faces>().removeClosestFace(-(cubeGrid.getObjectAtPosition(positionCheked).transform.InverseTransformPoint(cubeAttractedToTransform.TransformPoint(endPositionRelativeToAttractedCube))));
                    List<Vector3> cubeAtFaceList = new List<Vector3>();
                    cubeAtFaceList.Add(cubeAtFace);
                    myCubeFaceList.Add(myCubeFace);
                    storedFaces.Add(cubeGrid.getObjectAtPosition(positionCheked), cubeAtFaceList);

                }
            }

            storedFaces.Add(gameObject, myCubeFaceList);
        }
    }

        private void RemoveFacesClean()
    {
        //Calculate position of face to remove in locale cordonates of the cube at end position
        Matrix4x4 projectionM = Matrix4x4.TRS(cubeAttractedToTransform.TransformPoint(endPositionRelativeToAttractedCube),
                    cubeAttractedToTransform.rotation, cubeAttractedToTransform.lossyScale);
        Vector4 cubeAtractedToPositionHomo = MathExtension.toHomogeneousCords(cubeAttractedToTransform.position);
        Vector3 faceToRemove = MathExtension.toEuclidianVector(projectionM.inverse * cubeAtractedToPositionHomo);

        //Look if the face is there are other cubes adjacent to our cube
        Transform mainCube = transform.root.GetComponent<PlayerObjects>().cubeRb.transform;
        GridSystem cubeGrid = mainCube.GetComponent<GridSystem>();
        Vector3 attractedCubePositionRelativeToMainBody = cubeGrid.getPositionOfObject(cubeAttractedToTransform.gameObject);

        foreach (Vector3 dir in directionsList)
        {
            // We assume all cubes have same orientation
            Vector3 cubeFinalPositionRelativeToMainBody = attractedCubePositionRelativeToMainBody + endPositionRelativeToAttractedCube;
            Vector3 positionCheked = cubeFinalPositionRelativeToMainBody + dir;
            //Check if there is adjecent cube

            if (cubeGrid.containsKey(positionCheked))
            {
                cubeAttractedToTransform.TransformPoint(endPositionRelativeToAttractedCube);
                //Remove adjecent cube face
                Vector3 cubeAtFace;
                Vector3 myCubeFace;
                cubeAtFace = cubeGrid.getObjectAtPosition(positionCheked).GetComponent<Faces>().removeClosestFace((cubeGrid.getObjectAtPosition(positionCheked).transform.InverseTransformPoint(cubeAttractedToTransform.TransformPoint(endPositionRelativeToAttractedCube))));
                myCubeFace = transform.GetComponent<Faces>().removeClosestFace(-(cubeGrid.getObjectAtPosition(positionCheked).transform.InverseTransformPoint(cubeAttractedToTransform.TransformPoint(endPositionRelativeToAttractedCube))));

            }

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

        private Vector3 CalculateClosestFace(Vector3 relativeDirection, GameObject cubeAtractedTo)
    {
        List<Vector3> faces = cubeAtractedTo.GetComponent<Faces>().faces;
        if (faces.Count != 0)
        {
            Vector3 direction = faces.First<Vector3>();

            foreach (Vector3 dir in faces)
            {
                if ((direction - relativeDirection).magnitude > (dir - relativeDirection).magnitude)
                {
                    direction = dir;
                }
            }
            return direction;
        }
        return Vector3.zero;
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

        bool inGrid = false;
        if (playerAtractedTo != null)
        {
            Transform mainBodyCube = playerAtractedTo.GetComponent<PlayerObjects>().cubeRb.transform;
            GridSystem grid = mainBodyCube.GetComponent<GridSystem>();

            Debug.Log("CF" + closestFace+"cA"+cubeAttractedToTransform);
            Vector3 positionFromMainBody = mainBodyCube.InverseTransformPoint(cubeAttractedToTransform.TransformPoint(closestFace));
            inGrid = grid.containsKey(positionFromMainBody);
            Debug.Log("Position:" + positionFromMainBody + "grid:" + inGrid);
        }
        return inGrid;
    }
}
