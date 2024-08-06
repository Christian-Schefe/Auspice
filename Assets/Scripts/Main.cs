using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Main : MonoBehaviour
{
    public Tilemap groundTilemap;

    public Puzzle puzzle;

    public TileBase wallTile;
    public TileBase waterTile;
    public TileBase onSpikeTile;
    public TileBase offSpikeTile;

    public List<GameObject> players, chests, buttons;
    public PuzzleObserver puzzleObserver;

    private void Start()
    {
        var positions = new List<Vector2Int>();
        var groundTiles = GetTiles(groundTilemap);
        positions.AddRange(groundTiles.Keys);

        puzzle = new Puzzle(positions);

        foreach (var tile in groundTiles)
        {
            if (wallTile == tile.Value)
            {
                puzzle.AddObject(tile.Key, Puzzle.PuzzleObject.Wall);
            }
            else if (waterTile == tile.Value)
            {
                puzzle.AddObject(tile.Key, Puzzle.PuzzleObject.Water);
            }
            else if (onSpikeTile == tile.Value)
            {
                puzzle.AddObject(tile.Key, Puzzle.PuzzleObject.OnSpike);
            }
            else if (offSpikeTile == tile.Value)
            {
                puzzle.AddObject(tile.Key, Puzzle.PuzzleObject.OffSpike);
            }
        }

        foreach (var player in players)
        {
            var playerPos = (Vector2Int)groundTilemap.WorldToCell(player.transform.position);
            puzzle.AddEntity(new CrabPlayer(playerPos));
        }

        foreach (var chest in chests)
        {
            var chestPos = (Vector2Int)groundTilemap.WorldToCell(chest.transform.position);
            puzzle.AddObject(chestPos, Puzzle.PuzzleObject.Chest);
        }

        foreach (var button in buttons)
        {
            var buttonPos = (Vector2Int)groundTilemap.WorldToCell(button.transform.position);
            puzzle.AddEntity(new ButtonEntity(buttonPos));
        }

        var solver = new PuzzleSolver();
        var solution = solver.Solve(puzzle);
        Debug.Log(solution);

        puzzleObserver.ReplayPuzzle(puzzle, solution);
    }

    private Dictionary<Vector2Int, TileBase> GetTiles(Tilemap tilemap)
    {
        BoundsInt bounds = tilemap.cellBounds;
        TileBase[] allTiles = tilemap.GetTilesBlock(bounds);
        var pointer = (Vector2Int)bounds.min;
        Dictionary<Vector2Int, TileBase> tiles = new();

        for (int x = 0; x < bounds.size.x; x++)
        {
            for (int y = 0; y < bounds.size.y; y++)
            {
                TileBase tile = allTiles[x + y * bounds.size.x];
                if (tile != null)
                {
                    tiles.Add(pointer + new Vector2Int(x, y), tile);
                }
            }
        }
        return tiles;
    }
}
