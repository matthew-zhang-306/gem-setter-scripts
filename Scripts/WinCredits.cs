using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WinCredits : MonoBehaviour
{
    public WinTile winTile;
    public GameObject creditsSequenceObject;

    public static EmptyDelegate OnWinCredits;

    private void OnEnable()
    {
        winTile.WinEvent.AddListener(SpawnCreditsSequence);
    }

    private void OnDisable() {
        winTile.WinEvent.RemoveListener(SpawnCreditsSequence);
    }

    
    private void SpawnCreditsSequence() {
        OnWinCredits?.Invoke();
        GameObject.Instantiate(creditsSequenceObject, Vector3.zero, Quaternion.identity);
    }
}
