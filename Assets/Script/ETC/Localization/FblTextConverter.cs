using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class FblTextConverter : MonoBehaviour {
    public string category;
    public string key;

    public TextType type;
    void Awake() {
        RefreshText();
    }

    public void Init(string category, string key, TextType type) {
        this.category = category;
        this.key = key;
        this.type = type;
    }

    public virtual void RefreshText() {
        if (string.IsNullOrEmpty(category) || string.IsNullOrEmpty(key)) return;

        AccountManager accountManager = AccountManager.Instance;
        ResourceManager resourceManager = accountManager.resource;
        var languageSetting = accountManager.GetLanguageSetting();

        var result = AccountManager.Instance.GetComponent<Fbl_Translator>().GetLocalizedText(category, key);
        if (string.IsNullOrEmpty(result)) return;
        result = result.Replace("\\n", "\n");

        switch (type) {
            case TextType.TEXTMESHPROUGUI:
                try {
                    var tmProComp = GetComponent<TextMeshProUGUI>();
                    if (tmProComp.font.name.Contains("Regular")) tmProComp.font = resourceManager.tmp_fonts[languageSetting + "_Regular"];
                    else if (tmProComp.font.name.Contains("Bold")) tmProComp.font = resourceManager.tmp_fonts[languageSetting + "_Bold"];
                    
                    tmProComp.text = result;
                }
                catch(Exception ex) {
                    Logger.Log("TextMeshProUGUI 컴포넌트를 찾을 수 없습니다. \n 대상 : " + transform.parent.name);
                }
                break;
            case TextType.UGUITEXT:
                try {
                    var textComp = GetComponent<Text>();
                    if (textComp.font.name.Contains("Regular")) textComp.font = resourceManager.fonts[languageSetting + "_Regular"];
                    else if (textComp.font.name.Contains("Bold")) textComp.font = resourceManager.fonts[languageSetting + "_Bold"];

                    textComp.text = result;
                }
                catch(Exception ex) {
                    Logger.Log("Text 컴포넌트를 찾을 수 없습니다. \n 대상 : " + transform.parent.name);
                }
                break;
        }
    }

    public enum TextType {
        UGUITEXT,
        TEXTMESHPROUGUI
    }
}
