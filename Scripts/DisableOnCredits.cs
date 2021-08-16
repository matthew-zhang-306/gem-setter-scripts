using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisableOnCredits : MonoBehaviour
{
    public UIAnimator animator;

    private void OnEnable() {
        CreditsSequence.OnStart += Disable;
    }
    private void OnDisable() {
        CreditsSequence.OnStart -= Disable;
    }

    private void Disable() {
        animator.Exit();
    }
}
