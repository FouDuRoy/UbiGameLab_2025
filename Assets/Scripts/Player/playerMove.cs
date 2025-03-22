using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using System.Security.Cryptography;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.HID;
using UnityEngine.InputSystem.Switch;
using UnityEngine.ProBuilder.Shapes;
using static UnityEngine.Rendering.DebugUI;
using static UnityEngine.Rendering.DebugUI.Table;
using Matrix4x4 = UnityEngine.Matrix4x4;
using Quaternion = UnityEngine.Quaternion;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

//Federico Barallobres
public class PlayerMouvement : MonoBehaviour
{
    [SerializeField] float explosionForce = 30;
    [SerializeField] float mouvementSpeed = 1f;
    [SerializeField] float pivotSpeed = 1f;
    [SerializeField] float rotationSpeed = 1f;
    [SerializeField] float playerCharge = 1000f;
    [SerializeField] float rotParam;
    [SerializeField] float rotationDamping =10f;
    [SerializeField] float weightMouvementFactor =1f;
    [SerializeField] float weightRotationFactor = 1f;
    [SerializeField] public MouvementType moveType;


    PlayerInput playerInput;
    InputAction moveAction;
    InputAction rotateAction;
    InputAction throwCubes;
    InputAction rotateActionZ;
    InputAction rotateActionX;

    float weightRotation;
    float weightTranslation;
    float totalMass;
    float weight;
    Rigidbody golem;
    Rigidbody rb;

    private GameObject reff;

    bool rotatingRight = false;
    HapticFeedbackController feedback;

    GridSystem gridPlayer;

    void Start()
    {
        playerInput = GetComponent<PlayerInput>();
        moveAction = playerInput.actions.FindAction("Move");
        rotateAction = playerInput.actions.FindAction("Rotate");
        throwCubes = playerInput.actions.FindAction("ThrowCubes");
        rotateActionZ = playerInput.actions.FindAction("RotateZ");
        rotateActionX = playerInput.actions.FindAction("RotateX");
        gridPlayer = GetComponent<GridSystem>();
        rb = this.GetComponent<PlayerObjects>().cubeRb;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        weight = this.GetComponent<PlayerObjects>().weight;
         weightRotation = Mathf.Clamp(weight * weightRotationFactor, 1, 10 * weight);
         weightTranslation = Mathf.Clamp(weight * weightMouvementFactor, 1, 10 * weight);
        Vector3 direction2 = moveAction.ReadValue<Vector3>();
        Vector3 direction = new Vector3(direction2.x,0,direction2.y);
        float rotationY = rotateAction.ReadValue<float>();
        float rotationZ = rotateActionZ.ReadValue<float>();
        float rotationX = rotateActionX.ReadValue<float>();


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
        }

      
            ThrowCubes();
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
        rb.AddForceAtPosition(direction * mouvementSpeed/weightTranslation, CalculateCenterMass(),ForceMode.Acceleration);
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
        rb.AddForceAtPosition(direction * mouvementSpeed / weightTranslation, CalculateCenterMass(), ForceMode.Acceleration);
        if (!rotatingRight)
        {
            rotateAndDirection2(direction);

        }
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
    private void Move3dSpringBothJoystickSnap90(Vector3 direction, float rotationY)
    {
        rb.AddForceAtPosition(direction * mouvementSpeed / (weight + weightMouvementFactor), CalculateCenterMass(), ForceMode.Acceleration);
        if (!rotatingRight)
        {
            rotateAndDirection2(direction);

        }
        if (Mathf.Abs(rotationY) > 0)
        {
            if (!rotatingRight)
            {
                rotatingRight = true;
                rb.transform.Find("GolemBuilt").GetComponent<SynchroGolem>().setLockRotation(true);
                StartCoroutine(AngleRotation());
            }

        }
    }

    private void BoothJoystickMove(Vector3 direction, float rotationY)
    {
        //Left joystick
        rb.AddForce(direction * mouvementSpeed / (weight+weightMouvementFactor),ForceMode.Acceleration);

        if (!rotatingRight)
        {
            rotateAndDirection2(direction);
        }

        //Right joystick
        if (Mathf.Abs(rotationY) > 0)
        {
            rotatingRight = true;

            rb.AddTorque(Vector3.up * rotationY * rotationSpeed/(weight+weightRotationFactor), ForceMode.Acceleration);
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


    

    public void ThrowCubes()
    {
        if (throwCubes.ReadValue<float>() == 1)
        {
            foreach (var item in gridPlayer.grid)
            {
                GameObject cube = item.Value;
                if(cube != this.rb.gameObject){
          
                cube.GetComponent<Rigidbody>().AddForce((cube.GetComponent<Rigidbody>().position-rb.position).normalized*explosionForce, ForceMode.VelocityChange);
                cube.GetComponent<Bloc>().state = BlocState.projectile;
                 //Remove cube
                GetComponent<PlayerObjects>().removeCube(cube);
                }
            }
            gridPlayer.clearGrid();
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


    public void rotateAndDirection2(Vector3 direction)
    {
        Vector3 planeProjection = rb.transform.Find("GolemBuilt").forward;
        float angle = Vector3.SignedAngle(planeProjection, direction.normalized, Vector3.up);
        Vector3 angularVelocity = Vector3.up * (angle * Mathf.Deg2Rad) * direction.magnitude * pivotSpeed / weightRotation;

        if (direction != Vector3.zero)
        {
            if(Mathf.Abs(angle) > 1)
            {
                if (!rotatingRight)
                {
                    rb.AddTorque(angularVelocity, ForceMode.Acceleration);
                    rb.AddTorque(-rb.angularVelocity * rotationDamping, ForceMode.Acceleration);
                }
                else
                {
                    Quaternion rot = Quaternion.AngleAxis(angularVelocity.y*0.01f, Vector3.up);
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

    public Vector3 QuaternionToAngularVelocity(Quaternion rotation)
    {
        // Extract the axis and angle from the quaternion
        rotation.ToAngleAxis(out float angle, out Vector3 axis);

        // Convert the angle to radians and scale by the rotation speed
        return axis * (angle * Mathf.Deg2Rad / Time.fixedDeltaTime);
    }
    private IEnumerator AngleRotation()
    {
        float t = 0;
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

