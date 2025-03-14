using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class HapticFeedbackController : MonoBehaviour
{
    private Gamepad playerGamepad;

    void Start()
    {
        // Récupérer la manette liée à ce joueur (nécessite un script qui gère l'attribution des manettes)
        var playerInput = GetComponent<PlayerInput>(); // Assurez-vous que chaque joueur a un PlayerInput
        if (playerInput != null && playerInput.devices.Count > 0)
        {
            playerGamepad = playerInput.devices[0] as Gamepad;
        }

        if (playerGamepad == null)
        {
            Debug.LogError($"Aucune manette trouvée pour {gameObject.name}");
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
