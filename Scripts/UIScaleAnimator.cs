using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class UIScaleAnimator : UIAnimator
{
    [Header("Scale")]
    private Vector3 baseScale;


    protected override void Init() {
        base.Init();

        baseScale = rectTransform.localScale;
    }


    protected override Tween GetEnterAnimation() {
        return rectTransform.DOScale(baseScale, enterTime).SetEase(enterEase);
    }

    protected override Tween GetExitAnimation() {
        return rectTransform.DOScale(Vector3.zero, exitTime).SetEase(exitEase);
    }

    protected override void SetOnScreen() {
        rectTransform.localScale = baseScale;
    }
    
    protected override void SetOffScreen() {
        rectTransform.localScale = Vector3.zero;
    }
}
