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
    [SerializeField] private Tilemap objectTilemap;

    [SerializeField] private GameObject buttonPrefab;
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private GameObject chestPrefab;

    private Dictionary<Vector2Int, GameObject> objects;
    public List<GameObject> players, buttons;

    public Vector2Int size;
    private Puzzle puzzle;

    private void Awake()
    {
        var positions = new List<Vector2Int>();
        objects = new();

        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                var pos = new Vector2Int(x, y);
                positions.Add(pos);
                objects.Add(pos, null);
            }
        }

        puzzle = new Puzzle(positions);

        foreach (var pos in positions)
        {
            groundTilemap.SetTile((Vector3Int)pos, groundTile);
            if (pos.x == 0 || pos.x == size.x - 1 || pos.y == 0 || pos.y == size.y - 1)
            {
                objectTilemap.SetTile((Vector3Int)pos, wallTile);
                puzzle.AddObject(pos, Puzzle.PuzzleObject.Wall);
            }
        }

        var camPos = WorldPos(new Vector2Int(size.x / 2, size.y / 2));
        cam.transform.position = new Vector3(camPos.x, camPos.y, cam.transform.position.z);
    }

    public Puzzle GetPuzzle()
    {
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
        puzzle.RemoveObjects(pos);
        puzzle.RemoveEntities(pos);
        if (objects[pos] != null)
        {
            players.Remove(objects[pos]);
            buttons.Remove(objects[pos]);
            Destroy(objects[pos]);
            objects[pos] = null;
        }
    }

    public void SetWall(Vector2Int pos)
    {
        if (!IsInsideArea(pos)) return;
        if (objects[pos] != null || objectTilemap.HasTile((Vector3Int)pos)) return;

        objectTilemap.SetTile((Vector3Int)pos, wallTile);
        puzzle.AddObject(pos, Puzzle.PuzzleObject.Wall);
    }

    public void SetButton(Vector2Int pos)
    {
        if (!IsInsideArea(pos)) return;
        if (objects[pos] != null || objectTilemap.HasTile((Vector3Int)pos)) return;

        var instance = Instantiate(buttonPrefab, WorldPos(pos), Quaternion.identity);
        objects[pos] = instance;
        puzzle.AddEntity(new ButtonEntity(pos));
        buttons.Add(instance);
    }

    public void SetSpikes(bool initialState, Vector2Int pos)
    {
        if (!IsInsideArea(pos)) return;
        if (objects[pos] != null || objectTilemap.HasTile((Vector3Int)pos)) return;

        objectTilemap.SetTile((Vector3Int)pos, initialState ? onSpikeTile : offSpikeTile);
        puzzle.AddObject(pos, initialState ? Puzzle.PuzzleObject.OnSpike : Puzzle.PuzzleObject.OffSpike);
    }

    public void SetPlayer(Vector2Int pos)
    {
        if (!IsInsideArea(pos)) return;
        if (objects[pos] != null || objectTilemap.HasTile((Vector3Int)pos)) return;

        var instance = Instantiate(playerPrefab, WorldPos(pos), Quaternion.identity);
        objects[pos] = instance;
        puzzle.AddEntity(new CrabPlayer(pos));
        players.Add(instance);
    }

    public void SetChest(Vector2Int pos)
    {
        if (!IsInsideArea(pos)) return;
        if (objects[pos] != null || objectTilemap.HasTile((Vector3Int)pos)) return;

        var instance = Instantiate(chestPrefab, WorldPos(pos), Quaternion.identity);
        objects[pos] = instance;
        puzzle.AddObject(pos, Puzzle.PuzzleObject.Chest);
    }

    public Vector3 WorldPos(Vector2Int pos)
    {
        return groundTilemap.GetCellCenterWorld((Vector3Int)pos);
    }

    public Vector2Int CellPos(Vector3 pos)
    {
        return (Vector2Int)groundTilemap.WorldToCell(pos);
    }
}
