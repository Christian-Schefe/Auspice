using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tweenables;

public class PlayerVisuals : EntityVisuals
{
    public SpriteRenderer spriteRenderer;
    public List<SpriteEntry> sprites;

    private Dictionary<PlayerType, Sprite> spriteMap;

    private void Awake()
    {
        spriteMap = new();
        foreach (var entry in sprites)
        {
            spriteMap.Add(entry.playerType, entry.sprite);
        }
    }

    public void SetType(EntityType type)
    {
        var sprite = spriteMap[type.playerType];
        spriteRenderer.sprite = sprite;
    }

    [System.Serializable]
    public class SpriteEntry
    {
        public PlayerType playerType;
        public Sprite sprite;
    }
}
