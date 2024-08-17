using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PuzzleData
{
    public HashSet<Vector2Int> positions = new();
    public Dictionary<Vector2Int, Dictionary<EntityType, (PuzzleEntity, bool)>> entities = new();

    public Dictionary<BuildEntityType, int?> editableEntities = new();
    public List<int> starTresholds = new();

    public PuzzleData() { }

    public void SetPositions(HashSet<Vector2Int> positions) => this.positions = positions;

    public bool CanAddEntity(PuzzleEntity entity)
    {
        if (!positions.Contains(entity.position)) return false;
        if (!entities.TryGetValue(entity.position, out var dict)) return true;
        return !dict.ContainsKey(entity.GetEntityType());
    }

    public bool TryAddEntity(PuzzleEntity entity, bool isEditable = true)
    {
        if (!CanAddEntity(entity)) return false;
        AddEntity(entity, isEditable);
        return true;
    }

    public void AddEntity(PuzzleEntity entity, bool isEditable = true)
    {
        if (!entities.ContainsKey(entity.position))
        {
            entities.Add(entity.position, new Dictionary<EntityType, (PuzzleEntity, bool)> { { entity.GetEntityType(), (entity, isEditable) } });
        }
        else
        {
            entities[entity.position].Add(entity.GetEntityType(), (entity, isEditable));
        }
    }

    public bool TryGetEditableEntity<T>(Vector2Int pos, EntityType type, out T entity) where T : PuzzleEntity
    {
        if (entities.ContainsKey(pos) && entities[pos].TryGetValue(type, out var e) && e.Item2)
        {
            entity = (T)e.Item1;
            return true;
        }
        entity = null;
        return false;
    }

    public List<PuzzleEntity> Remove(Vector2Int pos)
    {
        var removedEntities = new List<PuzzleEntity>();

        if (entities.ContainsKey(pos))
        {
            foreach (var (type, (e, isEditable)) in entities[pos].ToList())
            {
                if (!isEditable) continue;
                entities[pos].Remove(type);
                removedEntities.Add(e);
            }
        }
        return removedEntities;
    }

    public bool RemoveEntity(Vector2Int pos, EntityType type)
    {
        if (entities.ContainsKey(pos) && entities[pos].TryGetValue(type, out var e) && e.Item2)
        {
            entities[pos].Remove(type);
            return true;
        }
        return false;
    }
}
