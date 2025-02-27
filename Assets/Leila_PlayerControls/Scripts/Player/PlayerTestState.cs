using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerTestState : PlayerBaseState
{
    public PlayerTestState(PlayerStateMachine stateMachine) : base(stateMachine) {}

    public override void Enter()
    {
        stateMachine.inputReader.ThrowCubeEvent += OnThrow;
    }
    public override void Tick(float deltaTime)
    {
        Vector3 movement = new Vector3();

        movement.x = stateMachine.inputReader.MovementValue.x;
        movement.y = 0;
        movement.z = stateMachine.inputReader.MovementValue.y;

        stateMachine.Controller.Move(movement * deltaTime * stateMachine.MovementSpeed);


        if (stateMachine.inputReader.MovementValue == Vector2.zero) { return; }

        // fed deplacement x et y
        //Vector2 direction = moveAction.ReadValue<Vector2>();
        //transform.position += new Vector3(direction.x, 0, direction.y) * speed * Time.deltaTime;
        //fed rotation x et y

        //float rotation = rotateAction.ReadValue<float>();

        float rotation = stateMachine.inputReader.rotationValue;

        stateMachine.transform.Rotate(Vector3.up, rotation * Time.deltaTime, Space.World);

        /*
        Vector3 rotation = new Vector3();
        rotation.x = 0;
        rotation.y = stateMachine.inputReader.rotationValue;
        rotation.z = 0;
        stateMachine.transform.rotation = Quaternion.LookRotation(rotation);
        
        //stateMachine.transform.Rotate(Vector3.up, rotation * deltaTime);
        */


    }
    public override void Exit()
    {
        stateMachine.inputReader.ThrowCubeEvent -= OnThrow;
    }

    
    private void OnThrow()
    {
        stateMachine.SwitchState(new PlayerTestState(stateMachine));
    }
    


}
