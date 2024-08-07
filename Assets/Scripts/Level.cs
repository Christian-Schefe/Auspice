using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Level : MonoBehaviour
{
    [SerializeField] private Camera cam;

    [SerializeField] private TileBase groundTile;
    [SerializeField] private TileBase wallTile;
    [SerializeField] private TileBase onSpikeTile;
    [SerializeField] private TileBase offSpikeTile;

    [SerializeField] private Tilemap groundTilemap;
    public Tilemap objectTilemap;

    [SerializeField] private GameObject buttonPrefab;
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private GameObject chestPrefab;

    private List<Vector2Int> positions;
    private Dictionary<Vector2Int, (Puzzle.PuzzleObject, GameObject)> objects;
    private Dictionary<Vector2Int, (EntityType, System.Func<PuzzleEntity>, GameObject)> entities;

    private Dictionary<EntityType, List<GameObject>> entitiesByType;
    public Vector2Int size;

    private void Awake()
    {
        positions = new List<Vector2Int>();
        objects = new();
        entities = new();
        entitiesByType = new();

        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                var pos = new Vector2Int(x, y);
                positions.Add(pos);
            }
        }

        foreach (var pos in positions)
        {
            groundTilemap.SetTile((Vector3Int)pos, groundTile);
            if (pos.x == 0 || pos.x == size.x - 1 || pos.y == 0 || pos.y == size.y - 1)
            {
                objectTilemap.SetTile((Vector3Int)pos, wallTile);
                objects[pos] = (Puzzle.PuzzleObject.Wall, null);
            }
        }

        var camPos = WorldPos((Vector2)(size - Vector2Int.one) * 0.5f);
        cam.transform.position = new Vector3(camPos.x, camPos.y, cam.transform.position.z);
    }

    public List<GameObject> GetEntityGameObjects(EntityType type)
    {
        if (!entitiesByType.ContainsKey(type))
        {
            return new();
        }
        return entitiesByType[type];
    }

    public Puzzle GetPuzzle()
    {
        var puzzle = new Puzzle(positions);
        foreach (var (pos, (objType, _)) in objects)
        {
            puzzle.AddObject(pos, objType);
        }
        foreach (var (_, (_, entityFactory, _)) in entities)
        {
            puzzle.AddEntity(entityFactory.Invoke());
        }


        return puzzle;
    }

    public bool IsInsideArea(Vector2Int pos)
    {
        return pos.x > 0 && pos.x < size.x - 1 && pos.y > 0 && pos.y < size.y - 1;
    }

    public void Erase(Vector2Int pos)
    {
        if (!IsInsideArea(pos)) return;
        objectTilemap.SetTile((Vector3Int)pos, null);
        if (objects.Remove(pos, out var objData) && objData.Item2 != null)
        {
            Destroy(objData.Item2);
        }
        if (entities.Remove(pos, out var entityData) && entityData.Item3 != null)
        {
            Destroy(entityData.Item3);
        }
        if (entitiesByType.ContainsKey(entityData.Item1))
        {
            entitiesByType[entityData.Item1].Remove(entityData.Item3);
        }
    }

    public bool CanPlace(Vector2Int pos)
    {
        return IsInsideArea(pos) && !objects.ContainsKey(pos) && !entities.ContainsKey(pos);
    }

    public void SetWall(Vector2Int pos)
    {
        if (!CanPlace(pos)) return;

        objects.Add(pos, (Puzzle.PuzzleObject.Wall, null));
        objectTilemap.SetTile((Vector3Int)pos, wallTile);
    }

    public void SetButton(Vector2Int pos)
    {
        if (!CanPlace(pos)) return;

        var instance = Instantiate(buttonPrefab, WorldPos(pos), Quaternion.identity);
        AddEntity(() => new ButtonEntity(pos), instance, pos);
    }

    public void SetSpikes(bool initialState, Vector2Int pos)
    {
        if (!CanPlace(pos)) return;

        objects.Add(pos, (initialState ? Puzzle.PuzzleObject.OnSpike : Puzzle.PuzzleObject.OffSpike, null));
        objectTilemap.SetTile((Vector3Int)pos, initialState ? onSpikeTile : offSpikeTile);
    }

    public void SetPlayer(Vector2Int pos)
    {
        if (!CanPlace(pos)) return;

        var instance = Instantiate(playerPrefab, WorldPos(pos), Quaternion.identity);
        AddEntity(() => new CrabPlayer(pos), instance, pos);
    }

    public void SetChest(Vector2Int pos)
    {
        if (!CanPlace(pos)) return;

        var instance = Instantiate(chestPrefab, WorldPos(pos), Quaternion.identity);
        objects.Add(pos, (Puzzle.PuzzleObject.Chest, instance));
    }

    private void AddEntity(System.Func<PuzzleEntity> entityFactory, GameObject obj, Vector2Int pos)
    {
        var dummy = entityFactory.Invoke();
        var type = dummy.GetEntityType();
        entities.Add(pos, (type, entityFactory, obj));
        if (!entitiesByType.ContainsKey(type))
        {
            entitiesByType[type] = new List<GameObject>() { obj };
        }
        else
        {
            entitiesByType[type].Add(obj);
        }
    }

    public Vector3 WorldPos(Vector2Int pos)
    {
        return groundTilemap.GetCellCenterWorld((Vector3Int)pos);
    }

    public Vector3 WorldPos(Vector2 pos)
    {
        var floored = Vector2Int.FloorToInt(pos);
        var frac = pos - floored;
        var flooredWorld = WorldPos(floored);

        var ceilX = floored + Vector2Int.right;
        var ceilY = floored + Vector2Int.up;

        var interX = (WorldPos(ceilX) - flooredWorld) * frac.x;
        var interY = (WorldPos(ceilY) - flooredWorld) * frac.y;

        print(frac);
        print(flooredWorld);
        print(interX);
        print(interY);

        return flooredWorld + interX + interY;
    }

    public Vector2Int CellPos(Vector3 pos)
    {
        return (Vector2Int)groundTilemap.WorldToCell(pos);
    }
}
