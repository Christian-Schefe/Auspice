using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuzzleReplayer : MonoBehaviour
{
    public LevelVisuals levelVisuals;

    public System.Action<int> stepCallback;
    public System.Action endCallback;

    public void ReplayPuzzle(Puzzle puzzle, SolutionData solution)
    {
        StopAllCoroutines();
        StartCoroutine(ReplayPuzzleCoroutine(puzzle, solution));
    }

    private IEnumerator ReplayPuzzleCoroutine(Puzzle puzzle, SolutionData solution)
    {
        var players = puzzle.GetEntities<PlayerEntity>(PuzzleEntityType.Player);
        var buttons = puzzle.GetEntities<ButtonEntity>(PuzzleEntityType.Button);
        var spikes = puzzle.GetEntities<GenericEntity>(PuzzleEntityType.Spike);

        for (int i = 0; i < solution.steps.Count; i++)
        {
            var step = solution.steps[i];
            var wait = SetState(step);
            stepCallback?.Invoke(i);
            yield return new WaitForSeconds(wait);
        }

        yield return new WaitForSeconds(0.8f);
        SetState(solution.steps[0]);
        yield return new WaitForSeconds(0.2f);
        endCallback?.Invoke();


        float SetState(SolutionStep step)
        {
            var shifts = new Dictionary<PuzzleEntity, ShiftEvent>();
            var teleports = new Dictionary<PuzzleEntity, TeleportEvent>();
            foreach (var turnEvent in step.events)
            {
                if (turnEvent is ShiftEvent shiftEvent)
                {
                    shifts[shiftEvent.entity] = shiftEvent;
                }
                else if (turnEvent is TeleportEvent teleportEvent)
                {
                    teleports[teleportEvent.entity] = teleportEvent;
                }
            }

            float animationWait = 0;

            puzzle.SetState(step.state);
            foreach (var player in players)
            {
                var shiftEvent = shifts.TryGetValue(player, out var sev) ? sev : null;
                var teleportEvent = teleports.TryGetValue(player, out var tev) ? tev : null;

                var thisAnimationWait = levelVisuals.UpdateEntityPosition(player, shiftEvent, teleportEvent);
                animationWait = Mathf.Max(animationWait, thisAnimationWait);
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
            return animationWait;
        }
    }
}
