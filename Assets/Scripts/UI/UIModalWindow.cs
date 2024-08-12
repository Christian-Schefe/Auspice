using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIModalWindow : MonoBehaviour
{
    public Button closeButton;
    public Action onClose;

    private void OnEnable()
    {
        closeButton.onClick.AddListener(Close);
    }

    private void OnDisable()
    {
        closeButton.onClick.RemoveListener(Close);
    }

    public void Close()
    {
        gameObject.SetActive(false);
        onClose?.Invoke();
    }

    public void Open()
    {
        gameObject.SetActive(true);
    }
}
