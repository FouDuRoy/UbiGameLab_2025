using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerSpawner : MonoBehaviour
{
    public Transform[] spawnPoints; // Array contenant les positions de spawn
    private int playerCount = 0; // Nombre de joueurs déjà ajoutés

    public void OnPlayerJoined(PlayerInput playerInput)
    {
            playerInput.gameObject.name = "Joueur " + playerInput.playerIndex;
            // Assigner une position et une rotation au joueur
            playerInput.gameObject.transform.position = spawnPoints[playerCount].position;
            playerInput.gameObject.transform.rotation = spawnPoints[playerCount].rotation;

            Debug.Log($"Joueur {playerCount + 1} spawn à {spawnPoints[playerCount].position}");

            playerCount++; // Augmenter le compteur de joueurs
        if(playerCount == 2)
            this.GetComponent<PlayerInputManager>().enabled = false;
    }
}
