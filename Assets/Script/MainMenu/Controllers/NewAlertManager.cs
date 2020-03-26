using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
using System.IO;
using Sirenix.OdinInspector;
using System;
using System.Linq;
using Spine.Unity;
using UnityEngine.UI.Extensions;

/// <summary>
/// 메인화면의 Alert를 관리함
/// </summary>
public class NewAlertManager : SerializedMonoBehaviour {
    public static NewAlertManager Instance;
    public GameObject alertPref, alertWithNumPref;
    public List<string> buttonName;
    public Dictionary<ButtonName, GameObject> buttonDic;          //느낌표를 갖고 있는 버튼 딕셔너리
    public List<string> unlockConditionsList;                      //느낌표에 대한 세부 제거 조건
    public Dictionary<ButtonName, GameObject> referenceToInit;    //처음 초기화를 위한 참조

    public string playerPrefKey = "AlertIcon";
    private string 
        alertListFileName = "AlertList.csv",
        alertUnlockConditionsFileName = "AlertConditions.csv";

    NoneIngameSceneEventHandler eventHandler;
    [SerializeField] Transform buttonAnimation;                   //spine button set
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

    public List<string> GetUnlockCondionsList() {
        return unlockConditionsList;
    }

    /// <summary>
    /// 느낌표 추가
    /// </summary>
    /// <param name="button">클릭 대상 GameObject</param>
    /// <param name="enumName">key</param>
    /// <param name="normalRemoveCond">단순히 해당 느낌표 클릭만 해도 제거되는 조건인가?</param>
    public void SetUpButtonToAlert(GameObject button, ButtonName enumName, bool normalRemoveCond = true) {
        if (buttonDic == null) buttonDic = new Dictionary<ButtonName, GameObject>();

        if (buttonDic.ContainsKey(enumName)) return;
        
        buttonDic.Add(enumName, button);
        WriteAlertListFile();
        if (button.name == "ResultUI") return;
        GameObject alert = referenceToInit[enumName].transform.Find("alert").gameObject;
        alert.gameObject.SetActive(true);

        if (normalRemoveCond) {
            button.GetComponent<Button>()
            .OnClickAsObservable()
            .First()
            .Subscribe(_ => Onclick(button, enumName)).AddTo(button);

            button.SetActive(true);
        }
    }

    private void Onclick(GameObject button, ButtonName enumName) {
        if(!unlockConditionsList.Exists(x => x.Contains(enumName.ToString()))) {
            DisableButtonToAlert(button, enumName);
        }
    }

    /// <summary>
    /// 숫자 표기 알람 추가
    /// </summary>
    /// <param name="button"></param>
    /// <param name="enumName"></param>
    /// <param name="initNum"></param>
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

    /// <summary>
    /// 느낌표 제거 조건 추가
    /// </summary>
    /// <param name="enumName"></param>
    /// <param name="targetId"></param>
    public void SetUpButtonToUnlockCondition(ButtonName enumName, string targetId, bool isHero = false) {
        if (unlockConditionsList == null) unlockConditionsList = new List<string>();
        string newKey;
        if (enumName != ButtonName.DICTIONARY)
            newKey = enumName + "_" + targetId;
        else {
            if(isHero)
                newKey = enumName + "_hero_" + targetId;
            else
                newKey = enumName + "_card_" + targetId;
        }
        if (!unlockConditionsList.Exists(x => x == newKey)) {

            unlockConditionsList.Add(newKey);
        }

        WriteAlertConditionsFile();
    }
    
    /// <summary>
    /// 느낌표 제거
    /// </summary>
    /// <param name="button">느낌표가 붙어있는 버튼 GameObject</param>
    /// <param name="enumName">Key 값</param>
    private void DisableButtonToAlert(GameObject button, ButtonName enumName) {
        if (!buttonDic.ContainsKey(enumName)) return;

        button.transform.Find("alert").gameObject.SetActive(false);
        buttonDic.Remove(enumName);
        WriteAlertListFile();
    }

    private void RemoveAlertConditionsFile() {
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
        
        string filePath = dir + "/" + alertUnlockConditionsFileName;
        File.Delete(filePath);
    }
    
    /// <summary>
    /// 느낌표 목록에 대한 CSV 파일 생성
    /// </summary>
    public string WriteAlertListFile(bool forceToRemove = false) {
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

        string filePath = dir + "/" + alertListFileName;
        
        if (forceToRemove) {
            buttonDic.Clear();
            File.Delete(filePath);
            return null;
        }
        
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        if(buttonDic == null) buttonDic = new Dictionary<ButtonName, GameObject>();
        if(buttonDic.Count > 0) {
            var last = buttonDic.Last();
            foreach (var dic in buttonDic) {
                if (dic.Equals(last)) {
                    sb.Append(dic.Key);
                }
                else sb.Append(dic.Key + ",");
            }
        }
            
        File.WriteAllBytes(
            filePath,
            System.Text.Encoding.UTF8.GetBytes(sb.ToString())
        );

        return filePath;
    }

