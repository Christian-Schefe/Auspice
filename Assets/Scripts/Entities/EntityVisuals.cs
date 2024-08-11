using System.Collections;
using System.Collections.Generic;
using Tweenables;
using UnityEngine;

public class EntityVisuals : MonoBehaviour
{
    public void SetPosition(Vector3 pos)
    {
        this.TweenPosition().To(pos).Duration(0.15f).Ease(Easing.QuadInOut).RunNew();
    }
}
