using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class LoadingScript : MonoBehaviour
{
    public GameObject player1;
    public GameObject player2;
    private PlayerInput player1Input;
    private PlayerInput player2Input;
    void Start()
    {
        player1Input = player1.GetComponent<PlayerInput>();
        player2Input = player2.GetComponent<PlayerInput>();
        StartCoroutine(load());
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    IEnumerator load()
    {
        yield return new WaitForSeconds(3f);
        gameObject.SetActive(false);
    }
}
