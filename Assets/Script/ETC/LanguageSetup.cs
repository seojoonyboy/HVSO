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

    Color32 deactiveColor = new Color32(100, 100, 100, 255);
    Color32 activeColor = new Color32(255, 255, 255, 255);

    public void Init() {
        ButtonInit();
    }

    private void ButtonInit() {
        languageChangeBtn.onClick.RemoveAllListeners();
        languageChangeBtn.onClick.AddListener(OnLanguageButtonClick);
    }

    private void OnLanguageButtonClick() {
        languageSelectModal.SetActive(true);

        var prevLanguage = PlayerPrefs.GetString("Language", AccountManager.Instance.GetLanguageSetting());
        foreach (Transform button in languageSelectModal.transform.Find("InnerModal/Buttons")) {
            if (button.name == "NotReady") continue;

            if (prevLanguage != button.name) {
                button.GetComponent<Image>().color = deactiveColor;
                button.transform.Find("Text").GetComponent<TMPro.TextMeshProUGUI>().color = deactiveColor;
            }
            else {
                button.GetComponent<Image>().color = activeColor;
                button.transform.Find("Text").GetComponent<TMPro.TextMeshProUGUI>().color = activeColor;
            }
        }
    }
}
