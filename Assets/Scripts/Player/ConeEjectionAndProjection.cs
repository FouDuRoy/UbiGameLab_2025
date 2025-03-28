using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;


public class ConeEjectionAndProjection : MonoBehaviour
{
    [SerializeField] float rightDriftProportion = 0.1f;

    // Start is called before the first frame update
    [SerializeField] float attractionForce = 10f;
    [SerializeField] float initialAngle = 45f;
    [SerializeField] float maxAngleRepulsion = 90f;
    [SerializeField] float secondsForMaxCharging = 2f;
    [SerializeField] float distance = 10f;
    [SerializeField] float secondsForMaxChargingEjection = 3f;
    [SerializeField] float ejectionSpeed = 3f;
    [SerializeField] float colorChangeIntensity = 3f;
    List<Collider> magneticLast = new List<Collider>();
    List<GameObject> blocsToEject = new List<GameObject>();
    GridSystem playerGrid;
    PlayerInput playerInput;
    

    InputAction ejectCubes;
    InputAction AttractCubes;
    HapticFeedbackController feedback;
    float timeHeld = 0;
    float handTimer = 0;
    Rigidbody mainCubeRb;
    Transform golem;
    bool rightTriggerHeld = false;
    bool leftTriggerHeld = false;
    Transform leftHandTransform;
    Transform rightHandTransform;
    Vector3 leftHandInitialPoint;
    Vector3 rightHandInitialPoint;
    Vector3 leftHandFinalPoint;
    Vector3 rightHandFinalPoint;
    MouvementType moveType;
    Color chargedColor;
    [Header("LineSection")]
    [SerializeField] Material lineMat;
    public float maxAngle = 15f;
    LineRenderer leftRay;
    LineRenderer rightRay;
    void Start()
    {
        playerGrid = GetComponent<GridSystem>();
        playerInput = GetComponent<PlayerInput>();
        ejectCubes =  playerInput.actions.FindAction("BlocEjection");
        AttractCubes = playerInput.actions.FindAction("AttractCubes");
        feedback =  this.GetComponent<HapticFeedbackController>();
        mainCubeRb = this.GetComponent<PlayerObjects>().cubeRb;
        golem = this.GetComponent<PlayerObjects>().golem.transform;
        if (secondsForMaxCharging >= 2)
        {
            secondsForMaxCharging -= 1;
        }
        moveType = this.transform.GetComponent<PlayerMouvement>().moveType;
        leftHandTransform = golem.Find("L_Hand_IK_Target");
        rightHandTransform = golem.Find("R_Hand_IK_Target");
        leftRay = CreateRay(Color.red);
        rightRay = CreateRay(Color.red);
        Color curentColor = playerGrid.playerMat.color;
        chargedColor = new Color(curentColor.r * colorChangeIntensity, curentColor.g * colorChangeIntensity, curentColor.b * colorChangeIntensity);
    }
    void FixedUpdate()
    {
        float rightTrigger = ejectCubes.ReadValue<float>();
        float leftTrigger = AttractCubes.ReadValue<float>();
        if ((leftTrigger > 0 && rightTrigger==0) )
        {
            if(leftTriggerHeld == false)
            {
                feedback.AttractionVibrationStart();
               
            }

            int maxX = playerGrid.grid.Keys.Max(x => x.x);
            int minX = playerGrid.grid.Keys.Min(x => x.x);
            int maxZ = playerGrid.grid.Keys.Max(x => x.z);
            int minZ = playerGrid.grid.Keys.Min(x => x.z);
            int radiusInBlocs = Mathf.Max(Mathf.Abs(maxX), Mathf.Abs(minX), Mathf.Abs(maxZ), Mathf.Abs(minZ));
            float blocSizeWorld = playerGrid.cubeSize * playerGrid.kernel.transform.lossyScale.x;
            float radius = radiusInBlocs * blocSizeWorld;
            float maxDistance = MaxDistanceForDirection((golem.forward).normalized, radius);
            leftHandInitialPoint = mainCubeRb.position + Quaternion.AngleAxis(-initialAngle, Vector3.up) * golem.forward * maxDistance + new Vector3(0, 0.15f, 0);
            rightHandInitialPoint = mainCubeRb.position + Quaternion.AngleAxis(+initialAngle, Vector3.up) * golem.forward * maxDistance + new Vector3(0, 0.15f, 0);
            leftHandFinalPoint = mainCubeRb.position + Quaternion.AngleAxis(-initialAngle, Vector3.up) * golem.forward * distance + new Vector3(0, 0.15f, 0); 
            rightHandFinalPoint = mainCubeRb.position + Quaternion.AngleAxis(+initialAngle, Vector3.up) * golem.forward * distance + new Vector3(0, 0.15f, 0); 
            if (handTimer <= 1f / 2)
            {
                
                leftHandTransform.position = Vector3.Lerp(leftHandInitialPoint, leftHandFinalPoint, 2 * handTimer);
                rightHandTransform.position = Vector3.Lerp(rightHandInitialPoint, rightHandFinalPoint, 2 * handTimer);

            }
            else
            {

                leftHandTransform.position = Vector3.Lerp( leftHandFinalPoint, leftHandInitialPoint, 2*(handTimer-1f/2));
                rightHandTransform.position = Vector3.Lerp( rightHandFinalPoint, rightHandInitialPoint, 2 * (handTimer - 1f/ 2));
            }
            
            
            handTimer += Time.fixedDeltaTime;
            handTimer = handTimer % 1;
            coneAttraction(golem,attractionForce,initialAngle,distance,1);
            leftTriggerHeld = true;

       
        }
        else if (leftTriggerHeld)
        {
            feedback.AttractionVibrationEnd();
            leftTriggerHeld = false;
            leftRay.gameObject.SetActive(false);
            rightRay.gameObject.SetActive(false);
            resetMagneticLast();
        }

        if((rightTrigger > 0))
        {
            leftRay.gameObject.SetActive(true);
            rightRay.gameObject.SetActive(true);
            coneProjectionColor(timeHeld);
            if (timeHeld == 0) //On appelle VibrationStart une seule fois, au début
            {
                feedback.RepulsionVibrationStart(secondsForMaxCharging);
            }
            timeHeld += Time.fixedDeltaTime;
            rightTriggerHeld = true;

            //Draw rays to indicate current range
            timeHeld = Mathf.Clamp(timeHeld, 0, secondsForMaxChargingEjection);
            float maxAngle = initialAngle+(maxAngleRepulsion-initialAngle)*(timeHeld/secondsForMaxChargingEjection);

            //Draw Rays in Build
            Vector3 origin = golem.position;

            Vector3 rightDir = Quaternion.AngleAxis(maxAngle, Vector3.up) * golem.forward;
            Vector3 leftDir = Quaternion.AngleAxis(-maxAngle, Vector3.up) * golem.forward;


            rightRay.SetPosition(0, origin);
            rightRay.SetPosition(1, origin + rightDir * distance);

            leftRay.SetPosition(0, origin);
            leftRay.SetPosition(1, origin + leftDir * distance);
            //Debug.DrawRay(golem.position, Quaternion.AngleAxis(maxAngle, Vector3.up) * golem.forward*distance,Color.red, Time.deltaTime);
            //Debug.DrawRay(golem.position, Quaternion.AngleAxis(-maxAngle, Vector3.up) * golem.forward *distance, Color.red, Time.deltaTime);
        }
        else if (rightTriggerHeld)
        {
            feedback.RepulsionVibrationEnd(timeHeld);
            coneProjection(timeHeld);
            leftRay.gameObject.SetActive(false);
            rightRay.gameObject.SetActive(false);
            rightTriggerHeld = false;
            timeHeld = 0;
        }
    }
    LineRenderer CreateRay(Color color)
    {
        GameObject go = new GameObject("Ray");
        var lr = go.AddComponent<LineRenderer>();
        lr.material = lineMat;
        lr.startColor = color;
        lr.endColor = color;
        lr.startWidth = 0.05f;
        lr.endWidth = 0.05f;
        lr.positionCount = 2;
        return lr;
    }
    public void coneAttraction(Transform player,float attractionForce,float angle, float distance,float magnitude)
    {
        leftRay.gameObject.SetActive(true);
        rightRay.gameObject.SetActive(true);
        LayerMask mask = LayerMask.GetMask("magnetic");
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
            
            return Vector3.Angle(distanceBetweenPlayerAndCube, player.forward) <= angle;
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
            cubeRB.useGravity = false;

        });
        //Remagnetize those that are not in range anymore
        magneticLast = magneticLast.FindAll(cube => !magnetic.Contains(cube));
        magneticLast.ForEach(cube => cube.GetComponent<Feromagnetic>().enabled = true);

        //Draw Rays in Build
        Vector3 origin = golem.position;

        Vector3 rightDir = Quaternion.AngleAxis(angle, Vector3.up) * golem.forward;
        Vector3 leftDir = Quaternion.AngleAxis(-angle, Vector3.up) * golem.forward;


        rightRay.SetPosition(0, origin);
        rightRay.SetPosition(1, origin + rightDir * distance);

        leftRay.SetPosition(0, origin);
        leftRay.SetPosition(1, origin + leftDir * distance);
        //Debug.DrawRay(player.position, Quaternion.AngleAxis(angle, Vector3.up) * player.forward*distance,Color.red, Time.deltaTime);
        //Debug.DrawRay(player.position, Quaternion.AngleAxis(-angle, Vector3.up) * player.forward *distance, Color.red, Time.deltaTime);
        magneticLast = magnetic;
    }

    public void resetMagneticLast()
    {
        magneticLast.ForEach(cube => {
            cube.GetComponent<Feromagnetic>().enabled = true;
            cube.GetComponent<Rigidbody>().useGravity = true; });
        magneticLast.Clear();
    }

    public void coneProjection(float time){
        
        int maxX = playerGrid.grid.Keys.Max(x => x.x);
        int minX = playerGrid.grid.Keys.Min(x => x.x);
        int maxZ = playerGrid.grid.Keys.Max(x => x.z);
        int minZ = playerGrid.grid.Keys.Min(x => x.z);
        int radiusInBlocs = Mathf.Max(Mathf.Abs(maxX),Mathf.Abs(minX),Mathf.Abs(maxZ),Mathf.Abs(minZ));
        
        float blocSizeWorld = playerGrid.cubeSize*playerGrid.kernel.transform.lossyScale.x;
        float radius = radiusInBlocs*blocSizeWorld;
        float boundaryDistanceRatio = time/secondsForMaxChargingEjection;
        float maxAngle = initialAngle+(maxAngleRepulsion-initialAngle)*(boundaryDistanceRatio);
       
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
    public void coneProjectionColor(float time)
    {

        int maxX = playerGrid.grid.Keys.Max(x => x.x);
        int minX = playerGrid.grid.Keys.Min(x => x.x);
        int maxZ = playerGrid.grid.Keys.Max(x => x.z);
        int minZ = playerGrid.grid.Keys.Min(x => x.z);
        int radiusInBlocs = Mathf.Max(Mathf.Abs(maxX), Mathf.Abs(minX), Mathf.Abs(maxZ), Mathf.Abs(minZ));

        float blocSizeWorld = playerGrid.cubeSize * playerGrid.kernel.transform.lossyScale.x;
        float radius = radiusInBlocs * blocSizeWorld;
        float boundaryDistanceRatio = time / secondsForMaxChargingEjection;
        float maxAngle = initialAngle + (maxAngleRepulsion - initialAngle) * (boundaryDistanceRatio);

        List<Collider> magnetic = Physics.OverlapSphere(golem.position, radius).ToList<Collider>();
        magnetic = magnetic.FindAll(cube => {
            //Look if cube is on the player
            cube.gameObject.GetComponent<Renderer>().material.color = playerGrid.playerMat.color;
            if (cube.gameObject == mainCubeRb.gameObject)
            {
                return false;
            }
            if (cube.transform.root != transform)
            {

                return false;
            }
            if(cube.gameObject.GetComponent<Renderer>().material.color == chargedColor)
            {
                return false;
            }
            //Look if the cube is within the angle of ejection
            float angle = Vector3.Angle(cube.transform.position - golem.position, golem.forward);
            if (angle > maxAngle)
            {
                return false;
            }

            //look if the cube is within the neighberhood of the boundary
            float maxDistance = MaxDistanceForDirection((cube.transform.position - golem.position).normalized, radius);
            float distance = (cube.transform.position - golem.position).magnitude;
            float boundaryDistanceMax = (1 - boundaryDistanceRatio) * maxDistance;
            if (distance < boundaryDistanceMax)
            {
                return false;
            }
            return true;
        });
        magnetic.ForEach(x => {
            x.gameObject.GetComponent<Renderer>().material.color = chargedColor;
                });
        playerGrid.coneEjectRest(ejectionSpeed, rightDriftProportion);
    }
    private void EjectBloc(GameObject cube, Transform golem)
    {
   
        cube.transform.parent = this.transform.parent;
        playerGrid.DetachBlocSingle(cube);

        //Add rigidBody
      
        float rightDrift = golem.InverseTransformPoint(cube.transform.position).x;
        cube.GetComponent<Rigidbody>().interpolation = RigidbodyInterpolation.Interpolate;
        cube.GetComponent<Rigidbody>().AddForce((golem.forward + golem.right *rightDrift * rightDriftProportion) *ejectionSpeed, ForceMode.VelocityChange );
        cube.GetComponent<Bloc>().state = BlocState.projectile;
       
        //Remove owner of cube
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
