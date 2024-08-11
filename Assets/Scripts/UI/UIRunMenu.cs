using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIRunMenu : MonoBehaviour
{
    public Main main;
    public Button submitButton;
    public TextMeshProUGUI stepsText;

    public UIStarDisplay starDisplay;
    private List<int> stepThresholds;

    private void Awake()
    {
        submitButton.onClick.AddListener(main.Solve);
    }

    public void SetStepBounds(List<int> thresholds)
    {
        stepThresholds = thresholds;
        SetSteps(0);
    }

    public void SetSteps(int steps)
    {
        stepsText.text = steps.ToString();
        int stars = 0;
        for (int i = 0; i < stepThresholds.Count; i++)
        {
            if (steps >= stepThresholds[i])
            {
                stars = i + 1;
            }
        }
        starDisplay.SetStars(stars);
    }
}
