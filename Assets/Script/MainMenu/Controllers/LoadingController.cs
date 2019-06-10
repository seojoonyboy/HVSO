using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class LoadingController : MonoBehaviour {

    [SerializeField] private Slider slider;
    SceneManager manager;
    
    IEnumerator Start() {
        yield return null;
        manager = SceneManager.Instance;
        manager.LoadScene(SceneManager.Scene.PVP_READY_SCENE);
    }

    void Update() {
        if(manager == null) return;
        slider.value = manager.LoadingProgress();
    }
}
