using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIMainMenu : MonoBehaviour
{
    public readonly PersistentValue<int?> selectedLevel = new("selectedLevelIndex", PersistenceMode.GlobalRuntime, null);

    public Button levelMenuButton;
    public Button editorButton;
    public Button settingsMenuButton;
    public Button quitButton;

    public UILevelMenu levelMenu;
    public UISettingsMenu settingsMenu;

    public SceneSO levelScene;

    private void Awake()
    {
        levelMenuButton.onClick.AddListener(OnLevelMenuButtonClicked);
        editorButton.onClick.AddListener(OnEditorButtonClicked);
        settingsMenuButton.onClick.AddListener(OnSettingsMenuButtonClicked);
        quitButton.onClick.AddListener(OnQuitButtonClicked);
    }

    public void OnLevelMenuButtonClicked()
    {
        levelMenu.window.Open();
    }

    public void OnEditorButtonClicked()
    {
        selectedLevel.Set(null);
        SceneSystem.LoadScene(levelScene);
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
