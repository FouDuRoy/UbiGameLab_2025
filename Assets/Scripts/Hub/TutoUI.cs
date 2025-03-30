using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum TutoType
{
    Attraction,
    Movement,
    Attack,
    EmergencyEjection
}

public class TutoUI : MonoBehaviour
{
    [SerializeField] private PlayerMouvement playerMouvement;
    [SerializeField] private Color playerColor;
    [SerializeField] private TutoType tutoType;
    [SerializeField] private TutoUI nextTuto;

    [SerializeField] private Sprite attractionInput;
    [SerializeField] private Sprite movementIntput;
    [SerializeField] private Sprite attackInput;
    [SerializeField] private Sprite emergencyEjectionInput;

    private Image inputImage;
    private Camera mainCamera;
    private GridSystem playerGrid;
    private float normalPlayerSpeed;

    // Start is called before the first frame update
    void Start()
    {
        foreach (Image img in GetComponentsInChildren<Image>())
        {
            if (img.name == "TutoImage")
            {
                inputImage = img;
            }
            else
            {
                img.color = playerColor;
            }
        }

        mainCamera = Camera.main;
        playerGrid = playerMouvement.GetComponent<GridSystem>();

        switch (tutoType)
        {
            case TutoType.Attraction:
                inputImage.sprite = attractionInput;
                break;

            case TutoType.Movement:
                inputImage.sprite = movementIntput;
                break;

            case TutoType.Attack:
                inputImage.sprite = attackInput;
                break;

            case TutoType.EmergencyEjection:
                inputImage.sprite = emergencyEjectionInput;
                break;
        }

        if (tutoType != TutoType.Attraction)
        {
            gameObject.SetActive(false);
        }
        else
        {
            normalPlayerSpeed = playerMouvement.mouvementSpeed;
            playerMouvement.mouvementSpeed = 0;
        }

        if (tutoType == TutoType.Movement)
        {
            
        }
    }

    private void FixedUpdate()
    {
        if (tutoType == TutoType.Attraction)
        {
            if (playerGrid.grid.Count > 1)
            {
                playerMouvement.mouvementSpeed = normalPlayerSpeed;
                NextTuto();
            }
        }
        else if (tutoType == TutoType.Movement)
        {
            
        }
    }

    void LateUpdate()
    {
        // Met à jour la rotation pour que la barre fasse face à la caméra
        transform.rotation = Quaternion.LookRotation(mainCamera.transform.forward);
    }

    private void NextTuto()
    {
        Debug.Log(name + " skips to next tuto : " + nextTuto.name);
        nextTuto.gameObject.SetActive(true);
        gameObject.SetActive(false);
    }
}
