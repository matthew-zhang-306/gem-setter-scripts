using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class LevelNamePanel : MonoBehaviour
{
    public float stayTime;

    [Header("Components")]
    public UIAnimator animator;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI authorText;
    public LevelTiles levelTiles;


    private void OnEnable() {
        PlayerController.OnStartMove += Dismiss;
    }
    private void OnDisable() {
        PlayerController.OnStartMove -= Dismiss;
    }

    private void Start() {
        nameText.text = levelTiles.levelName;
        authorText.text = "by " + levelTiles.levelAuthor;

        animator.EnterThenExit(stayTime);
    }

    private void Dismiss(Vector2 _) {
        if (animator.IsOnScreen) {
            animator.Exit();
        }
    }
}
