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
    public bool isAllUnlocked = false;

    void Awake() {
        eventHandler = NoneIngameSceneEventHandler.Instance;
        eventHandler.AddListener(NoneIngameSceneEventHandler.EVENT_TYPE.API_TUTORIAL_INFOS_UPDATED, OnTutorialInfoUpdated);
    }

    void Start() {
        AccountManager.Instance.RequestTutorialUnlockInfos(true);
    }

    void OnDestroy() {
        eventHandler.RemoveListener(NoneIngameSceneEventHandler.EVENT_TYPE.API_TUTORIAL_INFOS_UPDATED, OnTutorialInfoUpdated);
    }

    private void OnTutorialInfoUpdated(Enum Event_Type, Component Sender, object Param) {
        //모두 해금된 상태
        if (isAllUnlocked) {
            foreach(KeyValuePair<string, GameObject> keyValuePair in menues) {
                keyValuePair.Value.transform.Find("Lock").GetComponent<MenuLocker>().UnlockWithNoEffect();
            }
            return;
        }

        object[] parm = (object[])Param;
        bool isInitRequest = (bool)parm[0];

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

        var questInfos = AccountManager.Instance.questInfos;
        if(questInfos != null) {
            isAllUnlocked = !AccountManager.Instance.questInfos.Exists(x => x.cleared == false);
        }
        
        if (isAllUnlocked) {
            Logger.Log("///////////////////////");
            Logger.Log("모두 해금됨");

            GetComponent<MenuSceneController>().CheckDailyQuest();
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
        if(translatedKeyword == "Story") {
            string storyAlreadyUnlocked = PlayerPrefs.GetString("StoryUnlocked");
            if(storyAlreadyUnlocked != "true") {
                menues["Mode"].transform.parent.parent.Find("SelectedModeImage/Lock").GetComponent<MenuLocker>().OnlyUnlockEffect();
                PlayerPrefs.SetString("StoryUnlocked", "true");
            }
            else {
                menues["Mode"].transform.parent.parent.Find("SelectedModeImage/Lock").GetComponent<MenuLocker>().UnlockWithNoEffect();
            }
            
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

            //MainScrollSnapContent.parent.GetComponent<HorizontalScrollSnap>().CurrentPage = 2;
            //int mainSibilingIndex = MainScrollSnapContent.Find("MainWindow").GetSiblingIndex();
            MainScrollSnapContent.parent.GetComponent<HorizontalScrollSnap>().UpdateLayout();

            //MainScrollSnapContent.parent.GetComponent<HorizontalScrollSnap>().GoToScreen(mainSibilingIndex);

            if (isNeedEffect) {
                menu.transform.Find("Lock").GetComponent<MenuLocker>().Unlock();
            }
            else menu.transform.Find("Lock").GetComponent<MenuLocker>().UnlockWithNoEffect();
        }
        else {
            if(translatedKeyword.Contains("HumanBaseDeck") || translatedKeyword.Contains("OrcBaseDeck")) {
                foreach (Transform deckObject in deckEditListParent) {
                    if (deckObject.GetSiblingIndex() == 0) continue;
                    var buttons = deckObject.Find("DeckObject/Buttons");
                    if (isNeedEffect) {
                        buttons.Find("DeleteBtn/Lock").GetComponent<MenuLocker>().Unlock();
                        buttons.Find("AiBattleBtn/Lock").GetComponent<MenuLocker>().Unlock();
                    }
                    else {
                        buttons.Find("DeleteBtn/Lock").GetComponent<MenuLocker>().UnlockWithNoEffect();
                        buttons.Find("AiBattleBtn/Lock").GetComponent<MenuLocker>().UnlockWithNoEffect();
                    }
                }
            }
            else {
                menu.transform.Find("Lock").GetComponent<MenuLocker>().Unlock();
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
