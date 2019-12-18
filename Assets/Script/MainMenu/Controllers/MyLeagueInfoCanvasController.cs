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
        yield return 0;
        currentMMRValue.text = newLeagueInfo.ratingPoint.ToString();
        currentMMRIndicator.text = newLeagueInfo.ratingPoint.ToString();
        mmrName.text = newLeagueInfo.rankDetail.minorRankName;
        MMRDownStandardValue.text = newLeagueInfo.rankDetail.pointOverThen.ToString();
        MMRUpStandardValue.text = newLeagueInfo.rankDetail.pointLessThen.ToString();

        int pointOverThen = newLeagueInfo.rankDetail.pointOverThen;
        int pointLessThen = newLeagueInfo.rankDetail.pointLessThen;
        prevMaxMMRSlider.maxValue = pointLessThen - pointOverThen;
        currentMMRSlider.maxValue = pointLessThen - pointOverThen;

        int ratingPointTop = newLeagueInfo.ratingPointTop ?? default(int);
        prevMaxMMRSlider.value = ratingPointTop;
        currentMMRSlider.value = newLeagueInfo.ratingPoint - pointOverThen;

        rankIcon.sprite = accountManager.resource.rankIcons[newLeagueInfo.rankDetail.minorRankName];
    }

    IEnumerator MakeLeaderBoardList() {
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
