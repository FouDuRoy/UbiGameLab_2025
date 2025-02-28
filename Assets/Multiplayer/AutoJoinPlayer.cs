using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class AutoJoinPlayer : MonoBehaviour
{
    public GameObject Player; // reference player prefab
    [SerializeField] private float spawnDistanceX; // distance X du player 2 au player 1
    [SerializeField] private float spawnDistanceZ; // distance X du player 2 au player 1
    public PlayerInputManager playerInputManager;

    void Start()
    {
        SpawnSecondPlayer();
    }

    private void SpawnSecondPlayer()
    {
        Debug.Log("assigning Second Player");
        if (playerInputManager.playerCount < playerInputManager.maxPlayerCount)
        {
            //on cherche premier joueur
            if (Player != null)
            {
                Vector3 firstPlayerPosition = Player.transform.position;

                Vector3 spawnPoint = firstPlayerPosition + new Vector3(spawnDistanceX, 0f, spawnDistanceZ);
                //creation du nouvel objet
                GameObject secondPlayer = Instantiate(Player, spawnPoint, Quaternion.identity);

                PlayerInput secondPlayerInput = secondPlayer.GetComponent<PlayerInput>();

                int playerIndex = playerInputManager.playerCount;

                secondPlayerInput.SwitchCurrentActionMap("Player");
            }
            else
            {
                Debug.LogError("Couldnt find first player");
            }


        }
    }
}
