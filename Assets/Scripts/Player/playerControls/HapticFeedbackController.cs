using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class HapticFeedbackController : MonoBehaviour
{
    [System.Serializable]
    public struct HoldHapticPattern
    {
        public float leftMotorMin;
        public float leftMotorMax;
        public float rightMotorMin;
        public float rightMotorMax;
        public float duration;
    }

    [System.Serializable]
    public struct ImpulseHapticPattern
    {
        public float leftMotorMax;
        public float leftMidTime;
        public float rightMotorMax;
        public float rightMidTime;
        public float totalDuration;
    }

    public ImpulseHapticPattern blocAttached = new ImpulseHapticPattern
    {
        leftMotorMax=.5f,
        leftMidTime=0,
        rightMotorMax=.5f,
        rightMidTime=.02f,
        totalDuration=.06f,
    };
    public HoldHapticPattern attraction =new HoldHapticPattern
    {
        leftMotorMin=0,
        leftMotorMax=.1f,
        rightMotorMin=0,
        rightMotorMax=.5f,
        duration=.2f,
    };

    public HoldHapticPattern repulsionCharge=new HoldHapticPattern
    {
        leftMotorMin=0,
        leftMotorMax=.0f,
        rightMotorMin=0,
        rightMotorMax=.0f,
        duration=.5f,
    };
    public ImpulseHapticPattern repulsionShoot = new ImpulseHapticPattern
    {
        leftMotorMax = .5f,
        leftMidTime = 0,
        rightMotorMax = .5f,
        rightMidTime = 0,
        totalDuration = .5f,
    };
    private HoldHapticPattern ejectionCharge=new HoldHapticPattern // Deprecated
    {
        leftMotorMin=0,
        leftMotorMax=.1f,
        rightMotorMin=0,
        rightMotorMax=.5f,
        duration=.5f,
    };
    public ImpulseHapticPattern ejectionShoot = new ImpulseHapticPattern
    {
        leftMotorMax = .5f,
        leftMidTime = 0,
        rightMotorMax = .5f,
        rightMidTime = .02f,
        totalDuration = .06f,
    };
    public ImpulseHapticPattern damageTaken = new ImpulseHapticPattern
    {
        leftMotorMax = .2f,
        leftMidTime = .5f,
        rightMotorMax = .8f,
        rightMidTime = .5f,
        totalDuration = 0f,
    };

    private PlayerInfo playerInfo;

    private Gamepad playerGamepad;
    private Coroutine attractionCoroutine;
    private Coroutine blocAttachedCoroutine;
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
            Debug.LogError($"Aucune manette trouvée pour {gameObject.name}");
            enabled = false;
        }

        if (playerGamepad != null)
        {
            playerGamepad.SetMotorSpeeds(0, 0);
        }

        playerInfo = GetComponent<PlayerInfo>();
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
            yield return null;

            elapsed += Time.deltaTime;
        }

        if (!crescendo)
        {
            playerGamepad.SetMotorSpeeds(0, 0);
        }
    }

    private IEnumerator VibrationTransition(HoldHapticPattern pattern, bool crescendo)
    {
        float elapsed = 0f;

        while (elapsed < pattern.duration)
        {
            float progress = elapsed / pattern.duration; // Progression entre 0 et 1
            float leftMotor, rightMotor;

            if (crescendo)
            {
                leftMotor = Mathf.Lerp(pattern.leftMotorMin, pattern.leftMotorMax, progress);
                rightMotor = Mathf.Lerp(pattern.rightMotorMin, pattern.rightMotorMax, progress);
            }
            else
            {
                leftMotor = Mathf.Lerp(pattern.leftMotorMax, pattern.leftMotorMin, progress);
                rightMotor = Mathf.Lerp(pattern.rightMotorMax, pattern.rightMotorMin, progress);
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

    private IEnumerator ImpulseVibration(ImpulseHapticPattern pattern)
    {
        pattern.leftMidTime = Mathf.Min(pattern.leftMidTime, pattern.totalDuration);
        pattern.rightMidTime = Mathf.Min(pattern.rightMidTime, pattern.totalDuration);

        float elapsed = 0f;

        // Phase de montée
        while (elapsed < Mathf.Max(pattern.leftMidTime, pattern.rightMidTime))
        {
            float leftProgress = pattern.leftMidTime > 0 ? Mathf.Clamp01(elapsed / pattern.leftMidTime) : 1f;
            float rightProgress = pattern.rightMidTime > 0 ? Mathf.Clamp01(elapsed / pattern.rightMidTime) : 1f;

            float leftMotor = Mathf.Lerp(0, pattern.leftMotorMax, leftProgress);
            float rightMotor = Mathf.Lerp(0, pattern.rightMotorMax, rightProgress);

            playerGamepad.SetMotorSpeeds(leftMotor, rightMotor);

            elapsed += Time.deltaTime;
            yield return null;
        }

        // Phase de descente
        float remainingTime = pattern.totalDuration - Mathf.Max(pattern.leftMidTime, pattern.rightMidTime);
        elapsed = 0f;

        while (elapsed < remainingTime)
        {
            float progress = elapsed / remainingTime;

            float leftMotor = Mathf.Lerp(pattern.leftMotorMax, 0, progress);
            float rightMotor = Mathf.Lerp(pattern.rightMotorMax, 0, progress);

            playerGamepad.SetMotorSpeeds(leftMotor, rightMotor);

            elapsed += Time.deltaTime;
            yield return null;
        }

        // Arrêt complet
        playerGamepad.SetMotorSpeeds(0f, 0f);
    }

    private IEnumerator RepeatedImpulseVibration(ImpulseHapticPattern pattern, float totalDuration, float interval, float reductionSpeed)
    {
        float elapsed = 0f;
        float currentFactor = 1f;  // Facteur d'atténuation initial (1 = intensité maximale)

        while (elapsed < totalDuration)
        {
            // Ajuster l'intensité en fonction du facteur, qui diminue progressivement
            float adjustedLeftMotor = pattern.leftMotorMax * currentFactor;
            float adjustedRightMotor = pattern.rightMotorMax * currentFactor;

            // Lancer une impulsion et attendre qu'elle soit terminée
            yield return StartCoroutine(ImpulseVibration(adjustedLeftMotor, pattern.leftMidTime, adjustedRightMotor, pattern.rightMidTime, pattern.totalDuration));

            // Attendre un intervalle avant de relancer l'impulsion
            yield return new WaitForSeconds(interval);

            // Réduire l'intensité de manière fluide en fonction du temps écoulé
            currentFactor -= reductionSpeed * Time.deltaTime;  // Diminuer l'intensité en fonction de `deltaTime`
            currentFactor = Mathf.Max(currentFactor, 0f);  // Assure que le facteur ne devienne pas négatif

            elapsed += pattern.totalDuration + interval;
        }
    }


    public void BlocAttachedVibration()
    {
        if (playerGamepad != null && attractionCoroutine == null && repulsionCoroutine == null) // Onvérifie qu'une vibration prioritaire n'est pas en cours
        {
            if (blocAttachedCoroutine != null)
            {
                StopCoroutine(blocAttachedCoroutine);
            }
            blocAttachedCoroutine = StartCoroutine(ImpulseVibration(blocAttached));
        }
    }

    public void AttractionVibrationStart()
    {/*
        if (playerGamepad != null)
        {
            if (attractionCoroutine != null)
            {
                StopCoroutine(attractionCoroutine); 
            }
            attractionCoroutine = StartCoroutine(VibrationTransition(attraction, true));
        }*/
    }

    public void AttractionVibrationEnd()
    {/*
        if (playerGamepad != null)
        {
            StopCoroutine(attractionCoroutine);
            attractionCoroutine = null;

            StartCoroutine(VibrationTransition(attraction.leftMotorMin,attraction.leftMotorMax,attraction.rightMotorMin,attraction.rightMotorMax,.2f, false));
        }*/
    }

    public void RepulsionVibrationStart(float maxChargeTime)
    {
        if (playerGamepad != null)
        {
            if (repulsionCoroutine != null)
            {
                StopCoroutine(repulsionCoroutine);
            }
            if (attractionCoroutine != null)
            {
                StopCoroutine(attractionCoroutine);
                attractionCoroutine = null;
            }
            repulsionCoroutine = StartCoroutine(VibrationTransition(repulsionCharge.leftMotorMin, repulsionCharge.leftMotorMax, repulsionCharge.rightMotorMin, repulsionCharge.rightMotorMax, maxChargeTime, true));
        }
    }

    public void RepulsionVibrationEnd(float chargeWhenReleased, bool noBlocShot)
    {
        if (playerGamepad != null)
        {
            StopCoroutine(repulsionCoroutine);
            StopVibrations();
            repulsionCoroutine = null;

            if (!noBlocShot)
            {
                StartCoroutine(ImpulseVibration(repulsionShoot.leftMotorMax, repulsionShoot.leftMidTime, repulsionShoot.rightMotorMax, repulsionShoot.rightMidTime, repulsionShoot.totalDuration));
            }
        }
    }

    public void EjectionVibrationStart()
    {/*
        if (playerGamepad != null)
        {
            StartCoroutine(VibrationTransition(ejectionCharge, true));
        }*/
    }

    public void EjectionVibrationEnd()
    {
        if (playerGamepad != null)
        {
            StartCoroutine(ImpulseVibration(ejectionShoot));
        }
    }

    public void damageTakenVibration()
    {
        StopAllCoroutines();
        StartCoroutine(RepeatedImpulseVibration(damageTaken, playerInfo.invincibilityDelay,0,30f)); 
    }

    public void StopVibrations()
    {
        playerGamepad.SetMotorSpeeds(0, 0);
    }

     
}
