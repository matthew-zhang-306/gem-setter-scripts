using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuButtonEdgeParticles : MonoBehaviour
{
    public Image buttonGraphic;
    public ParticleSystem particles;

    private void Update() {
        particles.SetColor(buttonGraphic.color);
    }
}
