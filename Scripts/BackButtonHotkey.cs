using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BackButtonHotkey : MonoBehaviour
{
    private Button button;

    private void Start() {
        button = GetComponent<Button>();
    }

    private void Update() {
        if (button != null && button.IsInteractable() && Input.GetKeyDown(KeyCode.Escape)) {
            button.onClick?.Invoke();
        }
    }
}
