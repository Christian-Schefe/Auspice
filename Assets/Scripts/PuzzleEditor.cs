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

    public Level level;

    private void Update()
    {
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
            var cellPosition = level.CellPos(position);
            OnPlace(cellPosition);
        }
    }

    private void OnPlace(Vector2Int position)
    {
        if (mode == Mode.Erase)
        {
            level.Erase(position);
        }
        else if (mode == Mode.Wall)
        {
            level.SetWall(position);
        }
        else if (mode == Mode.OnSpike)
        {
            level.SetSpikes(true, position);
        }
        else if (mode == Mode.OffSpike)
        {
            level.SetSpikes(false, position);
        }
        else if (mode == Mode.Button)
        {
            level.SetButton(position);
        }
        else if (mode == Mode.Chest)
        {
            level.SetChest(position);
        }
        else if (mode == Mode.Player)
        {
            level.SetPlayer(position);
        }
    }
}
