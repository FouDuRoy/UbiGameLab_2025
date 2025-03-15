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

        playerGamepad.SetMotorSpeeds(0, 0);
        //TriggerRampVibration(.2f, 1, .2f, .1f, 10);
    }

    public void AttractionVibration()
    {
        playerGamepad.SetMotorSpeeds(0.2f, 0);
    }

    public void StopVibrations()
    {
        playerGamepad.SetMotorSpeeds(0, 0);
    }

    public void TriggerRampVibration(float leftMotorMin, float leftMotorMax, float rightMotorMin, float rightMotorMax, float duration)
    {
        if (playerGamepad != null)
        {
            StartCoroutine(RampVibration(leftMotorMin, leftMotorMax, rightMotorMin, rightMotorMax, duration));
        }
    }

    private IEnumerator RampVibration(float leftMotorMin, float leftMotorMax, float rightMotorMin, float rightMotorMax, float duration)
    {
        float halfDuration = duration / 2f;
        float elapsed = 0f;

        // Phase de mont�e (0 � moiti� du temps)
        while (elapsed < halfDuration)
        {
            float progress = elapsed / halfDuration; // Entre 0 et 1
            float leftMotor = Mathf.Lerp(leftMotorMin, leftMotorMax, progress);
            float rightMotor = Mathf.Lerp(rightMotorMin, rightMotorMax, progress);
            playerGamepad.SetMotorSpeeds(leftMotor, rightMotor);

            elapsed += Time.deltaTime;
            yield return null;
        }

        // Phase de descente (moiti� � fin)
        elapsed = 0f;
        while (elapsed < halfDuration)
        {
            float progress = elapsed / halfDuration; // Entre 0 et 1
            float leftMotor = Mathf.Lerp(leftMotorMax, leftMotorMin, progress);
            float rightMotor = Mathf.Lerp(rightMotorMax, rightMotorMin, progress);
            playerGamepad.SetMotorSpeeds(leftMotor, rightMotor);

            elapsed += Time.deltaTime;
            yield return null;
        }

        // Arr�t complet de la vibration
        playerGamepad.SetMotorSpeeds(0, 0);
    }
}
