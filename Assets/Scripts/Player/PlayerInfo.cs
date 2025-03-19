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
    [SerializeField] private GameObject _restartMenu;

    void Start()
    {
        gameOverCanvas.SetActive(false); // Hide canvas at start
    }

    // Call this function when player gets hit
    public void TakeDamage(string attackerName)
    {
        this.GetComponent<PlayerInput>().enabled = false;
        this.GetComponent<PlayerMouvement>().ThrowCubes();
        this.GetComponent<PlayerObjects>().cubeRb.AddTorque(0, 1000f, 0,ForceMode.VelocityChange);
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
    IEnumerator gameOver(string attackerName)
    {

        yield return new WaitForSeconds(1.5f);
        gameOverCanvas.SetActive(true); // Show UI
        attackerText.text = "Victoire par : " + attackerName; // Display attacker's name
        EventSystem.current.SetSelectedGameObject(_restartMenu);

    }

}
