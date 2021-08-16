using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LevelSlotButton : MonoBehaviour
{
    public TextMeshProUGUI nameText;
    public Button button;
    public Image innerPanel;
    public LevelFlagsDisplay flagsDisplay;

    [Header("Colors")]
    public ColorData normalColorData;
    public Color invalidColor;
    public Color defaultColor;
    public float normalInnerAlpha;

    public int number { get; private set; }
    public string fileName { get { return "level" + number; }}
    public LevelInfo level { get; private set; }

    public enum LevelSlotType {
        DEFAULT, NORMAL, EMPTY, INVALID
    }
    private LevelSlotType slotType;
    public bool IsEmpty { get { return slotType == LevelSlotType.EMPTY || slotType == LevelSlotType.INVALID; }}
    public bool IsValid { get { return slotType != LevelSlotType.INVALID; }}
    public bool IsDefault { get { return slotType == LevelSlotType.DEFAULT; }}

    [HideInInspector] public LevelSlots slots;

    public void Init(int number, LevelSlots slots, bool isDefault = false) {
        this.number = number;
        this.slots = slots;

        FetchFileData(isDefault);

        // set interactable
        if (IsDefault) {
            button.interactable = slots.canSelectDefault;
        }
        else if (IsEmpty) {
            button.interactable = slots.canSelectEmpty;
        }
        else {
            button.interactable = true;
        }

        // handle flags
        if (slotType == LevelSlotType.NORMAL) {
            flagsDisplay.gameObject.SetActive(true);
            flagsDisplay.SetFlags(level.flags);
        }
        else {
            flagsDisplay.gameObject.SetActive(false);
        }

    }


    public void FetchFileData(bool isDefault) {
        if (isDefault) {
            slotType = LevelSlotType.DEFAULT;
            SetDefaultAppearance();
            return;
        }

        if (Managers.FileManager.LevelExists(fileName)) {
            try {
                level = Managers.FileManager.LoadLevelInfo(fileName);
                
                // initialize button for NORMAL
                slotType = LevelSlotType.NORMAL;
                nameText.text = level.levelName;
                
                SetNormalAppearance(level);
            }
            catch (System.Exception e) {
                Debug.LogWarning("Invalid file " + fileName + ": " + e.Message);

                // initialize button for INVALID
                slotType = LevelSlotType.INVALID;
                nameText.text = "~ error ~";

                SetInvalidAppearance();
            }
        }
        else {
            // initialize button for EMPTY
            slotType = LevelSlotType.EMPTY;
            nameText.text = "empty";

            SetEmptyAppearance();
        }
    }


    public void SetDefaultAppearance() {
        nameText.color = Color.black;
        button.image.color = Color.white;
        innerPanel.color = defaultColor;
    }

    public void SetNormalAppearance(LevelInfo levelInfo) {
        nameText.color = Color.white;
        normalColorData.hue = levelInfo.colorHues[2];
        button.image.color = normalColorData.Color;
        innerPanel.color = Color.black.WithAlpha(normalInnerAlpha);
    }

    public void SetEmptyAppearance() {
        nameText.color = Color.black;
        button.image.color = Color.white;
        innerPanel.color = Color.clear;
    }

    public void SetInvalidAppearance() {
        nameText.color = Color.white;
        button.image.color = invalidColor;
        innerPanel.color = Color.clear;
    }


    public void OnPressed() {
        slots.OnSlotSelected?.Invoke(this);
    }
}