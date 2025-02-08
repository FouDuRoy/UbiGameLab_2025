using UnityEngine;

public class Magnetic : MonoBehaviour
{
    Vector3 centerOfMassPosition;
    Rigidbody rb;
   
    [SerializeField] float radius = 5.0f;
    [SerializeField] float charge = 5.0f;
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        centerOfMassPosition = transform.position;
        LayerMask mask = LayerMask.GetMask("magnetic");
        Collider[] magnetic = Physics.OverlapSphere(centerOfMassPosition, radius, mask);
        
        foreach (Collider c in magnetic)
        {
            
            if (c.gameObject != gameObject) {
                Vector3 distance =transform.position-c.transform.position;
                float charge2 = c.GetComponent<Magnetic>().getCharge();
                this.rb.AddForce(CoulombLaw(distance, charge, charge2));
                
            }
                
        }
        
    }

    private static Vector3 CoulombLaw(Vector3 distance,float charge1,float charge2)
    {
        float normSqauredInverse = 1.0f / Mathf.Pow(distance.magnitude, 2);
        return -charge1 * charge2*normSqauredInverse* distance.normalized;

    }
    public float getCharge()
    {
        return charge; 
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(centerOfMassPosition, radius);
    }
}
