using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveParticles : MonoBehaviour
{
    private bool active { get {
        if (tile is PlayerController _)
            return true;
        if (tile is AttachableBox box)
            return box.IsAttached;
        
        return false;
    }}

    public TileItem tile;


    // persistent particles
    public ParticleSystem moveParticles;

    // one shots
    public GameObject startDustParticlesObj;

    
    private void OnEnable() {
        PlayerController.OnStartMove += OnStartMove;
        PlayerController.OnEndMove += OnEndMove;
    }
    private void OnDisable() {
        PlayerController.OnStartMove -= OnStartMove;
        PlayerController.OnEndMove -= OnEndMove;
    }


    private void Update() {
        if (!active)
            return;

        // appear directly behind the tile
        moveParticles.GetComponent<Renderer>().sortingOrder = tile.GetSortingLayer() - 1;
    }


    private void OnStartMove(Vector2 dir) {
        if (!active)
            return;

        Quaternion rot = Quaternion.FromToRotation(Vector2.right, dir);

        GameObject startMove = Instantiate(startDustParticlesObj, transform.position, rot);
        startMove.GetComponent<ParticleSystem>().GetComponent<Renderer>().sortingOrder = tile.GetSortingLayer() - 1;

        moveParticles.transform.rotation = rot;
        moveParticles.Play();
    }

    private void OnEndMove(Vector2 dir) {
        if (moveParticles.isPlaying) {
            moveParticles.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }
    }
}
