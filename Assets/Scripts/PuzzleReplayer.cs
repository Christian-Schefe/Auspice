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

        var players = puzzle.GetEntities<PlayerEntity>(EntityType.Player);
        var buttons = puzzle.GetEntities<ButtonEntity>(EntityType.Button);
        var offSpikes = puzzle.GetObjects(PuzzleObject.OffSpike);
        var onSpikes = puzzle.GetObjects(PuzzleObject.OnSpike);

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
            foreach (var pos in offSpikes)
            {
                levelVisuals.UpdateOffSpikeTile(pos, buttonState);
            }
            foreach (var pos in onSpikes)
            {
                levelVisuals.UpdateOnSpikeTile(pos, buttonState);
            }
        }
    }
}
