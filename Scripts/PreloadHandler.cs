using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PreloadHandler : MonoBehaviour
{
    public UIAnimator resolutionAnimator;
    
    private void Start() {
        if (PlayerPrefs.GetInt("Preloaded", 0) > 0) {
            Managers.ScenesManager.SetTransitionTag("preload");
            Managers.ScenesManager.GoToSceneImmediate(Managers.ScenesManager.MainMenuIndex);
        }
        else {
            // before installing levels, check if there are any existing levels. if there are, then don't install anything
            bool levelsExist = false;
            for (int i = 0; !levelsExist && i < Managers.FileManager.NumDefaultLevels; i++) {
                levelsExist = Managers.FileManager.LevelExists("level" + (i+1));
            }

            if (!levelsExist) {
                Managers.FileManager.InstallDefaultLevels();
            }
        }
    }

    public void Continue() {
        PlayerPrefs.SetInt("Preloaded", 1);

        resolutionAnimator.Exit();
        Managers.ScenesManager.SetTransitionTag("preload");
        Managers.ScenesManager.GoToScene(Managers.ScenesManager.MainMenuIndex);
    }


    [ContextMenu("Reset")]
    public void ResetPreloaded() {
        PlayerPrefs.SetInt("Preloaded", 0);
    }
}
