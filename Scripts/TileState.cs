using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// a data class. used to store what a tile was like before something happened.
public class TileState
{
    public Vector2 position { get; private set; }
    

    public TileState(Vector2 position) {
        this.position = position;
    }


    public override string ToString() {
        return "[STATE at " + position + "]";
    }

}
