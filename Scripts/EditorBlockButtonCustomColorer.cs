using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EditorBlockButtonCustomColorer : MonoBehaviour
{
    public EditorBlockButton blockButton;
    public Image image;

    public string colorName; // -246.6 -389.8
    
    private void Update()
    {
        if (colorName.Length > 0) {
            // look at this sick reference chain to reach the level tiles smh
            image.color = blockButton.buttonArea.blockBar.levelTiles.GetColor(colorName);
        }
    }
}
