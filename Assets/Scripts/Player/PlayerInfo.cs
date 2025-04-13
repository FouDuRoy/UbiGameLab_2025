using System.Collections;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerInfo : MonoBehaviour
{
    
    [Header("Info joueur")]
    public GameObject gameOverCanvas; // Assign in Inspector
    public Button restartButton;
    public float maxDamage =30f;
    public float impulsionWhenHitMelee = 30f;
    public float impulsionWhenHitRanged = 30f;
    public float invincibilityDelay=1f;
    public float hitStopDelay = 0.5f;
    public float deathHitStopDelay = 5f;
    public float MaxhealthValue = 100f;
    public float waitDelayAfterPlayerDeath = 1.5f;
    public float timeScaleFactor = 0.1f;
    public float transitionTimeBegin = 0.05f;
    public float transitionTimeEnd = 0.1f;
    public bool invun = false;
    public bool recovering = false;
    public float healthValue;
    public float waitTimeBeforeVictoryCanvas = 3f;
    private Animator animator;

    [Header("Debogage")]
    [SerializeField] GameObject playerLife;
    [SerializeField] bool isInvincible;

    private DynamicCamera dynamicCamera;

    [Header("SFX")]
    public GameObject takeDamangeSfx;
    public GameObject dieSfx;

    void OnEnable()
    {
        animator = GetComponentInChildren<Animator>();
        healthValue = MaxhealthValue;

        if (isInvincible)
        {
            playerLife.SetActive(false);
        }

        dynamicCamera=Camera.main.GetComponentInParent<DynamicCamera>();
    }

    // Call this function when player gets hit
    public void TakeDamage(GameObject attackerName,Vector3 impactForce, bool melee)
    {
        if(!isInvincible)
        {
            if (!invun)
            {
                this.GetComponent<HapticFeedbackController>().damageTakenVibration();
                float damage = Mathf.Clamp(impactForce.magnitude, 10, maxDamage);
                healthValue -= damage;
                animator.SetTrigger("IsHit");

                Rigidbody cubeRb = GetComponent<PlayerObjects>().cubeRb;

                if (healthValue > 0)
                {
                    invun = true;
                    if (takeDamangeSfx != null)
                    {
                        takeDamangeSfx.GetComponent<AudioSource>().Play();
                    }
                    this.GetComponent<ColorFlicker>().SetFlickerEnabled(true);
                    StartCoroutine(DoHitStop(hitStopDelay));

                    dynamicCamera.HitEffect(transitionTimeBegin+hitStopDelay+transitionTimeEnd, cubeRb);

                    if(melee){
                        cubeRb.AddForce(impactForce.normalized * impulsionWhenHitMelee, ForceMode.VelocityChange);
                        
                    }else{
                        cubeRb.AddForce(impactForce.normalized * impulsionWhenHitRanged, ForceMode.VelocityChange);
                    }
                    StartCoroutine(invunerable());

                }
                else
                {
                    invun = true;
                    if (takeDamangeSfx != null)
                    {
                        takeDamangeSfx.GetComponent<AudioSource>().Play();
                    }
                    if (dieSfx != null)
                    {
                        dieSfx.GetComponent<AudioSource>().Play();
                    }
                    StartCoroutine(DoHitStop(deathHitStopDelay));
                    this.GetComponent<PlayerInput>().enabled = false;
                     if(melee){
                        cubeRb.AddForce(impactForce.normalized * impulsionWhenHitMelee*1.5f, ForceMode.VelocityChange);
                        
                    }else{
                        cubeRb.AddForce(impactForce.normalized * impulsionWhenHitRanged*1.5f, ForceMode.VelocityChange);
                    }
                    deathRotation(attackerName.name);
                    GetComponent<PlayerInput>().enabled = false;
                    attackerName.GetComponent<PlayerInput>().enabled=false;
                    //StartCoroutine(gameOver(attackerName));

                }
            }
        }

       
    }

    private void deathRotation(string attackerName)
    {
        this.GetComponent<PlayerMouvement>().ThrowCubes();
        this.GetComponent<PlayerObjects>().cubeRb.AddTorque(0, 100f, 0, ForceMode.VelocityChange);
        if(!isInvincible)
        {
            StartCoroutine(gameOver(attackerName));
        }
    }

    public void RestartLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    public void QuitGame()
    {
        Application.Quit();
    }
    IEnumerator invunerable()
    {
        recovering = true;
        yield return new WaitForSeconds(invincibilityDelay);
        recovering = false;
        invun = false;
        this.GetComponent<ColorFlicker>().SetFlickerEnabled(false);
    }
    IEnumerator gameOver(string attackerName)
    {
        dynamicCamera.PlayVictoryAnimation(attackerName);
        yield return new WaitForSeconds(waitTimeBeforeVictoryCanvas);
        //Time.timeScale = 0;
        gameOverCanvas.SetActive(true); // Show UI
        EventSystem.current.SetSelectedGameObject(restartButton.gameObject);

    }
     IEnumerator DoHitStop(float duration)
    {
        float time = 0;
        float t=0;
        while (t <= 1)
        {
            t = time / transitionTimeBegin;
            Time.timeScale = Mathf.Lerp(1, timeScaleFactor, t);
            yield return new WaitForSeconds(Time.deltaTime);
            time += Time.deltaTime;
        }
        Time.timeScale = timeScaleFactor; 
        yield return new WaitForSecondsRealtime(duration);
        time = 0;
        t = 0;
        while (t <= 1)
        {
            t = time / transitionTimeEnd;
            Time.timeScale = Mathf.Lerp(timeScaleFactor, 1, t);
            yield return new WaitForSeconds(Time.deltaTime);
            time += Time.deltaTime;
        }
        Time.timeScale = 1f; 
    }
}
