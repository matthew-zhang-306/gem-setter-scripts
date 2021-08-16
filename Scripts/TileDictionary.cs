using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Tile Dictionary", menuName = "GemEditor/Tile Dictionary", order = 1)]
public class TileDictionary : ScriptableObject
{
    public TileObjectData[] tileTypes;

    [Serializable]
    public struct TileObjectData {
        public string name;
        public GameObject prefab;
        public Sprite image;
    }
}
