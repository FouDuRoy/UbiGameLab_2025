using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.ProBuilder.MeshOperations;
using UnityEngine.UIElements;


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
    [SerializeField] float secondsForMaxChargingEjectionLength = 10f;
    [SerializeField] float maxNumberBlocsEjection = 20f;
    [SerializeField] float ejectionSpeed = 3f;
    [SerializeField] float colorChangeIntensity = 3f;
    int nbBlocsSelect;
    List<Collider> magneticLast = new List<Collider>();


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
    void Start()
    {

    }
    void Awake()

    {
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
        float rightTrigger = ejectCubes.ReadValue<float>();
        float leftTrigger = AttractCubes.ReadValue<float>();
        if ((leftTrigger > 0 && rightTrigger == 0))
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
        if ((rightTrigger > 0))
        {
            leftRay.gameObject.SetActive(true);
            rightRay.gameObject.SetActive(true);
            coneProjectionColor(timeHeld);
            if (timeHeld == 0) //On appelle VibrationStart une seule fois, au d�but
            {
                feedback.RepulsionVibrationStart(secondsForMaxCharging);
            }
            timeHeld += Time.deltaTime;
            rightTriggerHeld = true;

            //Draw rays to indicate current range
            float timeHeldAngle = Mathf.Clamp(timeHeld, 0, secondsForMaxChargingEjection);
            float maxAngle = initialAngle + (maxAngleRepulsion - initialAngle) * (timeHeldAngle / secondsForMaxChargingEjection);

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
            //Debug.DrawRay(golem.position, Quaternion.AngleAxis(-maxAngle, Vector3.up) * golem.forward *distance, Color.red, Time.deltaTime);
        }
        else if (rightTriggerHeld)
        {
            feedback.RepulsionVibrationEnd(timeHeld,true);
            coneProjection();
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
        leftRay.gameObject.SetActive(true);
        rightRay.gameObject.SetActive(true);
        LayerMask mask = LayerMask.GetMask("magnetic");
        // Find all magneticblocs in radiusZone
        float sphereRadius = distance;
        List<Collider> magnetic = Physics.OverlapSphere(player.position, sphereRadius).ToList<Collider>();

        //Look for those that are in front of the player with maximum angle
        magnetic = magnetic.FindAll(cube =>
        {
            //If the cube is used by a player dont pull
            if (cube.transform.root.GetComponent<PlayerObjects>() != null || cube.transform.tag != "magnetic" || cube.GetComponent<Bloc>().owner != "Neutral")
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


        visionConeObject.SetActive(true);
        visionConeObject.transform.position = player.position + new Vector3(0f, -0.5f, 0f);
        visionConeObject.transform.rotation = Quaternion.LookRotation(player.forward);
        GenerateMesh(maxAngle);

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
        magneticLast.ForEach(cube =>
        {
            cube.GetComponent<Feromagnetic>().enabled = true;
            cube.GetComponent<Rigidbody>().useGravity = true;
        });
        magneticLast.Clear();
    }

 
    public void coneProjection()
    {

        foreach (var v in playerGrid.grid)
        {
            if (v.Value != playerGrid.kernel)
                v.Value.GetComponent<Bloc>().changeMeshMaterialColor(playerGrid.playerMat.color);
        }
        for (int i = 0; i < blocsToEject.Count && i < nbBlocsSelect; i++)
        {
            EjectBloc(blocsToEject[i], golem);
        }
        playerGrid.coneEjectRest(ejectionSpeed, rightDriftProportion);

    }
    public void coneProjectionColor(float time)
    {
        blocsToEject.Clear();
        int maxX = playerGrid.grid.Keys.Max(x => x.x);
        int minX = playerGrid.grid.Keys.Min(x => x.x);
        int maxZ = playerGrid.grid.Keys.Max(x => x.z);
        int minZ = playerGrid.grid.Keys.Min(x => x.z);
        int maxY = playerGrid.grid.Keys.Max(x => x.y);
        int minY = playerGrid.grid.Keys.Min(x => x.y);

        int radiusInBlocs = Mathf.Max(Mathf.Abs(maxX), Mathf.Abs(minX), Mathf.Abs(maxZ), Mathf.Abs(minZ), Mathf.Abs(maxY), Mathf.Abs(minY));
        float blocSizeWorld = playerGrid.cubeSize * playerGrid.kernel.transform.lossyScale.x;
        float radius = radiusInBlocs * blocSizeWorld * 1.2f;
        nbBlocsSelect = Math.Max((int) ((time / secondsForMaxChargingEjectionLength) * (maxNumberBlocsEjection)),1);
        float timeHeldAngle = Mathf.Clamp(timeHeld, 0, secondsForMaxChargingEjection);
        float timeHeldLength = Mathf.Clamp(timeHeld, 0, secondsForMaxChargingEjectionLength);
        float boundaryDistanceRatio = timeHeldLength / secondsForMaxChargingEjectionLength;
        float angleRatio = timeHeldAngle / secondsForMaxChargingEjection;
        float maxAngle = initialAngle + (maxAngleRepulsion - initialAngle) * (angleRatio);
        LayerMask mask = LayerMask.GetMask("magnetic");
        int nbHits = Physics.OverlapSphereNonAlloc(golem.position, radius, magnetic, mask,QueryTriggerInteraction.Ignore);
        for (int i = 0; i < nbHits; i++)
        {

            //look if the cube is within the neighberhood of the boundary

            if (magnetic[i].GetComponent<BoxCollider>() == null)
            {
                magnetic[i] = null;
                continue;
            }
            if (magnetic[i].gameObject == mainCubeRb.gameObject)
            {
                magnetic[i] = null;
                continue;
            }
            if (magnetic[i].transform.root != transform)
            {
                magnetic[i] = null;
                continue;
            }
           
            magnetic[i].gameObject.GetComponent<Bloc>().changeMeshMaterialColor(playerGrid.playerMat.color);
            //Look if the cube is within the angle of ejection
            Vector3 planeProjection = Vector3.ProjectOnPlane(magnetic[i].transform.position, Vector3.up);
            Vector3 golemProjection = Vector3.ProjectOnPlane(golem.position, Vector3.up);
            float angle = Vector3.Angle(planeProjection - golemProjection, golem.forward);
            if (angle > maxAngle)
            {
                magnetic[i] = null;
                continue;
            }
            if (magnetic[i] != null)
            {
                blocsToEject.Add(magnetic[i].gameObject);
            }
        }
        blocsToEject.Sort((x, y) =>
        {
            Vector3 positionX = new Vector3(x.transform.position.x, 0, x.transform.position.z);
            Vector3 positionY = new Vector3(y.transform.position.x, 0, y.transform.position.z);
            float distanceX = (positionX - new Vector3(golem.position.x, 0, golem.position.z)).sqrMagnitude;
            float distanceY = (positionY - new Vector3(golem.position.x, 0, golem.position.z)).sqrMagnitude;
            return -Math.Sign(distanceX - distanceY);
        });
        for (int i = 0; i < blocsToEject.Count && i < nbBlocsSelect; i++)
        {
            blocsToEject[i].gameObject.GetComponent<Bloc>().changeMeshMaterialColor(chargedColor);
        }
    }
    private void EjectBloc(GameObject cube, Transform golem)
    {

        cube.transform.parent = this.transform.parent;
        playerGrid.DetachBlocSingle(cube);

        //Add rigidBody

        float rightDrift = golem.InverseTransformPoint(cube.transform.position).x;
        cube.GetComponent<Rigidbody>().interpolation = RigidbodyInterpolation.Interpolate;
        cube.GetComponent<Rigidbody>().AddForce((golem.forward + golem.right * rightDrift * rightDriftProportion) * ejectionSpeed, ForceMode.VelocityChange);
        cube.GetComponent<Bloc>().state = BlocState.projectile;

        //Remove owner of cube
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

