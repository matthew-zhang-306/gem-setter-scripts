using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class EditCommand {

    protected LevelTiles levelTiles;

    public EditCommand(LevelTiles level) {
        levelTiles = level;
    }

    public bool hasExecuted { get; protected set; }

    public virtual void Execute() { hasExecuted = true; }
    public virtual void Undo() { hasExecuted = false; }
    public virtual void Redo() { hasExecuted = true; }

}