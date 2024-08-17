using System.Collections.Generic;
using TMPro;
using Tweenables;
using UnityEngine;

public class PuzzleEditor : MonoBehaviour
{
    private readonly ReadonlyPersistentValue<int> selectedLevelIndex = new("selectedLevelIndex", PersistenceMode.GlobalRuntime);

    public LevelRegistry levelRegistry;

    public LevelVisuals visuals;
    public UIBuildMenu buildMenu;
    public UIRunMenu runMenu;
    public Main main;
    public TextMeshProUGUI impossibleText;

    private EntityType selectedType;

    private PuzzleData data;
    private HashSet<Vector2Int> usedPositions;

    private Vector2Int? lastPortalPosition;

    public int GetSelectedLevelIndex() => selectedLevelIndex.Get();

    private void Awake()
    {
        if (!selectedLevelIndex.TryGet(out int levelIndex)) return;

        data = levelRegistry.GetPuzzleDataInstance(levelIndex);

        usedPositions = new HashSet<Vector2Int>(data.entities.Keys);
        visuals.SetData(data);

        buildMenu.SetData(data.editableEntities);
        runMenu.SetStepBounds(data.starTresholds);
        impossibleText.gameObject.SetActive(false);
    }

    public Puzzle BuildPuzzle()
    {
        return new Puzzle(data);
    }

    public void ShowImpossible()
    {
        impossibleText.gameObject.SetActive(true);
        var runner = impossibleText.TweenScale().From(Vector3.zero).To(Vector3.one).Duration(0.3f).Ease(Easing.CubicOut).RunNew();
        impossibleText.TweenScale().Delay(2f).From(Vector3.one).To(Vector3.zero).Duration(0.3f).Ease(Easing.CubicIn).OnFinally(() =>
        {
            impossibleText.gameObject.SetActive(false);
        }).RunQueued(ref runner);
    }

    public void SetSelectedType(EntityType type)
    {
        selectedType = type;
    }

    private void Update()
    {
        if (main.CurrentState != MainState.Editing || main.IsPaused || data == null)
        {
            visuals.PreviewEntity(Vector2Int.zero, selectedType, false);
            return;
        }

        var cellPosition = visuals.MouseCellPos();
        if (!data.positions.Contains(cellPosition))
        {
            visuals.PreviewEntity(cellPosition, selectedType, false);
            return;
        }

        if (Input.GetMouseButton(1))
        {
            OnPlace(cellPosition, EntityType.None);
            visuals.PreviewEntity(cellPosition, EntityType.None, true);
            return;
        }

        if (Input.GetMouseButton(0))
        {
            OnPlace(cellPosition, selectedType);
        }

        bool showPreview = !usedPositions.Contains(cellPosition) || selectedType == EntityType.None;
        visuals.PreviewEntity(cellPosition, selectedType, showPreview);
    }

    private void OnPlace(Vector2Int position, EntityType type)
    {
        if (type.basicType == PuzzleEntityType.None)
        {
            RemoveAt(position);
            return;
        }

        if (type.basicType == PuzzleEntityType.Portal)
        {
            if (lastPortalPosition is Vector2Int portalPos && portalPos != position)
            {
                if (TryAddPortalPair(portalPos, position)) lastPortalPosition = null;
                else lastPortalPosition = position;
            }
            else
            {
                lastPortalPosition = position;
            }
            return;
        }
        lastPortalPosition = null;

        PuzzleEntity entity = type.basicType switch
        {
            PuzzleEntityType.Player => PlayerEntity.CreatePlayer(type.playerType, position),
            PuzzleEntityType.Button => new ButtonEntity(type.buttonColor, position),
            PuzzleEntityType.PressurePlate => new PressurePlateEntity(type.buttonColor, position),
            PuzzleEntityType.Portal => throw new System.Exception("Unreachable"),
            _ => new GenericEntity(position, type)
        };

        TryAddEntity(entity);
    }

    private bool TryAddEntity(PuzzleEntity entity)
    {
        var position = entity.position;
        var type = entity.GetEntityType();

        if (usedPositions.Contains(position)) return false;
        if (!buildMenu.CanConsumeEntity(type)) return false;

        if (!data.CanAddEntity(entity)) return false;

        data.AddEntity(entity);
        visuals.AddEntity(entity, true);

        usedPositions.Add(position);
        buildMenu.ConsumeEntity(type);

        return true;
    }

    private bool TryAddPortalPair(Vector2Int pos1, Vector2Int pos2)
    {
        if (usedPositions.Contains(pos1) || usedPositions.Contains(pos2)) return false;
        if (!buildMenu.CanConsumeEntity(EntityType.Portal)) return false;

        var portal1 = new PortalEntity(pos1, pos2);
        var portal2 = new PortalEntity(pos2, pos1);

        if (!data.CanAddEntity(portal1) || !data.CanAddEntity(portal2)) return false;

        data.AddEntity(portal1);
        data.AddEntity(portal2);

        visuals.AddEntity(portal1, true);
        visuals.AddEntity(portal2, false);

        usedPositions.Add(pos1);
        usedPositions.Add(pos2);

        buildMenu.ConsumeEntity(EntityType.Portal);

        return true;
    }

    private void RemoveAt(Vector2Int position)
    {
        var removedEntities = data.Remove(position);
        if (removedEntities.Count == 0) return;

        if (position == lastPortalPosition)
        {
            lastPortalPosition = null;
        }

        visuals.Remove(position);
        usedPositions.Remove(position);

        foreach (var removedEntity in removedEntities)
        {
            buildMenu.ReturnEntity(removedEntity.GetEntityType());

            if (removedEntity is PortalEntity portal && portal.destination is Vector2Int otherPortalPos)
            {
                data.RemoveEntity(otherPortalPos, EntityType.Portal);
                visuals.RemoveEntity(otherPortalPos, EntityType.Portal);
                usedPositions.Remove(otherPortalPos);
            }
        }
    }
}
