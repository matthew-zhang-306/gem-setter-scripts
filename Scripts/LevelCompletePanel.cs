using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelCompletePanel : MonoBehaviour
{
    public bool isForCompleted;
    public UIAnimator animator;
    public TMPro.TMP_Text description;

    private void OnEnable() {
        if (isForCompleted) {
            PlayerController.OnWin += Show;
        } else {
            PlayerController.OnDie += Show;
        }
        
        LevelManager.OnUndo += Dismiss;
        LevelManager.OnReset += Dismiss;
    }
    private void OnDisable() {
        if (isForCompleted) {
            PlayerController.OnWin -= Show;
        } else {
            PlayerController.OnDie -= Show;
        }

        LevelManager.OnUndo -= Dismiss;
        LevelManager.OnReset -= Dismiss;
    }

    private void Start() {
        if (isForCompleted && Managers.ScenesManager.GetTransitionTag().EndsWith("seq")) {
            description.text = "press space to advance or esc to exit.";
        }
    }

    private void Show() {
        animator.Enter();
    }

    private void Dismiss() {
        animator.Exit();
    }
}
