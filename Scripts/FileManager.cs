using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using MongoDB.Bson;
using MongoDB.Driver;

public class FileManager : MonoBehaviour
{
    private const double UPLOAD_COOLDOWN = 1.5;
    private const double REPORT_COOLDOWN = 0.5;

    private MongoClient client;
    private IMongoDatabase database;
    private IMongoCollection<LevelInfoDoc> gemCollection;
    private MongoClient clienc;
    private IMongoDatabase datacase;
    private IMongoCollection<Sad> sadCollection;


    public DefaultLevels defaultLevels;
    public int NumDefaultLevels { get { return defaultLevels.defaultLevels.Count; }}

    [SerializeField] private int numLevelSlots = 150;
    public int NumLevelSlots => numLevelSlots;


    [Header("Debug")]
    public string debugWrite;
    public string debugRead;


    private void Awake() {
        if (Managers.FileManager != this) {
            return;
        }
        
        client = new MongoClient(MongoAuthDetails.DB_URI);
        database = client.GetDatabase(MongoAuthDetails.DB_PATH[0]);
        gemCollection = database.GetCollection<LevelInfoDoc>(MongoAuthDetails.DB_PATH[1]);
        clienc = new MongoClient(MongoAuthDetails.DC_URI);
        datacase = clienc.GetDatabase(MongoAuthDetails.DC_PATH[0]);
        sadCollection = datacase.GetCollection<Sad>(MongoAuthDetails.DC_PATH[1]);
    }
    

    [ContextMenu("Print Data Path")]
    public void PrintDataPath() {
        Debug.Log(Application.persistentDataPath);
    }

    [ContextMenu("Print User Hash")]
    public void PrintUserHash() {
        Debug.Log(GetUserHash());
    }

    private int GetUserHash() {
        return Mathf.Abs(Application.persistentDataPath.GetHashCode()) % 2_000_000_000 + 1;
    }

    public bool CheckCooldown(string playerPrefName, double cooldown, string actionName, out string message) {
        string dateString = PlayerPrefs.GetString(playerPrefName, "");
        double minutesSince = 2 * cooldown; // this is a default value in case the date string cannot be parsed

        if (dateString.Length > 0) {
            // there was a date string on this player pref. try to get the time difference
            if (DateTime.TryParse(dateString, out DateTime dateTime)) {
                minutesSince = DateTime.Now.Subtract(dateTime).TotalMinutes;
            }
            else {
                Debug.LogWarning("FileManager: Could not parse date string " + dateString);
            }
        }

        if (minutesSince < cooldown) {
            // the cooldown is not yet over.
            // tell user how much time they will have to wait in seconds
            int secondsUntil = (int)Math.Round(60 * (cooldown - minutesSince));

            message = $"you're {actionName} too fast! try again in {secondsUntil} second(s).";
            return false;
        }

        // the cooldown is over, so set a new cooldown and let the action through
        PlayerPrefs.SetString(playerPrefName, DateTime.Now.ToString());
        message = "";
        return true;
    }


    //       //
    // FILES //
    //       //

    // return value corresponds to whether a file was overwritten
    public bool SaveLevel(LevelTiles levelTiles, string fileName) {
        LevelInfo levelInfo = levelTiles.GenerateLevelInfo();
        return SaveLevelInfo(levelInfo, fileName);
    }

    public bool SaveLevelInfo(LevelInfo info, string fileName) {
        info.fileName = fileName;
        
        // check for overwrite at existing path
        string path = Path.Combine(Application.persistentDataPath, $"{info.fileName}.gem");
        bool overwritten = File.Exists(path);

        BinaryFormatter formatter = new BinaryFormatter();
        using (FileStream stream = new FileStream(path, FileMode.Create)) {
            formatter.Serialize(stream, info);
        }

        return overwritten;
    }


    public bool InstallDefaultLevels() {
        bool overwritten = false;

        for (int i = 0; i < NumDefaultLevels; i++) {
            overwritten = SaveLevelInfo(defaultLevels.defaultLevels[i], "level" + (i+1)) || overwritten;

            try {
                Managers.FileManager.DownloadLevel(defaultLevels.defaultLevels[i], "level" + (i+1), false, false, out _);
            }
            catch (Exception e) {
                Debug.LogError(e);
            }
        }

        return overwritten;
    }


    public void LoadLevel(LevelTiles levelTiles, string fileName) {
        LevelInfo info = LoadLevelInfo(fileName);

        if (info != null) {
            levelTiles.LoadFromLevelInfo(info);
        }
    }

