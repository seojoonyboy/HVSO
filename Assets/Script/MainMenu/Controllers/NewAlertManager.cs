using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
using System.IO;
using Sirenix.OdinInspector;
using System;
using System.Linq;
using UnityEngine.UI.Extensions;

/// <summary>
/// 메인화면의 Alert를 관리함
/// </summary>
public class NewAlertManager : SerializedMonoBehaviour {
    public static NewAlertManager Instance;
    public GameObject alertPref, alertWithNumPref;
    public List<string> buttonName;
    public Dictionary<ButtonName, GameObject> buttonDic;
    public Dictionary<ButtonName, GameObject> referenceToInit;    //처음 초기화를 위한 참조

    public string playerPrefKey = "AlertIcon";
    string fileName = "AlertList.csv";

    NoneIngameSceneEventHandler eventHandler;
    private void Awake() {
        Instance = this;
        eventHandler = NoneIngameSceneEventHandler.Instance;
    }

    private void OnDestroy() {
        if (Instance != null)
            Instance = null;
        if (buttonName.Count > 0) {
            SaveButtonName();
            buttonName.Clear();
        }       
    }

    private void SaveButtonName() {
        string nameData = "";
        for(int i = 0; i < buttonName.Count; i++) {
            nameData += buttonName[i];
            nameData += '/';
        }
        PlayerPrefs.SetString(playerPrefKey, nameData);
    }

    private void LoadButtonName() {
        string nameData = PlayerPrefs.GetString(playerPrefKey);
        string[] data = nameData.Split('/');
        buttonName.AddRange(data);
    }

    public void SetUpButtonToAlert(GameObject button, ButtonName enumName) {
        if (buttonDic == null) buttonDic = new Dictionary<ButtonName, GameObject>();

        if (buttonDic.ContainsKey(enumName)) return;
        
        buttonDic.Add(enumName, button);
        WriteCsvFile();

        GameObject alert = Instantiate(alertPref);
        alert.transform.SetParent(button.transform);
        alert.name = "alert";
        alert.transform.SetAsLastSibling();
        RectTransform rect = alert.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(1, 1);
        rect.anchorMax = new Vector2(1, 1);
        rect.offsetMax = new Vector2(-20f, 0);
        rect.offsetMin = new Vector2(-20f, 0);

        if(enumName == ButtonName.MODE) {
            alert.transform.SetParent(button.transform.Find("AlertPos"));

            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.offsetMax = new Vector2(0, 0);
            rect.offsetMin = new Vector2(0, 0);
        }

        button.GetComponent<Button>()
            .OnClickAsObservable()
            .First()
            .Subscribe(_ => DisableButtonToAlert(button, enumName)).AddTo(button);

        button.SetActive(true);
    }

    public void SetupButtonToAlertWithNum(GameObject button, ButtonName enumName, int initNum) {
        if (buttonDic == null) buttonDic = new Dictionary<ButtonName, GameObject>();
        if (buttonDic.ContainsKey(enumName)) DisableButtonToAlert(button, enumName);

        buttonDic.Add(enumName, button);

        GameObject alert = Instantiate(alertWithNumPref);
        alert.transform.SetParent(button.transform);
        alert.name = "alert";
        alert.transform.SetAsLastSibling();
        RectTransform rect = alert.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(1, 1);
        rect.anchorMax = new Vector2(1, 1);
        rect.offsetMax = new Vector2(-20f, 0);
        rect.offsetMin = new Vector2(-20f, 0);

        alert.transform.Find("BoxNum").GetComponent<TMPro.TextMeshProUGUI>().text = initNum.ToString();
    }
    
    private void DisableButtonToAlert(GameObject button, ButtonName enumName) {
        if (!buttonDic.ContainsKey(enumName)) return;

        if (enumName == ButtonName.MODE) {
            Destroy(button.transform.Find("AlertPos/alert").gameObject);
        }
        else {
            Destroy(button.transform.Find("alert").gameObject);
        }

        buttonDic.Remove(enumName);
        WriteCsvFile();
    }

