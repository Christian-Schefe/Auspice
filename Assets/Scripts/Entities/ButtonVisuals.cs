using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonVisuals : EntityVisuals
{
    public List<SpriteEntry> sprites;
    private Dictionary<ButtonColor, Sprite> pressedSprites;

    private void Awake()
    {
        pressedSprites = new();
        foreach (var sprite in sprites)
        {
            pressedSprites.Add(sprite.color, sprite.pressedSprite);
        }
    }

    public void SetState(bool pressed)
    {
        var pressedSprite = pressedSprites[type.buttonColor];
        spriteRenderer.sprite = pressed ? pressedSprite : typeDefaultSprite;
    }

    [System.Serializable]
    public class SpriteEntry
    {
        public ButtonColor color;
        public Sprite pressedSprite;
    }
}
