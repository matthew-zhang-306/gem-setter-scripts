using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DrawTool : EditorTool
{
    public EditorBlockBar blockBar;
    public TileItem player;

    private EditorBlockButton selectedBlockType { get { return blockBar.selectedBlockType; }}

    private EditCommand currentEditCommand;
    
    public delegate void DrawAction();
    public static EmptyDelegate BlockAdded;
    public static EmptyDelegate BlockAltered;
    public static EmptyDelegate BlockRemoved;


    private void OnEnable() {
        EditorManager.OnLoad += OnLoad;
    }
    private void OnDisable() {
        EditorManager.OnLoad -= OnLoad;
    }


    private void Start() {
        Reset();
        blockBar.drawTool = this;
    }

    private void OnLoad(string _) {
        Reset();
    }

    private void Reset() {
        HashSet<PlayerController> players = levelTiles.GetTiles<PlayerController>();

        if (players.Count > 0) {
            if (players.Count > 1) {
                Debug.LogWarning("This level contains more than one player! Only the first player will be considered.");
            }

            var playersEnum = players.GetEnumerator();
            playersEnum.MoveNext();
            player = playersEnum.Current;
        }
    }


    protected override void HandleInput(Vector2 clickPoint, int buttonId) {
        if (blockBar.IsHovering) {
            // ignore inputs when the block bar is being hovered over
            return;
        }

        switch (buttonId) {
            case 0:
                if (isShifting) {
                    // shift left click. change a block
                    HandleBlockChange(clickPoint);
                } else {
                    // left click. place a block
                    HandleBlockPlace(clickPoint);
                }
                break;
            case 1:
                // right click. delete a block
                HandleBlockDelete(clickPoint);
                break;
        }
    }

    private void HandleBlockPlace(Vector2 clickPoint) {
        if (selectedBlockType == null) {
            return;
        }

        if (!levelTiles.IsInBounds(clickPoint)) {
            return;
        }

        // check to make sure this position does not already have this block type on it
        foreach (TileItem tile in levelTiles.GetTilesAt(clickPoint)) {  
            if (tile.id == selectedBlockType.id && tile.GetAltCode() == selectedBlockType.alt) {
                return;
            }
        }

        editorManager.EditInProgress = true;

        if (!selectedBlockType.IsPlayer) {
            // place this tile like normal
            PlaceTile(clickPoint);
        } else {
            PlacePlayer(clickPoint);
        }

        BlockAdded?.Invoke();
    }

    private TileItem PlaceTile(Vector2 clickPoint) {
        TileInfoData tile = new TileInfoData(selectedBlockType.id, selectedBlockType.alt, levelTiles.TileToGridPosition(clickPoint));

        if (currentEditCommand == null) {
            currentEditCommand = new PlaceCommand(tile, levelTiles);
            editorManager.AddCommand(currentEditCommand);
        }
        else if (currentEditCommand is PlaceCommand currentPlaceCommand) {
            currentPlaceCommand.AddTile(tile);
        }
        else {
            Debug.LogError("DrawTool tried to place a tile when the current edit command " + currentEditCommand + " is not a PlaceCommand!");
            currentEditCommand = new PlaceCommand(tile, levelTiles);
            editorManager.AddCommand(currentEditCommand);
        }

        return levelTiles.GetTile(tile);
    }

    private void PlacePlayer(Vector2 clickPoint) {
        if (player == null) {
            // there is currently no player! time to make one
            player = PlaceTile(clickPoint);
            return;
        }

        TileInfoData playerData = player.GetData();
        Vector2Int gridPosition = levelTiles.TileToGridPosition(clickPoint);

        if (currentEditCommand == null || (currentEditCommand is PlaceCommand _)) {
            currentEditCommand = new MoveCommand(playerData, gridPosition, levelTiles);
            editorManager.AddCommand(currentEditCommand);
        }
        else if (currentEditCommand is MoveCommand currentPlaceCommand) {
            currentPlaceCommand.ChangeFinalPosition(gridPosition);
        }
        else {
            Debug.LogError("DrawTool tried to move the player when the current edit command " + currentEditCommand + " is not a MoveCommand or PlaceCommand!");
            currentEditCommand = new MoveCommand(playerData, gridPosition, levelTiles);
            editorManager.AddCommand(currentEditCommand);
        }
    }

    private void HandleBlockDelete(Vector2 clickPoint) {
        if (!levelTiles.IsInBounds(clickPoint)) {
            return;
        }

        if (levelTiles.GetTilesAt(clickPoint).Count > 0) {
            // tiles will be destroyed by this
            editorManager.EditInProgress = true;
            EraseTile(clickPoint);
            BlockRemoved?.Invoke();
        }
    }

    private void EraseTile(Vector2 clickPoint) {
        Vector2Int gridPosition = levelTiles.TileToGridPosition(clickPoint);

        if (currentEditCommand == null) {
            currentEditCommand = new EraseCommand(gridPosition, levelTiles);
            editorManager.AddCommand(currentEditCommand);
        }
        else if (currentEditCommand is EraseCommand currentEraseCommand) {
            currentEraseCommand.AddPosition(gridPosition);
        }
        else {
            Debug.LogError("DrawTool tried to erase a tile when the current edit command " + currentEditCommand + " is not an EraseCommand!");
            
            currentEditCommand = new EraseCommand(gridPosition, levelTiles);
            editorManager.AddCommand(currentEditCommand);
        }
    }

    private void HandleBlockChange(Vector2 clickPoint) {
        HashSet<TileItem> tiles = levelTiles.GetTilesAt(clickPoint);
        if (tiles.Count > 0) {
            TileItem tile = tiles.First();
            Func<byte> altProvider = null;

            if (tile is SwitchWall switchWall) {
                altProvider = switchWall.GetSwitchedAltCode;
            } else if (tile is Switcher switcher) {
                altProvider = switcher.GetSwitchedAltCode;
            } else if (tile is OneWayTile oneWay) {
                altProvider = oneWay.GetNextAltCode;
            }

            if (altProvider != null) {
                // a block can be changed here.
                ChangeTile(tile, altProvider.Invoke());
                BlockAltered?.Invoke();
            }
        }
    }

    private void ChangeTile(TileItem tile, byte newAlt) {
        if (currentEditCommand == null) {
            currentEditCommand = new AltCommand(tile.GetData(), newAlt, levelTiles);
            editorManager.AddCommand(currentEditCommand);
        }
        else if (currentEditCommand is AltCommand currentAltCommand) {
            currentAltCommand.AddTile(tile.GetData(), newAlt);
        }
        else {
            Debug.LogError("DrawTool tried to change a tile when the current edit command " + currentEditCommand + " is not an AltCommand!");
            
            currentEditCommand = new AltCommand(tile.GetData(), newAlt, levelTiles);
            editorManager.AddCommand(currentEditCommand);
        }
    }


    protected override void HandleInputUp(int buttonId) {
        currentEditCommand = null;
        editorManager.EditInProgress = false;
    }


    public override void SelectTool() {
        base.SelectTool();
        blockBar.OnDrawToolSelected();
    }

    public override void DeselectTool() {
        base.DeselectTool();
        blockBar.OnDrawToolDeselected();
    }
}
