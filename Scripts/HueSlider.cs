using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HueSlider : MonoBehaviour
{
    public UnityEngine.UI.Slider slider;
    public TMPro.TextMeshProUGUI hueText;
    public UIAnimator hueTextAnimator;

    private void Update() {
        hueText.text = "" + (int)slider.value;
    }

    public void OnPointerEnter() {
        hueTextAnimator.Enter();
    }

    public void OnPointerExit() {
        hueTextAnimator.Exit();
    }
}
