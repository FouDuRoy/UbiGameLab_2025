using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.Rendering.DebugUI;


public class ConeEjectionAndProjection : MonoBehaviour
{
    [SerializeField] float rightDriftProportion = 0.1f;

    // Start is called before the first frame update
    [SerializeField] float attractionForce = 10f;
    [SerializeField] float ejectionAngle = 90f;
    public bool cancelEjectionShoot = false;
    [SerializeField] float initialAngle = 45f;
    [SerializeField] float angleRepulsion = 90f;
    float angle;
    public int blocHeight =1;
    public float blocDiffY = 0.1f;
    [SerializeField] float secondsForMaxCharging = 2f;
    [SerializeField] float distance = 10f;
    [SerializeField] float secondsForMaxChargingEjection = 3f;
    [SerializeField] float secondsForMaxChargingEjectionLength = 10f;
    [SerializeField] float maxNumberBlocsEjection = 20f;
    [SerializeField] float ejectionSpeed = 3f;
    [SerializeField] float colorChangeIntensity = 3f;
    [SerializeField] int maxBlocs = 5;
    [SerializeField] float displaceTimeBloc = 0.1f;
    int nbBlocsSelect;
    List<Collider> magneticLast = new List<Collider>();

    Animator animator;


    RaycastHit[] hitsArray = new RaycastHit[500];
    Collider[] magnetic = new Collider[500];
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
    string lastHold = string.Empty;
    Transform leftHandTransform;
    Transform rightHandTransform;
    Vector3 leftHandInitialPoint;
    Vector3 rightHandInitialPoint;
    Vector3 leftHandFinalPoint;
    Vector3 rightHandFinalPoint;
    MouvementType moveType;
    Color chargedColor;
    LayerMask mask;

    [Header("LineSection")]
    [SerializeField] Material lineMat;
    public float maxAngle = 15f;
    LineRenderer leftRay;
    LineRenderer rightRay;


    [Header("ConeSection")]
    [SerializeField] GameObject visionConeObject;
    private MeshFilter coneMeshFilter;
    private MeshRenderer coneRenderer;
    public float distanceCone = 5f;
    public int resolution = 20; // Plus c�est haut, plus le c�ne est lisse
    public Material visionMaterial;
    List<GameObject> blocsToEject = new List<GameObject>();
    List<GameObject> potentialBlocs= new List<GameObject>();
    bool readyToEject = false;
    void Start()
    {

    }
    void Awake()

