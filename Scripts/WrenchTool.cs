using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WrenchTool : EditorTool
{
    protected override void HandleInput(Vector2 clickPoint, int buttonId) {
        if (buttonId == 0) {
            HandleBlockChange(clickPoint);
        }
    }

    private void HandleBlockChange(Vector2 clickPoint) {
        FlipSwitchWalls(clickPoint);
        FillSwitchers(clickPoint);
        RotateOneWays(clickPoint);
    }


    private void FlipSwitchWalls(Vector2 clickPoint) {
        bool anythingFlipped = false;

        foreach (SwitchWall switchWall in levelTiles.GetTilesAt<SwitchWall>(clickPoint)) {
            switchWall.Switch();
            anythingFlipped = true;
        }

        if (anythingFlipped) {
            // play a sound or something
        }
    }

    private void FillSwitchers(Vector2 clickPoint) {
        bool anythingFilled = false;

        foreach (Switcher switcher in levelTiles.GetTilesAt<Switcher>(clickPoint)) {
            switcher.SetFilled(!switcher.isFilled);
            anythingFilled = true;
        }

        if (anythingFilled) {
            // play a sound or something
        }
    }

    private void RotateOneWays(Vector2 clickPoint) {
        bool anythingRotated = false;

        foreach (OneWayTile oneWay in levelTiles.GetTilesAt<OneWayTile>(clickPoint)) {
            oneWay.Rotate();
            anythingRotated = true;
        }

        if (anythingRotated) {
            // play a sound or something
        }
    }
}
