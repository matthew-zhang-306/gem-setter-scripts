using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class BrowsePanelHeaderEventTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public BrowsePanel browsePanel;
    public UIButtonSound buttonSound;

    public void OnPointerEnter(PointerEventData eventData) {
        browsePanel.OnPointerEnter();
    }

    public void OnPointerExit(PointerEventData eventData) {
        browsePanel.OnPointerExit();
    }
    
    public void OnPointerClick(PointerEventData eventData) {
        browsePanel.scrollPanel.OnClick(browsePanel);
    }
}
