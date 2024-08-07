using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SocialPlatforms;
using UnityEngine.Tilemaps;

public class PuzzleReplayer : MonoBehaviour
{
    public Level level;

    public Sprite buttonNormalSprite, buttonPressedSprite;
    public Tile spikeOnTile, spikeOffTile;

    public void ReplayPuzzle(Puzzle puzzle, List<Puzzle.PuzzleState> solution, System.Action callback)
    {
        StopAllCoroutines();
        StartCoroutine(ReplayPuzzleCoroutine(puzzle, solution, callback));
    }

    private IEnumerator ReplayPuzzleCoroutine(Puzzle puzzle, List<Puzzle.PuzzleState> actions, System.Action callback)
    {
        Debug.Log("Start Replay: " + actions.Count + " Items");
        var playersGO = level.GetEntityGameObjects(EntityType.Player);
        var buttonsGO = level.GetEntityGameObjects(EntityType.Button);

        var players = puzzle.GetEntities<PlayerEntity>(EntityType.Player);
        var buttons = puzzle.GetEntities<ButtonEntity>(EntityType.Button);

        foreach (var state in actions)
        {
            Debug.Log("Replaying: " + state);
            SetState(state);
            yield return new WaitForSeconds(0.2f);
        }

        yield return new WaitForSeconds(1f);
        SetState(actions[0]);
        callback?.Invoke();


        void SetState(Puzzle.PuzzleState state)
        {
            puzzle.SetState(state);
            for (int i = 0; i < playersGO.Count; i++)
            {
                playersGO[i].transform.position = level.WorldPos(players[i].position);
            }
            for (int i = 0; i < buttonsGO.Count; i++)
            {
                buttonsGO[i].GetComponent<SpriteRenderer>().sprite = buttons[i].isPressed ? buttonPressedSprite : buttonNormalSprite;
            }
            var buttonState = puzzle.GetButtonToggleState();
            foreach (var spike in puzzle.GetObjects(Puzzle.PuzzleObject.OffSpike))
            {
                level.objectTilemap.SetTile((Vector3Int)spike, buttonState ? spikeOnTile : spikeOffTile);
            }
            foreach (var spike in puzzle.GetObjects(Puzzle.PuzzleObject.OnSpike))
            {
                level.objectTilemap.SetTile((Vector3Int)spike, buttonState ? spikeOffTile : spikeOnTile);
            }
        }
    }
}
