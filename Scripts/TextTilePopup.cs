using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextTilePopup : MonoBehaviour
{
    public UIAnimator animator;
    public TMPro.TextMeshProUGUI textField;

    private void OnEnable() {
        TextTile.OnDisplay += Display;
    }
    private void OnDisable() {
        TextTile.OnDisplay -= Display;
    }


    private void Display(string text) {
        textField.text = text;
        animator.ExitImmediate();
        animator.Enter();
    }
}
