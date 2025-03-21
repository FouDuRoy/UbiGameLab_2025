using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [SerializeField] private PlayerInfo playerInfo;
    [SerializeField] private Image bar;

    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;  // Récupère la caméra principale

    }

    void LateUpdate()
    {
        if (mainCamera != null)
        {
            transform.rotation = Quaternion.LookRotation(mainCamera.transform.forward);
        }

        bar.fillAmount = playerInfo.healthValue/playerInfo.MaxhealthValue;
    }
}
