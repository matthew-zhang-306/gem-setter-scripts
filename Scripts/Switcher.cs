using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Switcher : TileItem
{
    public int switchType;
    public bool isFilled;

    private List<SwitchWall> switchWalls;
    private bool isOverlapping;

    public TileAmbientParticles particles;

    [Header("Sprites")]
    public Sprite[] unfilledSprites;
    public Sprite[] filledSprites;

    public static EmptyDelegate OnHitSwitch;


    // alt code: represents switch type.
    public override byte GetAltCode() {
        return (byte)((isFilled ? 0x80 : 0x00) | (switchType & 0x7F));
    }

    public byte GetSwitchedAltCode() {
        return (byte)((isFilled ? 0x00 : 0x80) | (switchType & 0x7F));
    }

    public override bool SetAltCode(byte alt) {
        if ((alt & 0x7F) >= 4) {
            return false;
        }

        // extract switch type
        switchType = alt & 0x7F;

        // extract filled
        SetFilled((alt & 0x80) != 0);

        SetColor();

        return true;
    }

    public void SetFilled(bool isFilled) {
        this.isFilled = isFilled;
        sr.sprite = isFilled ? filledSprites[switchType] : unfilledSprites[switchType];
    }


    public override void StartPlay() {
        switchWalls = levelManager.GetTiles<SwitchWall>()
            .Where(swall => swall.switchType == switchType).ToList();

        isOverlapping = false;
    }

    public override void OnWeakOverlap(TileItem overlap, PlayerController player) {
        if (!isPlaying) {
            return;
        }
        
        if (!isOverlapping) {
            RecordState();
            isOverlapping = true;
            
            Switch();
            OnHitSwitch?.Invoke();
        }
    }

    public override void OnExitOverlap(PlayerController player) {
        if (!isOverlapping) {
            Debug.LogError("Switcher " + this + " was not marked as previously overlapping anything when OnExitOverlap was called.");
        }
        
        RecordState();
        isOverlapping = false;
    
        if (isFilled) { 
            Switch();
        }
    }

    public void Switch() {
        if (switchWalls == null) {
            Debug.LogError("Switch " + this + " currently has a null collection of switchwalls when it shouldn't");
        }

        foreach (SwitchWall switchWall in switchWalls) {
            switchWall.Switch(true);
        }
    }


    private void Update() {
        SetColor();
    }

    private void SetColor() {
        sr.color = levelTiles.GetColor("Switch " + (switchType + 1));
        particles.ambientParticles.SetColor(sr.color);
    }


    public override TileState GetTileState() {
        return new SwitcherTileState(tilePosition, isOverlapping);
    }

    public override void RevertState(TileState state) {
        if (state is SwitcherTileState tileState) {
            base.RevertState(state);

            // handle overlapping
            isOverlapping = tileState.overlapping;
        }
        else {
            Debug.LogError("Cannot revert Switcher using non-switcher state " + state + "!");
        }
    }


    public override void Reset(bool undoable = true) {
        if (undoable) {
            RecordState();
        }

        isOverlapping = false;
    }



    private class SwitcherTileState : TileState {
        public bool overlapping { get; private set; }

        public SwitcherTileState(Vector2 position, bool overlapping) : base(position) 
        {
            this.overlapping = overlapping;
        }
    }
}
