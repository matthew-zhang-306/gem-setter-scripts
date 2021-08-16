using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class OneWayTile : TileItem
{
    public static readonly Vector2[] DIRECTIONS = { Vector2.right, Vector2.up, Vector2.left, Vector2.down };

    public int directionIndex { get; private set; }
    public Vector2 direction { get { return DIRECTIONS[directionIndex]; }}


    [Header("Animation")]
    public float rotateTime;
    public Ease rotateEase;
    private Tween currentRotate;


    public override byte GetAltCode() {
        return (byte)directionIndex;
    }

    public byte GetNextAltCode() {
        return (byte)((directionIndex + 1) % 4);
    }

    public override bool SetAltCode(byte alt) {
        if (alt < 0 || alt >= 4) {
            return false;
        }

        RotateTo(alt);
        return true;
    }


    private void Awake() {
        directionIndex = GetDirectionIndexFromRotation();
    }


    public bool ShouldStopPlayer(PlayerController player) {
        if ((this.direction + player.currentDirection).sqrMagnitude < 0.01f) {
            // player's movement direction competes with one way gate direction. see if the player should be stopped
            return player.OccupiesTile(tilePosition + direction) && !player.OccupiesTile(tilePosition);
        }

        return false;
    }


    public void Rotate() {
        directionIndex = (directionIndex + 1) % 4;

        currentRotate?.Kill();
        currentRotate = spriteContainer.transform.DORotate(new Vector3(0, 0, directionIndex * 90), rotateTime).SetEase(rotateEase);
    }

    public void RotateTo(int dirIndex) {
        directionIndex = dirIndex % 4;

        currentRotate?.Kill();
        currentRotate = spriteContainer.transform.DORotate(new Vector3(0, 0, directionIndex * 90), rotateTime).SetEase(rotateEase);
    }

    public void RotateImmediate() {
        directionIndex = GetDirectionIndexFromRotation();
        directionIndex = (directionIndex + 1) % 4;
        spriteContainer.transform.rotation = Quaternion.Euler(0, 0, directionIndex * 90);
    }


    private int GetDirectionIndexFromRotation() {
        // perform circular mod that works for negative numbers, just in case
        float rot = spriteContainer.transform.rotation.eulerAngles.z % 360;
        if (rot < 0) {
            rot += 360;
        }

        return Mathf.FloorToInt(rot / 90);
    }

}
