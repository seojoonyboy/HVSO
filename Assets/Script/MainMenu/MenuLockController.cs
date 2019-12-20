using Newtonsoft.Json;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;

public class MenuLockController : SerializedMonoBehaviour {
    [SerializeField] Transform MainScrollSnapContent;
    [SerializeField] GameObject mainButtonsParent;
    [SerializeField] Transform deckEditListParent;

    [SerializeField] Dictionary<string, GameObject> menues;

    NoneIngameSceneEventHandler eventHandler;
    bool isAllUnlocked = false;

    void Awake() {
        eventHandler = NoneIngameSceneEventHandler.Instance;
        eventHandler.AddListener(NoneIngameSceneEventHandler.EVENT_TYPE.API_USER_UPDATED, OnUserDataUpdated);
    }

    void OnDestroy() {
        eventHandler.RemoveListener(NoneIngameSceneEventHandler.EVENT_TYPE.API_USER_UPDATED, OnUserDataUpdated);
    }

    private void OnUserDataUpdated(Enum Event_Type, Component Sender, object Param) {
        if (isAllUnlocked) {
            foreach(KeyValuePair<string, GameObject> keyValuePair in menues) {
                keyValuePair.Value.transform.Find("Lock").GetComponent<MenuLocker>().UnlockWithNoEffect();
            }
            return;
        }

        string _lockMenuList = PlayerPrefs.GetString("lockMenuList");
        if (string.IsNullOrEmpty(_lockMenuList)) return;

        var lockMenuList = dataModules.JsonReader.Read<MenuNameData>(PlayerPrefs.GetString("lockMenuList"));
        if (lockMenuList.menuNameList == null) lockMenuList.menuNameList = new List<string>();

        foreach (string menuName in lockMenuList.menuNameList) {
            Lock(menuName, false);
        }

        var etcInfo = AccountManager.Instance.userData.etcInfo;
        if (etcInfo != null) {
            var unlockInfo = etcInfo.Find(x => x.key == "unlockInfo");
            if (unlockInfo == null) return;

            string data = unlockInfo.value;
            string[] menuNames = data.Split(',');
            foreach (string name in menuNames) {
                Unlock(name, true);
            }
        }

        var unlockMenuList = dataModules.JsonReader.Read<MenuNameData>(PlayerPrefs.GetString("unlockMenuList"));
        if (unlockMenuList.menuNameList == null) unlockMenuList.menuNameList = new List<string>();
        
        StringBuilder sb = new StringBuilder();
        StringBuilder sb2 = new StringBuilder();
        foreach (string name in unlockMenuList.menuNameList) {
            sb.Append(name + ", ");

            Unlock(name, false);
            if (lockMenuList.menuNameList.Exists(x => x == name)) lockMenuList.menuNameList.Remove(name);
        }
        foreach (string name in lockMenuList.menuNameList) {
            sb2.Append(name + ", ");
            Lock(name, false);
        }

        MainScrollSnapContent.GetComponent<HorizontalLayoutGroup>().enabled = false;
        MainScrollSnapContent.GetComponent<HorizontalLayoutGroup>().enabled = true;

        Logger.Log("unlockMenuList : " + sb.ToString());
        Logger.Log("lockMenuList : " + sb2.ToString());

        if (lockMenuList.menuNameList.Count == 0) {
            Logger.Log("///////////////////////");
            Logger.Log("모두 해금됨");
            isAllUnlocked = true;
        }
    }

