using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EditorToolBar : EditorComponent
{
    public UIAnimator animator;
    [SerializeField] private EditorTool currentTool;
    

    public override void Activate() {
        currentTool?.SelectTool();
        animator.Enter();
    }

    public override void Deactivate() {
        currentTool?.DeselectTool();
        animator.Exit();
    }


    public void SelectTool(EditorTool tool) {
        if (tool == currentTool) {
            return;
        }
        
        currentTool?.DeselectTool();
        tool.SelectTool();

        currentTool = tool;
    }
}
