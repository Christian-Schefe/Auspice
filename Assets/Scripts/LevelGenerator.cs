using System.Collections.Generic;
using UnityEngine;

public static class LevelGenerator
{
    public static PuzzleData GenerateData(Vector2Int size)
    {
        var positions = new HashSet<Vector2Int>();

        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                positions.Add(new Vector2Int(x, y));
            }
        }

        var data = new PuzzleData();
        data.SetPositions(positions);

        foreach (var position in positions)
        {
            if (position.x == 0 || position.x == size.x - 1 || position.y == 0 || position.y == size.y - 1)
            {
                data.AddEntity(new GenericEntity(position, new EntityType(PuzzleEntityType.Wall)), isEditable: false);
            }
        }

        data.SetBuildableEntityCounts(new()
        {
            {BuildEntityType.Wall, null},
            {BuildEntityType.Chest, null},
            {BuildEntityType.RedButton, null},
            {BuildEntityType.BlueButton, null},
            {BuildEntityType.RedSpike, null},
            {BuildEntityType.BlueSpike, null},
            {BuildEntityType.Ice, null},
            {BuildEntityType.Crab, null},
            {BuildEntityType.Octopus, null},
            {BuildEntityType.Fish, null},
            {BuildEntityType.Starfish, null},
            {BuildEntityType.Penguin, null},
            {BuildEntityType.Conveyor, null},
            {BuildEntityType.RedPressurePlate, null},
            {BuildEntityType.BluePressurePlate, null},
            {BuildEntityType.Portal, null},
        });

        data.SetStarThresholds(new());

        return data;
    }
}
