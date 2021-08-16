using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class FireTile : TileItem
{
    public Sprite burntSprite;
    public Sprite activeSprite;
    public Sprite burntRowSprite;
    public Sprite activeRowSprite;

    public bool active { get; private set; }

    public GameObject burnParticles;

    public float pulseTime;
    public float pulseAlpha;
    private float pulseTimer;


    public override void StartPlay() {
        Reset(false);
        pulseTimer = 0;
    }

    public override void StopPlay() {
        base.StopPlay();

        foreach (SpriteRenderer sprite in spriteContainer.spriteRenderers) {
            sprite.color = sprite.color.WithAlpha(1);
        }
    }


    private void Update() {
        if (!isPlaying) {
            return;
        }

        pulseTimer += Time.deltaTime;
        if (pulseTimer > pulseTime * 2) {
            pulseTimer -= pulseTime * 2;
        }

        if (active) {
            for (int s = 0; s < spriteContainer.spriteRenderers.Count; s++) {
                float timer = pulseTimer / pulseTime + s * 2 * pulseTime / spriteContainer.spriteRenderers.Count;
                float lerpAmount = (Mathf.Cos(timer * Mathf.PI) + 1f) / 2;
                spriteContainer.spriteRenderers[s].color = spriteContainer.spriteRenderers[s].color.WithAlpha(Mathf.Lerp(pulseAlpha, 1, lerpAmount));
            }
        } else {
            foreach (SpriteRenderer sprite in spriteContainer.spriteRenderers) {
                sprite.color = sprite.color.WithAlpha(1);
            }
        }
    }


    public override void OnWeakOverlap(TileItem overlap, PlayerController player) {
        if (!isPlaying || !active) {
            return;
        }

        if (overlap == player) {
            // kill the player
            player.DieByFire();
        }
        else if (overlap is AttachableBox box) {
            // trade
            BurnOut();
            box.Die();
        }
    }


    public void BurnOut() {
        if (!active) {
            return;
        }

        RecordState();

        SetActive(false);
        Instantiate(burnParticles, transform.position, Quaternion.identity);
    }


    public override TileState GetTileState() {
        return new FireTileState(tilePosition, active);
    }

    public override void RevertState(TileState state) {
        if (state is FireTileState tileState) {
            base.RevertState(state);

            // handle active
            SetActive(tileState.active);
        }
        else {
            Debug.LogError("Cannot revert FireTile using non-fire state " + state + "!");
        }
    }

    public override void Reset(bool undoable = true) {
        if (undoable) {
            RecordState();
        }

        SetActive(true);
    }


    private void SetActive(bool isActive) {
        active = isActive;

        foreach (SpriteRenderer sprite in spriteContainer.spriteRenderers) {
            sprite.sprite = active ? activeRowSprite : burntRowSprite;
        }
    }



    private class FireTileState : TileState {
        public bool active { get; private set; }

        public FireTileState(Vector2 position, bool active) : base(position) 
        {
            this.active = active;
        }
    }
}