    {
        animator = GetComponentInChildren<Animator>();

        //Cone vision attribution Mesh
        mask = LayerMask.GetMask("magnetic");
        coneMeshFilter = visionConeObject.GetComponent<MeshFilter>();
        coneRenderer = visionConeObject.GetComponent<MeshRenderer>();
        coneRenderer.material = visionMaterial;


        playerGrid = GetComponent<GridSystem>();
        playerInput = GetComponent<PlayerInput>();
        ejectCubes = playerInput.actions.FindAction("BlocEjection");
        AttractCubes = playerInput.actions.FindAction("AttractCubes");
        feedback = this.GetComponent<HapticFeedbackController>();
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


    void Update()
    {
        if(blocsToEject.Count == 0)
        {
            readyToEject = false;
        }
        float rightTrigger = ejectCubes.ReadValue<float>();
        float leftTrigger = AttractCubes.ReadValue<float>();
        if (AttractCubes.WasPressedThisFrame())
        {
            lastHold = "left";
        }
        else if (ejectCubes.WasPressedThisFrame())
        {
            lastHold = "right";
        }

        if ((leftTrigger > 0 && lastHold == "left"))
        {
            if (leftTriggerHeld == false)
            {
                feedback.AttractionVibrationStart();

            }

            int maxX = playerGrid.grid.Keys.Max(x => x.x);
            int minX = playerGrid.grid.Keys.Min(x => x.x);
            int maxZ = playerGrid.grid.Keys.Max(x => x.z);
            int minZ = playerGrid.grid.Keys.Min(x => x.z);
            int maxY = playerGrid.grid.Keys.Max(x => x.y);
            int minY = playerGrid.grid.Keys.Min(x => x.y);

            int radiusInBlocs = Mathf.Max(Mathf.Abs(maxX), Mathf.Abs(minX), Mathf.Abs(maxZ), Mathf.Abs(minZ), Mathf.Abs(maxY), Mathf.Abs(minY));
            float blocSizeWorld = playerGrid.cubeSize * playerGrid.kernel.transform.lossyScale.x;
            float radius = radiusInBlocs * blocSizeWorld;
            float maxDistance = MaxDistanceForDirection((golem.forward).normalized, radius);
            leftHandInitialPoint = mainCubeRb.position + Quaternion.AngleAxis(-initialAngle, Vector3.up) * golem.forward * maxDistance + new Vector3(0, 0.15f, 0);
            rightHandInitialPoint = mainCubeRb.position + Quaternion.AngleAxis(+initialAngle, Vector3.up) * golem.forward * maxDistance + new Vector3(0, 0.15f, 0);
            leftHandFinalPoint = mainCubeRb.position + Quaternion.AngleAxis(-initialAngle, Vector3.up) * golem.forward * distance + new Vector3(0, 0.15f, 0);
            rightHandFinalPoint = mainCubeRb.position + Quaternion.AngleAxis(+initialAngle, Vector3.up) * golem.forward * distance + new Vector3(0, 0.15f, 0);
            if (handTimer <= 1f / 2)
            {

                //                leftHandTransform.position = Vector3.Lerp(leftHandInitialPoint, leftHandFinalPoint, 2 * handTimer);
                // rightHandTransform.position = Vector3.Lerp(rightHandInitialPoint, rightHandFinalPoint, 2 * handTimer);

            }
            else
            {

                // leftHandTransform.position = Vector3.Lerp(leftHandFinalPoint, leftHandInitialPoint, 2 * (handTimer - 1f / 2));
                // rightHandTransform.position = Vector3.Lerp(rightHandFinalPoint, rightHandInitialPoint, 2 * (handTimer - 1f / 2));
            }


            handTimer += Time.deltaTime;
            handTimer = handTimer % 1;
            coneAttraction(golem, attractionForce, initialAngle, distance, 1);
            leftTriggerHeld = true;


        }
        else if (leftTriggerHeld)
        {
            animator.SetBool("IsPulling", false);
            feedback.AttractionVibrationEnd();
            leftTriggerHeld = false;
            leftRay.gameObject.SetActive(false);
            rightRay.gameObject.SetActive(false);
            visionConeObject.gameObject.SetActive(false);
            resetMagneticLast();
        }
        EjectionAlgo(rightTrigger);
    }

    private void EjectionAlgo(float rightTrigger)
    {
        if (rightTrigger > 0 && lastHold == "right")
        {
            animator.SetTrigger("IsChargingLaunch");
            leftRay.gameObject.SetActive(true);
            rightRay.gameObject.SetActive(true);
            if(blocsToEject.Count< maxBlocs && playerGrid.grid.Count > 1)
            {
                ConeProjectionSelection(timeHeld);

            }
          
            timeHeld += Time.deltaTime;
            rightTriggerHeld = true;
            /*
            //Draw rays to indicate current range
            float maxAngle = angleRepulsion;

            //Draw Rays in Build
            Vector3 origin = golem.position;

            Vector3 rightDir = Quaternion.AngleAxis(maxAngle, Vector3.up) * golem.forward;
            Vector3 leftDir = Quaternion.AngleAxis(-maxAngle, Vector3.up) * golem.forward;


            rightRay.SetPosition(0, origin);
            rightRay.SetPosition(1, origin + rightDir * distance);

            leftRay.SetPosition(0, origin);
            leftRay.SetPosition(1, origin + leftDir * distance);


            visionConeObject.SetActive(true);
            visionConeObject.transform.position = golem.position + new Vector3(0f, -0.5f, 0f);
            visionConeObject.transform.rotation = Quaternion.LookRotation(golem.forward);
            GenerateMesh(maxAngle);
            //Debug.DrawRay(golem.position, Quaternion.AngleAxis(maxAngle, Vector3.up) * golem.forward*distance,Color.red, Time.deltaTime);
            //Debug.DrawRay(golem.position, Quaternion.AngleAxis(-maxAngle, Vector3.up) * golem.forward *distance, Color.red, Time.deltaTime);*/ // Pas de cône affiché pour la répulsion
        }
        else if (rightTriggerHeld)
        {
           
            if (lastHold == "right" || cancelEjectionShoot)
            {
                coneProjection();

            }else{
                coneReset();
            }
            animator.SetTrigger("IsLaunching");
            animator.ResetTrigger("IsChargingLaunch");

            visionConeObject.SetActive(false);
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
    public void coneAttraction(Transform player, float attractionForce, float angle, float distance, float magnitude)
    {
        /*
        leftRay.gameObject.SetActive(true);
        rightRay.gameObject.SetActive(true);*/ // Plus besoin des ray, le cône au sol fait le taff nickel

        // Find all magneticblocs in radiusZone
        float sphereRadius = distance;
        List<Collider> magnetic = Physics.OverlapSphere(player.position, sphereRadius).ToList<Collider>();

        //Look for those that are in front of the player with maximum angle
        magnetic = magnetic.FindAll(cube =>
        {
            //If the cube is used by a player dont pull
            if (cube.transform.root.GetComponent<PlayerObjects>() != null || (cube.transform.tag != "magnetic" && cube.transform.tag != "explosive" ) || cube.GetComponent<Bloc>().owner != "Neutral")
            {
                return false;
            }
            Vector3 projectedCubePosition = new Vector3(cube.transform.position.x, 0, cube.transform.position.z);
            Vector3 projectedPlayerPosition = new Vector3(player.position.x, 0, player.position.z);
            Vector3 distanceBetweenPlayerAndCube = projectedCubePosition - projectedPlayerPosition;

            return Vector3.Angle(distanceBetweenPlayerAndCube, player.forward) <= angle;
        });

        // pull blocks in range
        magnetic.ForEach(cube =>
        {
            Vector3 distanceBetweenPlayerAndCube = player.position - cube.transform.position;
            Rigidbody cubeRB = cube.GetComponent<Rigidbody>();
            cubeRB.AddForce(distanceBetweenPlayerAndCube.normalized * attractionForce * magnitude * Time.deltaTime * 100f, ForceMode.Acceleration);
            Feromagnetic fero = cube.GetComponent<Feromagnetic>();
            fero.ResetObject();
            fero.enabled = false;
            cubeRB.useGravity = false;

        });
        //Remagnetize those that are not in range anymore
        magneticLast = magneticLast.FindAll(cube => !magnetic.Contains(cube));
        magneticLast.ForEach(cube =>
        {
            cube.GetComponent<Feromagnetic>().enabled = true;
            cube.GetComponent<Rigidbody>().useGravity = true;
        });

        //Draw Rays in Build 
        Vector3 origin = golem.position;

        Vector3 rightDir = Quaternion.AngleAxis(angle, Vector3.up) * golem.forward;
        Vector3 leftDir = Quaternion.AngleAxis(-angle, Vector3.up) * golem.forward;

        animator.SetBool("IsPulling", true);
        visionConeObject.SetActive(true);
        visionConeObject.transform.position = player.position + new Vector3(0f, -0.5f, 0f);
        visionConeObject.transform.rotation = Quaternion.LookRotation(player.forward);
        GenerateMesh(maxAngle);

        /*rightRay.SetPosition(0, origin);
        rightRay.SetPosition(1, origin + rightDir * distance);

        leftRay.SetPosition(0, origin);
        leftRay.SetPosition(1, origin + leftDir * distance);*/

        magneticLast = magnetic;
    }

    public void resetMagneticLast()
    {
        magneticLast.ForEach(cube =>
        {
            cube.GetComponent<Feromagnetic>().enabled = true;
            cube.GetComponent<Rigidbody>().useGravity = true;
        });
        magneticLast.Clear();
    }


    public void coneProjection()
    {
        if (blocsToEject.Count > 0) { feedback.RepulsionVibrationShoot(blocsToEject.Count); }

        for (int i = 0; i < blocsToEject.Count && i < nbBlocsSelect; i++)
        {
            EjectBloc(blocsToEject[i], golem);
        }
    }
    public void coneReset(){
        for (int i = 0; i < blocsToEject.Count && i < nbBlocsSelect; i++)
        {
            resetBloc(blocsToEject[i], golem);
        }
    }
    public void ConeProjectionSelection(float time)
    {
        potentialBlocs.Clear();
        LayerMask mask = 1 << playerGrid.kernel.layer;
        float timel = Mathf.Min(1, (time / secondsForMaxChargingEjectionLength));
        nbBlocsSelect = Math.Max((int)(timel * maxBlocs), 1);
        
        foreach(var v in playerGrid.grid){
             if (v.Value.GetComponent<BoxCollider>() == null)
            {
                continue;
            }
            if (v.Value.gameObject == mainCubeRb.gameObject)
            {
                continue;
            }
            if (v.Value.transform.root != transform)
            {
                continue;
            }
            if (blocsToEject.Contains(v.Value))
            {
                continue;
            }
            if(v.Value.tag == "magneticCube"){
                continue;
            }
            //Look if the cube is within the angle of ejection
       
           
            potentialBlocs.Add(v.Value);
            
        }

       
        potentialBlocs.Sort((x, y) =>
        {
        Vector3 planeProjectionX = Vector3.ProjectOnPlane(x.transform.position, Vector3.up);
        Vector3 planeProjectionY = Vector3.ProjectOnPlane(y.transform.position, Vector3.up);
        Vector3 golemProjection = Vector3.ProjectOnPlane(golem.position, Vector3.up);
        float angleX = Vector3.Angle(planeProjectionX - golemProjection, golem.forward);
        float  angleY = Vector3.Angle(planeProjectionY - golemProjection, golem.forward);

        Vector3 positionX = new Vector3(x.transform.position.x, 0, x.transform.position.z);
        Vector3 positionY = new Vector3(y.transform.position.x, 0, y.transform.position.z);
        float distanceX = (x.transform.position - golem.position).sqrMagnitude;
        float distanceY = (y.transform.position - golem.position).sqrMagnitude;
        if(angleX <=  angleRepulsion && angleY <= angleRepulsion || (angleX > angleRepulsion && angleY > angleRepulsion))
            return -Math.Sign(distanceX - distanceY);
        else if(angleX <= angleRepulsion){
            return -1;
        }else{
            return 1;
        }
        });

        if (blocsToEject.Count < nbBlocsSelect && potentialBlocs.Count>0)
        {
            feedback.RepulsionVibrationSelect();
            placeBolcAtPosition(potentialBlocs.First(),blocsToEject.Count);
            blocsToEject.Add(potentialBlocs.First());
            if(potentialBlocs.First().tag != "explosive")
                potentialBlocs.First().gameObject.GetComponent<Bloc>().changeMeshMaterialColor(chargedColor);
        }
        
    }
    public void placeBolcAtPosition(GameObject bloc,int number)
    {
        if(playerGrid.kernel.layer == LayerMask.NameToLayer("magneticPlayer1")){
             bloc.layer = LayerMask.NameToLayer("ejection1");
        }else{
            bloc.layer = LayerMask.NameToLayer("ejection2");
        }
        playerGrid.DetachBlocSingleProjection(bloc);
        playerGrid.ejectRest(0);
        bloc.GetComponent<Bloc>().state = BlocState.projectileAnimation;
        // travel to position
        StartCoroutine(displaceBloc(bloc,number));
    }

    private  IEnumerator displaceBloc( GameObject bloc ,int number)
    {
        int sign;

        if (number % 2 == 0)
        {
            sign = -1;
        }
        else
        {
            sign = 1;
        }
        int right = Mathf.CeilToInt(number / 2f);
        Vector3 destination = golem.position + golem.forward * 2 * (1.2f-right*blocDiffY) + golem.right * 1.4f * right * sign + golem.up * 1.2f*blocHeight;
        Rigidbody blocRb = bloc.GetComponent<Rigidbody>();
        blocRb.useGravity = false;

        Vector3 initialPosition = bloc.transform.position;
        float time = 0;
        time += Time.deltaTime;
        float t = time / displaceTimeBloc;
        Vector3 position = Vector3.Lerp(initialPosition, destination, t);
        blocRb.MovePosition(position);

        while (!readyToEject)
        {
            yield return new WaitForSeconds(Time.deltaTime);
            time += Time.deltaTime;
            t = time / displaceTimeBloc;
            destination = golem.position + golem.forward * 2 * (1.2f-right*blocDiffY) + golem.right * 1.4f * right * sign + golem.up * 1.2f*blocHeight;
            position = Vector3.Lerp(initialPosition, destination, t);
            blocRb.MovePosition(position);
        }
         blocRb.position = new Vector3(bloc.transform.position.x,0.6f,bloc.transform.position.z);
         blocRb.velocity = new Vector3(blocRb.velocity.x,0,blocRb.velocity.z).normalized*blocRb.velocity.magnitude;
        blocsToEject.Remove(bloc);

    }
    private void EjectBloc(GameObject cube, Transform golem)
    {
        readyToEject = true;
        float rightDrift = golem.InverseTransformPoint(cube.transform.position).x;
        Rigidbody cubeRb = cube.GetComponent<Rigidbody>();
        Bloc cubeBloc = cube.GetComponent<Bloc>();
        cubeBloc.changeMeshMaterialColor(playerGrid.playerMat.color);
        cubeRb.interpolation = RigidbodyInterpolation.Interpolate;
        animator.SetTrigger("IsEjecting");
        print("IsEjecting");
        cubeRb.AddForce((golem.forward + golem.right * rightDrift * rightDriftProportion) * ejectionSpeed, ForceMode.VelocityChange);
        cubeBloc.state = BlocState.projectile;
        cubeBloc.ownerTranform.GetComponent<PlayerObjects>().finishEjection(cube);
    }
    private void resetBloc(GameObject cube, Transform golem)
    {
        readyToEject = true;
        Bloc cubeBloc = cube.GetComponent<Bloc>();
        cubeBloc.changeMeshMaterialColor(playerGrid.playerMat.color);
        cubeBloc.ownerTranform.GetComponent<PlayerObjects>().resetEjection(cube);
    }


    public float MaxDistanceForDirection(Vector3 direction, float radius)
    {

        Vector3 golemProjection = new Vector3(golem.position.x, 0, golem.position.z);
        LayerMask mask = LayerMask.GetMask("magnetic");
        int nbHits = Physics.BoxCastNonAlloc(mainCubeRb.position, new Vector3(0.01f, 20f, 0.01f), direction, hitsArray, mainCubeRb.rotation, radius, mask, QueryTriggerInteraction.Ignore);

        for (int i = nbHits - 1; i >= 0; i--)
        {
            Collider hit = hitsArray[i].collider;
            if (hitsArray[i].collider.transform.root == transform)
            {
                return Vector3.Distance(golemProjection, new Vector3(hit.transform.position.x, 0, hit.transform.position.z));
            }
        }
        return 0;
    }

    void GenerateMesh(float maxAngle)
    {
        Mesh mesh = new Mesh();
        coneMeshFilter.mesh = mesh;

        Vector3[] vertices = new Vector3[resolution + 2];
        int[] triangles = new int[resolution * 3];

        vertices[0] = Vector3.zero;

        float step = maxAngle * 2f / resolution;

        for (int i = 0; i <= resolution; i++)
        {
            float currentAngle = -maxAngle + step * i;
            float rad = currentAngle * Mathf.Deg2Rad;
            Vector3 dir = new Vector3(Mathf.Sin(rad), 0f, Mathf.Cos(rad));
            vertices[i + 1] = dir * distance;
        }

        for (int i = 0; i < resolution; i++)
        {
            triangles[i * 3] = 0;
            triangles[i * 3 + 1] = i + 1;
            triangles[i * 3 + 2] = i + 2;
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
    }
}

