using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

public class VortexAnimation : MonoBehaviour
{
    public GameObject vortex;
    float seuil;
    ParticleSystem vortexParticle;
    GridSystem gridPlayer;
    public float fadeInTime = 1f;
    public float fadeOutTime = 1f;
    private ParticleSystem.MainModule mainModule;
    bool isActive = false;
    HapticFeedbackController feedback;
    Transform golem;
    void Start()
    {
        golem = GetComponent<PlayerObjects>().golem.transform;
        seuil = GetComponentInChildren<PlayerObjects>().cubeRb.gameObject.GetComponent<WinCondition>().victoryConditionSpeedMelee;
        gridPlayer = GetComponent<GridSystem>();
        vortexParticle = vortex.GetComponent<ParticleSystem>();
        feedback = GetComponent<HapticFeedbackController>();
        mainModule = vortexParticle.main;
        var color = mainModule.startColor;
        color.color = new Color(color.color.r, color.color.g, color.color.b, 0);
        mainModule.startColor = color;
        vortexParticle.Play();
    }

    // Update is called once per frame
    void Update()
    {
        StartCoroutine(RotationDirecton());
        foreach (var v in gridPlayer.grid)
        {
            if (gridPlayer.grid.Count > 1)
            {
                Rigidbody rb = v.Value.GetComponent<Rigidbody>();

                float cubeVeolcityMag = (gridPlayer.kernel.GetComponent<StoredVelocity>().lastTickVelocity 
                    + Vector3.Cross((v.Value.transform.position - gridPlayer.kernel.transform.position), gridPlayer.kernel.GetComponent<Rigidbody>().angularVelocity)).magnitude;



                // Fait tourner le vortex selon la rotation Y du joueur (pour l'effet visuel)

                StartCoroutine(RotationDirecton());
                if (rb != null && cubeVeolcityMag >= seuil)
                {
                    //active rumble feedback
                    feedback.MeleeSpeedVibrationStart();

                    if (!isActive)
                    {
                        isActive = true;
                        StartCoroutine(FadeIn());
                    }
                    return; // On quitte dès qu’on l’a activé
                }
            }
        }
        if (isActive)
        {
            isActive = false;
            feedback.MeleeSpeedVibrationEnd();
            StartCoroutine(FadeOut());
        }
    }
    IEnumerator FadeIn()
    {
        float timer = 0f;
        float alpha=0;
        while (alpha < 1)
        {
            alpha = Mathf.Clamp01(timer / fadeInTime);
            var color = mainModule.startColor;
            color.color = new Color(color.color.r, color.color.g, color.color.b, alpha);
            mainModule.startColor = color;
            yield return new WaitForSeconds(Time.deltaTime);
            timer += Time.deltaTime;
            
        }


    }
    IEnumerator FadeOut()
    {
        float timer = 0f;
        float alpha=1;
        while (alpha > 0)
        {
            alpha = 1f - Mathf.Clamp01(timer / fadeOutTime);
            var color = mainModule.startColor;
            color.color = new Color(color.color.r, color.color.g, color.color.b, alpha);
            mainModule.startColor = color;
            yield return new WaitForSeconds(Time.deltaTime);
            timer += Time.deltaTime;
        }
    }
    IEnumerator RotationDirecton()
    {
        Vector3 lastFoward = golem.transform.forward;
        yield return new WaitForSeconds(0.05f);
        Vector3 newFoward  = golem.transform.forward;
        var vortexModule = vortexParticle.velocityOverLifetime;
        float angle = Vector3.SignedAngle(lastFoward, newFoward,Vector3.up);
        if(angle > 0 || GetComponent<PlayerMouvement>().moveType == MouvementType.HyperVite)
        {
            vortexModule.orbitalZ = new ParticleSystem.MinMaxCurve(0,10);
        }
        else
        {
            vortexModule.orbitalZ = new ParticleSystem.MinMaxCurve(0,-10);
        }
    }
}
