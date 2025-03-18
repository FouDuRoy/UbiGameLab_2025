using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor.Callbacks;
using UnityEditor.ShaderGraph.Internal;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.ProBuilder.Shapes;
using UnityEngine.UI;


public class ConeEjectionAndProjection : MonoBehaviour
{
    [SerializeField] float rightDriftProportion = 0.1f;

    // Start is called before the first frame update
    [SerializeField] float attractionForce = 10f;
    [SerializeField] float initialAngle = 45f;
    [SerializeField] float secondsForMaxCharging = 2f;
    [SerializeField] float distance = 10f;
    [SerializeField] float secondsForMaxChargingEjection = 3f;
    [SerializeField] float ejectionSpeed = 3f;
    List<Collider> magneticLast = new List<Collider>();
    List<GameObject> blocsToEject = new List<GameObject>();
    GridSystem playerGrid;
    PlayerInput playerInput;
    

    InputAction ejectCubes;
    InputAction AttractCubes;
    HapticFeedbackController feedback;
    float timeHeld = 0;
    Rigidbody mainCubeRb;
    Transform golem;
    bool rightTriggerHeld = false;
    bool leftTriggerHeld = false;
    MouvementType moveType;
    void Start()
    {
        playerGrid = GetComponent<GridSystem>();
        playerInput = GetComponent<PlayerInput>();
        ejectCubes =  playerInput.actions.FindAction("BlocEjection");
        AttractCubes = playerInput.actions.FindAction("AttractCubes");
        feedback =  this.GetComponent<HapticFeedbackController>();
        mainCubeRb = this.GetComponent<PlayerObjects>().cubeRb;
        golem = mainCubeRb.transform.Find("GolemBuilt");
          if (secondsForMaxCharging >= 2)
        {
            secondsForMaxCharging -= 1;
        }
        moveType = this.transform.GetComponent<PlayerMouvement>().moveType;
    }
    void FixedUpdate()
    {
        float rightTrigger = ejectCubes.ReadValue<float>();
        float leftTrigger = AttractCubes.ReadValue<float>();
        if ((leftTrigger > 0 && rightTrigger==0) ||(leftTrigger > 0 && leftTriggerHeld ) )
        {
            if(leftTriggerHeld == false)
            {
                feedback.AttractionVibrationStart();
            }
            coneAttraction(mainCubeRb.transform.Find("GolemBuilt").transform, attractionForce
                ,initialAngle,distance,leftTrigger, timeHeld);
            leftTriggerHeld = true;

            timeHeld += Time.fixedDeltaTime*5f/6;
        }
        else if (leftTriggerHeld)
        {
            feedback.AttractionVibrationEnd();
            leftTriggerHeld = false;
            resetMagneticLast();
            timeHeld = 0;
        }

        if((rightTrigger > 0 && leftTrigger==0) ||(rightTrigger > 0 && rightTriggerHeld ))
        {
            if (timeHeld == 0) //On appelle VibrationStart une seule fois, au début
            {
                feedback.RepulsionVibrationStart(secondsForMaxCharging);
            }
            timeHeld += Time.fixedDeltaTime*5f/6;
            rightTriggerHeld = true;

            //Draw rays to indicate current range
            float maxAngle = initialAngle+(90-initialAngle)*(timeHeld/secondsForMaxChargingEjection);
             Debug.DrawRay(golem.position, Quaternion.AngleAxis(maxAngle, Vector3.up) * golem.forward*distance,Color.red, Time.deltaTime);
             Debug.DrawRay(golem.position, Quaternion.AngleAxis(-maxAngle, Vector3.up) * golem.forward *distance, Color.red, Time.deltaTime);
        }
        else if (rightTriggerHeld)
        {
            feedback.RepulsionVibrationEnd(timeHeld);
            coneProjection(timeHeld);
            rightTriggerHeld = false;
            timeHeld = 0;
        }
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
            if (cube.transform.root.GetComponent<PlayerObjects>() != null || cube.transform.tag != "magnetic" || cube.GetComponent<Bloc>().owner!="Neutral")
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

    public void coneProjection(float time){
        
        Transform golem = mainCubeRb.transform.Find("GolemBuilt").transform ; 
        int maxX = playerGrid.grid.Keys.Max(x => x.x);
        int minX = playerGrid.grid.Keys.Min(x => x.x);
        int maxZ = playerGrid.grid.Keys.Max(x => x.z);
        int minZ = playerGrid.grid.Keys.Min(x => x.z);
        int radiusInBlocs = Mathf.Max(Mathf.Abs(maxX),Mathf.Abs(minX),Mathf.Abs(maxZ),Mathf.Abs(minZ));
        
        float blocSizeWorld = playerGrid.cubeSize*playerGrid.kernel.transform.lossyScale.x;
        float radius = radiusInBlocs*blocSizeWorld;
        float boundaryDistanceRatio = time/secondsForMaxChargingEjection;
        float maxAngle = initialAngle+(90-initialAngle)*(boundaryDistanceRatio);
       
       List<Collider> magnetic = Physics.OverlapSphere(golem.position, radius).ToList<Collider>();
       magnetic = magnetic.FindAll(cube => {
            //Look if cube is on the player
            if(cube.gameObject == mainCubeRb.gameObject){
                return false;
            }
            if(cube.transform.root != transform){
                
                return false;
            }

            //Look if the cube is within the angle of ejection
            float angle =  Vector3.Angle(cube.transform.position-golem.position,golem.forward);
            if(angle > maxAngle){
                return false;
            }
            
            //look if the cube is within the neighberhood of the boundary
            float maxDistance = MaxDistanceForDirection((cube.transform.position-golem.position).normalized,radius);
            float distance = (cube.transform.position-golem.position).magnitude;
            float boundaryDistanceMax = (1-boundaryDistanceRatio)*maxDistance;
            if(distance < boundaryDistanceMax){
                return false;
            }
            return true;
        });
       magnetic.ForEach(x => EjectBloc(x.gameObject,golem));
       playerGrid.coneEjectRest(ejectionSpeed, rightDriftProportion);
    }

    private void EjectBloc(GameObject cube, Transform golem)
    {
        cube.gameObject.layer = 0;
        cube.transform.parent = this.transform.parent;
        playerGrid.DetachBlocSingle(cube);

        //Add rigidBody
      
        float rightDrift = golem.InverseTransformPoint(cube.transform.position).x;
        cube.GetComponent<Rigidbody>().interpolation = RigidbodyInterpolation.Interpolate;
        cube.GetComponent<Rigidbody>().AddForce((golem.forward + golem.right *rightDrift * rightDriftProportion) *ejectionSpeed, ForceMode.VelocityChange );
        cube.GetComponent<Bloc>().state = BlocState.projectile;
       
        //Remove owner of cube
    }
    IEnumerator blockNeutral(GameObject block)
    {
        yield return new WaitForSeconds(3f);
        if(block !=null)
        {
            block.GetComponent<Bloc>().setOwner("Neutral");
            block.GetComponent<Bloc>().state = BlocState.none;
        }
       
    }
    public float MaxDistanceForDirection(Vector3 direction, float radius){


        Ray ray = new Ray(mainCubeRb.position, direction);
        List<RaycastHit> hits = Physics.RaycastAll(ray, radius, ~0,QueryTriggerInteraction.Ignore).ToList<RaycastHit>();
        hits = hits.FindAll(hit => {
            if(hit.collider.transform.root != transform){
                return false;
            }
            return true;
        });
        
        float maxDistance = 0;

        foreach (RaycastHit hit in hits)
        {
            float distance = Vector3.Distance(mainCubeRb.position, hit.collider.transform.position);
            if (distance > maxDistance)
            {
                maxDistance = distance;
            }
        }
        return maxDistance;
    }
}
