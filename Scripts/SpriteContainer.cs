using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using DG.Tweening;

public class SpriteContainer : MonoBehaviour
{
    public List<SpriteRenderer> spriteRenderers;
    public List<SpriteRenderer> flashRenderers;

    public Animator animator;

    private Tween bumpTween;
    private Tween shakeTween;


    public UnityEngine.Events.UnityEvent OnPop;


    private void Start() {
        foreach (SpriteRenderer flash in flashRenderers) {
            flash.color = Color.white.WithAlpha(0);
        }
    }

    public void Pop() {
        if (animator != null) {
            animator.SetTrigger("Pop" + Random.Range(0, 2));
            OnPop?.Invoke();
        }
    }


    public void SetColor(Color color) {
        foreach (SpriteRenderer sprite in spriteRenderers) {
            sprite.color = color.WithAlpha(sprite.color.a);
        }
    }

    public void Flash() {
        foreach (SpriteRenderer flash in flashRenderers) {
            flash.DOKill();
            flash.color = Color.white;
            flash.DOFade(0, 0.3f);
        }
    }


    public void Shake() {
        shakeTween?.Complete();
        transform.localPosition = Vector3.zero;
        shakeTween = transform.DOShakePosition(0.3f, 0.1f, 100);
    }

    public void Bump(Vector2 direction) {
        bumpTween?.Complete();
        transform.localPosition = direction.ToVector3() * 0.2f;
        bumpTween = transform.DOLocalMove(Vector3.zero, 0.3f).SetEase(Ease.OutCubic);
    }
}
