using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

public class SpinningPlayerImage : MonoBehaviour
{
    public Image image;
    public TextMeshProUGUI text;
    public Slider slider;
    public Sprite[] playerSprites;
    public float spinSpeed;

    private Tween currentShake;

    private void OnEnable() {
        int c = PlayerPrefs.GetInt("character", 0);
        image.sprite = playerSprites[c];
        text.text = c == 1 ? "tess!" : "meg!";
        slider.value = c;

        OptionsMenu.OnSelectCharacter += OnSelectCharacter;
    }

    private void OnDisable() {
        OptionsMenu.OnSelectCharacter -= OnSelectCharacter;
    }

    private void Update() {
        if (gameObject.activeInHierarchy) {
            transform.rotation *= Quaternion.Euler(0, 0, spinSpeed * Time.deltaTime);
        }
    }


    private void OnSelectCharacter(int c) {
        image.sprite = playerSprites[c];
        text.text = c == 1 ? "tess!" : "meg!";

        currentShake?.Complete();
        currentShake = image.rectTransform.DOShakeAnchorPos(0.3f, 10, 100);
    }
}
