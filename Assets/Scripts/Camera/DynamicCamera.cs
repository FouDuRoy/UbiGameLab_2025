using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.ProBuilder;

public class DynamicCamera : MonoBehaviour
{
    [SerializeField] private GameObject Player1;
    [SerializeField] private GameObject Player2;
    [SerializeField] private GameObject ArenaCenter;

    [SerializeField] private float maxSpeed = Mathf.Infinity;

    [SerializeField] private float horizontalInterpTime = .5f;

    [SerializeField] private float distanceFromPlayersFactor = 2f;
    [SerializeField] private float minDistance;
    [SerializeField] private float maxDistance;
    [SerializeField] private float distanceInterpTime = .5f;
    [SerializeField] private bool isOrthographic = true;

    private Vector3 currentHorizontalVelocity = Vector3.zero;
    private Vector3 currentDistanceVelocity = Vector3.zero;
    private float currentOrthoSizeVelocity;
    private Vector3 playerOnePlanePos;
    private Vector3 playerTwoPlanePos;
    private Vector3 arenaCenterPlanePos;
    private Vector2 angleCam;
    private Vector2 camLocalPos;
    private float camOrthoSize;
    private float distanceBetweenPlayers=0;

    private Camera cam;

    private void Start()
    {
        cam = GetComponentInChildren<Camera>();

        //Récupère l'angle de la caméra par rapport à son pivot
        angleCam =new Vector2(cam.transform.localPosition.z, cam.transform.localPosition.y).normalized;

        if (isOrthographic)
        {
            cam.orthographicSize=maxDistance;
        }
    }

    void Update()
    {
        // POSTION DE L'OBJET DYNAMIC CAMERA

        //Récupère la position des joueurs sur un plan XZ pour que l'objet caméra reste fixe sur l'axe Y
        playerOnePlanePos = new Vector3 (Player1.transform.position.x, 0, Player1.transform.position.z);
        playerTwoPlanePos = new Vector3 (Player2.transform.position.x, 0, Player2.transform.position.z);
        arenaCenterPlanePos = new Vector3(ArenaCenter.transform.position.x, 0, ArenaCenter.transform.position.z);

        transform.position= Vector3.SmoothDamp(transform.position, (playerOnePlanePos + playerTwoPlanePos + arenaCenterPlanePos) / 3, ref currentHorizontalVelocity, horizontalInterpTime, maxSpeed);

        // DISTANCE DE LA CAMERA PAR RAPPORT AU PIVOT DE L'OBJET DYNAMIC CAMERA

        distanceBetweenPlayers = (playerOnePlanePos - playerTwoPlanePos).magnitude;

        if (!isOrthographic)
        {
            camLocalPos = angleCam * Mathf.Clamp(distanceBetweenPlayers * distanceFromPlayersFactor, minDistance, maxDistance);
            cam.transform.localPosition = Vector3.SmoothDamp(cam.transform.localPosition, new Vector3(0, camLocalPos.y, camLocalPos.x), ref currentDistanceVelocity, distanceInterpTime, maxSpeed);
        }
        else
        {
            camOrthoSize= Mathf.Clamp(distanceBetweenPlayers * distanceFromPlayersFactor, minDistance, maxDistance);
            cam.orthographicSize = Mathf.SmoothDamp(cam.orthographicSize, camOrthoSize, ref currentOrthoSizeVelocity, distanceInterpTime,maxSpeed);
            //print(camOrthoSize);
        }
    }
}
