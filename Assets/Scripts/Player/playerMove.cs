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
    [SerializeField] float rotParam;
    [SerializeField] MouvementType moveType;
    PlayerInput playerInput;
    InputAction moveAction;
    InputAction rotateAction;
    InputAction throwCubes;
    InputAction rotateActionZ;
    Rigidbody rb;
    float totalMass;
    


    void Start()
    {
        playerInput = GetComponent<PlayerInput>();
        moveAction = playerInput.actions.FindAction("Move");
        rotateAction = playerInput.actions.FindAction("Rotate");
        throwCubes = playerInput.actions.FindAction("ThrowCubes");
        rotateActionZ = playerInput.actions.FindAction("RotateZ");
        rb = this.GetComponent<PlayerObjects>().cubeRb;
        if (moveType == MouvementType.rigidBody)
        {
            rb.centerOfMass = Vector3.zero;
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        Vector2 direction2 = moveAction.ReadValue<Vector2>();
        Vector3 direction = new Vector3(direction2.x, 0, direction2.y);
        float rotation = rotateAction.ReadValue<float>();
        float rotationZ = rotateActionZ.ReadValue<float>();
    
        if (moveType == MouvementType.rigidBody)
        {
            RigidBodyMouvement(direction, rotation);
        }
        else if (moveType == MouvementType.spring)
        {
            rb.AddForceAtPosition(direction * speed, CalculateCenterMass());
            rb.AddTorque(Vector3.up * rotation * speedRotation,ForceMode.Force);
        }
        else if (moveType == MouvementType.transform)
        {
            TranslateMouvement(direction, rotation);
        }

        ThrowCubes();


    }

    private void TranslateMouvement(Vector3 direction, float rotation)
    {
        transform.position += direction * speed * Time.fixedDeltaTime;
        transform.Rotate(Vector3.up, rotation * speedRotation * Time.fixedDeltaTime, Space.World);
    }
    private void RigidBodyMouvement(Vector3 direction, float rotation) {
        rb.AddForce(direction * speed);
        rb.AddTorque(Vector3.up * rotation * speedRotation);
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
       // Debug.Log(center);
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

}

