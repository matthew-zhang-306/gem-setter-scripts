using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class OpenHyperlinks : MonoBehaviour, IPointerClickHandler {

    private TMP_Text pTextMeshPro;
    public Camera cam;

    private void Start() {
        pTextMeshPro = GetComponent<TMP_Text>();
    }

    public void OnPointerClick(PointerEventData eventData) {
        if (pTextMeshPro == null) return;

        int linkIndex = TMP_TextUtilities.FindIntersectingLink(pTextMeshPro, Input.mousePosition, cam);
        Debug.Log("detected click: " + linkIndex);
        if (linkIndex != -1) {
            TMP_LinkInfo linkInfo = pTextMeshPro.textInfo.linkInfo[linkIndex];
            OpenLink(linkInfo.GetLinkID());
        }
    }

    public void OpenLink(string link) {
        Debug.Log("open link: " + link);
        Application.OpenURL(link);
    }
}