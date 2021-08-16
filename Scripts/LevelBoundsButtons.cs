using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelBoundsButtons : EditorComponent
{
    public Direction direction;
    
    public Button plusButton;
    public Button minusButton;

    public UIAnimator tabAnimator;
    public Image tabImage;

    public float yRaise;

    public static EmptyDelegate OnResize;

    private void Update() {
        // place at the right spot
        Vector2 desiredEdge = Vector2.zero;
        switch (direction) {
            case Direction.LEFT:
                desiredEdge = new Vector2(levelTiles.Rect.min.x, levelTiles.Rect.center.y); break;
            case Direction.RIGHT:
                desiredEdge = new Vector2(levelTiles.Rect.max.x, levelTiles.Rect.center.y); break;
            case Direction.DOWN:
                desiredEdge = new Vector2(levelTiles.Rect.center.x, levelTiles.Rect.min.y); break;
            case Direction.UP:
                desiredEdge = new Vector2(levelTiles.Rect.center.x, levelTiles.Rect.max.y); break;
        }

        transform.position = desiredEdge + direction.ToVector2() + yRaise * Vector2.up;


        // size constraints
        if (direction.IsHorizontal()) {
            plusButton.interactable = levelTiles.Width < levelTiles.MaxWidth;
            minusButton.interactable = levelTiles.Width > levelTiles.MinWidth;
        }
        else if (direction.IsVertical()) {
            plusButton.interactable = levelTiles.Height < levelTiles.MaxHeight;
            minusButton.interactable = levelTiles.Height > levelTiles.MinHeight;
        }
        else {
            plusButton.interactable = false;
            minusButton.interactable = false;
        }


        // color
        tabImage.color = levelTiles.GetColor("Wall 1");
    }

    public void PressedPlus() {
        editorManager.AddCommand(new SizeCommand(direction, true, levelTiles));
        OnResize?.Invoke();
    }

    public void PressedMinus() {
        editorManager.AddCommand(new SizeCommand(direction, false, levelTiles));
        OnResize?.Invoke();
    }


    public override void Activate() {
        tabAnimator.Enter();
    }

    public override void Deactivate() {
        tabAnimator.Exit();
    }
}
