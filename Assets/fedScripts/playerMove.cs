using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Switch;

public class PlayerMouvement : MonoBehaviour
{
    [SerializeField] float speed = 1f;
    [SerializeField] float speedRotation = 1f;
    [SerializeField] float playerCharge = 1000f;
    PlayerInput playerInput;
    InputAction moveAction;
    InputAction rotateAction;
    InputAction throwCubes;
    InputAction rotateActionZ;

    void Start()
    {
        playerInput = GetComponent<PlayerInput>();
        moveAction = playerInput.actions.FindAction("Move");
        rotateAction = playerInput.actions.FindAction("Rotate");
        throwCubes = playerInput.actions.FindAction("ThrowCubes");
        rotateActionZ = playerInput.actions.FindAction("RotateZ");

    }

    // Update is called once per frame
    void Update()
    {
        Vector2 direction = moveAction.ReadValue<Vector2>();
        float rotation = rotateAction.ReadValue<float>();
        float rotationZ = rotateActionZ.ReadValue<float>();
        transform.position += new Vector3(direction.x, 0, direction.y) * speed * Time.deltaTime;
        transform.Rotate(Vector3.up, rotation * speedRotation * Time.deltaTime,Space.World);
       
        transform.Rotate(Vector3.right, rotationZ * speedRotation * Time.deltaTime, Space.World);
       

        if (throwCubes.ReadValue<float>()==1)
        {
            foreach (Transform child in this.transform)
            {
                if (child.tag != "Player")
                {
                    child.GetComponentInChildren<Rigidbody>().isKinematic = false;
                    child.GetComponentInChildren<Rigidbody>().interpolation = RigidbodyInterpolation.Interpolate;
                    child.gameObject.layer = 0;
                    child.parent = this.transform.parent;
                    GameObject.Destroy(child.GetChild(0).gameObject);
                    
                    child.GetComponentInChildren<Rigidbody>().AddExplosionForce(10000, this.transform.position, playerCharge);

                }

            }

        }

    }
}

