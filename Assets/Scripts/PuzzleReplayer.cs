using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuzzleReplayer : MonoBehaviour
{
    public LevelVisuals levelVisuals;

    public System.Action<int> stepCallback;
    public System.Action endCallback;

    public void ReplayPuzzle(Puzzle puzzle, List<PuzzleState> solution)
    {
        StopAllCoroutines();
        StartCoroutine(ReplayPuzzleCoroutine(puzzle, solution));
    }

    private IEnumerator ReplayPuzzleCoroutine(Puzzle puzzle, List<PuzzleState> actions)
    {
        var players = puzzle.GetEntities<PlayerEntity>(PuzzleEntityType.Player);
        var buttons = puzzle.GetEntities<ButtonEntity>(PuzzleEntityType.Button);
        var spikes = puzzle.GetEntities<GenericEntity>(PuzzleEntityType.Spike);

        for (int i = 0; i < actions.Count; i++)
        {
            var state = actions[i];
            SetState(state);
            stepCallback?.Invoke(i);
            yield return new WaitForSeconds(0.2f);
        }

        yield return new WaitForSeconds(0.8f);
        SetState(actions[0]);
        yield return new WaitForSeconds(0.2f);
        endCallback?.Invoke();


        void SetState(PuzzleState state)
        {
            puzzle.SetState(state);
            foreach (var player in players)
            {
                levelVisuals.UpdateEntityPosition(player);
            }
            foreach (var button in buttons)
            {
                levelVisuals.UpdateButtonState(button);
            }
            foreach (var spike in spikes)
            {
                var buttonState = puzzle.GetButtonToggleState(spike.GetEntityType().buttonColor);
                levelVisuals.UpdateSpikeState(spike, buttonState);
            }
            SFX.Play(SFX.Type.Move);
        }
    }
}