    /// <summary>
    /// 특정 메뉴 잠금
    /// </summary>
    /// <param name="keyword"></param>
    /// <param name="needTranslate"></param>
    public void Lock(string keyword, bool fromServer) {
        var translatedKeyword = keyword;
        if (fromServer) {
            translatedKeyword = FindMenuObject(keyword);
        }

        //Logger.Log("Lock : " + keyword);
        if (!menues.ContainsKey(translatedKeyword)) {
            Logger.LogError("keyword no exist : " + translatedKeyword);
            return;
        }

        GameObject menu = menues[translatedKeyword];

        if (IsMainMenu(translatedKeyword)) {
            //Logger.Log("Lock : " + keyword);
            switch (translatedKeyword) {
                case "DeckEdit":
                    Transform DeckEditWindow = MainScrollSnapContent.Find("DeckEditWindow");
                    if (DeckEditWindow == null) break;
                    DeckEditWindow.SetParent(MainScrollSnapContent.parent);
                    DeckEditWindow.gameObject.SetActive(false);
                    break;
                case "Dictionary":
                    Transform DictionarySelect = MainScrollSnapContent.Find("DictionaryWindow");
                    if (DictionarySelect == null) break;
                    DictionarySelect.SetParent(MainScrollSnapContent.parent);
                    DictionarySelect.gameObject.SetActive(false);
                    break;
                case "Shop":
                    Transform ShopWindow = MainScrollSnapContent.Find("ShopWindow");
                    if (ShopWindow == null) break;
                    ShopWindow.SetParent(MainScrollSnapContent.parent);
                    ShopWindow.gameObject.SetActive(false);
                    break;
            }

            MainScrollSnapContent.parent.GetComponent<HorizontalScrollSnap>().enabled = false;
            MainScrollSnapContent.parent.GetComponent<HorizontalScrollSnap>().enabled = true;

            MainScrollSnapContent.parent.GetComponent<HorizontalScrollSnap>().CurrentPage = 2;
            int mainSibilingIndex = MainScrollSnapContent.Find("MainWindow").GetSiblingIndex();
            MainScrollSnapContent.parent.GetComponent<HorizontalScrollSnap>().GoToScreen(mainSibilingIndex);

            menu.transform.Find("Lock").GetComponent<MenuLocker>().Lock();
        }
        else {
            if (translatedKeyword.Contains("HumanBaseDeck") || translatedKeyword.Contains("OrcBaseDeck")) {
                foreach (Transform deckObject in deckEditListParent) {
                    if (deckObject.GetSiblingIndex() == 0) continue;
                    var buttons = deckObject.Find("DeckObject/Buttons");
                    buttons.Find("DeleteBtn/Lock").GetComponent<MenuLocker>().Lock();
                    buttons.Find("AiBattleBtn/Lock").GetComponent<MenuLocker>().Lock();
                }
            }
            else {
                menu.transform.Find("Lock").GetComponent<MenuLocker>().Lock();
            }
        }

        var unlockMenuList = dataModules.JsonReader.Read<MenuNameData>(PlayerPrefs.GetString("unlockMenuList"));
        var lockMenuList = dataModules.JsonReader.Read<MenuNameData>(PlayerPrefs.GetString("lockMenuList"));

        if (!lockMenuList.menuNameList.Exists(x => x == translatedKeyword)) lockMenuList.menuNameList.Add(translatedKeyword);
        if (unlockMenuList.menuNameList.Exists(x => x == translatedKeyword)) unlockMenuList.menuNameList.Remove(translatedKeyword);

        PlayerPrefs.SetString("lockMenuList", JsonConvert.SerializeObject(lockMenuList));
        PlayerPrefs.SetString("unlockMenuList", JsonConvert.SerializeObject(unlockMenuList));
    }

