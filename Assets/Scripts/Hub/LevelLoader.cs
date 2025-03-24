using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEngine;
using System.Collections;
using TMPro;

public class LevelLoader : MonoBehaviour
{
    [SerializeField] private SceneAsset levelToLoad;
    [SerializeField] private int nPlayers = 2;
    [SerializeField] private TMP_Text j1Ready;
    [SerializeField] private TMP_Text j2Ready;

    private Collider trigger;
    private List<GameObject> playersReady = new List<GameObject>();
    

    private void Start()
    {
        trigger = GetComponent<Collider>();
        j1Ready.enabled = false;
        j2Ready.enabled = false;
    }


    private void OnTriggerEnter(Collider other)
    {
        WinCondition player=other.GetComponent<WinCondition>(); // Vérifie si l'objet entrant est bien le mainbody du Player

        if (player!=null) // Si oui, on l'ajoute à la liste des joueurs prêts
        {
            playersReady.Add(player.gameObject);
            player.gameObject.SetActive(false);

            if (player.gameObject.name.Contains("1")){ j1Ready.enabled=true; }
            else if(player.gameObject.name.Contains("2")) { j2Ready.enabled=true; }

            if(playersReady.Count >= nPlayers)
            {
                StartCoroutine(AsyncLevelLoad(levelToLoad.name));
            }
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
