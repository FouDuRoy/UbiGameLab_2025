using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;

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
        gameOverCanvas.SetActive(true); // Show UI
        attackerText.text = "Victoire par : " + attackerName; // Display attacker's name
        EventSystem.current.SetSelectedGameObject(_restartMenu);

    }

    public void RestartLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    public void QuitGame()
    {
        Application.Quit();
    }


}
