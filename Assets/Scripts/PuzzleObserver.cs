using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PuzzleObserver : MonoBehaviour
{
    public List<GameObject> players;
    public List<GameObject> buttons;
    public Tilemap groundTilemap;

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
            for (int i = 0; i < players.Count; i++)
            {
                players[i].transform.position = groundTilemap.GetCellCenterWorld((Vector3Int)puzzle.players[i].position);
            }
            for (int i = 0; i < buttons.Count; i++)
            {
                buttons[i].GetComponent<SpriteRenderer>().sprite = puzzle.buttons[i].isPressed ? buttonPressedSprite : buttonNormalSprite;
            }

            yield return new WaitForSeconds(0.2f);
        }
    }
}
