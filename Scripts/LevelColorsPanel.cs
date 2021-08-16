using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelColorsPanel : MonoBehaviour
{
    public ColorDictionary colorDictionary;

    public Image[] bgImages;
    public Image[] floorImages;
    public Image[] wallImages;

    public void SetColors(LevelInfo levelInfo) {
        foreach (Image bg in bgImages) {
            bg.color = colorDictionary.GetColorData(0, levelInfo.colorHues[0]).Color;
        }

        foreach (Image floor in floorImages) {
            floor.color = colorDictionary.GetColorData(1, levelInfo.colorHues[1]).Color;
        }

        foreach (Image wall in wallImages) {
            wall.color = colorDictionary.GetColorData(2, levelInfo.colorHues[2]).Color;
        }
    }
}
