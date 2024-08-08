using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuzzleReplayer : MonoBehaviour
{
    public LevelVisuals levelVisuals;

    public void ReplayPuzzle(Puzzle puzzle, List<PuzzleState> solution, System.Action callback)
    {
        StopAllCoroutines();
        StartCoroutine(ReplayPuzzleCoroutine(puzzle, solution, callback));
    }

    private IEnumerator ReplayPuzzleCoroutine(Puzzle puzzle, List<PuzzleState> actions, System.Action callback)
    {
        Debug.Log("Start Replay: " + actions.Count + " Items");

        var players = puzzle.GetEntities<PlayerEntity>(PuzzleEntityType.Player);
        var buttons = puzzle.GetEntities<ButtonEntity>(PuzzleEntityType.Button);
        var offSpikes = puzzle.GetEntities<GenericEntity>(PuzzleEntityType.OffSpike);
        var onSpikes = puzzle.GetEntities<GenericEntity>(PuzzleEntityType.OnSpike);

        foreach (var state in actions)
        {
            Debug.Log("Replaying: " + state);
            SetState(state);
            yield return new WaitForSeconds(0.2f);
        }

        yield return new WaitForSeconds(1f);
        SetState(actions[0]);
        callback?.Invoke();


        void SetState(PuzzleState state)
        {
            puzzle.SetState(state);
            foreach (var player in players)
            {
                levelVisuals.UpdatePlayerPosition(player);
            }
            foreach (var button in buttons)
            {
                levelVisuals.UpdateButtonState(button);
            }

            var buttonState = puzzle.GetButtonToggleState();
            foreach (var spike in offSpikes)
            {
                levelVisuals.UpdateOffSpikeTile(spike.position, buttonState);
            }
            foreach (var spike in onSpikes)
            {
                levelVisuals.UpdateOnSpikeTile(spike.position, buttonState);
            }
        }
    }
}
