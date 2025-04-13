using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.Windows;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

//Federico Barallobres
public class PlayerMouvement : MonoBehaviour
{
    [SerializeField] float explosionForce = 30;
    [SerializeField] public float mouvementSpeed = 1f;
    public float deadZone = 0.9f;
    [SerializeField] float mouvementReductionFactor = 1f;
    [SerializeField] float pivotSpeed = 1f;
    [SerializeField] float rotationSpeed = 1f;
    [SerializeField] float rotParam;
    [SerializeField] float rotationDamping = 10f;
    [SerializeField] float weightMouvementFactor = 1f;
    [SerializeField] float weightRotationFactor = 1f;
    [SerializeField] public MouvementType moveType;
    [SerializeField] private Transform cameraOrientationReference;

    [SerializeField] float rotationSpeedPowerUp = 1000f;
    public GameObject pauseMenu; // Assign in Inspector
    public GameObject selectedGUI; // Assign in Inspector
    public GameObject inputUI;
    bool isPaused = false;

    PlayerInput playerInput;
    [SerializeField] GameObject other;
    InputAction moveAction;
    InputAction rotateAction;
    InputAction dashMove;
    InputAction pauseAction;

    InputAction ejectCubes;
    InputAction attractCubes;

    float weightRotation;
    float weightTranslation;
    float totalMass;
    float weight;
    float t = 0;
    Transform golem;
    Rigidbody rb;

    float leftTrigger;
    float rightTrigger;

    private GameObject reff;

    bool rotatingRight = false;
    HapticFeedbackController feedback;
    Dash dash;
    private bool shoulderLeftPressed = false;
    private bool shoulderRightPressed = false;
    GridSystem gridPlayer;
    private Animator animator;

    void Start()
    {
       
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        gridPlayer = GetComponent<GridSystem>();
        if (moveType != MouvementType.none)
        {
            weight = this.GetComponent<PlayerObjects>().weight;
            weightRotation = Mathf.Clamp(weight * weightRotationFactor, 1, 10 * weight);
            weightTranslation = Mathf.Clamp(weight * weightMouvementFactor, 1, 10 * weight);
            Vector3 direction2 = moveAction.ReadValue<Vector3>();
            
            // Adaptation de l'input direction à la rotation de la caméra
            Vector3 forward = cameraOrientationReference.forward;
            Vector3 right = cameraOrientationReference.right;
            forward.y = 0;
            right.y = 0;
            forward.Normalize();
            right.Normalize();

            Vector3 direction = direction2.x * right + direction2.y * forward;

            float rotationY = rotateAction.ReadValue<float>();

            leftTrigger = attractCubes.ReadValue<float>();
            rightTrigger = ejectCubes.ReadValue<float>();

            switch (moveType)
            {

               
                case MouvementType.Joystick4:
                    joystick4(direction);
                    break;
                case MouvementType.HyperVite:
                    HyperVite(direction);
                    break;
            }
        }


        // ThrowCubes();
    }

    private void OnEnable()
    {
        animator = GetComponentInChildren<Animator>();
        playerInput = GetComponent<PlayerInput>();
        moveAction = playerInput.currentActionMap.FindAction("Move");
        rotateAction = playerInput.currentActionMap.FindAction("Rotate");
        dashMove =playerInput.currentActionMap.FindAction("dash");
        ejectCubes = playerInput.actions.FindAction("BlocEjection");
        attractCubes =playerInput.currentActionMap.FindAction("AttractCubes");

       // moveAction.Enable();
       // rotateAction.Enable();
       // dashMove.Enable();
      //  ejectCubes.Enable();    
        //attractCubes.Enable();
        pauseMenu.SetActive(false); // Hide canvas at start
        pauseAction = playerInput.currentActionMap.FindAction("Pause");
        gridPlayer = GetComponent<GridSystem>();
        rb = GetComponent<PlayerObjects>().cubeRb;
        golem = GetComponent<PlayerObjects>().golem.transform;
        feedback = GetComponent<HapticFeedbackController>();
        dash = GetComponentInChildren<Dash>();
        pauseAction.performed += DoPause;
        dashMove.performed += _ =>
        {
            Ejection();
        };
    }

    private void Ejection()
    {
            if(dash!= null)
            {
                dash.TryToDash(rb, golem, this , animator);
            }

    }

