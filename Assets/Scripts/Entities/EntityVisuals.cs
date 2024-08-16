using System.Collections;
using System.Collections.Generic;
using Tweenables;
using UnityEngine;

public class EntityVisuals : MonoBehaviour
{
    public SpriteRegistry spriteRegistry;
    public SpriteRenderer spriteRenderer;

    protected Sprite typeDefaultSprite;
    protected EntityType type;

    private const float animationSpeed = 0.2f;

    public float SetPosition(Vector3 pos)
    {
        this.TweenPosition().To(pos).Duration(animationSpeed).Ease(Easing.Linear).RunNew();

        return animationSpeed;
    }

    public float ShiftSetPosition(Vector3 pos, Vector3 via)
    {
        var runner = this.TweenPosition().To(via).Duration(animationSpeed).Ease(Easing.Linear).RunNew();
        runner.RunQueued(this.TweenPosition().From(via).To(pos).Duration(animationSpeed).Ease(Easing.CubicOut));

        return animationSpeed * 2;
    }

    public float TeleportSetPosition(Vector3 pos, Vector3 via)
    {
        var runner = this.TweenPosition().To(via).Duration(animationSpeed).Ease(Easing.Linear).RunNew();
        this.TweenScale().To(Vector3.zero).Duration(animationSpeed * 0.5f).Ease(Easing.CubicIn).OnFinally(() =>
        {
            transform.position = pos;
        }).RunQueued(ref runner);
        this.TweenScale().From(Vector3.zero).To(Vector3.one).Duration(animationSpeed * 0.5f).Ease(Easing.CubicOut).RunQueued(ref runner);

        return animationSpeed * 2;
    }

    public float ShiftTeleportSetPosition(Vector3 pos, Vector3 shiftVia, Vector3 teleportVia)
    {
        var moveRunner = this.TweenPosition().To(shiftVia).Duration(animationSpeed).Ease(Easing.Linear).RunNew();
        this.TweenPosition().From(shiftVia).To(teleportVia).Duration(animationSpeed).Ease(Easing.CubicOut).RunQueued(ref moveRunner);

        this.TweenScale().To(Vector3.zero).Duration(animationSpeed * 0.5f).Ease(Easing.CubicIn).OnFinally(() =>
        {
            transform.position = pos;
        }).RunQueued(ref moveRunner);
        this.TweenScale().From(Vector3.zero).To(Vector3.one).Duration(animationSpeed * 0.5f).Ease(Easing.CubicOut).RunQueued(ref moveRunner);

        return animationSpeed * 3;
    }

    public void SetType(EntityType type)
    {
        this.type = type;
        typeDefaultSprite = spriteRegistry.GetSprite(type);
        spriteRenderer.sprite = typeDefaultSprite;
        spriteRenderer.sortingOrder = GetEntityOrder(type);
    }

    public static int GetEntityOrder(EntityType type)
    {
        var baseType = type.basicType;
        return baseType switch
        {
            PuzzleEntityType.Player => 2,
            _ => 1,
        };
    }
}
