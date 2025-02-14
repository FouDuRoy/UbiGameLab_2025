using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerManager : MonoBehaviour
{
    private PlayerSpawner spawner; // Référence au script PlayerSpawner

    void Awake()
    {
        spawner = FindObjectOfType<PlayerSpawner>(); // Trouver PlayerSpawner dans la scène

        if (spawner == null)
        {
            Debug.LogError("PlayerSpawner non trouvé !");
            return;
        }

        // Ajouter l'événement OnPlayerJoined de PlayerSpawner
        GetComponent<PlayerInputManager>().onPlayerJoined += spawner.OnPlayerJoined;
    }
}
