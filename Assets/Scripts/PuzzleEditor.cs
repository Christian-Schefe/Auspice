using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yeast.Json;

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

    public int GetSelectedLevelIndex() => selectedLevelIndex.Get();

    private void Awake()
    {
        if (!selectedLevelIndex.TryGet(out int levelIndex))
        {
            return;
        }
        Debug.Log(levelIndex);
        var levelText = levelRegistry.entries[levelIndex].levelData;
        data = JSON.Parse<PuzzleData>(levelText.text);

        usedPositions = new HashSet<Vector2Int>(data.entities.Keys);
        visuals.SetData(data);

        buildMenu.SetData(data.editableEntities);
        runMenu.SetStepBounds(data.starTresholds);
    }

    public void SetSelectedType(EntityType type)
    {
        selectedType = type;
    }

    public void PlaybackSolution(Puzzle puzzle, List<PuzzleState> solution)
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
        if (!data.positions.Contains(cellPosition)) return;

        if (Input.GetMouseButton(0))
        {
            OnPlace(cellPosition);
        }
        else if (Input.GetMouseButton(1))
        {
            var oldType = selectedType;
            selectedType = new(PuzzleEntityType.None);
            OnPlace(cellPosition);
            selectedType = oldType;
        }
    }

    private void OnPlace(Vector2Int position)
    {
        if (selectedType.basicType == PuzzleEntityType.None)
        {
            var removedEntities = data.Remove(position);
            if (removedEntities.Count == 0) return;

            visuals.Remove(position);
            usedPositions.Remove(position);

            foreach (var removedEntity in removedEntities)
            {
                buildMenu.ReturnEntity(removedEntity);
            }
            return;
        }

        if (usedPositions.Contains(position)) return;
        if (!buildMenu.CanConsumeEntity(selectedType)) return;

        PuzzleEntity entity = selectedType.basicType switch
        {
            PuzzleEntityType.Player => PlayerEntity.CreatePlayer(selectedType.playerType, position),
            PuzzleEntityType.Button => new ButtonEntity(selectedType.buttonColor, position),
            _ => new GenericEntity(position, selectedType)
        };

        if (!data.TryAddEntity(entity)) return;
        visuals.AddEntity(entity, true);

        usedPositions.Add(position);
        buildMenu.ConsumeEntity(selectedType);
    }
}
