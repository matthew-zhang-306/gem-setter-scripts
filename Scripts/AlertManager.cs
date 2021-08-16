using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AlertManager : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private Canvas canvas              = default;
    [SerializeField] private UIAnimator backAnimator    = default;
    [SerializeField] private UIAnimator panelAnimator   = default;
    [SerializeField] private TextMeshProUGUI promptText = default;
    [SerializeField] private MenuButton[] optionButtons = default;
    [SerializeField] private Button closeButton         = default;

    private Action[] optionCallbacks;

    public static EmptyDelegate OnAlert;


    private void Awake() {
        canvas.gameObject.SetActive(true);
    }


    /* GENERAL ALERT SYSTEM THINGS */
    public void DoAlert(string prompt, bool canClose, params (string name, Action callback)[] options) {
        promptText.text = prompt;
        closeButton.gameObject.SetActive(canClose);

        // initialzie option buttons
        optionCallbacks = new Action[options.Length];
        for (int o = 0; o < optionButtons.Length; o++) {
            if (o < options.Length) {
                optionButtons[o].SetText(options[o].name);
                optionCallbacks[o] = options[o].callback;

                optionButtons[o].gameObject.SetActive(true);
            }
            else {
                optionButtons[o].gameObject.SetActive(false);
            }
        }
        if (options.Length > optionButtons.Length) {
            Debug.LogWarning("AlertManager does not have enough option buttons for all of the desired options here. Some options will be ignored.");
        }

        backAnimator.Enter();
        panelAnimator.Enter();

        OnAlert?.Invoke();
    }


    public void OptionSelected(int id) {
        if (id >= optionCallbacks.Length) {
            return;
        }

        Close();
        optionCallbacks[id]?.Invoke();
    }

    public void Close() {
        backAnimator.Exit();
        panelAnimator.Exit();
    }


    /* ALERT SHORTCUTS */
    public void DoAreYouSureAlert(string prompt, bool condition, Action confirmEvent) {
        if (condition) {
            DoAlert(prompt, true,
                ("no go back", null),
                ("yes do it", confirmEvent)
            );
        }
        else {
            confirmEvent?.Invoke();
        }
    }

    public void DoAreYouSureAlert(string prompt1, bool condition1, string prompt2, bool condition2, Action confirmEvent) {
        DoAreYouSureAlert(prompt1, condition1, () => DoAreYouSureAlert(prompt2, condition2, confirmEvent));
    }

    public void DoNotificationAlert(string notification, string buttonText = "ok") {
        DoAlert(notification, true,
            (buttonText, null)
        );
    }

}
