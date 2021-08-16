using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class ReleaseTrailer : MonoBehaviour
{
    public SpriteRenderer ghost;
    public GameObject textOne;
    public GameObject textTwo;
    public GameObject textThree;
    public Camera cam;
    public GameObject part;

    private int frame = -1; 
    
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space)) {
            switch (frame) {
                case -1:
                    ghost.DOFade(0, 1f);
                    break;
                case 0:
                    textOne.SetActive(true);
                    break;
                case 1:
                    textTwo.SetActive(true);
                    break;
                case 2:
                    textThree.SetActive(true);
                    break;
                case 3:
                    cam.backgroundColor = Color.white;
                    cam.DOColor(new Color(0.3f, 0.3f, 0.6f), 1f);
                    part.SetActive(true);
                    break;  
            }

            frame++;
        }
    }
}
