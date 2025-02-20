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

    // Fero
    [SerializeField] float passiveRadius = 5.0f;
    [SerializeField] float activeRadius = 5f;
    [SerializeField] float charge = 5.0f;
    [SerializeField] float distanceLerp = 0.5f;
    [SerializeField] float autoMagError = 0.5f;
    [SerializeField] bool useLerp;
    [SerializeField] bool interpolates;
    [SerializeField] float deltaLerp = 0.2f;
    [SerializeField] float spacingBetweenCubes = 0.1f;

    //Joint settings
    [SerializeField] float mainLinearDrive = 1000f;
    [SerializeField] float mainLinearDamp = 50f;
    [SerializeField] float secondaryLinearDrive = 5000f;
    [SerializeField] float secondaryLinearDamp = 100f;
    [SerializeField] float angularDrive = 5000f;
    [SerializeField] float angularDamp = 100f;

    Vector3 centerOfMassPosition;
    Rigidbody rb;
    Collider cubeAttractedTo;
    Transform cubeAttractedToTransform;
    LayerMask mask;
    float cubeSize = 0.5f;
    bool lerping = false;
    Vector3 endPosition;
    Vector3 startPosition;
    Quaternion endRotation;
    Quaternion startRotation;
    List<Quaternion> quaternions;
    float time = 0;
    bool jointConstraint = true;
    bool jointConstraintAt = false;
    bool attached = false;

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
            if (lerping && time <= 1)
            {


                transform.localPosition = Vector3.Lerp(startPosition, endPosition, time);
                transform.localRotation = Quaternion.Slerp(startRotation, endRotation, time);
                // rb.MovePosition(Vector3.Lerp(rb.position, cubeAttractedToTransform.TransformPoint(endPosition), time)); 
                time += deltaLerp;



            }
            else if (time > 1)
            {
                transform.localPosition = endPosition;
                transform.localRotation = endRotation;
                if (cubeAttractedToTransform.tag != "Player")
                {
                    this.transform.parent = cubeAttractedToTransform.parent;
                }
                AttachCubeC();
            }
        }else if (jointConstraintAt)
        {
            if (cubeAttractedToTransform.tag != "Player")
            {
                this.transform.parent = cubeAttractedToTransform.parent;
            }
            AttachCubeC();
        }
        else
        {
            centerOfMassPosition = transform.position;
            Collider[] magnetic = Physics.OverlapSphere(centerOfMassPosition, passiveRadius, mask);
            rb = this.GetComponent<Rigidbody>();
            //Check for closest magnetic cube
            CheckClosestMagnet(magnetic);
            if (magnetic.Length > 0)
            {

                cubeAttractedToTransform = cubeAttractedTo.transform;
                Vector3 direction = cubeAttractedToTransform.position - centerOfMassPosition;
                Vector3 relativePosition = cubeAttractedTo.transform.InverseTransformPoint(transform.position);
                Vector3 closestFace = CalculateClosestFace(relativePosition, cubeAttractedTo.gameObject);

                if (interpolates)
                {
                    rb.interpolation = RigidbodyInterpolation.Interpolate;
                }

                //if not stable we keep applying force

                if (useLerp)
                {
                    lerpingMagents(direction, relativePosition, closestFace);
                }
                else
                {
                    autoMagnets(direction, relativePosition, closestFace);
                }
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

    private void AttachCube()
    {

        if (lerping)
        {
            lerping = false;
            time = 0;
            rb.isKinematic = false;
        }

        //Attach magnetic field
        this.gameObject.AddComponent<SphereCollider>();
        gameObject.GetComponent<SphereCollider>().transform.position = this.transform.position;
        gameObject.GetComponent<SphereCollider>().radius = activeRadius;
        gameObject.GetComponent<SphereCollider>().isTrigger = true;


        //Set rigidBody constraints
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.solverIterations = 40;
        rb.solverVelocityIterations = 20;
        rb.constraints = RigidbodyConstraints.None;

        // Attach joint
        attachJoint();

        this.GetComponent<Cube>().setOwner(this.transform.parent.gameObject.name);
        transform.parent.GetComponent<PlayerObjects>().cubes.Add(gameObject);
        Invoke("setLayer", 0.01f);
        cubeAttractedTo.gameObject.layer = 3;
        this.GetComponent<Feromagnetic>().enabled = false;
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
        rb.solverIterations = 40;
        rb.solverVelocityIterations = 20;
        rb.constraints = RigidbodyConstraints.None;

        // Attach joint
        attachJointC();

        this.GetComponent<Cube>().setOwner(this.transform.parent.gameObject.name);
        transform.parent.GetComponent<PlayerObjects>().cubes.Add(gameObject);
        Invoke("setLayer", 0.01f);
        cubeAttractedTo.gameObject.layer = 3;
        this.GetComponent<Feromagnetic>().enabled = false;
    }


    private void setLayer()
    {
        gameObject.layer = 3;
    }

    private void attachJoint()
    {
        Vector3[] Directions = { new Vector3(1, 0, 0), new Vector3(-1, 0, 0), new Vector3(0, 0, 1), new Vector3(0, 0, -1) };
        foreach(Vector3 direction in Directions)
        {

       
           
            Debug.DrawRay(transform.position, direction * 20f, Color.red, 2f);
            int layerMask = LayerMask.GetMask("magnetic");
            RaycastHit[] hits = Physics.RaycastAll(transform.position, direction, 20f, layerMask, QueryTriggerInteraction.Ignore);
            
            foreach(RaycastHit hit in hits)
            {
                Debug.Log(hit.collider.gameObject.name); 
            }
           
        }

        this.AddComponent<ConfigurableJoint>();
        ConfigurableJoint joint = this.GetComponent<ConfigurableJoint>();

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

        // Determine main axis and secondary axis
        Vector3 positionOfCubeAttractedToRelative = transform.InverseTransformPoint(cubeAttractedToTransform.position).normalized;
        Vector3 posRel = transform.InverseTransformPoint(cubeAttractedToTransform.position);
        joint.axis = positionOfCubeAttractedToRelative;
        joint.enableCollision = true;
        joint.connectedBody = cubeAttractedTo.GetComponent<Rigidbody>();
        //joint.anchor = positionOfCubeAttractedToRelative/2+positionOfCubeAttractedToRelative*spacingBetweenCubes;
        joint.anchor = posRel;
        //test(joint, positionOfCubeAttractedToRelative);
    }
    private void attachJointC()
    {
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.solverIterations = 40;
        rb.solverVelocityIterations = 20;
        rb.constraints = RigidbodyConstraints.None;

        this.AddComponent<ConfigurableJoint>();
        ConfigurableJoint joint = this.GetComponent<ConfigurableJoint>();

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

        // Determine main axis and secondary axis
        Vector3 positionOfCubeAttractedToRelative = transform.InverseTransformPoint(cubeAttractedToTransform.position).normalized;
        Vector3 posRel = transform.InverseTransformPoint(cubeAttractedToTransform.position);
        //joint.axis = positionOfCubeAttractedToRelative;
        joint.enableCollision = true;
        joint.connectedBody = cubeAttractedTo.GetComponent<Rigidbody>();
        //joint.anchor = positionOfCubeAttractedToRelative/2+positionOfCubeAttractedToRelative*spacingBetweenCubes;
        joint.anchor = Vector3.zero;
        joint.autoConfigureConnectedAnchor = false;
        joint.connectedAnchor = endPosition;
   
  
        //test(joint, positionOfCubeAttractedToRelative);
    }

    private void test(ConfigurableJoint joint, Vector3 positionOfCubeAttractedToRelative)
    {
        if (Mathf.Abs(Vector3.Dot(positionOfCubeAttractedToRelative, Vector3.up)) - 1 < toleranceDirection)
        {
            float sign = Mathf.Sign(Vector3.Dot(positionOfCubeAttractedToRelative, Vector3.up));
            joint.axis = Vector3.up;
            joint.secondaryAxis = Vector3.right;
            joint.anchor = transform.InverseTransformPoint(transform.parent.TransformPoint(transform.localPosition)) -
                Vector3.up * sign;
        }
        else if (Mathf.Abs(Vector3.Dot(positionOfCubeAttractedToRelative, Vector3.right)) - 1 < toleranceDirection)
        {
            float sign = Mathf.Sign(Vector3.Dot(positionOfCubeAttractedToRelative, Vector3.right));
            joint.axis = Vector3.right;
            joint.secondaryAxis = Vector3.up;
            joint.anchor = transform.InverseTransformPoint(transform.parent.TransformPoint(transform.localPosition)) -
              Vector3.right * sign;
        }
        else
        {
            float sign = Mathf.Sign(Vector3.Dot(positionOfCubeAttractedToRelative, Vector3.forward));
            joint.axis = Vector3.forward;
            joint.secondaryAxis = Vector3.up;
            joint.anchor = transform.InverseTransformPoint(transform.parent.TransformPoint(transform.localPosition)) -
              Vector3.forward * sign;
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
            
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

           // rb.interpolation = RigidbodyInterpolation.None;

            this.transform.parent = cubeAttractedToTransform;
     
            //Start moving towards final position
            lerping = true;
            jointConstraintAt = true;
            cubeAttractedTo.gameObject.layer = 0;
            startPosition = transform.localPosition;
            startRotation = transform.localRotation;
            endPosition = closestFace; 
            endRotation = RotationChoice(this.transform.localRotation);
            cubeAttractedTo.GetComponent<Faces>().faces.Remove(closestFace);
              
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
        Vector3 direction = faces[0];
        foreach(Vector3 dir in faces)
        {
            if ((direction - relativeDirection).magnitude > (dir - relativeDirection).magnitude)
            {
                direction = dir;
            }
        }
        return direction;
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
