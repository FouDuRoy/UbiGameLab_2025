using System.Collections;
using System.Collections.Generic;
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

    Vector3 centerOfMassPosition;
    Rigidbody rb;
    Collider cubeAttractedTo;
    Transform cubeAttractedToTransform;
    LayerMask mask;
    float cubeSize = 0.4f;
    bool lerping = false;
    Vector3 endPosition;
    Vector3 startPosition;
    Quaternion endRotation;
    Quaternion startRotation;
    List<Quaternion> quaternions;
    float time = 0;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        mask = LayerMask.GetMask("magnetic");
        cubeSize = transform.localScale.x;
        quaternions = createList();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        // Set the position of the block and look for all magnetic blocs in range
        centerOfMassPosition = transform.position;
        Collider[] magnetic = Physics.OverlapSphere(centerOfMassPosition, passiveRadius, mask);

        //Check for closest magnetic cube
        CheckClosestMagnet(magnetic);
        
        //Calculate mouvement of atracted bloc
        if (magnetic.Length > 0)
        {
            cubeAttractedToTransform = cubeAttractedTo.transform.parent;
            Vector3 direction = cubeAttractedToTransform.position - centerOfMassPosition;
            Vector3 relativeDirection = -cubeAttractedToTransform.InverseTransformDirection(direction);
            Vector3 closestFace = FaceVector(relativeDirection);
        

            if (interpolates && !lerping)
            {
                rb.interpolation = RigidbodyInterpolation.Interpolate;
            }
            
            //if not stable we keep applying force
           
            if (useLerp)
            {
                lerpingMagents(direction,relativeDirection, closestFace);
            }
            else
            {
                autoMagnets(direction, relativeDirection, closestFace);
            }
               

        }

        void CheckClosestMagnet(Collider[] magnetic)
        {
            if (magnetic.Length > 0)
            {
                cubeAttractedTo = magnetic[0];
                float shortDistance = (cubeAttractedTo.transform.parent.position - centerOfMassPosition).magnitude;
                foreach (Collider col in magnetic)
                {
                    float distance = (col.transform.parent.position - centerOfMassPosition).magnitude;
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
            
            transform.localPosition = ConvertPointIgnoringScale(closestFace, cubeAttractedToTransform, this.transform.parent);
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
                

                Debug.Log("Attached!!");

                this.transform.parent = cubeAttractedToTransform.parent;
              
                //Put the block in the correct position
                allignBlock(closestFace);

                rb.interpolation = RigidbodyInterpolation.None;
                GameObject magneticField = new GameObject();
                magneticField.name = "magField";
                magneticField.transform.localScale = Vector3.one;
                magneticField.transform.parent = this.transform;

                magneticField.AddComponent<SphereCollider>();
                magneticField.GetComponent<SphereCollider>().radius = activeRadius;
                magneticField.GetComponent<SphereCollider>().transform.position = this.transform.position;
                magneticField.GetComponent<SphereCollider>().isTrigger = true;
                magneticField.layer = 3;
                this.GetComponent<Feromagnetic>().enabled = false;


            }
            else
            {
                rb.AddForce(CoulombLaw(direction, charge, charge));
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
            
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;


            
            rb.interpolation = RigidbodyInterpolation.None;

            //Attach the object to the player
            this.transform.parent = cubeAttractedToTransform.parent;

            lerping = true;
            startPosition = transform.localPosition;
            startRotation = transform.localRotation;
            endPosition = ConvertPointIgnoringScale(closestFace, cubeAttractedToTransform, this.transform.parent);
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
            transform.localPosition = endPosition;
            transform.localRotation = endRotation;

            GameObject magneticField = new GameObject();
            magneticField.name = "magField";
            magneticField.transform.parent = this.transform;
            magneticField.transform.localScale = Vector3.one;
            magneticField.AddComponent<SphereCollider>();
            
            magneticField.GetComponent<SphereCollider>().radius = activeRadius;
            magneticField.GetComponent<SphereCollider>().transform.position = this.transform.position;
            magneticField.GetComponent<SphereCollider>().isTrigger = true;
            magneticField.layer = 3;
            this.GetComponent<Feromagnetic>().enabled = false;
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
    private Vector3 FaceVector(Vector3 relativeDirection)
    {
        Vector3[] list = { new Vector3(cubeSize,0,0), new Vector3(-cubeSize, 0, 0) , new Vector3(0,cubeSize, 0),
            new Vector3(0, -cubeSize, 0), new Vector3(0,0, cubeSize),new Vector3(0,0, -cubeSize) };
        Vector3 direction = list[0];

        foreach(Vector3 dir in list)
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
       // Quaternion[] list = { Quaternion.identity, Quaternion.Euler(90, 0, 0), Quaternion.Euler(180, 0, 0)
               // , Quaternion.Euler(270, 0, 0), Quaternion.Euler(360, 0, 0), Quaternion.Euler(0,90, 0),Quaternion.Euler(0,180, 0)
               // ,Quaternion.Euler(0,270, 0),Quaternion.Euler(0,360, 0) };

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
    public List<Quaternion> createList()
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
