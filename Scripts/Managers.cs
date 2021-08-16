using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

// a singleton!
public class Managers : MonoBehaviour
{
    public static Managers instance;
    public static Managers Instance { get { return instance; }}

    public AudioManager audioManager;
    public static AudioManager AudioManager { get { return instance?.audioManager; }}

    public FileManager fileManager;
    public static FileManager FileManager { get { return instance?.fileManager; }}

    public ScenesManager scenesManager;
    public static ScenesManager ScenesManager { get { return instance?.scenesManager; }}

    public AlertManager alertManager;
    public static AlertManager AlertManager { get { return instance?.alertManager; }}


    private void Awake() {
        if (instance == null) {
            instance = this;
        } else if (instance != this) {
            Destroy(gameObject);
        }

        DontDestroyOnLoad(gameObject);

        // more tweening spaces!
        DOTween.SetTweensCapacity(2000, 50);
    }
}
