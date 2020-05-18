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
using Object = System.Object;

public class GameResultManager : MonoBehaviour {
    [SerializeField] Transform BgCanvas;
    [SerializeField] GameObject tierChangeEffectModal;
    [SerializeField] Sprite winningStreak, losingStreak;

    private float lv;
    private float exp;
    private float lvExp;
    private float nextLvExp;
    private int getExp = 0;
    private int crystal = 0;
    private int supply = 0;
    private int getSupply = 0;
    private int additionalSupply = 0;
    private int winSupply = 0;
    private int supplyBox = 0;
    private int maxDeckNum = 0;
    private bool isHuman;
    private string result;


    public bool stopNextReward = false;
    private bool scenarioCleard;

    public UnityEvent EndRewardLoad = new UnityEvent();
    public LeagueData scriptable_leagueData;

    string battleType;

    public GameObject specialRewarder;
    public GameObject skipResult;

    public RewardClass[] rewards;

    private void Awake() {
        lv = AccountManager.Instance.userResource.lv;
        exp = AccountManager.Instance.userResource.exp;
        lvExp = AccountManager.Instance.userResource.lvExp;
        supply = AccountManager.Instance.userResource.supply;
        nextLvExp = AccountManager.Instance.userResource.nextLvExp;
        maxDeckNum = AccountManager.Instance.userData.maxDeckCount;
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
        PlayMangement.instance.SettingMethod(BattleConnector.SendMessageList.end_game);
        FBL_SceneManager.Instance.LoadScene(FBL_SceneManager.Scene.MAIN_SCENE);

        AccountManager.Instance.needToReturnBattleReadyScene = false;
    }

    /// <summary>
    /// 대전 준비화면으로 바로 이동 버튼
    /// </summary>
    public void OnBattleReadyBtn() {
        AccountManager.Instance.visitDeckNow = 1;
        PlayMangement.instance.SettingMethod(BattleConnector.SendMessageList.end_game);
        FBL_SceneManager.Instance.LoadScene(FBL_SceneManager.Scene.MAIN_SCENE);

        AccountManager.Instance.needToReturnBattleReadyScene = true;
    }

    /// <summary>
    /// 매칭 화면으로 바로 이동 버튼
    /// </summary>
    public void OnNewGameBtn() {
        PlayerPrefs.SetString("SelectedBattleType", "league");
        FBL_SceneManager.Instance.LoadScene(FBL_SceneManager.Scene.CONNECT_MATCHING_SCENE);
    }

    public void OnMoveSceneBtn() {
        FBL_SceneManager.Instance.LoadScene(FBL_SceneManager.Scene.MAIN_SCENE);
    }

    SocketFormat.ResultFormat resultData;

    public void SetResultWindow(string result, bool isHuman, SocketFormat.ResultFormat resultData) {
        this.isHuman = isHuman;
        this.result = result;
        this.resultData = resultData;

        if (result == "win")
            RequestReward();


        if (ScenarioGameManagment.scenarioInstance != null) ScenarioGameManagment.scenarioInstance.challengeUI.SetActive(false);
        PlayerPrefs.DeleteKey("ReconnectData");
        gameObject.SetActive(true);
        transform.Find("FirstWindow").gameObject.SetActive(true);
        BgCanvas.gameObject.SetActive(true);
        GameObject heroSpine = transform.Find("FirstWindow/HeroSpine/" + PlayMangement.instance.player.heroID).gameObject;
        heroSpine.SetActive(true);
        iTween.ScaleTo(heroSpine, iTween.Hash("scale", Vector3.one, "islocal", true, "time", 0.3f));
        getExp = resultData.reward.userExp;
        getSupply = resultData.reward.supply;
        additionalSupply = resultData.reward.x2supply;
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

                    var chapterData = PlayMangement.chapterData;
                    if (chapterData != null) {
                        string camp = isHuman ? "human" : "orc";
                        GetComponent<ChapterAlertHandlerIngame>()
                            .RequestChangeChapterAlert(camp, chapterData.chapter, chapterData.stage_number);
                    }
                    break;
                }
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

