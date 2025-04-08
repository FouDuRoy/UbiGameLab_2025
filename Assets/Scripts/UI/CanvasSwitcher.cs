using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CanvasSwitcher : MonoBehaviour
{
    public GameObject[] canvases;

    public void ShowOnly(GameObject canvasToShow)
    {
        foreach (var canvas in canvases)
        {
            canvas.SetActive(canvas == canvasToShow);
        }
    }
    public void ShowButton(Button selected)
    {
        if (selected != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(selected.gameObject);
        }
    }
}