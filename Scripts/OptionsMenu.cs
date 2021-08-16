using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class OptionsMenu : MonoBehaviour
{
    public GameObject mainCamera;
    public MainMenu mainMenu;
    public LevelSlots levelSlots;

    [Header("Components")]
    public CanvasGroup canvasGroup;
    public MenuButton backButton;
    public UIAnimator[] panelAnimators;
    private int currentPanelIndex;

    public TextMeshProUGUI songNameText;
    public Slider musicVolumeSlider;
    public Slider soundVolumeSlider;
    public Toggle vinylToggle;
    public Toggle loopToggle;


    public delegate void OptionsIntAction(int value);
    public static OptionsIntAction OnSelectCharacter;
    

    private void Awake() {
        canvasGroup.interactable = false;

        foreach (UIAnimator panel in panelAnimators) {
            panel.gameObject.SetActive(true);
        }
    }

    private void Start() {
        musicVolumeSlider.value = Managers.AudioManager.MusicVolume * musicVolumeSlider.maxValue;
        soundVolumeSlider.value = Managers.AudioManager.SoundVolume * soundVolumeSlider.maxValue;
        vinylToggle.SetIsOnWithoutNotify(Managers.AudioManager.Vinyl);
        loopToggle.SetIsOnWithoutNotify(Managers.AudioManager.ShouldLoopForever);
    }

    private void Update() {
        songNameText.text = Managers.AudioManager?.GetFullSongName() ?? "???";
    }

    public void Open() {
        canvasGroup.interactable = true;

        mainCamera.transform.DOKill();
        mainCamera.transform.DOMove(transform.position.WithZ(mainCamera.transform.position.z), 0.5f).SetEase(Ease.InOutCubic);
    }

    public void Back() {
        if (currentPanelIndex > 0) {
            SwitchPanel(0);
        }
        else {
            canvasGroup.interactable = false;
            mainMenu.Open();
        }
    }


    public void SwitchPanel(int panelIndex) {
        int previousPanelIndex = currentPanelIndex;
        currentPanelIndex = panelIndex;

        backButton.SetInteractable(false);

        // do exit -> enter -> set back button
        panelAnimators[previousPanelIndex].Exit();
        panelAnimators[previousPanelIndex].currentTween.OnComplete(() => {
            panelAnimators[currentPanelIndex].Enter();
            panelAnimators[currentPanelIndex].currentTween.OnComplete(() => {
                backButton.SetInteractable(true);
            });
        });
    }


    public void AudioPrevPressed() {
        Managers.AudioManager?.CycleSong(false);
    }

    public void AudioNextPressed() {
        Managers.AudioManager?.CycleSong(true);
    }

    public void AudioMusicVolumeChanged(float value) {
        Managers.AudioManager.MusicVolume = value / musicVolumeSlider.maxValue;
    }

    public void AudioSoundVolumeChanged(float value) {
        Managers.AudioManager.SoundVolume = value / soundVolumeSlider.maxValue;
    }

    public void AudioVinyl(bool value) {
        Managers.AudioManager.Vinyl = value;
    }

    public void AudioLoop(bool value) {
        Managers.AudioManager.SetLoopForever(value);
    }


    public void SelectCharacter(float c) {
        PlayerPrefs.SetInt("character", Mathf.RoundToInt(c));
        OnSelectCharacter?.Invoke(Mathf.RoundToInt(c));
    }


    public void RemoveBookmarks() {
        Managers.AlertManager.DoAreYouSureAlert(
            $"are you sure? this will remove {LevelFlags.GetNumBookmarked()} bookmark(s).", true,
            () => {
                LevelFlags.ClearAllBookmarked();
                levelSlots.Reset();
                Managers.AlertManager.DoNotificationAlert("clear successful.");
            }
        );
    }

    public void UncompleteLevels() {
        int numCompleted = 0;
        for (int i = 1; i <= Managers.FileManager.NumLevelSlots; i++) {
            if (Managers.FileManager.LevelExists("level" + i) && LevelFlags.GetCompleted("level" + i)) {
                numCompleted++;
            }
        }

        Managers.AlertManager.DoAreYouSureAlert(
            $"are you sure? this will remove the completed flag from {numCompleted} level(s).", true,
            () => {
                for (int i = 1; i <= Managers.FileManager.NumLevelSlots; i++) {
                    LevelFlags.SetCompleted(false, "level" + i);
                }
                levelSlots.Reset();

                Managers.AlertManager.DoNotificationAlert("operation successful.");
            }
        );
    }

    public void ReinstallDefaults() {
        int numOverwrites = 0;
        for (int i = 1; i <= Managers.FileManager.NumDefaultLevels; i++) {
            if (Managers.FileManager.LevelExists("level" + i)) {
                numOverwrites++;
            }
        }

        Managers.AlertManager.DoAreYouSureAlert(
            $"are you sure? this will overwrite {numOverwrites} level(s).", true,
            () => {
                Managers.FileManager.InstallDefaultLevels();
                levelSlots.Reset();
                Managers.AlertManager.DoNotificationAlert("reinstallation successful.");
            }
        );
    }
}
