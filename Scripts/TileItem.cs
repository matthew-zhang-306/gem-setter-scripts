using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileItem : MonoBehaviour
{
    public int id;

    public Vector2 tilePosition { get {
        return transform.position.ToTilePosition();
    }}

    public virtual byte GetAltCode() {
        // by default, the alt code is always 0
        return 0;
    }

    // returns whether or not the alt code passed in was valid.
    // if valid, the method should ensure that GetAltCode() will return alt after this method is called.
    public virtual bool SetAltCode(byte alt) {
        // by default, the alt code is always 0
        return alt == 0;
    }


    [HideInInspector] public LevelTiles levelTiles;
    [HideInInspector] public LevelManager levelManager;
    public bool isPlaying { get { return levelManager != null; }}

    protected BoxCollider2D coll;
    public BoxCollider2D Collider { get { return coll; }}

    public SpriteContainer spriteContainer;
    public SpriteRenderer sr;
    

    protected virtual void Start() {
        if (sr == null) {
            sr = GetComponent<SpriteRenderer>();
        }

        if (coll == null) {
            coll = GetComponent<BoxCollider2D>();
        }

        SetSortingLayer();
    }


    public virtual void StartPlay() {
        // by default, do nothing
    }


    public virtual void OnClick(int mouseButton) {
        if (mouseButton == 0) {
            spriteContainer.Pop();
        }
    }

    public virtual void OnDrag(int mouseButton) {
        // by default, do nothing
    }


    public virtual void OnWeakOverlap(TileItem overlap, PlayerController player) {
        // by default, do nothing
    }

    public virtual void OnStrongOverlap(TileItem overlap, PlayerController player) {
        // by default, do nothing
    }

    public virtual void OnExitOverlap(PlayerController player) {
        // by default, do nothing
    }

    public virtual TileState GetTileState() {
        return new TileState(tilePosition);
    }

    protected virtual void RecordState() {
        levelManager.RecordState(this);
    }

    public virtual void RevertState(TileState state) {
        // by default, reset the position of the tile item
        if (levelTiles.IsDynamic(this)) {
            transform.position = state.position;
            SetSortingLayer();
        } else {
            levelTiles.MoveStaticTile(this, state.position);
        }
    }

    public virtual void Die() {
        // by default, remove from play
        levelTiles.RemoveFromPlay(this);
    }

    public virtual void Reset(bool undoable = true) {
        // by default, do nothing
    }

    public virtual void StopPlay() {
        // by default, reset and disconnect from the level manager
        Reset(false);
        levelManager = null;
    }


    public virtual void SetSortingLayer() {
        if (sr == null) {
            sr = GetComponent<SpriteRenderer>();
        }

        sr.sortingOrder = -(2 * Mathf.FloorToInt(transform.position.y - 0.5f) + 1);
    }

    public virtual int GetSortingLayer() {
        return sr.sortingOrder;
    }


    public TileInfoData GetData() {
        TileInfoData data = new TileInfoData();

        data.i = (byte)this.id;
        data.a = this.GetAltCode();
        data.x = (byte)levelTiles.GetGridPosition(this).x;
        data.y = (byte)levelTiles.GetGridPosition(this).y;
        
        return data;
    }
}
