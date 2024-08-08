using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yeast.Json;

public class PuzzleEditor : MonoBehaviour
{
    public TextAsset levelData;

    public LevelVisuals visuals;
    public UIBuildMenu buildMenu;
    public UIRunMenu runMenu;
    public PuzzleReplayer replayer;

    private EntityType selectedType;

    private bool isReplaying;

    private PuzzleData data;
    private HashSet<Vector2Int> usedPositions;

    private void Awake()
    {
        data = JSON.Parse<PuzzleData>(levelData.text);

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
        if (isReplaying)
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

            foreach (var entity in removedEntities)
            {
                buildMenu.ReturnEntity(entity);
            }
            return;
        }

        if (usedPositions.Contains(position))
        {
            return;
        }

        if (!buildMenu.CanConsumeEntity(selectedType)) return;

        if (selectedType.basicType == PuzzleEntityType.Wall)
        {
            var wall = new GenericEntity(position, selectedType);
            if (!data.TryAddEntity(wall)) return;
            visuals.AddWall(wall);
        }
        else if (selectedType.basicType == PuzzleEntityType.Spike)
        {
            var spike = new GenericEntity(position, selectedType);
            if (!data.TryAddEntity(spike)) return;
            visuals.AddSpikes(spike);
        }
        else if (selectedType.basicType == PuzzleEntityType.Chest)
        {
            var chest = new GenericEntity(position, selectedType);
            if (!data.TryAddEntity(chest)) return;
            visuals.AddChest(chest);
        }
        else if (selectedType.basicType == PuzzleEntityType.Button)
        {
            var button = new ButtonEntity(selectedType.buttonColor, position);
            if (!data.TryAddEntity(button)) return;
            visuals.AddButton(button);
        }
        else if (selectedType.basicType == PuzzleEntityType.Player)
        {
            var player = new CrabPlayer(position);
            if (!data.TryAddEntity(player)) return;
            visuals.AddPlayer(player);
        }

        Debug.Log($"Placed {selectedType} at {position}");
        usedPositions.Add(position);
        buildMenu.ConsumeEntity(selectedType);
    }
}
