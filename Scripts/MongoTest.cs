using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MongoDB.Bson;
using MongoDB.Driver;

public class MongoTest : MonoBehaviour
{
    private MongoClient client;
    private IMongoDatabase database;
    private IMongoCollection<LevelInfoDoc> collection;


    public int flagSize;
    public int newFlagSize;
    public bool verified;
    public bool official;
    public bool featured;


    private void Awake() {
        client = new MongoClient(MongoAuthDetails.DB_URI);
        database = client.GetDatabase(MongoAuthDetails.DB_PATH[0]);
        collection = database.GetCollection<LevelInfoDoc>(MongoAuthDetails.DB_PATH[1]);
    }

    private void Start()
    {
        // UploadMany();
        // DeleteMany();
    }


    private void UploadMany() {
        LevelInfo levelInfo = Managers.FileManager.LoadLevelInfo("level16");

        if (levelInfo != null) {
            for (int i = 0; i < 45; i++) {
                if (!Managers.FileManager.UploadLevel(levelInfo, out string message)) {
                    Debug.LogError(message);
                }
            }
        }
    }


    private void DeleteMany() {
        collection.DeleteMany(doc => doc.levelName == "tutorial 1");
    }


    [ContextMenu("Get New FlagSize")]
    public void GetNewFlagSize() {
        newFlagSize = flagSize & 0x0000FFFF;
        newFlagSize = newFlagSize | (verified ? 0x01000000 : 0);
        newFlagSize = newFlagSize | (official ? 0x00100000 : 0);
        newFlagSize = newFlagSize | (featured ? 0x00010000 : 0);
    }
}
