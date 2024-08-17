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
    [SerializeField] private ButtonVisuals pressurePlatePrefab;
    [SerializeField] private PortalVisuals portalPrefab;
    [SerializeField] private EntityVisuals entityVisuals;

    [SerializeField] private PreviewVisuals preview;

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

    public void PreviewEntity(Vector2Int position, EntityType type, bool visible)
    {
        preview.gameObject.SetActive(visible);
        if (!visible) return;
        preview.SetType(type);
        preview.transform.position = WorldPos(position);
    }

    public void AddEntity(PuzzleEntity entity, bool playSFX)
    {
        var type = entity.GetEntityType();
        if (type.basicType == PuzzleEntityType.Ice) AddTileEntity(entity, iceTile);
        else if (type.basicType == PuzzleEntityType.Wall) AddTileEntity(entity, wallTile);
        else if (type.basicType == PuzzleEntityType.Button) AddButton(entity);
        else if (type.basicType == PuzzleEntityType.PressurePlate) AddPressurePlate(entity);
        else if (type.basicType == PuzzleEntityType.Spike) AddSpikes(entity);
        else if (type.basicType == PuzzleEntityType.Portal) AddPortal(entity);
        else AddEntity(entity);

        if (playSFX) SFX.Play(SFX.Type.Place);
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
        instance.SetStateInstant(false);
        puzzleEntities.Add(button, instance);
    }

    public void AddPressurePlate(PuzzleEntity button)
    {
        var instance = Instantiate(pressurePlatePrefab, WorldPos(button.position), Quaternion.identity);
        instance.SetType(button.GetEntityType());
        instance.SetStateInstant(false);
        puzzleEntities.Add(button, instance);
    }

    public void AddPortal(PuzzleEntity portal)
    {
        var instance = Instantiate(portalPrefab, WorldPos(portal.position), Quaternion.identity);
        instance.SetType(portal.GetEntityType());
        instance.SetDestination(WorldPos(((PortalEntity)portal).destination));
        puzzleEntities.Add(portal, instance);
    }

    public void AddEntity(PuzzleEntity entity)
    {
        var instance = Instantiate(entityVisuals, WorldPos(entity.position), Quaternion.identity);
        instance.SetType(entity.GetEntityType());
        puzzleEntities.Add(entity, instance);
    }

    public void AddTileEntity(PuzzleEntity entity, TileBase tile)
    {
        objectTilemap.SetTile((Vector3Int)entity.position, tile);
        puzzleEntities.Add(entity, null);
    }

    public void AddSpikes(PuzzleEntity spike)
    {
        var type = spike.GetEntityType();
        var instance = Instantiate(spikePrefab, WorldPos(spike.position), Quaternion.identity);
        instance.SetType(type);
        instance.SetStateInstant(type.spikeInitialState);
        puzzleEntities.Add(spike, instance);
    }

    public float UpdateEntityPosition(PuzzleEntity entity, ShiftEvent shiftEvent = null, TeleportEvent teleportEvent = null)
    {
        if (shiftEvent != null && teleportEvent != null) return puzzleEntities[entity].ShiftTeleportSetPosition(WorldPos(entity.position), WorldPos(shiftEvent.via), WorldPos(teleportEvent.via));
        else if (shiftEvent != null) return puzzleEntities[entity].ShiftSetPosition(WorldPos(entity.position), WorldPos(shiftEvent.via));
        else if (teleportEvent != null) return puzzleEntities[entity].TeleportSetPosition(WorldPos(entity.position), WorldPos(teleportEvent.via));
        else return puzzleEntities[entity].SetPosition(WorldPos(entity.position));
    }

    public void UpdateButtonState(ButtonEntity button)
    {
        ((ButtonVisuals)puzzleEntities[button]).SetState(button.isPressed);
    }

    public void UpdatePressurePlateState(PressurePlateEntity pressurePlate)
    {
        ((ButtonVisuals)puzzleEntities[pressurePlate]).SetState(pressurePlate.isPressed);
    }

    public void UpdateSpikeState(PuzzleEntity spike, bool buttonState)
    {
        ((SpikeVisuals)puzzleEntities[spike]).SetState(buttonState != spike.GetEntityType().spikeInitialState);
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

    public void RemoveEntity(Vector2Int pos, EntityType type)
    {
        var entities = puzzleEntities.Keys.Where(e => e.position == pos && e.GetEntityType() == type).ToList();
        foreach (var entity in entities)
        {
            if (!puzzleEntities.TryGetValue(entity, out var go)) continue;
            if (go != null) Destroy(go.gameObject);
            else objectTilemap.SetTile((Vector3Int)pos, null);
            puzzleEntities.Remove(entity);
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
