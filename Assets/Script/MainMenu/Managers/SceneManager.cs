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
            UnityEngine.SceneManagement.SceneManager.LoadScene(numberOfScene);
        }
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
