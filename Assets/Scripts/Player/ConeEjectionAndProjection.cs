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
    void Start()
    {
    }

    public static void coneAttraction(Transform player,float attractionForce,float angle, float distance)
    {
        LayerMask mask = LayerMask.GetMask("magnetic");
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
            return Vector3.Angle(distanceBetweenPlayerAndCube, player.forward) < angle;
        });
        // pull blocks in range
        magnetic.ForEach(cube =>
        {
            Vector3 distanceBetweenPlayerAndCube = player.position - cube.transform.position;
            Rigidbody cubeRB = cube.GetComponent<Rigidbody>();
            cubeRB.AddForce(distanceBetweenPlayerAndCube.normalized* attractionForce, ForceMode.Acceleration);

        });
    }
}