    public LevelInfo LoadLevelInfo(string fileName) {
        // check for file existing
        string path = Path.Combine(Application.persistentDataPath, $"{fileName}.gem");
        if (!File.Exists(path)) {
            Debug.LogError("No file exists at " + path + "!");
            return null;
        }

        LevelInfo info;
        BinaryFormatter formatter = new BinaryFormatter();
        using (FileStream stream = new FileStream(path, FileMode.Open)) {
            info = formatter.Deserialize(stream) as LevelInfo;
        }

        return info;
    }

    public bool DeleteLevel(string fileName) {
        string path = Path.Combine(Application.persistentDataPath, $"{fileName}.gem");
        if (!File.Exists(path)) {
            Debug.LogError("No file exists at " + path + "!");
            return false;
        }

        File.Delete(path);
        return true;
    }


    public bool LevelExists(string fileName) {
        // check for file existing
        string path = Path.Combine(Application.persistentDataPath, $"{fileName}.gem");
        return File.Exists(path);
    }

    
    //          //
    // DATABASE //
    //          //

    public bool UploadLevel(LevelInfo level, out string message) {
        if (!CheckCooldown("upload time", UPLOAD_COOLDOWN, "uploading", out string m)) {
            message = m;
            return false;
        }

        if (SadExists()) {
            message = "upload failed: please try again later.";
            return false;
        }

        // set the unique id to a random number
        level.flags.uniqueId = new System.Random().Next();

        try {
            LevelInfoDoc levelDoc = level.ToDoc();
            levelDoc.uploaderId = GetUserHash();
            gemCollection.InsertOne(levelDoc);

            level.flags.uploadStr = levelDoc._id.ToString();
            level.flags.modified = false;
            SaveLevelInfo(level, level.fileName);

            message = "upload successful!";
            return true;
        }
        catch (System.Exception e) {
            Debug.LogError(e);

            message = "upload failed: there was a problem contacting the database.";
            return false;
        }
    }

    public LevelInfo DownloadLevel(LevelInfo existingLevel, string fileName, bool shouldCheckIfRemoved, bool shouldIncreaseDownloads, out string message) {
        existingLevel.fileName = fileName;

        try {
            ObjectId objectId = new ObjectId(existingLevel.flags.uploadStr);

            LevelInfoDoc levelDoc = gemCollection.Find(doc => doc._id.Equals(objectId)).Limit(1).ToList<LevelInfoDoc>().FirstOrDefault();
            LevelInfo level;

            if (levelDoc != null) {
                level = LevelInfo.FromDoc(levelDoc);
                
                // remove completed flag
                existingLevel.flags.SetCompleted(false);

                if (shouldIncreaseDownloads) {
                    TryIncreaseDownloads(levelDoc);
                }

                message = "download successful.";
            } else {
                // this level doesn't exist on the database!
                level = existingLevel;

                if (shouldCheckIfRemoved) {
                    // revoke uploaded status 
                    existingLevel.uploadStr = null;
                }

                message = "this level appears to no longer exist. there could have been a problem finding it, or it could have been removed. if you feel like this is a mistake, please contact me (1f1n1ty) with your concerns.";
            }
            
            SaveLevelInfo(level, existingLevel.fileName);

            return level;
        }
        catch (System.Exception e) {
            Debug.LogError(e);

            message = "download failed: there was a problem contacting the database.";
            return null;
        }
    }

    public List<LevelInfoDoc> FetchRecentLevels(int limit, int skip = 0) {
        try {
            return gemCollection.Find(_ => true).SortByDescending(doc => doc._id).Skip(skip).Limit(limit).ToList<LevelInfoDoc>();
        }
        catch (System.Exception e) {
            Debug.LogError(e);
            return new List<LevelInfoDoc>();
        }
    }


    public List<LevelInfoDoc> FetchLevels(int limit, int skip, BrowseSearchOptions searchOptions) {
        try {
            var search = gemCollection.Find(GetSearchFilter(searchOptions));

            // do sorting
            if (searchOptions.sortBy == BrowseSearchOptions.SortBy.RECENT) {
                search = search.SortByDescending(doc => doc._id);   
            }
            else if (searchOptions.sortBy == BrowseSearchOptions.SortBy.DOWNLOADS) {
                search = search.SortByDescending(doc => doc.downloads);
            }

            return search.Skip(skip).Limit(limit).ToList<LevelInfoDoc>();
        }
        catch (System.Exception e) {
            Debug.LogError(e);
            return new List<LevelInfoDoc>();
        }
    }


