using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using System.Security.Cryptography;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.HID;
using UnityEngine.InputSystem.Switch;
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
    Rigidbody rb;
    float totalMass;
    


    void Start()
    {
        playerInput = GetComponent<PlayerInput>();
        moveAction = playerInput.actions.FindAction("Move");
        rotateAction = playerInput.actions.FindAction("Rotate");
        throwCubes = playerInput.actions.FindAction("ThrowCubes");
        rotateActionZ = playerInput.actions.FindAction("RotateZ");
        rotateActionX = playerInput.actions.FindAction("RotateX");
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
       // planeOrientation.transform.rotation = Quaternion.Euler(0,rb.rotation.eulerAngles.y,0);
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
           // rotateAndDirection(direction);
            //rotateXandZ(rotationX,rotationZ);
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
         Vector3 angularVelocity = Vector3.up * (angle * Mathf.Deg2Rad) * direction.magnitude * speedRotation;
        Quaternion quat1 = Quaternion.AngleAxis((angle * Mathf.Deg2Rad) * direction.magnitude * speedRotation, Vector3.up);
        if (Mathf.Abs(rotationX) >= Mathf.Abs(rotationZ))
        {
            Quaternion quat2 = Quaternion.AngleAxis(rotationX * speedRotation, rb.transform.right);
            //Quaternion quat3 = quat1 * rb.rotation * quat2;
        
            Quaternion quat3 = quat1 * quat2;
            rb.angularVelocity = QuaternionToAngularVelocity(quat3);
        }
        else
        {
            Quaternion quat2 = Quaternion.AngleAxis(rotationZ * speedRotation, rb.transform.forward);
            //Quaternion quat3 = quat1 * rb.rotation * quat2;
            Quaternion quat3 = quat1 * quat2;
            rb.angularVelocity = QuaternionToAngularVelocity(quat3);
        }
        //reff.GetComponent<Rigidbody>().MoveRotation(quat1 * reff.GetComponent<Rigidbody>().rotation);
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

