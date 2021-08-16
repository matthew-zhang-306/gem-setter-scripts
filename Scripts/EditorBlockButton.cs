using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class EditorBlockButton : MonoBehaviour
{
    [SerializeField] private TileDictionary tileDictionary;

    [HideInInspector] public int buttonNum;

    public int id;
    public byte alt;

    public GameObject block { get { return tileDictionary.tileTypes[id].prefab; }}
    

    [SerializeField] private bool isPlayer;
    public bool IsPlayer { get { return isPlayer; }}

    public bool isDefaultSelected;

    public bool isOnMainBar;

    [Header("Components")]
    public Image backMask;
    public Image backImage;
    public RectTransform imageContainer;
    public Image blockImage;
    public EditorBlockButtonCustomColorer blockColorer;

    public EditorBlockButtonArea buttonArea;

    [Header("Animation")]
    public TweenTiming normalTiming;
    public TweenTiming popTiming;

    [System.Serializable]
    public struct SelectionAnimation {
        public Vector2 backMaskSize;
        public Vector2 imageScale;
        public float backAlpha;
    }
    public SelectionAnimation selectedAnimation;
    private SelectionAnimation deselectedAnimation;

    bool previousIsSelected;
    bool isSelected { get { return buttonArea?.blockBar?.selectedBlockType == this; }}


    private void Start() {
        deselectedAnimation = new SelectionAnimation {
            backMaskSize = backMask.rectTransform.sizeDelta,
            imageScale = imageContainer.localScale,
            backAlpha = backImage.color.a
        };

        if (isDefaultSelected) {
            SwitchBlock();
        }

        if (isPlayer) {
            // do nothing. this is just to supress warnings.
        }
    }


    public void SwitchBlock() {
        buttonArea.SwitchBlock(this);
    }


    private void Update() {
        if (previousIsSelected != isSelected) {
            AnimateSelectionChange();
        }

        previousIsSelected = isSelected;
    }


    private void AnimateSelectionChange() {
        SelectionAnimation anim = isSelected ? selectedAnimation : deselectedAnimation;

        Sequence seq = DOTween.Sequence();
        
        seq.Insert(0, backMask.rectTransform.DOSizeDelta(anim.backMaskSize, normalTiming.easeTime).ApplyTiming(normalTiming));
        seq.Insert(0, backImage.DOFade(anim.backAlpha, normalTiming.easeTime).ApplyTiming(normalTiming));

        if (!isSelected) {
            // decrease image size normally
            seq.Insert(0, imageContainer.DOScale(anim.imageScale, normalTiming.easeTime).ApplyTiming(normalTiming));
        }
    }

    public void ScaleImageOnSelected(bool shouldPop) {
        TweenTiming timing = shouldPop ? popTiming : normalTiming;

        if (shouldPop) {
            // start the image at a small size
            imageContainer.localScale = new Vector3(0, 0, 1);
        }

        imageContainer.DOScale(selectedAnimation.imageScale, timing.easeTime).ApplyTiming(timing);
    }


    public void OnHover() {
        backImage.DOFade(selectedAnimation.backAlpha, normalTiming.easeTime).ApplyTiming(normalTiming);
    }

    public void OnUnhover() {
        if (!isSelected) {
            backImage.DOFade(deselectedAnimation.backAlpha, normalTiming.easeTime).ApplyTiming(normalTiming);
        }
    }
}
