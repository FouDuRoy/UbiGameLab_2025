using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class CustomPlayerSpawner : MonoBehaviour
{
    public GameObject[] playerPrefabs; // Assign both prefabs in the inspector
    public Transform[] playerPositions; // Assign both prefabs in the inspector
    public GameObject[] playersNew = new GameObject[2];
    public List<InputDevice> playerDevicesSet = new List<InputDevice>();
    public PlayerInput[] playerInputs = new PlayerInput[2];
    private int playerIndex = 0;
    private InputAction anyKeyAction;
    public Transform cameraOrientation;
    public TextMeshProUGUI attackerText; // Assign in Inspector
    public Button restartButton;
    public GameObject tutoAttracRed;
    public GameObject tutoAreneRed;
    public GameObject tutoCacRed;
    public GameObject tutoFoncerRed;
    public GameObject tutoShootRed;
    public GameObject tutoAttracBlue;
    public GameObject tutoAreneBlue;
    public GameObject tutoCacBlue;
    public GameObject tutoFoncerBlue;
    public GameObject tutoShootBlue;
    public GameObject victoryCanvas;
    public GameObject pauseCanvas;
    void Awake()
    {

        DontDestroyOnLoad(gameObject); // Make manager persistent

    }
    void OnEnable()
    {
        anyKeyAction = new InputAction(
           "AnyKeyPress",
           binding: "<gamepad>/<button>",  // Matches any button press (gamepad)
           interactions: "press"     // Only trigger on press (not hold/release)
       );
        anyKeyAction.performed += OnAnyKey;
        anyKeyAction.Enable();
    }

    void OnDisable()
    {
        if (anyKeyAction != null)
        {
            anyKeyAction.performed -= OnAnyKey;
            anyKeyAction.Disable();
        }
    }

    public void OnAnyKey(InputAction.CallbackContext context)
    {
    
            foreach (InputDevice playerInput in playerDevicesSet)
            {
                if (playerInput.device == context.control.device)
                {
                    return;
                }
            }
        
        
        playerDevicesSet.Add ( context.control.device);
        // 1. Check array bounds
        if (playerIndex >= playerPrefabs.Length || playerIndex >= playerPositions.Length)
        {
            Debug.LogWarning($"Max players reached! ({playerIndex}/{playerPrefabs.Length})");
            return;
        }

        // 2. Verify prefab and position exist
        if (playerPrefabs[playerIndex] == null || playerPositions[playerIndex] == null)
        {
            Debug.LogError($"Missing prefab or position for player {playerIndex}!");
            return;
        }

        Debug.Log($"Player {playerIndex} joined via {context.control.device}");




        // 4. Spawn with explicit device pairing
        var newPlayer = PlayerInput.Instantiate(
            playerPrefabs[playerIndex],
            playerIndex,
            controlScheme: "Player" + (playerIndex + 1),
            pairWithDevice: context.control.device
        );

        // 5. Position and configure player
        newPlayer.GetComponent<PlayerObjects>().cubeRb.position = playerPositions[playerIndex].position;
        playerInputs[playerIndex] = newPlayer;


      
        if (playerIndex == 0)
        {
            tutoAreneRed.GetComponent<TutoUI>().playerMouvement = newPlayer.GetComponent<PlayerMouvement>();
            tutoAreneRed.GetComponent<TutoUI>().enabled = true;
            tutoAttracRed.GetComponent<TutoUI>().playerMouvement = newPlayer.GetComponent<PlayerMouvement>();
            tutoAttracRed.GetComponent<TutoUI>().enabled = true;
            tutoShootRed.GetComponent<TutoUI>().playerMouvement = newPlayer.GetComponent<PlayerMouvement>();
            tutoShootRed.GetComponent<TutoUI>().enabled = true;
            tutoCacRed.GetComponent<TutoUI>().playerMouvement = newPlayer.GetComponent<PlayerMouvement>();
            tutoCacRed.GetComponent<TutoUI>().enabled = true;
            tutoFoncerRed.GetComponent<TutoUI>().playerMouvement = newPlayer.GetComponent<PlayerMouvement>();
            tutoFoncerRed.GetComponent<TutoUI>().enabled = true;
        }
        else
        {
            tutoFoncerBlue.GetComponent<TutoUI>().playerMouvement = newPlayer.GetComponent<PlayerMouvement>();
            tutoFoncerBlue.GetComponent<TutoUI>().enabled = true;
            tutoCacBlue.GetComponent<TutoUI>().playerMouvement = newPlayer.GetComponent<PlayerMouvement>();
            tutoCacBlue.GetComponent<TutoUI>().enabled = true;
            tutoShootBlue.GetComponent<TutoUI>().playerMouvement = newPlayer.GetComponent<PlayerMouvement>();
            tutoShootBlue.GetComponent<TutoUI>().enabled = true;
            tutoAreneBlue.GetComponent<TutoUI>().playerMouvement = newPlayer.GetComponent<PlayerMouvement>();
            tutoAreneBlue.GetComponent<TutoUI>().enabled = true;
            tutoAttracBlue.GetComponent<TutoUI>().playerMouvement = newPlayer.GetComponent<PlayerMouvement>();
            tutoAttracBlue.GetComponent<TutoUI>().enabled = true;
        }
        newPlayer.gameObject.GetComponent<PlayerMouvement>().pauseMenu = pauseCanvas;
        newPlayer.gameObject.GetComponent<PlayerInfo>().gameOverCanvas = victoryCanvas;
        DontDestroyOnLoad(newPlayer);
        playersNew[playerIndex] = newPlayer.gameObject;
        playerIndex++;


    }
    public void spawnPlayer(GameObject[] positions)
    {
        Debug.Log("hereSpawn");

        for (int i = 0; i < positions.Length; i++)
        {
            playersNew[i].SetActive(true);
            playersNew[i].GetComponent<PlayerObjects>().cubeRb.position = positions[i].transform.position;
            var device = playerDevicesSet[i];
            playerInputs[i].SwitchCurrentControlScheme("Player" + (i+1),device);
            if (i == 0)
            {
                Debug.Log(playersNew[i].GetComponent<PlayerObjects>().cubeRb.gameObject);
                Camera.main.transform.root.GetComponent<DynamicCamera>().Player1 = playersNew[i].GetComponent<PlayerObjects>().cubeRb.gameObject;
                Debug.Log(Camera.main.transform.root.GetComponent<DynamicCamera>().Player1);

            }
            else
            {
                Debug.Log(playersNew[i].GetComponent<PlayerObjects>().cubeRb.gameObject);
                Camera.main.transform.root.GetComponent<DynamicCamera>().Player2 = playersNew[i].GetComponent<PlayerObjects>().cubeRb.gameObject;
                Debug.Log(Camera.main.transform.root.GetComponent<DynamicCamera>().Player2);
            }
        }
    }
}
