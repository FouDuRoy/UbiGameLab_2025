using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public enum TutoType
{
    Attraction,
    Repulsion,
    Cac,
    Dash,
    Arene
}

public class TutoUI : MonoBehaviour
{
    [SerializeField] private PlayerMouvement playerMouvement;
    [SerializeField] private Color playerColor;
    [SerializeField] public TutoType tutoType;
    [SerializeField] private TutoUI nextTuto;

    [SerializeField] private Sprite attractionInput;
    [SerializeField] private Sprite foncerInput;
    [SerializeField] private Sprite shootInput;
    [SerializeField] private Sprite cacInput;
    [SerializeField] private GameObject animatedCacInput;
    [SerializeField] private GridSystem dummyGrid;
    [SerializeField] private GameObject doorBarriere;
    [SerializeField] private GameObject tutoInputToShow;

    private Image inputImage;
    private TMP_Text tutoText;
    private TMP_Text countText;
    private Camera mainCamera;
    private GridSystem playerGrid;
    private Dash playerDash;
    private float normalPlayerSpeed;
    int lastGridCount=0;

    // Start is called before the first frame update
    private void Update()
    {
        if (tutoType == TutoType.Dash)
        {
            if (dummyGrid.grid.Count< lastGridCount)
            {
                NextTuto();
            }
        }
        lastGridCount = dummyGrid.grid.Count;
    }
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
        
        foreach(TMP_Text txt in GetComponentsInChildren<TMP_Text>())
        {
            if(txt.name == "TutoCount")
            {
                countText = txt;
                countText.color = playerColor;
            }
        }

        

        mainCamera = Camera.main;
        playerGrid = playerMouvement.GetComponent<GridSystem>();
        playerDash = playerMouvement.GetComponentInChildren<Dash>();

        switch (tutoType)
        {
            case TutoType.Attraction:
                inputImage.sprite = attractionInput;
                countText.gameObject.SetActive(false);
                break;

            case TutoType.Dash:
                inputImage.sprite = foncerInput;
                countText.gameObject.SetActive(false);
                break;

            case TutoType.Repulsion:
                inputImage.sprite = shootInput;
                break;

            case TutoType.Cac: inputImage.sprite = cacInput; break;
            case TutoType.Arene:inputImage.enabled = false; countText.gameObject.SetActive(false);  break;
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

        if (tutoType != TutoType.Cac)
        {
            animatedCacInput.SetActive(false);
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

        if (nextTuto != null)
        {
            nextTuto.gameObject.SetActive(true);
        }
        if (tutoType == TutoType.Cac)
        {
            doorBarriere.SetActive(false);
        }
        if(tutoInputToShow != null)
        {
            tutoInputToShow.SetActive(true );
        }

        gameObject.SetActive(false);
    }

    public void SetTutoCount(int actuelCount, int maxCount)
    {
        countText.text = actuelCount.ToString()+" / "+maxCount.ToString();
    }

    IEnumerator waitForCubesToConnect()
    {
        Debug.Log(lastGridCount);
        yield return new WaitForSeconds(2f);
        lastGridCount = dummyGrid.grid.Count;
        Debug.Log(lastGridCount);
    }
}
