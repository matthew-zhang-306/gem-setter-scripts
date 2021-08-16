using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelControlPanel : MonoBehaviour
{
    public Button undoButton;
    public Button restartButton;
    
    public UIAnimator nextAnimator;
    private bool canGoToNext { get { return !inputDisabled && (nextAnimator?.IsOnScreen ?? false); }}

    public LevelPlayer levelPlayer;
    public LevelManager levelManager;

    private bool inputDisabled;

    private void OnEnable() {
        PlayerController.OnWin += ShowNext;
        CreditsSequence.OnStart += DisableInput;
    }
    private void OnDisable() {
        PlayerController.OnWin -= ShowNext;   
        CreditsSequence.OnStart -= DisableInput;
    }


    private void Start() {
        if (!Managers.ScenesManager.GetTransitionTag().EndsWith("seq")) {
            // never reveal the next button
            nextAnimator = null;
        }

        if (levelPlayer != null) {
            levelManager = levelPlayer.levelManager;
        }
    }


    private void ShowNext() {
        nextAnimator?.Enter();
    }


    private void Update() {
        undoButton.interactable = !levelManager.isCurrentStateFrameOpen;
        restartButton.interactable = !levelManager.isCurrentStateFrameOpen;

        if (Input.GetKeyDown(KeyCode.Space) && canGoToNext) {
            NextPressed();
        }
    }


    public void UndoPressed() {
        if (levelManager.isCurrentStateFrameOpen) {
            return;
        }

        levelManager.Undo();
    }

    public void RestartPressed() {
        if (levelManager.isCurrentStateFrameOpen) {
            return;
        }

        levelManager.Reset();
    }

    public void NextPressed() {
        if (!canGoToNext) {
            return;
        }

        // check what the next level is
        int desiredId = levelPlayer.levelId;
        while (desiredId <= Managers.FileManager.NumLevelSlots) {
            desiredId++;
            if (Managers.FileManager.LevelExists("level" + desiredId)) {
                break;
            }
        }

        if (desiredId > Managers.FileManager.NumLevelSlots) {
            // there is no next level, so quit out
            QuitPressed();
        }
        else {
            // reload with the next level, but keep the scene name in the transition tag
            string tag = Managers.ScenesManager.GetTransitionTag();
            Managers.ScenesManager.SetTransitionTag(tag.Split(' ')[0] + " level" + desiredId + " seq");
            Managers.ScenesManager.GoToScene(Managers.ScenesManager.CurrentScene);
        }
    }

    public void QuitPressed() {
        if (levelPlayer != null) {
            levelPlayer.Quit();
        }
        else {
            Managers.ScenesManager.SetTransitionTag("level");
            Managers.ScenesManager.GoToScene(Managers.ScenesManager.MainMenuIndex);
        }
    }


    private void DisableInput() {
        inputDisabled = true;
    }
}
