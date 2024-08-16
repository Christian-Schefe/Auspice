using System.Collections.Generic;
using UnityEngine;
using Yeast;

[CreateAssetMenu(fileName = "LevelRegistry", menuName = "Data/LevelRegistry")]
public class LevelRegistry : ScriptableObject
{
    [SerializeField] private List<Entry> entries;

    private List<PuzzleData> puzzleData;

    [System.Serializable]
    public class Entry
    {
        public TextAsset levelData;
    }

    private void Initialize()
    {
        puzzleData = new List<PuzzleData>();
        foreach (var entry in entries)
        {
            if (entry.levelData != null && entry.levelData.text.TryFromJson<PuzzleData>(out var data))
            {
                puzzleData.Add(data);
            }
        }
    }

    public PuzzleData GetPuzzleData(int index)
    {
        if (puzzleData == null) Initialize();
        return puzzleData[index];
    }

    public PuzzleData GetPuzzleDataInstance(int index)
    {
        if (puzzleData == null) Initialize();
        return puzzleData[index].ToJson().FromJson<PuzzleData>();
    }

    public int GetLevelCount()
    {
        if (puzzleData == null) Initialize();
        return puzzleData.Count;
    }
}
