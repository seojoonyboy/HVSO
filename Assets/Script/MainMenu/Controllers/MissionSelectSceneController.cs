using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MissionSelectSceneController : MonoBehaviour {
    [SerializeField] GameObject Fader;
    [SerializeField] Transform stageParent;

    // Start is called before the first frame update
    void Start() {
        Fader.GetComponent<Fader>().StartFade(global::Fader.FadeDirection.Out, OnFadeFinished);
    }

    private void OnFadeFinished() {

    }

    // Update is called once per frame
    void Update() {

    }

    public void OnMissionStart() {
        SceneManager.Instance.LoadScene(SceneManager.Scene.MISSION_INGAME);
    }

    public void OnBackBtn() {
        SceneManager.Instance.LoadScene(SceneManager.Scene.MAIN_SCENE);
    }
}
