using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EditorTool : EditorComponent
{
    protected Camera cam { get { return editorManager.cam; }}

    public bool isSelected { get; private set; }
    public bool isEditing { get; private set; }

    protected Vector2 previousMousePoint;
    protected int currentButtonDown;
    protected bool isShifting;

    public virtual void SelectTool() {
        isSelected = true;
    }

    public virtual void DeselectTool() {
        isSelected = false;
    }


    public virtual void Update() {
        if (!editorManager.canEdit || !isSelected) {
            return;
        }

        Vector2 mousePoint = cam.ScreenToWorldPoint(Input.mousePosition).ToTilePosition();

        if (currentButtonDown >= 0 && !Input.GetMouseButton(currentButtonDown)) {
            // the mouse button has been lifted
            HandleInputUp(currentButtonDown);
            
            currentButtonDown = -1;
            isShifting = false;
        }

        for (int i = 0; i <= 1; i++) {
            if (Input.GetMouseButtonDown(i)) {
                // this is a new mouse input
                currentButtonDown = i;
                previousMousePoint = mousePoint;
                isShifting = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

                HandleInput(mousePoint, i);
            }

            if (currentButtonDown == i && mousePoint != previousMousePoint) {
                // the mouse has been dragged
                HandleInput(mousePoint, i);
            }
        }

        previousMousePoint = mousePoint;
    }

    protected virtual void HandleInput(Vector2 clickPoint, int buttonId) {
        // to be implemented
    }

    protected virtual void HandleInputUp(int buttonId) {
        // to be implemented
    }
}