        //if (maxDeckNum != AccountManager.Instance.userData.maxDeckCount) {
        //    NewAlertManager.Instance.SimpleInitialize();
        //    NewAlertManager.Instance.SetUpButtonToAlert(gameObject, NewAlertManager.ButtonName.DECK_NUMBERS);
        //}
        StartCoroutine(SetRewards(result));
    }

    private bool isFirstWinningTalkRevealed = true;
    
    public IEnumerator SetRewards(string result = "") {
        Transform rewards = transform.Find("SecondWindow/ResourceRewards");
        Transform playerSup = transform.Find("SecondWindow/PlayerSupply");
        playerSup.Find("ExpSlider/Slider").GetComponent<Slider>().value = supply / 100.0f;
        playerSup.Find("ExpSlider/SupValue").GetComponent<TMPro.TextMeshProUGUI>().text = supply.ToString();
        yield return new WaitForSeconds(0.1f);
        Slider expSlider = transform.Find("SecondWindow/PlayerExp/ExpSlider/Slider").GetComponent<Slider>();
        expSlider.value = exp / lvExp;
        TMPro.TextMeshProUGUI doubleCoupons = transform.Find("SecondWindow/PlayerSupply/ExtraSupply/DoubleButton/Value").GetComponent<TMPro.TextMeshProUGUI>();

        if (PlayMangement.instance.socketHandler.result.reward.x2supply > 0) 
            doubleCoupons.text = (AccountManager.Instance.userData.supplyX2Coupon + 1).ToString();
        

        Transform playerExp = transform.Find("SecondWindow/PlayerExp");
        playerExp.Find("LevelIcon/Value").GetComponent<Text>().text = lv.ToString();
        playerExp.Find("UserName").GetComponent<TMPro.TextMeshProUGUI>().text = AccountManager.Instance.userData.nickName;
        playerExp.Find("ExpSlider/ExpValue").GetComponent<TMPro.TextMeshProUGUI>().text = ((int)exp).ToString();
        playerExp.Find("ExpSlider/ExpMaxValue").GetComponent<TMPro.TextMeshProUGUI>().text = "/" + ((int)lvExp).ToString() + " " + "(" + "+" + getExp.ToString() + ")";
        iTween.ScaleTo(playerExp.gameObject, iTween.Hash("scale", Vector3.one, "islocal", true, "time", 0.5f));
        yield return new WaitForSeconds(0.1f);
        iTween.ScaleTo(transform.Find("SecondWindow/PlayerMmr").gameObject, iTween.Hash("scale", Vector3.one, "islocal", true, "time", 0.5f));
        yield return new WaitForSeconds(0.1f);
        
        iTween.ScaleTo(playerSup.gameObject, iTween.Hash("scale", Vector3.one, "islocal", true, "time", 0.5f));
        //iTween.ScaleTo(transform.Find("SecondWindow/Buttons").gameObject, iTween.Hash("scale", Vector3.one, "islocal", true, "time", 0.5f));
        
        yield return new WaitForSeconds(0.1f);        
        if (getExp > 0) 
            yield return GetUserExp(expSlider);
        yield return SetLevelUP();
        yield return new WaitUntil(() => stopNextReward == false);

        string battleType = PlayerPrefs.GetString("SelectedBattleType");
        if (battleType == "league" || battleType == "leagueTest") {
            yield return SetLeagueData(result);
            yield return StartThreeWinEffect();
            FirstWinningTalking();
        }

        if (getSupply > 0 || winSupply > 0)            
            yield return GetUserSupply(playerSup.Find("ExpSlider/Slider").GetComponent<Slider>(), getSupply, additionalSupply, winSupply > 0 ? resultData.leagueWinReward[0].amount : 0);

        yield return StartShowReward(result);
        skipResult?.SetActive(false);
    }

    public void RequestReward() {
        if (PlayMangement.instance.rewarder == null) return;
        PlayMangement.instance.rewarder.SetRewardBox();
    }

    protected IEnumerator StartShowReward(string result) {
        if(result != "win") { ActivateMenuButton(); yield break; };

        if (rewards == null && PlayMangement.chapterData != null  && result == "win") {
            string playerCamp = (PlayMangement.instance.player.isHuman == true) ? "human" : "orc";
            scenarioCleard = (PlayMangement.chapterData == null) ? true : AccountManager.Instance.clearedStages.Exists(x => x.camp == playerCamp && x.chapterNumber.Value == PlayMangement.chapterData.chapter && x.stageNumber == PlayMangement.chapterData.stage_number);

            rewards = new RewardClass[PlayMangement.chapterData.scenarioReward.Length];
            for (int i = 0; i < rewards.Length; i++) {
                rewards[i] = new RewardClass();
                rewards[i].type = "item";
                rewards[i].item = PlayMangement.chapterData.scenarioReward[i].reward;
                rewards[i].amount = PlayMangement.chapterData.scenarioReward[i].count;
            }            
        }

        if (scenarioCleard == true || PlayMangement.instance.rewarder == null || rewards.Length == 0) { ActivateMenuButton(); yield break; };
        yield return ShowItemReward(rewards);
        ShowingRewarder(rewards);
        yield return new WaitForSeconds(2.0f);        
    }
    
    private void ShowingRewarder(RewardClass[] rewards) {
        int scenarioNum = PlayMangement.chapterData.stageSerial;        
        if (scenarioNum == 3 || scenarioNum == 4) {
            specialRewarder.SetActive(true);

            SkeletonGraphic boxAnimation = specialRewarder.transform.Find("Box").gameObject.GetComponent<SkeletonGraphic>();

            boxAnimation.Initialize(true);
            boxAnimation.Update(0);

            boxAnimation.AnimationState.SetAnimation(0, "01.START", false);
            boxAnimation.AnimationState.AddAnimation(0, "02.IDLE", true, 0f);

            Button btn = specialRewarder.GetComponent<Button>();

            btn.onClick.AddListener(() => {
                PlayMangement.instance.rewarder.BoxSetFinish();
                specialRewarder.SetActive(false);
                Invoke("ActivateMenuButton", 2.0f);
            });
        }
        else {
            ActivateMenuButton();
        }
    }

    private void ActivateMenuButton() {
        iTween.ScaleTo(transform.Find("SecondWindow/Buttons").gameObject, iTween.Hash("scale", Vector3.one, "islocal", true, "time", 0.5f));
    }



    /// <summary>
    /// 리그 관련 UI 처리
    /// </summary>
    /// <param name="result"></param>
    /// <returns></returns>
    public IEnumerator SetLeagueData(string result) {
        var leagueInfo = scriptable_leagueData.leagueInfo;
        if (leagueInfo != null) {
            Transform secondWindow = transform.Find("SecondWindow");
            Transform playerMMR = secondWindow.Find("PlayerMmr");
            Transform mmrSlider = playerMMR.Find("MMRSlider");
            
            Image rankIcon = playerMMR.Find("RankIcon").GetComponent<Image>();
            rankIcon.gameObject.SetActive(true);
            
            Image streakFlag = playerMMR.Find("StreakFlag").gameObject.GetComponent<Image>();
            var icons = AccountManager.Instance.resource.rankIcons;
            if (icons.ContainsKey(scriptable_leagueData.prevLeagueInfo.rankDetail.id.ToString())) {
                rankIcon.sprite = icons[scriptable_leagueData.prevLeagueInfo.rankDetail.id.ToString()];
            }
            else {
                rankIcon.sprite = icons["default"];
            }
            playerMMR.transform.Find("Name").GetComponent<TMPro.TextMeshProUGUI>().text = scriptable_leagueData.prevLeagueInfo.rankDetail.minorRankName;

            var description = playerMMR.Find("VictoryInfo").GetComponent<TMPro.TextMeshProUGUI>();
            string streak;
         


            StringBuilder sb = new StringBuilder();
            if(leagueInfo.winningStreak > 1) {
                streak = PlayMangement.instance.uiLocalizeData["ui_ingame_result_winstreak"];
                streak = streak.Replace("{n}", "<color=yellow>" + leagueInfo.winningStreak + "</color>");
                sb
                    .Append(streak);
                streakFlag.sprite = winningStreak;
            }
            else if(leagueInfo.losingStreak > 1) {
                streak = PlayMangement.instance.uiLocalizeData["ui_ingame_result_losestreak"];
                streak = streak.Replace("{n}", "<color=red>" + leagueInfo.losingStreak + "</color>");
                sb
                    .Append(streak);
                streakFlag.sprite = losingStreak;
            }
            description.text = sb.ToString();

            //MMR 증가
            if (scriptable_leagueData.leagueInfo.ratingPoint > scriptable_leagueData.prevLeagueInfo.ratingPoint) {
                coroutine = ProgressLeagueBar(result == "win");
            }
            //MMR 감소
            else {
                coroutine = ProgressLeagueBar(result == "win");
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

            if (icons.ContainsKey(leagueInfo.rankDetail.id.ToString())) {
                rankIcon.sprite = icons[leagueInfo.rankDetail.id.ToString()];
                playerMMR.transform.Find("Name").GetComponent<TMPro.TextMeshProUGUI>().text = leagueInfo.rankDetail.minorRankName;
            }
            else {
                rankIcon.sprite = icons["default"];
            }


        }
        yield return 0;
    }




    /// <summary>
    /// 승급전 혹은 강등전 발생 이벤트 처리
    /// </summary>
    /// <returns></returns>
    IEnumerator ShowRankChangeChanceUI() {
        Logger.Log("승급전/강등전 발생 이벤트 UI 처리");

        Transform secondWindow = transform.Find("SecondWindow");
        Transform playerMmr = secondWindow.Find("PlayerMmr");
        Transform expSlider = playerMmr.Find("MMRSlider");


        expSlider.Find("RankChangeEffect").gameObject.SetActive(true);

        var leagueInfo = scriptable_leagueData.leagueInfo;
        string upDown;
        if (leagueInfo.rankingBattleState == "rank_up") {
            Logger.Log("Case 1");

            upDown = PlayMangement.instance.uiLocalizeData["ui_ingame_result_promotechance"];
            expSlider.Find("RankChangeEffect/Text").GetComponent<TMPro.TextMeshProUGUI>().text = upDown;
        }
        else if(leagueInfo.rankingBattleState == "rank_down") {
            Logger.Log("Case 2");
            upDown = PlayMangement.instance.uiLocalizeData["ui_ingame_result_demotewarning"];
            expSlider.Find("RankChangeEffect/Text").GetComponent<TMPro.TextMeshProUGUI>().text = upDown;
        }
        else {
            Logger.Log("Unknown Case");
            expSlider.Find("RankChangeEffect").gameObject.SetActive(false);
        }
        yield return 0;
    }

    /// <summary>
    /// 승급전 결과판 처리 (예 : 3판2선승제 => 하나씩 결과 보여주기)
    /// </summary>
    IEnumerator ShowLeagueEffect() {
        Logger.Log("승급전 진행중 관련 UI 처리");

        Transform secondWindow = transform.Find("SecondWindow");
        Transform playerMmr = secondWindow.Find("PlayerMmr");
        Transform expSlider = playerMmr.Find("MMRSlider");
        Transform rankBoard = playerMmr.Find("RankBoard");

        expSlider.gameObject.SetActive(false);
        rankBoard.gameObject.SetActive(true);
        Transform slotParent = rankBoard.Find("Bottom");

        var leagueInfo = scriptable_leagueData.leagueInfo;
        string upDown;
        int slotCnt = 0;
        if(leagueInfo.rankingBattleState == "rank_up") {
            Logger.Log("Case 1");
            upDown = PlayMangement.instance.uiLocalizeData["ui_ingame_result_promotematch"];
            slotCnt = leagueInfo.rankDetail.rankUpBattleCount.battles;
            rankBoard.Find("Top/Text").GetComponent<TMPro.TextMeshProUGUI>().text = "upDown";
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
    /// 승급/강등 이펙트 처리
    /// </summary>
    IEnumerator ShowTierChangeEffect(bool isRankUp) {
        yield return null;

        tierChangeEffectModal.SetActive(true);
        var leagueInfo = scriptable_leagueData.leagueInfo;
        var prevLeagueInfo = scriptable_leagueData.prevLeagueInfo;

        string animName = GetTierAnimName(leagueInfo.rankDetail.id.ToString());
        SkeletonGraphic skeletonGraphic = tierChangeEffectModal.transform.Find("Spine").GetComponent<SkeletonGraphic>();
        skeletonGraphic.Initialize(true);

        try {
            if (animName != null) skeletonGraphic.Skeleton.SetSkin(animName);
            skeletonGraphic.Skeleton.SetSlotsToSetupPose();
        }
        catch (Exception ex) {
            Logger.Log("Skin Not Found");
        }

        var message = tierChangeEffectModal.transform.Find("Message").GetComponent<TMPro.TextMeshProUGUI>();

        //TODO : 승급인지 강등인지 구분 필요
        if (isRankUp) {
            message.text = leagueInfo.rankDetail.minorRankName + "으로 승급하셨습니다.";
            skeletonGraphic.AnimationState.SetAnimation(0, "UP", false);
            Logger.Log("승급!");
        }
        else {
            message.text = leagueInfo.rankDetail.minorRankName + "으로 강등되었습니다.";
            skeletonGraphic.AnimationState.SetAnimation(0, "DOWN", false);
            Logger.Log("강등!");
        }
    }


    void FastShowTierChangeEffect(bool isRankUp) {
        tierChangeEffectModal.SetActive(true);
        var leagueInfo = scriptable_leagueData.leagueInfo;
        var prevLeagueInfo = scriptable_leagueData.prevLeagueInfo;

        string animName = GetTierAnimName(leagueInfo.rankDetail.id.ToString());
        SkeletonGraphic skeletonGraphic = tierChangeEffectModal.transform.Find("Spine").GetComponent<SkeletonGraphic>();
        skeletonGraphic.Initialize(true);

        try {
            if (animName != null) skeletonGraphic.Skeleton.SetSkin(animName);
            skeletonGraphic.Skeleton.SetSlotsToSetupPose();
        }
        catch (Exception ex) {
            Logger.Log("Skin Not Found");
        }

        var message = tierChangeEffectModal.transform.Find("Message").GetComponent<TMPro.TextMeshProUGUI>();

        //TODO : 승급인지 강등인지 구분 필요
        if (isRankUp) {
            message.text = leagueInfo.rankDetail.minorRankName + "으로 승급하셨습니다.";
            skeletonGraphic.AnimationState.SetAnimation(0, "UP", false);
            Logger.Log("승급!");
        }
        else {
            message.text = leagueInfo.rankDetail.minorRankName + "으로 강등되었습니다.";
            skeletonGraphic.AnimationState.SetAnimation(0, "DOWN", false);
            Logger.Log("강등!");
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

    IEnumerator LeagueAnimation(int amount) {
        yield return new WaitForSeconds(1.2f);
        Transform playermmr = transform.Find("SecondWindow/PlayerMmr");
        RectTransform animationObject = playermmr.Find("Result_League").gameObject.GetComponent<RectTransform>();
        SkeletonGraphic leagueAnimation = animationObject.gameObject.GetComponent<SkeletonGraphic>();
        UnityEngine.Animation iconAnimation = playermmr.Find("RankIcon").gameObject.GetComponent<UnityEngine.Animation>();
        float second = 0;
        TrackEntry entry;
        string ani = "";

        if (amount < 0) {
            ani = "down";
            
        }
        else if (amount == 0) {
            entry = null;
            ani = "";
            second = 0;
        }
        else if (amount > 0 && amount < 6)
            ani = "up1";
        else if (amount > 5 && amount < 16)
            ani = "up2";
        else
            ani = "up3";
        
        if(string.IsNullOrEmpty(ani) == false) {
            entry = leagueAnimation.AnimationState.SetAnimation(0, ani, false);
            second = leagueAnimation.SkeletonData.FindAnimation(ani).Duration;
            iconAnimation.Play();
        }
        yield return new WaitForSeconds(second);
    }


    IEnumerator ProgressLeagueBar(bool isWin) {
        Transform secondWindow = transform.Find("SecondWindow");
        Transform playerMMR = secondWindow.Find("PlayerMmr");
        Slider slider = playerMMR.Find("MMRSlider/Slider").GetComponent<Slider>();
        TMPro.TextMeshProUGUI label = playerMMR.Find("MMRSlider/Label").GetComponent<TMPro.TextMeshProUGUI>();

        var prevLeagueInfo = scriptable_leagueData.prevLeagueInfo;
        var newLeagueInfo = scriptable_leagueData.leagueInfo;
        int prevMMR = prevLeagueInfo.ratingPoint;
        int newMMR = newLeagueInfo.ratingPoint;
        int amount = newMMR - prevMMR;

        slider.maxValue = prevLeagueInfo.rankDetail.pointLessThen;
        slider.value = prevMMR;
        label.text = prevMMR + "/" + prevLeagueInfo.rankDetail.pointLessThen + " " + "(" + amount.ToString() + ")";

        if (amount != 0)
            yield return LeagueAnimation(amount);


        if (prevLeagueInfo.rankDetail.id != newLeagueInfo.rankDetail.id) {
            Logger.Log("등급 변동");
            //1. 승급 혹은 강등전 결과 보여주기 UI
            //2. 승급, 강등 이펙트
            Transform victoryInfo = transform.Find("SecondWindow/PlayerMmr/VictoryInfo");
            victoryInfo.gameObject.SetActive(false);
            yield return ShowBattleTableUI(true, isWin);

            if (prevMMR < newMMR) {
                Logger.Log("승급");
                int from = prevMMR;
                int to = prevLeagueInfo.rankDetail.pointLessThen;
                
                int value = from;
                while (value < to) {
                    yield return new WaitForSeconds(0.1f);
                    slider.value = value;
                    label.text = value + "/" + (prevLeagueInfo.rankDetail.pointLessThen - 1) + " " + "(" + amount.ToString() + ")";
                    value++;

                    Logger.Log("승급 기회");
                }

                int value2 = 0;
                to = newLeagueInfo.ratingPoint - newLeagueInfo.rankDetail.pointLessThen;
                slider.maxValue = newLeagueInfo.rankDetail.pointLessThen;
                slider.value = 0;

                while (value2 < to) {
                    yield return new WaitForSeconds(0.1f);
                    slider.value = value2;
                    label.text = (value2 + newLeagueInfo.rankDetail.pointLessThen) + "/" + (newLeagueInfo.rankDetail.pointLessThen - 1) + " " + "(" + amount.ToString() + ")";
                    value2++;
                }

                yield return ShowTierChangeEffect(true);
            }
            else {
                Logger.Log("강등");

                int from = prevMMR;
                int to = prevLeagueInfo.rankDetail.pointOverThen;

                int value = from;
                while (value > to) {
                    yield return new WaitForSeconds(0.1f);
                    slider.value = value;
                    label.text = value + "/" + (prevLeagueInfo.rankDetail.pointLessThen - 1) + " " + "(" + amount.ToString() + ")";
                    value--;
                }



                int value2 = newLeagueInfo.rankDetail.pointLessThen;

                from = value2;
                to = newLeagueInfo.ratingPoint;
                slider.maxValue = newLeagueInfo.rankDetail.pointLessThen;
                slider.value = value2;

                while (value2 >= to) {
                    yield return new WaitForSeconds(0.1f);
                    slider.value = value2;
                    label.text = value2 + "/" + (newLeagueInfo.rankDetail.pointLessThen - 1) + " " + "(" + amount.ToString() + ")";
                    value2--;
                }

                yield return ShowTierChangeEffect(false);
            }
        }
        //승급이나 강등 없음
        else {
            //yield return ShowBattleTableUI(true, isWin);

            if (newMMR > prevLeagueInfo.rankDetail.pointLessThen || newMMR < prevLeagueInfo.rankDetail.pointOverThen) {
                //게이지 최저치보다 낮은 경우
                if (newMMR < prevLeagueInfo.rankDetail.pointOverThen) {
                    slider.value = 0;
                }
                //게이지 최대치보다 높은 경우
                if(newMMR > prevLeagueInfo.rankDetail.pointLessThen) {
                    slider.value = slider.maxValue;
                }

                int from = prevMMR;
                int to = newMMR;

                if(from < to) {
                    while(from <= to) {
                        yield return new WaitForSeconds(0.1f);
                        label.text = from + "/" + (newLeagueInfo.rankDetail.pointLessThen - 1) + " " + "(" + amount.ToString() + ")";
                        from++;
                    }
                }
                else {
                    while(from >= to) {
                        yield return new WaitForSeconds(0.1f);
                        label.text = from + "/" + (newLeagueInfo.rankDetail.pointLessThen - 1) + " " + "(" + amount.ToString() + ")";
                        from--;
                    }
                }
            }
            else {
                if (prevMMR < newMMR) {
                    Logger.Log("MMR 증가");

                    int from = prevMMR;
                    int to = newMMR;

                    int value = from;
                    if (newMMR > prevLeagueInfo.rankDetail.pointLessThen) {
                        slider.value = prevLeagueInfo.rankDetail.pointLessThen;
                        label.text = newMMR + "/" + (newLeagueInfo.rankDetail.pointLessThen - 1) + " " + "(" + amount.ToString() + ")";
                    }
                    else {
                        slider.value = from;

                        while (value <= to) {
                            yield return new WaitForSeconds(0.1f);
                            slider.value = value;
                            label.text = value + "/" + (newLeagueInfo.rankDetail.pointLessThen - 1) + " " + "(" + amount.ToString() + ")";
                            value++;
                        }
                    }
                }
                else {
                    Logger.Log("MMR 감소");

                    int from = prevMMR;
                    int to = newMMR;

                    int value = from;
                    if (newMMR < prevLeagueInfo.rankDetail.pointOverThen) {
                        slider.value = 0;
                        label.text = newMMR + "/" + (newLeagueInfo.rankDetail.pointLessThen - 1) + " " + "(" + amount.ToString() + ")";

                        Logger.Log("강등 위기");
                    }

                    else {
                        while (value >= to) {
                            yield return new WaitForSeconds(0.1f);
                            slider.value = value;
                            label.text = value + "/" + newLeagueInfo.rankDetail.pointLessThen + " " + "(" + amount.ToString() + ")";
                            value--;
                        }
                    }
                }
            }
        }

        if (newMMR <= 0) 
            slider.gameObject.transform.Find("Fill Area/Fill/Effect").gameObject.SetActive(false);
        else
            slider.gameObject.transform.Find("Fill Area/Fill/Effect").gameObject.SetActive(true);

    }

    public IEnumerator ShowItemReward(RewardClass[] rewards) {
        if (rewards == null || rewards.Length == 0) yield break;

        gameObject.transform.Find("SecondWindow/GainReward").gameObject.SetActive(true);
        
        Transform rewardParent = gameObject.transform.Find("SecondWindow/GainReward/ResourceRewards");

        int scenarioNum = PlayMangement.chapterData.stageSerial;
        if (scenarioNum == 3 || scenarioNum == 4) {
            Transform slot = rewardParent.GetChild(0);
            ShowBox();
            yield return new WaitForSeconds(0.2f);
            slot.Find("Effects").gameObject.SetActive(true);
        }
        else {
            for (int i = 0; i < rewards.Length; i++) {
                if (rewards[i].item == "exp" || rewards[i].item == "supply") continue;

                Transform slot = rewardParent.GetChild(i);
                slot.gameObject.SetActive(true);
                Sprite Image;

                if (rewards[i].type == "card")
                    Image = AccountManager.Instance.resource.scenarioRewardIcon["cardCommon"];
                else
                    Image = AccountManager.Instance.resource.scenarioRewardIcon[rewards[i].item];

                slot.Find("Gold").gameObject.GetComponent<Image>().sprite = Image;
                slot.Find("Value").gameObject.GetComponent<TMPro.TextMeshProUGUI>().text = "x" + " " + rewards[i].amount.ToString();
                iTween.ScaleTo(slot.gameObject, iTween.Hash("x", 1f, "y", 1f, "islocal", true, "time", 0.3f));
                yield return new WaitForSeconds(0.3f);
                slot.Find("Effects").gameObject.SetActive(true);

                int cloneIndex = i;
                var btn = slot.Find("Frame").GetComponent<Button>();
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() => {
                    RewardDescriptionHandler.instance.RequestDescriptionModal(rewards[cloneIndex].item, 900);
                });
            }
        }
    }

    public void ShowBox() {
        gameObject.transform.Find("SecondWindow/GainReward").gameObject.SetActive(true);
        Transform rewardParent = gameObject.transform.Find("SecondWindow/GainReward/ResourceRewards");

        Transform slot = rewardParent.GetChild(0);
        slot.gameObject.SetActive(true);
        Sprite Image;

        Image = AccountManager.Instance.resource.scenarioRewardIcon["reinforcedBox"];


        slot.Find("Gold").gameObject.GetComponent<Image>().sprite = Image;
        slot.Find("Value").gameObject.GetComponent<TMPro.TextMeshProUGUI>().text = "x" + " " + 1.ToString();
        iTween.ScaleTo(slot.gameObject, iTween.Hash("x", 1f, "y", 1f, "islocal", true, "time", 0.2f));
        
    }

    /// <summary>
    /// 승급, 강등전 결과 테이블 UI
    /// </summary>  
    /// <returns></returns>
    IEnumerator ShowBattleTableUI(bool isRankChanged, bool isWin) {
        var prevLeagueInfo = scriptable_leagueData.prevLeagueInfo;
        var newLeagueInfo = scriptable_leagueData.leagueInfo;
        var prevRankDetail = prevLeagueInfo.rankDetail;
        var rankDetail = newLeagueInfo.rankDetail;

        Transform mmrSlider = transform.Find("SecondWindow/PlayerMmr/MMRSlider");
        Transform rankBoard = transform.Find("SecondWindow/PlayerMmr/RankBoard");
        TMPro.TextMeshProUGUI description = rankBoard.Find("Top/Text").GetComponent<TMPro.TextMeshProUGUI>();

        //단판승인가?
        bool isBattleOnce = true;
        if(prevLeagueInfo.rankingBattleCount != null && prevLeagueInfo.rankingBattleCount.Length > 0) {
            isBattleOnce = false;
        }
        
        //승급이나 강등이 되어 랭크가 변화함
        if (isRankChanged) {
            mmrSlider.gameObject.SetActive(false);
            rankBoard.gameObject.SetActive(true);
            yield return new WaitForSeconds(1.0f);

            Transform slots = rankBoard.Find("Bottom");
            if (isBattleOnce) {
                slots.GetChild(0).gameObject.SetActive(true);

                isWin = prevLeagueInfo.ratingPoint < newLeagueInfo.ratingPoint;
                if (isWin) {
                    slots.GetChild(0).Find("Win").gameObject.SetActive(true);
                    description.text = "승급!";
                }
                else {
                    slots.GetChild(0).Find("Lose").gameObject.SetActive(true);
                    description.text = "강등!";
                }
            }
            else {
                if(prevLeagueInfo.rankingBattleState == "rank_down") {
                    description.text = "강등전 진행중";
                }
                else if(prevLeagueInfo.rankingBattleState == "rank_up") {
                    description.text = "승급전 진행중";
                }

                for(int i=0; i<prevLeagueInfo.rankingBattleCount.Length; i++) {
                    slots.GetChild(i).gameObject.SetActive(true);
                    if (prevLeagueInfo.rankingBattleCount[i] == true) {
                        slots.GetChild(i).Find("Win").gameObject.SetActive(true);
                    }
                    else {
                        slots.GetChild(i).Find("Lose").gameObject.SetActive(true);
                    }
                }

                slots.GetChild(prevLeagueInfo.rankingBattleCount.Length).gameObject.SetActive(true);

                isWin = prevLeagueInfo.ratingPoint < newLeagueInfo.ratingPoint;
                if (isWin) slots.GetChild(0).Find("Win").gameObject.SetActive(true);
                else slots.GetChild(0).Find("Lose").gameObject.SetActive(true);
            }
        }
        else {
            mmrSlider.gameObject.SetActive(false);
            rankBoard.gameObject.SetActive(true);
            yield return new WaitForSeconds(1.0f);

            Transform slots = rankBoard.Find("Bottom");

            for (int i = 0; i < prevLeagueInfo.rankingBattleCount.Length; i++) {
                slots.GetChild(i).gameObject.SetActive(true);
                if (prevLeagueInfo.rankingBattleCount[i] == true) {
                    slots.GetChild(i).Find("Win").gameObject.SetActive(true);
                }
                else {
                    slots.GetChild(i).Find("Lose").gameObject.SetActive(true);
                }
            }
        }
    }

    void OnTimeOut() {
        if(battleType == "solo") {
            //OnReturnBtn();
        }
        else {
            GameObject secondWindow = transform.Find("SecondWindow").gameObject;
            if (!secondWindow.activeSelf) {
                OpenSecondWindow();

                currentTime = 0;
                OnTimerToExit();
            }
            else {
                //OnReturnBtn();
            }
        }
    }

    IEnumerator GetUserExp(Slider slider) {
        float gain = getExp;
        int showgain = (int)gain;
        TMPro.TextMeshProUGUI expValueText = transform.Find("SecondWindow/PlayerExp/ExpSlider/ExpValue").GetComponent<TMPro.TextMeshProUGUI>();
        TMPro.TextMeshProUGUI lvUpValueText = transform.Find("SecondWindow/PlayerExp/ExpSlider/ExpMaxValue").GetComponent<TMPro.TextMeshProUGUI>();
        // + " " + "(" + "+" + showgain.ToString() + ")"
        while (gain > 0) {
            exp += 1;
            gain -= 1;
            slider.value = exp / lvExp;

            expValueText.text = ((int)exp).ToString();
            lvUpValueText.text = " / " + ((int)lvExp).ToString() + " " + "(" + "+" + getExp.ToString() + ")";
            if (exp == (int)lvExp) {
                lv++;
                lvExp = nextLvExp;
                //SkeletonGraphic effect = transform.Find("SecondWindow/PlayerExp/LvUpEffect").GetComponent<SkeletonGraphic>();
                transform.Find("SecondWindow/PlayerExp/LevelIcon/Value").GetComponent<Text>().text = lv.ToString();
                //effect.gameObject.SetActive(true);
                //effect.Initialize(true);
                //effect.Update(0);
                //effect.AnimationState.SetAnimation(0, "animation", false);
                slider.value = 0;

                exp = 0;
                expValueText.text = ((int)exp).ToString();
                lvUpValueText.text = " / " + ((int)nextLvExp).ToString() + " " + "(" + "+" + getExp.ToString() + ")";
            }
            yield return new WaitForSeconds(0.01f);
        }
    }

    



    IEnumerator SetLevelUP() {
        if (PlayMangement.instance.socketHandler.result.lvUp == null) yield break;
        stopNextReward = true;
        PlayMangement.instance.levelCanvas.SetActive(true);

        SocketFormat.LevelUp levelData = PlayMangement.instance.socketHandler.result.lvUp;

        Transform levelCanvas = PlayMangement.instance.levelCanvas.transform;

        Transform levelup = levelCanvas.Find("LevelUP");
        Transform reward = levelCanvas.Find("Reward");
        Text leveltext = levelCanvas.Find("Level").gameObject.GetComponent<Text>();
        Button confirmBtn = levelCanvas.Find("ConfirmBtn").gameObject.GetComponent<Button>();
        SkeletonGraphic levelUPEffect = levelup.gameObject.GetComponent<SkeletonGraphic>();
        UnityEngine.Animation rewardAnimation = reward.gameObject.GetComponent<UnityEngine.Animation>();

        levelUPEffect.Initialize(true);
        levelUPEffect.Update(0);

        leveltext.text = levelData.lv.ToString();
        confirmBtn.onClick.AddListener(delegate () { levelCanvas.gameObject.SetActive(false); stopNextReward = false; rewardAnimation.Stop(); });


        levelup.gameObject.SetActive(true);
        TrackEntry entry;
        entry = levelUPEffect.AnimationState.AddAnimation(0, "01.start", false, 0);
        entry = levelUPEffect.AnimationState.AddAnimation(0, "02.play", true, 0);
        yield return new WaitForSeconds(levelUPEffect.AnimationState.Data.SkeletonData.FindAnimation("01.start").Duration - 0.2f);
        leveltext.gameObject.SetActive(true);       
         
        if (levelData.rewards.Length == 0)
            reward.Find("RewardLayout").gameObject.SetActive(false);
        else {
            rewardAnimation.Play();
            yield return new WaitForSeconds(0.2f);
            Transform layout = reward.Find("RewardLayout");
            layout.gameObject.SetActive(true);
            for (int i = 0; i < levelData.rewards.Length; i++) {
                Transform slot = layout.GetChild(i);
                Image slotSprite = slot.Find("rewardSprite").gameObject.GetComponent<Image>();
                TMPro.TextMeshProUGUI amoutObject = slot.Find("rewardAmount").gameObject.GetComponent<TMPro.TextMeshProUGUI>();
                slot.gameObject.SetActive(true);
                switch (levelData.rewards[i].kind) {
                    case "goldFree":
                        slotSprite.sprite = AccountManager.Instance.resource.scenarioRewardIcon["goldFree"];
                        break;
                    case "crystal":
                    case "manaCrystal":
                        slotSprite.sprite = AccountManager.Instance.resource.scenarioRewardIcon["crystal"];
                        break;
                    case "supplyBox":
                        slotSprite.sprite = AccountManager.Instance.resource.scenarioRewardIcon["supplyBox"];
                        break;
                    case "add_deck":
                        slotSprite.sprite = AccountManager.Instance.resource.scenarioRewardIcon["deck"];
                        break;
                    default:
                        slotSprite.sprite = AccountManager.Instance.resource.scenarioRewardIcon["supplyBox"];
                        break;
                }
                amoutObject.text = "x" + levelData.rewards[i].amount.ToString();
                iTween.ScaleTo(slot.gameObject, iTween.Hash("x", 1f, "y", 1f, "islocal", true, "time", 0.3f));
                yield return new WaitForSeconds(0.3f);
                slot.Find("Effects").gameObject.SetActive(true);

                int cloneIndex = i;
                Button btn = slot.Find("Frame").GetComponent<Button>();
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() => {
                    RewardDescriptionHandler.instance.RequestDescriptionModal(levelData.rewards[cloneIndex].kind, 900);
                });
            }
        }
        
        PlayMangement.instance.socketHandler.result.lvUp = null;
    }

    

    IEnumerator GetUserSupply(Slider slider, int getSup, int addSup, int winSup = 0, bool isAdditional = false) {
        TMPro.TextMeshProUGUI value = transform.Find("SecondWindow/PlayerSupply/ExpSlider/SupValue").GetComponent<TMPro.TextMeshProUGUI>();
        SkeletonGraphic boxSpine = transform.Find("SecondWindow/PlayerSupply/BoxSpine").GetComponent<SkeletonGraphic>();
        SkeletonGraphic supplySpine = transform.Find("SecondWindow/PlayerSupply/SupplySpine").gameObject.GetComponent<SkeletonGraphic>();
        TMPro.TextMeshProUGUI basicVal = transform.Find("SecondWindow/PlayerSupply/ExtraSupply/Basic/Value").GetComponent<TMPro.TextMeshProUGUI>();
        TMPro.TextMeshProUGUI winVal = transform.Find("SecondWindow/PlayerSupply/ExtraSupply/Win/Value").GetComponent<TMPro.TextMeshProUGUI>();
        TMPro.TextMeshProUGUI totalVal = transform.Find("SecondWindow/PlayerSupply/SupplyText/Value").GetComponent<TMPro.TextMeshProUGUI>();

        
        var battleType = PlayerPrefs.GetString("SelectedBattleType");
        if (battleType == "league" || battleType == "leagueTest") {
            yield return new WaitForSeconds(2f);
        }
        
        if (battleType.Contains("league")) {
            AccountManager
                .Instance
                .mainSceneEffects
                .Enqueue(
                    new MainWindowEffectManager.Effect(
                        MainWindowEffectManager.EffectType.LEAGUE_REWARD, 
                        new object[]{"league", getSupply + additionalSupply + winSup}
                    )
                );    
        }
        else if (battleType.Equals("story")) {
            AccountManager
                .Instance
                .mainSceneEffects
                .Enqueue(
                    new MainWindowEffectManager.Effect(
                        MainWindowEffectManager.EffectType.LEAGUE_REWARD, 
                        new object[]{"story", getSupply + additionalSupply + winSup}
                    )
                );
        }
        
        boxSpine.Initialize(true);
        boxSpine.Update(0);
        
        supplySpine.Initialize(true);
        supplySpine.Update(0);
        supplySpine.AnimationState.SetAnimation(0, "NOANI", false);
        int start = getSup;
        int total = 0;
        int box = 0;

        if (isAdditional) {
            int.TryParse(totalVal.text, out total);
        }
        totalVal.text = 0.ToString();

        //if (getSup > 0) {

        //    for (int i = 0; i < getSup / 2; i++)
        //        
        //}

        supplySpine.AnimationState.SetAnimation(0, "animation", false);

        while (getSup > 0) {
            supply++;
            getSup--;
            basicVal.text = (start - getSup).ToString();
            totalVal.text = (++total).ToString();            
            slider.value = supply / 100.0f;

            value.text = supply.ToString();

            if (getSup % 10 == 0) {
                supplySpine.AnimationState.AddAnimation(0, "animation", false, 0);
                boxSpine.AnimationState.SetAnimation(0, "02.vibration1", false);
            }
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
                Transform alertIcon = boxSpine.gameObject.transform.Find("AlertIcon");

                alertIcon.gameObject.SetActive(true);
                alertIcon.Find("SupplyText").gameObject.SetActive(true);
                alertIcon.Find("SupplyText").gameObject.GetComponent<TMPro.TextMeshProUGUI>().text = (++box).ToString();
                
                boxSpine.AnimationState.SetAnimation(0, "02.vibration1", true);
            }
            yield return new WaitForSeconds(0.01f);            
        }      
        supplySpine.AnimationState.AddAnimation(0, "NOANI", true, 0);
        yield return new WaitForSeconds(0.5f);


        if (PlayMangement.instance.socketHandler.result.reward.x2supply > 0) {
            if (ScenarioGameManagment.scenarioInstance == null) {
                SkeletonGraphic couponAnimation = transform.Find("SecondWindow/PlayerSupply/DoubleTicket").gameObject.GetComponent<SkeletonGraphic>();
                TMPro.TextMeshProUGUI doubleCoupons = transform.Find("SecondWindow/PlayerSupply/ExtraSupply/DoubleButton/Value").GetComponent<TMPro.TextMeshProUGUI>();

                couponAnimation.Initialize(true);
                couponAnimation.Update(0);

                couponAnimation.AnimationState.SetAnimation(0, "animation", false);
                yield return new WaitForSeconds(0.2f);
                doubleCoupons.text = AccountManager.Instance.userData.supplyX2Coupon.ToString();
            }
        }

        //yield return new WaitForSeconds(0.6f);
        start += addSup;
        while (addSup > 0) {
            supply++;
            addSup--;
            basicVal.text = (start - addSup).ToString();
            totalVal.text = (++total).ToString();
            slider.value = supply / 100.0f;

            if (addSup % 10 == 0) {
                supplySpine.AnimationState.AddAnimation(0, "animation", false, 0);
                boxSpine.AnimationState.SetAnimation(0, "02.vibration1", false);
            }
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
                //boxSpine.gameObject.GetComponent<Button>().enabled = true;


                Transform alertIcon = boxSpine.gameObject.transform.Find("AlertIcon");
                alertIcon.gameObject.SetActive(true);
                alertIcon.Find("SupplyText").gameObject.SetActive(true);
                alertIcon.Find("SupplyText").gameObject.GetComponent<TMPro.TextMeshProUGUI>().text = (++box).ToString();
                //if (ScenarioGameManagment.scenarioInstance == null) {
                //    boxSpine.gameObject.GetComponent<Button>().enabled = true;
                //    boxSpine.gameObject.GetComponent<Button>().onClick.AddListener(delegate () {
                //        box--;
                //        alertIcon.Find("SupplyText").gameObject.GetComponent<TMPro.TextMeshProUGUI>().text = box.ToString();
                //        if (box < 1) {
                //            boxSpine.gameObject.GetComponent<Button>().enabled = false;
                //            alertIcon.gameObject.SetActive(false);
                //        }
                //    });
                //}

                boxSpine.AnimationState.SetAnimation(0, "02.vibration1", true);
            }
            yield return new WaitForSeconds(0.01f);
        }
        yield return new WaitForSeconds(0.5f);

        start = winSup;
        while (winSup > 0) {
            supply++;
            winSup--;
            winVal.text = (start - winSup).ToString();
            totalVal.text = (++total).ToString();
            slider.value = supply / 100.0f;

            if (winSup % 10 == 0) {
                supplySpine.AnimationState.AddAnimation(0, "animation", false, 0);
                boxSpine.AnimationState.SetAnimation(0, "02.vibration1", false);
            }
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
                //boxSpine.gameObject.GetComponent<Button>().enabled = true;


                Transform alertIcon = boxSpine.gameObject.transform.Find("AlertIcon");
                alertIcon.gameObject.SetActive(true);
                alertIcon.Find("SupplyText").gameObject.SetActive(true);
                alertIcon.Find("SupplyText").gameObject.GetComponent<TMPro.TextMeshProUGUI>().text = (++box).ToString();

                boxSpine.AnimationState.SetAnimation(0, "02.vibration1", true);
            }
            yield return new WaitForSeconds(0.01f);
        }


        boxSpine.AnimationState.SetAnimation(0, "01.vibration0", true);
        EndRewardLoad.Invoke();
    }

    void SkipUserExp(Slider slider) {
        TMPro.TextMeshProUGUI expValueText = transform.Find("SecondWindow/PlayerExp/ExpSlider/ExpValue").GetComponent<TMPro.TextMeshProUGUI>();
        TMPro.TextMeshProUGUI lvUpValueText = transform.Find("SecondWindow/PlayerExp/ExpSlider/ExpMaxValue").GetComponent<TMPro.TextMeshProUGUI>();
        transform.Find("SecondWindow/PlayerExp/LevelIcon/Value").GetComponent<Text>().text = AccountManager.Instance.userData.lv.ToString();
        slider.value = (float)AccountManager.Instance.userData.exp / lvExp;
        expValueText.text = AccountManager.Instance.userData.exp.ToString();
        lvUpValueText.text = " / " + ((int)lvExp).ToString() + " " + "(" + "+" + getExp.ToString() + ")";
    }

    void SkipUserSupply(Slider slider) {
        TMPro.TextMeshProUGUI value = transform.Find("SecondWindow/PlayerSupply/ExpSlider/SupValue").GetComponent<TMPro.TextMeshProUGUI>();
        SkeletonGraphic boxSpine = transform.Find("SecondWindow/PlayerSupply/BoxSpine").GetComponent<SkeletonGraphic>();
        SkeletonGraphic supplySpine = transform.Find("SecondWindow/PlayerSupply/SupplySpine").gameObject.GetComponent<SkeletonGraphic>();
        TMPro.TextMeshProUGUI basicVal = transform.Find("SecondWindow/PlayerSupply/ExtraSupply/Basic/Value").GetComponent<TMPro.TextMeshProUGUI>();
        TMPro.TextMeshProUGUI winVal = transform.Find("SecondWindow/PlayerSupply/ExtraSupply/Win/Value").GetComponent<TMPro.TextMeshProUGUI>();
        TMPro.TextMeshProUGUI totalVal = transform.Find("SecondWindow/PlayerSupply/SupplyText/Value").GetComponent<TMPro.TextMeshProUGUI>();
        TMPro.TextMeshProUGUI doubleCoupons = transform.Find("SecondWindow/PlayerSupply/ExtraSupply/DoubleButton/Value").GetComponent<TMPro.TextMeshProUGUI>();

        slider.value = (float)AccountManager.Instance.userData.supply / 100.0f;
        value.text = AccountManager.Instance.userData.supply.ToString();
        basicVal.text = (getSupply + additionalSupply).ToString();
        winVal.text = (winSupply > 0) ? winSupply.ToString() : 0.ToString();
        totalVal.text = (getSupply + additionalSupply + winSupply).ToString();
        doubleCoupons.text = AccountManager.Instance.userData.supplyX2Coupon.ToString();
        
        var battleType = PlayerPrefs.GetString("SelectedBattleType");
        if (battleType.Contains("league")) {
            AccountManager
                .Instance
                .mainSceneEffects
                .Enqueue(
                    new MainWindowEffectManager.Effect(
                        MainWindowEffectManager.EffectType.LEAGUE_REWARD, 
                        new object[]{"league", getSupply + additionalSupply + winSupply}
                    )
                );    
        }
        else if (battleType.Equals("story")) {
            AccountManager
                .Instance
                .mainSceneEffects
                .Enqueue(
                    new MainWindowEffectManager.Effect(
                        MainWindowEffectManager.EffectType.LEAGUE_REWARD, 
                        new object[]{"story", getSupply + additionalSupply + winSupply}
                    )
                );
        }
    }


    protected void SkipThreeWinReward() {
        SocketFormat.ResultFormat resultData = this.resultData;
        if (resultData == null) return;
        if (threeWin == null) return;

        Transform slots = threeWin.Find("Slots");
        var winCount = resultData.leagueWinCount;
        var rewardData = resultData.leagueWinReward;

        Logger.Log("winCount" + winCount);

        if (winCount != 0) {
            for (int i = 0; i < winCount - 1; i++) {
                slots.GetChild(i).gameObject.SetActive(true);
            }
        }

        threeWin.gameObject.SetActive(true);

        if (winCount != 0) slots.GetChild(winCount - 1).gameObject.SetActive(true);

        if (winCount == 3) {
            Logger.Log("winCount == 3");

            var spreader = GetComponentInChildren<ResourceSpreader>();
            spreader.SetRandomRange(1, 1);
            spreader.StartSpread(resultData.leagueWinReward[0].amount);

            Transform playerSup = transform.Find("SecondWindow/PlayerSupply");
            var slider = playerSup.Find("ExpSlider/Slider").GetComponent<Slider>();
            winSupply = resultData.leagueWinReward[0].amount;
            //yield return GetUserSupply(slider, 0, 0, resultData.leagueWinReward[0].amount, true);

            for (int i = 0; i < 3; i++) {
                slots.GetChild(i).gameObject.SetActive(false);
            }
            
            AccountManager.Instance.mainSceneEffects.Enqueue(
                new MainWindowEffectManager.Effect(
                    MainWindowEffectManager.EffectType.THREE_WIN,
                    new Object[]{20}
                )
            );
        }
    }


    protected void SkipLeagueData(string result) {
        var leagueInfo = scriptable_leagueData.leagueInfo;
        if (leagueInfo != null) {
            Transform secondWindow = transform.Find("SecondWindow");
            Transform playerMMR = secondWindow.Find("PlayerMmr");
            Transform mmrSlider = playerMMR.Find("MMRSlider");
            
            Image rankIcon = playerMMR.Find("RankIcon").GetComponent<Image>();
            rankIcon.gameObject.SetActive(true);
            
            Image streakFlag = playerMMR.Find("StreakFlag").gameObject.GetComponent<Image>();
            Transform mmrName = playerMMR.transform.Find("Name");
            Transform rankBoard = transform.Find("SecondWindow/PlayerMmr/RankBoard");
            bool isWin = (result == "win") ? true : false;
            var icons = AccountManager.Instance.resource.rankIcons;
            if (icons.ContainsKey(scriptable_leagueData.prevLeagueInfo.rankDetail.id.ToString())) {
                rankIcon.sprite = icons[scriptable_leagueData.prevLeagueInfo.rankDetail.id.ToString()];
            }
            else {
                rankIcon.sprite = icons["default"];
            }
            playerMMR.transform.Find("Name").GetComponent<TMPro.TextMeshProUGUI>().text = scriptable_leagueData.prevLeagueInfo.rankDetail.minorRankName;

            var description = playerMMR.Find("VictoryInfo").GetComponent<TMPro.TextMeshProUGUI>();
            string streak;

            StringBuilder sb = new StringBuilder();
            if (leagueInfo.winningStreak > 1) {
                streak = PlayMangement.instance.uiLocalizeData["ui_ingame_result_winstreak"];
                streak = streak.Replace("{n}", "<color=yellow>" + leagueInfo.winningStreak + "</color>");
                sb
                    .Append(streak);
                streakFlag.sprite = winningStreak;
            }
            else if (leagueInfo.losingStreak > 1) {
                streak = PlayMangement.instance.uiLocalizeData["ui_ingame_result_losestreak"];
                streak = streak.Replace("{n}", "<color=red>" + leagueInfo.losingStreak + "</color>");
                sb
                    .Append(streak);
                streakFlag.sprite = losingStreak;
            }
            description.text = sb.ToString();

            //MMR 증가
            Slider slider = playerMMR.Find("MMRSlider/Slider").GetComponent<Slider>();
            TMPro.TextMeshProUGUI label = playerMMR.Find("MMRSlider/Label").GetComponent<TMPro.TextMeshProUGUI>();

            var prevLeagueInfo = scriptable_leagueData.prevLeagueInfo;
            var newLeagueInfo = scriptable_leagueData.leagueInfo;
            int prevMMR = prevLeagueInfo.ratingPoint;
            int newMMR = newLeagueInfo.ratingPoint;
            int amount = newMMR - prevMMR;

            slider.maxValue = prevLeagueInfo.rankDetail.pointLessThen;
            slider.value = prevMMR;
            label.text = prevMMR + "/" + prevLeagueInfo.rankDetail.pointLessThen + " " + "(" + amount.ToString() + ")";



            if (prevLeagueInfo.rankDetail.id != newLeagueInfo.rankDetail.id) {
                Logger.Log("등급 변동");
                mmrName.gameObject.SetActive(false);
                //1. 승급 혹은 강등전 결과 보여주기 UI
                //2. 승급, 강등 이펙트
                Transform victoryInfo = transform.Find("SecondWindow/PlayerMmr/VictoryInfo");
                victoryInfo.gameObject.SetActive(false);


                if (scriptable_leagueData.prevLeagueInfo.rankingBattleState != "normal") {
                    Logger.Log("승급전 혹은 강등전 진행중!");
                    mmrName.gameObject.SetActive(false);

                    mmrSlider.gameObject.SetActive(false);
                    rankBoard.gameObject.SetActive(true);
                    Transform slotParent = rankBoard.Find("Bottom");
                    string upDown;
                    int slotCnt = 0;
                    if (leagueInfo.rankingBattleState == "rank_up") {
                        Logger.Log("Case 1");
                        upDown = PlayMangement.instance.uiLocalizeData["ui_ingame_result_promotematch"];
                        slotCnt = leagueInfo.rankDetail.rankUpBattleCount.battles;
                        rankBoard.Find("Top/Text").GetComponent<TMPro.TextMeshProUGUI>().text = "upDown";
                    }
                    else if (leagueInfo.rankingBattleState == "rank_down") {
                        Logger.Log("Case 2");
                        slotCnt = leagueInfo.rankDetail.rankDownBattleCount.battles;
                        rankBoard.Find("Top/Text").GetComponent<TMPro.TextMeshProUGUI>().text = "강등전 진행중";
                    }
                    else {
                        Logger.Log("Unknown Case");
                        mmrSlider.gameObject.SetActive(true);
                        rankBoard.gameObject.SetActive(false);
                    }

                    for (int i = 0; i < slotCnt; i++) {
                        slotParent.GetChild(i).gameObject.SetActive(true);
                    }

                    bool[] battleResults = leagueInfo.rankingBattleCount;
                    if (battleResults != null) {
                        for (int i = 0; i < battleResults.Length; i++) {
                            if (battleResults[i]) {
                                slotParent.GetChild(i).Find("Win").gameObject.SetActive(true);
                                slotParent.GetChild(i).Find("Lose").gameObject.SetActive(false);
                            }
                            else {
                                slotParent.GetChild(i).Find("Win").gameObject.SetActive(false);
                                slotParent.GetChild(i).Find("Lose").gameObject.SetActive(true);
                            }
                        }
                    }
                }
                if (scriptable_leagueData.prevLeagueInfo.rankingBattleState == "normal" && scriptable_leagueData.leagueInfo.rankingBattleState != "normal") {
                    Logger.Log("승급전 혹은 강등전 발생!");
                    mmrName.gameObject.SetActive(false);
                    mmrSlider.Find("RankChangeEffect").gameObject.SetActive(true);

                    string upDown;
                    if (leagueInfo.rankingBattleState == "rank_up") {
                        Logger.Log("Case 1");

                        upDown = PlayMangement.instance.uiLocalizeData["ui_ingame_result_promotechance"];
                        mmrSlider.Find("RankChangeEffect/Text").GetComponent<TMPro.TextMeshProUGUI>().text = upDown;
                    }
                    else if (leagueInfo.rankingBattleState == "rank_down") {
                        Logger.Log("Case 2");
                        upDown = PlayMangement.instance.uiLocalizeData["ui_ingame_result_demotewarning"];
                        mmrSlider.Find("RankChangeEffect/Text").GetComponent<TMPro.TextMeshProUGUI>().text = upDown;
                    }
                    else {
                        Logger.Log("Unknown Case");
                        mmrSlider.Find("RankChangeEffect").gameObject.SetActive(false);
                    }
                }


                var prevRankDetail = prevLeagueInfo.rankDetail;
                var rankDetail = newLeagueInfo.rankDetail;

                

                //단판승인가?
                bool isBattleOnce = true;
                if (prevLeagueInfo.rankingBattleCount != null && prevLeagueInfo.rankingBattleCount.Length > 0) {
                    isBattleOnce = false;
                }

                //승급이나 강등이 되어 랭크가 변화함
                mmrSlider.gameObject.SetActive(false);
                rankBoard.gameObject.SetActive(true);
                Transform slots = rankBoard.Find("Bottom");
                if (isBattleOnce) {
                    slots.GetChild(0).gameObject.SetActive(true);
                    isWin = prevLeagueInfo.ratingPoint < newLeagueInfo.ratingPoint;
                    if (isWin) {
                        slots.GetChild(0).Find("Win").gameObject.SetActive(true);
                        description.text = "승급!";
                    }
                    else {
                        slots.GetChild(0).Find("Lose").gameObject.SetActive(true);
                        description.text = "강등!";
                    }
                }
                else {
                    if (prevLeagueInfo.rankingBattleState == "rank_down") {
                        description.text = "강등전 진행중";
                    }
                    else if (prevLeagueInfo.rankingBattleState == "rank_up") {
                        description.text = "승급전 진행중";
                    }

                    for (int i = 0; i < prevLeagueInfo.rankingBattleCount.Length; i++) {
                        slots.GetChild(i).gameObject.SetActive(true);
                        if (prevLeagueInfo.rankingBattleCount[i] == true) {
                            slots.GetChild(i).Find("Win").gameObject.SetActive(true);
                        }
                        else {
                            slots.GetChild(i).Find("Lose").gameObject.SetActive(true);
                        }
                    }

                    slots.GetChild(prevLeagueInfo.rankingBattleCount.Length).gameObject.SetActive(true);

                    isWin = prevLeagueInfo.ratingPoint < newLeagueInfo.ratingPoint;
                    if (isWin) slots.GetChild(0).Find("Win").gameObject.SetActive(true);
                    else slots.GetChild(0).Find("Lose").gameObject.SetActive(true);
                }


                if (prevMMR < newMMR) {
                    Logger.Log("승급");
                    int from = prevMMR;
                    int to = prevLeagueInfo.rankDetail.pointLessThen;
                    label.text = to + "/" + (prevLeagueInfo.rankDetail.pointLessThen - 1) + " " + "(" + amount.ToString() + ")";

                    to = newLeagueInfo.ratingPoint - newLeagueInfo.rankDetail.pointLessThen;
                    slider.maxValue = newLeagueInfo.rankDetail.pointLessThen;
                    slider.value = 0;

                    label.text = (to + newLeagueInfo.rankDetail.pointLessThen) + "/" + (newLeagueInfo.rankDetail.pointLessThen - 1) + " " + "(" + amount.ToString() + ")";
                    slider.value = to;
                    
                    description.text = "승급!";
                    FastShowTierChangeEffect(true);
                }
                else {
                    Logger.Log("강등");

                    int from = prevMMR;
                    int to = prevLeagueInfo.rankDetail.pointOverThen;
                    int value2 = newLeagueInfo.rankDetail.pointLessThen;
                    from = value2;
                    to = newLeagueInfo.ratingPoint;
                    slider.maxValue = newLeagueInfo.rankDetail.pointLessThen;
                    slider.value = to;
                    label.text = to + "/" + (newLeagueInfo.rankDetail.pointLessThen - 1) + " " + "(" + amount.ToString() + ")";
                    description.text = "강등!";
                    FastShowTierChangeEffect(false);
                }
            }
            //승급이나 강등 없음
            else {
                //yield return ShowBattleTableUI(true, isWin);

                if (newMMR > prevLeagueInfo.rankDetail.pointLessThen || newMMR < prevLeagueInfo.rankDetail.pointOverThen) {
                    //게이지 최저치보다 낮은 경우
                    if (newMMR < prevLeagueInfo.rankDetail.pointOverThen) {
                        slider.value = 0;
                    }
                    //게이지 최대치보다 높은 경우
                    if (newMMR > prevLeagueInfo.rankDetail.pointLessThen) {
                        slider.value = slider.maxValue;
                    }

                    int from = prevMMR;
                    int to = newMMR;
                    label.text = to + "/" + (newLeagueInfo.rankDetail.pointLessThen - 1) + " " + "(" + amount.ToString() + ")";
                }
                else {
                    if (prevMMR < newMMR) {
                        Logger.Log("MMR 증가");

                        int from = prevMMR;
                        int to = newMMR;

                        int value = from;
                        if (newMMR > prevLeagueInfo.rankDetail.pointLessThen) {
                            slider.value = prevLeagueInfo.rankDetail.pointLessThen;
                            label.text = newMMR + "/" + (newLeagueInfo.rankDetail.pointLessThen - 1) + " " + "(" + amount.ToString() + ")";
                        }
                        else {
                            slider.value = to;
                            label.text = to + "/" + (newLeagueInfo.rankDetail.pointLessThen - 1) + " " + "(" + amount.ToString() + ")";
                        }
                    }
                    else {
                        Logger.Log("MMR 감소");

                        int from = prevMMR;
                        int to = newMMR;

                        int value = from;
                        if (newMMR < prevLeagueInfo.rankDetail.pointOverThen) {
                            slider.value = 0;
                            label.text = newMMR + "/" + (newLeagueInfo.rankDetail.pointLessThen - 1) + " " + "(" + amount.ToString() + ")";
                            Logger.Log("강등 위기");
                        }

                        else {
                            slider.value = to;
                            label.text = to + "/" + newLeagueInfo.rankDetail.pointLessThen + " " + "(" + amount.ToString() + ")";
                        }
                    }
                }
            }

            if (newMMR <= 0)
                slider.gameObject.transform.Find("Fill Area/Fill/Effect").gameObject.SetActive(false);
            else
                slider.gameObject.transform.Find("Fill Area/Fill/Effect").gameObject.SetActive(true);

            
            if (icons.ContainsKey(leagueInfo.rankDetail.id.ToString())) {
                rankIcon.sprite = icons[leagueInfo.rankDetail.id.ToString()];
                playerMMR.transform.Find("Name").GetComponent<TMPro.TextMeshProUGUI>().text = leagueInfo.rankDetail.minorRankName;
            }
            else {
                rankIcon.sprite = icons["default"];
            }
        }
    }

    //쿠폰 사용 버튼 클릭
    public void OnDoubleCouponButton() {
        Modal.instantiate("보상을 추가로 획득 가능합니다.\n보급품 2배 획득 쿠폰을 사용하시겠습니까?", Modal.Type.YESNO, () => { ClaimDoubleReq(); });
    }

    private void ClaimDoubleReq() {
        Logger.Log("ClaimDoubleReq");
        PlayMangement.instance.SettingMethod(BattleConnector.SendMessageList.claim_2x_reward);
    }

    /// <summary>
    /// AI 대전에서는 결과화면에서 보상화면이 나오지 않게 바꿔야함
    /// </summary>
    public void ChangeResultButtonFunction() {
        Logger.Log("AI 대전의 결과창 화면 세팅 변경");
        Button btn = transform.Find("FirstWindow/GotoRewardButton").GetComponent<Button>();
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(() => {
            PlayMangement.instance.SettingMethod(BattleConnector.SendMessageList.end_game);
            OnMoveSceneBtn();
        });
        //btn.transform.GetChild(0).GetComponent<TMPro.TextMeshProUGUI>().text = "메인으로";
    }

    private string GetTierAnimName(string keyword) {
        var rankIcons = AccountManager.Instance.resource.rankIcons;
        if (rankIcons.ContainsKey(keyword)) {
            return rankIcons[keyword].name;
        }
        return "1";
    }


    private void FirstWinningTalking() {
        int winningStreak = scriptable_leagueData.leagueInfo.winningStreak;
        isFirstWinningTalkRevealed = true;
        
        if (winningStreak == 1 && result == "win") {
            List<Tutorial.CommonTalking> talkingScript = new List<Tutorial.CommonTalking>();

            string dataAsJson = ((TextAsset)Resources.Load("TutorialDatas/CommonTalkData")).text;
            talkingScript = dataModules.JsonReader.Read<List<Tutorial.CommonTalking>>(dataAsJson);
            if (talkingScript.Count <= 0) return;
            PlayMangement.instance.RefreshScript();
            Tutorial.CommonTalking whatTalk = talkingScript.Find(x => x.talkingTiming == "AfterFirstWinLeague");
            Tutorial.ScriptData script = whatTalk.scripts[0];
            PlayMangement.instance.gameObject.GetComponent<ScenarioExecuteHandler>().Initialize(script);
        }
    }

    [SerializeField] Transform threeWin;
    /// <summary>
    /// 3승 UI 처리
    /// </summary>
    /// <returns></returns>
    IEnumerator StartThreeWinEffect() {
        SocketFormat.ResultFormat resultData = this.resultData;
        if(resultData == null) yield return 0;

        Transform slots = threeWin.Find("Slots");
        var winCount = resultData.leagueWinCount;
        var rewardData = resultData.leagueWinReward;

        //test code
        //winCount = 3;
        //end test code

        Logger.Log("winCount" + winCount);

        if(winCount != 0) {
            for (int i = 0; i < winCount - 1; i++) {
                slots.GetChild(i).gameObject.SetActive(true);
            }
        }

        threeWin.gameObject.SetActive(true);

        yield return new WaitForSeconds(1.0f);

        if(winCount != 0) slots.GetChild(winCount - 1).gameObject.SetActive(true);

        if(winCount == 3) {
            Logger.Log("winCount == 3");

            var spreader = GetComponentInChildren<ResourceSpreader>();
            spreader.SetRandomRange(1, 1);
            spreader.StartSpread(resultData.leagueWinReward[0].amount);

            Transform playerSup = transform.Find("SecondWindow/PlayerSupply");
            var slider = playerSup.Find("ExpSlider/Slider").GetComponent<Slider>();
            winSupply = resultData.leagueWinReward[0].amount;
            //yield return GetUserSupply(slider, 0, 0, resultData.leagueWinReward[0].amount, true);

            for (int i = 0; i < 3; i++) {
                slots.GetChild(i).gameObject.SetActive(false);
            }
            
            AccountManager.Instance.mainSceneEffects.Enqueue(
                new MainWindowEffectManager.Effect(
                    MainWindowEffectManager.EffectType.THREE_WIN,
                    new Object[]{20}
                )
            );
        }
    }
    



    protected IEnumerator WaitDeadAni(string result) {
        if (result == "win" && PlayMangement.instance.enemyPlayer.HP.Value == 0)
            yield return new WaitForSeconds(PlayMangement.instance.enemyPlayer.DeadAnimationTime);
        else if (result == "win")
            yield return null;
        else if (result == "lose" && PlayMangement.instance.player.HP.Value == 0)
            yield return new WaitForSeconds(PlayMangement.instance.player.DeadAnimationTime);
        else
            yield return null;
    }

    public IEnumerator WaitResult(string result, bool isHuman, SocketFormat.ResultFormat resultData, bool skipAni = false) {
        if (skipAni == false)
            yield return WaitDeadAni(result);
        yield return new WaitUntil(() => PlayMangement.instance.waitShowResult == false);
        yield return new WaitUntil(() => PlayMangement.instance.socketHandler.result != null);
        SetResultWindow(result, isHuman, resultData);
    }


    public void SkipResult() {
        StopAllCoroutines();
        skipResult?.SetActive(false);
        Transform playerExp = transform.Find("SecondWindow/PlayerExp");
        Transform playerSup = transform.Find("SecondWindow/PlayerSupply");
        Transform playerMMR = transform.Find("SecondWindow/PlayerMmr");

        playerExp.localScale = Vector3.one;
        playerSup.localScale = Vector3.one;
        playerMMR.localScale = Vector3.one;

        Slider expSlider = transform.Find("SecondWindow/PlayerExp/ExpSlider/Slider").GetComponent<Slider>();
        Slider supplySlider = playerSup.Find("ExpSlider/Slider").GetComponent<Slider>();

        SkipUserExp(expSlider);
        StartCoroutine(SetLevelUP());
        SkipThreeWinReward();
        SkipUserSupply(supplySlider);
        SkipLeagueData(result);
        StartCoroutine(StartShowReward(result));
        
        FirstWinningTalking();
    }
}