
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using TMPro;

public class CreditsSequence : MonoBehaviour
{
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI madeByText;
    public TextMeshProUGUI levelHeaderText;
    public TextMeshProUGUI levelListText;
    public TextMeshProUGUI soundHeaderText;
    public TextMeshProUGUI soundListText;
    public TextMeshProUGUI skipText;

    private Sequence sequence;

    public static EmptyDelegate OnStart;
    public static EmptyDelegate OnCameraMove;

    private void Start() {
        OnStart?.Invoke();
        Managers.AudioManager.PlayTheme();
        
        sequence = DOTween.Sequence();
        InsertAction(sequence, 0.25f, () => {
            OnCameraMove?.Invoke();
        });
        InsertAction(sequence, 1f, () => {
            skipText.DOFade(1, 1);
        });
        InsertAction(sequence, 1.5f, () => {
            titleText.gameObject.SetActive(true);
            titleText.text = "gem";
        });
        InsertAction(sequence, 1.9f, () => {
            titleText.text = "gem setter";
        });
        InsertAction(sequence, 3.7f, () => {
            DOTween.To(
                () => titleText.text.Length,
                len => titleText.text = titleText.text.Substring(0, len),
                0, 0.8f);
        });
        InsertAction(sequence, 4.5f, () => {
            madeByText.gameObject.SetActive(true);
            madeByText.text = "made by";
        });
        InsertAction(sequence, 4.9f, () => {
            madeByText.text = "made by 1f1n1ty";
        });
        InsertAction(sequence, 6.8f, () => {
            DOTween.To(
                () => madeByText.text.Length,
                len => madeByText.text = madeByText.text.Substring(0, len),
                0, 0.7f);
        });
        InsertAction(sequence, 7.5f, () => {
            levelHeaderText.gameObject.SetActive(true);
            levelHeaderText.text = "additional";
        });
        InsertAction(sequence, 7.85f, () => {
            levelHeaderText.text = "additional level design";
        });
        InsertAction(sequence, 8.65f, () => {
            levelListText.gameObject.SetActive(true);
        });
        InsertAction(sequence, 10.5f, () => {
            levelHeaderText.gameObject.SetActive(false);
            levelListText.gameObject.SetActive(false);
            soundHeaderText.gameObject.SetActive(true);
        });
        InsertAction(sequence, 10.9f, () => {
            soundListText.gameObject.SetActive(true);
        });
        InsertAction(sequence, 12.95f, () => {
            ToMenu();
        });
    }

    private void Update() {
        if (skipText.color.a > 0.1f && Input.GetKeyDown(KeyCode.Space)) {
            ToMenu();
        }
    }

    public void ToMenu() {
        sequence?.Kill();
        Managers.ScenesManager.SetTransitionTag("credits");
        Managers.ScenesManager.GoToScene(Managers.ScenesManager.MainMenuIndex);
    }


    private Sequence InsertAction(Sequence currentSequence, float time, Action action) {
        return currentSequence.Insert(time, DOTween.To(() => 0, _ => { action(); }, 1, 0.01f));
    }
}
