using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using DG.Tweening;

public class AttachableBox : TileItem
{
    Transform defaultParent;

    Sprite unattachedSprite;
    public Sprite attachedSprite;

    public SpriteRenderer flashSprite;

    PlayerController attachedPlayer;

    public bool IsAttached { get { return attachedPlayer != null; }}
    public bool destroyed { get; private set; }

    private Vector2 initialPosition;

    public static EmptyDelegate OnDie;


    private void OnEnable() {
        PlayerController.OnStartMove += OnStartMove;
    }
    private void OnDisable() {
        PlayerController.OnStartMove -= OnStartMove;
    }


    protected override void Start() {
        base.Start();
        unattachedSprite = sr.sprite;
    }

    public override void StartPlay() {
        destroyed = false;
        defaultParent = transform.parent;
        initialPosition = transform.position.ToVector2();
    }

    public void AttachTo(PlayerController player) {
        if (IsAttached) {
            return;
        }

        RecordState();
        SetAttached(player);
    }

    public void Detach() {
        if (!IsAttached) {
            return;
        }

        RecordState();
        SetAttached(null);
    }

    private void SetAttached(PlayerController player) {
        bool oldIsAttached = IsAttached;
        attachedPlayer = player;
        
        if (IsAttached) {
            transform.parent = player.transform;
            gameObject.layer = LayerMask.NameToLayer("Attached");
            sr.sprite = attachedSprite;
            levelTiles.SetDynamic(this);
        }
        else {
            transform.parent = defaultParent;
            gameObject.layer = LayerMask.NameToLayer("Wall");
            sr.sprite = unattachedSprite;
            levelTiles.SetStatic(this);
        }

        if (IsAttached != oldIsAttached) {
            // newly attached: flash animation
            spriteContainer.Flash();
        }
    }

    public void RejectAttach() {
        spriteContainer.Pop();
    }


    public bool IsAdjacentToPlayer(PlayerController player) {
        foreach (TileItem item in player.AllPlayerTiles) {
            if ((item.transform.position - transform.position).sqrMagnitude < 1.1f) {
                return true;
            }
        }

        return false;
    }


    public override void SetSortingLayer() {
        base.SetSortingLayer();
        flashSprite.sortingOrder = GetSortingLayer() + 1;
    }


    private void OnStartMove(Vector2 dir) {
        if (IsAttached) {
            RecordState();
        }
    }


    public override void Die() {
        RecordState();

        attachedPlayer?.DetachBox(this);
        destroyed = true;

        base.Die();
        OnDie?.Invoke();
    }


    public override TileState GetTileState() {
        return new BoxTileState(tilePosition, attachedPlayer, destroyed);
    }

    public override void RevertState(TileState state) {
        if (state is BoxTileState tileState) {
            base.RevertState(state);

            // handle destroyed
            destroyed = tileState.destroyed;

            // handle attached
            SetAttached(tileState.attachedPlayer);
        }
        else {
            Debug.LogError("Cannot revert state of AttachableBox with non-box state " + state + "!");
        }
    }


    public override void Reset(bool undoable = true) {
        if (undoable) {
            RecordState();
        }

        SetAttached(null);
        destroyed = false;
        levelTiles.MoveStaticTile(this, initialPosition);
    }



    private class BoxTileState : TileState {
        public PlayerController attachedPlayer { get; private set; }
        public bool destroyed { get; private set; }

        public BoxTileState(Vector2 position, PlayerController attachedPlayer, bool destroyed) : base(position) 
        {
            this.attachedPlayer = attachedPlayer;
            this.destroyed = destroyed;
        }
    }

}
