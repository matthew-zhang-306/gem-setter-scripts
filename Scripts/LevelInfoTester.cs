using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MongoDB.Driver;

public class LevelInfoTester : MonoBehaviour
{  
    public string fileName;

    public LevelInfo info;
    public LevelInfoDoc infoDoc;

    public LevelTiles levelTiles;

    private MongoClient client;
    private IMongoDatabase database;
    private IMongoCollection<LevelInfoDoc> collection;


    private void Start() {
        if (levelTiles != null) {
            info = levelTiles.GenerateLevelInfo();
        }
    }


    [ContextMenu("Load")]
    public void LoadLevel() {
        info = Managers.FileManager.LoadLevelInfo(fileName);
        levelTiles.LoadFromLevelInfo(info);
    }

    [ContextMenu("Save")]
    public void SaveLevel() {
        Managers.FileManager.SaveLevelInfo(info, fileName);
    }


    [ContextMenu("Test Unique")]
    public void TestUnique() {
        Debug.Log(info.flags.uniqueId + " " + info.flags.uniqueStr + " " + LevelFlags.GetUniqueIdFromStr(info.flags.uniqueStr));
    }


    [ContextMenu("Prepare Levels")]
    public void PrepareLevels() {
        for (int i = 1; i <= 55; i++) {
            LevelInfo levelInfo = Managers.FileManager.LoadLevelInfo("level" + i);
            Debug.Log(i + ": " + levelInfo.levelName + " | " + levelInfo.levelAuthor);

            if (i <= 50) {
                levelInfo.description = $"official level set {i}/50";
            }
            
            levelInfo.flags.official = true;
            levelInfo.flags.verified = true;
            levelInfo.flags.uploadStr = "";
            levelInfo.flags.modified = false;

            Managers.FileManager.SaveLevelInfo(levelInfo, "level" + i);
        }
    }


    [ContextMenu("Upload All")]
    public void UploadAll() {
        for (int i = 1; i <= 55; i++) {
            LevelInfo levelInfo = Managers.FileManager.LoadLevelInfo("level" + i);
            Debug.Log(i + ": " + levelInfo.levelName + " | " + levelInfo.levelAuthor);

            Managers.FileManager.UploadLevel(levelInfo, out string _);
        }
    }
}
