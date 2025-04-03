using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SliderVisualState : MonoBehaviour, ISelectHandler, IDeselectHandler
{
    public Image backgroundImage;
    public Image handleImage;
    public Image fillColor;

    public Sprite defaultBackground;
    public Sprite selectedBackground;

    public Sprite defaultHandle;
    public Sprite selectedHandle;

    public Color defaultColor;
    public Color selectedColor;

    public void OnSelect(BaseEventData eventData)
    {
        if (backgroundImage != null) backgroundImage.sprite = selectedBackground;
        if (handleImage != null) handleImage.sprite = selectedHandle;
        if (fillColor != null) fillColor.color = selectedColor;
    }

    public void OnDeselect(BaseEventData eventData)
    {
        if (backgroundImage != null) backgroundImage.sprite = defaultBackground;
        if (handleImage != null) handleImage.sprite = defaultHandle;
        if (fillColor != null) fillColor.color = defaultColor;
    }
}
