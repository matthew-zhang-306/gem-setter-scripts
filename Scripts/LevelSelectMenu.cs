using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;
using TMPro;

public class LevelSelectMenu : MonoBehaviour
{
    public CanvasGroup canvasGroup;

    public GameObject mainCamera;
    public LevelSlots levelSlots;
    public LevelPopupPanel popupPanel;
    public MainMenu mainMenu;

    public string levelSceneName;
    public string editorSceneName;
    public string mainSceneName;


    [Header("Path Display")]
    public CanvasGroup pathPanel;
    public TextMeshProUGUI pathText;



    private void Start() {
        canvasGroup.interactable = false;

        if (Managers.ScenesManager.GetTransitionTag()?.StartsWith("level") ?? false) {
            Open();
        }
    }

    public void OnSlotSelect(LevelSlotButton button) {
        if (button.IsDefault) {
            LoadLevel(button, true);
        }
        else {
            popupPanel.Open(button);
        }
    }

    public void LoadLevel(LevelSlotButton button, bool seq) {
        if (button.IsDefault) {
            Managers.ScenesManager.GoToScene(button.number);
        }
        else {
            string transitionTag = $"{mainSceneName} {button.fileName}";
            if (seq) {
                transitionTag += " seq";
            }

            Managers.ScenesManager.SetTransitionTag(transitionTag);
            Managers.ScenesManager.GoToScene(levelSceneName);
        }
    }

    public void EditLevel(LevelSlotButton button) {
        Managers.ScenesManager.SetTransitionTag(button.fileName);
        Managers.ScenesManager.GoToScene(editorSceneName);
    }


    public void UploadLevel(LevelSlotButton button) {
        bool status = Managers.FileManager.UploadLevel(button.level, out string message);
        
        if (status) {
            button.Init(button.number, button.slots, button.IsDefault);
            popupPanel.InitData(button);
        }

        Managers.AlertManager.DoNotificationAlert(message);
    }

    public void RedownloadLevel(LevelSlotButton button) {
        var download = Managers.FileManager.DownloadLevel(button.level, button.fileName, true, false, out string message);

        if (download != null) {
            button.Init(button.number, button.slots, button.IsDefault);
            popupPanel.InitData(button);
        }

        Managers.AlertManager.DoNotificationAlert(message);
    }


    public void Paste(LevelSlotButton button, LevelInfo copied) {
        Managers.FileManager.SaveLevelInfo(copied.Clone(button.fileName), button.fileName);

        // extra stuff to remove flags
        LevelFlags.SetCompleted(false, button.fileName);

        button.Init(button.number, button.slots, button.IsDefault);
    }

    public void PasteAsNew(LevelSlotButton button, LevelInfo copied) {
        Managers.FileManager.SaveLevelInfo(copied.Clone(button.fileName), button.fileName);

        // extra stuff to remove flags
        LevelInfo pasted = Managers.FileManager.LoadLevelInfo(button.fileName);
        pasted.flags.SetCompleted(false);
        pasted.uploadStr = null;
        pasted.flags.official = false;
        Managers.FileManager.SaveLevelInfo(pasted, button.fileName);

        button.Init(button.number, button.slots, button.IsDefault);
    }


    public void DeleteLevel(LevelSlotButton button) {
        if (Managers.FileManager.DeleteLevel(button.fileName)) {
            button.Init(button.number, button.slots, button.IsDefault);
        }
    }


    public void Back() {
        canvasGroup.interactable = false;
        mainMenu.Open();
    }


    public void Open() {
        canvasGroup.interactable = true;
        
        mainCamera.transform.DOKill();
        mainCamera.transform.DOMove(transform.position.WithZ(mainCamera.transform.position.z), 0.5f).SetEase(Ease.InOutCubic);
    }


    private void Update() {
        if (Input.GetKeyDown(KeyCode.Q)) {
            pathPanel.alpha = 1;
            pathText.text = $"level files are stored at ${Application.persistentDataPath}";
        }
    }
}
