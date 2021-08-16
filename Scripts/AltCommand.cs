using System.Collections.Generic;
using UnityEngine;

public class AltCommand : EditCommand
{

    Dictionary<Vector2Int, AltData> changes;


    public AltCommand(TileInfoData oldTile, byte newAlt, LevelTiles level) : base(level) {
        changes = new Dictionary<Vector2Int, AltData>();

        TileInfoData newTile = new TileInfoData(oldTile.i, newAlt, oldTile.position);
        changes.Add(oldTile.position, new AltData(oldTile, newTile));
    }

    public override void Execute() {
        base.Execute();
        
        foreach (Vector2Int gridPosition in changes.Keys) {
            AltNew(changes[gridPosition].oldData, gridPosition);
        }
    }

    public override void Undo() {
        base.Undo();

        foreach (Vector2Int gridPosition in changes.Keys) {
            levelTiles.GetTile(changes[gridPosition].newData).SetAltCode(changes[gridPosition].oldData.a);
        }
    }

    public override void Redo() {
        base.Redo();
        
         foreach (Vector2Int gridPosition in changes.Keys) {
            levelTiles.GetTile(changes[gridPosition].oldData).SetAltCode(changes[gridPosition].newData.a);
        }
    }


    public void AddTile(TileInfoData tile, byte newAlt) {
        if (changes.ContainsKey(tile.position)) {
            // clone this for the AltNew call
            TileInfoData oldNewData = changes[tile.position].newData;

            // then replace the alt code of the new version for this
            AltData newAltData = changes[tile.position];
            newAltData.newData.a = newAlt;
            changes[tile.position] = newAltData;

            if (hasExecuted) {
                AltNew(oldNewData, tile.position);
            }
        } else {
            TileInfoData newTile = new TileInfoData(tile.i, newAlt, tile.position);
            changes.Add(tile.position, new AltData(tile, newTile));
        
            if (hasExecuted) {
                AltNew(tile, tile.position);
            }
        }
    }

    private void AltNew(TileInfoData tile, Vector2Int gridPosition) {
        if (!changes.ContainsKey(gridPosition)) {
            Debug.LogError("Cannot AltNew with position " + gridPosition + ": no tiles at this position.");
            return;
        }

        levelTiles.GetTile(tile).SetAltCode(changes[gridPosition].newData.a);
    }


    public struct AltData {
        public TileInfoData oldData;
        public TileInfoData newData;

        public AltData(TileInfoData oldD, TileInfoData newD) {
            oldData = oldD;
            newData = newD;
        }
    }
}
