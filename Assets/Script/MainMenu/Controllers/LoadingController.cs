using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class LoadingController : MonoBehaviour {

    [SerializeField] private Slider slider;
    FBL_SceneManager manager;
    
    IEnumerator Start() {
        yield return null;
        manager = FBL_SceneManager.Instance;
        manager.LoadScene(FBL_SceneManager.Scene.PVP_READY_SCENE);
    }

    void Update() {
        if(manager == null) return;
        slider.value = manager.LoadingProgress();
    }
}
