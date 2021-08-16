using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BrowseSlotPicker : MonoBehaviour
{
    public UIAnimator panelAnimator;
    public UIAnimator backAnimator;

    public LevelSlots levelSlots;

    private LevelInfo targetLevel;
    private Action<string> downloadCallback;

    public delegate void BrowseSlotAction(string fileName);
    public static BrowseSlotAction OnDownload;
    public static BrowseSlotAction OnFailDownload;

    private void Open() {
        levelSlots.Reset();
        
        backAnimator.Enter();
        panelAnimator.Enter();
    }

    public void Close() {
        backAnimator.Exit();
        panelAnimator.Exit();
    }


    public void PickSlotForDownload(LevelInfo level, Action<string> onDownload = null) {
        targetLevel = level;
        downloadCallback = onDownload;
        Open();
    }


    public void OnSlotSelected(LevelSlotButton button) {
        Managers.AlertManager.DoAreYouSureAlert(
            "are you sure you want to overwrite this level?", !button.IsEmpty,
            () => Download(button)
        );
    }

    private void Download(LevelSlotButton button) {
        var download = Managers.FileManager.DownloadLevel(targetLevel, button.fileName, false, true, out string message);

        if (download == null) {
            // error oh no
            Debug.LogError("Download failed in BrowseSlotPicker: " + message);
            OnFailDownload?.Invoke(button.fileName);
            Close();
            return;
        }

        OnDownload?.Invoke(button.fileName);

        if (downloadCallback == null) {
            Close();
        }
        else {
            Action<string> callback = downloadCallback;
            downloadCallback = null;
            callback.Invoke(button.fileName);
        }
    }
}
