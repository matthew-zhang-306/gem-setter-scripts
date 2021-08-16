using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BookmarkButton : MonoBehaviour
{
    private Sprite unbookmarkedSprite;
    public Sprite bookmarkedSprite;

    public LevelFlagsDisplay flagsDisplay;

    public Button button;

    private LevelInfo level;


    private void Awake() {
        unbookmarkedSprite = button.image.sprite;        
    }


    public void SetLevel(LevelInfo levelInfo) {
        this.level = levelInfo;
        
        if (!level.flags.uploaded) {
            button.interactable = false;
            button.image.color = button.image.color.WithAlpha(0);
            return;
        }
        else {
            button.interactable = true;
            button.image.color = button.image.color.WithAlpha(1);
        }

        SetImage(level.flags.GetBookmarked());
    }


    private void SetImage(bool isBookmarked) {
        button.image.sprite = isBookmarked ? bookmarkedSprite : unbookmarkedSprite;
    }


    public void ToggleBookmarked() {
        bool isBookmarked = !level.flags.GetBookmarked();
        level.flags.SetBookmarked(isBookmarked);

        SetImage(isBookmarked);

        flagsDisplay?.SetFlags(level.flags);
    }
}
