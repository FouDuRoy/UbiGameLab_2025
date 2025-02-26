using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.HID;
using UnityEngine.InputSystem.Switch;
using UnityEngine.ProBuilder;
using UnityEngine.UIElements;

public class PlayerMouvement : MonoBehaviour
{
    private const float collisionDistance = 0.1f;
    [SerializeField] float speed = 1f;
    [SerializeField] float speedRotation = 1f;
    [SerializeField] float playerCharge = 1000f;
    [SerializeField] float maxSpeed;
    [SerializeField] bool yLock = true;
    PlayerInput playerInput;
    InputAction moveAction;
    InputAction rotateAction;
    InputAction throwCubes;
    InputAction rotateActionZ;
    [SerializeField] Rigidbody rb;
    [SerializeField] float rotParam;
    [SerializeField] float fParam;
    float totalMass;
    

    void Start()
    {
        playerInput = GetComponent<PlayerInput>();
        moveAction = playerInput.actions.FindAction("Move");
        rotateAction = playerInput.actions.FindAction("Rotate");
        throwCubes = playerInput.actions.FindAction("ThrowCubes");
        rotateActionZ = playerInput.actions.FindAction("RotateZ");
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (!yLock)
        {
            rb.constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        }
        else
        {
            rb.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionY;
        }
        Vector2 direction = moveAction.ReadValue<Vector2>();
        float rotation = rotateAction.ReadValue<float>();
        Vector3 LocalForceDirection = new Vector3(direction.x, 0, direction.y).normalized;

        //Move and rotation
        CalculateCenterMassForce(LocalForceDirection);
        if (rotation == 0)
        {
            foreach (GameObject obj in transform.GetComponent<PlayerObjects>().cubes)
            {
               // Rigidbody rbb = obj.GetComponent<Rigidbody>();
               // rbb.AddTorque(-rbb.angularVelocity * 5000f,ForceMode.Acceleration);

            }
        }
        addTorque(rotation);

        //ThrowCube
        //ThrowCubes();
    }
   

    private void ThrowCubes()
    {
        if (throwCubes.ReadValue<float>() == 1)
        {

            List<GameObject> cubes = GetComponent<PlayerObjects>().cubes;

            foreach (GameObject cube in cubes)
            {
                cube.gameObject.layer = 0;
                cube.transform.parent = this.transform.parent;
                GameObject.Destroy(cube.GetComponent<SphereCollider>());

                //Add rigidBody
                cube.AddComponent<Rigidbody>();
                Rigidbody rb = cube.GetComponent<Rigidbody>();
                Rigidbody rb2 = GetComponent<PlayerObjects>().cubeRb;
                rb.mass = rb2.mass;
                rb.drag = rb2.drag;
                rb.angularDrag = rb2.angularDrag;
                rb.collisionDetectionMode = rb2.collisionDetectionMode;
                rb.useGravity = rb2.useGravity;
                rb.constraints = rb2.constraints;



                cube.GetComponent<Rigidbody>().interpolation = RigidbodyInterpolation.Interpolate;
                cube.GetComponent<Rigidbody>().AddExplosionForce(10000, this.transform.position, playerCharge);

                //Remove owner of cube
                StartCoroutine(blockNeutral(cube));
            }
            cubes.Clear();





        }
    }

    IEnumerator blockNeutral(GameObject block)
    {

        yield return new WaitForSeconds(3f);
        if(block !=null)
        {
            block.GetComponent<Cube>().setOwner("Neutral");
        }
       
    }

    public void ApplyEvenForce(Vector3 LocalForceDirection)
    {
        Vector3 center = Vector3.zero;
        float massSum = 0;
        foreach(Rigidbody rb in this.GetComponentsInChildren<Rigidbody>())
        {
            if(rb.mass > 0)
            {
                massSum += rb.mass;
            }
        }
        foreach (Rigidbody rb in this.GetComponentsInChildren<Rigidbody>())
        {
            if(rb.mass > 0)
            {
                rb.AddForce(LocalForceDirection * (speed * rb.mass / (massSum)), ForceMode.Force);
            }
        }
            
    }
    public void CalculateCenterMassForce(Vector3 LocalForceDirection)
    {
        Vector3 centerMass = CalculateCenterMass();

        foreach (GameObject obj in transform.GetComponent<PlayerObjects>().cubes)
        {
            Rigidbody rbb = obj.GetComponent<Rigidbody>();
            float proportionalForce = (rbb.mass) * speed;
            rbb.AddForceAtPosition(LocalForceDirection* proportionalForce, centerMass, ForceMode.Force); 
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
                center += rbb.worldCenterOfMass*rbb.mass;
                totalMass += rbb.mass;
            }
        }
        
        center = center / totalMass;
        return center;

    }
    public void addTorque(float rotation)
    {

        Vector3 pivotPoint = rb.position;
        if(yLock)
            rb.MoveRotation(rb.rotation * Quaternion.Euler(0, (speedRotation* rotation) / rotParam, 0));
        foreach (GameObject obj in transform.GetComponent<PlayerObjects>().cubes)
        {
            Rigidbody rbb = obj.GetComponent<Rigidbody>();
             //pivotPoint = rb.position;

            Vector3 radiusVector = rbb.position-pivotPoint;
              Vector3 rot = Vector3.Cross(radiusVector, Vector3.up);
              rbb.AddForce(-rot * rotation * speedRotation*fParam);
           
        }
    }
}

