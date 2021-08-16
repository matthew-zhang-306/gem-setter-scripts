using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BrowseScrollPanel : MonoBehaviour
{
    public int dbBatchSize;
    private bool hasAllResults;
    public float fetchWaitTime;
    private float fetchTimer;

    private BrowseSearchOptions searchOptions;

    public string levelSceneName;
    public string browseSceneName;

    [Header("Arrangement")]
    public float panelPadding;
    public float panelHeight;
    public float expandedPanelHeight;
    public float footerHeight;
    private float minimumContentHeight;

    [Header("References")]
    public ScrollRect scrollRect;
    public RectTransform panelsParent;
    public RectTransform footer;
    public TextMeshProUGUI footerText;
    public Transform topMarker;
    public Transform bottomMarker;
    public BrowseSlotPicker slotPicker;

    private List<LevelInfoDoc> levels;

    private LevelInfoDoc GetLevelInfoDoc(int index) {
        return index < levels.Count ? levels[index] : null;
    }
    private LevelInfo GetLevelInfo(int index) {
        return index < levels.Count ? LevelInfo.FromDoc(levels[index]) : null;
    }

    private BrowsePanel[] levelPanels;
    private int indexOfTopPanel;
    private int topPanelLevelIndex;
    public int indexOfExpandedPanel { get; private set; }

    private RectTransform GetLevelPanelRect(int index) {
        return levelPanels[index].transform as RectTransform;
    }
    private int indexOfNextButton { get {
        if (indexOfTopPanel == levelPanels.Length - 1) {
            return 0;
        } else {
            return indexOfTopPanel + 1;
        }
    }}
    private int indexOfBottomPanel { get {
        if (indexOfTopPanel == 0) {
            return levelPanels.Length - 1;
        } else {
            return indexOfTopPanel - 1;
        }
    }}
    private int bottomPanelLevelIndex { get {
        return topPanelLevelIndex + levelPanels.Length - 1;
    }}
    private int maxLevelIndex { get { return levels.Count - 1; }}


    private void Awake() {
        minimumContentHeight = (transform as RectTransform).sizeDelta.y;
    }

    private void Start() {
        InitContent();
    }

    private void InitContent() {
        levels = new List<LevelInfoDoc>();
        hasAllResults = false;
        FetchNextBatch();
        
        indexOfTopPanel = 0;
        topPanelLevelIndex = 0;
        indexOfExpandedPanel = -1;

        levelPanels = new BrowsePanel[panelsParent.childCount];
        for (int i = 0; i < levelPanels.Length; i++) {
            // here i have to grab children in backwards order because bottom of hierarchy = top of list
            levelPanels[i] = panelsParent.GetChild(levelPanels.Length - 1 - i).GetComponent<BrowsePanel>();
            GetLevelPanelRect(i).anchoredPosition = new Vector2(0, -panelPadding - i * (panelHeight + panelPadding));

            levelPanels[i].SetLevelIndex(i, GetLevelInfoDoc(i), this);
        }

        scrollRect.verticalScrollbar.value = 1;
    }


    private void Update() {
        if (footerText.transform.position.y > bottomMarker.position.y) {
            fetchTimer += Time.deltaTime;

            if (fetchTimer >= fetchWaitTime) {
                FetchNextBatch();
                fetchTimer = 0;
            }
        }
    }


    public void SetSearchOptions(BrowseSearchOptions options) {
        searchOptions = options;
        InitContent();
    }


    private void FetchNextBatch() {
        if (hasAllResults) {
            return;
        }

        List<LevelInfoDoc> nextBatch = Managers.FileManager.FetchLevels(dbBatchSize, levels.Count, searchOptions);
        levels.AddRange(nextBatch);

        if (nextBatch.Count < dbBatchSize) {
            hasAllResults = true;
        }

        UpdateContent();
        OnScrollValueChanged(scrollRect.normalizedPosition);
    }


    private void UpdateContent() {
        float desiredHeight = 2 * panelPadding + levels.Count * (panelHeight + panelPadding) + footerHeight;
        (transform as RectTransform).sizeDelta = (transform as RectTransform).sizeDelta.WithY(Mathf.Max(desiredHeight, minimumContentHeight));
    
        footerText.text = hasAllResults ? "end of results" : "loading more...";
    }


    private void UpdatePanels() {
        if (levelPanels == null || levels.Count < levelPanels.Length) {
            return;
        }

        while (topPanelLevelIndex > 0 && levelPanels[indexOfTopPanel].transform.position.y < topMarker.position.y) {
            // need to move the bottom button to the top.
            GetLevelPanelRect(indexOfBottomPanel).anchoredPosition = 
                GetLevelPanelRect(indexOfBottomPanel).anchoredPosition.WithY(GetLevelPanelRect(indexOfTopPanel).anchoredPosition.y + panelPadding + panelHeight);
            indexOfTopPanel = indexOfBottomPanel;

            topPanelLevelIndex--;
            levelPanels[indexOfTopPanel].SetLevelIndex(topPanelLevelIndex, GetLevelInfoDoc(topPanelLevelIndex), this);

            levelPanels[indexOfTopPanel].transform.SetAsLastSibling();
        }

        while (bottomPanelLevelIndex < maxLevelIndex &&
            levelPanels[indexOfBottomPanel].transform.position.y > bottomMarker.position.y) {
            // need to move the top button to the bottom.
            GetLevelPanelRect(indexOfTopPanel).anchoredPosition = 
                GetLevelPanelRect(indexOfTopPanel).anchoredPosition.WithY(GetLevelPanelRect(indexOfBottomPanel).anchoredPosition.y - panelPadding - panelHeight);
            indexOfTopPanel = indexOfNextButton;

            topPanelLevelIndex++;
            levelPanels[indexOfBottomPanel].SetLevelIndex(bottomPanelLevelIndex, GetLevelInfoDoc(bottomPanelLevelIndex), this);
        
            levelPanels[indexOfBottomPanel].transform.SetAsFirstSibling();
        }
    }


    public void OnScrollValueChanged(Vector2 _) {
        UpdatePanels();
    }


    public void OnClick(BrowsePanel browsePanel) {
        if (indexOfExpandedPanel == browsePanel.index) {
            indexOfExpandedPanel = -1;
            browsePanel.Shrink();
        }
        else {
            if (indexOfExpandedPanel >= 0) {
                // let's just shrink all the panels instead of finding the right one why not
                foreach (BrowsePanel panel in levelPanels) {
                    panel.Shrink();
                }
            }

            indexOfExpandedPanel = browsePanel.index;
            browsePanel.Expand();
        }
    }


    public void PlayLevel(string fileName) {
        Managers.ScenesManager.SetTransitionTag(browseSceneName + " " + fileName);
        Managers.ScenesManager.GoToScene(levelSceneName);
    }

    public void BackToMenu() {
        Managers.ScenesManager.GoToScene(Managers.ScenesManager.MainMenuIndex);
    }



}
