using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class TriggerDoor : MonoBehaviour
{
    [SerializeField] int nDoor;
    private LevelLoader levelLoader;

    void Start()
    {
        levelLoader=GetComponentInParent<LevelLoader>();
    }

    private void OnTriggerEnter(Collider other)
    {
        WinCondition player = other.GetComponent<WinCondition>(); // V�rifie si l'objet entrant est bien le mainbody du Player

        if (player != null) // Si oui, on l'ajoute � la liste des joueurs pr�ts
        {
            levelLoader.AddPlayerReady(player, nDoor);
        }
    } 
}
