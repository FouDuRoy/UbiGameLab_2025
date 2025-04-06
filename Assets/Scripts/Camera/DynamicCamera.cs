using Cinemachine.Utility;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Animations;
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
    [SerializeField] private bool isOrthographic = true;
    [SerializeField] private float maxSpeed = Mathf.Infinity;
    [SerializeField] private float distanceFromPlayersFactor = 2f;
    [SerializeField] private float minDistance;
    [SerializeField] private float maxDistance;
    [SerializeField] private float distanceInterpTime = .5f;
    [SerializeField] private float horizontalInterpTime = .5f;
    [SerializeField] private bool simpleCamera = false;
    [SerializeField] private float maxRotSpeed = Mathf.Infinity;
    [SerializeField] private float minCamDistanceForRot = 13f;
    [SerializeField] private float rotationInterpTime = .5f;
    

    [Header("Animation Settings")]
    [SerializeField] private bool playIntroAnimation=true;

    private Vector3 currentHorizontalVelocity = Vector3.zero;
    private Vector3 currentDistanceVelocity = Vector3.zero;
    private Vector3 currentRotVelocity= Vector3.zero;
    private float currentOrthoSizeVelocity;
    private Vector3 playerOnePlanePos;
    private Vector3 playerTwoPlanePos;
    private Vector3 arenaCenterPlanePos;
    private Vector3 cameraRot;
    private Vector2 angleCam;
    private Vector2 camLocalPos;
    private float camOrthoSize;
    private float distanceBetweenPlayers=0;
    private Vector3 targetEuler;

    private Animator animator;
    private PlayerInput playerOneInputs;
    private PlayerInput playerTwoInputs;
    private bool shouldFollowPlayers;

    private void Start()
    {
   // QualitySettings.vSyncCount = 1;
	//Application.targetFrameRate = 120;
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

    void LateUpdate()
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

            if (!simpleCamera && cam.orthographicSize>=minCamDistanceForRot)
            {
                // ROTATION

                // Rotation cible
                targetEuler = Quaternion.LookRotation(playerOnePlanePos-playerTwoPlanePos).eulerAngles+new Vector3(0,90,0);
                //targetEuler.y=Mathf.Clamp(targetEuler.y,baseCamRot-maxRotAngle,baseCamRot+maxRotAngle);
                

                // Interpolation de la rotation
                Vector3 currentEuler = transform.rotation.eulerAngles;
                Vector3 smoothedEuler = new Vector3(
                    0,
                    Mathf.SmoothDampAngle(currentEuler.y, targetEuler.y, ref currentRotVelocity.y, rotationInterpTime,maxRotSpeed),
                    0
                );

                transform.rotation = Quaternion.Euler(smoothedEuler);
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

    public void PlayVictoryAnimation(string winnerName)
    {
        GameObject winner;

        if (Player1.name == winnerName)
        {
            winner = Player1;
        }
        else
        {
            winner = Player2;
        }

        shouldFollowPlayers = false;
        
        StartCoroutine(SmoothTransitionToPodium(1.5f, new Vector3(1, 1.2f, -3f), -5, .5f, .2f, winner));
    }

    private IEnumerator SmoothTransitionToPodium(float desiredOrthoSize, Vector3 desiredPosition, float localXRot, float transitionTime, float teleportTime, GameObject winner)
    {
        float elapsedTime = 0f;
        Vector3 startPosition = cam.transform.position;
        float startOrthoSize = cam.orthographicSize;
        Vector3 startLocalPosition = cam.transform.localPosition;
        float startXRot = cam.transform.localEulerAngles.x;

        bool functionCalled = false;

        while (elapsedTime < transitionTime)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / transitionTime;
            t = Mathf.SmoothStep(0f, 1f, t);

            // Interpolation de la position de la caméra
            cam.transform.position = Vector3.Lerp(startPosition, desiredPosition, t);

            // Interpolation de la taille orthographique
            cam.orthographicSize = Mathf.Lerp(startOrthoSize, desiredOrthoSize, t);
            camUI.orthographicSize=cam.orthographicSize;

            // Rotation de la caméra
            cam.transform.localEulerAngles = new Vector3(Mathf.Lerp(startXRot, localXRot, t), cam.transform.localEulerAngles.y, cam.transform.localEulerAngles.z);

            if(elapsedTime >= teleportTime && !functionCalled)
            {
                functionCalled = true;
                
                ClearingSphere(20, new string[] { "wood", "magnetic", "explosive" });

                winner.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
                winner.GetComponent<Rigidbody>().isKinematic = true;

                winner.transform.position = new Vector3(0, 0.6f, 0);
                winner.transform.eulerAngles = new Vector3(0, 170, 0);
                Physics.SyncTransforms();

                camUI.enabled = false;
            }

            yield return null;
        }

        // S'assurer que les valeurs finales sont bien atteintes
        cam.transform.position = desiredPosition;
        cam.orthographicSize = desiredOrthoSize;
        camUI.orthographicSize = desiredOrthoSize;
        cam.transform.localEulerAngles = new Vector3(localXRot, cam.transform.localEulerAngles.y, cam.transform.localEulerAngles.z);
    }

    private void ClearingSphere(float radius, string[] tags)
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, radius);
        foreach (Collider col in colliders)
        {
            foreach (string tag in tags)
            {
                if (col.CompareTag(tag))
                {
                    col.gameObject.SetActive(false);
                }
            }
        }
    }
}
