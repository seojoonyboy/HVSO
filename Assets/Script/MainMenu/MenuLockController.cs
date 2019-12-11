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
                
                menu.transform.Find("Lock").GetComponent<MenuLocker>().Lock();
            }
            catch(Exception ex) {
                //Logger.LogError(ex);
            }
        }
        else {
            menu.transform.Find("Lock").GetComponent<MenuLocker>().Lock();
        }

        if (!scriptable_menuLockData.lockMenuList.Exists(x => x == translatedKeyword)) scriptable_menuLockData.lockMenuList.Add(translatedKeyword);
        if (scriptable_menuLockData.unlockMenuList.Exists(x => x == translatedKeyword)) scriptable_menuLockData.unlockMenuList.Remove(translatedKeyword);
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

                menu.transform.Find("Lock").GetComponent<MenuLocker>().Unlock();
            }
            catch (Exception ex) {
                //Logger.LogError(ex);
            }
        }
        else {
            menu.transform.Find("Lock").GetComponent<MenuLocker>().Unlock();
        }

        if (scriptable_menuLockData.lockMenuList.Exists(x => x == translatedKeyword)) scriptable_menuLockData.lockMenuList.Remove(translatedKeyword);
        if (!scriptable_menuLockData.unlockMenuList.Exists(x => x == translatedKeyword)) scriptable_menuLockData.unlockMenuList.Add(translatedKeyword);
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
        scriptable_menuLockData.lockMenuList.Clear();
        scriptable_menuLockData.lockMenuList.Add("League");
        scriptable_menuLockData.lockMenuList.Add("Story");
        scriptable_menuLockData.lockMenuList.Add("DeckEdit");
        scriptable_menuLockData.lockMenuList.Add("Dictionary");
        scriptable_menuLockData.lockMenuList.Add("Shop");
        scriptable_menuLockData.lockMenuList.Add("RewardBox");
        scriptable_menuLockData.lockMenuList.Add("Mode");

        scriptable_menuLockData.unlockMenuList.Clear();
    }
}
