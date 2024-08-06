using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Puzzle
{
    public enum PuzzleObject
    {
        Wall,
        Water,
        Chest,
        OffSpike,
        OnSpike
    }

    public List<Vector2Int> positions;
    public Dictionary<Vector2Int, HashSet<PuzzleObject>> puzzleObjects;
    public Dictionary<Vector2Int, Dictionary<Type, PuzzleEntity>> puzzleEntities;

    public List<Player> players;
    public List<ButtonEntity> buttons;

    public bool GetButtonToggleState()
    {
        int pressedButtonCount = 0;
        for (int i = 0; i < buttons.Count; i++)
        {
            if (buttons[i].isPressed) pressedButtonCount++;
        }
        return pressedButtonCount % 2 == 1;
    }

    public Puzzle(List<Vector2Int> positions)
    {
        this.positions = positions;

        puzzleObjects = new();
        puzzleEntities = new();
        foreach (var position in positions)
        {
            puzzleObjects[position] = new();
            puzzleEntities[position] = new();
        }

        players = new();
        buttons = new();
    }

    public void AddObject(Vector2Int position, PuzzleObject puzzleObject)
    {
        puzzleObjects[position].Add(puzzleObject);
    }

    public void RemoveObject(Vector2Int position, PuzzleObject puzzleObject)
    {
        puzzleObjects[position].Remove(puzzleObject);
    }
    public void RemoveObjects(Vector2Int position)
    {
        puzzleObjects[position].Clear();
    }

    public void AddEntity<T>(T entity) where T : PuzzleEntity
    {
        puzzleEntities[entity.position].Add(typeof(T), entity);
        if (entity is Player player)
        {
            players.Add(player);
        }
        else if (entity is ButtonEntity button)
        {
            buttons.Add(button);
        }
    }

    public void RemoveEntity<T>(Vector2Int position) where T : PuzzleEntity
    {
        if (!puzzleEntities[position].Remove(typeof(T), out var entity)) return;
        if (entity is Player player)
        {
            players.Remove(player);
        }
        else if (entity is ButtonEntity button)
        {
            buttons.Remove(button);
        }
    }

    public void RemoveEntities(Vector2Int position)
    {
        foreach (var entity in puzzleEntities[position].Values)
        {
            if (entity is Player player)
            {
                players.Remove(player);
            }
            else if (entity is ButtonEntity button)
            {
                buttons.Remove(button);
            }
        }
        puzzleEntities[position].Clear();
    }

    public PuzzleState GetState()
    {
        var playerPosition = new Vector2Int[players.Count];
        for (int i = 0; i < players.Count; i++)
        {
            playerPosition[i] = players[i].position;
        }

        var buttonStates = new bool[buttons.Count];
        for (int i = 0; i < buttons.Count; i++)
        {
            buttonStates[i] = buttons[i].isPressed;
        }

        return new PuzzleState(playerPosition, buttonStates);
    }

    public bool IsWon()
    {
        foreach (var player in players)
        {
            if (!HasObject(player.position, PuzzleObject.Chest))
            {
                return false;
            }
        }
        return true;
    }

    public void SetState(PuzzleState state)
    {
        for (int i = 0; i < players.Count; i++)
        {
            players[i].position = state.playerPosition[i];
        }

        int pressedButtonCount = 0;
        for (int i = 0; i < buttons.Count; i++)
        {
            buttons[i].isPressed = state.buttonStates[i];
            if (buttons[i].isPressed) pressedButtonCount++;
        }
    }

    public bool HasObject(Vector2Int position, PuzzleObject puzzleObject)
    {
        return puzzleObjects.TryGetValue(position, out var set) && set.Contains(puzzleObject);
    }

    public bool HasEntity<T>(Vector2Int position, out T entity) where T : PuzzleEntity
    {
        if (puzzleEntities.TryGetValue(position, out var set) && set.TryGetValue(typeof(T), out var puzzleEntity))
        {
            entity = (T)puzzleEntity;
            return true;
        }
        entity = null;
        return false;
    }

    public void DebugPrint()
    {
        foreach (var position in positions)
        {
            if (HasObject(position, PuzzleObject.Wall))
            {
                Debug.Log($"Wall: {position}");
            }
            if (HasObject(position, PuzzleObject.Water))
            {
                Debug.Log($"Water: {position}");
            }
            if (HasObject(position, PuzzleObject.Chest))
            {
                Debug.Log($"Chest: {position}");
            }
        }
    }

    public struct PuzzleState
    {
        public Vector2Int[] playerPosition;
        public bool[] buttonStates;

        public override readonly string ToString()
        {
            var sb = new System.Text.StringBuilder();
            sb.Append("Puzzle(");
            for (int i = 0; i < playerPosition.Length; i++)
            {
                sb.Append($"Player {i + 1}: {playerPosition[i]}");
                if (i < playerPosition.Length - 1)
                {
                    sb.Append(", ");
                }
            }
            for (int i = 0; i < buttonStates.Length; i++)
            {
                sb.Append($"Button {i + 1}: {buttonStates[i]}");
                if (i < buttonStates.Length - 1)
                {
                    sb.Append(", ");
                }
            }
            sb.Append(")");
            return sb.ToString();
        }

        public PuzzleState(Vector2Int[] playerPosition, bool[] buttonStates)
        {
            this.playerPosition = playerPosition;
            this.buttonStates = buttonStates;
        }

        public override readonly bool Equals(object obj)
        {
            if (obj is not PuzzleState other)
            {
                return false;
            }
            if (playerPosition.Length != other.playerPosition.Length || buttonStates.Length != other.buttonStates.Length)
            {
                return false;
            }
            for (int i = 0; i < playerPosition.Length; i++)
            {
                if (playerPosition[i] != other.playerPosition[i])
                {
                    return false;
                }
            }
            for (int i = 0; i < buttonStates.Length; i++)
            {
                if (buttonStates[i] != other.buttonStates[i])
                {
                    return false;
                }
            }
            return true;
        }

        public override readonly int GetHashCode()
        {
            var hash = new HashCode();
            foreach (var position in playerPosition)
            {
                hash.Add(position);
            }
            foreach (var buttonState in buttonStates)
            {
                hash.Add(buttonState);
            }
            return hash.ToHashCode();
        }
    }
}