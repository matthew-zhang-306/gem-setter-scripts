using System.Collections.Generic;
using UnityEngine;

public class EraseCommand : EditCommand
{
    private List<Vector2Int> gridPositions;
    private List<TileInfoData> deletions;

    public EraseCommand(Vector2Int gridPosition, LevelTiles level) : base(level) {
        gridPositions = new List<Vector2Int>();
        deletions = new List<TileInfoData>();

        gridPositions.Add(gridPosition);
    }

    public override void Execute() {
        base.Execute();
        
        foreach (Vector2Int gridPosition in gridPositions) {
            EraseNew(gridPosition);
        }
    }

    public override void Undo() {
        base.Undo();

        Debug.Log(deletions.Count);

        foreach (TileInfoData deletion in deletions) {
            levelTiles.CreateTile(deletion);
        }
    }

    public override void Redo() {
        base.Redo();
        
        foreach (TileInfoData deletion in deletions) {
            levelTiles.DestroyTile(deletion);
        }
    }

    public void AddPosition(Vector2Int gridPosition) {
        gridPositions.Add(gridPosition);

        if (hasExecuted) {
            EraseNew(gridPosition);
        }
    }

    private void EraseNew(Vector2Int gridPosition) {
        List<TileInfoData> newDeletions = levelTiles.DestroyTilesAt(levelTiles.GridToTilePosition(gridPosition));

        if (newDeletions.Count != 1) {
            Debug.LogError("The number of tiles to erase at (" + gridPosition.x + ", " + gridPosition.y + ") is not 1! It is " + newDeletions.Count);
        }

        deletions.AddRange(newDeletions);
    }
}
