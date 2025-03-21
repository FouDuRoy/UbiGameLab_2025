using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEngine;
using System.Collections;

public class LevelLoader : MonoBehaviour
{
    [SerializeField] private SceneAsset levelToLoad;
    [SerializeField] private int nPlayers=2;

    private Collider trigger;
    private List<GameObject> playersReady = new List<GameObject>();
    

    private void Start()
    {
        trigger = GetComponent<Collider>();
    }


    private void OnTriggerEnter(Collider other)
    {
        WinCondition player=other.GetComponent<WinCondition>(); // Vérifie si l'objet entrant est bien le mainbody du Player

        if (player!=null) // Si oui, on l'ajoute à la liste des joueurs prêts
        {
            playersReady.Add(player.gameObject);
            player.gameObject.SetActive(false);

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
