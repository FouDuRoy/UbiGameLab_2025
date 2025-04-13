using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [SerializeField] private PlayerInfo playerInfo;
    [SerializeField] private Image mainBar;
    [SerializeField] private Image secondBar;

    [SerializeField] private GameObject GigaRepulsion;
    [SerializeField] private GameObject SuperDash;
    [SerializeField] private GameObject AttractionOmnisciente;
    [SerializeField] private GameObject HyperVitesse;

    [SerializeField] private float mainLoweringSpeed = .5f;
    [SerializeField] private float secondLoweringSpeed = 1f;
    [SerializeField] private float referenceOrthoSize = 14f;
    [SerializeField] private float referenceScale = 1f;
    [SerializeField] private float scaleFactor = 1f;

    private Camera mainCamera;
    private float currentVelocityMain;
    private float currentVelocitySecond;
    private void Awake()
    {
        SetVisible(false, GigaRepulsion);
        SetVisible(false, SuperDash);
        SetVisible(false, AttractionOmnisciente);
        SetVisible(false, HyperVitesse);
    }
    void Start()
    {
        mainCamera = Camera.main;
    }

    void LateUpdate()
    {
        // Met à jour la rotation pour que la barre fasse face à la caméra
        transform.rotation = Quaternion.LookRotation(mainCamera.transform.forward);

        // Scale en fonction de la distance de la caméra
        float scale = referenceScale * Mathf.Pow(mainCamera.orthographicSize / referenceOrthoSize, scaleFactor);

        transform.localScale = Vector3.one * scale;

        // Utilise Time.unscaledDeltaTime pour que la vitesse de damp soit constante, peu importe le timeScale
        mainBar.fillAmount = Mathf.SmoothDamp(mainBar.fillAmount, playerInfo.healthValue / playerInfo.MaxhealthValue, ref currentVelocityMain, mainLoweringSpeed, Mathf.Infinity, Time.unscaledDeltaTime);
        secondBar.fillAmount = Mathf.SmoothDamp(secondBar.fillAmount, playerInfo.healthValue / playerInfo.MaxhealthValue, ref currentVelocitySecond, secondLoweringSpeed, Mathf.Infinity, Time.unscaledDeltaTime);
    }

    public void AddPowerup(string powerupName, float duration)
    {
        switch (powerupName)
        {
            case ("GigaRepulsion"): SetVisible(true, GigaRepulsion); break;
            case ("SuperDash"): SetVisible(true, SuperDash); break;
            case ("AttractionOmnisciente"): SetVisible(true, AttractionOmnisciente); break;
            case ("HyperVitesse"): SetVisible(true, HyperVitesse); break;
        }
    }

    public void DisablePowerUp(string powerupName)
    {
        switch (powerupName)
        {
            case ("GigaRepulsion"): SetVisible(false, GigaRepulsion); break;
            case ("SuperDash"): SetVisible(false, SuperDash); break;
            case ("AttractionOmnisciente"): SetVisible(false, AttractionOmnisciente); break;
            case ("HyperVitesse"): SetVisible(false, HyperVitesse); break;
        }
    }
    public void SetVisible(bool visible, GameObject text)
    {
        text.SetActive(visible);

        /*
        TextMeshProUGUI tmpText = text.GetComponent<TextMeshProUGUI>();
        Color color = tmpText.color;
        color.a = visible ? 1 : 0;
        tmpText.color = color;
        tmpText.raycastTarget = visible;
        */
    }
}
