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
        
        
       
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 direction = moveAction.ReadValue<Vector2>();
        float rotation = rotateAction.ReadValue<float>();
        float rotationZ = rotateActionZ.ReadValue<float>();
        transform.position += new Vector3(direction.x, 0, direction.y) * speed * Time.deltaTime;
        transform.Rotate(Vector3.up, rotation * speedRotation * Time.deltaTime, Space.World);
        //rb.MovePosition(rb.position+ new Vector3(direction.x, 0, direction.y) * speed * Time.deltaTime);
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
            block.GetComponent<Bloc>().setOwner("Neutral");
        }
       
    }

}

