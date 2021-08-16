using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class UIAlphaAnimator : UIAnimator
{
    [Header("Alpha")]
    [SerializeField] private Vector2 moveOffset = default;
    [SerializeField] private float baseAlpha = 1;

    public Vector2 onScreenPosition { get; private set; }
    public Vector2 offScreenPosition { get; private set; }


    protected override void Init() {
        base.Init();
    
        onScreenPosition  = isOnInEditor ? rectTransform.anchoredPosition : rectTransform.anchoredPosition + moveOffset;
        offScreenPosition = isOnInEditor ? rectTransform.anchoredPosition - moveOffset : rectTransform.anchoredPosition;
    }

    protected override Tween GetEnterAnimation() {
        return DOTween.Sequence()
            .Insert(0, rectTransform.DOAnchorPos(onScreenPosition, 0).SetEase(enterEase))
            .Insert(0, canvasGroup.DOFade(baseAlpha, enterTime).SetEase(enterEase));
    }

    protected override Tween GetExitAnimation() {
        return DOTween.Sequence()
            .Append(canvasGroup.DOFade(0, exitTime).SetEase(exitEase))
            .Append(rectTransform.DOAnchorPos(offScreenPosition, 0));
    }

    protected override void SetOnScreen() {
        rectTransform.anchoredPosition = onScreenPosition;
        canvasGroup.alpha = baseAlpha;
    }
    
    protected override void SetOffScreen() {
        rectTransform.anchoredPosition = offScreenPosition;
        canvasGroup.alpha = 0;
    }
}
