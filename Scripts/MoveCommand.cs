using System.Collections.Generic;
using UnityEngine;

public class MoveCommand : EditCommand
{

    private TileInfoData oldTile;
    private TileInfoData newTile;

    private List<TileInfoData> deletions;

    public MoveCommand(TileInfoData move, Vector2Int position, LevelTiles level) : base(level) {
        oldTile = move;
        newTile = new TileInfoData(oldTile.i, oldTile.a, position);

        deletions = new List<TileInfoData>();
    }

    public override void Execute() {
        base.Execute();
        
        MoveNew(oldTile, newTile.position);
    }

    public override void Undo() {
        base.Undo();

        levelTiles.MoveStaticTile(levelTiles.GetTile(newTile), levelTiles.GridToTilePosition(oldTile.position));

        foreach (TileInfoData deletion in deletions) {
            levelTiles.CreateTile(deletion);
        }
    }

    public override void Redo() {
        base.Redo();

        foreach (TileInfoData deletion in deletions) {
            levelTiles.DestroyTile(deletion);
        }

        levelTiles.MoveStaticTile(levelTiles.GetTile(oldTile), levelTiles.GridToTilePosition(newTile.position));
    }


    public void ChangeFinalPosition(Vector2Int newPosition) {
        TileInfoData newNewTile = new TileInfoData(newTile.i, newTile.a, newPosition);

        if (hasExecuted) {
            MoveNew(newTile, newPosition);
        }

        newTile = newNewTile;
    }

    private void MoveNew(TileInfoData tile, Vector2Int gridPosition) {
        List<TileInfoData> newDeletions = levelTiles.DestroyTilesAt(levelTiles.GridToTilePosition(gridPosition));

        if (newDeletions.Count > 1) {
            Debug.LogError("When moving a tile to (" + gridPosition.x + ", " + gridPosition.y + "), the number of tiles that were already there was " + newDeletions.Count + ". This should never happen.");
        }

        deletions.AddRange(newDeletions);
        
        TileItem tileItem = levelTiles.GetTile(tile);
        levelTiles.MoveStaticTile(tileItem, levelTiles.GridToTilePosition(gridPosition));
        tileItem.spriteContainer.Pop();
    }
}
