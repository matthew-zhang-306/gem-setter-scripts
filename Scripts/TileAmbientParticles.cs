using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileAmbientParticles : MonoBehaviour
{
    public TileItem tile;

    public ParticleSystem ambientParticles;

    private bool oldActive;
    private bool active;

    private void Update() {
        bool active = true;
        if (tile is AttachableBox box) {
            active = box.IsAttached;
        }

        // set particle system to active or inactive
        if (active && !ambientParticles.isPlaying) {
            ambientParticles.Play();
        }
        else if (!active && ambientParticles.isPlaying) {
            ambientParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        // appear directly in front of the tile
        ambientParticles.GetComponent<Renderer>().sortingOrder = tile.GetSortingLayer() + 1;
    }

}
