using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EditorHelpPanel : EditorComponent
{
    public UIAnimator animator;

    public Image helpImage;
    public Sprite[] helpSprites;
    public GameObject[] pageObjects;
    
    public Transform pageIndicatorsParent;
    private Image[] pageIndicators;

    private int currentPage;
    private Action closedCallback;

    public Button prevButton;

    public string playerPref;

    private void Start() {
        pageIndicators = pageIndicatorsParent.GetComponentsInChildren<Image>();
        if (pageIndicators.Length != helpSprites.Length) {
            Debug.LogError("Wrong number of page indicators in EditorHelpPanel: expected " + helpSprites.Length + " but found " + pageIndicators.Length);
        }

        if (playerPref != null && playerPref.Length > 0 && PlayerPrefs.GetInt(playerPref, 0) == 0) {
            PlayerPrefs.SetInt(playerPref, 1);
            Open();
        }
    }

    public void Open() {
        Open(null);
    }

    public void Open(Action callback) {
        editorManager?.EnableRaycastBlocker();
        animator.Enter();
    
        SetPage(0);
        closedCallback = callback;
    }

    public void Close() {
        editorManager?.DisableRaycastBlocker();
        animator.Exit();

        closedCallback?.Invoke();
    }


    private void SetPage(int page) {
        if (page >= pageIndicators.Length) {
            Close();
            return;
        }

        currentPage = page;
        helpImage.sprite = helpSprites[page];

        // set page objects
        for (int o = 0; o < pageObjects.Length; o++) {
            if (pageObjects[o] != null) {
                pageObjects[o]?.SetActive(o == page);
            }
        }

        // set page indicators
        for (int p = 0; p < pageIndicators.Length; p++) {
            pageIndicators[p].color = pageIndicators[p].color.WithAlpha(p == page ? 1 : 0.2f);            
        }
    }


    public void NextPage() {
        SetPage(currentPage + 1);
    }

    public void PrevPage() {
        SetPage(Mathf.Max(0, currentPage - 1));
    }


    private void Update() {
        prevButton.interactable = currentPage != 0;        
    }
}
