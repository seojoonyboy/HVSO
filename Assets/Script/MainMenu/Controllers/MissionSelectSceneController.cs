using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MissionSelectSceneController : MonoBehaviour {
    [SerializeField] GameObject Fader;

    // Start is called before the first frame update
    void Start() {
        Fader.GetComponent<Fader>().StartFade(global::Fader.FadeDirection.Out, OnFadeFinished);
    }

    private void OnFadeFinished() {

    }

    // Update is called once per frame
    void Update() {

    }
}
