using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// couldn't call this "SceneManager" so this will have to do
public class ScenesManager : MonoBehaviour
{
    // information to pass on to the next scene goes here.
    private string transitionTag;
    private bool isTransitionTagSet;

    [SerializeField] private ScreenTransition transition = default;
    [SerializeField] private LoadingPanel loading = default;

    [Header("Scene Data")]
    [SerializeField] private int mainMenuIndex;
    public int MainMenuIndex { get { return mainMenuIndex; }}


    private bool onMainMenu;

    public bool isTransitioning { get; private set; }
    private Coroutine loadAsyncCoroutine;

    private void Awake() {
        OnSceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);
    }

    private void OnEnable() {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    private void OnDisable() {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode) {
        transition.TransitionIntoScene();
        isTransitioning = false;

        onMainMenu = scene.buildIndex == mainMenuIndex;

        isTransitionTagSet = false;
    }


    private float returnToMenuTimer;

    private void Update() {
        if (isTransitioning || onMainMenu) {
            return;
        }

        returnToMenuTimer += Input.GetKey(KeyCode.Escape) ? Time.deltaTime : -2 * Time.deltaTime;
        returnToMenuTimer = Mathf.Max(returnToMenuTimer, 0);

        if (returnToMenuTimer > 0.5f) {
            GoToScene(mainMenuIndex);
        }
    }


    public string CurrentScene => SceneManager.GetActiveScene().name;


    public string GetTransitionTag() {
        return transitionTag;
    }
    public void SetTransitionTag(string tag) {
        transitionTag = tag;
        isTransitionTagSet = true;
    }


    public void GoToScene(int buildIndex) {
        if (isTransitioning) {
            return;
        }

        if (!isTransitionTagSet) {
            // reset transition tag
            transitionTag = "";
        }

        isTransitioning = true;
        transition.TransitionOutOfScene(() => LoadScene("", buildIndex));
    }

    public void GoToScene(string name) {
        if (isTransitioning) {
            return;
        }

        if (!isTransitionTagSet) {
            // reset transition tag
            transitionTag = "";
        }

        isTransitioning = true;
        transition.TransitionOutOfScene(() => LoadScene(name, 0));
    }

    public void GoToSceneImmediate(int buildIndex) {
        if (isTransitioning) {
            return;
        }

        // if there is an existing fade in happening right now, kill it
        transition.KillTransition();

        if (!isTransitionTagSet) {
            // reset transition tag
            transitionTag = "";
        }

        isTransitioning = true;
        LoadScene("", buildIndex); // call this method directly
    }

    public void GoToSceneImmediate(string sceneName) {
        if (isTransitioning) {
            return;
        }

        // if there is an existing fade in happening right now, kill it
        transition.KillTransition();

        if (!isTransitionTagSet) {
            // reset transition tag
            transitionTag = "";
        }

        isTransitioning = true;
        LoadScene(sceneName, 0); // call this method directly
    }


    public void Quit() {
        if (isTransitioning) {
            return;
        }

        isTransitioning = true;
        transition.TransitionOutOfScene(() => Application.Quit());
    }


    private void LoadScene(string sceneName, int buildIndex) {
        if (loadAsyncCoroutine != null) {
            StopCoroutine(loadAsyncCoroutine);
        }
        loadAsyncCoroutine = StartCoroutine(LoadSceneAsync(sceneName, buildIndex));
    }

    private IEnumerator LoadSceneAsync(string sceneName, int buildIndex) {
        loading.Show();
        yield return (sceneName.Length > 0 ? SceneManager.LoadSceneAsync(sceneName) : SceneManager.LoadSceneAsync(buildIndex));
        loading.Hide();
        loadAsyncCoroutine = null;
    }
}
