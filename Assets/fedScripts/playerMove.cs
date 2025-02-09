using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMouvement : MonoBehaviour
{
    [SerializeField] float speed = 1f;
    [SerializeField] float speedRotation = 1f;
    PlayerInput playerInput;
    InputAction moveAction;
    InputAction rotateAction;
    void Start()
    {
        playerInput = GetComponent<PlayerInput>();
        moveAction = playerInput.actions.FindAction("Move");
        rotateAction = playerInput.actions.FindAction("Rotate");
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 direction = moveAction.ReadValue<Vector2>();
        float rotation = rotateAction.ReadValue<float>();
        
        transform.position += new Vector3(direction.x, 0, direction.y)*speed*Time.deltaTime; 
        transform.Rotate(Vector3.up,rotation*speedRotation*Time.deltaTime);
        
    }
}
