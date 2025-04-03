using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
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

    public GameObject pauseMenu; // Assign in Inspector
    public GameObject selectedGUI; // Assign in Inspector
    bool isPaused = false;

    PlayerInput playerInput;
    InputAction moveAction;
    InputAction rotateAction;
    InputAction throwCubes;
    InputAction rightShoulder;
    InputAction rotateActionZ;
    InputAction rotateActionX;
    InputAction pauseAction;
    InputAction rotateTwinStick;

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

    void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        moveAction = playerInput.actions.FindAction("Move");
        rotateAction = playerInput.actions.FindAction("Rotate");
        throwCubes = playerInput.actions.FindAction("ThrowCubes");
        rotateActionZ = playerInput.actions.FindAction("RotateZ");
        rotateActionX = playerInput.actions.FindAction("RotateX");
        rotateTwinStick = playerInput.actions.FindAction("RotateTwinStick");
        ejectCubes = playerInput.actions.FindAction("BlocEjection");
        attractCubes = playerInput.actions.FindAction("AttractCubes");
        rightShoulder = playerInput.actions.FindAction("RightShoulder");
        pauseMenu.SetActive(false); // Hide canvas at start
        pauseAction = playerInput.actions.FindAction("Pause");
        gridPlayer = GetComponent<GridSystem>();
        rb = GetComponent<PlayerObjects>().cubeRb;
        golem = GetComponent<PlayerObjects>().golem.transform;
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
            Vector3 direction = new Vector3(direction2.x, 0, direction2.y);
            Vector3 directionTwin2 = rotateTwinStick.ReadValue<Vector3>();
            Vector3 directionTwin = new Vector3(directionTwin2.x, 0, directionTwin2.y);
            float rotationY = rotateAction.ReadValue<float>();
            float rotationZ = rotateActionZ.ReadValue<float>();
            float rotationX = rotateActionX.ReadValue<float>();

            leftTrigger = attractCubes.ReadValue<float>();
            rightTrigger = ejectCubes.ReadValue<float>();

            switch (moveType)
            {

                case MouvementType.spring:
                    Spring(direction, rotationY);
                    break;

                case MouvementType.move3d:
                    Move3d(direction, rotationY);
                    break;

                case MouvementType.move3dSpring:
                    Move3dSpring(direction, rotationY);
                    break;

                case MouvementType.Move3dBothJoystick:
                    BoothJoystickMove(direction, rotationY);
                    break;
                case MouvementType.Move3dBothJoystickSpring:
                    Move3dSpringBothJoystick(direction, rotationY);
                    break;
                case MouvementType.TwinStickShooter:
                    twinStickShooter(direction, directionTwin);
                    break;
                case MouvementType.TwinStickShooter2:
                    twinStickShooter2(direction, directionTwin);
                    break;
                case MouvementType.TwinStickShooter3:
                    twinStickShooter3(direction, directionTwin);
                    break;
                case MouvementType.Joystick4:
                    joystick4(direction);
                    break;
            }
        }


        // ThrowCubes();
    }

    private void OnEnable()
    {
        feedback = GetComponent<HapticFeedbackController>();
        dash = GetComponent<Dash>();
        pauseAction.performed += OnPause;
        pauseAction.Enable();
        throwCubes.performed += _ =>
        {
            shoulderLeftPressed = true;
            Ejection();
        };
        rightShoulder.performed += _ =>
        {
            shoulderRightPressed = true;
            Ejection();
        };
        throwCubes.canceled += _ =>
        {
            shoulderLeftPressed = false;
        };
        rightShoulder.canceled += _ =>
        {
            shoulderRightPressed = false;
        };

    }

    private void Ejection()
    {
        if(shoulderLeftPressed && shoulderRightPressed)
        {
            feedback.EjectionVibrationEnd();
            ThrowCubes();
            if(dash!= null)
            {
                dash.Dash(rb);
            }
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
        feedback.EjectionVibrationEnd();
    }

    private void ShouldersPressed(InputAction.CallbackContext context)
    {

        feedback.EjectionVibrationStart();
    }

    private void OnDisable()
    {
        pauseAction.performed -= OnPause;
        pauseAction.Disable();
    }

    public void OnPause(InputAction.CallbackContext context)
    {
        TogglePause();
    }

    public void TogglePause()
    {
        if (isPaused)
        {
            Time.timeScale = 0f;
            pauseMenu.SetActive(true);
            EventSystem.current.SetSelectedGameObject(selectedGUI);
        }
        else
        {
            Time.timeScale = 1f;
            pauseMenu.SetActive(false);
        }
        isPaused = !isPaused;
    }

    private void Spring(Vector3 direction, float rotationY)
    {
        rb.AddForceAtPosition(direction * mouvementSpeed, CalculateCenterMass());

        if (Mathf.Abs(rotationY) > 0)
        {
            if (!rotatingRight)
            {
                rb.transform.Find("GolemBuilt").GetComponent<SynchroGolem>().setLockRotation(true);
            }
            rotatingRight = true;
            rb.AddTorque(Vector3.up * rotationY * rotationSpeed / weightRotation, ForceMode.Acceleration);
        }
        else
        {
            if (rotatingRight)
            {
                rb.angularVelocity = Vector3.zero;
                rb.transform.Find("GolemBuilt").GetComponent<SynchroGolem>().setLockRotation(false);
                rotatingRight = false;
            }
        }
    }

    private void Move3d(Vector3 direction, float rotationY)
    {
        //Left joystick
        rb.AddForce(direction * mouvementSpeed / weightMouvementFactor, ForceMode.Acceleration);
        rotateAndDirection2(direction);

        //Right joystick
        if (Mathf.Abs(rotationY) > 0)
        {
            if (!rotatingRight)
            {
                rb.transform.Find("GolemBuilt").GetComponent<SynchroGolem>().setLockRotation(true);
            }
            rotatingRight = true;
            rb.AddTorque(Vector3.up * rotationY * rotationSpeed / weightRotation, ForceMode.Acceleration);
        }
        else
        {
            if (rotatingRight)
            {
                rb.angularVelocity = Vector3.zero;
                rb.transform.Find("GolemBuilt").GetComponent<SynchroGolem>().setLockRotation(false);
                rotatingRight = false;
            }
        }
    }

    private void Move3dSpring(Vector3 direction, float rotationY)
    {
        if (rotationY > 0.1)
        {
            rb.AddForceAtPosition(mouvementReductionFactor * direction * mouvementSpeed / weightTranslation, CalculateCenterMass(), ForceMode.Acceleration);
        }
        else
        {
            rb.AddForceAtPosition(direction * mouvementSpeed / weightTranslation, CalculateCenterMass(), ForceMode.Acceleration);
        }
        rotateAndDirection2(direction);
        if (Mathf.Abs(rotationY) > 0)
        {
            if (!rotatingRight)
            {
                rb.transform.Find("GolemBuilt").GetComponent<SynchroGolem>().setLockRotation(true);
            }
            rotatingRight = true;
            rb.AddTorque(Vector3.up * rotationY * rotationSpeed / weightRotation, ForceMode.Acceleration);
        }
        else
        {
            if (rotatingRight)
            {
                rb.angularVelocity = Vector3.zero;
                rb.transform.Find("GolemBuilt").GetComponent<SynchroGolem>().setLockRotation(false);
                rotatingRight = false;
            }
        }
    }
    private void Move3dSpringBothJoystick(Vector3 direction, float rotationY)
    {
        if (rotationY > 0.1)
        {
            rb.AddForceAtPosition(mouvementReductionFactor * direction * mouvementSpeed / weightTranslation, CalculateCenterMass(), ForceMode.Acceleration);
        }
        else
        {
            rb.AddForceAtPosition(direction * mouvementSpeed / weightTranslation, CalculateCenterMass(), ForceMode.Acceleration);
        }
        if (!rotatingRight)
        {
            rotateAndDirection2(direction);

        }
        if (Mathf.Abs(rotationY) > 0)
        {
            if (!rotatingRight)
            {
                golem.GetComponent<SynchroGolem>().setLockRotation(true);
            }
            rotatingRight = true;
            rb.AddTorque(Vector3.up * rotationY * rotationSpeed / weightRotation, ForceMode.Acceleration);
        }
        else
        {
            if (rotatingRight)
            {
                rb.angularVelocity = Vector3.zero;
                golem.GetComponent<SynchroGolem>().setLockRotation(false);
                rotatingRight = false;
            }
        }
    }

    private void twinStickShooter(Vector3 direction, Vector3 directionTwin)
    {
        if (directionTwin.magnitude > 0.1)
        {
            rb.AddForceAtPosition(mouvementReductionFactor * direction * mouvementSpeed / weightTranslation, CalculateCenterMass(), ForceMode.Acceleration);
        }
        else
        {
            rb.AddForceAtPosition(direction * mouvementSpeed / weightTranslation, CalculateCenterMass(), ForceMode.Acceleration);
        }

        if (!rotatingRight)
        {
            rotateAndDirection2(direction);

        }
        if (directionTwin.sqrMagnitude > 0.1f)
        {
            float targetAngle = Mathf.Atan2(directionTwin.x, directionTwin.z) * Mathf.Rad2Deg;
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
            rotatingRight = false;
            t = 0;
        }

    }
    private void twinStickShooter2(Vector3 direction, Vector3 directionTwin)
    {
        if (directionTwin.magnitude > 0.1 || direction.magnitude > 0.1)
        {
            golem.GetComponent<Synchro2>().rotationFixed = false;
        }
        else
        {
            golem.GetComponent<Synchro2>().rotationFixed = true;
        }
        if (directionTwin.magnitude > 0.1)
        {
            rb.AddForceAtPosition(mouvementReductionFactor * direction * mouvementSpeed / weightTranslation, CalculateCenterMass(), ForceMode.Acceleration);
        }
        else
        {
            rb.AddForceAtPosition(direction * mouvementSpeed / weightTranslation, CalculateCenterMass(), ForceMode.Acceleration);
        }


        rotateAndDirection3(direction);


        if (directionTwin.sqrMagnitude > 0.1f)
        {
            float targetAngle = Mathf.Atan2(directionTwin.x, directionTwin.z) * Mathf.Rad2Deg;
            Quaternion targetRotation = Quaternion.Euler(0f, targetAngle, 0f);

            golem.rotation = Quaternion.Lerp(
                golem.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );

        }
        else
        {
            t = 0;
        }

    }
    private void twinStickShooter3(Vector3 direction, Vector3 directionTwin)
    {
        if (directionTwin.magnitude > 0.1)
        {
            golem.GetComponent<Synchro2>().rotationFixed = false;
        }
        else
        {
            golem.GetComponent<Synchro2>().rotationFixed = true;
        }
        if (leftTrigger > .1f || rightTrigger > .1f)
        {
            rb.AddForceAtPosition(mouvementReductionFactor * direction * mouvementSpeed / weightTranslation, CalculateCenterMass(), ForceMode.Acceleration);
        }
        else
        {
            rb.AddForceAtPosition(direction * mouvementSpeed / weightTranslation, CalculateCenterMass(), ForceMode.Acceleration);
        }

        rotateAndDirection5(direction);

        if (directionTwin.sqrMagnitude > 0.1f)
        {
            float targetAngle = Mathf.Atan2(directionTwin.x, directionTwin.z) * Mathf.Rad2Deg;
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
            rotatingRight = false;
            t = 0;
        }

    }
    private void joystick4(Vector3 direction)
    {
  
        if ((leftTrigger > .1f || rightTrigger > .1f) && direction.magnitude>0.1f)
        {
            golem.GetComponent<Synchro2>().rotationFixed = false;

            if(direction.magnitude > deadZone ){
                 rb.AddForceAtPosition(mouvementReductionFactor * direction * mouvementSpeed / weightTranslation, CalculateCenterMass(), ForceMode.Acceleration);
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
            rb.AddForceAtPosition(direction * mouvementSpeed / weightTranslation, CalculateCenterMass(), ForceMode.Acceleration);
            rotatingRight = false;
            rotateAndDirection5(direction);
        }
    }
    private void BoothJoystickMove(Vector3 direction, float rotationY)
    {
        //Left joystick
        rb.AddForce(direction * mouvementSpeed / (weight + weightMouvementFactor), ForceMode.Acceleration);

        if (!rotatingRight)
        {
            rotateAndDirection2(direction);
        }

        //Right joystick
        if (Mathf.Abs(rotationY) > 0)
        {
            rotatingRight = true;

            rb.AddTorque(Vector3.up * rotationY * rotationSpeed / (weight + weightRotationFactor), ForceMode.Acceleration);
            rb.transform.Find("GolemBuilt").GetComponent<SynchroGolem>().setLockRotation(true);



        }
        else
        {
            if (rotatingRight)
            {
                rb.angularVelocity = Vector3.zero;
                rb.transform.Find("GolemBuilt").GetComponent<SynchroGolem>().setLockRotation(false);
                rotatingRight = false;
            }
        }
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

    public void stopStructure()
    {
        foreach (var v in gridPlayer.grid)
        {
            Rigidbody cubeRigidBody = v.Value.GetComponent<Rigidbody>();
            cubeRigidBody.angularVelocity = Vector3.zero;
            cubeRigidBody.velocity = Vector3.zero;
        }
    }
    public void rotateAndDirection2(Vector3 direction)
    {
        Vector3 planeProjection = golem.transform.forward;
        float angle = Vector3.SignedAngle(planeProjection, direction.normalized, Vector3.up);
        Vector3 angularVelocity = Vector3.up * (angle * Mathf.Deg2Rad) * direction.magnitude * pivotSpeed / weightRotation;

        if (direction != Vector3.zero)
        {
            if (Mathf.Abs(angle) > 1)
            {
                if (!rotatingRight)
                {
                    rb.AddTorque(angularVelocity, ForceMode.Acceleration);
                    rb.AddTorque(-rb.angularVelocity * rotationDamping, ForceMode.Acceleration);
                }
                else
                {
                    Quaternion rot = Quaternion.AngleAxis(angularVelocity.y * 0.01f, Vector3.up);
                    rb.transform.Find("GolemBuilt").GetComponent<SynchroGolem>().setRotationAdd(rot);
                }
            }
            else
            {
                if (!rotatingRight)
                {
                    rb.angularVelocity = Vector3.zero;
                }
                else
                {
                    rb.transform.Find("GolemBuilt").GetComponent<SynchroGolem>().setRotationAdd(Quaternion.identity);
                }


            }
        }
    }
    public void rotateAndDirection3(Vector3 direction)
    {
        Rigidbody rb = GetComponent<PlayerObjects>().cubeRb;
        Vector3 structureFoward = rb.transform.forward; ;

        float angle = Vector3.SignedAngle(structureFoward, direction.normalized, Vector3.up);
        Vector3 angularVelocity = Vector3.up * (angle * Mathf.Deg2Rad) * direction.magnitude * pivotSpeed / weightRotation;

        if (direction != Vector3.zero)
        {
            if (Mathf.Abs(angle) > 1)
            {
                rb.AddTorque(angularVelocity, ForceMode.Acceleration);
                rb.AddTorque(-rb.angularVelocity * rotationDamping, ForceMode.Acceleration);
            }
            else
            {
                rb.angularVelocity = Vector3.zero;
            }
        }
    }
    public void rotateAndDirection4(Vector3 direction)
    {
        Rigidbody rb = GetComponent<PlayerObjects>().cubeRb;
        Vector3 structureFoward = rb.transform.forward; ;

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
    public Vector3 QuaternionToAngularVelocity(Quaternion rotation)
    {
        // Extract the axis and angle from the quaternion
        rotation.ToAngleAxis(out float angle, out Vector3 axis);

        // Convert the angle to radians and scale by the rotation speed
        return axis * (angle * Mathf.Deg2Rad / Time.fixedDeltaTime);
    }
    private IEnumerator AngleRotation()
    {
        Quaternion rotationAmount = Quaternion.AngleAxis(90f, Vector3.up);
        Quaternion initialRotation = rb.rotation;
        Quaternion rotationTarget = initialRotation * rotationAmount;
        rb.MoveRotation(rotationTarget);
        rb.transform.Find("GolemBuilt").GetComponent<SynchroGolem>().setLockRotation(false);
        rb.angularVelocity = Vector3.zero;

        foreach (var v in gridPlayer.grid)
        {
            GameObject obj = v.Value;
            Rigidbody rbb = obj.GetComponent<Rigidbody>();
            rbb.angularVelocity = Vector3.zero;
            rbb.velocity = Vector3.zero;
        }
        yield return new WaitForSeconds(3f);
        rotatingRight = false;

    }
}

