using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

/*
 * An abstract class that handles enter/exit animations for a UI panel.
 */
[RequireComponent(typeof(CanvasGroup))]
public abstract class UIAnimator : MonoBehaviour
{
    protected bool inited;

    protected CanvasGroup canvasGroup;
    protected RectTransform rectTransform;

    public bool IsOnScreen { get { return canvasGroup.blocksRaycasts; }}

#pragma warning disable 649
    [Header("Awake Behavior")]
    [SerializeField] protected bool isOnInEditor;
    [SerializeField] protected bool startsOnAtRuntime;
    [SerializeField] protected bool animatesOnStart;
    [SerializeField] protected bool animationCompletesOnKill;

    [Header("Animation Timing")]
    [SerializeField] protected float enterTime;
    [SerializeField] protected Ease enterEase;
    [SerializeField] protected float exitTime;
    [SerializeField] protected Ease exitEase;
#pragma warning restore 649

    public Tween currentTween { get; protected set; }

    // prepares the UIEntrancer for use.
    protected virtual void Init() {
        inited = true;
        
        rectTransform = transform as RectTransform;
        canvasGroup = GetComponent<CanvasGroup>();
    }


    protected virtual void Awake() {
        if (!inited) {
            Init();

            if (animatesOnStart != startsOnAtRuntime) {
                EnterImmediate();
            } else {
                ExitImmediate();
            }
        }
    }

    protected virtual void Start() {
        if (animatesOnStart) {
            if (startsOnAtRuntime) {
                Enter();
            } else {
                Exit();
            }
        }
    }

    protected void PrepareToAnimate(bool isOnScreen, bool newAnimation = true) {
        if (!inited) {
            Init();
        }
        
        if (newAnimation) {
            currentTween?.Kill(animationCompletesOnKill);
        }

        canvasGroup.interactable = isOnScreen;
        canvasGroup.blocksRaycasts = isOnScreen;
    }


    /*
     * Animates the panel on screen.
     */
    public virtual void Enter() {
        PrepareToAnimate(true);
        currentTween = GetEnterAnimation();
    }

    /*
     * Animates the panel off screen.
     */
    public virtual void Exit() {
        PrepareToAnimate(false);
        currentTween = GetExitAnimation();
    }

    /*
     * Animates the panel on screen, keeps it there for a given amount of time, and then removes the panel.
     */
    public virtual void EnterThenExit(float duration) {
        PrepareToAnimate(true);
        currentTween = DOTween.Sequence()
            .Insert(0, GetEnterAnimation())
            .InsertCallback(duration, () => PrepareToAnimate(false, false))
            .Insert(duration, GetExitAnimation());
    }


    /*
     * Immediately sets the panel's state to on screen without animating.
     */
    public void EnterImmediate() {
        PrepareToAnimate(true);
        SetOnScreen();
    }

    /*
     * Immediately sets the panel's state to off screen without animating.
     */
    public void ExitImmediate() {
        PrepareToAnimate(false);
        SetOffScreen();
    }


    protected abstract Tween GetEnterAnimation();
    protected abstract Tween GetExitAnimation();
    protected abstract void SetOnScreen();
    protected abstract void SetOffScreen();
    
}