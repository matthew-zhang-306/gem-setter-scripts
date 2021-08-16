using System.Collections.Generic;
using UnityEngine;

public class PlaceCommand : EditCommand {

    private List<TileInfoData> additions;
    private List<TileInfoData> deletions;

    public PlaceCommand(TileInfoData addition, LevelTiles level) : base(level) {
        additions = new List<TileInfoData>();
        deletions = new List<TileInfoData>();

        additions.Add(addition);
    }

    public override void Execute() {
        base.Execute();
    
        foreach (TileInfoData addition in additions) {
            PlaceNew(addition);
        }
    }

    public override void Undo() {
        base.Undo();

        foreach (TileInfoData addition in additions) {
            levelTiles.DestroyTile(addition);
        }
        foreach (TileInfoData deletion in deletions) {
            levelTiles.CreateTile(deletion);
        }
    }

    public override void Redo() {
        base.Redo();
        
        foreach (TileInfoData deletion in deletions) {
            levelTiles.DestroyTile(deletion);
        }
        foreach (TileInfoData addition in additions) {
            levelTiles.CreateTile(addition);
        }
    }


    public void AddTile(TileInfoData newAddition) {
        additions.Add(newAddition);

        if (hasExecuted) {
            PlaceNew(newAddition);
        }
    }

    private void PlaceNew(TileInfoData addition) {
        List<TileInfoData> newDeletions = levelTiles.DestroyTilesAt(levelTiles.GridToTilePosition(addition.x, addition.y));
        
        TileItem tile = levelTiles.CreateTile(addition);
        tile.spriteContainer.Pop();

        if (newDeletions.Count > 1) {
            Debug.LogError("When initially placing a tile at (" + addition.x + ", " + addition.y + "), there were " + newDeletions.Count + " tiles occupying the same spot. This should never happen.");
        }

        deletions.AddRange(newDeletions);
    }
}