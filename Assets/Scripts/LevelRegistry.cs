using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LevelRegistry", menuName = "LevelRegistry")]
public class LevelRegistry : ScriptableObject
{
    public List<Entry> entries;

    [System.Serializable]
    public class Entry
    {
        public TextAsset levelData;
    }
}
