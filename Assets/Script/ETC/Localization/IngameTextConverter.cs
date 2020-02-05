using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class IngameTextConverter : FblTextConverter {

    // ui이면 category는 필요없습니다.
    private void Awake() {
        
    }

    // Start is called before the first frame update
    void Start()
    {
        RefreshText();
    }

    // Update is called once per frame

    public override void RefreshText() {
        if (string.IsNullOrEmpty(key)) return;
        string result;
        if (PlayMangement.instance.uiLocalizeData.ContainsKey(key)) 
            result = PlayMangement.instance.uiLocalizeData[key];        
        else 
            result = (string.IsNullOrEmpty(category)) ? "" : AccountManager.Instance.GetComponent<Fbl_Translator>().GetLocalizedText(category, key);       
        if (string.IsNullOrEmpty(result)) return;
        result = result.Replace("\\n", "\n");

        switch (type) {
            case TextType.TEXTMESHPROUGUI:
                try {
                    GetComponent<TextMeshProUGUI>().text = result;
                }
                catch (Exception ex) {
                    Logger.Log("TextMeshProUGUI 컴포넌트를 찾을 수 없습니다. \n 대상 : " + transform.parent.name);
                }
                break;
            case TextType.UGUITEXT:
                try {
                    GetComponent<Text>().text = result;
                }
                catch (Exception ex) {
                    Logger.Log("Text 컴포넌트를 찾을 수 없습니다. \n 대상 : " + transform.parent.name);
                }
                break;
        }
    }
}
