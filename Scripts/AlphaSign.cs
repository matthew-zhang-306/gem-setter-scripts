using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class AlphaSign : MonoBehaviour
{
    public PlayerController player;

    // Start is called before the first frame update
    void Start()
    {
        GetComponent<SpriteRenderer>().color = Color.white.WithAlpha(0);
        transform.rotation = Quaternion.Euler(0, 0, -140);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space)) {
            player.spriteContainer.Pop();
            player.playerAnimation.Win();

            GetComponent<SpriteRenderer>().DOFade(1, 0.5f);
            transform.DORotate(new Vector3(0, 0, -10), 0.8f).SetEase(Ease.OutBack);
        }
    }
}
