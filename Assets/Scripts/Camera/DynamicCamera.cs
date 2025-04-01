using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.ProBuilder;

public class DynamicCamera : MonoBehaviour
{
    [Header("Cameras")]
    [SerializeField] private Camera cam;
    [SerializeField] private Camera camUI;

    [Header("References")]
    [SerializeField] private GameObject Player1;
    [SerializeField] private GameObject Player2;
    [SerializeField] private GameObject ArenaCenter;

    [Header("Parameters")]
    [SerializeField] private float maxSpeed = Mathf.Infinity;
    [SerializeField] private float horizontalInterpTime = .5f;
    [SerializeField] private float distanceFromPlayersFactor = 2f;
    [SerializeField] private float minDistance;
    [SerializeField] private float maxDistance;
    [SerializeField] private float distanceInterpTime = .5f;
    [SerializeField] private bool isOrthographic = true;

    [Header("Animation Settings")]
    [SerializeField] private bool playIntroAnimation=true;

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

    private Animator animator;
    private PlayerInput playerOneInputs;
    private PlayerInput playerTwoInputs;
    private bool shouldFollowPlayers;

    private void Start()
    {
    QualitySettings.vSyncCount = 1;
	Application.targetFrameRate = 120;
        animator = GetComponent<Animator>();

        playerOneInputs=Player1.GetComponentInParent<PlayerInput>();
        playerTwoInputs=Player2.GetComponentInParent<PlayerInput>();

        if(!playIntroAnimation)
        {
            IntroFinished();
        }
        else
        {
            playerOneInputs.DeactivateInput();
            playerTwoInputs.DeactivateInput();
            camUI.enabled = false;
        }

        //R�cup�re l'angle de la cam�ra par rapport � son pivot
        angleCam =new Vector2(cam.transform.localPosition.z, cam.transform.localPosition.y).normalized;

        if (isOrthographic)
        {
            cam.orthographicSize=maxDistance;
        }
    }

    void Update()
    {
        if (shouldFollowPlayers)
        {
            // POSTION DE L'OBJET DYNAMIC CAMERA

            //R�cup�re la position des joueurs sur un plan XZ pour que l'objet cam�ra reste fixe sur l'axe Y
            playerOnePlanePos = new Vector3(Player1.transform.position.x, 0, Player1.transform.position.z);
            playerTwoPlanePos = new Vector3(Player2.transform.position.x, 0, Player2.transform.position.z);
            arenaCenterPlanePos = new Vector3(ArenaCenter.transform.position.x, 0, ArenaCenter.transform.position.z);

            transform.position = Vector3.SmoothDamp(transform.position, (playerOnePlanePos + playerTwoPlanePos + arenaCenterPlanePos) / 3, ref currentHorizontalVelocity, horizontalInterpTime, maxSpeed);

            // DISTANCE DE LA CAMERA PAR RAPPORT AU PIVOT DE L'OBJET DYNAMIC CAMERA

            distanceBetweenPlayers = (playerOnePlanePos - playerTwoPlanePos).magnitude;

            if (!isOrthographic)
            {
                camLocalPos = angleCam * Mathf.Clamp(distanceBetweenPlayers * distanceFromPlayersFactor, minDistance, maxDistance);
                cam.transform.localPosition = Vector3.SmoothDamp(cam.transform.localPosition, new Vector3(0, camLocalPos.y, camLocalPos.x), ref currentDistanceVelocity, distanceInterpTime, maxSpeed);
            }
            else
            {
                camOrthoSize = Mathf.Clamp(distanceBetweenPlayers * distanceFromPlayersFactor, minDistance, maxDistance);
                cam.orthographicSize = Mathf.SmoothDamp(cam.orthographicSize, camOrthoSize, ref currentOrthoSizeVelocity, distanceInterpTime, maxSpeed);
                camUI.orthographicSize = cam.orthographicSize;
            }
        }
    }

    public void IntroFinished()
    {
        //Debug.Log("Intro anim finished");

        animator.enabled = false;
        playerOneInputs.ActivateInput();
        playerTwoInputs.ActivateInput();
        camUI.enabled = true;

        shouldFollowPlayers = true;
    }
}
