using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using System.Security.Cryptography;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.VFX.UI;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.HID;
using UnityEngine.InputSystem.Switch;
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
    [SerializeField] MouvementType moveType;


    PlayerInput playerInput;
    InputAction moveAction;
    InputAction rotateAction;
    InputAction throwCubes;
    InputAction rotateActionZ;
    InputAction rotateActionX;

   
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
        if (moveType == MouvementType.rigidBody || moveType == MouvementType.move3d)
        {
          ///  rb.centerOfMass = Vector3.zero;
           // rb.inertiaTensor= new Vector3(1,1,1);
        }
        //golem = transform.Find("GolemBuilt").GetComponent<Rigidbody>();
      
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        weight = this.GetComponent<PlayerObjects>().weight;

        Vector3 direction2 = moveAction.ReadValue<Vector3>();
        Vector3 direction = new Vector3(direction2.x,0,direction2.y);
        float rotationY = rotateAction.ReadValue<float>();
        float rotationZ = rotateActionZ.ReadValue<float>();
        float rotationX = rotateActionX.ReadValue<float>();


        switch (moveType)
        {
            case MouvementType.rigidBody:
                RigidBodyMouvement(direction, rotationY);
                break;

            case MouvementType.spring:
                Spring(direction, rotationY);
                break;

            case MouvementType.move3d:
                Move3d(direction, rotationY);
                break;

            case MouvementType.move3dSpring:
                Move3dSpring(direction, rotationY);
                break;

            case MouvementType.move3d_RbGolem:
                HingeMove(direction, rotationY);
                break;

            case MouvementType.Move3dBothJoystick:
                BoothJoystickMove(direction, rotationY);
                break;
        }

      
            ThrowCubes();
        
    }

    private void Spring(Vector3 direction, float rotationY)
    {
        rb.AddForceAtPosition(direction * mouvementSpeed, CalculateCenterMass());
        rotateAndDirection(direction);
        if (Mathf.Abs(rotationY) > 0)
        {
            rotatingRight = true;
            Quaternion rot = Quaternion.AngleAxis(rotationY * 1, Vector3.up);
            rb.AddTorque(Vector3.up * rotationY * rotationSpeed/weight, ForceMode.Acceleration);
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

    private void Move3d(Vector3 direction, float rotationY)
    {
        //Left joystick
        rb.AddForce(direction * mouvementSpeed / (weight+weightMouvementFactor),ForceMode.Acceleration);
        rotateAndDirection2(direction);

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

    private void Move3dSpring(Vector3 direction, float rotationY)
    {
        rb.AddForceAtPosition(direction * mouvementSpeed/(weight+weightMouvementFactor), CalculateCenterMass(),ForceMode.Acceleration);
        rotateAndDirection2(direction);
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

    private void HingeMove(Vector3 direction, float rotationY)
    {
        golem.AddForce(direction * mouvementSpeed / weight);
        //rb.AddForceAtPosition(direction * mouvementSpeed, CalculateCenterMass());
        rotateAndDirection(direction);


        //Right joystick
        if (Mathf.Abs(rotationY) > 0)
        {
            rotatingRight = true;
            rb.GetComponent<HingeJoint>().useLimits = false;
            rb.AddTorque(Vector3.up * rotationY * rotationSpeed/weight, ForceMode.Acceleration);

        }
        else
        {
            if (rotatingRight)
            {
                rb.angularVelocity = Vector3.zero;
                rotatingRight = false;

                HingeJoint joint = rb.GetComponent<HingeJoint>();
                Rigidbody anchor = joint.connectedBody;
                DestroyImmediate(rb.GetComponent<HingeJoint>());

                rb.AddComponent<HingeJoint>();
                HingeJoint newJoint = rb.GetComponent<HingeJoint>();
                newJoint.axis = Vector3.up;
                newJoint.connectedBody = anchor;
                newJoint.anchor = Vector3.zero;
                newJoint.useLimits = true;
                newJoint.extendedLimits = true;
                newJoint.useAcceleration = true;
                newJoint.autoConfigureConnectedAnchor = false;
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

    private void TranslateMouvement(Vector3 direction, float rotation)
    {
        transform.position += direction * mouvementSpeed * Time.fixedDeltaTime;
        transform.Rotate(Vector3.up, rotation * pivotSpeed * Time.fixedDeltaTime, Space.World);
    }
    private void RigidBodyMouvement(Vector3 direction, float rotationY) {
        rb.AddForce(direction * mouvementSpeed/ weight,ForceMode.Acceleration);
       
        if (Mathf.Abs(rotationY) > 0)
        {
            rotatingRight = true;
            rb.AddTorque(Vector3.up * rotationY * rotationSpeed, ForceMode.Acceleration);
            rb.transform.Find("GolemBuilt").GetComponent<SynchroGolem>().setLockRotation(true);

        }
        else
        {
            if (rotatingRight)
            {
                rb.GetComponent<HingeJoint>().useLimits = true;
                rb.angularVelocity = Vector3.zero;
                rb.transform.Find("GolemBuilt").GetComponent<SynchroGolem>().setLockRotation(false);
                rotatingRight = false;
            }
        }
    }

    private void ThrowCubes()
    {
        if (throwCubes.ReadValue<float>() == 1)
        {
            foreach (var item in gridPlayer.grid)
            {
                GameObject cube = item.Value;
                if(cube != this.rb.gameObject){
                cube.gameObject.layer = 0;
                cube.transform.parent = this.transform.parent;
                GameObject.Destroy(cube.GetComponent<SphereCollider>());

                //Add rigidBody
                
                if(moveType == MouvementType.move3dSpring){
                    ConfigurableJoint[] joints = cube.GetComponents<ConfigurableJoint>();
                    foreach (ConfigurableJoint joint in joints)
                      {
                        DestroyImmediate(joint);
                      }
                }
                else
                {
                    GetComponent<PlayerObjects>().addRigidBody(cube);
                }
              
                cube.GetComponent<Rigidbody>().interpolation = RigidbodyInterpolation.Interpolate;
                cube.GetComponent<Rigidbody>().AddExplosionForce(explosionForce, this.rb.position, playerCharge);
                cube.GetComponent<Bloc>().owner += "projectile"; 
                //Remove owner of cube
                StartCoroutine(blockNeutral(cube));
                }
                
            }
            gridPlayer.clearGrid();
        }
    }
   
    IEnumerator blockNeutral(GameObject block)
    {

        yield return new WaitForSeconds(3f);
        if(block !=null)
        {
            block.GetComponent<Bloc>().setOwner("Neutral");
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
    public Vector3 geometricCenter()
    {
        Vector3 center = Vector3.zero;
        foreach (var v  in gridPlayer.grid)
        {
            GameObject obj = v.Value;
            center += obj.transform.position;
        }
        center = center / gridPlayer.grid.Count;
        center = rb.transform.InverseTransformPoint(center);
        return center;
       
    }
    public void CalculateCenterMassForce(Vector3 LocalForceDirection)
    {
        Vector3 centerMass = CalculateCenterMass();

        foreach (var v  in gridPlayer.grid)
        {
            GameObject obj = v.Value;
            Rigidbody rbb = obj.GetComponent<Rigidbody>();
            float proportionalForce = (rbb.mass) * mouvementSpeed;
            rbb.AddForceAtPosition(LocalForceDirection * proportionalForce, centerMass, ForceMode.Force);
        }

    }
    public void addTorque(float rotation)
    {

        Vector3 pivotPoint = rb.position;
        rb.MoveRotation(rb.rotation * Quaternion.Euler(0, (pivotSpeed * rotation) / rotParam, 0));
        foreach (var v in gridPlayer.grid)
        {
            GameObject obj = v.Value;
            Rigidbody rbb = obj.GetComponent<Rigidbody>();
            Vector3 radiusVector = rbb.position - pivotPoint;
            Vector3 rot = Vector3.Cross(radiusVector, Vector3.up);
            rbb.AddForce(-rot * rotation * pivotSpeed );

        }
    }
    public void addTorqueSingle(float rotation)
    {
        rb.AddTorque(Vector3.up * rotation * pivotSpeed);
    }
    public void rotateAndDirection(Vector3 direction){
        
       Vector3 planeProjection = golem.transform.forward;
       float angle = Vector3.SignedAngle(planeProjection,direction.normalized,Vector3.up);
        Debug.Log(angle);
       Vector3 angularVelocity = Vector3.up * (angle*Mathf.Deg2Rad )* direction.magnitude*pivotSpeed/ weight;
        if (direction != Vector3.zero && Mathf.Abs(angle)>1) {
          
                golem.AddTorque(angularVelocity,ForceMode.Acceleration);
                golem.AddTorque(-rb.angularVelocity * rotationDamping,ForceMode.Acceleration);

        }
        else if(Mathf.Abs(angle) <1)
        {
            
            golem.angularVelocity = Vector3.zero;

        }
    }
    public void rotateAndDirection2(Vector3 direction)
    {
        Vector3 planeProjection = rb.transform.Find("GolemBuilt").forward;
        float angle = Vector3.SignedAngle(planeProjection, direction.normalized, Vector3.up);
        Vector3 angularVelocity = Vector3.up * (angle * Mathf.Deg2Rad) * direction.magnitude * pivotSpeed / (weight+weightRotationFactor);

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

}

