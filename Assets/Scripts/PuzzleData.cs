using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PuzzleData
{
    public HashSet<Vector2Int> positions = new();
    public Dictionary<Vector2Int, Dictionary<PuzzleEntityType, (PuzzleEntity, bool)>> entities = new();

    public Dictionary<PuzzleEntityType, int?> editableEntities = new();

    public PuzzleData() { }

    public void SetPositions(HashSet<Vector2Int> positions) => this.positions = positions;

    public bool TryAddEntity(PuzzleEntity entity, bool isEditable = true)
    {
        if (!entities.ContainsKey(entity.position))
        {
            entities.Add(entity.position, new Dictionary<PuzzleEntityType, (PuzzleEntity, bool)> { { entity.GetEntityType(), (entity, isEditable) } });
            return true;
        }
        else if (!entities[entity.position].ContainsKey(entity.GetEntityType()))
        {
            entities[entity.position].Add(entity.GetEntityType(), (entity, isEditable));
            return true;
        }
        return false;
    }

    public List<PuzzleEntityType> Remove(Vector2Int pos)
    {
        var removedEntities = new List<PuzzleEntityType>();

        if (entities.ContainsKey(pos))
        {
            foreach (var (type, (_, isEditable)) in entities[pos].ToList())
            {
                if (!isEditable) continue;
                entities[pos].Remove(type);
                removedEntities.Add(type);
            }
        }
        return removedEntities;
    }
}
