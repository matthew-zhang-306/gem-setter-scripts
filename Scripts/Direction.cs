using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Direction
{
    NONE = 0,

    UP    = 0b1000,
    DOWN  = 0b0100,
    RIGHT = 0b0010,
    LEFT  = 0b0001,
    
    UP_RIGHT   = UP | RIGHT,
    DOWN_RIGHT = DOWN | RIGHT,
    UP_LEFT    = UP | LEFT,
    DOWN_LEFT  = DOWN | LEFT,
}

public static class DirectionExt {

    public static Direction[] cardinals = new Direction[] { Direction.LEFT, Direction.RIGHT, Direction.DOWN, Direction.UP };

    public static Vector2 ToVector2(this Direction dir) {
        Vector2 vector = Vector2.zero;
        
        if ((dir & Direction.UP) > 0) {
            vector += Vector2.up;
        }
        if ((dir & Direction.DOWN) > 0) {
            vector += Vector2.down;
        }
        if ((dir & Direction.RIGHT) > 0) {
            vector += Vector2.right;
        }
        if ((dir & Direction.LEFT) > 0) {
            vector += Vector2.left;
        }

        return vector;
    }

    public static Vector3 ToVector3(this Direction dir) {
        return dir.ToVector2().ToVector3();
    }


    public static bool IsHorizontal(this Direction dir) {
        return dir.GetHorizontal() > 0 && dir.GetVertical() == 0;
    }
    public static bool IsVertical(this Direction dir) {
        return dir.GetHorizontal() == 0 && dir.GetVertical() > 0;
    }
    public static bool IsDiagonal(this Direction dir) {
        return dir.GetHorizontal() > 0 && dir.GetVertical() > 0;
    }


    public static Direction GetHorizontal(this Direction dir) {
        return (Direction)((int)dir & 0b0011);
    }
    public static Direction GetVertical(this Direction dir) {
        return (Direction)((int)dir & 0b1100);
    }
}
