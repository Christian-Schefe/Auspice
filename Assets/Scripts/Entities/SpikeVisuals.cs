using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpikeVisuals : EntityVisuals
{
    public void SetState(bool extended)
    {
        var switchedType = type;
        switchedType.spikeInitialState = extended;
        spriteRenderer.sprite = spriteRegistry.GetSprite(switchedType);
    }
}
