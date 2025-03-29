using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEngine;
using System.Collections;
using TMPro;

public class LevelLoader : MonoBehaviour
{
    [SerializeField] private string levelToLoad;
    [SerializeField] private int nPlayers = 2;
    [SerializeField] private TMP_Text j1Ready;
    [SerializeField] private TMP_Text j2Ready;

    private List<Collider> triggers =new List<Collider>();
    private List<GameObject> playersReady = new List<GameObject>();
    

    private void Start()
    {
        foreach(Collider trigger in GetComponentsInChildren<Collider>())
        {
            triggers.Add(trigger);
        }
        j1Ready.enabled = false;
        j2Ready.enabled = false;
    }


    public void AddPlayerReady(WinCondition player, int nPlayer)
    {
        playersReady.Add(player.gameObject);
        player.GetComponentInParent<PlayerInfo>().gameObject.SetActive(false);

        if (nPlayer==1) { j1Ready.enabled = true; }
        else if (nPlayer==2) { j2Ready.enabled = true; }

        if (playersReady.Count >= nPlayers)
        {
            StartCoroutine(AsyncLevelLoad(levelToLoad));
        }
    }

    private IEnumerator AsyncLevelLoad(string sceneName)
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
        while (!operation.isDone)
        {
            yield return null;
        }
    }
}
