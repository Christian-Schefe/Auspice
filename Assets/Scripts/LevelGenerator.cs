using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yeast;

[CreateAssetMenu(fileName = "LevelGenerator", menuName = "LevelGenerator")]
public class LevelGenerator : ScriptableObject
{
    public Vector2Int size;
    public string levelName;
    public List<int> starThresholds;

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
                data.TryAddEntity(new GenericEntity(position, new EntityType(PuzzleEntityType.Wall)), isEditable: false);
            }
        }

        data.editableEntities = new Dictionary<EntityType, int?>
        {
            { new EntityType(PuzzleEntityType.Wall), null },
            { new EntityType(PuzzleEntityType.Spike, buttonColor: ButtonColor.Red, spikeInitialState:true), null },
            { new EntityType(PuzzleEntityType.Spike, buttonColor: ButtonColor.Red, spikeInitialState:false), 5 },
            { new EntityType(PuzzleEntityType.Spike, buttonColor: ButtonColor.Blue, spikeInitialState:true), null },
            { new EntityType(PuzzleEntityType.Spike, buttonColor: ButtonColor.Blue, spikeInitialState:false), 5 },
            { new EntityType(PuzzleEntityType.Chest), 2 },
            { new EntityType(PuzzleEntityType.Conveyor, direction: Direction.Up), null },
            { new EntityType(PuzzleEntityType.Conveyor, direction: Direction.Down), null },
            { new EntityType(PuzzleEntityType.Conveyor, direction: Direction.Left), null },
            { new EntityType(PuzzleEntityType.Conveyor, direction: Direction.Right), null },
            { new EntityType(PuzzleEntityType.Button, buttonColor: ButtonColor.Red), null },
            { new EntityType(PuzzleEntityType.Button, buttonColor: ButtonColor.Blue), null },
            { new EntityType(PuzzleEntityType.Player, playerType: PlayerType.Crab), 3 },
            { new EntityType(PuzzleEntityType.Player, playerType: PlayerType.Octopus), 32 },
            { new EntityType(PuzzleEntityType.Player, playerType: PlayerType.Fish), 3 }
        };

        data.starTresholds = starThresholds;

        var json = data.ToJson();
        WriteTextAsset(json);

        //test roundtrip
        var parsedData = json.FromJson<PuzzleData>();
        var roundtripJson = parsedData.ToJson();
        Debug.Log(json == roundtripJson);
    }

    private void WriteTextAsset(string json)
    {
#if UNITY_EDITOR
        var path = $"Assets/Resources/Levels/{levelName}.json";
        System.IO.File.WriteAllText(path, json);
        UnityEditor.AssetDatabase.Refresh();
#endif
    }
}
