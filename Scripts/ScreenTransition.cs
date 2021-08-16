using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class ScreenTransition : MonoBehaviour
{
    public int numRows;
    public int numCols;

    public GameObject squarePrefab;
    private Image[,] squares;

    public TweenTiming intoSceneTiming;
    public TweenTiming outOfSceneTiming;

    public RectTransform tweenTracker;
    public CanvasGroup canvasGroup;
    private Coroutine currentTransition;
    private Tween currentAnim;

    private void Awake() {
        if (squares == null) {
            Init();
        }
    }

    private void Init() {
        // initialize things!
        squares = new Image[numRows, numCols];

        for (int r = 0; r < numRows; r++) {
            for (int c = 0; c < numCols; c++) {
                squares[r,c] = Instantiate(squarePrefab, transform.position, Quaternion.identity, transform).GetComponent<Image>();
                
                squares[r,c].rectTransform.anchoredPosition = new Vector2(
                    r * squares[r,c].rectTransform.rect.width,
                    - c * squares[r,c].rectTransform.rect.height
                );
            }
        }
    }


    public void TransitionIntoScene(Action callback = null) {
        Transition(0, intoSceneTiming.easeTime, intoSceneTiming.easingType, callback);
    }

    public void TransitionOutOfScene(Action callback = null) {
        Transition(1, outOfSceneTiming.easeTime, outOfSceneTiming.easingType, callback);
    }

    private void Transition(float desiredAlpha, float time, Ease easeType, Action callback) {
        if (squares == null) {
            Init();
        }

        // handle the case where a different screen transition is already happening (ie: complete that animation instantly)
        KillTransition();

        foreach (Image square in squares) {
            square.color = square.color.WithAlpha(1 - desiredAlpha);
        }

        currentTransition = StartCoroutine(DoTransition(desiredAlpha, time, easeType, callback));
    }


    private IEnumerator DoTransition(float desiredAlpha, float time, Ease easeType, Action callback) {
        canvasGroup.blocksRaycasts = desiredAlpha == 1;

        int numDiags = numRows + numCols - 1;
        int currentDiag = 0;

        tweenTracker.anchoredPosition = tweenTracker.anchoredPosition.WithX(0);
        currentAnim = tweenTracker.DOAnchorPosX(numDiags, time).SetEase(easeType);

        while (currentDiag < numDiags) {
            while (tweenTracker.anchoredPosition.x > currentDiag + 0.5f) {
                int r = Mathf.Min(currentDiag, numRows - 1);
                int c = Mathf.Max(0, currentDiag - (numRows - 1));
                
                while (r >= 0 && c < numCols) {
                    squares[r,c].color = squares[r,c].color.WithAlpha(desiredAlpha);
                    
                    r--;
                    c++;
                }

                currentDiag++;
            }

            yield return new WaitForSecondsRealtime(0.02f);
        }

        while (currentAnim.IsActive() && currentAnim.IsPlaying()) {
            yield return new WaitForSecondsRealtime(0.02f);
        }

        callback?.Invoke();
    }


    public void KillTransition() {
        currentAnim?.Kill();
        if (currentTransition != null) {
            StopCoroutine(currentTransition);
        }
    }
}
