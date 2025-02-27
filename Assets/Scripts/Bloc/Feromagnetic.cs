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
    
    [SerializeField] float passiveRadius = 5.0f;
    [SerializeField] float activeRadius = 5f;
    [SerializeField] float charge = 5.0f;
    [SerializeField] float distanceLerp = 0.5f;
    [SerializeField] float autoMagError = 0.5f;
    [SerializeField] bool useLerp;
    [SerializeField] bool interpolates;
    [SerializeField] float deltaLerp = 0.2f;
    [SerializeField] float spacingBetweenCubes = 0.1f;

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
    List<Vector3> faces;
    float time = 0;

    void Start()
    {
       
        mask = LayerMask.GetMask("magnetic");

        // We assume all cubes have same scale
        cubeSize = 1f + spacingBetweenCubes;
        quaternions = createListAngles();
        Vector3[] face ={ new Vector3(cubeSize, 0, 0), new Vector3(-cubeSize, 0, 0) , new Vector3(0, cubeSize, 0),
            new Vector3(0, -cubeSize, 0), new Vector3(0, 0, cubeSize),new Vector3(0, 0, -cubeSize) };
        faces = face.ToList();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        // Set the position of the block and look for all magnetic blocs in range
        centerOfMassPosition = transform.position;
        Collider[] magnetic = Physics.OverlapSphere(centerOfMassPosition, passiveRadius, mask);
        rb = this.GetComponent<Rigidbody>();
        //Check for closest magnetic cube
        CheckClosestMagnet(magnetic);
        
        //Calculate mouvement of atracted bloc
        if (magnetic.Length > 0)
        {
            
            cubeAttractedToTransform = cubeAttractedTo.transform;
            Vector3 direction = cubeAttractedToTransform.position - centerOfMassPosition;
            Vector3 relativePosition = cubeAttractedTo.transform.InverseTransformPoint(transform.position);
            Vector3 closestFace = CalculateClosestFace(relativePosition);
            
            if (interpolates && !lerping)
            {
                rb.interpolation = RigidbodyInterpolation.Interpolate;
            }
            
            //if not stable we keep applying force
           
            if (useLerp)
            {
                lerpingMagents(direction,relativePosition, closestFace);
            }
            else
            {
                autoMagnets(direction, relativePosition, closestFace);
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

            transform.localPosition = this.transform.parent.InverseTransformPoint(cubeAttractedToTransform.TransformPoint(closestFace));
            transform.rotation = cubeAttractedToTransform.rotation;

        }

        void autoMagnets(Vector3 direction,Vector3 relativeDirection, Vector3 closestFace)
        {
    
            if ((relativeDirection-closestFace).magnitude <= autoMagError)
            {

                //Set speed to zero and change layer to magnetic.
                //We also set the object rigidbody to kinematic mode.


                rb.angularVelocity = Vector3.zero;
                rb.isKinematic = true;


                if (cubeAttractedToTransform.tag == "Player")
                {
                    this.transform.parent = cubeAttractedToTransform;
                }
                else
                {

                    this.transform.parent = cubeAttractedToTransform.parent;
                }

                //Put the block in the correct position
                allignBlock(closestFace);
                AttachCube();

            }
            else
            {
                rb.AddForce(CoulombLaw(direction, charge, charge));
            }

        }
    }

    private void AttachCube()
    {
        
        rb.interpolation = RigidbodyInterpolation.None;

        //Attach magnetic field
        if (lerping)
        {
            lerping = false;
            time = 0;
        }

        this.gameObject.AddComponent<SphereCollider>();
        gameObject.GetComponent<SphereCollider>().transform.position = this.transform.position;
        gameObject.GetComponent<SphereCollider>().radius = activeRadius;
        gameObject.GetComponent<SphereCollider>().isTrigger = true;
        gameObject.layer = 3;

        GameObject.Destroy(this.GetComponent<Rigidbody>());
        this.GetComponent<Bloc>().setOwner(this.transform.parent.gameObject.name);
        transform.parent.GetComponent<PlayerObjects>().cubes.Add(gameObject);
        this.GetComponent<Feromagnetic>().enabled = false;
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
            rb.isKinematic = true;


            
            rb.interpolation = RigidbodyInterpolation.None;

            //Attach the object to the player
            if(cubeAttractedToTransform.tag == "Player")
            {
                this.transform.parent = cubeAttractedToTransform;
            }
            else
            {
                
                this.transform.parent = cubeAttractedToTransform.parent;
            }
           
            lerping = true;
            startPosition = transform.localPosition;
            startRotation = transform.localRotation;
            endPosition = this.transform.parent.InverseTransformPoint(cubeAttractedToTransform.TransformPoint(closestFace));
            endRotation = RotationChoice(this.transform.localRotation);
        }

        if (lerping && time <= 1)
        {

            
            transform.localPosition = Vector3.Lerp(startPosition, endPosition, time);
            transform.localRotation = Quaternion.Slerp(startRotation, endRotation, time);
            time += deltaLerp;
          


        }
        else if (time > 1)
        {
           // transform.localPosition = endPosition;
           // transform.localRotation = endRotation;
            AttachCube();
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
    private Vector3 CalculateClosestFace(Vector3 relativeDirection)
    {
       
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
