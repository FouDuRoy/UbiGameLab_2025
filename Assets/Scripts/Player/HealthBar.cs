using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [SerializeField] private PlayerInfo playerInfo;
    [SerializeField] private Image mainBar;
    [SerializeField] private Image secondBar;
    [SerializeField] private float mainLoweringSpeed = .5f;
    [SerializeField] private float secondLoweringSpeed = 1f;

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

        // Utilise Time.unscaledDeltaTime pour que la vitesse de damp soit constante, peu importe le timeScale
        mainBar.fillAmount = Mathf.SmoothDamp(mainBar.fillAmount, playerInfo.healthValue / playerInfo.MaxhealthValue, ref currentVelocityMain, mainLoweringSpeed, Mathf.Infinity, Time.unscaledDeltaTime);
        secondBar.fillAmount = Mathf.SmoothDamp(secondBar.fillAmount, playerInfo.healthValue / playerInfo.MaxhealthValue, ref currentVelocitySecond, secondLoweringSpeed, Mathf.Infinity, Time.unscaledDeltaTime);
    }
}
