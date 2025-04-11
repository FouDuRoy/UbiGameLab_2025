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
    void Start()
    {
        seuil = GetComponentInChildren<PlayerObjects>().cubeRb.gameObject.GetComponent<WinCondition>().victoryConditionSpeedMelee;
        gridPlayer = GetComponent<GridSystem>();
        vortexParticle = vortex.GetComponent<ParticleSystem>();

        mainModule = vortexParticle.main;
        var color = mainModule.startColor;
        color.color = new Color(color.color.r, color.color.g, color.color.b, 0);
        mainModule.startColor = color;
        vortexParticle.Play();
    }

    // Update is called once per frame
    void Update()
    {
        foreach (var v in gridPlayer.grid)
        {
            if (gridPlayer.grid.Count > 1)
            {
                Rigidbody rb = v.Value.GetComponent<Rigidbody>();

                float cubeVeolcityMag = (gridPlayer.kernel.GetComponent<StoredVelocity>().lastTickVelocity 
                    + Vector3.Cross((v.Value.transform.position - gridPlayer.kernel.transform.position), gridPlayer.kernel.GetComponent<Rigidbody>().angularVelocity)).magnitude;

                //Debug.Log("CubeVelMag:" + cubeVeolcityMag);
                if (rb != null && cubeVeolcityMag >= seuil)
                {
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
}
