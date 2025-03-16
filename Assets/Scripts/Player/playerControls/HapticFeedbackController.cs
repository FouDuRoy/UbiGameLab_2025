using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class HapticFeedbackController : MonoBehaviour
{
    private Gamepad playerGamepad;
    private Coroutine attractionCoroutine;
    private Coroutine impulseCoroutine;
    private Coroutine repulsionCoroutine;

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


    public void BlocAttachedVibration()
    {
        if (playerGamepad != null)
        {
            if (impulseCoroutine != null)
            {
                StopCoroutine(impulseCoroutine);
            }
            impulseCoroutine=StartCoroutine(ImpulseVibration(.5f, 0, .5f, .02f, .06f));
        }
    }

    private IEnumerator ImpulseVibration(float leftMotorMax, float leftMidTime, float rightMotorMax, float rightMidTime, float totalDuration)
    {
        leftMidTime = Mathf.Min(leftMidTime, totalDuration);
        rightMidTime = Mathf.Min(rightMidTime, totalDuration);

        float elapsed = 0f;

        // Phase de montée
        while (elapsed < Mathf.Max(leftMidTime, rightMidTime))
        {
            float leftProgress = leftMidTime > 0 ? Mathf.Clamp01(elapsed / leftMidTime) : 1f;
            float rightProgress = rightMidTime > 0 ? Mathf.Clamp01(elapsed / rightMidTime) : 1f;

            float leftMotor = Mathf.Lerp(0, leftMotorMax, leftProgress);
            float rightMotor = Mathf.Lerp(0, rightMotorMax, rightProgress);

            playerGamepad.SetMotorSpeeds(leftMotor, rightMotor);

            elapsed += Time.deltaTime;
            yield return null;
        }

        // Phase de descente
        float remainingTime = totalDuration - Mathf.Max(leftMidTime, rightMidTime);
        elapsed = 0f;

        while (elapsed < remainingTime)
        {
            float progress = elapsed / remainingTime;

            float leftMotor = Mathf.Lerp(leftMotorMax, 0, progress);
            float rightMotor = Mathf.Lerp(rightMotorMax, 0, progress);

            playerGamepad.SetMotorSpeeds(leftMotor, rightMotor);

            elapsed += Time.deltaTime;
            yield return null;
        }

        // Arrêt complet
        playerGamepad.SetMotorSpeeds(0f, 0f);
    }

    public void RepulsionVibrationStart(float maxChargeTime)
    {
        if (playerGamepad != null)
        {
            if (repulsionCoroutine != null)
            {
                StopCoroutine(repulsionCoroutine);
            }
            attractionCoroutine = StartCoroutine(VibrationTransition(0, 1f, 0, 0.2f, maxChargeTime, true));
        }
    }

    public void RepulsionVibrationEnd()
    {
        if (playerGamepad != null)
        {
            if (repulsionCoroutine != null)
            {
                StopCoroutine(repulsionCoroutine);
                repulsionCoroutine = null;
            }
            StartCoroutine(ImpulseVibration(1,.07f,1,.07f,2));
        }
    }

    public void AttractionVibrationStart()
    {
        if (playerGamepad != null)
        {
            if (attractionCoroutine != null)
            {
                StopCoroutine(attractionCoroutine); 
            }
            attractionCoroutine = StartCoroutine(VibrationTransition(0, 0.1f, 0, 0.5f, .5f, true));
        }
    }

    public void AttractionVibrationEnd()
    {
        if (playerGamepad != null)
        {
                if (attractionCoroutine != null)
                {
                    StopCoroutine(attractionCoroutine);
                    attractionCoroutine = null;
                }
            StartCoroutine(VibrationTransition(0, 0.1f, 0, 0.5f, .2f, false));
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

     IEnumerator VibrationTransition(float leftMotorMin, float leftMotorMax, float rightMotorMin, float rightMotorMax, float duration, bool crescendo)
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
            yield return null;

            elapsed += Time.deltaTime;
        }

        if (!crescendo)
        {
            playerGamepad.SetMotorSpeeds(0, 0);
        }
    }
}
