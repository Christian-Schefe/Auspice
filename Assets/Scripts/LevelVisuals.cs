using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;
using static UnityEditor.PlayerSettings;

public class LevelVisuals : MonoBehaviour
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

    [SerializeField] private Sprite buttonNotPressedSprite, buttonPressedSprite;

    private readonly Dictionary<(Vector2Int, PuzzleObject), GameObject> puzzleObjects = new();
    private readonly Dictionary<PuzzleEntity, GameObject> puzzleEntities = new();

    public void SetPositions(IEnumerable<Vector2Int> positions)
    {
        var min = positions.Aggregate(Vector2Int.Min);
        var max = positions.Aggregate(Vector2Int.Max);

        var camPos = WorldPos((Vector2)(max + min) * 0.5f);
        cam.transform.position = new Vector3(camPos.x, camPos.y, cam.transform.position.z);

        foreach (var pos in positions)
        {
            groundTilemap.SetTile((Vector3Int)pos, groundTile);
        }
    }

    public void AddWall(Vector2Int pos)
    {
        objectTilemap.SetTile((Vector3Int)pos, wallTile);
        puzzleObjects.Add((pos, PuzzleObject.Wall), null);
    }

    public void AddButton(ButtonEntity button)
    {
        var instance = Instantiate(buttonPrefab, WorldPos(button.position), Quaternion.identity);
        puzzleEntities.Add(button, instance);
    }

    public void AddPlayer(PlayerEntity player)
    {
        var instance = Instantiate(playerPrefab, WorldPos(player.position), Quaternion.identity);
        puzzleEntities.Add(player, instance);
    }

    public void AddChest(Vector2Int pos)
    {
        var instance = Instantiate(chestPrefab, WorldPos(pos), Quaternion.identity);
        puzzleObjects.Add((pos, PuzzleObject.Chest), instance);
    }

    public void AddOnSpikes(Vector2Int pos)
    {
        objectTilemap.SetTile((Vector3Int)pos, onSpikeTile);
        puzzleObjects.Add((pos, PuzzleObject.OnSpike), null);
    }

    public void AddOffSpikes(Vector2Int pos)
    {
        objectTilemap.SetTile((Vector3Int)pos, offSpikeTile);
        puzzleObjects.Add((pos, PuzzleObject.OffSpike), null);
    }

    public void UpdatePlayerPosition(PlayerEntity player)
    {
        foreach (var entity in puzzleEntities.Keys)
        {
            Debug.Log(entity.position + " " + (entity == player));
        }
        puzzleEntities[player].transform.position = WorldPos(player.position);
    }

    public void UpdateOffSpikeTile(Vector2Int pos, bool buttonState)
    {
        objectTilemap.SetTile((Vector3Int)pos, buttonState ? onSpikeTile : offSpikeTile);
    }

    public void UpdateOnSpikeTile(Vector2Int pos, bool buttonState)
    {
        objectTilemap.SetTile((Vector3Int)pos, buttonState ? offSpikeTile : onSpikeTile);
    }

    public void UpdateButtonState(ButtonEntity button)
    {
        var sprite = button.isPressed ? buttonPressedSprite : buttonNotPressedSprite;
        puzzleEntities[button].GetComponent<SpriteRenderer>().sprite = sprite;
    }

    public void Remove(Vector2Int pos)
    {
        var enumVals = System.Enum.GetValues(typeof(PuzzleObject)).Cast<PuzzleObject>();
        foreach (var objectType in enumVals)
        {
            if (!puzzleObjects.TryGetValue((pos, objectType), out var go)) continue;
            if (go != null) Destroy(go);
            puzzleObjects.Remove((pos, objectType));
        }

        var entities = puzzleEntities.Keys.Where(e => e.position == pos).ToList();
        foreach (var entity in entities)
        {
            if (!puzzleEntities.TryGetValue(entity, out var go)) continue;
            if (go != null) Destroy(go);
            puzzleEntities.Remove(entity);
        }

        Debug.Log("Removed: " + pos);

        objectTilemap.SetTile((Vector3Int)pos, null);
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
