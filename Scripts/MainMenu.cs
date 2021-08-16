using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class MainMenu : MonoBehaviour
{
    public GameObject mainCamera;
    public LevelSelectMenu levelSelect;
    public OptionsMenu options;
    public CreditsMenu credits;

    public string editorSceneName;
    public string browseSceneName;

    public GameObject titleContainer;
    public GameObject buttonsContainer;


    private void Start() {
        if (Managers.ScenesManager.GetTransitionTag() == "preload") {
            titleContainer.SetActive(false);
            buttonsContainer.SetActive(false);

            Managers.AudioManager.PlayTheme();

            DOTween.Sequence()
                .Insert(1.5f, DOTween.To(() => 0, val => { titleContainer.SetActive(true);   }, 1, 0.1f))
                .Insert(1.9f, DOTween.To(() => 0, val => { buttonsContainer.SetActive(true); }, 1, 0.1f));
        }
    }

    public void GoToLevelSelect() {
        levelSelect.Open();
    }

    public void Edit() {
        Managers.ScenesManager.GoToScene(editorSceneName);
    }

    public void Browse() {
        Managers.ScenesManager.GoToScene(browseSceneName);
    }

    public void GoToOptions() {
        options.Open();
    }

    public void GoToCredits() {
        credits.Open();
    }

    public void Quit() {
        Managers.ScenesManager.Quit();
    }


    public void Open() {
        mainCamera.transform.DOKill();
        mainCamera.transform.DOMove(transform.position.WithZ(mainCamera.transform.position.z), 0.5f).SetEase(Ease.InOutCubic);
    }
}
