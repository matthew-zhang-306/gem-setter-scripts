using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EditorSlotPicker : EditorComponent
{
    private int pickMode; // 0 for save, 1 for load

    public MenuButton hotbarButton1;
    public MenuButton hotbarButton2;

    public LevelSlots levelSlots;

    public UIAnimator panelAnimator;
    public UIAnimator backAnimator;

    public EditorControlMenu controlMenu;

    private void Open() {
        backAnimator.Enter();
        panelAnimator.Enter();
    }

    public void Close() {
        backAnimator.Exit();
        panelAnimator.Exit();
    }


    public void OpenForSave() {
        pickMode = 0;

        levelSlots.canSelectEmpty = true;
        levelSlots.Reset();

        hotbarButton1.SetText("current slot: " + editorManager.currentSlot);
        hotbarButton2.SetText("first empty slot: " + levelSlots.firstEmpty?.number);
    
        Open();
    }

    public void OpenForLoad() {
        pickMode = 1;

        levelSlots.canSelectEmpty = false;
        levelSlots.Reset();

        hotbarButton1.SetText("current slot: " + editorManager.currentSlot);
        hotbarButton2.SetText("last used slot: " + editorManager.currentSlot);
    
        Open();
    }


    public void OnHotbarSelect(int number) {
        if (number == 0) {
            // current slot button
            OnSlotSelected(levelSlots.GetLevelButton(editorManager.currentSlot));
        }
        else if (number == 1) {
            if (pickMode == 0) {
                // saving wants first empty
                OnSlotSelected(levelSlots.firstEmpty);
            }
            else if (pickMode == 1) {
                // loading wants last used
                OnSlotSelected(levelSlots.GetLevelButton(editorManager.currentSlot));
            }
        }
    }


    public void OnSlotSelected(LevelSlotButton button) {
        if (pickMode == 0) {
            // saving.
            Managers.AlertManager.DoAreYouSureAlert(
                "are you sure you want to overwrite this level?", !button.IsEmpty,
                () => Save(button)
            );
        } else {
            // loading.
            editorManager.DoUnsavedChangesAlert(() => {
                Managers.AlertManager.DoAreYouSureAlert(
                    "if you edit this level, it will be unverified. are you sure?", button.level.flags.verified,
                    () => Load(button)
                );
            });
        }
    }

    public void Save(LevelSlotButton button) {
        editorManager.Save(button.fileName);
        Close();
    }

    public void Load(LevelSlotButton button) {
        editorManager.Load(button.fileName);
        Close();
    }


    public void Back() {
        Close();
    }
}
