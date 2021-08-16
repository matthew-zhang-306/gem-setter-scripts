using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EditorFunctionBar : EditorComponent
{
    public UIAnimator animator;

    public Button undoButton;
    public Button redoButton;

    public EditorHelpPanel helpPanel;

    
    public override void Activate() {
        animator.Enter();
    }

    public override void Deactivate() {
        animator.Exit();
    }


    private void Update() {
        if (!editorManager.canEdit) {
            return;
        }
        
        undoButton.interactable = editorManager.canUndo;
        redoButton.interactable = editorManager.canRedo;
    
        // keyboard shortcuts
        if (editorManager.canUndo && Input.GetKeyDown(KeyCode.Z)) {
            Undo();
        }
        else if (editorManager.canRedo && Input.GetKeyDown(KeyCode.X)) {
            Redo();
        }
    }


    public void Undo() {
        editorManager.Undo();
    }

    public void Redo() {
        editorManager.Redo();
    }


    public void Help() {
        if (!editorManager.canEdit) {
            // not supposed to be able to press this button right now
            return;
        }

        helpPanel.Open();
    }
    
}
