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

        data.editableEntities = new()
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
            {BuildEntityType.Conveyor, null},
            {BuildEntityType.RedPressurePlate, null},
            {BuildEntityType.BluePressurePlate, null},
            {BuildEntityType.Portal, null},
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
