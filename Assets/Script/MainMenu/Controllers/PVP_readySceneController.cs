using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PVP_readySceneController : MonoBehaviour {
    // Start is called before the first frame update
    void Start() {

    }

    // Update is called once per frame
    void Update() {

    }

    public void OnStartButton() {
        SceneManager.Instance.LoadScene(SceneManager.Scene.MISSION_INGAME);
    }

    public void OnBackButton() {
        SceneManager.Instance.LoadScene(SceneManager.Scene.MAIN_SCENE);
    }
}
