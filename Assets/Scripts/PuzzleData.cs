using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Yeast;

public class PuzzleData
{
    public HashSet<Vector2Int> positions = new();
    public Dictionary<Vector2Int, Dictionary<EntityType, (PuzzleEntity, bool)>> entities = new();

    public Dictionary<BuildEntityType, int?> buildableEntityCounts = new();
    public List<int> starTresholds = new();

    public PuzzleData() { }

    public PuzzleData Clone(bool setUneditable = false)
    {
        var clone = new PuzzleData()
        {
            positions = new HashSet<Vector2Int>(positions),
            entities = new Dictionary<Vector2Int, Dictionary<EntityType, (PuzzleEntity, bool)>>(),
            buildableEntityCounts = new Dictionary<BuildEntityType, int?>(buildableEntityCounts),
            starTresholds = new List<int>(starTresholds)
        };

        foreach (var (pos, dict) in entities)
        {
            clone.entities.Add(pos, new Dictionary<EntityType, (PuzzleEntity, bool)>());
            foreach (var (type, (e, isEditable)) in dict)
            {
                clone.entities[pos].Add(type, (e.Clone(), isEditable && !setUneditable));
            }
        }
        return clone;
    }

    public HashSet<Vector2Int> GetUsedPositions()
    {
        var usedPositions = new HashSet<Vector2Int>();
        foreach (var (pos, dict) in entities)
        {
            if (dict.Count > 0) usedPositions.Add(pos);
        }
        return usedPositions;
    }

    public void SetBuildableEntityCounts(Dictionary<BuildEntityType, int?> buildableEntityCounts) => this.buildableEntityCounts = buildableEntityCounts;

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
            if (entities[pos].Count == 0) entities.Remove(pos);
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

    public string ComputeHash()
    {
        var bytes = this.ToBytes();
        using var md5 = System.Security.Cryptography.SHA256.Create();
        var hash = md5.ComputeHash(bytes);
        return System.BitConverter.ToString(hash).Replace("-", "").ToLower();
    }
}
