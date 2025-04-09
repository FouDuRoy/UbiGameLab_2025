using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public enum TutoType
{
    Attraction,
    Foncer,
    Shoot,
    Cac,
    Arene
}

public class TutoUI : MonoBehaviour
{
    [SerializeField] private PlayerMouvement playerMouvement;
    [SerializeField] private Color playerColor;
    [SerializeField] private TutoType tutoType;
    [SerializeField] private TutoUI nextTuto;

    [SerializeField] private Sprite attractionInput;
    [SerializeField] private Sprite foncerInput;
    [SerializeField] private Sprite shootInput;
    [SerializeField] private Sprite cacInput;

    private Image inputImage;
    private TMP_Text tutoText;
    private Camera mainCamera;
    private GridSystem playerGrid;
    private Dash playerDash;
    private float normalPlayerSpeed;
    private

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

        tutoText = GetComponentInChildren<TMP_Text>();
        tutoText.color = playerColor;

        mainCamera = Camera.main;
        playerGrid = playerMouvement.GetComponent<GridSystem>();
        playerDash = playerMouvement.GetComponentInChildren<Dash>();

        switch (tutoType)
        {
            case TutoType.Attraction:
                inputImage.sprite = attractionInput;
                break;

            case TutoType.Foncer:
                inputImage.sprite = foncerInput;
                break;

            case TutoType.Shoot:
                inputImage.sprite = shootInput;
                break;

            case TutoType.Cac: inputImage.sprite = cacInput; break;
            case TutoType.Arene:inputImage.enabled = false;break;
        }

        if (tutoType != TutoType.Attraction)
        {
            gameObject.SetActive(false);
        }
        else
        {
            normalPlayerSpeed = playerMouvement.mouvementSpeed;
            playerMouvement.mouvementSpeed = 0;
            playerDash.canDash = false;
        }
    }

    private void OnDestroy()
    {
        NextTuto();
    }

    private void FixedUpdate()
    {
        if (tutoType == TutoType.Attraction)
        {
            if (playerGrid.grid.Count > 1)
            {
                playerMouvement.mouvementSpeed = normalPlayerSpeed;
                playerDash.canDash = true;
                NextTuto();
            }
        }
    }

    void LateUpdate()
    {
        // Met à jour la rotation pour que la barre fasse face à la caméra
        transform.rotation = Quaternion.LookRotation(mainCamera.transform.forward);
    }

    public void NextTuto()
    {
        //Debug.Log(name + " skips to next tuto : " + nextTuto.name);

        if (nextTuto != null)
        {
            nextTuto.gameObject.SetActive(true);
        }
        gameObject.SetActive(false);
    }
}
