using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

// used when the level is in play.
// despite the name, this is NOT a manager accessible from the Managers class. it's not meant to be treated like a singleton
public class LevelManager : MonoBehaviour
{
    private bool inited;
    private bool inputDisabled;

    public LevelTiles levelTiles;
    private Camera cam;

    private int currentMouseDown;
    private Vector2 previousMousePoint;

    public delegate void LevelAction();
    public static LevelAction OnUndo;
    public static LevelAction OnReset;

    private HashSet<TileItem> overlaps;
 

    private Stack<TileStateFrame> stateStack;
    public bool isCurrentStateFrameOpen { get; private set; }
    public int moveCount { get; private set; }

    public GameObject poofParticles;


    public void StartPlay(LevelTiles level) {
        if (inited) {
            return;
        }
        inited = true;

        if (level == null) {
            Debug.LogError("Cannot initialize LevelManager " + this + " with a null LevelTiles.");
            return;
        }

        this.levelTiles = level;

        foreach (TileItem tile in levelTiles.allTiles) {
            tile.levelManager = this;
            tile.StartPlay();
        }

        overlaps = new HashSet<TileItem>();
        stateStack = new Stack<TileStateFrame>();
    }

    private void Start() {
        cam = Camera.main;
        StartPlay(levelTiles);
    }

    private void OnEnable() {
        CreditsSequence.OnStart += DisableInputs;
    }
    private void OnDisable() {
        CreditsSequence.OnStart -= DisableInputs;
    }

    
    private void Update() {
        if (inputDisabled) {
            return;
        }

        if (cam != null) {
            Vector2 mousePoint = cam.ScreenToWorldPoint(Input.mousePosition).ToTilePosition();

            if (currentMouseDown >= 0) {
                // there is a mouse button currently down, so handle that first
                if (!Input.GetMouseButton(currentMouseDown)) {
                    currentMouseDown = -1;
                }
                else if (mousePoint != previousMousePoint) {
                    // the mouse button is still held and has been dragged
                    HandleClick(currentMouseDown, mousePoint, false);
                }
            }

            if (currentMouseDown == -1) {
                // check for a new mouse input
                for (int i = 0; i <= 1; i++) {
                    if (Input.GetMouseButtonDown(i)) {
                        currentMouseDown = i;
                        HandleClick(currentMouseDown, mousePoint, true);
                    }
                }
            }

            previousMousePoint = mousePoint;
        }

        if (!isCurrentStateFrameOpen) {
            if (Input.GetKeyDown(KeyCode.Z)) {
                Undo();
            }
            else if (Input.GetKeyDown(KeyCode.R)) {
                Reset();
            }
        }
    }

    private void HandleClick(int mouseButton, Vector2 mousePoint, bool isNewInput) {
        foreach (TileItem tile in GetTilesAt(mousePoint)) {
            if (isNewInput) {
                tile.OnClick(mouseButton);
            }
            else {
                tile.OnDrag(mouseButton);
            }
        }
    }


    public HashSet<T> GetTiles<T>() where T : TileItem => levelTiles.GetTiles<T>();

    public HashSet<TileItem> GetTilesAt(Vector2 pos) => levelTiles.GetTilesAt(pos);

    public HashSet<T> GetTilesAt<T>(Vector2 pos) where T : TileItem => levelTiles.GetTilesAt<T>(pos);

    public HashSet<TileItem> GetWalls(Vector2 pos) => levelTiles.GetWalls(pos);

    public TileItem GetTileWithTagAt(string tag, Vector2 pos) => levelTiles.GetTileWithTagAt(tag, pos);


    public bool CheckForBlockage(Vector2 playerPos, Vector2 dir, bool doBumpAnimation = true) {
        List<TileItem> allBlocking = new List<TileItem>();

        foreach (TileItem tile in GetTilesAt(playerPos + dir)) {
            if (tile.Collider != null && tile.Collider.enabled
                && tile.gameObject.layer == LayerMask.NameToLayer("Wall")
                && !levelTiles.player.AllPlayerTiles.Contains(tile)
            ) {
                allBlocking.Add(tile);
            }

            else if (tile is OneWayTile oneWay) {
                if (oneWay.ShouldStopPlayer(levelTiles.player)) {
                    allBlocking.Add(tile);
                }
            }
        }

        if (doBumpAnimation) {
            foreach (TileItem tile in allBlocking) {
                tile.spriteContainer.Bump(dir);
            }
        }

        return allBlocking.Count > 0;    
    }


