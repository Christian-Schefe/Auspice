using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIStarDisplay : MonoBehaviour
{
    public Image[] stars;
    public Sprite starSprite, emptyStarSprite;

    public void SetStars(int starsCount)
    {
        for (int i = 0; i < stars.Length; i++)
        {
            if (i < starsCount)
            {
                stars[i].sprite = starSprite;
            }
            else
            {
                stars[i].sprite = emptyStarSprite;
            }
        }
    }
}
