using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SpriteRegistry", menuName = "Data/SpriteRegistry")]
public class SpriteRegistry : ScriptableObject
{
    [SerializeField] private List<Entry> entries = new();

    private Dictionary<EntityType, Sprite> spriteMap;

    private void Initialize()
    {
        spriteMap = new Dictionary<EntityType, Sprite>();
        foreach (var entry in entries)
        {
            spriteMap[entry.type] = entry.sprite;
        }
    }

    public Sprite GetSprite(EntityType type)
    {
        if (spriteMap == null) Initialize();

        if (spriteMap.TryGetValue(type, out var sprite))
        {
            return sprite;
        }

        throw new System.ArgumentException($"No sprite found for {type}");
    }

    [System.Serializable]
    public struct Entry
    {
        public EntityType type;
        public Sprite sprite;
    }
}
