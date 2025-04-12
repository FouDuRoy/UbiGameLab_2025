using System.Collections;
using UnityEngine;

public class TrailScript : MonoBehaviour
{
    [SerializeField] private float fadeDurationLong = 0.2f;
    [SerializeField] private float fadeDurationShort = 0.2f;
    private TrailRenderer trailRendShort;
    private TrailRenderer trailRendLong;
    private Transform parentBloc;
    WinCondition winCon;
    Rigidbody rb;
    Bloc blocScript;
    void Start()
    {

        parentBloc = transform.parent;
        blocScript = parentBloc.gameObject.GetComponent<Bloc>();

        trailRendShort = GetComponent<TrailRenderer>();
        trailRendShort.emitting = false;

        trailRendLong = transform.Find("second_longer_trail").GetComponent<TrailRenderer>();
        trailRendLong.emitting = false;
    }

    // Update is called once per frame
    void Update()
    {
        string owner = blocScript.owner;
        if (owner.Contains("Player") && !owner.Contains("Dummy"))
        {
            rb = parentBloc.GetComponent<Rigidbody>();
            winCon = blocScript.ownerTranform.GetComponent<PlayerObjects>().cubeRb.gameObject.GetComponent<WinCondition>();
            if (blocScript.state == BlocState.projectile && rb.velocity.magnitude >= winCon.victoryConditionSpeedRange)
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

        StartCoroutine(startEmiting(fadeDurationLong, trailRendLong));
        StartCoroutine(startEmiting(fadeDurationShort, trailRendShort));

    }

    public void FadeOut()
    {
        StartCoroutine(stopEmiting(0, trailRendLong));
        StartCoroutine(stopEmiting(0, trailRendShort));
    }
    IEnumerator startEmiting(float time, TrailRenderer trail)
    {
        yield return new WaitForSeconds(time);
        trail.emitting = true;
    }

    IEnumerator stopEmiting(float time, TrailRenderer trail)
    {
        yield return new WaitForSeconds(time);
        trail.emitting = false;
    }
}
