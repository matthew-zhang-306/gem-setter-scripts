using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class WinTile : TileItem
{
    public GameObject winParticles;

    public SpriteRenderer bottomSr;
    public SpriteRenderer topSr;

    public float pulseTime;
    public float pulseAlpha;
    private float pulseTimer;

    public UnityEvent WinEvent;


    public override void StartPlay() {
        pulseTimer = 0;
    }

    public override void StopPlay() {
        base.StopPlay();

        bottomSr.color = bottomSr.color.WithAlpha(1);
        topSr.color = topSr.color.WithAlpha(1);
    }


    private void Update() {
        if (!isPlaying) {
            return;
        }

        pulseTimer += Time.deltaTime;
        if (pulseTimer > pulseTime * 2) {
            pulseTimer -= pulseTime * 2;
        }

        float lerpAmount = (Mathf.Cos(pulseTimer / pulseTime * Mathf.PI) + 1f) / 2;
        float alpha = Mathf.Lerp(1, pulseAlpha, lerpAmount);
        
        bottomSr.color = bottomSr.color.WithAlpha(alpha);
        topSr.color = topSr.color.WithAlpha(alpha);
    }


    public override void SetSortingLayer() {
        base.SetSortingLayer();

        bottomSr.sortingOrder = sr.sortingOrder + 1;
        topSr.sortingOrder = -(2 * Mathf.FloorToInt(transform.position.y - 0.5f) + 1) - 1;
    }


    public override void OnStrongOverlap(TileItem overlap, PlayerController player) {
        if (overlap == player) {
            player.Win();        
            OnWin();
        }
    }

    public void OnWin() {
        Instantiate(winParticles, transform.position, Quaternion.identity);
        WinEvent?.Invoke();
    }
}
