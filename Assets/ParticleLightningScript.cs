using System.Collections;
using UnityEngine;

public class ParticleLightningScript : MonoBehaviour
{
    [SerializeField] private float fadeDurationLong = 0.2f;
    [SerializeField] private float fadeDurationShort = 0.2f;

    private ParticleSystem partSystemShort;
    private ParticleSystem partSystemLong;
    private Transform parentBloc;
    private WinCondition winCon;
    private Rigidbody rb;
    private Bloc blocScript;

    void Start()
    {
        parentBloc = transform.parent;
        blocScript = parentBloc.GetComponent<Bloc>();

        partSystemShort = GetComponent<ParticleSystem>();
        partSystemLong = transform.Find("LightningEffect_testing").GetComponent<ParticleSystem>();

        partSystemShort.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        partSystemLong.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    }

    void Update()
    {
        string owner = blocScript.owner;
        if (owner.Contains("Player") && !owner.Contains("Dummy"))
        {
            rb = parentBloc.GetComponent<Rigidbody>();
            winCon = blocScript.ownerTranform.GetComponent<PlayerObjects>().cubeRb.gameObject.GetComponent<WinCondition>();

            if (blocScript.state == BlocState.structure && rb.velocity.magnitude >= winCon.victoryConditionSpeedMelee)
            {
                FadeIn();
            }
            else
            {
                FadeOut();
            }
        }
    }

    public void FadeIn()
    {
        StartCoroutine(StartParticlesAfterDelay(fadeDurationLong, partSystemLong));
        StartCoroutine(StartParticlesAfterDelay(fadeDurationShort, partSystemShort));
    }

    public void FadeOut()
    {
        StartCoroutine(StopParticlesAfterDelay(0, partSystemLong));
        StartCoroutine(StopParticlesAfterDelay(0, partSystemShort));
    }

    IEnumerator StartParticlesAfterDelay(float delay, ParticleSystem ps)
    {
        yield return new WaitForSeconds(delay);
        if (!ps.isPlaying)
            ps.Play();
    }

    IEnumerator StopParticlesAfterDelay(float delay, ParticleSystem ps)
    {
        yield return new WaitForSeconds(delay);
        if (ps.isPlaying)
            ps.Stop(true, ParticleSystemStopBehavior.StopEmitting);
    }
}
