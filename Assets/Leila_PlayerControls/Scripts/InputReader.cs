using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputReader : MonoBehaviour, Controls.IPlayerActions
{
    public Vector2 MovementValue { get; private set; }
    public float rotationValue { get; private set; }
    
    public event Action ThrowCubeEvent;
    private Controls controls;

    [SerializeField] float playerCharge = 1000f;

    private void Start()
    {
        controls = new Controls();
        controls.Player.SetCallbacks(this);

        controls.Player.Enable();
    }

    private void OnDestroy()
    {
        controls.Player.Disable();
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        MovementValue = context.ReadValue<Vector2>();
    }

    public void OnRotate(InputAction.CallbackContext context)
    {
        rotationValue = context.ReadValue<float>();
    }

    public void OnThrowCubes(InputAction.CallbackContext context)
    {
        if (context.performed) { return; }

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

        ThrowCubeEvent?.Invoke();

        Debug.Log("testing_X_button");
    }
}
