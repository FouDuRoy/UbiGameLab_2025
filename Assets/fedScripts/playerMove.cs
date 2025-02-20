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
    Rigidbody rb;
    

    void Start()
    {
        playerInput = GetComponent<PlayerInput>();
        moveAction = playerInput.actions.FindAction("Move");
        rotateAction = playerInput.actions.FindAction("Rotate");
        throwCubes = playerInput.actions.FindAction("ThrowCubes");
        rotateActionZ = playerInput.actions.FindAction("RotateZ");
        rb = GetComponent<Rigidbody>();
        rb.solverIterations = 40;
        rb.solverVelocityIterations = 20;


    }

    // Update is called once per frame
    void FixedUpdate()
    {
        Vector2 direction = moveAction.ReadValue<Vector2>();
        float rotation = rotateAction.ReadValue<float>();
        float rotationZ = rotateActionZ.ReadValue<float>();
        Vector3 LocalForceDirection = new Vector3(direction.x, 0, direction.y).normalized;
        Vector3 Rotation = rb.rotation.eulerAngles;



        //transform.position += new Vector3(direction.x, 0, direction.y) * speed * Time.deltaTime;
        //transform.Rotate(Vector3.up, rotation * speedRotation * Time.deltaTime, Space.World);
        // rb.AddTorque(Vector3.up*rotation * speedRotation);
        // rb.MoveRotation(Quaternion.Euler(rotation * (Rotation + new Vector3(0, speedRotation, 0))));
        //rb.MovePosition(rb.position+ new Vector3(direction.x, 0, direction.y) * speed * Time.deltaTime);
        //rb.AddForce(LocalForceDirection * (speed));
        //Debug.Log(LocalForceDirection);
       // rb.AddForceAtPosition(LocalForceDirection * (speed), this.transform.InverseTransformPoint(CalculateCenterMass()), ForceMode.Force);
        //if (rb.velocity.magnitude > maxSpeed)
        //{
        // rb.velocity = rb.velocity.normalized * maxSpeed;
        //}
        ApplyEvenForce(LocalForceDirection );
        //Quaternion rotationQ = Quaternion.Euler(Vector3.up * rotation * speedRotation * Time.deltaTime
        // + rb.rotation.ToEuler());
        // rb.MoveRotation(rotationQ);
        //rb.MoveRotation(Quaternion.this.transform.up, rotationZ * speedRotation * Time.deltaTime, Space.World);


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
               // Debug.Log(rb.transform.localPosition.magnitude);
                rb.AddForce(LocalForceDirection * (speed * rb.mass / (massSum+(rb.transform.localPosition.magnitude)*1f)), ForceMode.Force);
            }
        }
            
    }
    public void CalculateCenterMassForce()
    {
        Vector3 center = Vector3.zero;
        float massSum = 0;
        foreach (Rigidbody rb in this.GetComponentsInChildren<Rigidbody>())
        {
            if (rb.mass > 0)
            {
                center += rb.centerOfMass;
                massSum += rb.mass;
            }
        }
        center = center / massSum;
        Vector3 closest = this.transform.position;
        foreach(Rigidbody rb in this.GetComponentsInChildren<Rigidbody>())
        {

        }
       

    }
    public Vector3 CalculateCenterMass()
    {
        Vector3 center = Vector3.zero;
        float massSum = 0;
        foreach (Rigidbody rb in this.GetComponentsInChildren<Rigidbody>())
        {
            if (rb.mass > 0)
            {
                center += rb.centerOfMass;
                massSum += rb.mass;
                rb.solverIterations = 40;
                rb.solverVelocityIterations = 20;
            }
        }
        center = center / massSum;
        return center;


    }
}

