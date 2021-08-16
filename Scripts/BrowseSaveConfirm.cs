using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class BrowseSaveConfirm : MonoBehaviour
{
    public UIAnimator animator;
    public TextMeshProUGUI saveText;

    private void OnEnable() {
        BrowseSlotPicker.OnDownload += DownloadDisplay;
        BrowseSlotPicker.OnFailDownload += FailDisplay;
    }
    private void OnDisable() {
        BrowseSlotPicker.OnDownload -= DownloadDisplay;
        BrowseSlotPicker.OnFailDownload -= FailDisplay;
    }


    private void DownloadDisplay(string fileName) {
        saveText.text = $"downloaded successfully to {fileName}.gem";
        animator.EnterThenExit(5f);
    }

    private void FailDisplay(string fileName) {
        saveText.text = $"a problem occurred when downloading to {fileName}.gem";
        animator.EnterThenExit(5f);
    }
}
