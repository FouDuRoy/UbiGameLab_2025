using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerTestState : PlayerBaseState
{
    public PlayerTestState(PlayerStateMachine stateMachine) : base(stateMachine) {}

    public override void Enter()
    {
        stateMachine.inputReader.ActionEvent += OnAction;
    }
    public override void Tick(float deltaTime)
    {
        Vector3 movement = new Vector3();

        movement.x = stateMachine.inputReader.MovementValue.x;
        movement.y = 0;
        movement.z = stateMachine.inputReader.MovementValue.y;

        stateMachine.Controller.Move(movement * deltaTime * stateMachine.MovementSpeed);

        if (stateMachine.inputReader.MovementValue == Vector2.zero) { return; }

        stateMachine.transform.rotation = Quaternion.LookRotation(movement);

    }
    public override void Exit()
    {
        stateMachine.inputReader.ActionEvent -= OnAction;
    }

    
    private void OnAction()
    {
        stateMachine.SwitchState(new PlayerTestState(stateMachine));
    }
    


}
