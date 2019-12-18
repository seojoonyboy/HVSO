using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MyLeagueInfoCanvasController : MonoBehaviour {
    [SerializeField] Transform myInfoArea, leaderBoardArea;
    [SerializeField] Transform rankingContent;
    [SerializeField] GameObject mmrSliderArea;
    [SerializeField] GameObject rankingPool;
    [SerializeField] HUDController hudController;
    [SerializeField] BattleReadySceneController battleReadySceneController;

    NoneIngameSceneEventHandler eventHandler;
    AccountManager accountManager;
    AccountManager.LeagueInfo prevLeagueInfo, newLeagueInfo;

    Transform banner;
    Image 
        rankIcon,
        MMRDownStandardIcon,
        MMRUpStandardIcon;
    TextMeshProUGUI 
        mmrName, 
        MMRDownStandardValue,
        MMRUpStandardValue,
        currentMMRIndicator;
    Text currentMMRValue;
    Slider prevMaxMMRSlider, currentMMRSlider;

    // Start is called before the first frame update
    void Awake() {
        accountManager = AccountManager.Instance;
        eventHandler = NoneIngameSceneEventHandler.Instance;

        banner = myInfoArea.Find("Banner");
        rankIcon = banner.Find("RankIcon").GetComponent<Image>();
        mmrName = banner.Find("TierName/Text").GetComponent<TextMeshProUGUI>();
        MMRDownStandardValue = mmrSliderArea.transform.Find("CurrentSlider/MMRDownStandardValue").GetComponent<TextMeshProUGUI>();
        MMRUpStandardValue = mmrSliderArea.transform.Find("CurrentSlider/MMRUpStandardValue").GetComponent<TextMeshProUGUI>();
        currentMMRValue = banner.Find("Trophy/Value").GetComponent<Text>();
        currentMMRIndicator = mmrSliderArea.transform.Find("CurrentSlider/FillArea/Fill/CurrentMMR/Text").GetComponent<TextMeshProUGUI>();
        MMRDownStandardIcon = mmrSliderArea.transform.Find("CurrentSlider/MMRDownStandardIcon").GetComponent<Image>();
        MMRUpStandardIcon = mmrSliderArea.transform.Find("CurrentSlider/MMRUpStandardIcon").GetComponent<Image>();

        prevMaxMMRSlider = mmrSliderArea.transform.Find("PrevSlider").GetComponent<Slider>();
        currentMMRSlider = mmrSliderArea.transform.Find("CurrentSlider").GetComponent<Slider>();
    }

    void Start() {

    }

    void OnPanel() {
        gameObject.SetActive(true);
        EscapeKeyController.escapeKeyCtrl.AddEscape(OffPanel);
        
        eventHandler.AddListener(NoneIngameSceneEventHandler.EVENT_TYPE.API_LEAGUE_INFO_UPDATED, LeagueInfoLoaded);
        accountManager.RequestLeagueInfo();
    }

    void OnDisable() {
        eventHandler.RemoveListener(NoneIngameSceneEventHandler.EVENT_TYPE.API_LEAGUE_INFO_UPDATED, LeagueInfoLoaded);
        StopAllCoroutines();
    }

    void OnDestroy() {
        StopAllCoroutines();
    }

    private void LeagueInfoLoaded(Enum Event_Type, Component Sender, object Param) {
        newLeagueInfo = accountManager.scriptable_leagueData.leagueInfo;
        prevLeagueInfo = accountManager.scriptable_leagueData.prevLeagueInfo;

        StartCoroutine(ShowLeagueInfoChange());
    }

    IEnumerator ShowLeagueInfoChange() {
        yield return SliderProceed();
        yield return MMRChangeProceed();

        yield return MakeLeaderBoardList();
    }

    IEnumerator SliderProceed() {
        yield return 0;
    }

    IEnumerator MMRChangeProceed() {
        currentMMRValue.text = prevLeagueInfo.ratingPoint.ToString();
        currentMMRIndicator.text = prevLeagueInfo.ratingPoint.ToString();
        mmrName.text = prevLeagueInfo.rankDetail.minorRankName;
        MMRDownStandardValue.text = prevLeagueInfo.rankDetail.pointOverThen.ToString();
        MMRUpStandardValue.text = prevLeagueInfo.rankDetail.pointLessThen.ToString();

        int pointOverThen = prevLeagueInfo.rankDetail.pointOverThen; //��� ���� �ּڰ�
        int pointLessThen = prevLeagueInfo.rankDetail.pointLessThen; //��� ���� �ִ�
        prevMaxMMRSlider.maxValue = pointLessThen - pointOverThen;
        currentMMRSlider.maxValue = pointLessThen - pointOverThen;

        int ratingPointTop = prevLeagueInfo.ratingPointTop ?? default(int);
        prevMaxMMRSlider.value = ratingPointTop;
        currentMMRSlider.value = prevLeagueInfo.ratingPoint - pointOverThen;

        rankIcon.sprite = accountManager.resource.rankIcons[prevLeagueInfo.rankDetail.minorRankName];
        currentMMRIndicator.text = prevLeagueInfo.ratingPoint.ToString();

        //start testcode
        //int prevRatingPoint = prevLeagueInfo.ratingPoint + 170;
        //newLeagueInfo.rankDetail.minorRankName = "�������� ��θӸ�";
        //newLeagueInfo.ratingPoint = prevRatingPoint;
        //end testcode

        Logger.Log("ratingPoint : " + newLeagueInfo.ratingPoint);
        Logger.Log("prevRatingPoint : " + prevLeagueInfo.ratingPoint);

        //mmr ��ȭ�� �ִ��� check
        bool isMMRChanged = false;
        isMMRChanged = prevLeagueInfo.ratingPoint != newLeagueInfo.ratingPoint;
        if (isMMRChanged) {
            int changeAmount = newLeagueInfo.ratingPoint - prevLeagueInfo.ratingPoint;
            int prevMMR = prevLeagueInfo.ratingPoint;

            //��ũ ��ȭ�� �ִ��� check
            bool isTierChanged = false;
            isTierChanged = prevLeagueInfo.rankDetail.minorRankName != newLeagueInfo.rankDetail.minorRankName;

            //mmr ����
            if (changeAmount > 0) {
                //�±�
                if (isTierChanged) {
                    Logger.Log("�±� �߻�");
                    currentMMRSlider.maxValue = prevLeagueInfo.rankDetail.pointLessThen - prevLeagueInfo.rankDetail.pointOverThen;
                    yield return ProceedNewMMRSlider(
                        prevMMR - prevLeagueInfo.rankDetail.pointOverThen,
                        (int)currentMMRSlider.maxValue,
                        prevLeagueInfo.rankDetail.pointOverThen);

                    MMRDownStandardValue.text = newLeagueInfo.rankDetail.pointOverThen.ToString();
                    MMRUpStandardValue.text = newLeagueInfo.rankDetail.pointLessThen.ToString();
                    mmrName.text = newLeagueInfo.rankDetail.minorRankName;
                    rankIcon.sprite = accountManager.resource.rankIcons[newLeagueInfo.rankDetail.minorRankName];

                    currentMMRSlider.maxValue = newLeagueInfo.rankDetail.pointLessThen - newLeagueInfo.rankDetail.pointOverThen;
                    yield return ProceedNewMMRSlider(
                        newLeagueInfo.rankDetail.pointOverThen, 
                        newLeagueInfo.ratingPoint - newLeagueInfo.rankDetail.pointOverThen,
                        newLeagueInfo.rankDetail.pointOverThen);
                }
                else {
                    Logger.Log("MMR ����");
                    currentMMRSlider.maxValue = newLeagueInfo.rankDetail.pointLessThen - newLeagueInfo.rankDetail.pointOverThen;
                    yield return ProceedNewMMRSlider(
                        prevLeagueInfo.ratingPoint - prevLeagueInfo.rankDetail.pointOverThen,
                        newLeagueInfo.ratingPoint - newLeagueInfo.rankDetail.pointOverThen,
                        newLeagueInfo.rankDetail.pointOverThen);
                }
            }
            //mmr ����
            else if(changeAmount < 0) {
                //����
                if (isTierChanged) {
                    Logger.Log("���� �߻�");
                    currentMMRSlider.maxValue = prevLeagueInfo.rankDetail.pointLessThen - prevLeagueInfo.rankDetail.pointOverThen;
                    yield return ProceedNewMMRSlider(
                        prevLeagueInfo.ratingPoint - prevLeagueInfo.rankDetail.pointOverThen,
                        prevLeagueInfo.rankDetail.pointOverThen - prevLeagueInfo.rankDetail.pointOverThen,
                        prevLeagueInfo.rankDetail.pointOverThen);

                    MMRDownStandardValue.text = newLeagueInfo.rankDetail.pointOverThen.ToString();
                    MMRUpStandardValue.text = newLeagueInfo.rankDetail.pointLessThen.ToString();
                    mmrName.text = newLeagueInfo.rankDetail.minorRankName;
                    rankIcon.sprite = accountManager.resource.rankIcons[newLeagueInfo.rankDetail.minorRankName];

                    currentMMRSlider.maxValue = newLeagueInfo.rankDetail.pointLessThen - newLeagueInfo.rankDetail.pointOverThen;
                    yield return ProceedNewMMRSlider(
                        newLeagueInfo.rankDetail.pointLessThen,
                        newLeagueInfo.ratingPoint - newLeagueInfo.rankDetail.pointOverThen,
                        newLeagueInfo.rankDetail.pointOverThen);
                }
                else {
                    Logger.Log("MMR ����");
                    currentMMRSlider.maxValue = newLeagueInfo.rankDetail.pointLessThen - newLeagueInfo.rankDetail.pointOverThen;
                    yield return ProceedNewMMRSlider(
                        newLeagueInfo.rankDetail.pointLessThen,
                        newLeagueInfo.ratingPoint - newLeagueInfo.rankDetail.pointOverThen,
                        newLeagueInfo.rankDetail.pointOverThen);
                }
            }
        }
    }

    IEnumerator MakeLeaderBoardList() {
        yield return 0;
    }

    IEnumerator ProceedNewMMRSlider(int from, int to, int offset) {
        currentMMRSlider.value = from;
        if (from < to) {
            while(from <= to) {
                yield return new WaitForSeconds(0.01f);
                currentMMRIndicator.text = (currentMMRSlider.value + offset).ToString();
                currentMMRSlider.value += 1;
                from += 1;
            }
        }
        else {
            while (from >= to) {
                yield return new WaitForSeconds(0.01f);
                currentMMRIndicator.text = (currentMMRSlider.value + offset).ToString();
                currentMMRSlider.value -= 1;
                from-= 1;
            }
        }
        yield return 0;
    }

    public void OnPanelByMain() {
        hudController.SetHeader(HUDController.Type.RESOURCE_ONLY_WITH_BACKBUTTON);
        hudController.SetBackButton(() => {
            OffPanelByMain();
        });

        OnPanel();
    }

    public void OnPanelByBattleReady() {
        hudController.SetHeader(HUDController.Type.RESOURCE_ONLY_WITH_BACKBUTTON);
        hudController.SetBackButton(() => {
            OffPanelByBattleReady();
        });

        OnPanel();
    }

    void OffPanel() {
        gameObject.SetActive(false);

        EscapeKeyController.escapeKeyCtrl.RemoveEscape(OffPanel);
    }

    public void OffPanelByBattleReady() {
        hudController.SetBackButton(() => battleReadySceneController.OnBackButton());
        OffPanel();
    }

    public void OffPanelByMain() {
        hudController.SetHeader(HUDController.Type.SHOW_USER_INFO);
        OffPanel();
    }
}
