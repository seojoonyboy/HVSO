using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 메인화면 설정모달에서의 언어설정 변경처리
/// </summary>
[Serializable]
public class LanguageSetup {
    public Button languageChangeBtn;
    public GameObject languageSelectModal;

    public void Init() {
        ButtonInit();
    }

    private void ButtonInit() {
        languageChangeBtn.onClick.RemoveAllListeners();
        languageChangeBtn.onClick.AddListener(OnLanguageButtonClick);
    }

    private void OnLanguageButtonClick() {
        languageSelectModal.SetActive(true);
    }
}
