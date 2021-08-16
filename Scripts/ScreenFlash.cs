using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class ScreenFlash : MonoBehaviour
{
    public Image winFlashBack;
    public Image undoFlashBack;
    public Image resetFlashBack;
    public CanvasGroup winFlashGroup;
    public CanvasGroup undoFlashGroup;
    public CanvasGroup resetFlashGroup;

    private Tween currentFlash;

    private void DoFlash(Image back, CanvasGroup canvasGroup, Color color, float duration) {
        currentFlash?.Complete();
        
        back.color = color;
        canvasGroup.alpha = 1;
        currentFlash = canvasGroup.DOFade(0, duration);
    }

    
    private void OnEnable() {
        LevelManager.OnUndo += DoUndoFlash;
        LevelManager.OnReset += DoResetFlash;
        PlayerController.OnWin += DoWinFlash;
    }

    private void OnDisable() {
        LevelManager.OnUndo -= DoUndoFlash;
        LevelManager.OnReset -= DoResetFlash;
        PlayerController.OnWin -= DoWinFlash;
    }


    private void DoWinFlash() {
        DoFlash(winFlashBack, winFlashGroup, Color.white.WithAlpha(0.9f), 0.5f);
    }

    private void DoUndoFlash() {
        DoFlash(undoFlashBack, undoFlashGroup, Color.white.WithAlpha(0.1f), 0.5f);
    }

    private void DoResetFlash() {
        DoFlash(resetFlashBack, resetFlashGroup, Color.white.WithAlpha(0.1f), 0.5f);
    }
}
