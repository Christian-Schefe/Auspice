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
    public SpriteRegistry spriteRegistry;
    public Button editPlusButton, editMinusButton;

    private Action<BuildEntityType> onClick;

    private BuildEntityType type;
    private int index;

    public void OnPointerClick(PointerEventData eventData)
    {
        onClick?.Invoke(type);
    }

    public void SetType(BuildEntityType type)
    {
        this.type = type;
        iconImage.sprite = spriteRegistry.GetSprite(BuildEntityTypeRef.GetEntityTypes(type)[index]);
    }

    public void SetIndex(int index)
    {
        this.index = index;
        iconImage.sprite = spriteRegistry.GetSprite(BuildEntityTypeRef.GetEntityTypes(type)[index]);
    }

    public void SetOnClick(Action<BuildEntityType> action)
    {
        onClick = action;
    }

    public void SetNumber(int? number, bool shouldDisplayNumber = true)
    {
        numberText.enabled = shouldDisplayNumber;
        numberText.text = number?.ToString() ?? "\u221E";
    }

    public void SetSelected(bool selected)
    {
        selectorImage.enabled = selected;
    }

    public void SetOnEditButton(Action<BuildEntityType, bool> onEditButton)
    {
        if (onEditButton != null)
        {
            editPlusButton.gameObject.SetActive(true);
            editMinusButton.gameObject.SetActive(true);

            editPlusButton.onClick.AddListener(() => onEditButton(type, true));
            editMinusButton.onClick.AddListener(() => onEditButton(type, false));
        } else
        {
            editPlusButton.gameObject.SetActive(false);
            editMinusButton.gameObject.SetActive(false);
        }
    }
}
