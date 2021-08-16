using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class LevelSlots : MonoBehaviour
{
    public GameObject buttonsPanel;

    [System.Serializable] public class SlotSelectedEvent : UnityEvent<LevelSlotButton> {}
    [SerializeField] public SlotSelectedEvent OnSlotSelected;

    private Dictionary<int, LevelSlotButton> levelDict;
    private Dictionary<int, LevelSlotButton> defaultLevelDict;
    public LevelSlotButton firstEmpty { get; private set; }


    [Header("Selection")]
    public bool canSelectEmpty;
    public bool canSelectDefault;


    [Header("Default Levels")]
    public int numDefaultLevels;
    public int firstDefaultBuildIndex;


    private void Awake() {
        Reset();
    }

    public void Reset() {
        levelDict = new Dictionary<int, LevelSlotButton>();
        defaultLevelDict = new Dictionary<int, LevelSlotButton>();

        firstEmpty = null;
        int number = 0;

        foreach (Transform button in buttonsPanel.transform) {
            LevelSlotButton levelButton = button.GetComponent<LevelSlotButton>();

            if (number >= numDefaultLevels) {
                levelButton.Init(number - numDefaultLevels + 1, this);
            
                if (firstEmpty == null && !levelButton.IsEmpty) {
                    firstEmpty = levelButton;
                }

                levelDict.Add(number - numDefaultLevels + 1, levelButton);
            }
            else {
                levelButton.Init(firstDefaultBuildIndex + number, this, true);
                defaultLevelDict.Add(number + 1, levelButton);
            }

            number++;
        }
    }


    public LevelSlotButton GetLevelButton(int number, bool isDefault = false) {
        if (!isDefault && levelDict.ContainsKey(number)) {
            return levelDict[number];
        }
        else if (isDefault && defaultLevelDict.ContainsKey(number)) {
            return defaultLevelDict[number];
        }

        // this level doesn't exist
        return null;
    }
}
