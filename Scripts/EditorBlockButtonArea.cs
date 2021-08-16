using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class EditorBlockButtonArea : MonoBehaviour
{
    public EditorBlockBar blockBar;
    [HideInInspector] public string hotkeyStr;

    public bool IsHovering { get; private set; }


    [Header("Component Links")]
    public RectTransform rectTransform;
    public RectTransform hoverRegion;
    public List<EditorBlockButton> blockButtons;
    public EditorBlockExtendBar extendBar;
    public Canvas canvas;


    [Header("Animation")]
    public float hoverRegionExtendedWidth;
    private float hoverRegionUnextendedWidth;


    private void Start() {
        hoverRegionUnextendedWidth = hoverRegion.sizeDelta.x;

        for (int b = 0; b < blockButtons.Count; b++) {
            blockButtons[b].buttonNum = b;
        }
    }


    private void Update() {
        // check for hover
        if (blockBar.editorManager.canEdit &&
            RectTransformUtility.RectangleContainsScreenPoint(hoverRegion, Input.mousePosition, canvas.worldCamera)
        ) {
            if (!IsHovering) {
                // mouse just now moved into the rectangle
                OnHover();
            }
            
            IsHovering = true;
        } else {
            if (IsHovering) {
                // mouse just now moved off the rectangle
                OnUnhover();
            }

            IsHovering = false;
        }

        // check for hotkey
        if (blockBar.editorManager.canEdit && hotkeyStr != null && hotkeyStr.Length > 0 && Input.GetKeyDown(hotkeyStr)) {
            if (blockButtons[0] == blockBar.selectedBlockType) {
                // need to cycle through the extend bar list
                int num = (blockButtons[0].buttonNum + 1) % blockButtons.Count;
                SwitchBlock(blockButtons.Find(button => button.buttonNum == num));
            }
            else {
                SwitchBlock(blockButtons[0]);
            }
        }
    }


    public void SwitchBlock(EditorBlockButton blockButton) {
        if (!blockButton.isOnMainBar) {
            MoveToMainBar(blockButton);

            // tell the main button image to pop
            blockButtons[0].ScaleImageOnSelected(true);

            // close the extend bar menu
            OnUnhover();
            IsHovering = false;
        } else {
            // tell the main button image to scale normally
            blockButtons[0].ScaleImageOnSelected(false);
        }

        blockBar.selectedBlockType = blockButtons[0];
    }

    private void MoveToMainBar(EditorBlockButton newMainButton) {
        EditorBlockButton currentMainButton = blockButtons[0];
        
        // we're not actually going to swap the positions of the buttons. we're just going to swap the data. we will swap:

        // block data
        int id = currentMainButton.id;
        byte alt = currentMainButton.alt;
        currentMainButton.id = newMainButton.id;
        currentMainButton.alt = newMainButton.alt;
        newMainButton.id = id;
        newMainButton.alt = alt;

        // image sprite
        Sprite sprite = currentMainButton.blockImage.sprite;
        currentMainButton.blockImage.sprite = newMainButton.blockImage.sprite;
        newMainButton.blockImage.sprite = sprite;

        // image color name
        string colorName = currentMainButton.blockColorer.colorName;
        currentMainButton.blockColorer.colorName = newMainButton.blockColorer.colorName;
        newMainButton.blockColorer.colorName = colorName;

        // image scale
        Vector3 scale = currentMainButton.blockImage.rectTransform.localScale;
        currentMainButton.blockImage.rectTransform.localScale = newMainButton.blockImage.rectTransform.localScale;
        newMainButton.blockImage.rectTransform.localScale = scale;

        // block button nums
        int num = currentMainButton.buttonNum;
        currentMainButton.buttonNum = newMainButton.buttonNum;
        newMainButton.buttonNum = num;
    }


    public void OnHover() {
        if (extendBar != null) {
            extendBar.MoveOnScreen();
            hoverRegion.sizeDelta = new Vector2(hoverRegionExtendedWidth, hoverRegion.sizeDelta.y);
        }
    }

    public void OnUnhover() {
        if (extendBar != null) {
            extendBar.MoveOffScreen();
            hoverRegion.sizeDelta = new Vector2(hoverRegionUnextendedWidth, hoverRegion.sizeDelta.y);
        }
    }

}
