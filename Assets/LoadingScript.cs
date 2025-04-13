using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class LoadingScript : MonoBehaviour
{
    public GameObject player1;
    public GameObject player2;
    public GameObject audioSource;
    private PlayerInput player1Input;
    private PlayerInput player2Input;
    void Start()
    {
        
        player1Input = player1.GetComponent<PlayerInput>();
        player2Input = player2.GetComponent<PlayerInput>();
        player1Input.actions.Disable();
        player2Input.actions.Disable();
        player1Input.enabled = false;
        audioSource.SetActive(false);
        StartCoroutine(load());
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    IEnumerator load()
    {
        yield return new WaitForSeconds(3f);
        player1Input.actions.Enable();
        player2Input.actions.Enable();
        audioSource.SetActive(true);
        gameObject.SetActive(false);
    }
}
