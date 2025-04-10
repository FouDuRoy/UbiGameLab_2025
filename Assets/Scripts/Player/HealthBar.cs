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
            case ("GigaRepulsion"): GigaRepulsion.SetActive(true); break;
            case ("SuperDash"): SuperDash.SetActive(true); break;
            case ("AttractionOmnisciente"): AttractionOmnisciente.SetActive(true); break;
            case ("HyperVitesse"): HyperVitesse.SetActive(true); break;
        }
    }
}
