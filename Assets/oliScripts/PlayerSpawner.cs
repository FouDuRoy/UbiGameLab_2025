using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerSpawner : MonoBehaviour
{
    public Transform[] spawnPoints; // Array contenant les positions de spawn
    private int playerCount = 0; // Nombre de joueurs déjà ajoutés

    public void OnPlayerJoined(PlayerInput playerInput)
    {
        if (playerCount >= spawnPoints.Length)
        {
            Debug.LogWarning("Plus de spawn points disponibles !");
            return;
        }

        // Assigner une position et une rotation au joueur
        playerInput.gameObject.transform.position = spawnPoints[playerCount].position;
        playerInput.gameObject.transform.rotation = spawnPoints[playerCount].rotation;

        Debug.Log($"Joueur {playerCount + 1} spawn à {spawnPoints[playerCount].position}");

        playerCount++; // Augmenter le compteur de joueurs
    }
}
