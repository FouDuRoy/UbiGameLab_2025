using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputAssigner : MonoBehaviour
{
    [SerializeField] PlayerInput player1Input;
    [SerializeField] PlayerInput player2Input;

    void Start()
    {
        var gamepads = Gamepad.all;

        if (gamepads.Count >= 2)
        {
            player1Input.SwitchCurrentControlScheme(gamepads[0]);
            player2Input.SwitchCurrentControlScheme(gamepads[1]);
        }
        else
        {
            Debug.LogWarning("Pas assez de manettes connectées !");
        }
    }
}