    public void WriteCsvFile() {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        if(buttonDic != null && buttonDic.Count > 0) {
            var last = buttonDic.Last();
            foreach (KeyValuePair<ButtonName, GameObject> dic in buttonDic) {
                if (dic.Equals(last)) {
                    sb.Append(dic.Key);
                }
                else sb.Append(dic.Key + ",");
            }
        }

        string dir = string.Empty;

        if (Application.platform == RuntimePlatform.Android) {
            dir = Application.persistentDataPath;
        }
        else if (Application.platform == RuntimePlatform.IPhonePlayer) {
            dir = Application.persistentDataPath;
        }
        else {
            dir = Application.streamingAssetsPath;
        }

        string filePath = dir + "/" + fileName;
        File.WriteAllBytes(
            filePath,
            System.Text.Encoding.UTF8.GetBytes(sb.ToString())
        );
    }

    /// <summary>
    /// 메인 화면 왔을 때 초기화
    /// </summary>
    public void Initialize() {
        var pathToCsv = string.Empty;

        if (Application.platform == RuntimePlatform.Android) {
            pathToCsv = Application.persistentDataPath + "/" + fileName;
        }
        else if (Application.platform == RuntimePlatform.IPhonePlayer) {
            pathToCsv = Application.persistentDataPath + "/" + fileName;
        }
        else {
            pathToCsv = Application.streamingAssetsPath + "/" + fileName;
        }

        if (!File.Exists(pathToCsv)) {
            WriteCsvFile();
            Initialize();
            return;
        }

        var lines = File.ReadAllText(pathToCsv);
        if (string.IsNullOrEmpty(lines)) return;
        string[] keys = lines.Split(',');
        foreach(string key in keys) {
            var enumVal = (ButtonName)(Enum.Parse(typeof(ButtonName), key));
            Logger.Log("Alert Key : " + enumVal);

            if (key != null) {
                buttonDic = new Dictionary<ButtonName, GameObject>();
                var prevAlert = referenceToInit[enumVal].transform.Find("alert");
                if(prevAlert == null) {
                    SetUpButtonToAlert(referenceToInit[enumVal], enumVal);
                }
            }
        }
    }

    /// <summary>
    /// 첫 로그인 시 초기화
    /// </summary>
    public void ClearDic() {
        if(buttonDic != null) {
            buttonDic.Clear();
            WriteCsvFile();
        }   
    }

    public static void _ClearDic() {
        string dir = string.Empty;
        string fileName = "AlertList.csv";

        if (Application.platform == RuntimePlatform.Android) {
            dir = Application.persistentDataPath;
        }
        else if (Application.platform == RuntimePlatform.IPhonePlayer) {
            dir = Application.persistentDataPath;
        }
        else {
            dir = Application.streamingAssetsPath;
        }

        string filePath = dir + "/" + fileName;
        File.WriteAllBytes(
            filePath,
            new byte[0] {}
        );
    }

    public void MainScrollChanged(HorizontalScrollSnap scrollSnap) {
        var isTutorialFinished = MainSceneStateHandler.Instance.GetState("IsTutorialFinished");
        if (!isTutorialFinished) return;

        switch (scrollSnap.CurrentPage) {
            case 0:                 //부대편집
                DisableButtonToAlert(
                    referenceToInit[ButtonName.DECK_EDIT],
                    ButtonName.DECK_EDIT
                );
                break;
            case 2:                 //카드
                DisableButtonToAlert(
                    referenceToInit[ButtonName.DICTIONARY],
                    ButtonName.DICTIONARY
                );
                break;
            case 3:                 //상점
                DisableButtonToAlert(
                    referenceToInit[ButtonName.SHOP],
                    ButtonName.SHOP
                );
                break;
        }
    }

    public enum ButtonName {
        SOCIAL,
        DECK_EDIT,
        MAIN,
        DICTIONARY,
        SHOP,
        MODE,
        QUEST,
        LEVELUP_PACKAGE,
        REWARD_BOX,
        LEAGUE_REWARD,
        MAIL
    }
}
