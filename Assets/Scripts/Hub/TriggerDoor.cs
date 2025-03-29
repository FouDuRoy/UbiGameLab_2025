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
        WinCondition player = other.GetComponent<WinCondition>(); // Vérifie si l'objet entrant est bien le mainbody du Player

        if (player != null) // Si oui, on l'ajoute à la liste des joueurs prêts
        {
            levelLoader.AddPlayerReady(player, nDoor);
        }
    } 
}
