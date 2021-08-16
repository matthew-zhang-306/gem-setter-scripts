using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnapToGrid : MonoBehaviour
{
    public SpriteRenderer sr;
    public BoxCollider2D coll;

    public void Snap() {
        if (sr == null || coll == null) {
            sr = GetComponent<SpriteRenderer>();
            coll = GetComponent<BoxCollider2D>();

            if (sr == null && coll == null) {
                Debug.LogError("SnapToGrid requires either a SpriteRenderer or a BoxCollider2D to operate on!");
                return;
            }
        }

        Bounds bounds;
        if (sr != null) {
            bounds = sr.bounds;
        } else {
            // if the collider is disabled, we have to enable it or it will give us no bounds!
            bool oldState = coll.enabled;
            coll.enabled = true;

            bounds = coll.bounds;

            coll.enabled = oldState;
        }

        Vector2 topRight = bounds.max.ToVector2();
        Vector2 bottomLeft = bounds.min.ToVector2();
        
        topRight = new Vector2(Mathf.Round(topRight.x), Mathf.Round(topRight.y));
        bottomLeft = new Vector2(Mathf.Round(bottomLeft.x), Mathf.Round(bottomLeft.y));

        transform.position = 0.5f * topRight + 0.5f * bottomLeft;

        if (sr != null && sr.drawMode == SpriteDrawMode.Tiled) {
            sr.size = topRight - bottomLeft;    
        } else {
            transform.localScale = (topRight - bottomLeft).ToVector3(1);
        }

        // it's highly likely that this object has a tileitem script on it.
        // if there is one, we need to set its sorting layer.
        TileItem tileItem = GetComponent<TileItem>();
        if (tileItem != null) {
            tileItem.SetSortingLayer();
        }
    }


}
