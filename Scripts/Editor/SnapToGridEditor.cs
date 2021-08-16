using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(SnapToGrid))]
class SnapToGridEditor : Editor {
    SnapToGrid snap;

    private void Snap() {
        Undo.RecordObject(snap.gameObject, "Snap");
        snap.Snap();

        EditorUtility.SetDirty(snap.gameObject);

        if (snap.gameObject.scene.name == null) {
            // snap.gameObject is a prefab
            PrefabUtility.RecordPrefabInstancePropertyModifications(snap.gameObject);
        }
    }

    public override void OnInspectorGUI() {
        snap = target as SnapToGrid;

        if (GUILayout.Button("Snap")) {
            Snap();
        }

        DrawDefaultInspector();
    }

    public void OnSceneGUI() {
        if (snap == null) {
            return;
        }

        Event e = Event.current;
        if (e.type == EventType.KeyDown && e.keyCode == KeyCode.S) {
            Snap();
        }
    }
}