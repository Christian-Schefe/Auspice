using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yeast;

public class UILevelMenu : MonoBehaviour
{
    private readonly PersistentValue<Dictionary<string, SolutionData>> solutions = new("solutions", PersistenceMode.GlobalPersistence);

    public UIModalWindow window;

    public RectTransform levelButtonContainer;

    public UILevelButton levelButtonPrefab;

    private List<UILevelButton> levelButtons;

    public LevelRegistry levelRegistry;

    public SceneSO levelScene;
    public UIMainMenu mainMenu;

    private void Start()
    {
        Init();
        solutions.AddListener(OnSolutionsChanged);
        solutions.MarkDirty();
        SceneSystem.AddOnBeforeSceneUnload(RemoveListener);
    }

    private void RemoveListener()
    {
        solutions.RemoveListener(OnSolutionsChanged);
    }

    private void OnSolutionsChanged(bool isPresent, Dictionary<string, SolutionData> newSolutions)
    {
        if (!isPresent) newSolutions = new Dictionary<string, SolutionData>();
        Debug.Log(newSolutions.Count + " Solutions found.");

        var solvedLevels = new HashSet<int>();

        for (int i = 0; i < levelButtons.Count; i++)
        {
            var levelButton = levelButtons[i];
            var puzzleData = levelRegistry.GetOriginalPuzzleData(i);
            var hash = levelRegistry.GetPuzzleHash(i);

            if (newSolutions.TryGetValue(hash, out var solution))
            {
                var steps = solution.StepCount;
                var stars = GetStars(puzzleData.starTresholds, steps);
                levelButton.starDisplay.SetStars(stars);
                if (stars > 0) solvedLevels.Add(i);
            }
            else
            {
                levelButton.starDisplay.SetStars(0);
            }
        }

        for (int i = 0, n = levelButtons.Count; i < n; i++)
        {
            var levelButton = levelButtons[i];
            if (i <= 1 || solvedLevels.Contains(i) || solvedLevels.Contains(i - 1) || solvedLevels.Contains(i - 2))
            {
                levelButton.SetUnlocked(true);
            }
            else
            {
                levelButton.SetUnlocked(false);
            }
        }
    }

    private void Init()
    {
        levelButtons = new List<UILevelButton>();

        for (int i = 0; i < levelRegistry.GetLevelCount(); i++)
        {
            var levelIndex = i;
            var levelButton = Instantiate(levelButtonPrefab, levelButtonContainer);
            levelButton.SetText($"{levelIndex + 1}");

            levelButton.AddClickListener(() =>
            {
                mainMenu.selectedLevel.Set(levelIndex);
                SceneSystem.LoadScene(levelScene);
            });
            levelButtons.Add(levelButton);
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