    /// <summary>
    /// 특정 메뉴 해금
    /// </summary>
    /// <param name="keyword"></param>
    public void Unlock(string keyword, bool fromServer) {
        var translatedKeyword = keyword;
        if (fromServer) {
            translatedKeyword = FindMenuObject(keyword);
        }

        if (!menues.ContainsKey(translatedKeyword)) {
            Logger.LogError("keyword no exist : " + translatedKeyword);
            return;
        }
        GameObject menu = menues[translatedKeyword];
        Logger.Log(translatedKeyword + " 해금됨");
        if(translatedKeyword == "Story") {
            menues["Mode"].transform.parent.parent.Find("SelectedModeImage/Lock").GetComponent<MenuLocker>().OnlyUnlockEffect();
        }
        if(translatedKeyword == "Shop") {
            mainButtonsParent.transform.GetChild(4).Find("Lock").GetComponent<MenuLocker>().Unlock();
        }
        if(translatedKeyword == "DeckEdit") {
            mainButtonsParent.transform.GetChild(1).Find("Lock").GetComponent<MenuLocker>().Unlock();
        }

        if (IsMainMenu(translatedKeyword)) {
            switch (translatedKeyword) {
                case "DeckEdit":
                    Transform DeckEditWindow = MainScrollSnapContent.parent.Find("DeckEditWindow");
                    if (DeckEditWindow == null) break;
                    DeckEditWindow.SetParent(MainScrollSnapContent);
                    DeckEditWindow.transform.SetAsFirstSibling();    //메인화면보다 왼쪽
                    DeckEditWindow.gameObject.SetActive(true);
                    break;
                case "Dictionary":
                    Transform DictionaryWindow = MainScrollSnapContent.parent.Find("DictionaryWindow");
                    if (DictionaryWindow == null) break;
                    DictionaryWindow.SetParent(MainScrollSnapContent);
                    DictionaryWindow.transform.SetAsLastSibling();      //메인화면보다 오른쪽
                    DictionaryWindow.gameObject.SetActive(true);
                    break;
                case "Shop":
                    Transform ShopWindow = MainScrollSnapContent.parent.Find("ShopWindow");
                    if (ShopWindow == null) break;
                    ShopWindow.SetParent(MainScrollSnapContent);
                    ShopWindow.transform.SetAsLastSibling();            //메인화면보다 오른쪽
                    ShopWindow.gameObject.SetActive(true);
                    break;
            }

            MainScrollSnapContent.parent.GetComponent<HorizontalScrollSnap>().enabled = false;
            MainScrollSnapContent.parent.GetComponent<HorizontalScrollSnap>().enabled = true;

            MainScrollSnapContent.parent.GetComponent<HorizontalScrollSnap>().CurrentPage = 2;
            int mainSibilingIndex = MainScrollSnapContent.Find("MainWindow").GetSiblingIndex();
            MainScrollSnapContent.parent.GetComponent<HorizontalScrollSnap>().GoToScreen(mainSibilingIndex);

            menu.transform.Find("Lock").GetComponent<MenuLocker>().Unlock();
        }
        else {
            if(translatedKeyword.Contains("HumanBaseDeck") || translatedKeyword.Contains("OrcBaseDeck")) {
                foreach (Transform deckObject in deckEditListParent) {
                    if (deckObject.GetSiblingIndex() == 0) continue;
                    var buttons = deckObject.Find("DeckObject/Buttons");
                    buttons.Find("DeleteBtn/Lock").GetComponent<MenuLocker>().Unlock();
                    buttons.Find("AiBattleBtn/Lock").GetComponent<MenuLocker>().Unlock();
                }
            }
            else {
                menu.transform.Find("Lock").GetComponent<MenuLocker>().Unlock();
            }
        }

        var unlockMenuList = dataModules.JsonReader.Read<MenuNameData>(PlayerPrefs.GetString("unlockMenuList"));
        var lockMenuList = dataModules.JsonReader.Read<MenuNameData>(PlayerPrefs.GetString("lockMenuList"));

        if (lockMenuList.menuNameList.Exists(x => x == translatedKeyword)) lockMenuList.menuNameList.Remove(translatedKeyword);
        if (!unlockMenuList.menuNameList.Exists(x => x == translatedKeyword)) unlockMenuList.menuNameList.Add(translatedKeyword);

        PlayerPrefs.SetString("lockMenuList", JsonConvert.SerializeObject(lockMenuList));
        PlayerPrefs.SetString("unlockMenuList", JsonConvert.SerializeObject(unlockMenuList));
    }

    public string FindMenuObject(string keyword) {
        string translatedKeyword = string.Empty;
        switch (keyword) {
            case "스토리":
                translatedKeyword = "Story";
                break;
            case "카드":
                translatedKeyword = "Dictionary";
                break;
            case "배틀":
                translatedKeyword = "League";
                break;
        }
        return translatedKeyword;
    }

    public GameObject FindButtonObject(string name) {
        if (!menues.ContainsKey(name)) return null;
        else return menues[name];
    }

    public GameObject FindButtonLockObject(string name) {
        if (!menues.ContainsKey(name)) return null;
        else return menues[name].transform.Find("Lock").gameObject;
    }

    private bool IsMainMenu(string keyword) {
        if (keyword == "DeckEdit") return true;
        if (keyword == "DeckSetting") return true;
        if (keyword == "Dictionary") return true;
        if (keyword == "Shop") return true;
        return false;
    }

    /// <summary>
    /// scriptableObject 데이터 초기화
    /// </summary>
    public void ResetMenuLockData() {
        MenuNameData unlockMenuList = new MenuNameData();
        unlockMenuList.menuNameList = new List<string>();
       
        MenuNameData lockMenuList = new MenuNameData();
        lockMenuList.menuNameList = new List<string>();
        lockMenuList.menuNameList.Add("League");
        lockMenuList.menuNameList.Add("Story");
        lockMenuList.menuNameList.Add("DeckEdit");
        lockMenuList.menuNameList.Add("Dictionary");
        lockMenuList.menuNameList.Add("Shop");
        lockMenuList.menuNameList.Add("RewardBox");
        lockMenuList.menuNameList.Add("Mode");
        lockMenuList.menuNameList.Add("HumanBaseDeckAiBattleBtn");
        lockMenuList.menuNameList.Add("HumanBaseDeckDeleteBtn");
        lockMenuList.menuNameList.Add("OrcBaseDeckAiBattleBtn");
        lockMenuList.menuNameList.Add("OrcBaseDeckDeleteBtn");

        PlayerPrefs.SetString("lockMenuList", JsonConvert.SerializeObject(lockMenuList));
        PlayerPrefs.SetString("unlockMenuList", JsonConvert.SerializeObject(unlockMenuList));
    }

    public class MenuNameData {
        public List<string> menuNameList;
    }
}
