using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections;

public class ButtonTextColorChanger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler,
    ISelectHandler, IDeselectHandler, IPointerClickHandler, ISubmitHandler
{
    public TMP_Text targetText;

    public Color normalColor = Color.white;
    public Color highlightColor = Color.yellow;
    public Color selectedColor = Color.cyan;
    public Color pressedColor = Color.red;


    public void OnPointerEnter(PointerEventData eventData)
    {
        targetText.color = highlightColor;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        targetText.color = normalColor;
    }

    public void OnSelect(BaseEventData eventData)
    {
        targetText.color = selectedColor;
    }

    public void OnDeselect(BaseEventData eventData)
    {
        targetText.color = normalColor;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        SetPressedColor();
    }

    public void OnSubmit(BaseEventData eventData)
    {
        SetPressedColor();
    }

    private void SetPressedColor()
    {
        targetText.color = pressedColor;
        StartCoroutine(RevertToSelectedColor());
    }

    private IEnumerator RevertToSelectedColor()
    {
        yield return new WaitForSeconds(0.1f);
        targetText.color = selectedColor;
    }
}
