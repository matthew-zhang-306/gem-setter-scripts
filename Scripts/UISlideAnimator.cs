using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class UISlideAnimator : UIAnimator
{
    [Header("Slide")]
    [SerializeField] private Vector2 moveOffset = default;

    public Vector2 onScreenPosition { get; private set; }
    public Vector2 offScreenPosition { get; private set; }

    public bool doOpenEvent;
    public bool doCloseEvent;
    public static EmptyDelegate OnOpen;
    public static EmptyDelegate OnClose;


    protected override void Init() {
        base.Init();

        onScreenPosition  = isOnInEditor ? rectTransform.anchoredPosition : rectTransform.anchoredPosition + moveOffset;
        offScreenPosition = isOnInEditor ? rectTransform.anchoredPosition - moveOffset : rectTransform.anchoredPosition;
    }


    protected override Tween GetEnterAnimation() {
        return rectTransform.DOAnchorPos(onScreenPosition, enterTime).SetEase(enterEase);
    }

    protected override Tween GetExitAnimation() {
        return rectTransform.DOAnchorPos(offScreenPosition, exitTime).SetEase(exitEase);
    }

    protected override void SetOnScreen() {
        rectTransform.anchoredPosition = onScreenPosition;
    }
    
    protected override void SetOffScreen() {
        rectTransform.anchoredPosition = offScreenPosition;
    }


    public override void Enter() {
        base.Enter();
        if (doOpenEvent) {
            OnOpen?.Invoke();
        }
    }

    public override void Exit() {
        base.Exit();
        if (doCloseEvent) {
            OnClose?.Invoke();
        }
    }

    public override void EnterThenExit(float duration) {
        PrepareToAnimate(true);
        currentTween = DOTween.Sequence()
            .Insert(0, GetEnterAnimationWithEvent())
            .InsertCallback(duration, () => PrepareToAnimate(false, false))
            .Insert(duration, GetExitAnimationWithEvent());
    }


    private Tween GetEnterAnimationWithEvent() {
        if (doOpenEvent) {
            OnOpen?.Invoke();
        }
        return GetEnterAnimation();
    }

    private Tween GetExitAnimationWithEvent() {
        if (doCloseEvent) {
            OnClose?.Invoke();
        }
        return GetExitAnimation();
    }
}
