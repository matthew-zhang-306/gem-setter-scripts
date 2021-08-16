using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


// scriptable object list of the below
[CreateAssetMenu(fileName = "New Color Dictionary", menuName = "GemEditor/Color Dictionary", order = 2)]
public class ColorDictionary : ScriptableObject {
    public ColorData[] colorDatas;

    public ColorData GetColorData(int index, int hue = ColorData.TOTAL_HUE + 1) {
        return colorDatas[index].Clone(hue <= ColorData.TOTAL_HUE ? hue : colorDatas[index].hue);
    }
}


[Serializable]
public struct ColorData {
    public const int HUE_PRECISION = 5;
    public const int TOTAL_HUE = 360;

    public string name;
    
    public Color baseColor;
    public Color baseGray;
    public int hue;


    public Color Color { get {
        if (hue < 0) {
            return baseGray;
        } else {
            return baseColor.WithHue(((float)hue / (float)TOTAL_HUE * (float)HUE_PRECISION) % 1);
        }
    }}

    public ColorData Clone(int newHue) {
        ColorData cD = new ColorData();
        cD.baseColor = this.baseColor;
        cD.baseGray = this.baseGray;
        cD.hue = newHue;
        return cD;
    }
}