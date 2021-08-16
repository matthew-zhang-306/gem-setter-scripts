using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using MongoDB.Bson;


// this is the file version
[Serializable]
public class LevelInfo
{
    public string levelName;
    public string levelAuthor;
    public string description;

    public LevelFlags flags;
    public string fileName {
        get { return flags.fileName; }
        set { flags.fileName = value; }
    }
    public string uploadStr {
        get { return flags.uploadStr; }
        set { flags.uploadStr = value; }
    }

    public byte width;
    public byte height;

    public int[] colorHues;

    public List<TileInfoData> tiles;

    public LevelInfoDoc ToDoc() {
        LevelInfoDoc doc = new LevelInfoDoc();
        doc.levelName = levelName;
        doc.levelAuthor = levelAuthor;
        doc.description = description;
        
        doc.downloads = 0;
        doc.uniqueId = flags.uniqueId;
        
        // set flagSize
        doc.flagSize = 0;
        doc.SetVerified(flags.verified);
        doc.SetOfficial(flags.official);
        doc.SetWidth(width);
        doc.SetHeight(height);

        // clone colors
        doc.colorHues = colorHues.Where(_ => true).ToArray();

        // convert to tile numbers
        doc.tiles = tiles.Select(tile => tile.ToNumber()).ToArray();

        return doc;
    }

    public static LevelInfo FromDoc(LevelInfoDoc doc) {
        LevelInfo level = new LevelInfo();
        level.levelName = doc.levelName;
        level.levelAuthor = doc.levelAuthor;
        level.description = doc.description;

        level.flags.uniqueId = doc.uniqueId;

        // extract flagSize
        level.uploadStr = doc._id.ToString();
        level.flags.verified = doc.IsVerified();
        level.flags.official = doc.IsOfficial();
        level.flags.modified = false;
        level.width = doc.Width();
        level.height = doc.Height();

        // clone colors
        level.colorHues = doc.colorHues.Where(_ => true).ToArray();

        // convert to tile objects
        level.tiles = doc.tiles.Select(num => TileInfoData.FromNumber(num)).ToList();

        return level;
    }

    public LevelInfo Clone(string newFileName = "") {
        LevelInfo clone = new LevelInfo();
        clone.levelName = levelName;
        clone.levelAuthor = levelAuthor;
        clone.description = description;
        clone.flags = flags;
        clone.width = width;
        clone.height = height;
        
        // clone lists
        clone.colorHues = colorHues.Where(_ => true).ToArray();
        clone.tiles = tiles.Where(_ => true).ToList();

        // handle file name
        if (newFileName.Length > 0) {
            clone.fileName = newFileName;
        }

        return clone;
    }
}


// this is the database version
[Serializable]
public class LevelInfoDoc {
    public ObjectId _id;
    
    public string levelName;
    public string levelAuthor;
    public string description;

    public int downloads;

    public int uniqueId;

    // this is a non-zero number.
    // if it is negative, then the level has been reported.
    public int uploaderId;

    // each number contains:
    // empty nibble | verified nibble | official nibble | featured nibble | width byte | height byte
    public int flagSize;

    public int[] colorHues;

    // each numbers contains:
    // i byte | a byte | x byte | y byte
    public int[] tiles;


    /*
     * GETTERS AND SETTERS
     */
    public bool IsVerified() => (flagSize & 0x01000000) != 0;
    public void SetVerified(bool value) { flagSize = flagSize | (value ? (int)0x01000000 : 0); }

    public bool IsOfficial() => (flagSize & 0x00100000) != 0;
    public void SetOfficial(bool value) { flagSize = flagSize | (value ? (int)0x00100000 : 0); }

    public bool IsFeatured() => (flagSize & 0x00010000) != 0;
    
    public byte Width() => (byte)((flagSize & 0x0000FF00) >> 8);
    public void SetWidth(byte value) { flagSize = flagSize | (value << 8); }
    
    public byte Height() => (byte)(flagSize & 0x000000FF);
    public void SetHeight(byte value) { flagSize = flagSize | (value); }

    public bool GetReported() => uploaderId < 0;
}


[Serializable]
public struct TileInfoData {
    public byte i; // id
    public byte a; // alt code
    public byte x; // x grid pos
    public byte y; // y grid pos

    public Vector2Int position => new Vector2Int(x, y);

    public TileInfoData(int id, int alt, Vector2Int gridPosition) {
        i = (byte)id;
        a = (byte)alt;
        x = (byte)gridPosition.x;
        y = (byte)gridPosition.y;
    } 
    

    public int ToNumber() {
        int num = 0;
        num = num | (i << 24);
        num = num | (a << 16);
        num = num | (x << 8);
        num = num | (y);

        return num;
    }
    
    public static TileInfoData FromNumber(int num) {
        TileInfoData tile = new TileInfoData();
        tile.i = (byte)((num & 0xFF000000) >> 24);
        tile.a = (byte)((num & 0x00FF0000) >> 16);
        tile.x = (byte)((num & 0x0000FF00) >> 8);
        tile.y = (byte)( num & 0x000000FF);

        return tile;
    }
}


[Serializable]
public struct LevelFlags {
    public string fileName;
    public string uploadStr;
    public int uniqueId;

    public string uniqueStr { get {
        // change number into base 64
        return Convert.ToBase64String(BitConverter.GetBytes(uniqueId));
    }}

    public bool uploaded { get { return uploadStr != null && uploadStr.Length > 0; }}
    public bool verified;
    public bool official;
    public bool modified;

    public bool GetCompleted() => GetCompleted(fileName);
    public void SetCompleted(bool value) => SetCompleted(value, fileName);
    public bool GetBookmarked() => GetBookmarked(uploadStr);
    public void SetBookmarked(bool value) => SetBookmarked(value, uploadStr);

    public static bool GetCompleted(string fileName) {
        return PlayerPrefs.GetInt("completed " + fileName, 0) > 0;
    }

    public static void SetCompleted(bool value, string fileName) {
        PlayerPrefs.SetInt("completed " + fileName, value ? 1 : 0);
    }

    public static bool GetBookmarked(string uploadStr) {
        return PlayerPrefsX.GetStringArray("bookmarked").Contains(uploadStr);
    }

    public static void SetBookmarked(bool value, string uploadStr) {
        List<string> allBookmarked = PlayerPrefsX.GetStringArray("bookmarked").ToList();
        
        if (value) {
            allBookmarked.Add(uploadStr);
        }
        else {
            allBookmarked.Remove(uploadStr);
        }

        PlayerPrefsX.SetStringArray("bookmarked", allBookmarked.ToArray());
    }

    public static int GetNumBookmarked() {
        return PlayerPrefsX.GetStringArray("bookmarked").Length;
    }

    public static void ClearAllBookmarked() {
        PlayerPrefsX.SetStringArray("bookmarked", new string[0]);
    }


    public static int GetUniqueIdFromStr(string str) {
        try {
            return BitConverter.ToInt32(Convert.FromBase64String(str), 0);
        }
        catch {
            return 0;
        }
    }
}