using System.Collections.Generic;
using UnityEngine;

public class SizeCommand : EditCommand
{
    private Direction direction;
    private bool expand;

    private List<TileInfoData> deletions;

    public SizeCommand(Direction direction, bool expand, LevelTiles level) : base(level) {
        this.direction = direction;
        this.expand = expand;

        deletions = new List<TileInfoData>();
    }

    public override void Execute() {
        base.Execute();
        
        deletions = levelTiles.Resize(direction, expand);
        Debug.Log("Execute SizeCommand, deletions count " + deletions.Count);
    }

    public override void Undo() {
        base.Undo();

        levelTiles.Resize(direction, !expand);
        foreach (TileInfoData deletion in deletions) {
            Debug.Log("Undo SizeCommand, respawning at " + deletion.position);
            levelTiles.CreateTile(deletion);
        }
    }

    public override void Redo() {
        base.Redo();
        
        levelTiles.Resize(direction, expand);
    }
}
