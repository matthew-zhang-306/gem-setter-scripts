using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UIButtonSound : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler {
	
    private Selectable button;
	public bool doesNotDetect;

	[SerializeField] private bool doesHover = true;
	[SerializeField] private bool doesClick = true;

	private float interactableTimer;


    private void Start() {
        button = GetComponent<Selectable>();
        if (button == null && !doesNotDetect) {
            Debug.LogError("UIButtonSound " + this + " does not have a selectable component!");
        }
		else if (doesNotDetect) {
			// interactableTimer is always active
			interactableTimer = 1;
		}
    }

	private void Update() {
		if (doesNotDetect) {
			return;
		}

		interactableTimer = button.IsInteractable() ? 0.1f : Mathf.Max(0, interactableTimer - Time.deltaTime);
	}

	public void OnPointerEnter(PointerEventData _)
	{
		if (doesHover && interactableTimer > 0)
			Managers.AudioManager?.PlayUIHoverSound();
	}

	public void OnPointerClick(PointerEventData _)
	{
		if (doesClick && interactableTimer > 0)
			Managers.AudioManager?.PlayUIPressSound();
	}
}
