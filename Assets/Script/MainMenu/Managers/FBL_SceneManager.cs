using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FBL_SceneManager : Singleton<FBL_SceneManager> {
    private float secondsToLoadNextScene = 0.5f;
    private static int lastScene;
    private int mainScene = 1;
    private int currentScene;

    public static Stack<int> sceneStack = new Stack<int>();

    private void Awake() {
        currentScene = SceneManager.GetActiveScene().buildIndex;
    }

    void Start() {
#if !UNITY_EDITOR
        //for(int i = 2; i <= 5; i++) {
        //    StartCoroutine(PreLoadReadyScene(i));
        //}
#endif
        DontDestroyOnLoad(this);
    }

    public void LoadScene(Scene scene) {
        int numberOfScene = -1;
        switch (scene) {
            case Scene.LOGIN:
                numberOfScene = 1;
                break;
            case Scene.MAIN_SCENE:
                numberOfScene = 2;
                break;
            case Scene.LOADING_SCENE:
                numberOfScene = 3;
                break;
            case Scene.CONNECT_MATCHING_SCENE:
                numberOfScene = 4;
                break;
            case Scene.MISSION_INGAME:
                numberOfScene = 5;
                break;
            case Scene.TUTORIAL:
                numberOfScene = 6;
                break;
            case Scene.DICTIONARY_SCENE:
                numberOfScene = 7;
                break;
        }

        var currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        QualitySettings.asyncUploadTimeSlice = 4;
        StartCoroutine(LoadReadyScene(currentScene.buildIndex, numberOfScene));
        QualitySettings.asyncUploadTimeSlice = 2;
        EscapeKeyController.escapeKeyCtrl.ResetEscapeList();
    }

    AsyncOperation[] asyncOps = new AsyncOperation[7];

    IEnumerator PreLoadReadyScene(int load) {
        yield return null;
        if (load - 2 < 0) yield break;
        asyncOps[load - 2] = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(load, UnityEngine.SceneManagement.LoadSceneMode.Single);
        asyncOps[load - 2].allowSceneActivation = false;
        yield return null;
    }

    IEnumerator LoadReadyScene(int unload, int load) {
        yield return null;
        if (unload - 2 >= 0)
            asyncOps[unload - 2] = null;
        if (asyncOps[load - 2] == null) {
            UnityEngine.SceneManagement.SceneManager.LoadScene(load, UnityEngine.SceneManagement.LoadSceneMode.Single);
            if(load == 2)
                UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(7, UnityEngine.SceneManagement.LoadSceneMode.Additive);
            for (int i = 0; i < 7; i++) {
                asyncOps[i] = null;
            }
            yield break;
        }
        while (!asyncOps[load - 2].isDone) {
            asyncOps[load - 2].allowSceneActivation = true;
            yield return null;
        }
    }
    

    public float LoadingProgress() {
        if (asyncOps[2] == null) return 0f;
        return asyncOps[2].progress;
    }

    public enum Scene {
        MAIN_SCENE,
        MISSION_INGAME,
        DECK_LIST_SCNE,
        DECK_SETTING_SCENE,
        CONNECT_MATCHING_SCENE,
        LOADING_SCENE,
        LOGIN,
        TUTORIAL,
        DICTIONARY_SCENE
    }
}
