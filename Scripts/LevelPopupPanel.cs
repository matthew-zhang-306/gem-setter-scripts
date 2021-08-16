using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

public class LevelPopupPanel : MonoBehaviour
{
    private static string UPLOAD_HELP_PLAYER_PREF = "viewed uploadHelp";

    public LevelSelectMenu levelSelect;
    public EditorHelpPanel uploadHelpPanel;

    public UIAlphaAnimator backAnimator;
    public UIAnimator contentAnimator;
    public UIAnimator emptyContentAnimator;

    private LevelInfo copiedLevel;
    private LevelSlotButton levelButton;

    [Header("Components")]
    public CanvasGroup panelGroup;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI authorText;
    public TextMeshProUGUI descriptionText;
    public MenuButton uploadButton;
    public MenuButton copyButton;
    public MenuButton pasteButton;
    public Button uploadHelpButton;
    public LevelColorsPanel colorsPanel;
    public LevelFlagsDisplay flagsDisplay;
    public BookmarkButton bookmark;

    [Header("Empty Components")]
    public MenuButton emptyPasteButton;

    [Header("Animation")]
    public TweenTiming scaleTiming;
    public TweenTiming alphaTiming;
    public TweenTiming exitTiming;
    public float contentDelay;
    public float halfButtonHeight; // when i'm animating this in code i have to put the panel at the position of the button. it turns out i can get most of the way there but i need this manually input offset amount to adjust for differences in panel alignments

    private Tween panelTween;
    private Vector2 baseAnchorPos;
    private Rect baseRect;


    private void Start() {
        baseAnchorPos = (transform as RectTransform).anchoredPosition;
        baseRect = (transform as RectTransform).rect;

        contentAnimator.gameObject.SetActive(true);
        emptyContentAnimator.gameObject.SetActive(true);
    }

    public void Open(LevelSlotButton levelButton) {
        InitPosition(levelButton);
        InitData(levelButton);
        
        // do other tweens
        backAnimator.Enter();
        StartCoroutine(DoContentWithDelay(true));

        // do panel tween
        panelTween?.Complete();
        panelTween = DOTween.Sequence().Insert(
            0, panelGroup.DOFade(1, alphaTiming.easeTime).SetEase(alphaTiming.easingType)
        ).Insert(
            scaleTiming.delay, (transform as RectTransform).DOScale(new Vector3(1, 1, 1), scaleTiming.easeTime).SetEase(scaleTiming.easingType)
        ).Insert(
            scaleTiming.delay, (transform as RectTransform).DOAnchorPos(baseAnchorPos, scaleTiming.easeTime).SetEase(scaleTiming.easingType)
        ).InsertCallback(
            scaleTiming.delay + 0.05f, () => UISlideAnimator.OnOpen?.Invoke()
        );
    }

    public void Close() {
        backAnimator.Exit();
        contentAnimator.Exit();
        emptyContentAnimator.Exit();

        panelGroup.interactable = false;
        panelGroup.blocksRaycasts = false;

        panelTween?.Complete();
        panelTween = panelGroup.DOFade(0, exitTiming.easeTime).SetEase(exitTiming.easingType);

        UISlideAnimator.OnClose?.Invoke();
    }

    private void CloseImmediate() {
        backAnimator.ExitImmediate();
        contentAnimator.ExitImmediate();

        panelTween?.Complete();
        panelGroup.interactable = false;
        panelGroup.blocksRaycasts = false;
        panelGroup.alpha = 0;
    }

    private IEnumerator DoContentWithDelay(bool shouldEnter) {
        yield return new WaitForSeconds(contentDelay);

        UIAnimator desiredAnimator = !levelButton.IsEmpty ? contentAnimator : emptyContentAnimator;
        
        if (shouldEnter) {
            desiredAnimator.Enter();
        } else {
            desiredAnimator.Exit();
        }
    }

    
    public void InitData(LevelSlotButton levelButton) {
        if (levelButton.IsEmpty) {
            // set paste
            emptyPasteButton.SetInteractable(copiedLevel != null);
        } else {
            LevelInfo level = levelButton.level;

            // set text
            nameText.text = level.levelName;
            authorText.text = "by " + level.levelAuthor;
            descriptionText.text = "description: " + level.description;

            // set upload
            uploadButton.SetText(level.flags.uploaded ? "redownload" : "upload");
            uploadHelpButton.gameObject.SetActive(!level.flags.uploaded && PlayerPrefs.GetInt(UPLOAD_HELP_PLAYER_PREF, 0) > 0);

            // set copy/paste
            pasteButton.SetInteractable(copiedLevel != null);

            // set colors
            colorsPanel.SetColors(level);

            // set flags
            flagsDisplay.SetFlags(level.flags);
            bookmark.SetLevel(level);
        }
    }

