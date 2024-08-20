using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Yeast;

public class UICreationMenu : MonoBehaviour
{
    public Main main;
    public Button exportButton;
    public PuzzleEditor puzzleEditor;
    public LevelRegistry levelRegistry;

    private void Start()
    {
        if (puzzleEditor.IsEditMode())
        {
            exportButton.gameObject.SetActive(true);
            exportButton.onClick.AddListener(ExportLevel);
        }
        else
        {
            exportButton.gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        var interactable = main.CurrentState == MainState.Editing;
        exportButton.interactable = interactable;
    }

    public void ExportLevel()
    {
        var puzzle = puzzleEditor.GetEditedPuzzleDataClone();
        var levelJson = puzzle.ToJson();

        //test roundtrip
        var parsedData = levelJson.FromJson<PuzzleData>();
        var roundtripJson = parsedData.ToJson();
        Debug.Log(levelJson == roundtripJson);

        WriteTextAsset("level_" + System.DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss"), levelJson);
    }

    private void WriteTextAsset(string levelName, string json)
    {
#if UNITY_EDITOR
        var path = $"Assets/Resources/Levels/{levelName}.json";
        System.IO.File.WriteAllText(path, json);
        UnityEditor.AssetDatabase.Refresh();

        var asset = UnityEditor.AssetDatabase.LoadAssetAtPath<TextAsset>(path);
        levelRegistry.AddLevel(asset);
#endif
    }
}
