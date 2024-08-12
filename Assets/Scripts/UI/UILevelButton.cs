using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UILevelButton : MonoBehaviour
{
    public TextMeshProUGUI text;
    public Button button;
    public UIStarDisplay starDisplay;

    public void SetText(string text)
    {
        this.text.text = text;
    }

    public void AddClickListener(UnityAction action)
    {
        button.onClick.AddListener(action);
    }

    public void RemoveClickListener(UnityAction action)
    {
        button.onClick.RemoveListener(action);
    }
}
