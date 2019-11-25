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

public class GameResultManager : MonoBehaviour {
    public GameObject SocketDisconnectedUI;
    [SerializeField] Transform BgCanvas;

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
            btn.transform.GetChild(0).GetComponent<TMPro.TextMeshProUGUI>().text = "보상으로";
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
        transform.Find("SecondWindow/Buttons/FindNewGame").GetComponent<Button>().interactable = false;
        transform.Find("SecondWindow/Buttons/BattleReady").GetComponent<Button>().interactable = false;

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
        var battleConnector = PlayMangement.instance.SocketHandler;
        var leagueInfo = battleConnector.leagueInfo;
        if (leagueInfo != null) {
            Transform secondWindow = transform.Find("SecondWindow");
            Transform playerMMR = secondWindow.Find("PlayerMmr");
            Image rankIcon = playerMMR.Find("RankIcon").GetComponent<Image>();
            var icons = AccountManager.Instance.resource.rankIcons;
            if (!string.IsNullOrEmpty(scriptable_leagueData.prevRank) && icons.ContainsKey(scriptable_leagueData.prevRank)) {
                rankIcon.sprite = icons[scriptable_leagueData.prevRank];
            }
            else {
                rankIcon.sprite = icons["default"];
            }
            rankIcon.transform.Find("Name").GetComponent<TMPro.TextMeshProUGUI>().text = scriptable_leagueData.prevRank;

            var description = playerMMR.Find("VictoryInfo").GetComponent<TMPro.TextMeshProUGUI>();
            StringBuilder sb = new StringBuilder();
            if(leagueInfo.winningStreak > 0) {
                sb
                    .Append("<color=yellow>")
                    .Append(leagueInfo.winningStreak)
                    .Append("</color> 연승중");
            }
            else if(leagueInfo.losingStreak > 0) {
                sb
                    .Append("<color=red>")
                    .Append(leagueInfo.losingStreak)
                    .Append("</color> 연패중");
            }
            description.text = sb.ToString();

            int pointUp = PlayMangement.instance.SocketHandler.result.pointUp;
            Slider slider = playerMMR.Find("ExpSlider/Slider").GetComponent<Slider>();
            int sliderMaxVal = leagueInfo.ratingPoint + leagueInfo.rankDetail.next;
            float currentVal = leagueInfo.ratingPoint - pointUp;
            slider.value = currentVal / sliderMaxVal;

            var expValue = playerMMR.Find("ExpSlider/ExpValue").GetComponent<TMPro.TextMeshProUGUI>();

            if (scriptable_leagueData.prevMMR < leagueInfo.ratingPoint) {
                float offset = 1f;
                while (currentVal < leagueInfo.ratingPoint) {
                    currentVal += offset;
                    slider.value = currentVal / (float)sliderMaxVal;

                    expValue.text = currentVal + "/" + sliderMaxVal;
                    yield return new WaitForSeconds(0.01f);
                }
            }
            else {
                expValue.text = leagueInfo.ratingPoint + "/" + sliderMaxVal;
            }

            if (icons.ContainsKey(leagueInfo.rankDetail.minor)) {
                if(scriptable_leagueData.prevRank != null && scriptable_leagueData.prevRank != leagueInfo.rankDetail.minor) {
                    //TODO : 승급인지 강등인지 구분 필요
                    if(pointUp > 0) {
                        Logger.Log("승급!");
                    }
                    else {
                        Logger.Log("강등!");
                    }
                }

                rankIcon.sprite = icons[leagueInfo.rankDetail.minor];
                rankIcon.transform.Find("Name").GetComponent<TMPro.TextMeshProUGUI>().text = leagueInfo.rankDetail.minor;
            }
            else {
                rankIcon.sprite = icons["default"];
            }
        }
        yield return 0;

    }

    public IEnumerator SetTestLeagueData() {
        var battleConnector = PlayMangement.instance.SocketHandler;
        AccountManager.LeagueInfo leagueInfo = new AccountManager.LeagueInfo();
        leagueInfo.winningStreak = 1;
        leagueInfo.losingStreak = 0;
        leagueInfo.rankDetail = new AccountManager.RankDetail();
        leagueInfo.rankDetail.minor = "오합지졸 우두머리";
        leagueInfo.rankDetail.major = "지도자";
        leagueInfo.ratingPoint = 40;
        leagueInfo.rankDetail.next = 300;

        scriptable_leagueData.prevMMR = 280;
        scriptable_leagueData.newMMR = 320;
        scriptable_leagueData.prevRank = "무명 병사";

        int pointUp = 70;

        if (leagueInfo != null) {
            Transform secondWindow = transform.Find("SecondWindow");
            Transform playerMMR = secondWindow.Find("PlayerMmr");
            Image rankIcon = playerMMR.Find("RankIcon").GetComponent<Image>();

            var icons = AccountManager.Instance.resource.rankIcons;
            if (icons.ContainsKey(scriptable_leagueData.prevRank)) {
                rankIcon.sprite = icons[scriptable_leagueData.prevRank];
            }
            else {
                rankIcon.sprite = icons["default"];
            }
            rankIcon.transform.Find("Name").GetComponent<TMPro.TextMeshProUGUI>().text = scriptable_leagueData.prevRank;

            var description = playerMMR.Find("VictoryInfo").GetComponent<TMPro.TextMeshProUGUI>();
            StringBuilder sb = new StringBuilder();
            if (leagueInfo.winningStreak > 0) {
                sb
                    .Append("<color=yellow>")
                    .Append(leagueInfo.winningStreak)
                    .Append("</color> 연승중");
            }
            else if (leagueInfo.losingStreak > 0) {
                sb
                    .Append("<color=red>")
                    .Append(leagueInfo.losingStreak)
                    .Append("</color> 연패중");
            }
            description.text = sb.ToString();
            
            Image slider = playerMMR.Find("ExpSlider/SliderValue").GetComponent<Image>();
            int sliderMaxVal = leagueInfo.ratingPoint + leagueInfo.rankDetail.next;
            float currentVal = leagueInfo.ratingPoint - pointUp;
            slider.fillAmount = currentVal / sliderMaxVal;
            var expValue = playerMMR.Find("ExpSlider/ExpValue").GetComponent<TMPro.TextMeshProUGUI>();

            float offset = 1f;
            while (currentVal < leagueInfo.ratingPoint) {
                currentVal += offset;
                slider.fillAmount = currentVal / (float)sliderMaxVal;
                expValue.text = currentVal + "/" + sliderMaxVal;
                yield return new WaitForSeconds(0.01f);
            }

            if (icons.ContainsKey(leagueInfo.rankDetail.minor)) {
                if (scriptable_leagueData.prevRank != null && scriptable_leagueData.prevRank != leagueInfo.rankDetail.minor) {
                    //TODO : 승급인지 강등인지 구분 필요
                    if (pointUp > 0) {
                        Logger.Log("승급!");
                    }
                    else {
                        Logger.Log("강등!");
                    }
                }

                rankIcon.sprite = icons[leagueInfo.rankDetail.minor];
                rankIcon.transform.Find("Name").GetComponent<TMPro.TextMeshProUGUI>().text = leagueInfo.rankDetail.minor;
            }
            else {
                rankIcon.sprite = icons["default"];
            }
        }
        yield return 0;

    }

    public float currentTime = 0;
    private const float WAIT_TIME = 5.0f;
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
        btn.transform.GetChild(0).GetComponent<TMPro.TextMeshProUGUI>().text = "메인으로";
    }
}
