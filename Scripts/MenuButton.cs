using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;
using TMPro;

public class MenuButton : MonoBehaviour
{   
    public enum MenuButtonState { DISABLED, NORMAL, HOVERED, PRESSED };
    private MenuButtonState state;

    private bool isHovering;

#pragma warning disable 649
    [SerializeField] private Button button;
    [SerializeField] private RectTransform container;
    [SerializeField] private Image bg;
    [SerializeField] private TextMeshProUGUI text;

    [Header("Animation!!")]
    [SerializeField] private TweenTiming timing;
    [SerializeField] private Color bgNormalColor;
    [SerializeField] private Color bgHoverColor;
    [SerializeField] private Color textNormalColor;
    [SerializeField] private Color textHoverColor;
    [SerializeField] private float pressColorMultiplier;
    [SerializeField] private float disabledColorAlpha;
    [SerializeField] private float textNormalSizeMultiplier;
    [SerializeField] private float textHoverSizeMultiplier;
    [SerializeField] private float textPressSizeMultiplier;
    private Sprite unpressedSprite;
    [SerializeField] private Sprite pressedSprite;
    private Vector2 containerBasePos;
    [SerializeField] private float containerPressYOffset;
#pragma warning restore 649

    private Tween currentAnim;


    private void Start() {
        unpressedSprite = bg.sprite;
        containerBasePos = container.anchoredPosition;

        SetState(MenuButtonState.NORMAL);
    }


    private void Update() {
        if (state == MenuButtonState.DISABLED && button.IsInteractable()) {
            SetState(MenuButtonState.NORMAL);
        }

        if (state != MenuButtonState.DISABLED && !button.IsInteractable()) {
            SetState(MenuButtonState.DISABLED);
        }
    }


    private void SetState(MenuButtonState newState) {
        if (state == newState) {
            return;
        }

        state = newState;

        // button animation
        Color desiredBgColor = bgNormalColor;
        Color desiredTextColor = textNormalColor;
        float desiredTextScale = textNormalSizeMultiplier;
        bg.sprite = unpressedSprite;
        container.anchoredPosition = containerBasePos;

        switch (state) {
            case MenuButtonState.HOVERED:
                desiredBgColor = bgHoverColor;
                desiredTextColor = textHoverColor;
                desiredTextScale = textHoverSizeMultiplier;
                break;
            case MenuButtonState.PRESSED:
                desiredBgColor = bgHoverColor.WithVal(bgHoverColor.GetVal() * pressColorMultiplier);
                desiredTextColor = textHoverColor.WithVal(textHoverColor.GetVal() * pressColorMultiplier);
                desiredTextScale = textPressSizeMultiplier;

                // set these things immediately
                bg.color = bg.color.WithVal(bg.color.GetVal() * pressColorMultiplier);
                text.color = text.color.WithVal(text.color.GetVal() * pressColorMultiplier);
                text.rectTransform.localScale = textPressSizeMultiplier * new Vector3(1, 1, 1);
                bg.sprite = pressedSprite;
                container.anchoredPosition += containerPressYOffset * Vector2.up;
                break;
            case MenuButtonState.DISABLED:
                desiredBgColor.a = disabledColorAlpha;
                desiredTextColor = textNormalColor.WithAlpha(disabledColorAlpha);
                break;
        }

        currentAnim?.Kill();

        Sequence seq = DOTween.Sequence();
        seq.Insert(0, bg.DOColor(desiredBgColor, timing.easeTime).ApplyTiming(timing));
        seq.Insert(0, text.DOColor(desiredTextColor, timing.easeTime).ApplyTiming(timing));
        seq.Insert(0, text.rectTransform.DOScale(desiredTextScale * new Vector3(1, 1, 1), timing.easeTime).ApplyTiming(timing));

        currentAnim = seq;
    }
    
    public void OnPointerEnter(BaseEventData eventData) {
        isHovering = true;

        if (state == MenuButtonState.NORMAL) {
            SetState(MenuButtonState.HOVERED);
        }
    }

    public void OnPointerExit(BaseEventData eventData) {
        isHovering = false;

        if (state == MenuButtonState.HOVERED) {
            SetState(MenuButtonState.NORMAL);
        }
    }

    public void OnPointerDown(BaseEventData eventData) {
        if (state == MenuButtonState.HOVERED) {
            SetState(MenuButtonState.PRESSED);
        }
    }

    public void OnPointerUp(BaseEventData eventData) {
        if (state == MenuButtonState.PRESSED) {
            SetState(isHovering ? MenuButtonState.HOVERED : MenuButtonState.NORMAL);
        }
    }


    public void SetText(string s) {
        text.text = s;
    }

    public void SetInteractable(bool i) {
        button.interactable = i;
    }

    // colorId: 0 for bgNormal, 1 for bgHover, 2 for textNormal, 3 for textHover
    public void SetColor(int colorId, Color color) {
        switch (colorId) {
            case 0:
                bgNormalColor = color;
                break;
            case 1:
                bgHoverColor = color;
                break;
            case 2:
                textNormalColor = color;
                break;
            case 3:
                textHoverColor = color;
                break;
            default:
                Debug.LogWarning("MenuButton " + this + " cannot set color of colorId " + colorId);
                return;
        }

        // refresh button color
        SetState(state);
    }

}
