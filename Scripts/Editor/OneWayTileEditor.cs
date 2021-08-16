using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(OneWayTile))]
public class OneWayTileEditor : Editor
{
    OneWayTile oneWay;

    public override void OnInspectorGUI() {
        DrawDefaultInspector();

        oneWay = target as OneWayTile;

        if (GUILayout.Button("Rotate")) {
            RotateOneWay();
        }
    }

    public void OnSceneGUI() {
        if (oneWay == null) {
            return;
        }

        Event e = Event.current;
        if (e.type == EventType.KeyDown && e.keyCode == KeyCode.I) {
            RotateOneWay();
        }
    }


    private void RotateOneWay() {
        Undo.RecordObject(oneWay.gameObject, "Rotate OneWayTile");

        // rotate the gate
        oneWay.RotateImmediate();

        EditorUtility.SetDirty(oneWay.gameObject);
    }
}
