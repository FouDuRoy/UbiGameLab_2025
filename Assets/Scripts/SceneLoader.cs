using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class SceneEventSystem : MonoBehaviour
{

    public Button playButton;
    public SceneAsset sceneToLoad;

    private void Awake()
    {
        EventSystem.current.SetSelectedGameObject(playButton.gameObject);
    }
    public void LoadScene()
    {
        SceneManager.LoadScene(sceneToLoad.name);
    }

    public void ExitGame()
    {
        Application.Quit();
    }
}
