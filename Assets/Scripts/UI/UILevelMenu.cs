using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yeast;

public class UILevelMenu : MonoBehaviour
{
    private readonly PersistentValue<Dictionary<int, SolutionData>> solutions = new("solutions", PersistenceMode.GlobalPersistence, new());
    private readonly PersistentValue<int> selectedLevel = new("selectedLevelIndex", PersistenceMode.GlobalRuntime, 0);

    public UIModalWindow window;

    public RectTransform levelButtonContainer;

    public UILevelButton levelButtonPrefab;

    private List<UILevelButton> levelButtons;

    public LevelRegistry levelRegistry;

    public SceneSO levelScene;


    private void Start()
    {
        levelButtons = new List<UILevelButton>();
        var solutionDict = solutions.Get();

        for (int i = 0; i < levelRegistry.GetLevelCount(); i++)
        {
            var levelIndex = i;
            var levelButton = Instantiate(levelButtonPrefab, levelButtonContainer);
            levelButton.SetText($"{levelIndex + 1}");
            var puzzleData = levelRegistry.GetPuzzleData(levelIndex);

            levelButton.AddClickListener(() =>
            {
                selectedLevel.Set(levelIndex);
                SceneSystem.LoadScene(levelScene);
            });
            levelButtons.Add(levelButton);

            if (solutionDict.TryGetValue(i, out var solution))
            {
                var steps = solution.steps.Count - 1;
                var stars = GetStars(puzzleData.starTresholds, steps);
                levelButton.starDisplay.SetStars(stars);
            }
            else
            {
                levelButton.starDisplay.SetStars(0);
            }
        }
    }

    private static int GetStars(List<int> stepThresholds, int steps)
    {
        int stars = 0;
        for (int i = 0; i < stepThresholds.Count; i++)
        {
            if (steps >= stepThresholds[i])
            {
                stars = i + 1;
            }
        }
        return stars;
    }
}
