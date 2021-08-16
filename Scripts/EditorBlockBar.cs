using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EditorBlockBar : EditorComponent
{
    private List<EditorBlockButtonArea> blockButtonAreas;
    public DrawTool drawTool;

    public EditorBlockButton selectedBlockType;
    public UIAnimator animator;

    public Transform buttonAreaParent;

    public bool IsHovering => blockButtonAreas.Any(area => area.IsHovering);


    private void Start() {
        int hotkey = 0;
        blockButtonAreas = new List<EditorBlockButtonArea>();

        foreach (Transform child in buttonAreaParent) {
            EditorBlockButtonArea buttonArea = child.GetComponent<EditorBlockButtonArea>();
            if (buttonArea != null) {
                hotkey++;
                buttonArea.hotkeyStr = "" + hotkey;
            
                blockButtonAreas.Add(buttonArea);
            }
        }
    }
    
    public void OnDrawToolSelected() {
        animator.Enter();
    }

    public void OnDrawToolDeselected() {
        animator.Exit();
    }
}
