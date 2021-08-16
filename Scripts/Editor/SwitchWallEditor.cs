using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(SwitchWall))]
class SwitchWallEditor : Editor {

    SwitchWall wall;

    public override void OnInspectorGUI() {
        DrawDefaultInspector();

        wall = target as SwitchWall;

        if (GUILayout.Button("Invert Starting State")) {
            InvertSwitchWall();
        }
    }

    public void OnSceneGUI() {
        if (wall == null) {
            return;
        }

        Event e = Event.current;
        if (e.type == EventType.KeyDown && e.keyCode == KeyCode.I) {
            InvertSwitchWall();
        }
    }


    private void InvertSwitchWall() {
        Undo.RecordObject(wall.gameObject, "Invert SwitchWall");

        // update the wall
        wall.Switch(false, true);

        EditorUtility.SetDirty(wall.gameObject);
    }

}