using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuzzleEditor : MonoBehaviour
{
    public enum Mode
    {
        Erase, Wall, Button, Chest, Player, OnSpike, OffSpike
    }


    public Mode mode;

    public LevelVisuals visuals;

    public bool isPlaying;

    private PuzzleData data;
    private HashSet<Vector2Int> usedPositions;

    private void Awake()
    {
        var positions = new HashSet<Vector2Int>();

        var size = new Vector2Int(10, 8);

        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                positions.Add(new Vector2Int(x, y));
            }
        }

        data = new PuzzleData(positions);
        usedPositions = new HashSet<Vector2Int>();
        visuals.SetPositions(positions);
        for (int x = -1; x <= size.x; x++)
        {
            for (int y = -1; y <= size.y; y++)
            {
                var pos = new Vector2Int(x, y);
                if (pos.x == -1 || pos.y == -1 || pos.x == size.x || pos.y == size.y)
                {
                    visuals.AddWall(pos);
                }
            }
        }
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
            data.Remove(position);
            usedPositions.Remove(position);
            return;
        }

        if (usedPositions.Contains(position))
        {
            return;
        }
        usedPositions.Add(position);

        if (mode == Mode.Wall)
        {
            visuals.AddWall(position);
            data.AddObject(position, PuzzleObject.Wall);
        }
        else if (mode == Mode.OnSpike)
        {
            visuals.AddOnSpikes(position);
            data.AddObject(position, PuzzleObject.OnSpike);
        }
        else if (mode == Mode.OffSpike)
        {
            visuals.AddOffSpikes(position);
            data.AddObject(position, PuzzleObject.OffSpike);
        }
        else if (mode == Mode.Chest)
        {
            visuals.AddChest(position);
            data.AddObject(position, PuzzleObject.Chest);
        }
        else if (mode == Mode.Button)
        {
            var button = new ButtonEntity(position);
            visuals.AddButton(button);
            data.AddEntity(button);
        }
        else if (mode == Mode.Player)
        {
            var player = new CrabPlayer(position);
            visuals.AddPlayer(player);
            data.AddEntity(player);
        }
    }
}
