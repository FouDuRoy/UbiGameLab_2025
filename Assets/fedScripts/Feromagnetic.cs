using Cinemachine.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem.HID;
using UnityEngine.ProBuilder.Shapes;
using UnityEngine.UIElements;

public class Feromagnetic : MonoBehaviour
{
    private const float timeBeforeActiveMagnet = 1f;


    // Fero
    [SerializeField] float xLimit = 0f;
    [SerializeField] float AngleLimit = 0f;
    [SerializeField] float passiveRadius = 5.0f;
    [SerializeField] float activeRadius = 5f;
    [SerializeField] float charge = 5.0f;
    [SerializeField] float error = 0.05f;
    [SerializeField] bool interpolates;
    [SerializeField] float deltaLerp = 0.2f;
    [SerializeField] float spacingBetweenCubes = 0.1f;
    [SerializeField] float moveTime = 0.1f;
    [SerializeField] bool freeSprings = false;
    //Joint settings
    [SerializeField] float mainLinearDrive = 1000f;
    [SerializeField] float mainLinearDamp = 50f;
    [SerializeField] float secondaryLinearDrive = 5000f;
    [SerializeField] float secondaryLinearDamp = 100f;
    [SerializeField] float angularDrive = 5000f;
    [SerializeField] float angularDamp = 100f;

    Rigidbody rb;
    Collider cubeAttractedTo;
    Transform cubeAttractedToTransform;
    LayerMask mask;
    bool lerping = false;

    Vector3 endPosition;
    Vector3 worldEndPosition;
    Vector3 startPosition;
    Vector3 closestFace;
    Vector3 relativePosition;
    Vector3 centerOfMassPosition;


    Quaternion endRotation;
    Quaternion startRotation;
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
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        // Set the position of the block and look for all magnetic blocs in range
        if (lerping)
        {
            if (lerping && (errorP > error || errorR > error) && t < 1)
            {
                //Once its locked 
                timer += Time.fixedDeltaTime;
                t = Mathf.Clamp01(timer / moveTime);

                Vector3 absoluteStartP = cubeAttractedTo.transform.TransformPoint(startPosition);
                Vector3 absoluteEndPosition = cubeAttractedTo.transform.TransformPoint(endPosition);
                Quaternion absoluteRoatationStart = cubeAttractedTo.transform.rotation * startRotation;
                Quaternion absoluteEndRotation = cubeAttractedTo.transform.rotation * endRotation;
                Vector3 newPosition = Vector3.Lerp(absoluteStartP, absoluteEndPosition, t);
                Quaternion newRotation = Quaternion.Slerp(absoluteRoatationStart, absoluteEndRotation, t);
                Vector3 velocity = (newPosition - rb.position) / Time.fixedDeltaTime;

                //update velocity and rotation
                rb.velocity = velocity;
                rb.MoveRotation(newRotation);

                errorP = Vector3.Distance(absoluteEndPosition, rb.position);
                errorR = (absoluteEndRotation.eulerAngles - rb.rotation.eulerAngles).magnitude;
            }
            else if (errorP <= error && errorR <= error || t >= 1)
            {
                //Set location and velocity
                rb.velocity = Vector3.zero;
                transform.localPosition = endPosition;
                transform.localRotation = Quaternion.identity;

                //Set parent
                this.transform.parent = cubeAttractedToTransform.parent;

                AttachCubeC();
            }
        }
        else
        {
            centerOfMassPosition = transform.position;
            List<Collider> magnetic = Physics.OverlapSphere(centerOfMassPosition, passiveRadius, mask).ToList<Collider>();
            List<Collider> magneticC = magnetic.ToList<Collider>();
            rb = this.GetComponent<Rigidbody>();

            foreach (Collider mag in magneticC)
            {
                if (mag.GetComponent<Faces>().faces.Count == 0)
                {
                    magnetic.Remove(mag);
                }
            }
            //Check for closest magnetic cube
            CheckClosestMagnet(magnetic);

            //If there is a magnet execute attraction algo
            if (magnetic.Count > 0)
            {
                cubeAttractedToTransform = cubeAttractedTo.transform;
                Vector3 direction = cubeAttractedToTransform.position - centerOfMassPosition;
                relativePosition = cubeAttractedTo.transform.InverseTransformPoint(transform.position);

                lerpingMagents(direction, relativePosition, closestFace);
            }
        }

