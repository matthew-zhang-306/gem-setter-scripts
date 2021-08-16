using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EditorModeSwitcher : EditorComponent
{
    private bool inPlayMode;

    public GameObject levelManagerPrefab;
    private LevelManager currentLevelManager;

    public GameObject editorStuff;
    public EditorBlockBar blockBar;
    public UIAnimator levelUIAnimator;

    [Header("Sprites")]
    public Sprite playButton;
    public Sprite pauseButton;
    public Image image;

    private void Start() {
        image.sprite = playButton;
    }


    public void SwitchMode() {
        if (inPlayMode) {
            SwitchToEditMode();
        } else {
            SwitchToPlayMode();
        }
    }


    private void SwitchToPlayMode() {
        inPlayMode = true;

        // spawn a level manager
        GameObject levelManagerObj = GameObject.Instantiate(levelManagerPrefab);
        currentLevelManager = levelManagerObj.GetComponent<LevelManager>();
        currentLevelManager.StartPlay(levelTiles);

        editorManager.OnStartPlay();
        levelUIAnimator.Enter();
        
        image.sprite = pauseButton;
    }

    private void SwitchToEditMode() {
        inPlayMode = false;

        // delete level manager SAFELY
        currentLevelManager.StopPlay();
        
        editorManager.OnStopPlay();
        levelUIAnimator.Exit();

        image.sprite = playButton;
    }


    public void PressedUndo() {
        if (inPlayMode) {
            currentLevelManager.Undo();
        }
    }

    public void PressedRestart() {
        if (inPlayMode) {
            currentLevelManager.Reset();
        }
    }
}
