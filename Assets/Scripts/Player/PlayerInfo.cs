using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerInfo : MonoBehaviour
{

    [Header("Info joueur")]
    public GameObject gameOverCanvas; // Assign in Inspector
    public TextMeshProUGUI attackerText; // Assign in Inspector
    public Button restartButton;
    public float maxDamage =30f;
    public float impulsionWhenHitMelee = 30f;
    public float impulsionWhenHitRanged = 30f;
    public float invincibilityDelay=1f;
    public float hitStopDelay=0.5f;
    public float MaxhealthValue = 100f;
    public float waitDelayAfterPlayerDeath = 1.5f;
    public float timeScaleFactor = 0.2f;
    bool invun = false;
    public float healthValue;
    [Header("D�bogage")]
    [SerializeField] GameObject playerLife;
    [SerializeField] bool isInvincible;

    private DynamicCamera dynamicCamera;

    void Start()
    {
        healthValue = MaxhealthValue;
        gameOverCanvas.SetActive(false); // Hide canvas at start

        if (isInvincible)
        {
            playerLife.SetActive(false);
        }

        dynamicCamera=Camera.main.GetComponentInParent<DynamicCamera>();
    }

    // Call this function when player gets hit
    public void TakeDamage(string attackerName,Vector3 impactForce, bool melee)
    {
        if(isInvincible)
        {
            this.GetComponent<HapticFeedbackController>().damageTakenVibration();
            deathRotation(attackerName);
        }
        else
        {
            if (!invun)
            {
                this.GetComponent<HapticFeedbackController>().damageTakenVibration();
                float damage = Mathf.Clamp(impactForce.magnitude, 10, maxDamage);
                healthValue -= damage;
                Debug.Log("Current Health:" + healthValue + "damageTook:" + damage);
                if (healthValue > 0)
                {
                    invun = true;
                    StartCoroutine(DoHitStop(hitStopDelay));
                    if(melee){
                        this.GetComponent<PlayerObjects>().cubeRb.AddForce(impactForce.normalized * impulsionWhenHitMelee, ForceMode.VelocityChange);
                        
                    }else{
                        this.GetComponent<PlayerObjects>().cubeRb.AddForce(impactForce.normalized * impulsionWhenHitRanged, ForceMode.VelocityChange);
                    }
                    StartCoroutine(invunerable());

                }
                else
                {
                    invun = true;
                    StartCoroutine(DoHitStop(hitStopDelay));
                    this.GetComponent<PlayerInput>().enabled = false;
                     if(melee){
                        this.GetComponent<PlayerObjects>().cubeRb.AddForce(impactForce.normalized * impulsionWhenHitMelee*1.5f, ForceMode.VelocityChange);
                        
                    }else{
                        this.GetComponent<PlayerObjects>().cubeRb.AddForce(impactForce.normalized * impulsionWhenHitRanged*1.5f, ForceMode.VelocityChange);
                    }
                    deathRotation(attackerName);
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
        yield return new WaitForSeconds(invincibilityDelay);
        invun = false;
    }
    IEnumerator gameOver(string attackerName)
    {
        dynamicCamera.PlayVictoryAnimation(attackerName);
        yield return new WaitForSeconds(2f);
        //Time.timeScale = 0;
        gameOverCanvas.SetActive(true); // Show UI
        attackerText.text = "Victoire par : " + attackerName; // Display attacker's name
        EventSystem.current.SetSelectedGameObject(restartButton.gameObject);

    }
     IEnumerator DoHitStop(float duration)
    {
        Time.timeScale = timeScaleFactor; 
        yield return new WaitForSecondsRealtime(duration); 
        Time.timeScale = 1f; 
    }
}
