using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EditorColorPicker : MonoBehaviour
{
    public Sprite sliderBGColor;
    public Sprite sliderBGGray;

    [Header("Components")]
    public Slider hueSlider;
    public Toggle grayToggle;
    public Image hueSliderBG;
    public TextMeshProUGUI nameText;

    
    private void Start() {
        hueSlider.maxValue = ColorData.TOTAL_HUE / ColorData.HUE_PRECISION;
    }

    private void OnEnable() {
        
    }


    public void OnChangeSlider() {

    }

    public void OnToggle() {
        hueSlider.interactable = !grayToggle.isOn;
        hueSliderBG.sprite = grayToggle.isOn ? sliderBGGray : sliderBGColor;
    }


    public int GetHue() {
        return grayToggle.isOn ? -1 : (int)hueSlider.value;
    }

    public void SetHue(int hue) {
        grayToggle.isOn = hue < 0;
        OnToggle();

        hueSlider.value = Mathf.Clamp(hue, 0, hueSlider.maxValue);
    }

    public void SetName(string name) {
        nameText.text = name;
    }
}
