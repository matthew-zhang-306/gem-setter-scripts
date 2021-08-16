using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventManagerSingleton : MonoBehaviour
{
    private static EventManagerSingleton instance;

    private void Awake() {
        if (instance == null) {
            instance = this;
        } else if (instance != this) {
            Destroy(gameObject);
        }

        DontDestroyOnLoad(gameObject);
    }
}