    // find out what is on the tiles that the player is now moving over
    public void CheckOverlaps(bool isStrong) {
        HashSet<TileItem> newOverlaps = new HashSet<TileItem>();

        List<TileItem> playerTiles = new List<TileItem>(GetTiles<AttachableBox>());
        playerTiles.Add(levelTiles.player);

        foreach (TileItem tile in playerTiles) {
            CheckOverlap(tile, newOverlaps, isStrong);
        }

        // check for overlap exits
        foreach (TileItem item in overlaps) {
            if (!newOverlaps.Contains(item) && levelTiles.GetTilesAt<AttachableBox>(item.tilePosition).Count == 0) {
                item.OnExitOverlap(levelTiles.player);
            }
        }

        overlaps = newOverlaps;

        CheckIfInsideWall(playerTiles);
    }

    private void CheckOverlap(TileItem item, HashSet<TileItem> newOverlaps, bool isStrong) {
        HashSet<TileItem> overlappingTiles = GetTilesAt(item.transform.position);
        
        foreach (TileItem tileItem in overlappingTiles) {
            newOverlaps.Add(tileItem);

            // do on overlap functions for this tile item
            if (isStrong) {
                tileItem.OnStrongOverlap(item, levelTiles.player);
            }
            else {
                tileItem.OnWeakOverlap(item, levelTiles.player);
            }
        }
    }

    // playerTiles grabbed from CheckOverlap
    private void CheckIfInsideWall(List<TileItem> playerTiles) {
        foreach (TileItem tile in playerTiles) {
            if (!levelTiles.IsInBounds(tile.tilePosition)) {
                // this is already dead, so it can't die again
                continue;
            }

            // check for a wall at this position but make sure that the wall is not itself
            if (GetWalls(tile.tilePosition).Where(wall => wall != tile).ToList().Count > 0) {
                Instantiate(poofParticles, tile.transform.position, Quaternion.identity);
                tile.Die();

                // (note that since the player comes last in the playerTiles list, we know that the player will be checked for death last)
            }
        }
    }


    public void OpenNewStateFrame(string desc) {
        if (isCurrentStateFrameOpen) {
            Debug.LogWarning("You are trying to open a new state frame when the current state frame is not closed! You may have closed the old state frame a bit too late.");
        }

        stateStack.Push(new TileStateFrame(desc, moveCount, overlaps));
        isCurrentStateFrameOpen = true;
    
        if (desc.StartsWith("Move")) {
            moveCount++;
        }
    }

    public void CloseCurrentStateFrame() {
        isCurrentStateFrameOpen = false;
    }

    public void RecordState(TileItem item) {
        if (!isCurrentStateFrameOpen) {
            Debug.LogWarning("You are trying to record an undo state while the current state frame is closed! You may have closed it a bit too early.");
            return;
        }

        stateStack.Peek().AddState(item);
    }

    public void Undo(bool noEvent = false) {
        if (isCurrentStateFrameOpen) {
            Debug.LogWarning("You are trying to undo while the current state frame is open! You may have closed it a bit too late.");
            return;
        }

        if (stateStack.Count == 0) {
            return;
        }
        
        overlaps = stateStack.Peek().overlaps;
        moveCount = stateStack.Peek().moves;
        
        stateStack.Pop().RevertStates();

        if (!noEvent) {
            OnUndo?.Invoke();
        }
    }


    public void Reset() {
        if (stateStack.Count == 0 || stateStack.Peek().description == "Reset") {
            // resetting twice in a row, so just ignore this
            return;
        }

        OpenNewStateFrame("Reset");

        foreach (TileItem tile in levelTiles.allTiles) {
            tile.Reset();
        }
        overlaps = new HashSet<TileItem>();

        CloseCurrentStateFrame();
        moveCount = 0;

        OnReset?.Invoke();
    }

    public void StopPlay() {
        foreach (TileItem tile in levelTiles.allTiles) {
            tile.StopPlay();
        }

        Destroy(gameObject);
        Debug.Log("level manager destroys itself");
    }


    private void DisableInputs() {
        inputDisabled = true;
    }
}
