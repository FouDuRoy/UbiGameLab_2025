using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using System.Security.Cryptography;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.HID;
using UnityEngine.InputSystem.Switch;
using UnityEngine.UIElements;
using static UnityEngine.Rendering.DebugUI;
using Matrix4x4 = UnityEngine.Matrix4x4;
using Quaternion = UnityEngine.Quaternion;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

public class PlayerMouvement : MonoBehaviour
{
    private const float collisionDistance = 0.1f;
    [SerializeField] float explosionForce = 30;
    [SerializeField] float speed = 1f;
    [SerializeField] float speedRotation = 1f;
    [SerializeField] float playerCharge = 1000f;
    [SerializeField] float rotParam;
    [SerializeField] float rotationDamping =10f;
    [SerializeField] MouvementType moveType;
    [SerializeField] GameObject reff;
    PlayerInput playerInput;
    InputAction moveAction;
    InputAction rotateAction;
    InputAction throwCubes;
    InputAction rotateActionZ;
    InputAction rotateActionX;
    InputAction shoulderB;
    InputAction rRotate;
    Rigidbody rb;
    float totalMass;
    bool rotX = false;
    bool rotY = false;
    bool rotZ = false;
    bool adjust = false;
    bool holding = false;
    float sign = 0;
    


    void Start()
    {
        playerInput = GetComponent<PlayerInput>();
        moveAction = playerInput.actions.FindAction("Move");
        rotateAction = playerInput.actions.FindAction("Rotate");
        throwCubes = playerInput.actions.FindAction("ThrowCubes");
        rotateActionZ = playerInput.actions.FindAction("RotateZ");
        rotateActionX = playerInput.actions.FindAction("RotateX");
        shoulderB = playerInput.actions.FindAction("R90");
        rRotate = playerInput.actions.FindAction("RotateR");
        rb = this.GetComponent<PlayerObjects>().cubeRb;
        if (moveType == MouvementType.rigidBody || moveType == MouvementType.move3d)
        {
            rb.centerOfMass = Vector3.zero;
            rb.inertiaTensor= new Vector3(1,1,1);
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        Vector3 direction2 = moveAction.ReadValue<Vector3>();
        Vector3 direction = new Vector3(direction2.x,0,direction2.y);
        float rotationY = rotateAction.ReadValue<float>();
        float rotationZ = rotateActionZ.ReadValue<float>();
        float rotationX = rotateActionX.ReadValue<float>();
    
        if (moveType == MouvementType.rigidBody)
        {
            RigidBodyMouvement(direction, rotationY,rotationX);
            
        }
        else if (moveType == MouvementType.spring)
        {
            rb.AddForceAtPosition(direction * speed, CalculateCenterMass());
            rb.AddTorque(Vector3.up * rotationY * speedRotation,ForceMode.Force);
        }
        else if (moveType == MouvementType.transform)
        {
            TranslateMouvement(direction, rotationY);
        }else if(moveType == MouvementType.move3d){
            rb.AddForce(direction * speed);
            rotateAndDirection2(direction, rotationX,rotationZ);


        }
        else if (moveType == MouvementType.move3dSpring){
            rb.AddForceAtPosition(direction * speed,CalculateCenterMass());
            rotateAndDirection(direction);
            rotateXandZ(rotationX,rotationZ);
        }

        ThrowCubes();


    }

    private void TranslateMouvement(Vector3 direction, float rotation)
    {
        transform.position += direction * speed * Time.fixedDeltaTime;
        transform.Rotate(Vector3.up, rotation * speedRotation * Time.fixedDeltaTime, Space.World);
    }
    private void RigidBodyMouvement(Vector3 direction, float rotation,float rotationX) {
        rb.AddForce(direction * speed);
       
        rb.AddTorque(Vector3.up * rotation * speedRotation);
        if (shoulderB.triggered)
        {
            rb.MoveRotation(rb.rotation * Quaternion.Euler(90, 0, 0));
                
        }



    }

    private void ThrowCubes()
    {
        if (throwCubes.ReadValue<float>() == 1)
        {
            List<GameObject> cubes = GetComponent<PlayerObjects>().cubes;
            Dictionary<Vector3,GameObject> cubeGrid = GetComponent<PlayerObjects>().cubesHash;
            foreach (GameObject cube in cubes)
            {
                cube.GetComponent<Faces>().resetFaces();
                if(cube != this.rb.gameObject){
                cube.gameObject.layer = 0;
                cube.transform.parent = this.transform.parent;
                GameObject.Destroy(cube.GetComponent<SphereCollider>());

                //Add rigidBody
                GetComponent<PlayerObjects>().addRigidBody(cube);


                cube.GetComponent<Rigidbody>().interpolation = RigidbodyInterpolation.Interpolate;
                cube.GetComponent<Rigidbody>().AddExplosionForce(explosionForce, this.rb.position, playerCharge);

                //Remove owner of cube
                StartCoroutine(blockNeutral(cube));
                }
                
            }
            cubes.Clear();
            cubes.Add(this.rb.gameObject);
            cubeGrid.Clear();
            cubeGrid.Add(Vector3.zero,this.rb.gameObject);

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


        foreach (GameObject obj in transform.GetComponent<PlayerObjects>().cubes)
        {
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
        foreach (GameObject obj in transform.GetComponent<PlayerObjects>().cubes)
        {
            center += obj.transform.position;
        }
        center = center / transform.GetComponent<PlayerObjects>().cubes.Count;
        center = rb.transform.InverseTransformPoint(center);
        return center;
       
    }
    public void CalculateCenterMassForce(Vector3 LocalForceDirection)
    {
        Vector3 centerMass = CalculateCenterMass();

        foreach (GameObject obj in transform.GetComponent<PlayerObjects>().cubes)
        {
            Rigidbody rbb = obj.GetComponent<Rigidbody>();
            float proportionalForce = (rbb.mass) * speed;
            rbb.AddForceAtPosition(LocalForceDirection * proportionalForce, centerMass, ForceMode.Force);
        }

    }
    public void addTorque(float rotation)
    {

        Vector3 pivotPoint = rb.position;
        rb.MoveRotation(rb.rotation * Quaternion.Euler(0, (speedRotation * rotation) / rotParam, 0));
        foreach (GameObject obj in transform.GetComponent<PlayerObjects>().cubes)
        {
            Rigidbody rbb = obj.GetComponent<Rigidbody>();
            Vector3 radiusVector = rbb.position - pivotPoint;
            Vector3 rot = Vector3.Cross(radiusVector, Vector3.up);
            rbb.AddForce(-rot * rotation * speedRotation );

        }
    }
    public void addTorqueSingle(float rotation)
    {
        rb.AddTorque(Vector3.up * rotation * speedRotation);
    }
    public void rotateAndDirection(Vector3 direction){
        
       Vector3 planeProjection = reff.transform.forward;
       float angle = Vector3.SignedAngle(planeProjection,direction.normalized,Vector3.up);
       Vector3 angularVelocity = Vector3.up * (angle * Mathf.Deg2Rad)* direction.magnitude*speedRotation;
        if (direction != Vector3.zero) {
            rb.AddTorque(angularVelocity);
            rb.AddTorque(-rb.angularVelocity * rotationDamping);
            reff.GetComponent<Rigidbody>().AddTorque(angularVelocity);
            reff.GetComponent<Rigidbody>().AddTorque(-reff.GetComponent<Rigidbody>().angularVelocity * rotationDamping);
        }


    }
    public void rotateAndDirection2(Vector3 direction,float rotationX,float rotationZ)
    {

        Vector3 planeProjection = reff.transform.forward;
        float angle = Vector3.SignedAngle(planeProjection, direction.normalized, Vector3.up);
        Quaternion quat1 = Quaternion.AngleAxis((angle * Mathf.Deg2Rad) * direction.magnitude * speedRotation, Vector3.up);

        Vector3 rotate2 = rRotate.ReadValue<Vector3>();
        Vector3 rotateR = new Vector3(rotate2.y, 0, -rotate2.x);
        Vector3 rotateRLocal = rb.transform.InverseTransformDirection(rotateR.normalized);
        Vector3 clampRLocal = Vector3.zero;

        Vector3 xProjection = Vector3.ProjectOnPlane(rb.transform.right, Vector3.up);
        Vector3 yProjection = Vector3.ProjectOnPlane(rb.transform.up, Vector3.up);
        Vector3 zProjection = Vector3.ProjectOnPlane(rb.transform.forward, Vector3.up);
        

        if (rotate2.sqrMagnitude > 0.2 && !holding)
        {
            if (Mathf.Abs(rotateRLocal.x) > Mathf.Abs(rotateRLocal.y) && Mathf.Abs(rotateRLocal.x) > Mathf.Abs(rotateRLocal.z))
            {
                if (rotX)
                {
                    clampRLocal = rb.transform.right * Mathf.Sign(rotateRLocal.x);
                    sign = Mathf.Sign(rotateRLocal.x);
                    holding = true;
                }
                else if(rotY)
                {
                    clampRLocal = rb.transform.up * Mathf.Sign(rotateRLocal.y);
                    adjust = true;
                }
                else if(rotZ)
                {
                    clampRLocal = rb.transform.forward * Mathf.Sign(rotateRLocal.z);
                    adjust = true;
                }
                else
                {
                    clampRLocal = rb.transform.right * Mathf.Sign(rotateRLocal.x);
                    rotX = true;
                }
            }
            else if (Mathf.Abs(rotateRLocal.y) > Mathf.Abs(rotateRLocal.x) && Mathf.Abs(rotateRLocal.y) > Mathf.Abs(rotateRLocal.z))
            {
                if (rotY)
                {
                    clampRLocal = rb.transform.up * Mathf.Sign(rotateRLocal.y);
                    sign = Mathf.Sign(rotateRLocal.y);
                    holding = true;
                }
                else if (rotX)
                {
                    
                    clampRLocal = rb.transform.right * Mathf.Sign(rotateRLocal.x);
                    adjust = true;
                }
                else if (rotZ)
                {
                    clampRLocal = rb.transform.forward * Mathf.Sign(rotateRLocal.z);
                    adjust = true;
                }
                else
                {
                    clampRLocal = rb.transform.up * Mathf.Sign(rotateRLocal.y);
                    rotY = true;
                }
            }
            else
            {
                if (rotZ)
                {
                    clampRLocal = rb.transform.forward * Mathf.Sign(rotateRLocal.z);
                    sign = Mathf.Sign(rotateRLocal.z);
                    holding = true;
                }
                else if (rotY)
                {
                    clampRLocal = rb.transform.up * Mathf.Sign(rotateRLocal.y);
                    adjust = true;
                }
                else if (rotX)
                {
                    clampRLocal = rb.transform.right * Mathf.Sign(rotateRLocal.x);
                    adjust = true;
                }
                else
                {
                    clampRLocal = rb.transform.forward * Mathf.Sign(rotateRLocal.z);
                    rotZ = true;
                }
            }
           
        }
        else if(rotate2.sqrMagnitude == 0)
        {
            holding = false;
            sign = 0;
        }
        if (holding )
        {
            if (rotX)
            {
                clampRLocal = rb.transform.right * sign;
            }
            if (rotY)
            {
                clampRLocal = rb.transform.up * sign;
            }
            if (rotZ)
            {
                clampRLocal = rb.transform.forward * sign;
            }
        }
        Quaternion quat3;

        Quaternion quat2 = Quaternion.AngleAxis( rotate2.magnitude * speedRotation, clampRLocal);
        quat3 = quat1 * quat2;
        rb.angularVelocity = QuaternionToAngularVelocity(quat3);
      
        if (adjust && (xProjection.magnitude <0.1 || zProjection.magnitude < 0.1) && rotY)
        {
            rotX = false;
            rotZ = false;
            rotY = false;
            if (zProjection.magnitude < 0.1)
            {
                rb.rotation = Quaternion.LookRotation(rb.transform.right, Vector3.up * Mathf.Sign(rb.transform.forward.y));
            }
            else
            {
                rb.rotation = Quaternion.LookRotation(rb.transform.forward, Vector3.up * Mathf.Sign(rb.transform.right.y));
            }
            adjust = false;
        }
        if (adjust && (yProjection.magnitude < 0.1 || zProjection.magnitude < 0.1) && rotX)
        {
            rotX = false;
            rotZ = false;
            rotY = false;
            if (yProjection.magnitude < 0.1)
            {
                rb.rotation = Quaternion.LookRotation(rb.transform.forward,Vector3.up * Mathf.Sign(rb.transform.up.y));
            }
            else
            {
                Debug.Log("wow"); 
                rb.rotation = Quaternion.LookRotation(rb.transform.up, Vector3.up * Mathf.Sign(rb.transform.forward.y));
            }
            adjust = false;
        }
        if (adjust && (xProjection.magnitude < 0.1 || yProjection.magnitude < 0.1) && rotZ)
        {
            rotX = false;
            rotZ = false;
            rotY = false;
            if (xProjection.magnitude < 0.1)
            {
                rb.rotation = Quaternion.LookRotation(rb.transform.up, Vector3.up * Mathf.Sign(rb.transform.right.y));
            }
            else
            {
                rb.rotation = Quaternion.LookRotation(rb.transform.right, Vector3.up * Mathf.Sign(rb.transform.up.y));
            }

            adjust = false;
        }
        
        reff.GetComponent<Rigidbody>().angularVelocity = QuaternionToAngularVelocity(quat1);
    }
    public void rotateXandZ(float xValue, float zValue){
        if(Mathf.Abs(xValue)>=Mathf.Abs(zValue)){
          rb.AddRelativeTorque(Vector3.right*xValue*10000);
        }else{
          rb.AddRelativeTorque(Vector3.forward*zValue*speedRotation* 10000);
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

