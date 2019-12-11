using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuLockController : SerializedMonoBehaviour {
    [SerializeField] Transform MainScrollSnapContent;
    [SerializeField] Dictionary<string, GameObject> menues;
    [SerializeField] Dictionary<string, GameObject> mainMenues;

    NoneIngameSceneEventHandler eventHandler;

    void Awake() {
        eventHandler = NoneIngameSceneEventHandler.Instance;
        eventHandler.AddListener(NoneIngameSceneEventHandler.EVENT_TYPE.API_USER_UPDATED, OnUserDataUpdated);
    }

    void OnDestroy() {
        eventHandler.RemoveListener(NoneIngameSceneEventHandler.EVENT_TYPE.API_USER_UPDATED, OnUserDataUpdated);
    }

    private void OnUserDataUpdated(Enum Event_Type, Component Sender, object Param) {
        var etcInfo = AccountManager.Instance.userData.etcInfo;
        if (etcInfo == null) {
            Lock();
            return;
        }
        var unlockInfo = etcInfo.Find(x => x.key == "unlockInfo");
        if (unlockInfo == null) return;

        Lock(); //일단 다 잠금
        string data = unlockInfo.value;
        string[] menuNames = data.Split(',');
        foreach(string name in menuNames) {
            Unlock(name);
        }
    }

    /// <summary>
    /// 모두 잠금
    /// </summary>
    public void Lock() {
        foreach(KeyValuePair<string, GameObject> pair in menues) {
            Lock(pair.Key, false);
        }
    }

    /// <summary>
    /// 특정 메뉴 잠금
    /// </summary>
    /// <param name="keyword"></param>
    /// <param name="needTranslate"></param>
    public void Lock(string keyword, bool needTranslate) {
        GameObject menu = null;
        if (needTranslate) {
            var translatedKeyword = FindMenuObject(keyword);
            if (translatedKeyword == "cardMenu") {
                menues["cardMenu_orc"]
                    .transform.Find("Lock")
                    .GetComponent<MenuLocker>().Lock();
                menues["cardMenu_human"]
                    .transform.Find("Lock")
                    .GetComponent<MenuLocker>().Lock();
                return;
            }
            else {
                menu = menues[translatedKeyword];
            }
        }
        else {
            menu = menues[keyword];
        }
        if (menu == null) return;

        Logger.Log(menu.name);
        menu.transform.Find("Lock").GetComponent<MenuLocker>().Lock();
    }

    /// <summary>
    /// 특정 메인 메뉴 잠금
    /// </summary>
    /// <param name="page"></param>
    public void LockMainMenu(string name) {
        if (mainMenues.ContainsKey(name)) {
            mainMenues[name].GetComponentInChildren<MenuLocker>().Lock();
        }
    }

    public void LockAllMainMenues() {
        foreach(KeyValuePair<string, GameObject> pair in mainMenues) {
            mainMenues[pair.Key].GetComponentInChildren<MenuLocker>().Lock();
        }
    }

    /// <summary>
    /// 특정 메인 메뉴 해금
    /// </summary>
    /// <param name="name"></param>
    public void UnlockMainMenu(string name) {
        if (mainMenues.ContainsKey(name)) {
            mainMenues[name].GetComponentInChildren<MenuLocker>().Unlock();
        }
    }

    public void UnlockAllMainMenues() {
        foreach (KeyValuePair<string, GameObject> pair in mainMenues) {
            mainMenues[pair.Key].GetComponentInChildren<MenuLocker>().Unlock();
        }
    }

    /// <summary>
    /// 특정 메뉴 해금
    /// </summary>
    /// <param name="keyword"></param>
    public void Unlock(string keyword) {
        GameObject menu = null;

        var translatedKeyword = FindMenuObject(keyword);
        if (translatedKeyword == "cardMenu") {
            menues["cardMenu_orc"].GetComponent<Button>().enabled = true;
            menues["cardMenu_human"].GetComponent<Button>().enabled = true;
            return;
        }
        else {
            if(translatedKeyword == "league") {
                menues[translatedKeyword].transform.GetChild(0).gameObject.SetActive(false);
            }
            menu = menues[translatedKeyword];
        }
        if (menu == null) return;
        menu.GetComponent<Button>().enabled = true;
    }

    public string FindMenuObject(string keyword) {
        string translatedKeyword = string.Empty;
        switch (keyword) {
            case "스토리":
                translatedKeyword = "story";
                break;
            case "카드":
                translatedKeyword = "cardMenu";
                break;
            case "배틀":
                translatedKeyword = "league";
                break;
        }
        return translatedKeyword;
    }
}
