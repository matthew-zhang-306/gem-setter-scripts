using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EditorComponent : MonoBehaviour
{
    [HideInInspector] public EditorManager editorManager;
    public LevelTiles levelTiles { get { return editorManager.levelTiles; }}


    public virtual void Activate() {
        // by default, do nothing
    }
    public virtual void Deactivate() {
        // by default, do nothing
    }
}
