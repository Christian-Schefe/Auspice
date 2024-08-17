using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PreviewVisuals : EntityVisuals
{
    public override int GetEntityOrder()
    {
        return 100;
    }
}
