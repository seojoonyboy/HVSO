using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RankTableViewController : MonoBehaviour {
    [SerializeField] ScrollRect scrollRect;
    [SerializeField] Transform content;
    [SerializeField] GameObject rankObject;
    [SerializeField] HUDController hudController;
    [SerializeField] MyLeagueInfoCanvasController myLeagueInfoCanvasController;    
    [SerializeField] Sprite[] rankObjectBackgrounds, rankObjectLines;

    AccountManager accountManager;
    NoneIngameSceneEventHandler eventHandler;
    void Awake() {
        accountManager = AccountManager.Instance;
        eventHandler = NoneIngameSceneEventHandler.Instance;
    }

    void Start() {
        eventHandler.AddListener(NoneIngameSceneEventHandler.EVENT_TYPE.API_RANK_TABLE_RECEIVED, OnTableLoaded);
    }

    private void OnTableLoaded(Enum Event_Type, Component Sender, object Param) {

    }

    void OnEnable() {
        EscapeKeyController.escapeKeyCtrl.AddEscape(OnBackButton);
        hudController.SetHeader(HUDController.Type.ONLY_BAKCK_BUTTON);
        hudController.SetBackButton(() => {
            OnBackButton();
        });

        ClearList();
        MakeList();
    }

    void OnDisable() {
        //eventHandler.RemoveListener(NoneIngameSceneEventHandler.EVENT_TYPE.API_RANK_TABLE_RECEIVED, OnTableLoaded);
    }

    void OnBackButton() {
        EscapeKeyController.escapeKeyCtrl.RemoveEscape(OnBackButton);
        hudController.SetBackButton(() => {
        if (BattleReadySceneController.instance != null && BattleReadySceneController.instance.gameObject.activeSelf) {
                myLeagueInfoCanvasController.OffPanelByBattleReady();
            }
            else {
                myLeagueInfoCanvasController.OffPanelByMain();
            }
        });
        gameObject.SetActive(false);
    }

    void ClearList() {
        foreach(Transform child in content) {
            Destroy(child.gameObject);
        }
    }

    GameObject myObject;
    void MakeList() {
        string myRankName = accountManager.scriptable_leagueData.leagueInfo.rankDetail.minorRankName;
        int myMedalNum = accountManager.scriptable_leagueData.leagueInfo.ratingPoint;
        var tableData = accountManager.rankTable;

        int index = 0;
        int rank = 0;
        foreach(AccountManager.RankTableRow row in tableData) {
            GameObject rankObj = Instantiate(rankObject);
            rankObj.SetActive(true);
            rankObj.transform.SetParent(content, true);
            rankObj.transform.SetAsLastSibling();
            rankObj.name = "item_" + index;

            var rankTableRow = rankObj.GetComponent<dataModules.RankTableRow>();
            rankTableRow.mmr.text = row.pointOverThen.ToString();

            //Logger.Log("!! : " + row.minorRankName);
            rankTableRow.minorRankName.text = accountManager
                .GetComponent<Fbl_Translator>()
                .GetLocalizedText("Tier", row.minorRankName);

            rankTableRow.data = row;

            if (accountManager.resource.rankIcons.ContainsKey(row.id.ToString())) {
                rankTableRow.rankIcon.sprite = accountManager.resource.rankIcons[row.id.ToString()];
            }

            if (row.minorRankName == myRankName) {
                rankTableRow.background.sprite = GetBackgroundImage(Category.ME);
                rankTableRow.topLine.sprite = GetLineImage(Category.ME);

                rankTableRow.myLeagueMark.SetActive(true);
                myObject = rankObj;
                rank = index;
            }
            else {
                if (rankTableRow.data.pointLessThen != null && rankTableRow.data.pointLessThen < 2600) {
                    rankTableRow.background.sprite = GetBackgroundImage(Category.NORMAL);
                    rankTableRow.topLine.sprite = GetLineImage(Category.NORMAL);
                }
                else {
                    rankTableRow.background.sprite = GetBackgroundImage(Category.HIGH);
                    rankTableRow.topLine.sprite = GetLineImage(Category.HIGH);
                }
            }
            rankTableRow.topLine.transform.Find("max/Text").GetComponent<Text>().text = row.pointLessThen.ToString();

            if (row.pointLessThen != null && row.pointOverThen != null) {
                int pointGap = (int)row.pointLessThen - (int)row.pointOverThen;
                int myPointRate = (int)myMedalNum - (int)row.pointOverThen;
                rankTableRow.pointSlider.value = (float)myPointRate / (float)pointGap * 100;
                rankTableRow.pointSlider.handleRect.transform.GetChild(0).GetComponent<Text>().text = myMedalNum.ToString();
                if(myMedalNum < row.pointLessThen && myMedalNum >= row.pointOverThen)
                    rankTableRow.pointSlider.handleRect.gameObject.SetActive(true);
                else
                    rankTableRow.pointSlider.handleRect.gameObject.SetActive(false);
            }
            else {
                rankTableRow.pointSlider.gameObject.SetActive(false);
                rankTableRow.pointSlider.handleRect.gameObject.SetActive(false);
            }

            if (rank % 2 == 0) {
                if (index % 2 == 1) {
                    Color backColor = rankTableRow.background.color;
                    backColor.a = 0;
                    rankTableRow.background.color = backColor;
                }
            }
            else {
                if (index % 2 == 0) {
                    Color backColor = rankTableRow.background.color;
                    backColor.a = 0;
                    rankTableRow.background.color = backColor;
                }
            }
            index++;
        }

        GameObject topTier = Instantiate(rankObject);
        topTier.SetActive(true);
        topTier.transform.SetParent(content, true);
        topTier.transform.SetAsFirstSibling();
        topTier.name = "item_" + index++;

        var _rankTableRow = topTier.GetComponent<dataModules.RankTableRow>();
        AccountManager.RankTableRow topRow = new AccountManager.RankTableRow();
        topRow.pointOverThen = 3000;
        topRow.minorRankName = "살아있는 전설";
        topRow.id = 15;

        _rankTableRow.mmr.text = topRow.pointOverThen.ToString();
        _rankTableRow.minorRankName.text = topRow.minorRankName;
        _rankTableRow.data = topRow;
        _rankTableRow.rankIcon.sprite = accountManager.resource.rankIcons[topRow.id.ToString()];
        _rankTableRow.background.sprite = GetBackgroundImage(Category.HIGH);
        _rankTableRow.topLine.sprite = GetLineImage(Category.HIGH);
        _rankTableRow.pointSlider.gameObject.SetActive(false);
        _rankTableRow.pointSlider.handleRect.gameObject.SetActive(false);

        //medalSlider.value = 

        StartCoroutine(MoveScrollToMyRank());
    }

    IEnumerator MoveScrollToMyRank() {
        if (myObject != null) {
            yield return new WaitForEndOfFrame();
            float normalizedPosition = (float)myObject.transform.GetSiblingIndex() / (float)content.childCount;
            scrollRect.verticalNormalizedPosition = 1 - normalizedPosition;
        }
    }

    public Sprite GetBackgroundImage(Category category) {
        switch (category) {
            case Category.HIGH:
                return rankObjectBackgrounds[2];
            case Category.NORMAL:
                return rankObjectBackgrounds[0];
            case Category.ME:
                return rankObjectBackgrounds[1];
            default:
                return rankObjectBackgrounds[0];
        }
    }

    public Sprite GetLineImage(Category category) {
        switch (category) {
            case Category.HIGH:
                return rankObjectLines[2];
            case Category.NORMAL:
                return rankObjectLines[0];
            case Category.ME:
                return rankObjectLines[1];
            default:
                return rankObjectLines[0];
        }
    }

    public enum Category {
        HIGH,
        NORMAL,
        ME
    }
}
