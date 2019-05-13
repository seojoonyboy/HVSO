using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneManager : Singleton<SceneManager> {
    // Start is called before the first frame update
    void Start() {
        DontDestroyOnLoad(this);
    }

    // Update is called once per frame
    void Update() {

    }

    public void LoadScene(Scene scene) {
        string str = null;
        switch (scene) {
            case Scene.MAIN_SCENE:
                str = "MenuScene";
                break;
            case Scene.MISSION_SELECT_SCENE:
                str = "MissionSelectScene";
                break;
            case Scene.COLLECTION_SCENE:
                str = "CollectionScene";
                break;
        }
        if(str != null) {
            UnityEngine.SceneManagement.SceneManager.LoadScene(str);
        }
    }

    public enum Scene {
        MAIN_SCENE,
        COLLECTION_SCENE,
        MISSION_SELECT_SCENE
    }
}
