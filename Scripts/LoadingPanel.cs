using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoadingPanel : MonoBehaviour
{
    public UIAnimator animator;
    public CanvasGroup canvasGroup;
    public Image image;
    public float rotateSpeed;

    public void Show() {
        animator.Enter();

        if (canvasGroup.alpha == 0) {
            image.rectTransform.rotation = Quaternion.identity;
        }
    }

    public void Hide() {
        animator.Exit();
    }


    private void Update() {
        if (canvasGroup.alpha > 0) {
            image.rectTransform.rotation *= Quaternion.Euler(0, 0, rotateSpeed * Time.deltaTime);
        }
    }
}
