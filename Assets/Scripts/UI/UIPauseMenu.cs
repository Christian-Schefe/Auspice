using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIPauseMenu : MonoBehaviour
{
    public UISettingsMenu settingsMenu;
    public Button settingsButton;
    public Button returnToMenuButton;
    public UIModalWindow window;
    public Image raycastBlocker;

    public SceneSO mainMenuScene;

    private void Awake()
    {
        settingsButton.onClick.AddListener(OnSettingsButtonClicked);
        returnToMenuButton.onClick.AddListener(OnReturnToMenuButtonClicked);

        window.onClose += OnClose;
    }

    public void Open()
    {
        raycastBlocker.gameObject.SetActive(true);
        window.Open();
    }

    private void OnClose()
    {
        raycastBlocker.gameObject.SetActive(false);
    }

    public void OnSettingsButtonClicked()
    {
        settingsMenu.window.Open();
    }

    public void OnReturnToMenuButtonClicked()
    {
        SceneSystem.LoadScene(mainMenuScene);
    }
}
