using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonVisuals : EntityVisuals
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
            spriteMap.Add(sprite.color, (sprite.pressedSprite, sprite.notPressedSprite));
        }
    }

    public void SetType(EntityType type)
    {
        color = type.buttonColor;
    }

    public void SetState(bool pressed)
    {
        var (pressedSprite, notPressedSprite) = spriteMap[color];
        spriteRenderer.sprite = pressed ? pressedSprite : notPressedSprite;
    }

    [System.Serializable]
    public class SpriteEntry
    {
        public ButtonColor color;
        public Sprite pressedSprite, notPressedSprite;
    }
}
