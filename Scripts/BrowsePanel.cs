using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

public class BrowsePanel : MonoBehaviour
{
    public ColorDictionary colorDictionary;

#pragma warning disable 649
    [Header("Components")]
    [SerializeField] private RectTransform rectTransform;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private RectTransform header;
    [SerializeField] private TextMeshProUGUI numberText;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI authorText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private LevelColorsPanel colorsPanel;
    [SerializeField] private LevelFlagsDisplay flagsDisplay;
    [SerializeField] private BookmarkButton bookmark;
    [SerializeField] private TextMeshProUGUI downloadText;

    [Header("Animation")]
    [SerializeField] private float hoverScale;
    [SerializeField] private float hoverTime;
    [SerializeField] private Ease hoverEase;
    [SerializeField] private float expandTime;
    [SerializeField] private Ease expandEase;
    private Tween currentHoverTween;
    private Tween currentExpandTween;
#pragma warning restore 649

    public int index { get; private set; }
    public LevelInfoDoc levelDoc { get; private set; }
    public BrowseScrollPanel scrollPanel { get; private set; }

    public void SetLevelIndex(int index, LevelInfoDoc levelDoc, BrowseScrollPanel scrollPanel) {
        this.index = index;
        this.scrollPanel = scrollPanel;
        this.levelDoc = levelDoc;

        currentExpandTween?.Kill();

        if (levelDoc != null) {
            LevelInfo levelInfo = LevelInfo.FromDoc(levelDoc);

            // element size
            if (scrollPanel.indexOfExpandedPanel == index) {
                rectTransform.sizeDelta = rectTransform.sizeDelta.WithY(scrollPanel.expandedPanelHeight);
            } else {
                rectTransform.sizeDelta = rectTransform.sizeDelta.WithY(scrollPanel.panelHeight);
            }

            // canvas group
            canvasGroup.alpha = 1;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
            
            // text
            numberText.text = "" + (index + 1);
            nameText.text = levelDoc.levelName;
            authorText.text = "by " + levelDoc.levelAuthor;
            descriptionText.text = "description: " + levelDoc.description;

            downloadText.text = "downloads: " + levelDoc.downloads;
        
            // preview images
            colorsPanel.SetColors(levelInfo);

            // flags
            flagsDisplay.SetFlags(levelInfo.flags);
            bookmark.SetLevel(levelInfo);
        } else {
            canvasGroup.alpha = 0;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
    }


    public void DownloadButtonPressed() {
        scrollPanel.slotPicker.PickSlotForDownload(LevelInfo.FromDoc(levelDoc));
    }

    public void DownloadPlayButtonPressed() {
        scrollPanel.slotPicker.PickSlotForDownload(LevelInfo.FromDoc(levelDoc), fileName => scrollPanel.PlayLevel(fileName));
    }


    public void ReportButton() {
        Managers.AlertManager.DoAreYouSureAlert(
            "are you sure you want to report this level?", true,
            () => {
                Managers.FileManager.TryReport(levelDoc, out string message);
                Managers.AlertManager.DoNotificationAlert(message);
            }
        );
    }


    public void OnPointerEnter() {
        currentHoverTween?.Kill();
        currentHoverTween = header.DOScale(new Vector3(hoverScale, hoverScale, hoverScale), hoverTime).SetEase(hoverEase);
    }

    public void OnPointerExit() {
        currentHoverTween?.Kill();
        currentHoverTween = header.DOScale(new Vector3(1, 1, 1), hoverTime).SetEase(hoverEase);
    }


    public void Expand() {
        currentExpandTween?.Kill();
        currentExpandTween = rectTransform.DOSizeDelta(rectTransform.sizeDelta.WithY(scrollPanel.expandedPanelHeight), expandTime).SetEase(expandEase);
        UISlideAnimator.OnOpen?.Invoke();
    }

    public void Shrink() {
        currentExpandTween?.Kill();
        currentExpandTween = rectTransform.DOSizeDelta(rectTransform.sizeDelta.WithY(scrollPanel.panelHeight), expandTime).SetEase(expandEase);
        UISlideAnimator.OnClose?.Invoke();
    }
}
