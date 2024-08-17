using System.Collections;
using System.Collections.Generic;
using Tweenables;
using UnityEngine;

public class SpikeVisuals : EntityVisuals
{
    public void SetState(bool extended)
    {
        this.TweenDelayedAction(() => SetStateInstant(extended), animationSpeed * 0.5f).RunNew();
    }

    public void SetStateInstant(bool extended)
    {
        var switchedType = type;
        switchedType.spikeInitialState = extended;
        spriteRenderer.sprite = spriteRegistry.GetSprite(switchedType);
    }
}
