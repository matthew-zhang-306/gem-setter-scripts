using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileStateFrame
{
    public string description { get; private set; }
    public int moves { get; private set; }
    public HashSet<TileItem> overlaps { get; private set; } // this is extra storage for the level manager
    public Dictionary<TileItem, TileState> tileStates { get; private set; }

    public TileStateFrame(string desc, int moves, HashSet<TileItem> overlaps) {
        description = desc;
        
        this.moves = moves;
        this.overlaps = new HashSet<TileItem>(overlaps);
        tileStates = new Dictionary<TileItem, TileState>();
    }

    public bool AddState(TileItem item) {
        if (tileStates.ContainsKey(item)) {
            return false;
        }

        tileStates.Add(item, item.GetTileState());
        return true;
    }

    public void RevertStates() {
        if (tileStates.Count == 0) {
            return;
        }

        // to be totally safe, we should revert the player first before any other tile, so we'll look for it
        foreach (TileItem item in tileStates.Keys) {
            if (item is PlayerController player) {
                item.RevertState(tileStates[item]);
                break;
            }
        }

        // then, do the same thing but skip the player
        foreach (TileItem item in tileStates.Keys) {
            if (!(item is PlayerController _)) {
                item.RevertState(tileStates[item]);
            }
        }
    }
}
