using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class FblTextConverter : MonoBehaviour {
    public string category;
    public string key;
    public string basicText;

    public TextType type;
    void Awake() {
        RefreshText();
    }

    public void Init(string category, string key, TextType type) {
        this.category = category;
        this.key = key;
        this.type = type;
    }
    
    public void SetFont(ref TextMeshProUGUI textComp, bool isBold = true) {
        AccountManager accountManager = AccountManager.Instance;
        var language = accountManager.GetLanguageSetting();
        switch (language) {
            case "Korean":
                textComp.font = isBold ? accountManager.resource.tmp_fonts["Korean_Bold"] : accountManager.resource.tmp_fonts["Korean_Regular"];
                break;
            case "English":
                textComp.font = isBold ? accountManager.resource.tmp_fonts["English_Bold"] : accountManager.resource.tmp_fonts["English_Regular"];
                break;
        }
    }

    public void SetFont(ref Text textComp, bool isBold = true) {
        AccountManager accountManager = AccountManager.Instance;
        var language = accountManager.GetLanguageSetting();
        switch (language) {
            case "Korean":
                textComp.font = isBold ? accountManager.resource.fonts["Korean_Bold"] : accountManager.resource.fonts["Korean_Regular"];
                break;
            case "English":
                textComp.font = isBold ? accountManager.resource.fonts["English_Bold"] : accountManager.resource.fonts["English_Regular"];
                break;
        }
    }

    public virtual void RefreshText() {

        AccountManager accountManager = AccountManager.Instance;
        ResourceManager resourceManager = accountManager.resource;
        var languageSetting = accountManager.GetLanguageSetting();

        switch (type) {
            case TextType.TEXTMESHPROUGUI:
                try {
                    var tmProComp = GetComponent<TextMeshProUGUI>();

                    if (tmProComp.font.name.Contains("Regular")) tmProComp.font = resourceManager.tmp_fonts[languageSetting + "_Regular"];
                    else if (tmProComp.font.name.Contains("Bold")) tmProComp.font = resourceManager.tmp_fonts[languageSetting + "_Bold"];
                }
                catch (Exception ex) {
                    Logger.Log("TextMeshProUGUI 컴포넌트를 찾을 수 없습니다. \n 대상 : " + transform.parent.name);
                }
                break;
            case TextType.UGUITEXT:
                try {
                    var textComp = GetComponent<Text>();
                    if (textComp.font.name.Contains("Regular")) textComp.font = resourceManager.fonts[languageSetting + "_Regular"];
                    else if (textComp.font.name.Contains("Bold")) textComp.font = resourceManager.fonts[languageSetting + "_Bold"];
                }
                catch (Exception ex) {
                    Logger.Log("Text 컴포넌트를 찾을 수 없습니다. \n 대상 : " + transform.parent.name);
                }
                break;

            case TextType.UGUIIMAGE:
                break;
        }

        if (string.IsNullOrEmpty(category) || string.IsNullOrEmpty(key)) return;

        switch (type) {
            case TextType.TEXTMESHPROUGUI:
                try {
                    var result = AccountManager.Instance.GetComponent<Fbl_Translator>().GetLocalizedText(category, key);
                    if (string.IsNullOrEmpty(result)) return;
                    result = result.Replace("\\n", "\n");

                    var tmProComp = GetComponent<TextMeshProUGUI>();

                    tmProComp.text = result;
                    basicText = result;
                }
                catch(Exception ex) {
                    Logger.Log("TextMeshProUGUI 컴포넌트를 찾을 수 없습니다. \n 대상 : " + transform.parent.name);
                }
                break;
            case TextType.UGUITEXT:
                try {
                    var result = AccountManager.Instance.GetComponent<Fbl_Translator>().GetLocalizedText(category, key);
                    if (string.IsNullOrEmpty(result)) return;
                    result = result.Replace("\\n", "\n");

                    var textComp = GetComponent<Text>();

                    textComp.text = result;
                    basicText = result;
                }
                catch(Exception ex) {
                    Logger.Log("Text 컴포넌트를 찾을 수 없습니다. \n 대상 : " + transform.parent.name);
                }
                break;

            case TextType.UGUIIMAGE:
                try {
                    Image Image = GetComponent<Image>();
                    Image.sprite = resourceManager.localizeImage[languageSetting + '_' + key];

                    Button button = GetComponent<Button>();

                    if (button != null) {
                        SpriteState state = button.spriteState;
                        state.pressedSprite = resourceManager.localizeImage[languageSetting + '_' + key + '_' + "Press"];
                        button.spriteState = state;
                    }
                }
                catch(Exception ex) {
                    Logger.Log("IMAGE 컴포넌트를 찾을 수 없습니다. \n 대상 : " + transform.parent.name);
                }

                break;
        }
    }

    public void InsertText(ReplacePair pair) {
        string temp = basicText;
        if (temp.Contains(pair.prevStr))
            temp = temp.Replace(pair.prevStr, pair.newStr);
        
        __InsertText(type, temp);
    }

    public void InsertText(List<ReplacePair> pairs) {
        string temp = basicText;
        foreach (var pair in pairs) {
            if (temp.Contains(pair.prevStr))
                temp = temp.Replace(pair.prevStr, pair.newStr);
        }
        
        __InsertText(type, temp);
    }

    private void __InsertText(TextType type, string text) {
        switch (type) {
            case TextType.TEXTMESHPROUGUI:
                var tmProComp = GetComponent<TextMeshProUGUI>();
                tmProComp.text = text;
                break;
            case TextType.UGUITEXT:
                var textComp = GetComponent<Text>();
                textComp.text = text;
                break;
        }
    }

    public enum TextType {
        UGUITEXT,
        TEXTMESHPROUGUI,
        UGUIIMAGE
    }

    public class ReplacePair {
        public string prevStr;    //바꾸기 이전 괄호 형식
        public string newStr;     //대체할 Text

        public ReplacePair(string prevStr, string newStr) {
            this.prevStr = prevStr;
            this.newStr = newStr;
        }
    }
}
