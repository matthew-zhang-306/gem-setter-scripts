
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EditorControlMenu : EditorComponent
{
    public TMP_InputField nameText;
    public TMP_InputField authorText;
    public TMP_InputField descriptionText;

    public UIAnimator animator;

    [Header("Tab")]
    public UIAnimator tabAnimator;
    public Image tabImage;

    private Sprite menuButton;
    public Sprite xButton;

    [Header("Description")]
    public UIAnimator descriptionAnimator;
    public MenuButton descriptionButton;


    private void OnEnable() {
        EditorManager.OnSave += OnSave;
        EditorManager.OnLoad += OnLoad;
        EditorManager.OnNew += OnNew;
    }
    private void OnDisable() {
        EditorManager.OnSave -= OnSave;
        EditorManager.OnLoad -= OnLoad;
        EditorManager.OnNew -= OnNew;
    }

    private void Start() {
        menuButton = tabImage.sprite;
        UpdateTextFields();
    }

    private void Update() {
        // description button text
        descriptionButton.SetText(descriptionAnimator.IsOnScreen ? "close description" : "edit description");
    }

    public override void Activate() {
        tabAnimator.Enter();
        UpdateTextFields(); // just in case
    }
    public override void Deactivate() {
        tabAnimator.Exit();
    }

    public void OnTabButtonPressed() {
        if (animator.IsOnScreen) {
            editorManager.DisableRaycastBlocker();
            tabImage.sprite = menuButton;

            animator.Exit();
            descriptionAnimator.Exit();
        } else {
            if (!editorManager.canEdit) {
                // not supposed to be able to press the tab button right now
                return;
            }

            editorManager.EnableRaycastBlocker();
            tabImage.sprite = xButton;

            animator.Enter();
        }
    }

    public void ToggleDescription() {
        if (descriptionAnimator.IsOnScreen) {
            descriptionAnimator.Exit();
        }
        else {
            descriptionAnimator.Enter();
        }
    }

    public void OnNameChanged() {
        levelTiles.levelName = nameText.text;
    }

    public void OnAuthorChanged() {
        levelTiles.levelAuthor = authorText.text;
    }

    public void OnDescriptionChanged() {
        levelTiles.description = descriptionText.text;
    }

    public void UpdateTextFields() {
        nameText.text = levelTiles.levelName;
        authorText.text = levelTiles.levelAuthor;
        descriptionText.text = levelTiles.description;
    }

    public void OnSave(string _) {
        if (animator.IsOnScreen) {
            OnTabButtonPressed();
        }
    }

    public void OnLoad(string _) {
        OnNew();
    }

    public void OnNew() {
        UpdateTextFields();

        if (animator.IsOnScreen) {
            OnTabButtonPressed();
        }
    }


    public void Clear() {
        // no alert to call here
        editorManager.Clear();
    }

    public void New() {
        editorManager.DoUnsavedChangesAlert(editorManager.New);
    }

    public void QuitToMenu() {
        editorManager.DoUnsavedChangesAlert(editorManager.QuitToMenu);
    }
}
