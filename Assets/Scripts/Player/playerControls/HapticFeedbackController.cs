using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class HapticFeedbackController : MonoBehaviour
{
    private Gamepad playerGamepad;

    void Start()
    {
        // R�cup�rer la manette li�e � ce joueur (n�cessite un script qui g�re l'attribution des manettes)
        var playerInput = GetComponent<PlayerInput>(); // Assurez-vous que chaque joueur a un PlayerInput
        if (playerInput != null && playerInput.devices.Count > 0)
        {
            playerGamepad = playerInput.devices[0] as Gamepad;
        }

        if (playerGamepad == null)
        {
            Debug.LogError($"Aucune manette trouv�e pour {gameObject.name}");
        }

        TriggerVibration(1f, .0f,.10f);
    }

    public void TriggerVibration(float leftMotor, float rightMotor, float duration)
    {
        if (playerGamepad != null)
        {
            playerGamepad.SetMotorSpeeds(leftMotor, rightMotor);
            StartCoroutine(StopVibration(duration));
        }
    }

    private IEnumerator StopVibration(float duration)
    {
        yield return new WaitForSeconds(duration);
        if (playerGamepad != null)
        {
            playerGamepad.SetMotorSpeeds(0f, 0f);
        }
    }
}
