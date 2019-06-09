using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

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
        StartCoroutine(PreLoadReadyScene(1));
    }

    public void LoadScene(Scene scene) {
        int numberOfScene = -1;
        switch (scene) {
            /*case Scene.MAIN_SCENE:
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
            */
            case Scene.MAIN_SCENE :
                numberOfScene = 2;
                break;
            case Scene.PVP_READY_SCENE :
                numberOfScene = 3;
                break;
            case Scene.CONNECT_MATCHING_SCENE :
                numberOfScene = 4;
                break;
            case Scene.MISSION_INGAME :
                numberOfScene = 1;
                break;
        }
        var currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        QualitySettings.asyncUploadTimeSlice = 4;
        StartCoroutine(LoadReadyScene(currentScene.buildIndex, numberOfScene));
        QualitySettings.asyncUploadTimeSlice = 2;
    }

    AsyncOperation asyncOp;

    IEnumerator PreLoadReadyScene(int load) {
        yield return null;
        asyncOp = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(load, UnityEngine.SceneManagement.LoadSceneMode.Additive);
        asyncOp.allowSceneActivation = false;
        yield return null;
    }

    IEnumerator LoadReadyScene(int unload, int load) {
        yield return null;
        while(!asyncOp.isDone) {
            asyncOp.allowSceneActivation = true;
            yield return null;
        }
        UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(unload);
        yield return PreLoadReadyScene(load);
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
