using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yeast.Json;

public class PuzzleEditor : MonoBehaviour
{
    public enum Mode
    {
        Erase, Wall, Button, Chest, Player, OnSpike, OffSpike
    }

    public TextAsset levelData;


    public Mode mode;

    public LevelVisuals visuals;
    public UIBuildMenu buildMenu;

    public bool isPlaying;

    private PuzzleData data;
    private HashSet<Vector2Int> usedPositions;

    private void Awake()
    {
        data = JSON.Parse<PuzzleData>(levelData.text);

        usedPositions = new HashSet<Vector2Int>(data.entities.Keys);
        visuals.SetData(data);

        buildMenu.SetData(data.editableEntities, OnSelectErase, OnSelectEntity);
    }

    private void OnSelectEntity(PuzzleEntityType type)
    {
        mode = type switch
        {
            PuzzleEntityType.Wall => Mode.Wall,
            PuzzleEntityType.OnSpike => Mode.OnSpike,
            PuzzleEntityType.OffSpike => Mode.OffSpike,
            PuzzleEntityType.Chest => Mode.Chest,
            PuzzleEntityType.Button => Mode.Button,
            PuzzleEntityType.Player => Mode.Player,
            _ => mode
        };
    }

    private void OnSelectErase()
    {
        mode = Mode.Erase;
    }

    public Puzzle BuildPuzzle()
    {
        return new Puzzle(data);
    }

    private void Update()
    {
        if (isPlaying)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            mode = Mode.Erase;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            mode = Mode.Wall;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            mode = Mode.Button;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            mode = Mode.Chest;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            mode = Mode.Player;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha6))
        {
            mode = Mode.OnSpike;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha7))
        {
            mode = Mode.OffSpike;
        }

        if (Input.GetMouseButton(0))
        {
            var position = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            position.z = 0;
            var cellPosition = visuals.CellPos(position);
            if (!data.positions.Contains(cellPosition))
            {
                return;
            }
            OnPlace(cellPosition);
        }
    }

    private void OnPlace(Vector2Int position)
    {
        if (mode == Mode.Erase)
        {
            visuals.Remove(position);
            var removedEntities = data.Remove(position);
            if (removedEntities.Count > 0) usedPositions.Remove(position);

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

        Debug.Log($"Placing {mode} at {position}");
        if (mode == Mode.Wall)
        {
            if (!buildMenu.TryConsumeEntity(PuzzleEntityType.Wall)) return;
            if (!data.TryAddEntity(new GenericEntity(position, PuzzleEntityType.Wall))) return;
            visuals.AddWall(position);
        }
        else if (mode == Mode.OnSpike)
        {
            if (!buildMenu.TryConsumeEntity(PuzzleEntityType.OnSpike)) return;
            if (!data.TryAddEntity(new GenericEntity(position, PuzzleEntityType.OnSpike))) return;
            visuals.AddOnSpikes(position);
        }
        else if (mode == Mode.OffSpike)
        {
            if (!buildMenu.TryConsumeEntity(PuzzleEntityType.OffSpike)) return;
            if (!data.TryAddEntity(new GenericEntity(position, PuzzleEntityType.OffSpike))) return;
            visuals.AddOffSpikes(position);
        }
        else if (mode == Mode.Chest)
        {
            if (!buildMenu.TryConsumeEntity(PuzzleEntityType.Chest)) return;
            if (!data.TryAddEntity(new GenericEntity(position, PuzzleEntityType.Chest))) return;
            visuals.AddChest(position);
        }
        else if (mode == Mode.Button)
        {
            if (!buildMenu.TryConsumeEntity(PuzzleEntityType.Button)) return;
            var button = new ButtonEntity(position);
            if (!data.TryAddEntity(button)) return;
            visuals.AddButton(button);
        }
        else if (mode == Mode.Player)
        {
            if (!buildMenu.TryConsumeEntity(PuzzleEntityType.Player)) return;
            var player = new CrabPlayer(position);
            if (!data.TryAddEntity(player)) return;
            visuals.AddPlayer(player);
        }

        usedPositions.Add(position);
    }
}
