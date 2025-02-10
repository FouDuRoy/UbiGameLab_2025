using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStateMachine : StateMachine
{
    [field: SerializeField] public CharacterController Controller { get; private set; }
    [field: SerializeField] public InputReader inputReader { get; private set; }
    [field: SerializeField] public float MovementSpeed { get; private set; }
    private void Start()
    {
        SwitchState(new PlayerTestState(this));
    }

}
