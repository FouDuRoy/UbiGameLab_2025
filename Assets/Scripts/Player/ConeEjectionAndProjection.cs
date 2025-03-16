using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor.Callbacks;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.ProBuilder.Shapes;
using UnityEngine.UI;


public class ConeEjectionAndProjection : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] float attractionForce = 10f;
    [SerializeField] float initialAngle = 45f;
    [SerializeField] float secondsForMaxCharging = 2f;
    [SerializeField] float distance = 10f;
    List<Collider> magneticLast = new List<Collider>();
    List<GameObject> blocsToEject = new List<GameObject>();
    GridSystem playerGrid;
    PlayerInput playerInput;
    

    InputAction ejectCubes;
    InputAction AttractCubes;
    HapticFeedbackController feedback;
    float timeHeld = 0;
    Rigidbody mainCubeRb;
    bool rightTriggerHeld = false;
    bool leftTriggerHeld = false;
    void Start()
    {
        playerGrid = GetComponent<GridSystem>();
        playerInput = GetComponent<PlayerInput>();
        ejectCubes =  playerInput.actions.FindAction("BlocEjection");
        AttractCubes = playerInput.actions.FindAction("AttractCubes");
        feedback =  this.GetComponent<HapticFeedbackController>();
        mainCubeRb = this.GetComponent<PlayerObjects>().cubeRb;
          if (secondsForMaxCharging >= 2)
        {
            secondsForMaxCharging -= 1;
        }
    }
    void FixedUpdate()
    {
        float rightTrigger = ejectCubes.ReadValue<float>();
        float leftTrigger = AttractCubes.ReadValue<float>();
        
         if((leftTrigger > 0 && rightTrigger==0) ||(leftTrigger > 0 && leftTriggerHeld ) )
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
            timeHeld += Time.fixedDeltaTime*5f/6;
            rightTriggerHeld = true;
        }
        else if (rightTriggerHeld)
        {
           // coneProjection(timeHeld);
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

    public void coneProjection(float time){

        int maxX = playerGrid.grid.Keys.Max(x => x.x);
        int minX = playerGrid.grid.Keys.Min(x => x.x);
        int rows = 0;
        int numberPos = Mathf.Min(rows,maxX);
        int numberNeg = Mathf.Min(rows,minX);
        Vector3Int currentBloc = new Vector3Int(0,0,0);
        while(playerGrid.grid.ContainsKey(currentBloc)){

            currentBloc += new Vector3Int(0,0,1);
        }
        currentBloc -= new Vector3Int(0,0,1);
        EjectBloc(playerGrid.grid[currentBloc]);

        for(int i = 1; i <=numberPos; i++){
            currentBloc = new Vector3Int(i,0,0);
            while(playerGrid.grid.ContainsKey(currentBloc)){

                currentBloc += new Vector3Int(0,0,1);
            }
            currentBloc -= new Vector3Int(0,0,1);
            EjectBloc(playerGrid.grid[currentBloc]);
            
        }
        
        for(int i = 1; i <=numberNeg; i++){
            currentBloc = new Vector3Int(-i,0,0);
            while(playerGrid.grid.ContainsKey(currentBloc)){

                currentBloc += new Vector3Int(0,0,1);
            }
            currentBloc -= new Vector3Int(0,0,1);
            EjectBloc(playerGrid.grid[currentBloc]);
        }
    }

    private void EjectBloc(GameObject cube)
    {
        float ejectionSpeed = 10f;
        cube.gameObject.layer = 0;
        cube.transform.parent = this.transform.parent;
        playerGrid.DetachBlocSingle(cube);
        Rigidbody rb = this.GetComponent<PlayerObjects>().cubeRb;
        Debug.Log(cube);
        //Add rigidBody
        GetComponent<PlayerObjects>().addRigidBody(cube);

        cube.GetComponent<Rigidbody>().interpolation = RigidbodyInterpolation.Interpolate;
        cube.GetComponent<Rigidbody>().AddForce((rb.transform.forward + rb.transform.right * cube.transform.position.x * 0.1f)*ejectionSpeed, ForceMode.VelocityChange );
        cube.GetComponent<Bloc>().owner += "projectile";
        //Remove owner of cube
        StartCoroutine(blockNeutral(cube));
    }

    IEnumerator blockNeutral(GameObject block)
    {

        yield return new WaitForSeconds(3f);
        if(block !=null)
        {
            block.GetComponent<Bloc>().setOwner("Neutral");
        }
       
    }
}
