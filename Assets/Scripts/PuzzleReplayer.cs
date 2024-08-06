using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PuzzleReplayer : MonoBehaviour
{
    public Level level;

    public Sprite buttonNormalSprite, buttonPressedSprite;

    public void ReplayPuzzle(Puzzle puzzle, List<Puzzle.PuzzleState> solution)
    {
        StartCoroutine(ReplayPuzzleCoroutine(puzzle, solution));
    }

    private IEnumerator ReplayPuzzleCoroutine(Puzzle puzzle, List<Puzzle.PuzzleState> actions)
    {
        Debug.Log("Start Replay: " + actions.Count + " Items");
        foreach (var state in actions)
        {
            Debug.Log("Replaying: " + state);
            puzzle.SetState(state);
            for (int i = 0; i < level.players.Count; i++)
            {
                level.players[i].transform.position = level.WorldPos(puzzle.players[i].position);
            }
            for (int i = 0; i < level.buttons.Count; i++)
            {
                level.buttons[i].GetComponent<SpriteRenderer>().sprite = puzzle.buttons[i].isPressed ? buttonPressedSprite : buttonNormalSprite;
            }

            yield return new WaitForSeconds(0.2f);
        }
    }
}