    public void ThrowCubes()
    {
        foreach (var item in gridPlayer.grid)
        {
            GameObject cube = item.Value;
            if (cube != this.rb.gameObject)
            {

                cube.GetComponent<Rigidbody>().AddForce((cube.GetComponent<Rigidbody>().position - rb.position).normalized * explosionForce, ForceMode.VelocityChange);
                cube.GetComponent<Bloc>().state = BlocState.projectile;
                //Remove cube
                GetComponent<PlayerObjects>().removeCube(cube);
            }
        }
        gridPlayer.clearGrid();
        feedback.DashVibration();
    }

   
    public void DoPause(InputAction.CallbackContext context)
    {
        TogglePause();
    }

    public void TogglePause()
    {
        if (!isPaused)
        {
            Time.timeScale = 0f;
            pauseMenu.SetActive(true);
            inputUI.SetActive(false);
            EventSystem.current.SetSelectedGameObject(selectedGUI);
        }
        else
        {
            Time.timeScale = 1f;
            inputUI.SetActive(true);
            pauseMenu.SetActive(false);
        }
        isPaused = !isPaused;
    }

   
    private void joystick4(Vector3 direction)
    {
  
        if(direction.magnitude > 0.1f)
        {
            animator.SetBool("IsMoving", true);
        }
        else
        {
            animator.SetBool("IsMoving", false);
        }
        if ((leftTrigger > .1f || rightTrigger > .1f) && direction.magnitude>0.1f)
        {

            golem.GetComponent<Synchro2>().rotationFixed = false;

            if(direction.magnitude > deadZone ){
                rb.AddForce(mouvementReductionFactor * direction * mouvementSpeed / weightTranslation, ForceMode.Acceleration);
            }
            
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
            rotatingRight = true;
            Quaternion targetRotation = Quaternion.Euler(0f, targetAngle, 0f);
            golem.rotation = Quaternion.Lerp(
                golem.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );
        }
        else
        {
            golem.GetComponent<Synchro2>().rotationFixed = true;

            rb.AddForce(direction * mouvementSpeed / weightTranslation, ForceMode.Acceleration);
            rotatingRight = false;
            rotateAndDirection5(direction);
        }
    }
    private void HyperVite(Vector3 direction)
    {
        golem.GetComponent<Synchro2>().rotationFixed = false;
        if (direction.magnitude > 0.1f)
        {
            animator.SetBool("IsMoving", true);
        }
        else
        {
            animator.SetBool("IsMoving", false);
        }

        if ((leftTrigger > .1f || rightTrigger > .1f) && direction.magnitude > 0.1f)
        {
            if (direction.magnitude > deadZone)
            {
                rb.AddForce(mouvementReductionFactor * direction * mouvementSpeed / weightTranslation, ForceMode.Acceleration);
            }

        }

        else
        {
            rb.AddForce(direction * mouvementSpeed / weightTranslation, ForceMode.Acceleration);
        }
        if(direction.magnitude > deadZone)
        {
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
            rotatingRight = true;
            Quaternion targetRotation = Quaternion.Euler(0f, targetAngle, 0f);
            golem.rotation = Quaternion.Lerp(
                golem.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );
        }
        rb.AddTorque(Vector3.up * rotationSpeedPowerUp);
    }
    

    public Vector3 CalculateCenterMass()
    {
        Vector3 center = Vector3.zero;
        totalMass = 0;


        foreach (var v in gridPlayer.grid)
        {
            GameObject obj = v.Value;
            Rigidbody rbb = obj.GetComponent<Rigidbody>();

            if (rbb.mass > 0)
            {
                center += rbb.worldCenterOfMass * rbb.mass;
                totalMass += rbb.mass;
            }
        }

        center = center / totalMass;
        return center;

    }

  
    public void rotateAndDirection5(Vector3 direction)
    {
        Rigidbody rb = GetComponent<PlayerObjects>().cubeRb;
        Vector3 structureFoward = golem.transform.forward; ;
        
        float angle = Vector3.SignedAngle(structureFoward, direction.normalized, Vector3.up);
        Vector3 angularVelocity = Vector3.up * (angle * Mathf.Deg2Rad) * direction.magnitude * pivotSpeed / weightRotation;
        if (direction != Vector3.zero)
        {
            if (Mathf.Abs(angle) > 1)
            {
                rb.AddTorque(angularVelocity, ForceMode.Acceleration);
                rb.AddTorque(-rb.angularVelocity * rotationDamping, ForceMode.Acceleration);
                float angleRot = rb.angularVelocity.magnitude * Time.fixedDeltaTime;

                Vector3 axis = angleRot > Mathf.Epsilon ? angularVelocity.normalized : Vector3.forward;

                Quaternion deltaRotation = Quaternion.AngleAxis(angleRot * Mathf.Rad2Deg, axis);

                golem.rotation *= deltaRotation;
            }
            else
            {
                rb.angularVelocity = Vector3.zero;
            }
        }
    }
   
   
}

