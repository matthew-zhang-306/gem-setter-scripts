using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class EditorSaveConfirm : MonoBehaviour
{
    public UIAnimator animator;
    public TextMeshProUGUI saveText;

    private void OnEnable() {
        EditorManager.OnSave += SaveDisplay;
        EditorManager.OnLoad += LoadDisplay;
    }
    private void OnDisable() {
        EditorManager.OnSave -= SaveDisplay;
        EditorManager.OnLoad -= LoadDisplay;
    }


    private void SaveDisplay(string fileName) {
        saveText.text = $"saved successfully to {fileName}.gem";
        animator.EnterThenExit(5f);
    }

    private void LoadDisplay(string fileName) {
        saveText.text = $"loaded {fileName}.gem";
        animator.EnterThenExit(5f);
    }
}
