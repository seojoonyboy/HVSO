using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DictionaryWindow : MainWindowBase {
    [SerializeField] private MenuSceneController _menuSceneController;
    
    private void Awake() {
        pageName = "DictionaryWindow";
    }

    public override void OnPageLoaded() {
        _menuSceneController.SetCardNumbersPerDic();
    }
}