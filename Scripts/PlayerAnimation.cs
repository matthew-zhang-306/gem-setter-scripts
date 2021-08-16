using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{
    public SpriteContainer spriteContainer;
    public Animator animator;
    public Transform faceContainer;

    private float blinkTimer;
    private float currentBlinkPeriod;
    public float averageBlinkPeriod;
    public float blinkPeriodVariation;

    private bool shouldPop;
    private int stopCounter;


    private void Start() {
        currentBlinkPeriod = averageBlinkPeriod + Random.Range(-blinkPeriodVariation, blinkPeriodVariation);
    }


    private void OnEnable() {
        PlayerController.OnStartMove += StartMove;
        PlayerController.OnEndMove += EndMove;
        PlayerController.OnWin += Win;
        LevelManager.OnUndo += ForceIdle;
        LevelManager.OnReset += ForceIdle;

        spriteContainer.OnPop.AddListener(Pop);
    }

    private void OnDisable() {
        PlayerController.OnStartMove -= StartMove;
        PlayerController.OnEndMove -= EndMove;
        PlayerController.OnWin -= Win;
        LevelManager.OnUndo -= ForceIdle;
        LevelManager.OnReset -= ForceIdle;
        
        spriteContainer.OnPop.RemoveListener(Pop);
    }


    private void Update() {
        DoBlinkCycle();
        DoPop();
        DoStop();
    }

    private void DoBlinkCycle() {
        if (animator.GetBool("Blink")) {
            animator.SetBool("Blink", false);
        }

        blinkTimer += Time.deltaTime;

        if (blinkTimer > currentBlinkPeriod) {
            animator.SetBool("Blink", true);

            blinkTimer = 0;
            currentBlinkPeriod = averageBlinkPeriod + Random.Range(-blinkPeriodVariation, blinkPeriodVariation);
        }
    }

    private void DoPop() {
        if (animator.GetBool("Pop")) {
            animator.SetBool("Pop", false);
        }
        if (shouldPop) {
            shouldPop = false;
            animator.SetBool("Pop", true);
        }
    }

    private void DoStop() {
        if (stopCounter > 0) {
            stopCounter--;
            if (stopCounter == 0) {
                animator.SetBool("Stop", false);
            }
        }
    }


    public void Pop() {
        shouldPop = true;
    }

    public void Win() {
        animator.SetTrigger("Win");
    }

    public void ForceIdle() {
        animator.SetTrigger("ForceIdle");
    }


    public void StartMove(Vector2 dir) {
        if (dir.x > 0) {
            // right
            faceContainer.localScale = faceContainer.localScale.WithX(1);
            animator.SetTrigger("MoveForward");
        }
        else if (dir.x < 0) {
            // left
            faceContainer.localScale = faceContainer.localScale.WithX(-1);
            animator.SetTrigger("MoveForward");
        }

        else if (dir.y > 0) {
            // up
            animator.SetTrigger("MoveUp");
        }
        else if (dir.y < 0) {
            // down
            animator.SetTrigger("MoveDown");
        }
    }

    public void EndMove(Vector2 dir) {
        stopCounter = 3;
        animator.SetBool("Stop", true);
    }
}
