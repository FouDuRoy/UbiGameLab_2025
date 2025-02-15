using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
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
            foreach (Transform child in this.transform)
            {
                if (child.GetComponent<Cube>() != null)
                {

                    if (child.GetComponent<Cube>().owner.Equals(this.gameObject.name))
                    {

                        //child.gameObject.layer = 4;
                        child.parent = this.transform.parent;

                        GameObject.Destroy(child.GetChild(0).gameObject);
                        child.AddComponent<Rigidbody>();
                        Rigidbody rb = child.GetComponent<Rigidbody>();
                        rb.mass = 10f;
                        rb.drag = 0.5f;
                        rb.angularDrag = 0.5f;
                        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
                        rb.useGravity = false;
                        child.GetComponentInChildren<Rigidbody>().isKinematic = false;
                        child.GetComponentInChildren<Rigidbody>().interpolation = RigidbodyInterpolation.Interpolate;
                        child.GetComponentInChildren<Rigidbody>().AddExplosionForce(10000, this.transform.position, playerCharge);
                        //Remove owner of cube
                        //Debug.Log("yeah");
                        StartCoroutine(blockNeutral(child.gameObject));



                    }
                }




            }

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
    void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.tag=="Scene") {
            foreach (ContactPoint contact in collision.contacts)
            {
                // The contact.normal is the normal of the surface we collided with
                Vector3 collisionNormal = contact.normal.normalized;
                this.gameObject.GetComponent<Rigidbody>().velocity = Vector3.zero;
                //this.transform.position += collisionNormal* collisionDistance;
            }
        }
      

    }
}