        void CheckClosestMagnet(List<Collider> magnetic)
        {
            if (magnetic.Count > 0)
            {
                cubeAttractedTo = magnetic[0];
                closestFace = CalculateClosestFace(relativePosition, cubeAttractedTo.gameObject);

                float shortDistance = (cubeAttractedTo.transform.TransformPoint(CalculateClosestFace(relativePosition, cubeAttractedTo.gameObject)) - transform.position).magnitude;
                foreach (Collider col in magnetic)
                {
                    float distance = (col.transform.TransformPoint(CalculateClosestFace(relativePosition, col.gameObject)) - transform.position).magnitude;
                    if (distance < shortDistance)
                    {
                        closestFace = CalculateClosestFace(relativePosition, col.gameObject);
                        shortDistance = distance;
                        cubeAttractedTo = col;
                    }
                }

            }

        }
    }
    private void AttachCubeC()
    {
        //Attach magnetic field
        this.gameObject.AddComponent<SphereCollider>();
        gameObject.GetComponent<SphereCollider>().transform.position = this.transform.position;
        gameObject.GetComponent<SphereCollider>().radius = activeRadius;
        gameObject.GetComponent<SphereCollider>().isTrigger = true;


        //Set rigidBody constraints
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

        // Attach joint
        rb.mass = 1;
        cubeAttractedTo.gameObject.layer = 3;
        // attachJointC();
        attachJ();

        this.GetComponent<Cube>().setOwner(this.transform.parent.gameObject.name);
        transform.parent.GetComponent<PlayerObjects>().cubes.Add(gameObject);

        var myKey = transform.parent.GetComponent<PlayerObjects>().cubesHash.FirstOrDefault(x => x.Value == cubeAttractedTo.gameObject).Key;
        Vector3 positionP = endPosition + myKey;
        transform.parent.GetComponent<PlayerObjects>().cubesHash.Add(positionP, gameObject);
        Invoke("setLayer", timeBeforeActiveMagnet);
        this.GetComponent<Feromagnetic>().enabled = false;

    }


    private void setLayer()
    {
        RemoveFaces();
        gameObject.layer = 3;
    }


    private void attachJointC()
    {
        Vector3[] Directions = { new Vector3(1, 0, 0), new Vector3(-1, 0, 0), new Vector3(0, 0, 1), new Vector3(0, 0, -1) };
        int i = 0;
        foreach (Vector3 direction in Directions)
        {
            Debug.DrawRay(worldEndPosition, cubeAttractedTo.transform.TransformDirection(direction) * 0.5f, Color.red, 2f);
            int layerMask = LayerMask.GetMask("magnetic");
            RaycastHit[] hits = Physics.RaycastAll(cubeAttractedTo.transform.TransformPoint(endPosition), cubeAttractedTo.transform.TransformDirection(direction), 0.5f, layerMask, QueryTriggerInteraction.Ignore);
            foreach (RaycastHit hit in hits)
            {

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

                joint.projectionMode = JointProjectionMode.PositionAndRotation;
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
                    joint.xMotion = ConfigurableJointMotion.Limited;
                    joint.zMotion = ConfigurableJointMotion.Locked;
                }
                else if (yPosition > xPosition && yPosition > zPosition)
                {
                    joint.yMotion = ConfigurableJointMotion.Limited;
                    joint.xMotion = ConfigurableJointMotion.Locked;
                    joint.zMotion = ConfigurableJointMotion.Locked;
                }
                else if (zPosition > yPosition && zPosition > xPosition)
                {
                    joint.yMotion = ConfigurableJointMotion.Locked;
                    joint.xMotion = ConfigurableJointMotion.Locked;
                    joint.zMotion = ConfigurableJointMotion.Limited;
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
                joint.connectedBody = hit.transform.gameObject.GetComponent<Rigidbody>();
                joint.anchor = Vector3.zero;
                joint.autoConfigureConnectedAnchor = false;

                Vector3 positionBeforeCorrection = hit.collider.transform.InverseTransformPoint(cubeAttractedTo.transform.TransformPoint(endPosition));
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
                joint.connectedAnchor = correction;
                if (freeSprings)
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

    private void attachJ()
    {

        Transform mainCube = cubeAttractedTo.transform.parent.GetComponent<PlayerObjects>().cubeRb.transform;
        Dictionary<Vector3, GameObject> cubesHash = cubeAttractedTo.transform.parent.GetComponent<PlayerObjects>().cubesHash;
        Vector3[] Directions = { new Vector3(1.2f, 0, 0), new Vector3(-1.2f, 0, 0), new Vector3(0, 0, 1.2f), new Vector3(0, 0, -1.2f) };
        int i = 0;
        var myKey = cubesHash.FirstOrDefault(x => x.Value == cubeAttractedTo.gameObject).Key;
        foreach (Vector3 direction in Directions)
        {


            Vector3 positionCheck = myKey + endPosition + direction;
            foreach (var v in cubesHash)
            {
                if (v.Key == positionCheck)
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

                    joint.projectionMode = JointProjectionMode.PositionAndRotation;
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
                        joint.xMotion = ConfigurableJointMotion.Limited;
                        joint.zMotion = ConfigurableJointMotion.Locked;
                    }
                    else if (yPosition > xPosition && yPosition > zPosition)
                    {
                        joint.yMotion = ConfigurableJointMotion.Limited;
                        joint.xMotion = ConfigurableJointMotion.Locked;
                        joint.zMotion = ConfigurableJointMotion.Locked;
                    }
                    else if (zPosition > yPosition && zPosition > xPosition)
                    {
                        joint.yMotion = ConfigurableJointMotion.Locked;
                        joint.xMotion = ConfigurableJointMotion.Locked;
                        joint.zMotion = ConfigurableJointMotion.Limited;
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

                    Vector3 positionBeforeCorrection = toConnectTo.transform.InverseTransformPoint(cubeAttractedTo.transform.TransformPoint(endPosition));
                    Vector3 correction = Correct(positionBeforeCorrection);
                    joint.connectedAnchor = correction;
                    if (freeSprings)
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
    public Vector3 Correct2(Vector3 positionBeforeCorrection)
    {

        positionBeforeCorrection.x = Mathf.Round(positionBeforeCorrection.x / 1.2f) * 1.2f;
        positionBeforeCorrection.y = Mathf.Round(positionBeforeCorrection.y / 1.2f) * 1.2f;
        positionBeforeCorrection.z = Mathf.Round(positionBeforeCorrection.z / 1.2f) * 1.2f;


        return positionBeforeCorrection;
    }
    private void lerpingMagents(Vector3 direction, Vector3 relativeDirection, Vector3 closestFace)
    {
        if ((relativePosition - closestFace).magnitude > error)
        {
            rb.AddForce(CoulombLaw(direction, charge, charge));
        }
        else if (!lerping)
        {

            //Set speed to zero and change layer to magnetic.
            //We also set the object rigidbody to kinematic mode.
            //closestFace = CalculateClosestFace(relativePosition, cubeAttractedTo.gameObject);
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            //Set parent to attracted cube
            this.transform.parent = cubeAttractedToTransform;

            //Start moving towards final position
            lerping = true;
            startPosition = transform.localPosition;
            startRotation = transform.localRotation;
            endPosition = closestFace;
            endRotation = RotationChoice(this.transform.localRotation);
            worldEndPosition = cubeAttractedTo.transform.TransformPoint(endPosition);


            //Predict position and rotation to find face to remove
            RemoveFaces();
        }


    }

    private void RemoveFaces()
    {
        Matrix4x4 projectionM = Matrix4x4.TRS(cubeAttractedTo.transform.TransformPoint(endPosition),
                    cubeAttractedTo.transform.rotation, transform.lossyScale);
        Vector4 cubeAtractedPosition = new Vector4(cubeAttractedTo.transform.position.x, cubeAttractedTo.transform.position.y, cubeAttractedTo.transform.position.z, 1);
        Vector4 faceToRemove4 = projectionM.inverse * cubeAtractedPosition;
        Vector3 faceToRemove = new Vector3(faceToRemove4.x, faceToRemove4.y, faceToRemove4.z);

        Transform mainCube = cubeAttractedTo.transform.parent.GetComponent<PlayerObjects>().cubeRb.transform;
        Dictionary<Vector3, GameObject> cubesHash = cubeAttractedTo.transform.parent.GetComponent<PlayerObjects>().cubesHash;
        Vector3[] Directions = { new Vector3(1.2f, 0, 0), new Vector3(-1.2f, 0, 0), new Vector3(0, 0, 1.2f), new Vector3(0, 0, -1.2f) };
        var myKey = cubesHash.FirstOrDefault(x => x.Value == cubeAttractedTo.gameObject).Key;
        foreach (Vector3 dir in Directions)
        {

            foreach (var v in cubesHash)
            {

                Vector3 positionCheck = myKey + endPosition + dir;
                if (v.Key == positionCheck)
                {
                    v.Value.GetComponent<Faces>().removeClosestFace((v.Value.transform.InverseTransformPoint(worldEndPosition))); 
                    Vector3 closest = v.Value.transform.position;
                    Vector4 closestHomo = new Vector4(closest.x, closest.y, closest.z, 1);
                    closestHomo = projectionM.inverse * closestHomo;
                    closest = new Vector3(closestHomo.x, closestHomo.y, closestHomo.z);
                    this.transform.GetComponent<Faces>().removeClosestFace(closest);
                }
            }
        }
    }

    private static Vector3 CoulombLaw(Vector3 distance, float charge1, float charge2)
    {

        float normSqauredInverse = 1.0f / Mathf.Pow(distance.magnitude, 2);
        if (normSqauredInverse < 30)
        {
            return charge1 * charge2 * normSqauredInverse * distance.normalized;
        }
        else
        {
            return charge1 * charge2 * 30 * distance.normalized;
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
}
