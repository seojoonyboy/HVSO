using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;

public class MenuLockController : SerializedMonoBehaviour {
    [SerializeField] Transform MainScrollSnapContent;
    [SerializeField] Dictionary<string, GameObject> menues;

    NoneIngameSceneEventHandler eventHandler;
    [SerializeField] MenuLockData scriptable_menuLockData;

    void Awake() {
        eventHandler = NoneIngameSceneEventHandler.Instance;
        eventHandler.AddListener(NoneIngameSceneEventHandler.EVENT_TYPE.API_USER_UPDATED, OnUserDataUpdated);
    }

    void OnDestroy() {
        eventHandler.RemoveListener(NoneIngameSceneEventHandler.EVENT_TYPE.API_USER_UPDATED, OnUserDataUpdated);
    }

    private void OnUserDataUpdated(Enum Event_Type, Component Sender, object Param) {
        foreach(string menuName in scriptable_menuLockData.lockMenuList) {
            Lock(menuName);
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
    /// 모두 잠금
    /// </summary>
    public void Lock() {
        foreach(KeyValuePair<string, GameObject> pair in menues) {
            Lock(pair.Key);
        }
    }

    /// <summary>
    /// 특정 메뉴 잠금
    /// </summary>
    /// <param name="keyword"></param>
    /// <param name="needTranslate"></param>
    public void Lock(string keyword) {
        //Logger.Log("Lock : " + keyword);
        if (!menues.ContainsKey(keyword)) {
            Logger.LogError("keyword no exist : " + keyword);
            return;
        }
        GameObject menu = menues[keyword];

        if (IsMainMenu(keyword)) {
            //Logger.Log("Lock : " + keyword);
            try {
                switch (keyword) {
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
                
                menu.transform.Find("Lock").GetComponent<MenuLocker>().Lock();
            }
            catch(Exception ex) {
                Logger.LogError(ex);
            }
        }
        else {
            menu.transform.Find("Lock").GetComponent<MenuLocker>().Lock();
        }
    }

    /// <summary>
    /// 특정 메뉴 해금
    /// </summary>
    /// <param name="keyword"></param>
    public void Unlock(string keyword, bool fromServer) {
        GameObject menu = null;

        var translatedKeyword = keyword;
        if (fromServer) {
            translatedKeyword = FindMenuObject(keyword);
        }
        //Logger.Log("Unlock : " + translatedKeyword);
        if (translatedKeyword == "CardMenu") {
            menues["CardMenu_orc"].transform.Find("Lock").GetComponent<MenuLocker>().Unlock();
            menues["CardMenu_human"].transform.Find("Lock").GetComponent<MenuLocker>().Unlock();
            return;
        }
        else {
            if(translatedKeyword == "Story") {
                menues["Mode"].transform.parent.parent.Find("SelectedModeImage/Lock").GetComponent<MenuLocker>().OnlyUnlockEffect();
            }
            menu = menues[translatedKeyword];
            menu.transform.Find("Lock").GetComponent<MenuLocker>().Unlock();
        }

        if (!scriptable_menuLockData.unlockMenuList.Exists(x => x == translatedKeyword)) scriptable_menuLockData.unlockMenuList.Add(translatedKeyword);
        if (scriptable_menuLockData.lockMenuList.Exists(x => x == translatedKeyword)) scriptable_menuLockData.lockMenuList.Remove(translatedKeyword);
    }

    public string FindMenuObject(string keyword) {
        string translatedKeyword = string.Empty;
        switch (keyword) {
            case "스토리":
                translatedKeyword = "Story";
                break;
            case "카드":
                translatedKeyword = "CardMenu";
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
        scriptable_menuLockData.lockMenuList.Clear();
        scriptable_menuLockData.lockMenuList.Add("CardMenu_human");
        scriptable_menuLockData.lockMenuList.Add("CardMenu_orc");
        scriptable_menuLockData.lockMenuList.Add("League");
        scriptable_menuLockData.lockMenuList.Add("Story");
        scriptable_menuLockData.lockMenuList.Add("DectEdit");
        scriptable_menuLockData.lockMenuList.Add("Dictionary");
        scriptable_menuLockData.lockMenuList.Add("Shop");
        scriptable_menuLockData.lockMenuList.Add("RewardBox");
        scriptable_menuLockData.lockMenuList.Add("Mode");

        scriptable_menuLockData.unlockMenuList.Clear();
    }
}
