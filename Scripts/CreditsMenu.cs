using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class CreditsMenu : MonoBehaviour
{
    public CanvasGroup canvasGroup;

    public GameObject mainCamera;
    public MainMenu mainMenu;


    private void Start() {
        canvasGroup.interactable = false;

        if (Managers.ScenesManager.GetTransitionTag() == "credits") {
            Open();
        }
    }

    public void Open() {
        canvasGroup.interactable = true;

        mainCamera.transform.DOKill();
        mainCamera.transform.DOMove(transform.position.WithZ(mainCamera.transform.position.z), 0.5f).SetEase(Ease.InOutCubic);
    }

    public void Back() {
        canvasGroup.interactable = false;
        mainMenu.Open();
    }
}
