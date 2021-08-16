using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class SwitchWall : TileItem
{
    public SpriteRenderer sideR;
    public SpriteMask sideMask;
    public Transform maskedSpriteContainer;

    public int switchType;

    [Header("Sprites")]
    public Sprite[] topSprites;
    public Sprite[] sideSprites;
    
    // alt code: first bit represents state. remainder represents switchType.
    public override byte GetAltCode() {
        return (byte)((activated ? 0x80 : 0x00) | (switchType & 0x7F));
    }

    public byte GetSwitchedAltCode() {
        return (byte)((activated ? 0x00 : 0x80) | (switchType & 0x7F));
    }

    public override bool SetAltCode(byte alt) {
        if ((alt & 0x7F) >= 4) {
            return false;
        }

        // extract switch type
        switchType = alt & 0x7F;

        // extract state
        bool desiredState = (alt & 0x80) != 0;
        if (activated != desiredState) {
            Switch();
        }

        SetLook();

        return true;
    }

    public bool activated { get {
        if (coll == null) {
            Start();
        }

        // we use the collider to determine what state the switch wall is in
        return coll.enabled;
    }}
    private bool initialState;


    [Header("Animation")]
    public float onYOffset;
    public float offYOffset;
    public float offAlpha;
    public TweenTiming timing;
    

    public override void StartPlay() {
        initialState = activated;
    }

    public void Switch(bool shouldRecordState = false, bool inEditor = false) {
        if (sr == null || coll == null) {
            Start();
        }

        if (shouldRecordState) {
            RecordState();
        }

        // this sets activated
        coll.enabled = !coll.enabled;

        DoAnimation(inEditor);
    }

    private void DoAnimation(bool inEditor) {
        float desiredY = activated ? onYOffset : offYOffset;
        float desiredAlpha = activated ? 1 : offAlpha;

        if (inEditor) {
            maskedSpriteContainer.localPosition = maskedSpriteContainer.localPosition.WithY(desiredY);

            sr.color = sr.color.WithAlpha(desiredAlpha);
            sideR.color = sideR.color.WithAlpha(desiredAlpha);
        } else {
            maskedSpriteContainer.DOLocalMoveY(desiredY, timing.easeTime).SetEase(timing.easingType);
        
            sr.DOFade(desiredAlpha, timing.easeTime).SetEase(timing.easingType);
            sideR.DOFade(desiredAlpha, timing.easeTime).SetEase(timing.easingType);
        }
    }

    public override void SetSortingLayer() {
        base.SetSortingLayer();

        sr.sortingOrder--;

        sideR.sortingOrder = sr.sortingOrder;

        sideMask.frontSortingOrder = sideR.sortingOrder;
        sideMask.backSortingOrder = sideR.sortingOrder - 1;
    }


    private void Update() {
        SetLook();
    }

    private void SetLook() {
        sr.sprite = topSprites[switchType];
        sideR.sprite = sideSprites[switchType];
        sideMask.sprite = sideSprites[switchType];

        Color c = levelTiles.GetColor("Switch " + (switchType + 1));
        sr.color = c.WithAlpha(sr.color.a);
        sideR.color = c.WithAlpha(sideR.color.a);
    }


    public override TileState GetTileState() {
        return new SWTileState(tilePosition, activated);
    }

    public override void RevertState(TileState state) {
        if (state is SWTileState tileState) {
            base.RevertState(state);

            // handle state
            if (activated != tileState.activated) {
                Switch();
            }
        }
        else {
            Debug.LogError("Cannot revert SwitchWall using non-switchwall state " + state + "!");
        }
    }

    public override void Reset(bool undoable = true) {
        if (activated != initialState) {
            // this will handle the state record for resetting   
            Switch(undoable);
        }
    }



    private class SWTileState : TileState {
        public bool activated { get; private set; }

        public SWTileState(Vector2 position, bool activated) : base(position) 
        {
            this.activated = activated;
        }
    }

}
