using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using TMPro;

public class MoveCountDisplay : MonoBehaviour
{
    public LevelPlayer levelPlayer;
    public UIAnimator animator;
    public TextMeshProUGUI moveText;
    
    public Color normalColor;
    public Color completedColor;

    private void OnEnable() {
        PlayerController.OnStartMove += Show;
        PlayerController.OnWin += OnComplete;
        LevelManager.OnUndo += OnBack;
        LevelManager.OnReset += OnBack;
    }
    private void OnDisable() {
        PlayerController.OnStartMove -= Show;
        PlayerController.OnWin -= OnComplete;
        LevelManager.OnUndo -= OnBack;
        LevelManager.OnReset -= OnBack;
    }


    private void Start() {
        moveText.color = normalColor;
    }

    private void Update() {
        int moves = levelPlayer?.levelManager?.moveCount ?? 0;
        moveText.text = "moves: " + moves;
    }


    private void Show(Vector2 _) {
        if (!animator.IsOnScreen) {
            animator.Enter();
        }
    }

    private void OnComplete() {
        moveText.color = completedColor;
    }

    private void OnBack() {
        moveText.color = normalColor;
    }
}