    public void InitPosition(LevelSlotButton levelButton) {
        this.levelButton = levelButton;

        // initialize panelGroup
        panelGroup.alpha = 0;
        panelGroup.interactable = true;
        panelGroup.blocksRaycasts = true;

        // determine how small the panel has to be to fit on the level button
        Rect buttonRect = (levelButton.transform as RectTransform).rect;
        (transform as RectTransform).localScale =
            new Vector3(buttonRect.width / baseRect.width, buttonRect.height / baseRect.height, 1);
        
        // determine where the panel needs to go to be on the level button
        Vector3 buttonPos = levelButton.transform.position - new Vector3(0, halfButtonHeight, 0);
        (backAnimator.transform as RectTransform).anchoredPosition = backAnimator.onScreenPosition; // we need to move this on screen manually here because moving it will cause the panel to move
        transform.position = buttonPos;
    }


    public void Play() {
        levelSelect.LoadLevel(levelButton, true);
    }

    public void Edit() {
        if (!levelButton.IsEmpty) {
            Managers.AlertManager.DoAreYouSureAlert(
                "if you edit this level, it will be unverified. are you sure?", levelButton.level.flags.verified,
                () => levelSelect.EditLevel(levelButton)
            );
        } else {
            levelSelect.mainMenu.Edit();
        }
    }

    public void Upload() {
        if (levelButton.level.flags.uploaded) {
            // redownload.
            Managers.AlertManager.DoAreYouSureAlert(
                "any changes you made to the level will be lost. are you sure you want to redownload?",
                levelButton.level.flags.modified,
                () => levelSelect.RedownloadLevel(levelButton)
            );
        }
        else {
            // upload.
            if (PlayerPrefs.GetInt(UPLOAD_HELP_PLAYER_PREF, 0) == 0) {
                PlayerPrefs.SetInt(UPLOAD_HELP_PLAYER_PREF, 1);
                uploadHelpPanel.Open(FirstUploadAlert);
            }
            else {
                FirstUploadAlert();
            }
        }
    }


    private void FirstUploadAlert() {
        if (!levelButton.level.flags.verified) {
            // do unverified warning
            Managers.AlertManager.DoAlert(
                "this level is unverified. this means that other people won't be able to tell that your level is beatable if they find it. you can still upload like this, but if you intend for players to clear the level, you should first verify it by completing it without using the undo button.",
                true,
                ("don't upload", null),
                ("verify it", () => { levelSelect.LoadLevel(levelButton, false); }), // my syntax highlighting is broken for this line :(
                ("upload it anyway", SecondUploadAlert)
            );
        } else {
            SecondUploadAlert();
        }
    }

    private void SecondUploadAlert() {
        Managers.AlertManager.DoAreYouSureAlert(
            "are you sure you want to upload? you cannot edit or delete the uploaded level after this is done.", true,
            () => levelSelect.UploadLevel(levelButton)
        );
    }

    public void Copy() {
        copiedLevel = levelButton.level;
        Close();
    }

    public void Paste() {
        if (copiedLevel == null) {
            return;
        }

        Managers.AlertManager.DoAreYouSureAlert(
            "are you sure you want to overwrite this level? your clipboard contains '" + copiedLevel.levelName + "'.",
            !levelButton.IsEmpty,
            () => {
                if (copiedLevel.flags.uploaded) {
                    Managers.AlertManager.DoAlert(
                        "the copied level '" + copiedLevel.levelName + "' is marked as uploaded. do you want to unmark the pasted version?",
                        true,
                        ("keep uploaded", () => levelSelect.Paste(levelButton, copiedLevel)),
                        ("paste as new", () => levelSelect.PasteAsNew(levelButton, copiedLevel))
                    );
                } else {
                    levelSelect.Paste(levelButton, copiedLevel);
                }
            }
        );

        Close();
    }

    public void Delete() {
        // oh no
        Managers.AlertManager.DoAreYouSureAlert(
            "are you sure you want to delete this level?", true,
            () => {
                levelSelect.DeleteLevel(levelButton);
                CloseImmediate();
            }
        );
    }

    public void Back() {
        Close();
    }


    [ContextMenu("what's my anchor position")]
    public void PrintAnchorPos() {
        Debug.Log((transform as RectTransform).anchoredPosition);
    }

    [ContextMenu("what's my world position")]
    public void PrintWorldPos() {
        Debug.Log(transform.position);
    }


    [ContextMenu("Reset Viewed Upload Help")]
    public void ResetViewedUploadHelp() {
        PlayerPrefs.SetInt(UPLOAD_HELP_PLAYER_PREF, 0);
    }
}
