using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// despite the name, this is NOT a manager accessible from the Managers class. it's not meant to be treated like a singleton
public class EditorManager : MonoBehaviour
{
    public LevelTiles levelTiles;
    public Camera cam;

    public UIAnimator raycastBlocker;

    public bool canEdit { get; private set; }

    private bool editInProgress;
    public bool EditInProgress {
        get { return editInProgress; }
        set { editInProgress = value; /* add debug log here if necessary */ }
    }

    private EditorComponent[] editorComponents;
    private Stack<EditCommand> undoStack;
    private Stack<EditCommand> redoStack;
    public bool canUndo { get { return canEdit && !editInProgress && undoStack.Count > 0; }}
    public bool canRedo { get { return canEdit && !editInProgress && redoStack.Count > 0; }}

    public int currentSlot { get; private set; } // this will just be 0 for now
    public bool levelModified { get; private set; }

    public delegate void EditorSaveLoadAction(string fileName);
    public static EditorSaveLoadAction OnSave;
    public static EditorSaveLoadAction OnLoad;

    public delegate void EditorAction();
    public static EditorAction OnUndo;
    public static EditorAction OnRedo;
    public static EditorAction OnClear;
    public static EditorAction OnNew;


    private void Awake() {
        string fileName = Managers.ScenesManager?.GetTransitionTag();
        if (fileName != null && fileName.Length > 0) {
            try {
                Debug.Log("try load");
                Managers.FileManager.LoadLevel(levelTiles, fileName);

                OnLoad?.Invoke(fileName);
            }
            catch (System.Exception e) {
                Debug.LogError("Could not load level with fileName " + fileName + ":");
                Debug.LogError(e);
            }
        }
    }

    private void Start() {
        ClearCommands();
        
        editorComponents = GetComponentsInChildren<EditorComponent>();
        foreach (EditorComponent component in editorComponents) {
            component.editorManager = this;
            component.Activate();
        }

        canEdit = true;
    }


    private void ClearCommands() {
        undoStack = new Stack<EditCommand>();
        redoStack = new Stack<EditCommand>();
    }

    public void AddCommand(EditCommand command) {
        if (!command.hasExecuted) {
            command.Execute();
        }

        undoStack.Push(command);

        if (redoStack.Count > 0) {
            redoStack = new Stack<EditCommand>();
        }

        levelModified = true;
    }

    public void Undo() {
        if (!canUndo) {
            return;
        }

        Debug.Log(undoStack.Peek());

        EditCommand command = undoStack.Pop();
        command.Undo();
        redoStack.Push(command);
    }

    public void Redo() {
        if (!canRedo) {
            return;
        }

        EditCommand command = redoStack.Pop();
        command.Redo();
        undoStack.Push(command);
    }



    public void EnableRaycastBlocker() {
        raycastBlocker.Enter();
        canEdit = false;
    }

    public void DisableRaycastBlocker() {
        raycastBlocker.Exit();
        canEdit = true;
    }


    public void OnStartPlay() {
        canEdit = false;
        foreach (EditorComponent component in editorComponents) {
            component.Deactivate();
        }
    }

    public void OnStopPlay() {
        canEdit = true;
        foreach (EditorComponent component in editorComponents) {
            component.Activate();
        }
    }



    // this implementation of Clear is very elaborate bc it needs to be undoable
    public void Clear() {
        EraseCommand eraseCommand = null;
        for (byte x = 0; x < levelTiles.Width; x++) {
            for (byte y = 0; y < levelTiles.Height; y++) {
                HashSet<TileItem> tiles = levelTiles.GetTilesAt(levelTiles.GridToTilePosition(x, y));
                if (tiles.Count > 0) {
                    // erase these tiles
                    if (eraseCommand == null) {
                        eraseCommand = new EraseCommand(new Vector2Int(x, y), levelTiles);
                        AddCommand(eraseCommand);
                    } else {
                        eraseCommand.AddPosition(new Vector2Int(x, y));
                    }
                }
            }
        }

        if (eraseCommand != null) {
            OnClear?.Invoke();
        }
    }

    public void New() {
        // TODO: implement
        levelTiles.ClearEverything();
        ClearCommands();
        
        OnNew?.Invoke();
    }


    public void Save(string fileName) {
        LevelInfo levelInfo = levelTiles.GenerateLevelInfo();
        levelInfo.fileName = fileName.Length > 0 ? fileName : Managers.FileManager.debugWrite;

        // add default metadata
        if (levelInfo.levelName.Length == 0) {
            levelInfo.levelName = "untitled";
        }
        if (levelInfo.levelAuthor.Length == 0) {
            levelInfo.levelAuthor = "anonymous";
        }

        // remove some flags
        levelInfo.flags.verified = false;
        levelInfo.flags.SetCompleted(false);
        levelInfo.flags.modified = true;

        Managers.FileManager.SaveLevelInfo(levelInfo, levelInfo.fileName);

        levelModified = false;
        OnSave?.Invoke(fileName);
    }

    public void Load(string fileName) {
        if (fileName.Length > 0) {
            Managers.FileManager.LoadLevel(levelTiles, fileName);
        }
        else {
            Managers.FileManager.LoadLevel(levelTiles, Managers.FileManager.debugRead);
        }

        ClearCommands();

        OnLoad?.Invoke(fileName);
    }


    public void QuitToMenu() {
        Managers.ScenesManager.GoToScene(Managers.ScenesManager.MainMenuIndex);
    }


    public void DoUnsavedChangesAlert(System.Action confirmAction) =>
        Managers.AlertManager.DoAreYouSureAlert("are you sure? unsaved changes will be lost.", levelModified, confirmAction);
}
