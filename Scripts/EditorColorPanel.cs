using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EditorColorPanel : EditorComponent
{
    private static int[] copiedColors;
    public EditorColorPicker[] colorPickers;

    public UIAnimator panelAnimator;
    public UIAnimator backAnimator;

    public MenuButton pasteButton;


    public void Open() {
        // open panels
        backAnimator.Enter();
        panelAnimator.Enter();
        
        // set pickers to current level colors
        for (int p = 0; p < colorPickers.Length; p++) {
            colorPickers[p].SetHue(levelTiles.colorDatas[p].hue);
            colorPickers[p].SetName(levelTiles.colorDatas[p].name);
        }

        pasteButton.SetInteractable(copiedColors != null);
    }

    public void Close() {
        // close panels
        backAnimator.Exit();
        panelAnimator.Exit();
    }


    public void Copy() {
        copiedColors = colorPickers.Select(colorPicker => colorPicker.GetHue()).ToArray();
        pasteButton.SetInteractable(true);
    }

    public void Paste() {
        for (int i = 0; i < Mathf.Min(copiedColors.Length, colorPickers.Length); i++) {
            colorPickers[i].SetHue(copiedColors[i]);
        }
    }

    public void Cancel() {
        // do nothing and close
        Close();
    }

    public void Accept() {
        // set colors in level tiles
        // note: replace this with a call to some kind of level editor manager class
        for (int p = 0; p < colorPickers.Length; p++) {
            levelTiles.colorDatas[p].hue = colorPickers[p].GetHue();
        }

        // then close
        Close();
    }
}
