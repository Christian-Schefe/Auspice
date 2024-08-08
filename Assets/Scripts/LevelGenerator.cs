using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yeast.Json;

[CreateAssetMenu(fileName = "LevelGenerator", menuName = "LevelGenerator")]
public class LevelGenerator : ScriptableObject
{
    public Vector2Int size;
    public string levelName;

    [ContextMenu("Generate")]
    public void Generate()
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
                data.TryAddEntity(new GenericEntity(position, PuzzleEntityType.Wall), isEditable: false);
            }
        }

        data.editableEntities = new Dictionary<PuzzleEntityType, int?>
        {
            { PuzzleEntityType.Wall, null },
            { PuzzleEntityType.OnSpike, null },
            { PuzzleEntityType.OffSpike, null },
            { PuzzleEntityType.Chest, null },
            { PuzzleEntityType.Button, null },
            { PuzzleEntityType.Player, null }
        };

        var json = JSON.Stringify(data);
        WriteTextAsset(json);

        //test roundtrip
        var parsedData = JSON.Parse<PuzzleData>(json);
        var roundtripJson = JSON.Stringify(parsedData);
        Debug.Log(json == roundtripJson);
    }

    private void WriteTextAsset(string json)
    {
        var path = $"Assets/Resources/Levels/{levelName}.json";
        System.IO.File.WriteAllText(path, json);
        UnityEditor.AssetDatabase.Refresh();
    }
}
