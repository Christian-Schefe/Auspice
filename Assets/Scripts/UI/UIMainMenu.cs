using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIMainMenu : MonoBehaviour
{
    public Button levelMenuButton;
    public Button settingsMenuButton;
    public Button quitButton;

    public UILevelMenu levelMenu;
    public UISettingsMenu settingsMenu;

    private void Awake()
    {
        levelMenuButton.onClick.AddListener(OnLevelMenuButtonClicked);
        settingsMenuButton.onClick.AddListener(OnSettingsMenuButtonClicked);
        quitButton.onClick.AddListener(OnQuitButtonClicked);
    }

    public void OnLevelMenuButtonClicked()
    {
        levelMenu.window.Open();
    }

    public void OnSettingsMenuButtonClicked()
    {
        settingsMenu.window.Open();
    }

    public void OnQuitButtonClicked()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
