using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelPlayer : MonoBehaviour
{
    public static readonly string ERROR = "~~";

    public string debugFileName;
    private string fileName;
    private string returnSceneName;

    public int levelId { get; private set; }

    public LevelTiles levelTiles;
    private LevelInfo levelInfo;
    public GameObject levelManagerPrefab;

    public LevelManager levelManager { get; private set; }

    private bool hasUndone;
    private bool inputDisabled;

    private void Awake() {
        try {
            string[] transitionTag = Managers.ScenesManager?.GetTransitionTag().Split(' ');
            returnSceneName = transitionTag[0];

            fileName = transitionTag.Length > 1 ? transitionTag[1] : debugFileName;
            if (fileName == null || fileName.Length == 0) {
                fileName = debugFileName;
            }

            if (fileName.StartsWith("level") && int.TryParse(fileName.Substring(5), out int levelId)) {
                this.levelId = levelId;

                levelInfo = Managers.FileManager.LoadLevelInfo(fileName);
                levelTiles.LoadFromLevelInfo(levelInfo);
                levelManager = GameObject.Instantiate(levelManagerPrefab).GetComponent<LevelManager>();
                
                levelManager.StartPlay(levelTiles);
            }
            else {
                throw new System.Exception("The filename was not in the expected format.");
            }
        }
        catch (System.Exception e) {
            Debug.LogError("Could not load level with fileName " + fileName + ":");
            Debug.LogError(e);
        
            Managers.ScenesManager.SetTransitionTag("level" + ERROR);

            if (returnSceneName != null && returnSceneName.Length > 0) {
                Managers.ScenesManager.GoToSceneImmediate(returnSceneName);
            } else {
                Managers.ScenesManager.GoToSceneImmediate(Managers.ScenesManager.MainMenuIndex);
            }
        }
    }


    private void OnEnable() {
        LevelManager.OnUndo += OnUndo;
        PlayerController.OnWin += OnWin;
        CreditsSequence.OnStart += DisableInput;
    }

    private void OnDisable() {
        LevelManager.OnUndo -= OnUndo;
        PlayerController.OnWin -= OnWin;
        CreditsSequence.OnStart -= DisableInput;
    }


    private void Update() {
        if (inputDisabled) {
            return;
        }
        
        if (Input.GetKeyDown(KeyCode.Escape)) {
            Quit();
        }
    }


    private void OnUndo() {
        hasUndone = true;
    }

    private void OnWin() {
        levelInfo.flags.SetCompleted(true);
        if (!hasUndone) {
            levelInfo.flags.verified = true;
        }

        Managers.FileManager.SaveLevelInfo(levelInfo, fileName);
    }


    public void Quit() {
        Managers.ScenesManager.SetTransitionTag("level");
        Managers.ScenesManager.GoToScene(returnSceneName);
    }


    private void DisableInput() {
        inputDisabled = true;
    }
}
