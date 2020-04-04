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
    [SerializeField] AttendanceManager atm;
    [SerializeField] Dictionary<string, GameObject> menues;

    NoneIngameSceneEventHandler eventHandler;
    public bool isAllUnlocked = false;
    void Awake() {
        eventHandler = NoneIngameSceneEventHandler.Instance;
        eventHandler.AddListener(NoneIngameSceneEventHandler.EVENT_TYPE.API_TUTORIAL_INFOS_UPDATED, OnTutorialInfoUpdated);

        menues["HumanBaseDeckAiBattleBtn"] =
            deckEditListParent.GetChild(1).GetChild(0).Find("Buttons/AiBattleBtn").gameObject;
        menues["HumanBaseDeckDeleteBtn"] = 
            deckEditListParent.GetChild(1).GetChild(0).Find("Buttons/DeleteBtn").gameObject;
        menues["OrcBaseDeckAiBattleBtn"] =
            deckEditListParent.GetChild(2).GetChild(0).Find("Buttons/AiBattleBtn").gameObject;
        menues["OrcBaseDeckDeleteBtn"] = 
            deckEditListParent.GetChild(2).GetChild(0).Find("Buttons/DeleteBtn").gameObject;
    }

    void Start() {
        AccountManager.Instance.RequestTutorialUnlockInfos(true);
    }

    void OnDestroy() {
        eventHandler.RemoveListener(NoneIngameSceneEventHandler.EVENT_TYPE.API_TUTORIAL_INFOS_UPDATED, OnTutorialInfoUpdated);
    }

    public GameObject GetMenu(string key) {
        return menues[key];
    }

    private void OnTutorialInfoUpdated(Enum Event_Type, Component Sender, object Param) {
        object[] parm = (object[])Param;
        bool isInitRequest = (bool)parm[0];

        var questInfos = AccountManager.Instance.questInfos;
        if (questInfos != null) {
            var lockList = (List<string>)parm[2];

            isAllUnlocked = lockList.Count == 0;
        }
        else {
            var lockList = (List<string>)parm[2];
            if (lockList.Count == 0) isAllUnlocked = true;
            else isAllUnlocked = false;
        }
        if (isAllUnlocked) {
            Logger.Log("///////////////////////");
            Logger.Log("이미 모두 해금됨");

            foreach (KeyValuePair<string, GameObject> keyValuePair in menues) {
                keyValuePair.Value.transform.Find("Lock").GetComponent<MenuLocker>().UnlockWithNoEffect();
                
                if (keyValuePair.Key == "Shop") {
                    Transform targetWindow = MainScrollSnapContent.parent.Find("ShopWindow");
                    if (targetWindow == null) continue;
                    targetWindow.SetParent(MainScrollSnapContent);
                    targetWindow.transform.SetAsLastSibling();            //메인화면보다 오른쪽
                    targetWindow.gameObject.SetActive(true);
                    
                    int mainSibilingIndex = MainScrollSnapContent.Find("MainWindow").GetSiblingIndex();
                    
                    menues["Shop"].transform.Find("Lock").GetComponent<MenuLocker>().ActiveInnerImages();
                    
                    RefreshScrollSnap(mainSibilingIndex);
                    continue;
                }

                if (keyValuePair.Key == "DeckEdit") {
                    UnlockDeckEditButtons(false);
                }
            }

            var mainSceneStateHandler = MainSceneStateHandler.Instance;
            mainSceneStateHandler.TriggerAllMainMenuUnlocked();
            GetComponent<MenuSceneController>().CheckDailyQuest();

            return;
        }

        if (isInitRequest) {
            var unlockList = (List<string>)parm[1];
            var lockList = (List<string>)parm[2];

            foreach (string unlockName in unlockList) {
                Unlock(unlockName, false);
            }
            foreach(string lockName in lockList) {
                Lock(lockName);
            }
        }
        else {
            var unlockList = (List<string>)parm[1];
            var lockList = (List<string>)parm[2];

            foreach (string unlockName in unlockList) {
                Unlock(unlockName, true);
            }
            foreach (string lockName in lockList) {
                Lock(lockName);
            }
        }
    }

    /// <summary>
    /// 특정 메뉴 잠금
    /// </summary>
    /// <param name="keyword"></param>
    /// <param name="needTranslate"></param>
    public void Lock(string keyword) {
        var translatedKeyword = FindMenuObject(keyword);

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

            menu.transform.Find("Lock").GetComponent<MenuLocker>().Lock();
        }
        else {
            if (translatedKeyword == "AI") {
                foreach (Transform deckObject in deckEditListParent) {
                    if (deckObject.GetSiblingIndex() == 0) continue;
                    var buttons = deckObject.Find("DeckObject/Buttons");
                    var lockObj = buttons.Find("DeleteBtn/Lock");
                    if (lockObj == null) continue;
                    var menuLocker = buttons.Find("DeleteBtn/Lock").GetComponent<MenuLocker>();
                    if(menuLocker != null) {
                        menuLocker.Lock();
                        buttons.Find("AiBattleBtn/Lock").GetComponent<MenuLocker>().Lock();
                    }
                }

                //menues["Mode"].GetComponentInChildren<MenuLocker>().Lock();
            }
            else {
                menu.transform.Find("Lock").GetComponent<MenuLocker>().Lock();
            }
        }

        int mainSibilingIndex = MainScrollSnapContent.Find("MainWindow").GetSiblingIndex();
        RefreshScrollSnap(mainSibilingIndex);
    }

    private void RefreshScrollSnap(int sibilingIndex) {
        MainScrollSnapContent.parent.GetComponent<HorizontalScrollSnap>().enabled = false;
        MainScrollSnapContent.parent.GetComponent<HorizontalScrollSnap>().enabled = true;

        MainScrollSnapContent.parent.GetComponent<HorizontalScrollSnap>().CurrentPage = 2;
        
        MainScrollSnapContent.parent.GetComponent<HorizontalScrollSnap>().GoToScreen(sibilingIndex);

        //foreach(Transform window in MainScrollSnapContent) {
        //    window.gameObject.SetActive(false);
        //    window.gameObject.SetActive(true);
        //}

        MainScrollSnapContent.parent.GetComponent<HorizontalScrollSnap>().UpdateLayout();

        MainScrollSnapContent.GetComponent<HorizontalLayoutGroup>().enabled = false;
        MainScrollSnapContent.GetComponent<HorizontalLayoutGroup>().enabled = true;
    }

    /// <summary>
    /// 특정 메뉴 해금
    /// </summary>
    /// <param name="keyword"></param>
    public void Unlock(string keyword, bool isNeedEffect = true) {
        var translatedKeyword = FindMenuObject(keyword);
        if (string.IsNullOrEmpty(translatedKeyword)) return;

        if (!menues.ContainsKey(translatedKeyword)) {
            Logger.LogError("keyword no exist : " + translatedKeyword);
            return;
        }
        GameObject menu = menues[translatedKeyword];
        Logger.Log(translatedKeyword + " 해금됨");

        if (IsMainMenu(translatedKeyword)) {
            Transform targetWindow = null;
            switch (translatedKeyword) {
                case "DeckEdit":
                    targetWindow = MainScrollSnapContent.parent.Find("DeckEditWindow");
                    if (targetWindow == null) break;
                    targetWindow.SetParent(MainScrollSnapContent);
                    targetWindow.transform.SetAsFirstSibling();    //메인화면보다 왼쪽
                    targetWindow.gameObject.SetActive(true);
                    break;
                case "Dictionary":
                    targetWindow = MainScrollSnapContent.parent.Find("DictionaryWindow");
                    if (targetWindow == null) break;
                    targetWindow.SetParent(MainScrollSnapContent);
                    targetWindow.transform.SetAsLastSibling();      //메인화면보다 오른쪽
                    targetWindow.gameObject.SetActive(true);
                    break;
                case "Shop":
                    targetWindow = MainScrollSnapContent.parent.Find("ShopWindow");
                    if (targetWindow == null) break;
                    targetWindow.SetParent(MainScrollSnapContent);
                    targetWindow.transform.SetAsLastSibling();            //메인화면보다 오른쪽
                    targetWindow.gameObject.SetActive(true);
                    break;
            }

            if (isNeedEffect) {
                menu.transform.Find("Lock").GetComponent<MenuLocker>().Unlock();
            }
            else menu.transform.Find("Lock").GetComponent<MenuLocker>().UnlockWithNoEffect();
        }
        else {
            if(translatedKeyword == "AI") {
                UnlockDeckEditButtons(isNeedEffect);
            }
            else {
                menu.transform.Find("Lock").GetComponent<MenuLocker>().Unlock();
            }
        }

        int mainSibilingIndex = MainScrollSnapContent.Find("MainWindow").GetSiblingIndex();
        RefreshScrollSnap(mainSibilingIndex);
    }

    private void UnlockDeckEditButtons(bool isNeedEffect) {
        foreach (Transform deckObject in deckEditListParent) {
            if (deckObject.GetSiblingIndex() == 0) continue;
            var buttons = deckObject.Find("DeckObject/Buttons");
            if (isNeedEffect) {
                if (buttons.Find("DeleteBtn/Lock") != null) {
                    buttons.Find("DeleteBtn/Lock").GetComponent<MenuLocker>().Unlock();
                    buttons.Find("AiBattleBtn/Lock").GetComponent<MenuLocker>().Unlock();
                }
            }
            else {
                if(buttons.Find("DeleteBtn/Lock") != null) {
                    buttons.Find("DeleteBtn/Lock").GetComponent<MenuLocker>().UnlockWithNoEffect();
                    buttons.Find("AiBattleBtn/Lock").GetComponent<MenuLocker>().UnlockWithNoEffect();
                }
            }
        }
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
            case "퀘스트":
                translatedKeyword = "Quest";
                break;
            case "부대편집":
                translatedKeyword = "DeckEdit";
                break;
            case "보급상자":
                translatedKeyword = "RewardBox";
                break;
            case "상점":
                translatedKeyword = "Shop";
                break;
            case "ai대전":
                translatedKeyword = "AI";
                break;
            case "모드":
                translatedKeyword = "Mode";
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
        if (keyword == "Dictionary") return true;
        if (keyword == "Shop") return true;
        return false;
    }
}
