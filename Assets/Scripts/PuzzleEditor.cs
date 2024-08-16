using System.Collections.Generic;
using UnityEngine;

public class PuzzleEditor : MonoBehaviour
{
    private readonly ReadonlyPersistentValue<int> selectedLevelIndex = new("selectedLevelIndex", PersistenceMode.GlobalRuntime);

    public LevelRegistry levelRegistry;

    public LevelVisuals visuals;
    public UIBuildMenu buildMenu;
    public UIRunMenu runMenu;
    public PuzzleReplayer replayer;

    private EntityType selectedType;

    private bool isReplaying;
    public bool isPaused;

    private PuzzleData data;
    private HashSet<Vector2Int> usedPositions;

    private Vector2Int? lastPortalPosition;

    public int GetSelectedLevelIndex() => selectedLevelIndex.Get();

    private void Awake()
    {
        if (!selectedLevelIndex.TryGet(out int levelIndex))
        {
            return;
        }

        data = levelRegistry.GetPuzzleDataInstance(levelIndex);

        usedPositions = new HashSet<Vector2Int>(data.entities.Keys);
        visuals.SetData(data);

        buildMenu.SetData(data.editableEntities);
        runMenu.SetStepBounds(data.starTresholds);
    }

    public void SetSelectedType(EntityType type)
    {
        selectedType = type;
    }

    public void PlaybackSolution(Puzzle puzzle, SolutionData solution)
    {
        isReplaying = true;
        replayer.endCallback = () =>
        {
            isReplaying = false;
        };
        replayer.stepCallback = (step) =>
        {
            runMenu.SetSteps(step);
        };
        replayer.ReplayPuzzle(puzzle, solution);
    }

    public bool IsReplaying() => isReplaying;

    public Puzzle BuildPuzzle()
    {
        return new Puzzle(data);
    }

    private void Update()
    {
        if (isReplaying || isPaused || data == null)
        {
            return;
        }

        var cellPosition = visuals.MouseCellPos();
        if (!data.positions.Contains(cellPosition))
        {
            visuals.PreviewEntity(new GenericEntity(cellPosition, selectedType), false);
            return;
        }

        if (Input.GetMouseButton(1))
        {
            var oldType = selectedType;
            selectedType = new(PuzzleEntityType.None);
            OnPlace(cellPosition);
            visuals.PreviewEntity(new GenericEntity(cellPosition, selectedType), true);
            selectedType = oldType;
            return;
        }
        else if (Input.GetMouseButton(0))
        {
            OnPlace(cellPosition);
        }

        if (usedPositions.Contains(cellPosition) && selectedType != EntityType.None)
        {
            visuals.PreviewEntity(new GenericEntity(cellPosition, selectedType), false);
        }
        else
        {
            visuals.PreviewEntity(new GenericEntity(cellPosition, selectedType), true);
        }
    }

    private void OnPlace(Vector2Int position)
    {
        if (selectedType.basicType == PuzzleEntityType.None)
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
            return;
        }

        if (usedPositions.Contains(position)) return;
        if (!buildMenu.CanConsumeEntity(selectedType)) return;

        PuzzleEntity entity = selectedType.basicType switch
        {
            PuzzleEntityType.Player => PlayerEntity.CreatePlayer(selectedType.playerType, position),
            PuzzleEntityType.Button => new ButtonEntity(selectedType.buttonColor, position),
            PuzzleEntityType.Portal => new PortalEntity(position, lastPortalPosition ?? Vector2Int.zero),
            _ => new GenericEntity(position, selectedType)
        };

        if (!data.TryAddEntity(entity)) return;
        visuals.AddEntity(entity, true);

        usedPositions.Add(position);
        buildMenu.ConsumeEntity(selectedType);

        if (selectedType.basicType == PuzzleEntityType.Portal)
        {
            if (lastPortalPosition is Vector2Int lastPortalPos)
            {
                data.TryGetEditableEntity(lastPortalPos, EntityType.Portal, out PortalEntity otherPortal);
                otherPortal.destination = position;
                lastPortalPosition = null;
            }
            else
            {
                lastPortalPosition = position;
            }
        }
        else if (lastPortalPosition is Vector2Int lastPortalPos)
        {
            data.RemoveEntity(lastPortalPos, EntityType.Portal);
            visuals.RemoveEntity(lastPortalPos, EntityType.Portal);
            usedPositions.Remove(lastPortalPos);
            lastPortalPosition = null;
        }
    }
}
