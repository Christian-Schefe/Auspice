using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIBuildItem : MonoBehaviour, IPointerClickHandler
{
    public TextMeshProUGUI numberText;
    public Image iconImage;
    public Image selectorImage;

    private Action onClick;

    public void OnPointerClick(PointerEventData eventData)
    {
        onClick?.Invoke();
    }

    public void UpdateData(UIBuildMenu.BuildItemData data, bool displayNumber = true)
    {
        numberText.enabled = displayNumber;
        numberText.text = data.number?.ToString() ?? "\u221E";
        iconImage.sprite = data.icon;
        onClick = data.select;
    }

    public void SetSelected(bool selected)
    {
        selectorImage.enabled = selected;
    }
}