    /// <summary>
    /// 느낌표 제거 조건에 대한 CSV 파일 생성
    /// </summary>
    public string WriteAlertConditionsFile() {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        if (unlockConditionsList != null && unlockConditionsList.Count > 0) {
            var last = unlockConditionsList.Last();
            foreach (string condition in unlockConditionsList) {
                if (condition.Equals(last)) {
                    sb.Append(condition);
                }
                else sb.Append(condition + ",");
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

        string filePath = dir + "/" + alertUnlockConditionsFileName;
        File.WriteAllBytes(
            filePath,
            System.Text.Encoding.UTF8.GetBytes(sb.ToString())
        );

        return filePath;
    }

    /// <summary>
    /// 메인 화면 왔을 때 초기화
    /// </summary>
    public void Initialize() {
        ReadAlertUnlockConditionInCSV();
        ReadAlertListInCSV();
    }

    public void SimpleInitialize() {
        ReadAlertListInCSVIngame();
    }

    private void ReadAlertListInCSV() {
        var pathToCsv = GetFilePath(alertListFileName);

        if (!File.Exists(pathToCsv)) {
            pathToCsv = WriteAlertListFile();
        }

        var lines = File.ReadAllText(pathToCsv);
        if (string.IsNullOrEmpty(lines)) return;
        string[] keys = lines.Split(',');
        buttonDic = new Dictionary<ButtonName, GameObject>();
        foreach (string key in keys) {
            var enumVal = (ButtonName)(Enum.Parse(typeof(ButtonName), key));
            Logger.Log("Alert Key : " + enumVal);

            if (key != null) {
                buttonDic.Add(enumVal, referenceToInit[enumVal]);
                var prevAlert = referenceToInit[enumVal].transform.Find("alert");
                prevAlert.gameObject.SetActive(true);
            }
        }
    }

    private void ReadAlertUnlockConditionInCSV() {
        var pathToCsv = GetFilePath(alertUnlockConditionsFileName);

        if (!File.Exists(pathToCsv)) {
            pathToCsv = WriteAlertConditionsFile();
        }

        var lines = File.ReadAllText(pathToCsv);
        if (string.IsNullOrEmpty(lines)) return;

        unlockConditionsList = new List<string>();
        string[] keys = lines.Split(',');
        foreach (string key in keys) {
            if (!string.IsNullOrEmpty(key)) {
                unlockConditionsList.Add(key);
            }
        }
    }

    private void ReadAlertListInCSVIngame() {
        var pathToCsv = GetFilePath(alertListFileName);

        if (!File.Exists(pathToCsv)) {
            pathToCsv = WriteAlertListFile();
        }

        var lines = File.ReadAllText(pathToCsv);
    }
    /// <summary>
    /// 느낌표 제거가 가능한지 확인 (일반적인 클릭 후 바로 제거가 아닌 추가 조건이 있는 경우)
    /// </summary>
    /// <param name="buttonName"></param>
    /// <param name="detailId"></param>
    public void CheckRemovable(ButtonName buttonName, string detailId, bool isHero = false) {
        string keyword;
        if(buttonName != ButtonName.DICTIONARY)
            keyword = buttonName + "_" + detailId;
        else {
            if(isHero)
                keyword = buttonName + "_hero_" + detailId;
            else
                keyword = buttonName + "_card_" + detailId;
        }
            
        if(unlockConditionsList != null && unlockConditionsList.Exists(x => x == keyword)){
            unlockConditionsList.Remove(keyword);
            WriteAlertConditionsFile();
            transform.GetComponent<MenuSceneController>().SetCardInfoByRarelity();
        }

        if(unlockConditionsList != null && !unlockConditionsList.Exists(x => x.Contains(buttonName.ToString()))){
            DisableButtonToAlert(referenceToInit[buttonName], buttonName);
        }
    }

    private string GetFilePath(string fileName) {
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

        return pathToCsv;
    }

    /// <summary>
    /// 첫 로그인 시 초기화
    /// </summary>
    public void ClearDic() {
        RemoveAlertConditionsFile();
        WriteAlertListFile(true);
    }

    /// <summary>
    /// 첫 로그인 시(로그인 화면에서 튜토리얼 스킵하기 버튼을 눌렀을 때 처리를 위함)
    /// </summary>
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
                Onclick(
                    referenceToInit[ButtonName.DECK_EDIT],
                    ButtonName.DECK_EDIT
                );
                break;
            case 2:                 //카드
                Onclick(
                    referenceToInit[ButtonName.DICTIONARY],
                    ButtonName.DICTIONARY
                );
                break;
            case 3:                 //상점
                Onclick(
                    referenceToInit[ButtonName.SHOP],
                    ButtonName.SHOP
                );
                break;
        }
        if (scrollSnap.CurrentPage == 1) {
            if (buttonDic.ContainsKey(ButtonName.DECK_NUMBERS) && !buttonDic.ContainsKey(ButtonName.DECK_EDIT))
                DisableButtonToAlert(referenceToInit[ButtonName.DECK_NUMBERS], ButtonName.DECK_NUMBERS);
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
        MAIL,
        DECK_NUMBERS
    }
}
