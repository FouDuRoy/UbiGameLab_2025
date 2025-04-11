using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;

public class PlayerInputAssigner : MonoBehaviour
{
    [SerializeField] GameObject player1;
    [SerializeField] GameObject player2;
    public InputActionAsset map1;
    public InputActionAsset map2;
    void Awake()
    {
        QualitySettings.vSyncCount = 1;
        Application.targetFrameRate = 144;
        var gamepads = Gamepad.all;
        if (gamepads.Count >= 2)
        {

            PlayerInput player1Input = player1.GetComponent<PlayerInput>();
            player1Input.actions = map1;
            player1Input.SwitchCurrentActionMap("Mouvement");
            player1Input.neverAutoSwitchControlSchemes = true;
            player1Input.SwitchCurrentControlScheme("Player1", gamepads[0]);
            InputUser.PerformPairingWithDevice(gamepads[0], player1Input.user);
            player1Input.SwitchCurrentActionMap("Mouvement");
            player1Input.actions.FindActionMap("Mouvement").Enable();


        


            PlayerInput player2Input = player2.GetComponent<PlayerInput>();
            player2Input.actions = map2;
            player2Input.SwitchCurrentActionMap("Mouvement1");
            player2Input.neverAutoSwitchControlSchemes = true;
            player2Input.SwitchCurrentControlScheme("Player2", gamepads[1]);
            InputUser.PerformPairingWithDevice(gamepads[1], player2Input.user);
            player2Input.actions.FindActionMap("Mouvement1").Enable();


    

          
        }
        else
        {
            Debug.LogWarning("Pas assez de manettes connectées !");
        }
    }
   
}
 


