using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wall : TileItem
{
    public byte alt;

    public override byte GetAltCode() {
        return alt;
    }

    public override bool SetAltCode(byte alt) {
        if (alt < 0 || alt >= 2) {
            return false;
        }

        SetColor();

        this.alt = alt;
        return true;
    }


    private void Update() {
        SetColor();
    }

    private void SetColor() {
        // do color
        sr.color = levelTiles.GetColor("Wall " + (alt + 1));
    }
}
