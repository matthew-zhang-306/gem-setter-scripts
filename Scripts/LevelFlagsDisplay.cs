using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class LevelFlagsDisplay : MonoBehaviour
{
    public GameObject officialFlag;
    public GameObject uploadedFlag;
    public GameObject verifiedFlag;
    public GameObject modifiedFlag;
    public GameObject bookmarkedFlag;
    public GameObject completedFlag;

    public TextMeshProUGUI uniqueStringText;


    public void SetFlags(LevelFlags flags) {
        if (officialFlag != null) {
            officialFlag.SetActive(flags.official);
        }
        if (uploadedFlag != null) {
            uploadedFlag.SetActive(flags.uploaded);
        }
        if (verifiedFlag != null) {
            verifiedFlag.SetActive(flags.verified);
        }
        if (modifiedFlag != null) {
            modifiedFlag.SetActive(flags.uploaded && flags.modified);
        }
        if (bookmarkedFlag != null) {
            bookmarkedFlag.SetActive(flags.uploaded && flags.GetBookmarked());
        }
        if (completedFlag != null) {
            completedFlag.SetActive(flags.fileName != null && flags.GetCompleted());
        }

        if (uniqueStringText != null) {
            uniqueStringText.text = flags.uploaded ? flags.uniqueStr : "";
        }
    }
}
