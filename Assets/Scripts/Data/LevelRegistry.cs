using System.Collections.Generic;
using UnityEngine;
using Yeast;

[CreateAssetMenu(fileName = "LevelRegistry", menuName = "Data/LevelRegistry")]
public class LevelRegistry : ScriptableObject
{
    [SerializeField] private List<Entry> entries;

    private List<PuzzleData> puzzleData;
    private List<string> puzzleHashes;

    [System.Serializable]
    public class Entry
    {
        public TextAsset levelData;
    }

    private void Initialize()
    {
        puzzleData = new List<PuzzleData>();
        puzzleHashes = new List<string>();
        var shouldRemove = new HashSet<int>();
        for (int i = 0; i < entries.Count; i++)
        {
            var entry = entries[i];
            if (entry.levelData != null && entry.levelData.text.TryFromJson<PuzzleData>(out var data))
            {
                puzzleData.Add(data);
                puzzleHashes.Add(data.ComputeHash());
            }
            else
            {
                shouldRemove.Add(i);
            }
        }

        for (int i = entries.Count - 1; i >= 0; i--)
        {
            if (shouldRemove.Contains(i))
            {
                entries.RemoveAt(i);
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

    public string GetPuzzleHash(int index)
    {
        if (puzzleData == null) Initialize();
        return puzzleHashes[index];
    }

    public int GetLevelCount()
    {
        if (puzzleData == null) Initialize();
        return puzzleData.Count;
    }

    public void AddLevel(TextAsset levelData)
    {
        if (puzzleData == null) Initialize();
        entries.Add(new Entry { levelData = levelData });
        if (levelData.text.TryFromJson<PuzzleData>(out var data))
        {
            puzzleData.Add(data);
            puzzleHashes.Add(data.ComputeHash());
        }
    }
}