    private bool TryIncreaseDownloads(LevelInfoDoc levelDoc) {
        string uploadStr = levelDoc._id.ToString();
        List<string> downloadedList = PlayerPrefsX.GetStringArray("downloaded").ToList();

        if (!downloadedList.Contains(uploadStr)) {
            // this level has not been downloaded by this machine before, so increment the downloaded count
            levelDoc.downloads++;
            gemCollection.FindOneAndReplace(doc => doc._id.Equals(levelDoc._id), levelDoc);

            // only set the playerprefs flag if the level has a non-negligible number of downloads
            if (levelDoc.downloads >= 5) {
                if (downloadedList.Count >= 200) {
                    // there are too many items on this list, so discard the first one
                    downloadedList.RemoveAt(0);
                }
                
                downloadedList.Add(uploadStr);
                PlayerPrefsX.SetStringArray("downloaded", downloadedList.ToArray());
            }

            return true;
        }

        return false;
    }

    public bool TryReport(LevelInfoDoc levelDoc, out string message) {
        if (!CheckCooldown("report time", REPORT_COOLDOWN, "reporting", out string m)) {
            message = m;
            return false;
        }

        try {
            levelDoc.uploaderId = -levelDoc.uploaderId;
            gemCollection.FindOneAndReplace(doc => doc._id.Equals(levelDoc._id), levelDoc);
            
            message = "level reported.";
            return true;
        }
        catch (System.Exception e) {
            Debug.LogError(e);

            message = "the database could not be contacted. try again later, or contact me (1f1n1ty) about this directly.";
            return false;
        }
    }


    private FilterDefinition<LevelInfoDoc> GetSearchFilter(BrowseSearchOptions searchOptions) {
        var builder = Builders<LevelInfoDoc>.Filter;
        var filter = builder.Where(doc => true);

        // these do not work.
        if (searchOptions.requireOfficial) {
            filter &= builder.Where(doc => doc.IsOfficial());
        } 
        if (searchOptions.requireVerified) {
            filter &= builder.Where(doc => doc.IsVerified());
        }

        // this works i think
        if (searchOptions.requireBookmarked) {
            var bookmarkedFilter = builder.Where(_ => false);
            foreach (string bookmarkedStr in PlayerPrefsX.GetStringArray("bookmarked")) {
                ObjectId bookmarkedId = new ObjectId(bookmarkedStr);
                bookmarkedFilter |= builder.Where(doc => doc._id.Equals(bookmarkedId));
            }

            filter &= bookmarkedFilter;
        }

        // these work.
        if (searchOptions.searchText != null && searchOptions.searchText.Length > 0) {
            string searchText = searchOptions.searchText.ToLower();
            var nameFilter =        builder.Where(doc => doc.levelName  .ToLower().Contains(searchText));
            var authorFilter =      builder.Where(doc => doc.levelAuthor.ToLower().Contains(searchText));
            var descriptionFilter = builder.Where(doc => doc.description.ToLower().Contains(searchText));
            
            // if the search text happens to match a "unique" id for a level, we want it to pop up no matter what
            int uniqueId = LevelFlags.GetUniqueIdFromStr(searchOptions.searchText);
            var uniqueFilter = builder.Where(doc => doc.uniqueId == uniqueId);
            
            switch (searchOptions.searchBy) {
                case BrowseSearchOptions.SearchBy.ANY:
                    filter &= nameFilter | authorFilter | descriptionFilter | uniqueFilter;
                    break;
                case BrowseSearchOptions.SearchBy.NAME:
                    filter &= nameFilter | uniqueFilter;
                    break;
                case BrowseSearchOptions.SearchBy.AUTHOR:
                    filter &= authorFilter | uniqueFilter;
                    break;
                case BrowseSearchOptions.SearchBy.DESCRIPTION:
                    filter &= descriptionFilter | uniqueFilter;
                    break;
            }
        }

        return filter;
    }



    private bool SadExists() {
        try {
            int hash = GetUserHash();
            List<Sad> sad = sadCollection.Find(s => s.sadNumber == hash).ToList();
            return sad.Count > 0;
        }
        catch (System.Exception e) {
            Debug.LogError(e);
            return true;
        }
    }
}
