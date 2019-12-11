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
    [SerializeField] Dictionary<string, GameObject> menues;

    NoneIngameSceneEventHandler eventHandler;

    void Awake() {
        eventHandler = NoneIngameSceneEventHandler.Instance;
        eventHandler.AddListener(NoneIngameSceneEventHandler.EVENT_TYPE.API_USER_UPDATED, OnUserDataUpdated);
    }

    void OnDestroy() {
        eventHandler.RemoveListener(NoneIngameSceneEventHandler.EVENT_TYPE.API_USER_UPDATED, OnUserDataUpdated);
    }

    private void OnUserDataUpdated(Enum Event_Type, Component Sender, object Param) {
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
            try {
                switch (translatedKeyword) {
                    case "DeckEdit":
                        Transform DeckSettingWindow = MainScrollSnapContent.Find("DeckSettingWindow");
                        DeckSettingWindow.SetParent(MainScrollSnapContent.parent);
                        DeckSettingWindow.gameObject.SetActive(false);
                        break;
                    case "Dictionary":
                        Transform DictionarySelect = MainScrollSnapContent.Find("DictionarySelect");
                        DictionarySelect.SetParent(MainScrollSnapContent.parent);
                        DictionarySelect.gameObject.SetActive(false);
                        break;
                    case "Shop":
                        Transform ShopWindow = MainScrollSnapContent.Find("ShopWindow");
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
            catch(Exception ex) {
                //Logger.LogError(ex);
            }
        }
        else {
            menu.transform.Find("Lock").GetComponent<MenuLocker>().Lock();
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
        if(translatedKeyword == "Story") {
            menues["Mode"].transform.parent.parent.Find("SelectedModeImage/Lock").GetComponent<MenuLocker>().OnlyUnlockEffect();
        }

        if (IsMainMenu(translatedKeyword)) {
            try {
                switch (translatedKeyword) {
                    case "DeckEdit":
                        Transform DeckSettingWindow = MainScrollSnapContent.parent.Find("DeckSettingWindow");
                        DeckSettingWindow.SetParent(MainScrollSnapContent);
                        DeckSettingWindow.gameObject.SetActive(true);
                        break;
                    case "Dictionary":
                        Transform DictionarySelect = MainScrollSnapContent.parent.Find("DictionarySelect");
                        DictionarySelect.SetParent(MainScrollSnapContent);
                        DictionarySelect.gameObject.SetActive(true);
                        break;
                    case "Shop":
                        Transform ShopWindow = MainScrollSnapContent.parent.Find("ShopWindow");
                        ShopWindow.SetParent(MainScrollSnapContent);
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
            catch (Exception ex) {
                //Logger.LogError(ex);
            }
        }
        else {
            menu.transform.Find("Lock").GetComponent<MenuLocker>().Unlock();
        }

        var unlockMenuList = dataModules.JsonReader.Read<MenuNameData>(PlayerPrefs.GetString("unlockMenuList"));
        var lockMenuList = dataModules.JsonReader.Read<MenuNameData>(PlayerPrefs.GetString("lockMenuList"));

        if (unlockMenuList.menuNameList == null) unlockMenuList.menuNameList = new List<string>();
        if (lockMenuList.menuNameList == null) lockMenuList.menuNameList = new List<string>();

        if (lockMenuList.menuNameList.Exists(x => x == translatedKeyword)) lockMenuList.menuNameList.Remove(translatedKeyword);
        if (unlockMenuList.menuNameList.Exists(x => x == translatedKeyword)) unlockMenuList.menuNameList.Add(translatedKeyword);

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

        PlayerPrefs.SetString("lockMenuList", JsonConvert.SerializeObject(lockMenuList));
        PlayerPrefs.SetString("unlockMenuList", JsonConvert.SerializeObject(unlockMenuList));
    }

    public class MenuNameData {
        public List<string> menuNameList;
    }
}
