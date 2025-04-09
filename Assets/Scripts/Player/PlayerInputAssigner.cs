using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;

public class PlayerInputAssigner : MonoBehaviour
{
    [SerializeField] GameObject player1;
    [SerializeField] GameObject player2;
    public InputActionAsset map;
    public GameObject dynamicCamera;
    void Awake()
    {
        var gamepads = Gamepad.all;
        if (gamepads.Count >= 2)
        {

            PlayerInput player1Input = player1.AddComponent<PlayerInput>();
            player1Input.actions = map;
            player1Input.SwitchCurrentActionMap("Mouvement");
            player1Input.neverAutoSwitchControlSchemes = true;
            player1Input.SwitchCurrentControlScheme("Player1", gamepads[0]);
            InputUser.PerformPairingWithDevice(gamepads[0], player1Input.user);
            player1Input.SwitchCurrentActionMap("Mouvement");
            player1Input.actions.FindActionMap("Mouvement").Enable();


            player1Input.GetComponent<PlayerMouvement>().enabled = true;
            player1Input.GetComponent<ConeEjectionAndProjection>().enabled = true;
            player1Input.GetComponent<HapticFeedbackController>().enabled = true;



            PlayerInput player2Input = player2.AddComponent<PlayerInput>();
            player2Input.actions = Instantiate(map);
            player2Input.SwitchCurrentActionMap("Mouvement1");
            player2Input.neverAutoSwitchControlSchemes = true;
            player2Input.SwitchCurrentControlScheme("Player2", gamepads[1]);
            InputUser.PerformPairingWithDevice(gamepads[1], player2Input.user);
            player2Input.actions.FindActionMap("Mouvement1").Enable();


            player2Input.GetComponent<PlayerMouvement>().enabled = true;
            player2Input.GetComponent<ConeEjectionAndProjection>().enabled = true;
            player2Input.GetComponent<HapticFeedbackController>().enabled = true;
            if(dynamicCamera != null)
            {
                dynamicCamera.SetActive(true);
            }
        }
        else
        {
            Debug.LogWarning("Pas assez de manettes connectées !");
        }
    }
}
 


