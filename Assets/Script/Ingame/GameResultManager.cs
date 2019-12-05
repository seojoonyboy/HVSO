using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Spine;
using Spine.Unity;
using UnityEngine.Events;
using System;
using Newtonsoft.Json.Linq;
using UniRx;
using System.Text;
using System.Linq;

public class GameResultManager : MonoBehaviour {
    public GameObject SocketDisconnectedUI;
    [SerializeField] Transform BgCanvas;
    [SerializeField] GameObject tierChangeEffectModal;

    private float lv;
    private float exp;
    private float lvExp;
    private float nextLvExp;
    private int getExp = 0;
    private int crystal = 0;
    private int supply = 0;
    private int getSupply = 0;
    private int additionalSupply = 0;
    private int supplyBox = 0;
    private bool isHuman;
    private string result;

    public UnityEvent EndRewardLoad = new UnityEvent();
    public LeagueData scriptable_leagueData;

    string battleType;
    private void Awake() {
        lv = AccountManager.Instance.userResource.lv;
        exp = AccountManager.Instance.userResource.exp;
        lvExp = AccountManager.Instance.userResource.lvExp;
        supply = AccountManager.Instance.userResource.supply;
        nextLvExp = AccountManager.Instance.userResource.nextLvExp;
        gameObject.SetActive(false);

        battleType = PlayerPrefs.GetString("SelectedBattleType");
        if(battleType == "solo") {
            ChangeResultButtonFunction();
        }
        else {
            Button btn = transform.Find("FirstWindow/GotoRewardButton").GetComponent<Button>();
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() => {
                OpenSecondWindow();
            });
            //btn.transform.GetChild(0).GetComponent<TMPro.TextMeshProUGUI>().text = "보상으로";
        }
    }

    void OnDestroy() {
        if (observer_1 != null) observer_1.Dispose();
        if (observer_2 != null) observer_2.Dispose();
    }

    public void ExtraRewardReceived(JObject data) {
        TMPro.TextMeshProUGUI doubleCoupons = transform.Find("SecondWindow/PlayerSupply/ExtraSupply/DoubleButton/Value").GetComponent<TMPro.TextMeshProUGUI>();
        doubleCoupons.text = data["couponLeft"].ToString();

        int supply = 0;
        int.TryParse(data["supply"].ToString(), out supply);

        Transform playerSup = transform.Find("SecondWindow/PlayerSupply");
        StartCoroutine(GetUserSupply(playerSup.Find("ExpSlider/Slider").GetComponent<Slider>(), supply, 0, 0, true));

        var button = transform.Find("SecondWindow/PlayerSupply/ExtraSupply/DoubleButton/Button").GetComponent<Button>();
        button.enabled = false;
        button.GetComponent<Image>().color = new Color32(106, 106, 106, 255);
        button.transform.GetChild(0).GetComponent<TMPro.TextMeshProUGUI>().color = new Color32(106, 106, 106, 255);
        button.transform.GetChild(1).GetComponent<Text>().color = new Color32(106, 106, 106, 255);
    }

    public void OnReturnBtn() {
        PlayMangement.instance.SocketHandler.SendMethod("end_game");
        FBL_SceneManager.Instance.LoadScene(FBL_SceneManager.Scene.MAIN_SCENE);
    }

    /// <summary>
    /// 대전 준비화면으로 바로 이동 버튼
    /// </summary>
    public void OnBattleReadyBtn() {
        AccountManager.Instance.visitDeckNow = 1;
        OnReturnBtn();
    }

    /// <summary>
    /// 매칭 화면으로 바로 이동 버튼
    /// </summary>
    public void OnNewGameBtn() {
        PlayerPrefs.SetString("SelectedBattleType", "league");
        FBL_SceneManager.Instance.LoadScene(FBL_SceneManager.Scene.CONNECT_MATCHING_SCENE);
    }

    public void SocketErrorUIOpen(bool friendOut) {
        SocketDisconnectedUI.SetActive(true);
        if (friendOut)
            SocketDisconnectedUI.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = "상대방이 게임을 \n 종료했습니다.";
    }

    public void OnMoveSceneBtn() {
        FBL_SceneManager.Instance.LoadScene(FBL_SceneManager.Scene.MAIN_SCENE);
    }

    public void SetResultWindow(string result, bool isHuman) {
        this.isHuman = isHuman;
        this.result = result;
        PlayerPrefs.DeleteKey("ReconnectData");
        gameObject.SetActive(true);
        transform.Find("FirstWindow").gameObject.SetActive(true);
        BgCanvas.gameObject.SetActive(true);
        GameObject heroSpine = transform.Find("FirstWindow/HeroSpine/" + PlayMangement.instance.player.heroID).gameObject;
        heroSpine.SetActive(true);
        iTween.ScaleTo(heroSpine, iTween.Hash("scale", Vector3.one, "islocal", true, "time", 0.3f));
        getExp = PlayMangement.instance.socketHandler.result.reward.userExp;
        getSupply = PlayMangement.instance.socketHandler.result.reward.supply;
        additionalSupply = PlayMangement.instance.socketHandler.result.reward.x2supply;
        //additionalSupply = PlayMangement.instance.socketHandler.result.reward.additionalSupply;
        SoundManager.Instance.bgmController.SoundTrackLoopOff();

        SkeletonGraphic backSpine;
        SkeletonGraphic frontSpine;
        switch (result) {
            case "win": {
                    BgCanvas.Find("Particle/First").gameObject.SetActive(true);
                    heroSpine.GetComponent<SkeletonGraphic>().AnimationState.SetAnimation(0, "IDLE", true);
                    backSpine = transform.Find("FirstWindow/BackSpine/WinningBack").GetComponent<SkeletonGraphic>();
                    frontSpine = transform.Find("FirstWindow/FrontSpine/WinningFront").GetComponent<SkeletonGraphic>();
                    transform.Find("SecondWindow/BackSpine/WinningBack").gameObject.SetActive(true);
                    transform.Find("SecondWindow/FrontSpine/WinningFront").gameObject.SetActive(true);
                    transform.Find("SecondWindow/BackSpine/LosingBack").gameObject.SetActive(false);
                    transform.Find("SecondWindow/FrontSpine/LosingFront").gameObject.SetActive(false);
                    SoundManager.Instance.bgmController.PlaySoundTrack(BgmController.BgmEnum.VICTORY);
                }
                break;
            case "lose": {
                    BgCanvas.Find("Particle/First").gameObject.SetActive(false);
                    heroSpine.GetComponent<SkeletonGraphic>().Initialize(true);
                    heroSpine.GetComponent<SkeletonGraphic>().Update(0);
                    heroSpine.GetComponent<SkeletonGraphic>().AnimationState.SetAnimation(0, "DEAD", false);
                    backSpine = transform.Find("FirstWindow/BackSpine/LosingBack").GetComponent<SkeletonGraphic>();
                    frontSpine = transform.Find("FirstWindow/FrontSpine/LosingFront").GetComponent<SkeletonGraphic>();
                    transform.Find("SecondWindow/BackSpine/WinningBack").gameObject.SetActive(false);
                    transform.Find("SecondWindow/FrontSpine/WinningFront").gameObject.SetActive(false);
                    transform.Find("SecondWindow/BackSpine/LosingBack").gameObject.SetActive(true);
                    transform.Find("SecondWindow/FrontSpine/LosingFront").gameObject.SetActive(true);
                    SoundManager.Instance.bgmController.PlaySoundTrack(BgmController.BgmEnum.DEFEAT);
                }
                break;
            default:
                backSpine = null;
                frontSpine = null;
                break;
        }
        backSpine.Initialize(true);
        backSpine.Update(0);
        frontSpine.Initialize(true);
        frontSpine.Update(0);
        backSpine.gameObject.SetActive(true);
        frontSpine.gameObject.SetActive(true);
        backSpine.AnimationState.SetAnimation(0, "01.start", false);
        backSpine.AnimationState.AddAnimation(1, "02.play", true, 0.8f);
        if (isHuman) 
            frontSpine.Skeleton.SetSkin("human");
        else 
            frontSpine.Skeleton.SetSkin("orc");
        frontSpine.AnimationState.SetAnimation(0, "01.start", false);
        frontSpine.AnimationState.AddAnimation(1, "02.play", true, 0.8f);

        OnTimerToExit();
    }

    public void OpenSecondWindow() {
        transform.Find("FirstWindow").gameObject.SetActive(false);
        transform.Find("SecondWindow").gameObject.SetActive(true);
        BgCanvas.Find("Particle/First").gameObject.SetActive(false);
        if (isHuman) 
            BgCanvas.Find("BackgroundImg/human").gameObject.SetActive(true);
        else 
            BgCanvas.Find("BackgroundImg/orc").gameObject.SetActive(true);
        if (result == "win")
            BgCanvas.Find("Particle/Second").gameObject.SetActive(true);
        //transform.Find("SecondWindow/Buttons/FindNewGame").GetComponent<Button>().interactable = false;
        transform.Find("SecondWindow/Buttons/BattleReady").GetComponent<Button>().interactable = true;

        StartCoroutine(SetRewards());
        var battleType = PlayerPrefs.GetString("SelectedBattleType");
        if (battleType == "league" || battleType == "leagueTest") {
            StartCoroutine(SetLeagueData());
        }
        else {
            //테스트 코드
            //StartCoroutine(SetTestLeagueData());
        }
    }

    public IEnumerator SetRewards() {
        Transform rewards = transform.Find("SecondWindow/ResourceRewards");
        yield return new WaitForSeconds(0.1f);
        Slider expSlider = transform.Find("SecondWindow/PlayerExp/ExpSlider/Slider").GetComponent<Slider>();
        expSlider.value = exp / lvExp;

        Transform playerExp = transform.Find("SecondWindow/PlayerExp");
        playerExp.Find("LevelIcon/Value").GetComponent<Text>().text = lv.ToString();
        playerExp.Find("UserName").GetComponent<TMPro.TextMeshProUGUI>().text = AccountManager.Instance.userData.nickName;
        playerExp.Find("ExpSlider/ExpValue").GetComponent<TMPro.TextMeshProUGUI>().text = ((int)exp).ToString();
        playerExp.Find("ExpSlider/ExpMaxValue").GetComponent<TMPro.TextMeshProUGUI>().text = "/" + ((int)lvExp).ToString();
        iTween.ScaleTo(playerExp.gameObject, iTween.Hash("scale", Vector3.one, "islocal", true, "time", 0.5f));
        yield return new WaitForSeconds(0.1f);
        iTween.ScaleTo(transform.Find("SecondWindow/PlayerMmr").gameObject, iTween.Hash("scale", Vector3.one, "islocal", true, "time", 0.5f));
        yield return new WaitForSeconds(0.1f);
        Transform playerSup = transform.Find("SecondWindow/PlayerSupply");
        iTween.ScaleTo(playerSup.gameObject, iTween.Hash("scale", Vector3.one, "islocal", true, "time", 0.5f));
        playerSup.Find("ExpSlider/Slider").GetComponent<Slider>().value = supply / 100.0f;
        playerSup.Find("ExpSlider/SupValue").GetComponent<TMPro.TextMeshProUGUI>().text = supply.ToString();
        yield return new WaitForSeconds(0.1f);
        iTween.ScaleTo(transform.Find("SecondWindow/Buttons").gameObject, iTween.Hash("scale", Vector3.one, "islocal", true, "time", 0.5f));
        if (getExp > 0) {
            yield return new WaitForSeconds(0.5f);
            yield return StartCoroutine(GetUserExp(expSlider));
        }
        if (getSupply > 0) {
            yield return new WaitForSeconds(0.5f);
            yield return StartCoroutine(GetUserSupply(playerSup.Find("ExpSlider/Slider").GetComponent<Slider>(), getSupply, additionalSupply));
        }

        TMPro.TextMeshProUGUI doubleCoupons = transform.Find("SecondWindow/PlayerSupply/ExtraSupply/DoubleButton/Value").GetComponent<TMPro.TextMeshProUGUI>();
        doubleCoupons.text = AccountManager.Instance.userData.supplyX2Coupon.ToString();
        //if (supply > 0) {
        //    rewards.GetChild(0).gameObject.SetActive(true);
        //    rewards.GetChild(0).Find("Text/Value").GetComponent<TMPro.TextMeshProUGUI>().text = supply.ToString();
        //    Transform additionalSupTxt = rewards.GetChild(0).Find("Text/Additional");
        //    if (additionalSupply > 0) {
        //        additionalSupTxt.gameObject.SetActive(true);
        //        additionalSupTxt.GetComponent<TMPro.TextMeshProUGUI>().text = "+" + additionalSupply.ToString();
        //    }
        //    else
        //        additionalSupTxt.gameObject.SetActive(false);
        //    yield return new WaitForSeconds(0.1f);
        //    iTween.ScaleTo(rewards.GetChild(0).gameObject, iTween.Hash("scale", Vector3.one, "islocal", true, "time", 0.5f));
        //}
    }

    public IEnumerator SetLeagueData() {
        var leagueInfo = scriptable_leagueData.leagueInfo;
        if (leagueInfo != null) {
            Transform secondWindow = transform.Find("SecondWindow");
            Transform playerMMR = secondWindow.Find("PlayerMmr");
            Image rankIcon = playerMMR.Find("RankIcon").GetComponent<Image>();
            var icons = AccountManager.Instance.resource.rankIcons;
            if (!string.IsNullOrEmpty(scriptable_leagueData.prevLeagueInfo.rankDetail.minorRankName) && icons.ContainsKey(scriptable_leagueData.prevLeagueInfo.rankDetail.minorRankName)) {
                rankIcon.sprite = icons[scriptable_leagueData.prevLeagueInfo.rankDetail.minorRankName];
            }
            else {
                rankIcon.sprite = icons["default"];
            }
            rankIcon.transform.Find("Name").GetComponent<TMPro.TextMeshProUGUI>().text = scriptable_leagueData.prevLeagueInfo.rankDetail.minorRankName;

            var description = playerMMR.Find("VictoryInfo").GetComponent<TMPro.TextMeshProUGUI>();
            StringBuilder sb = new StringBuilder();
            if(leagueInfo.winningStreak > 1) {
                sb
                    .Append("<color=yellow>")
                    .Append(leagueInfo.winningStreak)
                    .Append("</color> 연승중");
            }
            else if(leagueInfo.losingStreak > 1) {
                sb
                    .Append("<color=red>")
                    .Append(leagueInfo.losingStreak)
                    .Append("</color> 연패중");
            }
            description.text = sb.ToString();
            //MMR 증가
            if (scriptable_leagueData.leagueInfo.ratingPoint > scriptable_leagueData.prevLeagueInfo.ratingPoint) {
                coroutine = ProgressLeagueBar(scriptable_leagueData.prevLeagueInfo.ratingPoint, scriptable_leagueData.leagueInfo.ratingPoint, true, leagueInfo);
            }
            //MMR 감소
            else {
                coroutine = ProgressLeagueBar(scriptable_leagueData.prevLeagueInfo.ratingPoint, scriptable_leagueData.leagueInfo.ratingPoint, false, leagueInfo);
            }

            yield return StartCoroutine(coroutine);
            if(scriptable_leagueData.prevLeagueInfo.rankingBattleState != "normal") {
                Logger.Log("승급전 혹은 강등전 진행중!");
                yield return ShowLeagueEffect();
            }
            if(scriptable_leagueData.prevLeagueInfo.rankingBattleState == "normal" && scriptable_leagueData.leagueInfo.rankingBattleState != "normal") {
                Logger.Log("승급전 혹은 강등전 발생!");
                yield return ShowRankChangeChanceUI();
            }
            yield return ShowTierChangeEffect();

            if (icons.ContainsKey(leagueInfo.rankDetail.minorRankName)) {
                rankIcon.sprite = icons[leagueInfo.rankDetail.minorRankName];
                rankIcon.transform.Find("Name").GetComponent<TMPro.TextMeshProUGUI>().text = leagueInfo.rankDetail.minorRankName;
            }
            else {
                rankIcon.sprite = icons["default"];
            }
        }
        yield return 0;

    }

    /// <summary>
    /// 승급전 혹은 강등전 발생했다는거 보여주기
    /// </summary>
    /// <returns></returns>
    IEnumerator ShowRankChangeChanceUI() {
        Logger.Log("승급전/강등전 발생 이벤트 UI 처리");

        Transform secondWindow = transform.Find("SecondWindow");
        Transform playerMmr = secondWindow.Find("PlayerMmr");
        Transform expSlider = playerMmr.Find("ExpSlider");
        expSlider.Find("RankChangeEffect").gameObject.SetActive(true);

        var leagueInfo = scriptable_leagueData.leagueInfo;
        if(leagueInfo.rankingBattleState == "rank_up") {
            Logger.Log("Case 1");
            expSlider.Find("RankChangeEffect/Text").GetComponent<TMPro.TextMeshProUGUI>().text = "승급전 발생";
        }
        else if(leagueInfo.rankingBattleState == "rank_down") {
            Logger.Log("Case 2");
            expSlider.Find("RankChangeEffect/Text").GetComponent<TMPro.TextMeshProUGUI>().text = "강등전 발생";
        }
        else {
            Logger.Log("Unknown Case");
            expSlider.Find("RankChangeEffect").gameObject.SetActive(false);
        }
        yield return 0;
    }

    /// <summary>
    /// 승급전 결과 보여주기 (예 : 3판2선승제 => 하나씩 결과 보여주기)
    /// </summary>
    IEnumerator ShowLeagueEffect() {
        Logger.Log("승급전 진행중 관련 UI 처리");

        Transform secondWindow = transform.Find("SecondWindow");
        Transform playerMmr = secondWindow.Find("PlayerMmr");
        Transform expSlider = playerMmr.Find("ExpSlider");
        Transform rankBoard = playerMmr.Find("RankBoard");

        expSlider.gameObject.SetActive(false);
        rankBoard.gameObject.SetActive(true);
        Transform slotParent = rankBoard.Find("Bottom");

        var leagueInfo = scriptable_leagueData.leagueInfo;
        int slotCnt = 0;
        if(leagueInfo.rankingBattleState == "rank_up") {
            Logger.Log("Case 1");
            slotCnt = leagueInfo.rankDetail.rankUpBattleCount.battles;
            rankBoard.Find("Top/Text").GetComponent<TMPro.TextMeshProUGUI>().text = "승급전 진행중";
        }
        else if(leagueInfo.rankingBattleState == "rank_down") {
            Logger.Log("Case 2");
            slotCnt = leagueInfo.rankDetail.rankDownBattleCount.battles;
            rankBoard.Find("Top/Text").GetComponent<TMPro.TextMeshProUGUI>().text = "강등전 진행중";
        }
        else {
            Logger.Log("Unknown Case");
            expSlider.gameObject.SetActive(true);
            rankBoard.gameObject.SetActive(false);
        }

        for (int i = 0; i < slotCnt; i++) {
            slotParent.GetChild(i).gameObject.SetActive(true);
        }

        bool[] battleResults = leagueInfo.rankingBattleCount;
        if(battleResults != null) {
            for (int i = 0; i < battleResults.Length; i++) {
                if (battleResults[i]) {
                    slotParent.GetChild(i).Find("Win").gameObject.SetActive(true);
                    slotParent.GetChild(i).Find("Lose").gameObject.SetActive(false);
                }
                else {
                    slotParent.GetChild(i).Find("Win").gameObject.SetActive(false);
                    slotParent.GetChild(i).Find("Lose").gameObject.SetActive(true);
                }
                yield return new WaitForSeconds(0.2f);
            }
        }
        yield return 0;
    }

    /// <summary>
    /// 승급/강등 이펙트 보여주기
    /// </summary>
    IEnumerator ShowTierChangeEffect() {
        var leagueInfo = scriptable_leagueData.leagueInfo;
        var prevLeagueInfo = scriptable_leagueData.prevLeagueInfo;

        yield return new WaitForSeconds(1.0f);
        if (scriptable_leagueData.prevLeagueInfo.rankDetail.minorRankName != scriptable_leagueData.leagueInfo.rankDetail.minorRankName) {
            tierChangeEffectModal.SetActive(true);
            string animName = GetTierAnimName(leagueInfo.rankDetail.minorRankName);
            SkeletonGraphic skeletonGraphic = tierChangeEffectModal.transform.Find("Spine").GetComponent<SkeletonGraphic>();
            skeletonGraphic.Initialize(true);

            if (animName != null) skeletonGraphic.Skeleton.SetSkin(animName);
            skeletonGraphic.Skeleton.SetSlotsToSetupPose();

            var message = tierChangeEffectModal.transform.Find("Message").GetComponent<TMPro.TextMeshProUGUI>();

            //TODO : 승급인지 강등인지 구분 필요
            if (leagueInfo.ratingPoint - prevLeagueInfo.ratingPoint > 0) {
                message.text = leagueInfo.rankDetail.minorRankName + "으로 승급하셨습니다.";
                skeletonGraphic.AnimationState.SetAnimation(0, "UP", false);
                Logger.Log("승급!");
            }
            else if(leagueInfo.ratingPoint - prevLeagueInfo.ratingPoint < 0) {
                message.text = leagueInfo.rankDetail.minorRankName + "으로 강등되었습니다.";
                skeletonGraphic.AnimationState.SetAnimation(0, "DOWN", false);
                Logger.Log("강등!");
            }
            else {
                Logger.Log("??");
            }
        }
    }

    IEnumerator coroutine;

    public float currentTime = 0;
    private const float WAIT_TIME = 10.0f;
    IDisposable observer_1, observer_2;

    void OnTimerToExit() {
        observer_1 = Observable
            .EveryUpdate()
            .Select(_ => currentTime += Time.deltaTime)
            .SkipWhile(x => x < WAIT_TIME)
            .First()
            .Subscribe(_ => OnTimeOut());
        observer_2 = Observable
            .EveryUpdate()
            .Where(_ => Input.GetMouseButton(0) == true)
            .Subscribe(_ => { currentTime = 0; });
    }

    IEnumerator ProgressLeagueBar(int from, int to, bool isAscend, AccountManager.LeagueInfo leagueInfo) {
        Transform secondWindow = transform.Find("SecondWindow");
        Transform playerMMR = secondWindow.Find("PlayerMmr");

        Slider slider = playerMMR.Find("ExpSlider/Slider").GetComponent<Slider>();
        float currentVal = scriptable_leagueData.prevLeagueInfo.ratingPoint;
        var expValue = playerMMR.Find("ExpSlider/ExpValue").GetComponent<TMPro.TextMeshProUGUI>();

        int prevMaxMMR = 3000;
        if(AccountManager.Instance.GetTargetRankArea(scriptable_leagueData.prevLeagueInfo.rankDetail.minorRankName) != null) {
            prevMaxMMR = AccountManager.Instance.GetTargetRankArea(scriptable_leagueData.prevLeagueInfo.rankDetail.minorRankName).Max;
        }
        int sliderMaxVal = prevMaxMMR;
        slider.value = currentVal / sliderMaxVal;

        //테스트 코드
        //from = 320;
        //to = 0;
        //isAscend = false;

        float val = 0;
        var result = SliceSliderData(from, to, isAscend, leagueInfo);
        foreach(SliderData data in result) {
            float offset = 1.0f;

            Logger.Log("from : " + data.from);
            Logger.Log("to : " + data.to);

            if (isAscend) {
                val = 0;
                while (val < data.to) {
                    val += offset;
                    slider.value = val / data.maxVal;
                    expValue.text = val + "/" + data.maxVal;
                    yield return new WaitForSeconds(0.01f);
                }
            }
            else {
                //마이너스 이동
                if (data.from == 0 && data.to < 0) {
                    slider.value = 0;
                    int _to = -1 * data.to;
                    float _val = data.minVal;

                    Logger.Log("마이너스 이동");
                    Logger.Log("_to : " + _to);
                    Logger.Log("_val : " + _val);

                    while (_val > _to) {
                        _val -= offset;
                        Logger.Log("_val : " + _val);
                        slider.value = _val / data.maxVal;
                        expValue.text = _val + "/" + data.maxVal;
                        yield return new WaitForSeconds(0.01f);
                    }
                }
                //일반적인 감소
                else {
                    val = data.from;

                    Logger.Log("마이너스 이동");
                    Logger.Log("to : " + to);
                    Logger.Log("val : " + val);

                    while (val > data.to) {
                        val -= offset;
                        slider.value = val / data.maxVal;
                        expValue.text = val + "/" + data.from;
                        yield return new WaitForSeconds(0.01f);
                    }
                }
            }
        }
        yield return 0;
    }

    void OnTimeOut() {
        if(battleType == "solo") {
            OnReturnBtn();
        }
        else {
            GameObject secondWindow = transform.Find("SecondWindow").gameObject;
            if (!secondWindow.activeSelf) {
                OpenSecondWindow();

                currentTime = 0;
                OnTimerToExit();
            }
            else {
                OnReturnBtn();
            }
        }
    }

    IEnumerator GetUserExp(Slider slider) {
        float gain = getExp;
        TMPro.TextMeshProUGUI expValueText = transform.Find("SecondWindow/PlayerExp/ExpSlider/ExpValue").GetComponent<TMPro.TextMeshProUGUI>();
        TMPro.TextMeshProUGUI lvUpValueText = transform.Find("SecondWindow/PlayerExp/ExpSlider/ExpMaxValue").GetComponent<TMPro.TextMeshProUGUI>();

        while (gain > 0) {
            exp += 1;
            gain -= 1;
            slider.value = exp / lvExp;

            expValueText.text = ((int)exp).ToString();
            lvUpValueText.text = " / " + ((int)lvExp).ToString();
            if (exp == (int)lvExp) {
                lv++;
                lvExp = nextLvExp;
                SkeletonGraphic effect = transform.Find("SecondWindow/PlayerExp/LvUpEffect").GetComponent<SkeletonGraphic>();
                transform.Find("SecondWindow/PlayerExp/LevelIcon/Value").GetComponent<Text>().text = lv.ToString();
                effect.gameObject.SetActive(true);
                effect.Initialize(true);
                effect.Update(0);
                effect.AnimationState.SetAnimation(0, "animation", false);
                slider.value = 0;

                exp = 0;
                expValueText.text = ((int)exp).ToString();
                lvUpValueText.text = " / " + ((int)lvExp).ToString();
            }
            yield return new WaitForSeconds(0.01f);
        }
    }

    IEnumerator GetUserSupply(Slider slider, int getSup, int addSup, int winSup = 0, bool isAdditional = false) {
        TMPro.TextMeshProUGUI value = transform.Find("SecondWindow/PlayerSupply/ExpSlider/SupValue").GetComponent<TMPro.TextMeshProUGUI>();
        SkeletonGraphic boxSpine = transform.Find("SecondWindow/PlayerSupply/BoxSpine").GetComponent<SkeletonGraphic>();
        TMPro.TextMeshProUGUI basicVal = transform.Find("SecondWindow/PlayerSupply/ExtraSupply/Basic/Value").GetComponent<TMPro.TextMeshProUGUI>();
        TMPro.TextMeshProUGUI winVal = transform.Find("SecondWindow/PlayerSupply/ExtraSupply/Win/Value").GetComponent<TMPro.TextMeshProUGUI>();
        TMPro.TextMeshProUGUI totalVal = transform.Find("SecondWindow/PlayerSupply/SupplyText/Value").GetComponent<TMPro.TextMeshProUGUI>();
        boxSpine.Initialize(true);
        boxSpine.Update(0);
        boxSpine.AnimationState.SetAnimation(0, "02.vibration1", true);
        int start = getSup;
        int total = 0;
        if (isAdditional) {
            int.TryParse(totalVal.text, out total);
        }

        while (getSup > 0) {
            supply++;
            getSup--;
            basicVal.text = (start - getSup).ToString();
            totalVal.text = (++total).ToString();

            slider.value = supply / 100.0f;

            value.text = supply.ToString();
            if(supply == 100) {
                boxSpine.AnimationState.SetAnimation(0, "03.vibration2", false);
                slider.transform.localScale = new Vector3(1.1f, 1.1f, 1.1f);
                yield return new WaitForSeconds(0.2f);
                slider.transform.localScale = Vector3.one;
                yield return new WaitForSeconds(0.2f);
                slider.transform.localScale = new Vector3(1.1f, 1.1f, 1.1f);
                yield return new WaitForSeconds(0.2f);
                slider.transform.localScale = Vector3.one;
                yield return new WaitForSeconds(0.3f);

                slider.value = 0;

                supply = 0;
                value.text = supply.ToString();
                boxSpine.AnimationState.SetAnimation(0, "02.vibration1", true);
            }
            yield return new WaitForSeconds(0.01f);
        }
        start = addSup;
        if (addSup > 0) {
            yield return new WaitForSeconds(0.5f);
            while (addSup > 0) {
                supply++;
                addSup--;
                totalVal.text = (++total).ToString();
                slider.value = supply / 100.0f;

                value.text = supply.ToString();
                if (supply == 100) {
                    boxSpine.AnimationState.SetAnimation(0, "03.vibration2", false);
                    slider.transform.localScale = new Vector3(1.1f, 1.1f, 1.1f);
                    yield return new WaitForSeconds(0.2f);
                    slider.transform.localScale = Vector3.one;
                    yield return new WaitForSeconds(0.2f);
                    slider.transform.localScale = new Vector3(1.1f, 1.1f, 1.1f);
                    yield return new WaitForSeconds(0.2f);
                    slider.transform.localScale = Vector3.one;
                    yield return new WaitForSeconds(0.3f);
                    slider.value = 0;

                    supply = 0;
                    value.text = supply.ToString();
                    boxSpine.AnimationState.SetAnimation(0, "02.vibration1", true);
                }
                yield return new WaitForSeconds(0.01f);
            }
        }
        boxSpine.AnimationState.SetAnimation(0, "01.vibration0", true);
        EndRewardLoad.Invoke();
    }

    //쿠폰 사용 버튼 클릭
    public void OnDoubleCouponButton() {
        Modal.instantiate("보상을 추가로 획득 가능합니다.\n보급품 2배 획득 쿠폰을 사용하시겠습니까?", Modal.Type.YESNO, () => { ClaimDoubleReq(); });
    }

    private void ClaimDoubleReq() {
        Logger.Log("ClaimDoubleReq");
        PlayMangement.instance.SocketHandler.SendMethod("claim_2x_reward");
    }

    /// <summary>
    /// AI 대전에서는 결과화면에서 보상화면이 나오지 않게 바꿔야함
    /// </summary>
    public void ChangeResultButtonFunction() {
        Logger.Log("AI 대전의 결과창 화면 세팅 변경");
        Button btn = transform.Find("FirstWindow/GotoRewardButton").GetComponent<Button>();
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(() => {
            PlayMangement.instance.SocketHandler.SendMethod("end_game");
        });
        //btn.transform.GetChild(0).GetComponent<TMPro.TextMeshProUGUI>().text = "메인으로";
    }

    private string GetTierAnimName(string keyword) {
        switch (keyword) {
            case "무명 병사":
                return "t5";
            case "오합지졸 우두머리":
                return "t4-3";
            case "소규모 무력집단":
                return "t4-2";
            case "지역 지도자":
                return "t4-1";
            default:
                return "t5";
        }
    }

    public List<int> standards = new List<int>() { 0, 150, 300, 450, 600, 800, 1000, 1200, 1400, 1700, 2000, 2300, 2600 };
    
    public List<SliderData> SliceSliderData(int from, int to, bool isAscend, AccountManager.LeagueInfo leagueInfo) {
        List<SliderData> datas = new List<SliderData>();

        //테스트 코드
        //isAscend = false;
        //from = 570;
        //to = 550;
        //leagueInfo.ratingPoint = 550;
        //leagueInfo.rankDetail.minorRankName = "자경대 대장";

        if (isAscend) {
            List<int> query1 = standards.FindAll(x => x > from && x < to);      //from과 to 사이에 존재하는 기준값
            query1.Sort();
            for (int i=0; i<query1.Count; i++) {
                int _from = 0;
                int _to = 0;
                if (i == 0) {
                    _from = from;
                    _to = query1[i] - 1;
                }
                else {
                    _from = query1[i - 1];
                    _to = query1[i];
                }
                datas.Add(new SliderData(_from, _to, query1[i + 1], 0));
            }
            int index = standards.IndexOf(query1[query1.Count - 1]);
            datas.Add(new SliderData(query1[query1.Count - 1], to, standards[index + 1], 0));
        }
        else {
            List<int> query1 = standards.FindAll(x => x > to && x < from);      //from과 to 사이에 존재하는 기준값
            query1.Reverse();
            for (int i = 0; i < query1.Count; i++) {
                int _from = 0;
                int _to = 0;
                if (i == 0) {
                    _from = from;
                    _to = query1[i];
                }
                else {
                    _from = query1[i - 1] - 1;
                    _to = query1[i];
                }
                datas.Add(new SliderData(_from, 0, _from, 0));
            }
            var rankArea = AccountManager.Instance.GetTargetRankArea(leagueInfo.rankDetail.minorRankName);
            if (rankArea == null) rankArea = new AccountManager.Area(3000, 3000);

            //최솟값보다 떨어지는 경우
            if(from < rankArea.Min) {
                datas.Add(new SliderData(0, -from, rankArea.Max, rankArea.Min));
            }
            else {
                datas.Add(new SliderData(from, to, rankArea.Max, rankArea.Min));
            }
        }
        return datas;
    }

    public class SliderData {
        public int from;
        public int to;
        public int maxVal;
        public int minVal;
        public SliderData(int from, int to, int maxVal, int minVal) {
            this.from = from;
            this.to = to;
            this.maxVal = maxVal;
            this.minVal = minVal;   //Minus 이동에서만 사용함
        }
    }
}
