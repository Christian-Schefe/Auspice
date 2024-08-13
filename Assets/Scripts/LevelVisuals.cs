using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public class LevelVisuals : MonoBehaviour
{
    [SerializeField] private Camera cam;

    [SerializeField] private TileBase groundTile;
    [SerializeField] private TileBase wallTile;
    [SerializeField] private TileBase iceTile;

    [SerializeField] private Tilemap groundTilemap;
    [SerializeField] private Tilemap objectTilemap;

    [SerializeField] private SpikeVisuals spikePrefab;
    [SerializeField] private ButtonVisuals buttonPrefab;
    [SerializeField] private PlayerVisuals playerPrefab;
    [SerializeField] private EntityVisuals chestPrefab;
    [SerializeField] private ConveyorVisuals conveyorPrefab;

    private readonly Dictionary<PuzzleEntity, EntityVisuals> puzzleEntities = new();

    public void SetData(PuzzleData data)
    {
        var min = data.positions.Aggregate(Vector2Int.Min);
        var max = data.positions.Aggregate(Vector2Int.Max);

        var camPos = WorldPos((Vector2)(max + min) * 0.5f);
        cam.transform.position = new Vector3(camPos.x, camPos.y, cam.transform.position.z);

        foreach (var pos in data.positions)
        {
            groundTilemap.SetTile((Vector3Int)pos, groundTile);

            if (!data.entities.TryGetValue(pos, out var entities)) continue;
            foreach (var (type, (entity, _)) in entities)
            {
                AddEntity(entity, false);
            }
        }
    }

    public void AddEntity(PuzzleEntity entity, bool playSFX)
    {
        var type = entity.GetEntityType();
        if (type.basicType == PuzzleEntityType.Wall) AddWall(entity);
        else if (type.basicType == PuzzleEntityType.Button) AddButton(entity);
        else if (type.basicType == PuzzleEntityType.Player) AddPlayer(entity);
        else if (type.basicType == PuzzleEntityType.Chest) AddChest(entity);
        else if (type.basicType == PuzzleEntityType.Spike) AddSpikes(entity);
        else if (type.basicType == PuzzleEntityType.Conveyor) AddConveyor(entity);
        else if (type.basicType == PuzzleEntityType.Ice) AddIce(entity);
        else throw new System.NotImplementedException();

        if (playSFX) SFX.Play(SFX.Type.Place);
    }

    public void AddWall(PuzzleEntity entity)
    {
        objectTilemap.SetTile((Vector3Int)entity.position, wallTile);
        puzzleEntities.Add(entity, null);
    }

    public void AddIce(PuzzleEntity entity)
    {
        objectTilemap.SetTile((Vector3Int)entity.position, iceTile);
        puzzleEntities.Add(entity, null);
    }

    public void AddButton(PuzzleEntity button)
    {
        var instance = Instantiate(buttonPrefab, WorldPos(button.position), Quaternion.identity);
        instance.SetType(button.GetEntityType());
        instance.SetState(false);
        puzzleEntities.Add(button, instance);
    }

    public void AddPlayer(PuzzleEntity player)
    {
        var instance = Instantiate(playerPrefab, WorldPos(player.position), Quaternion.identity);
        instance.SetType(player.GetEntityType());
        puzzleEntities.Add(player, instance);
    }

    public void AddChest(PuzzleEntity chest)
    {
        var instance = Instantiate(chestPrefab, WorldPos(chest.position), Quaternion.identity);
        puzzleEntities.Add(chest, instance);
    }

    public void AddSpikes(PuzzleEntity spike)
    {
        var type = spike.GetEntityType();
        var instance = Instantiate(spikePrefab, WorldPos(spike.position), Quaternion.identity);
        instance.SetType(type);
        instance.SetState(type.spikeInitialState);
        puzzleEntities.Add(spike, instance);
    }

    public void AddConveyor(PuzzleEntity conveyor)
    {
        var type = conveyor.GetEntityType();
        var instance = Instantiate(conveyorPrefab, WorldPos(conveyor.position), Quaternion.identity);
        instance.SetType(type);
        puzzleEntities.Add(conveyor, instance);
    }

    public void UpdateEntityPosition(PuzzleEntity entity)
    {
        puzzleEntities[entity].SetPosition(WorldPos(entity.position));
    }

    public void UpdateButtonState(ButtonEntity button)
    {
        puzzleEntities[button].GetComponent<ButtonVisuals>().SetState(button.isPressed);
    }

    public void UpdateSpikeState(PuzzleEntity spike, bool buttonState)
    {
        puzzleEntities[spike].GetComponent<SpikeVisuals>().SetState(buttonState != spike.GetEntityType().spikeInitialState);
    }

    public void Remove(Vector2Int pos)
    {
        var entities = puzzleEntities.Keys.Where(e => e.position == pos).ToList();
        foreach (var entity in entities)
        {
            if (!puzzleEntities.TryGetValue(entity, out var go)) continue;
            if (go != null) Destroy(go.gameObject);
            puzzleEntities.Remove(entity);
        }

        objectTilemap.SetTile((Vector3Int)pos, null);
        SFX.Play(SFX.Type.Erase);
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

        return flooredWorld + interX + interY;
    }

    public Vector2Int CellPos(Vector3 pos)
    {
        return (Vector2Int)groundTilemap.WorldToCell(pos);
    }

    public Vector2Int MouseCellPos()
    {
        var mousePos = (Vector2)Input.mousePosition;
        var worldPos = cam.ScreenToWorldPoint(mousePos);
        return CellPos(worldPos);
    }
}
