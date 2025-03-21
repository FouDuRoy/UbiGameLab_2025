using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerInfo : MonoBehaviour
{
    public GameObject gameOverCanvas; // Assign in Inspector
    public TextMeshProUGUI attackerText; // Assign in Inspector
    public Button restartButton;
    public float maxDamage =30f;
    public float impulsionWhenHit = 30f;
    public float invincibilityDelay=1f;
    public float hitStopDelay;
    public float MaxhealthValue = 100f;
    public float waitDelayAfterPlayerDeath = 1.5f;
    bool invun = false;
    public float healthValue;

    void Start()
    {
        healthValue = MaxhealthValue;
        gameOverCanvas.SetActive(false); // Hide canvas at start
    }

    // Call this function when player gets hit
    public void TakeDamage(string attackerName,Vector3 impactForce)
    {
        if (!invun)
        {
            float damage = Mathf.Clamp(impactForce.magnitude, 10, maxDamage);
            healthValue -= damage;
            Debug.Log("Current Health:" + healthValue + "damageTook:" + damage);
            if (healthValue > 0)
            {
                this.GetComponent<PlayerObjects>().cubeRb.AddForce(impactForce.normalized * impulsionWhenHit, ForceMode.VelocityChange);
                invun =true;
                StartCoroutine(invunerable());
            }
            else
            {
                this.GetComponent<PlayerInput>().enabled = false;
                this.GetComponent<PlayerObjects>().cubeRb.AddForce(impactForce.normalized * impulsionWhenHit * 1.5f, ForceMode.VelocityChange);
                deathRotation(attackerName);
                StartCoroutine(gameOver(attackerName));
            }
        }
       
    }

    private void deathRotation(string attackerName)
    {
        this.GetComponent<PlayerMouvement>().ThrowCubes();
        this.GetComponent<PlayerObjects>().cubeRb.AddTorque(0, 100f, 0, ForceMode.VelocityChange);
        StartCoroutine(gameOver(attackerName));
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
        yield return new WaitForSeconds(1.5f);
        //Time.timeScale = 0;
        gameOverCanvas.SetActive(true); // Show UI
        attackerText.text = "Victoire par : " + attackerName; // Display attacker's name
        EventSystem.current.SetSelectedGameObject(restartButton.gameObject);

    }

}
