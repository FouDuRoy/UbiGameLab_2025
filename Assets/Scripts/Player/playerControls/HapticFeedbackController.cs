using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class HapticFeedbackController : MonoBehaviour
{
    private Gamepad playerGamepad;

    public bool gauche;

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

        if (playerGamepad != null)
        {
            playerGamepad.SetMotorSpeeds(0, 0);
        }
    }

    public void AttractionVibrationStart()
    {
        if (playerGamepad != null)
        {
            StartCoroutine(VibrationTransition(0, 0, 0, .5f, .5f, true));
        }
    }

    public void AttractionVibrationEnd()
    {
        if (playerGamepad != null)
        {
            StartCoroutine(VibrationTransition(0, 0, 0, .5f, .2f, false));
        }
    }

    public void EjectionVibrationStart()
    {
        if (playerGamepad != null)
        {
            StartCoroutine(VibrationTransition(0, 1f, 0, 0, .2f, true));
        }
    }

    public void EjectionVibrationEnd()
    {
        if (playerGamepad != null)
        {
            StartCoroutine(VibrationTransition(0, 1f, 0, 0, .05f, false));
        }
    }

    public void StopVibrations()
    {
        playerGamepad.SetMotorSpeeds(0, 0);
    }

    private IEnumerator VibrationTransition(float leftMotorMin, float leftMotorMax, float rightMotorMin, float rightMotorMax, float duration, bool crescendo)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float progress = elapsed / duration; // Progression entre 0 et 1
            float leftMotor, rightMotor;

            if (crescendo)
            {
                leftMotor = Mathf.Lerp(leftMotorMin, leftMotorMax, progress);
                rightMotor = Mathf.Lerp(rightMotorMin, rightMotorMax, progress);
            }
            else
            {
                leftMotor = Mathf.Lerp(leftMotorMax, leftMotorMin, progress);
                rightMotor = Mathf.Lerp(rightMotorMax, rightMotorMin, progress);
            }

            playerGamepad.SetMotorSpeeds(leftMotor, rightMotor);

            elapsed += Time.deltaTime;
            yield return null;
        }
    }
}
