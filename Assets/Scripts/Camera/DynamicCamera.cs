using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class DynamicCamera : MonoBehaviour
{
    [Header("Cameras")]
    [SerializeField] private Camera mainCam;
    [SerializeField] private Camera mainCamUI;
    [SerializeField] private Camera cam1;
    [SerializeField] private Camera cam2;

    [Header("References")]
    [SerializeField] public GameObject Player1;
    [SerializeField] public GameObject Player2;
    [SerializeField] private GameObject ArenaCenter;
    [SerializeField] private Transform PlayersCenter;

    [Header("Parameters")]
    [Header("Global")]
    [SerializeField] private bool isOrthographic = true;
    [SerializeField] private bool simpleCamera = false;
    [SerializeField] private float maxSpeed = Mathf.Infinity;
    [SerializeField] private float horizontalInterpTime = .1f;
    [SerializeField] private float distanceInterpTime = .6f;
    [SerializeField] private float rotationInterpTime = .5f;
    [SerializeField] private float maxRotSpeed = 50f;
    [SerializeField] private float minCamDistanceForRot = 12.1f;
    [SerializeField] private float rotationSwitchThreshold = 5f;
    [Header("Main Cam")]
    [SerializeField] private float distanceFromPlayersFactor = .7f;
    [SerializeField] private float minDistance = 12f;
    [SerializeField] private float maxDistance = 24f;
    [Header("Cam 1")]
    [SerializeField] private float distanceFromPlayersFactor1 = .4f;
    [SerializeField] private float minDistance1 = 8f;
    [SerializeField] private float maxDistance1 = 24f;
    [Header("Cam 2")]
    [SerializeField] private float distanceFromPlayersFactor2 = .7f;
    [SerializeField] private float minDistance2;
    [SerializeField] private float maxDistance2;

    [Header("Animation Settings")]
    [SerializeField] private bool playIntroAnimation = true;

    private Vector3 currentHorizontalVelocity = Vector3.zero;
    private Vector3 currentDistanceVelocity = Vector3.zero;
    private Vector3 currentRotVelocity = Vector3.zero;
    private Vector3 currentPlayersCenterVelocity = Vector3.zero;
    private float currentOrthoSizeVelocity;
    private Vector3 playerOnePlanePos;
    private Vector3 playerTwoPlanePos;
    private Vector3 arenaCenterPlanePos;
    private Vector3 cameraRot;
    private Vector2 angleCam;
    private Vector2 camLocalPos;
    private float camOrthoSize;
    private float distanceBetweenPlayers = 0;
    private Vector3 targetEuler;

    private Animator animator;
    private Animator animatorPlayer1;
    private Animator animatorPlayer2;
    private PlayerInput playerOneInputs;
    private PlayerInput playerTwoInputs;
    private bool shouldFollowPlayers;
    private bool setAnimator = true;
    private int chosenRotation = 1;

    private void Start()
    {
        // QualitySettings.vSyncCount = 1;
        //Application.targetFrameRate = 120;


        mainCamUI.enabled = false;


        //R�cup�re l'angle de la cam�ra par rapport � son pivot
        angleCam = new Vector2(mainCam.transform.localPosition.z, mainCam.transform.localPosition.y).normalized;

        if (isOrthographic)
        {
            mainCam.orthographicSize = maxDistance;
        }

        //PlayersCenter.parent = null;
        cam1.enabled = false;
        cam2.enabled = false;
    }

    private void LateUpdate()
    {
        if (setAnimator && Player1 != null)
        {
            animator = GetComponent<Animator>();
            animatorPlayer1 = Player1.GetComponentInParent<PlayerInfo>().GetComponentInChildren<Animator>();
            animatorPlayer2 = Player2.GetComponentInParent<PlayerInfo>().GetComponentInChildren<Animator>();
            playerOneInputs = Player1.GetComponentInParent<PlayerInput>();
            playerTwoInputs = Player2.GetComponentInParent<PlayerInput>();
            playerOneInputs.DeactivateInput();
            playerTwoInputs.DeactivateInput();
            setAnimator = false;
        }


        if (!setAnimator && !playIntroAnimation)
        {
            IntroFinished();
        }
        if (shouldFollowPlayers)
        {
            // POSTION DES OBJETS DYNAMIC CAMERA & PLAYERS CENTER

            //R�cup�re la position des joueurs sur un plan XZ pour que l'objet cam�ra reste fixe sur l'axe Y
            playerOnePlanePos = new Vector3(Player1.transform.position.x, 0, Player1.transform.position.z);
            playerTwoPlanePos = new Vector3(Player2.transform.position.x, 0, Player2.transform.position.z);
            arenaCenterPlanePos = new Vector3(ArenaCenter.transform.position.x, 0, ArenaCenter.transform.position.z);

            transform.position = Vector3.SmoothDamp(transform.position, (playerOnePlanePos + playerTwoPlanePos + arenaCenterPlanePos) / 3, ref currentHorizontalVelocity, horizontalInterpTime, maxSpeed);
            PlayersCenter.position = Vector3.SmoothDamp(PlayersCenter.position, (playerOnePlanePos + playerTwoPlanePos) / 2, ref currentPlayersCenterVelocity, horizontalInterpTime, maxSpeed);

            // DISTANCE DE LA CAMERA PAR RAPPORT AU PIVOT DE L'OBJET DYNAMIC CAMERA

            distanceBetweenPlayers = (playerOnePlanePos - playerTwoPlanePos).magnitude;

            if (!isOrthographic)
            {
                camLocalPos = angleCam * Mathf.Clamp(distanceBetweenPlayers * distanceFromPlayersFactor, minDistance, maxDistance);
                mainCam.transform.localPosition = Vector3.SmoothDamp(mainCam.transform.localPosition, new Vector3(0, camLocalPos.y, camLocalPos.x), ref currentDistanceVelocity, distanceInterpTime, maxSpeed);
            }
            else
            {
                // Main cam
                if (mainCam.isActiveAndEnabled)
                {
                    camOrthoSize = Mathf.Clamp(distanceBetweenPlayers * distanceFromPlayersFactor, minDistance, maxDistance);
                    mainCam.orthographicSize = Mathf.SmoothDamp(mainCam.orthographicSize, camOrthoSize, ref currentOrthoSizeVelocity, distanceInterpTime, maxSpeed);
                    mainCamUI.orthographicSize = mainCam.orthographicSize;
                }

                // Cam 1
                if (cam1.isActiveAndEnabled)
                {
                    camOrthoSize = Mathf.Clamp(distanceBetweenPlayers * distanceFromPlayersFactor1, minDistance1, maxDistance1);
                    cam1.orthographicSize = Mathf.SmoothDamp(cam1.orthographicSize, camOrthoSize, ref currentOrthoSizeVelocity, distanceInterpTime, maxSpeed);
                }

                // Cam 2
                if (cam2.isActiveAndEnabled)
                {
                    camOrthoSize = Mathf.Clamp(distanceBetweenPlayers * distanceFromPlayersFactor2, minDistance2, maxDistance2);
                    cam2.orthographicSize = Mathf.SmoothDamp(cam2.orthographicSize, camOrthoSize, ref currentOrthoSizeVelocity, distanceInterpTime, maxSpeed);
                }
            }

            if (!simpleCamera && mainCam.orthographicSize >= minCamDistanceForRot)
            {
                // ROTATION

                // Choix de la rotation cible en fonction de la distance à parcourir pour l'atteindre
                Quaternion potentialRot1 = Quaternion.LookRotation(playerOnePlanePos - playerTwoPlanePos) * Quaternion.Euler(0, 90, 0);
                Quaternion potentialRot2 = Quaternion.LookRotation(playerTwoPlanePos - playerOnePlanePos) * Quaternion.Euler(0, 90, 0);

                float angleTo1 = Quaternion.Angle(transform.rotation, potentialRot1);
                float angleTo2 = Quaternion.Angle(transform.rotation, potentialRot2);

                if (Mathf.Abs(angleTo1 - angleTo2) > rotationSwitchThreshold)
                {
                    if (angleTo1 < angleTo2 && chosenRotation != 1)
                    {
                        chosenRotation = 1;
                        print("1");
                    }
                    else if (angleTo2 < angleTo1 && chosenRotation != 2)
                    {
                        chosenRotation = 2;
                        print("2");
                    }
                }

                if (chosenRotation == 1)
                {
                    targetEuler = potentialRot1.eulerAngles;
                }
                else
                {
                    targetEuler = potentialRot2.eulerAngles;
                }

                // Interpolation de la rotation
                Vector3 currentEuler = transform.rotation.eulerAngles;
                Vector3 smoothedEuler = new Vector3(
                    0,
                    Mathf.SmoothDampAngle(currentEuler.y, targetEuler.y, ref currentRotVelocity.y, rotationInterpTime, maxRotSpeed),
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
        mainCamUI.enabled = true;

        shouldFollowPlayers = true;
    }

    public void PlayVictoryAnimation(string winnerName)
    {
        GameObject winner;

        if (Player1.name == winnerName)
        {
            winner = Player1;
            animatorPlayer1.SetTrigger("PlayWinning");
        }
        else
        {
            winner = Player2;
            animatorPlayer2.SetTrigger("PlayWinning");
        }

        shouldFollowPlayers = false;

        StartCoroutine(SmoothTransitionToPodium(1.5f, new Vector3(1, 1.2f, -3f), -5, .5f, .2f, winner));
    }

    private IEnumerator SmoothTransitionToPodium(float desiredOrthoSize, Vector3 desiredPosition, float localXRot, float transitionTime, float teleportTime, GameObject winner)
    {
        float elapsedTime = 0f;
        Vector3 startPosition = mainCam.transform.position;
        float startOrthoSize = mainCam.orthographicSize;
        Vector3 startLocalPosition = mainCam.transform.localPosition;
        float startXRot = mainCam.transform.localEulerAngles.x;

        bool functionCalled = false;

        while (elapsedTime < transitionTime)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / transitionTime;
            t = Mathf.SmoothStep(0f, 1f, t);

            // Interpolation de la position de la caméra
            mainCam.transform.position = Vector3.Lerp(startPosition, desiredPosition, t);

            // Interpolation de la taille orthographique
            mainCam.orthographicSize = Mathf.Lerp(startOrthoSize, desiredOrthoSize, t);
            mainCamUI.orthographicSize = mainCam.orthographicSize;

            // Rotation de la caméra
            mainCam.transform.localEulerAngles = new Vector3(Mathf.Lerp(startXRot, localXRot, t), mainCam.transform.localEulerAngles.y, mainCam.transform.localEulerAngles.z);

            if (elapsedTime >= teleportTime && !functionCalled)
            {
                functionCalled = true;

                ClearingSphere(20, new string[] { "wood", "magnetic", "explosive" });

                winner.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
                winner.GetComponent<Rigidbody>().isKinematic = true;

                winner.transform.position = new Vector3(0, 0.6f, 0);
                winner.transform.eulerAngles = new Vector3(0, 170, 0);
                Physics.SyncTransforms();

                mainCamUI.enabled = false;
            }

            yield return null;
        }

        // S'assurer que les valeurs finales sont bien atteintes
        mainCam.transform.position = desiredPosition;
        mainCam.orthographicSize = desiredOrthoSize;
        mainCamUI.orthographicSize = desiredOrthoSize;
        mainCam.transform.localEulerAngles = new Vector3(localXRot, mainCam.transform.localEulerAngles.y, mainCam.transform.localEulerAngles.z);
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

    public void PlayerIntroPose(int nPlayer)
    {
        if (nPlayer == 1)
        {
            animatorPlayer1.SetTrigger("PlayIntro");
        }
        else if (nPlayer == 2)
        {
            animatorPlayer2.SetTrigger("PlayIntro");
        }
    }

    public void LoopBetweenCameras(float duration)
    {
        if (!simpleCamera)
        {
            mainCam.orthographicSize = Mathf.Clamp(distanceBetweenPlayers * distanceFromPlayersFactor1, minDistance1, maxDistance1);
            transform.eulerAngles = transform.eulerAngles + new Vector3(0, Random.Range(-60, 60), 0);
            transform.position = (playerOnePlanePos + playerTwoPlanePos) / 2;
        }
    }

    private IEnumerator CamerasChangePattern(float duration)
    {
        if (duration < 2)
        {

            mainCam.enabled = false;
            cam1.enabled = true;
            cam2.enabled = false;
            print("cam1");

            yield return new WaitForSeconds(duration);

            mainCam.enabled = true;
            cam1.enabled = false;
            cam2.enabled = false;
            print("main");
        }
        else
        {
            mainCam.enabled = false;
            cam1.enabled = true;
            cam2.enabled = false;
            print("cam1");

            yield return new WaitForSeconds(duration / 2);

            mainCam.enabled = false;
            cam1.enabled = false;
            cam2.enabled = true;
            print("cam2");

            yield return new WaitForSeconds(duration);

            mainCam.enabled = true;
            cam1.enabled = false;
            cam2.enabled = false;
            print("main");
        }
    }
}
