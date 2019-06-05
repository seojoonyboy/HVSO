using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneManager : Singleton<SceneManager> {
    private float secondsToLoadNextScene = 0.5f;
    private static int lastScene;
    private int mainScene = 1;
    private int currentScene;

    public static Stack<int> sceneStack = new Stack<int>();

    private void Awake() {
        currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex;
    }

    // Start is called before the first frame update
    void Start() {
        DontDestroyOnLoad(this);
    }

    // Update is called once per frame
    void Update() {
        BackButtonPressed();
    }

    public void LoadScene(Scene scene) {
        int numberOfScene = -1;
        switch (scene) {
            case Scene.MAIN_SCENE:
                numberOfScene = 1;
                break;
            case Scene.MISSION_SELECT_SCENE:
                numberOfScene = 2;
                break;
            case Scene.COLLECTION_SCENE:
                numberOfScene = 6;
                break;
            case Scene.MISSION_INGAME:
                numberOfScene = 3;
                break;
            case Scene.PVP_READY_SCENE:
                numberOfScene = 4;
                break;
            case Scene.DECK_LIST_SCNE:
                numberOfScene = 5;
                break;
            case Scene.DECK_SETTING_SCENE:
                numberOfScene = 7;
                break;
            case Scene.CONNECT_MATCHING_SCENE:
                numberOfScene = 8;
                break;
        }

        if(numberOfScene != -1) {
            try {
                Fader fader = GameObject.Find("FaderCanvas/Fader").GetComponent<Fader>();
                fader.StartFade(Fader.FadeDirection.In, delegate {
                    LoadNextScene(numberOfScene);
                });
            }
            catch(NullReferenceException ex) {
                LoadNextScene(numberOfScene);
            }
        }
    }

    public void LoadNextScene(int numberOfSceneToLoad) {
        StartCoroutine(LoadScene(numberOfSceneToLoad));
    }

    private IEnumerator LoadScene(int numberOfScene) {
        SetLastScene(currentScene);

        yield return new WaitForSeconds(secondsToLoadNextScene);
        LoadNewScene(numberOfScene);
    }

    public void BackButtonPressed() {
        if (Input.GetKey(KeyCode.Escape) && currentScene > mainScene) {
            if (lastScene == 0) {
                Debug.Log("Last scene was Splash Screen so load Main Scene instead.");
                UnityEngine.SceneManagement.SceneManager.LoadScene(mainScene);
            }
            else {
                LoadLastScene();
                if(PlayMangement.instance == null) return;
                if(PlayMangement.instance.socketHandler == null) return;
                DestroyImmediate(PlayMangement.instance.socketHandler.gameObject);
            }
        }
    }

    public void LoadNewScene(int sceneToLoad) {
        currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex;
        sceneStack.Push(currentScene);
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneToLoad);
    }

    public void LoadLastScene() {
        lastScene = sceneStack.Pop();
        UnityEngine.SceneManagement.SceneManager.LoadScene(lastScene);
    }

    public static void SetLastScene(int makeCurrentSceneTheLastScene) {
        lastScene = makeCurrentSceneTheLastScene;
    }

    public static int GetLastScene() {
        return lastScene;
    }

    public enum Scene {
        MAIN_SCENE,
        COLLECTION_SCENE,
        MISSION_SELECT_SCENE,
        MISSION_INGAME,
        PVP_READY_SCENE,
        DECK_LIST_SCNE,
        DECK_SETTING_SCENE,
        CONNECT_MATCHING_SCENE
    }
}
