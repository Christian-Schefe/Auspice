using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConveyorVisuals : EntityVisuals
{
    public SpriteRenderer spriteRenderer;
    public List<SpriteEntry> sprites;

    private Dictionary<Direction, Sprite> spriteMap;

    private void Awake()
    {
        spriteMap = new();
        foreach (var entry in sprites)
        {
            spriteMap.Add(entry.direction, entry.sprite);
        }
    }

    public void SetType(EntityType type)
    {
        spriteRenderer.sprite = spriteMap[type.direction];
    }

    [System.Serializable]
    public class SpriteEntry
    {
        public Direction direction;
        public Sprite sprite;
    }
}
