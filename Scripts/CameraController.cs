using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class CameraController : MonoBehaviour
{
    public Camera cam;

    public LevelTiles levelTiles;
    public Vector2 padding;

    public float aspectRatio;

    private bool controlDisabled;


    private void Start() {
        cam.orthographicSize = 0.5f;
    }

    private void OnEnable() {
        CreditsSequence.OnCameraMove += PanUp;
    }
    private void OnDisable() {
        CreditsSequence.OnCameraMove -= PanUp;
    }

    private void FixedUpdate() {
        if (controlDisabled) {
            return;
        }
        
        Vector3 desiredPosition = GetDesiredPosition();
        float desiredSize = GetDesiredSize();

        transform.position = Vector3.Lerp(transform.position, desiredPosition, 0.1f);
        cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, desiredSize, 0.1f);

        // set color
        cam.backgroundColor = levelTiles.GetColor("BG");
    }


    private Vector3 GetDesiredPosition() {
        return levelTiles.Rect.center.ToVector3(transform.position.z);
    }

    private float GetDesiredSize() {
        float sizeX = (float)levelTiles.Width / 2 / aspectRatio + padding.x;
        float sizeY = (float)levelTiles.Height / 2 + padding.y;

        return Mathf.Max(sizeX, sizeY);
    }


    private void PanUp() {
        controlDisabled = true;
        transform.DOMoveY(30, 1).SetEase(Ease.InOutCubic);        
    }
}
