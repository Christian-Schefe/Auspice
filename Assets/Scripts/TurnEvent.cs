using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnEvent
{

}

public class TeleportEvent : TurnEvent
{
    public PuzzleEntity entity;
    public Vector2Int via;

    public TeleportEvent() { }

    public TeleportEvent(PuzzleEntity entity, Vector2Int via)
    {
        this.entity = entity;
        this.via = via;
    }
}

public class ShiftEvent : TurnEvent
{
    public PuzzleEntity entity;
    public Vector2Int via;

    public ShiftEvent() { }

    public ShiftEvent(PuzzleEntity entity, Vector2Int via)
    {
        this.entity = entity;
        this.via = via;
    }
}