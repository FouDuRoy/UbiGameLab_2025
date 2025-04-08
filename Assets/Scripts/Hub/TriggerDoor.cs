using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class TriggerDoor : MonoBehaviour
{
    [SerializeField] int nDoor;
    [SerializeField] private TutoUI ejectionTutoUI;
    [SerializeField] LevelLoader levelLoader;

    void Start()
    {
    }

    private void OnTriggerEnter(Collider other)
    {
        WinCondition player = other.GetComponent<WinCondition>(); // Vérifie si l'objet entrant est bien le mainbody du Player

        if (player != null) // Si oui, on l'ajoute à la liste des joueurs prêts
        {
            levelLoader.AddPlayerReady(player, nDoor);
            ejectionTutoUI.NextTuto();
        }
    } 
}
