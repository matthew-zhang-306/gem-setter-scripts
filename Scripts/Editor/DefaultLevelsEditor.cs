using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(DefaultLevels))]
public class DefaultLevelsEditor : Editor
{
    public FileManager fileManager;

    private int index;
    private string levelName;

    private DefaultLevels defaultLevels;

    public override void OnInspectorGUI() {
        defaultLevels = target as DefaultLevels;
        if (fileManager == null) {
            fileManager = defaultLevels.GetComponentInParent<FileManager>();
        }

        GUILayout.Label("Default Levels:");

        int i = 0;
        foreach (LevelInfo level in defaultLevels.defaultLevels) {
            GUILayout.Label(i + ": " + level.levelName + " (" + level.fileName + ")");
            i++;
        }

        GUILayout.Space(20f);

        index = EditorGUILayout.IntField("Index", index);
        levelName = EditorGUILayout.TextField("Level Name", levelName);

        if (GUILayout.Button("Insert Level")) {
            InsertLevel();
        }
        
        if (GUILayout.Button("Remove Level")) {
            RemoveLevel();
        }
        
        if (GUILayout.Button("Add All")) {
            AddAll();
        }
    }


    private void InsertLevel() {
        if (index < 0 || index > defaultLevels.defaultLevels.Count) {
            Debug.LogError("Cannot insert default level at index " + index + " because it is out of bounds (size = " + defaultLevels.defaultLevels.Count + ")");
            return;
        }

        LevelInfo levelInfo = null;
        for (int i = 1; i <= fileManager.NumLevelSlots; i++) {
            if (fileManager.LevelExists("level" + i)) {
                LevelInfo info = fileManager.LoadLevelInfo("level" + i);
                if (info != null && info.levelName == levelName) {
                    levelInfo = info;
                    break;
                }
            }
        }

        if (levelInfo == null) {
            Debug.LogError("Cannot insert default level " + levelName + " because no such level was found");
            return;
        }

        defaultLevels.defaultLevels.Insert(index, levelInfo);
        EditorUtility.SetDirty(defaultLevels);
    }


    private void RemoveLevel() {
        if (index < 0 || index >= defaultLevels.defaultLevels.Count) {
            Debug.LogError("Cannot remove default level at index " + index + " because it is out of bounds (size = " + defaultLevels.defaultLevels.Count + ")");
            return;
        }
        
        defaultLevels.defaultLevels.RemoveAt(index);
        EditorUtility.SetDirty(defaultLevels);
    }


    private void AddAll() {
        for (int i = 0; i < 55; i++) {
            defaultLevels.defaultLevels.Add(fileManager.LoadLevelInfo("level" + i));
        }

        EditorUtility.SetDirty(defaultLevels);
    }
}
