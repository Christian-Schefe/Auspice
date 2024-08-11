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

            foreach (var removedEntity in removedEntities)
            {
                buildMenu.ReturnEntity(removedEntity);
            }
            return;
        }

        if (usedPositions.Contains(position))
        {
            return;
        }

        if (!buildMenu.CanConsumeEntity(selectedType)) return;

        PuzzleEntity entity;
        if (selectedType.basicType == PuzzleEntityType.Player)
        {
            entity = selectedType.playerType switch
            {
                PlayerType.Crab => new CrabPlayer(position),
                PlayerType.Octopus => new OctopusPlayer(position),
                PlayerType.Fish => new FishPlayer(position),
                _ => throw new System.NotImplementedException(),
            };
        }
        else if (selectedType.basicType == PuzzleEntityType.Button)
        {
            entity = new ButtonEntity(selectedType.buttonColor, position);
        }
        else
        {
            entity = new GenericEntity(position, selectedType);
        }

        if (!data.TryAddEntity(entity)) return;
        visuals.AddEntity(entity, true);

        Debug.Log($"Placed {selectedType} at {position}");
        usedPositions.Add(position);
        buildMenu.ConsumeEntity(selectedType);
    }
}
