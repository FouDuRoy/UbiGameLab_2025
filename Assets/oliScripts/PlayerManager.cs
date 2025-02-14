using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerManager : MonoBehaviour
{
    private PlayerSpawner spawner; // R�f�rence au script PlayerSpawner

    void Awake()
    {
        spawner = FindObjectOfType<PlayerSpawner>(); // Trouver PlayerSpawner dans la sc�ne

        if (spawner == null)
        {
            Debug.LogError("PlayerSpawner non trouv� !");
            return;
        }

        // Ajouter l'�v�nement OnPlayerJoined de PlayerSpawner
        GetComponent<PlayerInputManager>().onPlayerJoined += spawner.OnPlayerJoined;
    }
}
