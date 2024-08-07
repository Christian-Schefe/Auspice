using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PuzzleData
{
    public HashSet<Vector2Int> positions = new();
    public Dictionary<Vector2Int, HashSet<PuzzleObject>> puzzleObjects = new();
    public Dictionary<Vector2Int, Dictionary<EntityType, PuzzleEntity>> puzzleEntities = new();

    public PuzzleData() { }
    public PuzzleData(HashSet<Vector2Int> positions)
    {
        this.positions = positions;
    }

    public void AddObject(Vector2Int pos, PuzzleObject obj)
    {
        if (!puzzleObjects.ContainsKey(pos))
        {
            puzzleObjects.Add(pos, new HashSet<PuzzleObject> { obj });
        }
        else
        {
            puzzleObjects[pos].Add(obj);
        }
    }

    public void AddEntity(PuzzleEntity entity)
    {
        if (!puzzleEntities.ContainsKey(entity.position))
        {
            puzzleEntities.Add(entity.position, new Dictionary<EntityType, PuzzleEntity> { { entity.GetEntityType(), entity } });
        }
        else
        {
            puzzleEntities[entity.position].Add(entity.GetEntityType(), entity);
        }
    }

    public void Remove(Vector2Int pos)
    {
        if (puzzleObjects.ContainsKey(pos))
        {
            puzzleObjects.Remove(pos);
        }

        if (puzzleEntities.TryGetValue(pos, out var entities))
        {
            puzzleEntities.Remove(pos);
        }
    }
}
