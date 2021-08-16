using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EditorBlockExtendBar : MonoBehaviour
{
    public UIAnimator animator;

    public void MoveOffScreen() {
        animator.Exit();
    }
    public void MoveOnScreen() {
        animator.Enter();
    }
}
