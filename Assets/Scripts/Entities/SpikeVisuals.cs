using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpikeVisuals : EntityVisuals
{
    public SpriteRenderer spriteRenderer;
    public List<SpriteEntry> sprites;

    private ButtonColor color;
    private Dictionary<ButtonColor, (Sprite, Sprite)> spriteMap;

    private void Awake()
    {
        spriteMap = new();
        foreach (var sprite in sprites)
        {
            spriteMap.Add(sprite.color, (sprite.onSprite, sprite.offSprite));
        }
    }

    public void SetType(EntityType type)
    {
        color = type.buttonColor;
    }

    public void SetState(bool extended)
    {
        var (onSprite, offSprite) = spriteMap[color];
        spriteRenderer.sprite = extended ? onSprite : offSprite;
    }

    [System.Serializable]
    public class SpriteEntry
    {
        public ButtonColor color;
        public Sprite onSprite, offSprite;
    }
}
