using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.HID;
using UnityEngine.InputSystem.Switch;

public class PlayerMouvement : MonoBehaviour
{
    private const float collisionDistance = 0.1f;
    [SerializeField] float speed = 1f;
    [SerializeField] float speedRotation = 1f;
    [SerializeField] float playerCharge = 1000f;
    [SerializeField] float maxSpeed;
    PlayerInput playerInput;
    InputAction moveAction;
    InputAction rotateAction;
    InputAction throwCubes;
    InputAction rotateActionZ;
    [SerializeField] Rigidbody rb;
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
        Vector2 direction = moveAction.ReadValue<Vector2>();
        float rotation = rotateAction.ReadValue<float>();
        Vector3 LocalForceDirection = new Vector3(direction.x, 0, direction.y).normalized;

        //Move and rotation
        CalculateCenterMassForce(LocalForceDirection);
        addTorque(rotation);
        rb.AddForce(LocalForceDirection *speed, ForceMode.Force);

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

        float proportionalForce = (rb.mass) * speed;
        rb.AddForceAtPosition(LocalForceDirection * proportionalForce, centerMass, ForceMode.Force);

        foreach (GameObject obj in transform.GetComponent<PlayerObjects>().cubes)
        {
            Rigidbody rb = obj.GetComponent<Rigidbody>();
            proportionalForce = (rb.mass) * speed;
            rb.AddForceAtPosition(LocalForceDirection* proportionalForce, centerMass, ForceMode.Force); 
        }

    }
    public Vector3 CalculateCenterMass()
    {
        Vector3 center = Vector3.zero;
        totalMass = 0;
        totalMass += this.rb.mass;
        center += transform.position * this.rb.mass;

        foreach (GameObject obj in transform.GetComponent<PlayerObjects>().cubes)
        {
            Rigidbody rb = obj.GetComponent<Rigidbody>();

            if (rb.mass > 0)
            {
                center += rb.worldCenterOfMass*rb.mass;
                totalMass += rb.mass;
            }
        }
        
        center = center / totalMass;
        return center;

    }
    public void addTorque(float rotation)
    {
        
         rb.MoveRotation(rb.rotation*Quaternion.Euler(0, rotation*speedRotation/4, 0));

        Vector3 pivotPoint = rb.position;

        foreach (GameObject obj in transform.GetComponent<PlayerObjects>().cubes)
        {
           Rigidbody wow = obj.GetComponent<Rigidbody>();
            
              Vector3 radiusVector = wow.position-pivotPoint;
              Vector3 rot = Vector3.Cross(radiusVector, Vector3.up).normalized;
              wow.AddForce(-rot * rotation * speedRotation);
           
        }
    }
}

