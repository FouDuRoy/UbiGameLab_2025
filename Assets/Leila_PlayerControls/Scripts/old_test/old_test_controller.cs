/*
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputReader : MonoBehaviour //, Controls.IPlayerActions
{

    [Header("Input Action Asset")]
    [SerializeField] private InputActionAsset playerControls;

    //reference to action map
    [Header("Action Map Name References")]
    [SerializeField] private String actionMapName = "Player";

    [Header("Action Name References")]
    [SerializeField] private string action = "Action";
    [SerializeField] private string move = "Move";

    private InputAction actionAction;
    private InputAction moveAction;

    public Vector2 moveInput { get; private set; }
    public bool actionTriggered { get; private set; }

    public static PlayerInputReader Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            //dont destroy if this already existrs
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        moveAction = playerControls.FindActionMap(actionMapName).FindAction(move);
        actionAction = playerControls.FindActionMap(actionMapName).FindAction(action);
    }

    void RegisterInputActions()
    {
        moveAction.performed += context => moveInput = context.ReadValue<Vector2>();
        moveAction.canceled += context => moveInput = Vector2.zero;

        actionAction.performed += context => actionTriggered = true;
        actionAction.canceled += context => actionTriggered = false;
    }

    private void OnEnable()
    {
        moveAction.Enable();
        actionAction.Enable();
    }

    private void OnDisable()
    {
        moveAction.Disable();
        actionAction.Disable();
    }


}

*/