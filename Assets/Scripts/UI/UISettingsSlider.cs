using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UISettingsSlider : MonoBehaviour
{
    public Slider slider;
    public TextMeshProUGUI valueText;
    public System.Func<float, string> formatter = (value) => value.ToString("0.00");

    public UnityAction<float> onValueChanged;

    public void SetValue(float value)
    {
        OnValueChangedInternal(value);
        slider.value = value;
    }

    public float GetValue()
    {
        return slider.value;
    }

    private void OnEnable()
    {
        slider.onValueChanged.AddListener(OnValueChanged);
    }

    private void OnDisable()
    {
        slider.onValueChanged.RemoveListener(OnValueChanged);
    }

    private void OnValueChanged(float value)
    {
        OnValueChangedInternal(value);
        onValueChanged?.Invoke(value);
    }

    private void OnValueChangedInternal(float value)
    {
        valueText.text = formatter(value);
    }

    public void AddListener(UnityAction<float> action)
    {
        onValueChanged += action;
    }
}
