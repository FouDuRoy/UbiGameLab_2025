using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UI;


public class ConeEjectionAndProjection : MonoBehaviour
{
    // Start is called before the first frame update
    List<Collider> magneticLast = new List<Collider>();
    void Start()
    {
    }

    public void coneAttraction(Transform player,float attractionForce,float angle, float distance,float magnitude, float time)
    {
        LayerMask mask = LayerMask.GetMask("magnetic");
        float angleFactor = Mathf.Clamp (1 + time,1,2);
        // Find all magneticblocs in radiusZone
        float sphereRadius = distance;
        List<Collider> magnetic = Physics.OverlapSphere(player.position, sphereRadius).ToList<Collider>();

        //Look for those that are in front of the player with maximum angle
        magnetic = magnetic.FindAll(cube => {
            //If the cube is used by a player dont pull
            if (cube.transform.root.GetComponent<PlayerObjects>() != null || cube.transform.tag != "magnetic")
            {
                return false;
            }
            Vector3 distanceBetweenPlayerAndCube = cube.transform.position - player.position;
            return Vector3.Angle(distanceBetweenPlayerAndCube, player.forward) <= angle* angleFactor;
        });

        // pull blocks in range
        magnetic.ForEach(cube =>
        {
            Vector3 distanceBetweenPlayerAndCube = player.position - cube.transform.position;
            Rigidbody cubeRB = cube.GetComponent<Rigidbody>();
            cubeRB.AddForce(distanceBetweenPlayerAndCube.normalized* attractionForce* magnitude, ForceMode.Acceleration);
            Feromagnetic fero = cube.GetComponent<Feromagnetic>();
            fero.ResetObject();
            fero.enabled = false;

        });
        //Remagnetize those that are not in range anymore
        magneticLast = magneticLast.FindAll(cube => !magnetic.Contains(cube));
        magneticLast.ForEach(cube => cube.GetComponent<Feromagnetic>().enabled = true);

        Debug.DrawRay(player.position, Quaternion.AngleAxis(angle* angleFactor, Vector3.up) * player.forward*distance,Color.red, Time.deltaTime);
        Debug.DrawRay(player.position, Quaternion.AngleAxis(-angle* angleFactor, Vector3.up) * player.forward *distance, Color.red, Time.deltaTime);
        magneticLast = magnetic;
    }

    public void resetMagneticLast()
    {
        magneticLast.ForEach(cube => cube.GetComponent<Feromagnetic>().enabled = true);
        magneticLast.Clear();
    }
}
