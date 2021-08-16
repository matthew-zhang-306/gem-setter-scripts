using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

public class BrowseSearchPanel : MonoBehaviour
{
    public BrowseScrollPanel browsePanel;

    public float searchRefreshTime;
    public float optionsRefreshTime;
    private float refreshTimer;

    private BrowseSearchOptions searchOptions;


    [Header("Components")]
    public UIAnimator raycastBlocker;
    public UIAnimator optionsAnimator;
    public Image searchBackground;
    public Image optionsBackground;
    public Button expandButton;
    public RectTransform arrowTransform;

    [Header("Option Components")]
    public Toggle officialToggle;
    public Toggle verifiedToggle;
    public Toggle bookmarkedToggle;
    public TMP_Dropdown sortDropdown;
    public TMP_Dropdown searchDropdown;

    [Header("Alpha Animation")]
    public float defaultAlpha;
    public float expandedAlpha;
    public float alphaTime;

    [Header("Arrow Animation")]
    public float arrowRotateTime;
    public Ease arrowRotateEase;
    
    private Tween expandTween;
    private bool IsOpen { get { return optionsAnimator.IsOnScreen; }}


    private void Awake() {
        refreshTimer = -1;
    }

    private void Update() {
        if (refreshTimer >= 0) {
            refreshTimer = Mathf.Max(0, refreshTimer - Time.deltaTime);

            if (refreshTimer == 0) {
                Refresh();
            }
        }
    }

    private void OpenOptions() {
        raycastBlocker.Enter();
        optionsAnimator.Enter();

        // do extra tweens
        expandTween?.Kill();
        expandTween = DOTween.Sequence()
            .Insert(0, searchBackground.DOFade(expandedAlpha, alphaTime))
            .Insert(0, optionsBackground.DOFade(expandedAlpha, alphaTime))
            .Insert(0, arrowTransform.DORotate(Vector3.zero.WithZ(180), arrowRotateTime).SetEase(arrowRotateEase));
    }

    private void CloseOptions() {
        raycastBlocker.Exit();
        optionsAnimator.Exit();

        // do extra tweens
        expandTween?.Kill();
        expandTween = DOTween.Sequence()
            .Insert(0, searchBackground.DOFade(defaultAlpha, alphaTime))
            .Insert(0, optionsBackground.DOFade(defaultAlpha, alphaTime))
            .Insert(0, arrowTransform.DORotate(Vector3.zero.WithZ(0), arrowRotateTime).SetEase(arrowRotateEase));
    }

    public void ToggleExpand() {
        if (!IsOpen) {
            OpenOptions();
        }
        else {
            CloseOptions();
        }
    }


    public void OnSearchUpdate(string text) {
        refreshTimer = searchRefreshTime;
        searchOptions.searchText = text;
    }

    public void OnOptionUpdate() {
        refreshTimer = optionsRefreshTime;
    }


    public void Refresh() {
        // update options things to reflect new options
        searchOptions.requireOfficial = officialToggle.isOn;
        searchOptions.requireVerified = verifiedToggle.isOn;
        searchOptions.requireBookmarked = bookmarkedToggle.isOn;
        searchOptions.sortBy = (BrowseSearchOptions.SortBy)sortDropdown.value;
        searchOptions.searchBy = (BrowseSearchOptions.SearchBy)searchDropdown.value;

        refreshTimer = -1;
        browsePanel.SetSearchOptions(searchOptions);
    }
}


[System.Serializable]
public struct BrowseSearchOptions {
    public enum SortBy {
        RECENT = 0,
        DOWNLOADS = 1
    }

    public enum SearchBy {
        ANY = 0,
        NAME = 1,
        AUTHOR = 2,
        DESCRIPTION = 3
    }

    public string searchText;

    public bool requireOfficial;
    public bool requireVerified;
    public bool requireBookmarked;

    public SortBy sortBy;
    public SearchBy searchBy;
}