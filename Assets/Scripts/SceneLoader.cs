using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class SceneEventSystem : MonoBehaviour
{

    public Button playButton;

    private void Awake()
    {
        EventSystem.current.SetSelectedGameObject(playButton.gameObject);
    }
    public void LoadScene()
    {
        SceneManager.LoadScene("Level02");
    }

    public void ExitGame()
    {
        Application.Quit();
    }
}
