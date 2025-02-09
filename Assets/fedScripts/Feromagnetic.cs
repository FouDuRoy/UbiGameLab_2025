using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class Feromagnetic : MonoBehaviour
{
    Vector3 centerOfMassPosition;
    Rigidbody rb;

    [SerializeField] float passiveRadius = 5.0f;
    [SerializeField] float activeRadius = 5f;
    [SerializeField] float charge = 5.0f;
    [SerializeField] float minimalAlignment = 0.1f;
    [SerializeField] float minimalDistance = 0.5f;
    [SerializeField] float minimalAngle = 0.5f;
    [SerializeField] bool useLerp;
    [SerializeField] bool interpolates;
    [SerializeField] float deltaLerp = 0.2f;
    

    Collider cubeAttractedTo;
    LayerMask mask;
    float cubeSize = 0.4f;
    bool lerping = false;
    Vector3 endPosition;
    Vector3 startPosition;
    Quaternion endRotation;
    Quaternion startRotation;
    float time = 0;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        mask = LayerMask.GetMask("magnetic");
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

            Vector3 direction = cubeAttractedTo.transform.parent.position - centerOfMassPosition;
            Vector3 relativeDirection = cubeAttractedTo.transform.parent.InverseTransformDirection(direction);
            float rotation = Mathf.Abs((transform.rotation.eulerAngles.y - cubeAttractedTo.transform.parent.eulerAngles.y) % 90);
            float xDirection = -relativeDirection.x;
            float zDirection = -relativeDirection.z;

            if (interpolates && !lerping)
            {
                rb.interpolation = RigidbodyInterpolation.Interpolate;
            }
            
            //if not stable we keep applying force
           
            if (useLerp)
            {
                lerpingMagents(direction, rotation, xDirection, zDirection);
            }
            else
            {
                autoMagnets(direction, rotation, xDirection, zDirection);
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

        void allignBlock(float xDirection, float zDirection)
        {
            if (Mathf.Abs(xDirection) < minimalAlignment)
            {

                transform.position = cubeAttractedTo.transform.parent.position + cubeAttractedTo.transform.parent.forward * cubeSize * Mathf.Sign(zDirection);
                transform.rotation = cubeAttractedTo.transform.parent.rotation;

            }
            else
            {
                transform.position = cubeAttractedTo.transform.parent.position + cubeAttractedTo.transform.parent.right * cubeSize* Mathf.Sign(xDirection);
                transform.rotation = cubeAttractedTo.transform.parent.rotation;

            }
        }

        void autoMagnets(Vector3 direction, float rotation, float xDirection, float zDirection)
        {
            if (direction.magnitude <= minimalDistance && (rotation <= minimalAngle || 90 - rotation <= minimalAngle) && (Mathf.Abs(xDirection) <= minimalAlignment || Mathf.Abs(zDirection) <= minimalAlignment))
            {

                //Set speed to zero and change layer to magnetic.
                //We also set the object rigidbody to kinematic mode.
                
                //rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                rb.isKinematic = true;
                //gameObject.layer = 3;

                Debug.Log("Attached!!");

               
                this.transform.parent = cubeAttractedTo.transform.parent.parent;
              

                //Put the block in the correct position
                allignBlock(xDirection, zDirection);
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

    private void lerpingMagents(Vector3 direction, float rotation, float xDirection, float zDirection)
    {
        if (direction.magnitude > minimalDistance)
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


            Debug.Log("Attached!!");
            rb.interpolation = RigidbodyInterpolation.None;
            //Attach the object to the player
            this.transform.parent = cubeAttractedTo.transform.parent.parent;

            //Put the block in the correct position
            // allignBlock(xDirection, zDirection);
            //this.GetComponent<Feromagnetic>().enabled = false;
            lerping = true;
            startPosition = transform.localPosition;
            startRotation = transform.localRotation;

            float rotationSign = Mathf.Sign((transform.rotation.eulerAngles.y - cubeAttractedTo.transform.parent.eulerAngles.y) % 90);
            if (rotation > 45)
            {
                endRotation = Quaternion.Euler(new Vector3(0, (90 - rotation) * rotationSign, 0) + startRotation.eulerAngles);
            }
            else
            {
                endRotation = Quaternion.Euler(new Vector3(0, -rotation * rotationSign, 0) + startRotation.eulerAngles);
            }

            if (Mathf.Abs(xDirection) < Mathf.Abs(zDirection))
            {

                endPosition = transform.parent.InverseTransformPoint(cubeAttractedTo.transform.parent.position + cubeAttractedTo.transform.parent.forward * cubeSize * Mathf.Sign(zDirection));

            }
            else
            {
                endPosition = transform.parent.InverseTransformPoint(cubeAttractedTo.transform.parent.position + cubeAttractedTo.transform.parent.right * cubeSize * Mathf.Sign(xDirection));
            }
            rb.interpolation = RigidbodyInterpolation.None;
        }

        if (lerping && time <= 1)
        {
            
            transform.localPosition = Vector3.Lerp(startPosition, endPosition, time);
            transform.localRotation = Quaternion.Slerp(startRotation, endRotation, time);
            time += deltaLerp;
          


        }
        else if (time > 1)
        {
           // gameObject.layer = 3;
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
}
