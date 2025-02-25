using Cinemachine.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class Feromagnetic : MonoBehaviour
{
    private const double toleranceDirection = 0.1;
    private const float timeBeforeActiveMagnet = 0f;

    // Fero
    [SerializeField] float passiveRadius = 5.0f;
    [SerializeField] float activeRadius = 5f;
    [SerializeField] float charge = 5.0f;
    [SerializeField] float distanceLerp = 0.5f;
    [SerializeField] float error = 0.05f;
    [SerializeField] bool interpolates;
    [SerializeField] float deltaLerp = 0.2f;
    [SerializeField] float spacingBetweenCubes = 0.1f;
    [SerializeField] float moveTime = 0.1f;

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
    float time = 0;
    float errorP = 1;
    float errorR = 1;
    float timer = 0;
    float t = 0;
    float autoMagError;

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
            if (lerping && (errorP> error || errorR> error) && t<1)
            {
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
            else if (errorP<= error && errorR<= error || t>=1)
            {
             //Set location and velocity
             rb.velocity = Vector3.zero;
             transform.localPosition = endPosition;
             transform.localRotation = Quaternion.identity;

            //Set parent
             this.transform.parent = cubeAttractedToTransform.parent;
             rb.interpolation = RigidbodyInterpolation.Interpolate;
             rb.isKinematic = false;
             AttachCubeC();
            }
        }
        else
        {
            centerOfMassPosition = transform.position;
            Collider[] magnetic = Physics.OverlapSphere(centerOfMassPosition, passiveRadius, mask);
            rb = this.GetComponent<Rigidbody>();

            //Check for closest magnetic cube
            CheckClosestMagnet(magnetic);

            //If there is a magnet execute attraction algo
            if (magnetic.Length > 0)
            {
                cubeAttractedToTransform = cubeAttractedTo.transform;
                Vector3 direction = cubeAttractedToTransform.position - centerOfMassPosition;
                relativePosition = cubeAttractedTo.transform.InverseTransformPoint(transform.position);

                if (interpolates)
                {
                    rb.interpolation = RigidbodyInterpolation.Interpolate;
                }

               lerpingMagents(direction, relativePosition, closestFace);
            }
        }

        void CheckClosestMagnet(Collider[] magnetic)
        {
            if (magnetic.Length > 0)
            {
                cubeAttractedTo = magnetic[0];
                float shortDistance = (cubeAttractedTo.transform.position - centerOfMassPosition).magnitude;
                foreach (Collider col in magnetic)
                {
                    float distance = (col.transform.position - centerOfMassPosition).magnitude;
                    if (distance < shortDistance)
                    {
                        shortDistance = distance;
                        cubeAttractedTo = col;
                    }
                }
            }
        }

        void allignBlock(Vector3 closestFace)
        {

            transform.localPosition = closestFace;
            transform.rotation = cubeAttractedToTransform.rotation;

            if (cubeAttractedToTransform.tag != "Player")
            {
                this.transform.parent = cubeAttractedToTransform.parent;
            }
        }

        void autoMagnets(Vector3 direction,Vector3 relativeDirection, Vector3 closestFace)
        {
    
            if ((relativeDirection-closestFace).magnitude <= autoMagError)
            {

                //Set speed to zero and change layer to magnetic.
                //We also set the object rigidbody to kinematic mode.

                rb.angularVelocity = Vector3.zero;
                
                this.transform.parent = cubeAttractedToTransform;

                //Put the block in the correct position
                closestFace = CalculateClosestFace(relativePosition, cubeAttractedTo.gameObject);
                endPosition = closestFace;
                cubeAttractedTo.GetComponent<Faces>().faces.Remove(closestFace);
                allignBlock(closestFace);
                AttachCubeC();

            }
            else
            {
                rb.AddForce(CoulombLaw(direction, charge, charge));
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
        rb.solverIterations = 100;
        rb.solverVelocityIterations = 100;
        //rb.constraints = RigidbodyConstraints.None;
        rb.constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

        // Attach joint
        rb.mass = 1;
        attachJointC();

        this.GetComponent<Cube>().setOwner(this.transform.parent.gameObject.name);
        transform.parent.GetComponent<PlayerObjects>().cubes.Add(gameObject);
        transform.parent.GetComponent<PlayerObjects>().cubesHash.Add(transform.parent.GetComponent<PlayerObjects>()
            .player.transform.InverseTransformPoint(this.transform.position),gameObject);
        Invoke("setLayer", timeBeforeActiveMagnet);
        this.GetComponent<Feromagnetic>().enabled = false;
    }


    private void setLayer()
    {
        gameObject.layer = 3;
    }

  
    private void attachJointC()
    {
        Vector3[] Directions = { new Vector3(1, 0, 0), new Vector3(-1, 0, 0), new Vector3(0, 0, 1), new Vector3(0, 0, -1) };
        int i = 0;
        foreach (Vector3 direction in Directions)
        {
            Debug.DrawRay(cubeAttractedTo.transform.TransformPoint(endPosition), cubeAttractedTo.transform.TransformDirection(direction) * 0.5f, Color.red, 2f);
            int layerMask = LayerMask.GetMask("magnetic");
            RaycastHit[] hits = Physics.RaycastAll(cubeAttractedTo.transform.TransformPoint(endPosition), cubeAttractedTo.transform.TransformDirection(direction), 0.5f, layerMask, QueryTriggerInteraction.Ignore);
            foreach (RaycastHit hit in hits)
            {
                //More interpolation
                rb.interpolation = RigidbodyInterpolation.Interpolate;
                rb.solverIterations = 100;
                rb.solverVelocityIterations = 100;

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
                limitAy.limit = 2f;
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
                limitX.limit = 0f;
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
                if(Mathf.Abs(xPos)> Mathf.Abs(yPos) && Mathf.Abs(xPos) > Mathf.Abs(zPos))
                {
                     correction.x= 1.2f*Mathf.Sign(xPos);
                    correction.y = 0;
                    correction.z = 0;
                }
                else if (Mathf.Abs(yPos) > Mathf.Abs(xPos) && Mathf.Abs(yPos) > Mathf.Abs(zPos))
                {
                    correction.y = 1.2f * Mathf.Sign(yPos);
                    correction.x = 0;
                    correction.z= 0;
                }
                else
                {
                    correction.z=1.2f*Mathf.Sign(zPos);
                    correction.y = 0;
                    correction.x= 0;
                }
                joint.connectedAnchor = correction;
                //hit.transform.GetComponent<Faces>().removeClosestFace(correction);
               // this.GetComponent<Faces>().removeClosestFace(-correction);
            
                i++;
            }
           
        }        
    }


    private void lerpingMagents(Vector3 direction, Vector3 relativeDirection, Vector3 closestFace)
    {
        if (direction.magnitude > distanceLerp)
        {
            rb.AddForce(CoulombLaw(direction, charge, charge));
        }
        else if (!lerping)
        {

            //Set speed to zero and change layer to magnetic.
            //We also set the object rigidbody to kinematic mode.
            closestFace = CalculateClosestFace(relativePosition, cubeAttractedTo.gameObject);
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

            Vector3[] Directions = { new Vector3(1, 0, 0), new Vector3(-1, 0, 0), new Vector3(0, 0, 1), new Vector3(0, 0, -1) };
            int i = 0;
            //Predict position and rotation to find face to remove
            Matrix4x4 projectionM = Matrix4x4.TRS(cubeAttractedTo.transform.TransformPoint(endPosition),
            cubeAttractedTo.transform.rotation, transform.lossyScale);
            Vector4 cubeAtractedPosition = new Vector4(cubeAttractedTo.transform.position.x, cubeAttractedTo.transform.position.y, cubeAttractedTo.transform.position.z, 1);
            Vector4 faceToRemove4 = projectionM.inverse * cubeAtractedPosition;
            Vector3 faceToRemove = new Vector3(faceToRemove4.x, faceToRemove4.y, faceToRemove4.z);

            foreach (Vector3 dir in Directions)
            {
                Debug.DrawRay(cubeAttractedTo.transform.TransformPoint(endPosition), cubeAttractedTo.transform.TransformDirection(dir) * 0.5f, Color.red, 2f);
                int layerMask = LayerMask.GetMask("magnetic");
                RaycastHit[] hits = Physics.RaycastAll(cubeAttractedTo.transform.TransformPoint(endPosition), cubeAttractedTo.transform.TransformDirection(dir), 0.5f, layerMask, QueryTriggerInteraction.Ignore);

                foreach (RaycastHit hit in hits)
                {
                    hit.transform.GetComponent<Faces>().removeClosestFace(hit.transform.InverseTransformPoint(worldEndPosition));
                    Vector3 closest = hit.transform.position;
                    Vector4 closestHomo = new Vector4(closest.x, closest.y, closest.z, 1);
                    closestHomo = projectionM.inverse * closestHomo;
                    closest = new Vector3(closestHomo.x, closestHomo.y, closestHomo.z);
                    this.transform.GetComponent<Faces>().removeClosestFace(closest) ;
                }
            }
                    //enlever les faces
                   // cubeAttractedTo.GetComponent<Faces>().faces.Remove(closestFace);

 

            //Remove face
            //this.GetComponent<Faces>().removeClosestFace(faceToRemove);

        }

     
    }

    private static Vector3 CoulombLaw(Vector3 distance, float charge1, float charge2)
    {

        float normSqauredInverse = 1.0f / Mathf.Pow(distance.magnitude, 2);
        if (normSqauredInverse<30)
        {
            return charge1 * charge2 * normSqauredInverse * distance.normalized;
        }
        else
        {
            return charge1 * charge2 * 30 * distance.normalized;
        }
        

    }
    private Vector3 CalculateClosestFace(Vector3 relativeDirection,GameObject cubeAtractedTo)
    {
        List<Vector3> faces = cubeAtractedTo.GetComponent<Faces>().faces;
        if(faces.Count != 0)
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
    Vector3 ConvertPointIgnoringScale(Vector3 point, Transform fromLocal, Transform toLocal)
    {
        // Convert point from local to world space (ignoring scale)
        Vector3 worldPoint = fromLocal.position + fromLocal.rotation * point;
        

        // Convert from world space to new local space (ignoring scale)
        Vector3 newLocalPoint = Quaternion.Inverse(toLocal.rotation) * (worldPoint - toLocal.position);

        return newLocalPoint;
    }
    Vector3 ConvertPointIgnoringScale(Vector3 point, Transform toLocal)
    {
    
        // Convert from world space to new local space (ignoring scale)
        Vector3 newLocalPoint = Quaternion.Inverse(toLocal.rotation) * (point - toLocal.position);

        return newLocalPoint;
    }
    public List<Quaternion> createListAngles()
    {
        List<Quaternion> list = new List<Quaternion> ();
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
