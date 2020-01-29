using System;
using System.Linq;
using System.Threading.Tasks;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine;
using BestHTTP.WebSocket;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SocketFormat;
using IngameEditor;
using TMPro;

public delegate void DequeueCallback();

/// 서버로부터 데이터를 받아올 때 reflection으로 string을 함수로 바로 발동하게 하는 부분
public partial class BattleConnector : MonoBehaviour {
    public GameState gameState;
    private string raceName;
    public Queue<ReceiveFormat> queue = new Queue<ReceiveFormat>();


    protected BattleStack battleStack = new BattleStack();
    public ShieldStack shieldStack = new ShieldStack();

    private Type thisType;
    public ResultFormat result = null;
    public bool isOpponentPlayerDisconnected = false;

    string matchKey = string.Empty;
    public static bool canPlaySound = true;
    protected bool dequeueing = false;
    public DequeueCallback callback;
    private GameObject reconnectModal;


    private void ReceiveMessage(WebSocket webSocket, string message) {
        ReceiveFormat result = dataModules.JsonReader.Read<ReceiveFormat>(message);
        queue.Enqueue(result);
    }

    private void showMessage(ReceiveFormat result) {
        JObject json = null;
        if(result.gameState != null) {
            json = JObject.Parse(JsonConvert.SerializeObject(result.gameState.map));
            json["lines"].Parent.Remove();
            for(int i = 0; i < json["allMonster"].Count() ; i++) json["allMonster"][i]["skills"].Parent.Remove();
        }
        Logger.Log(string.Format("메소드 : {0}, args : {1}, map : {2}", result.method, result.args, 
        result.gameState != null ? JsonConvert.SerializeObject(json, Formatting.Indented)  : null));
    }
    #if UNITY_EDITOR
    private void FixedUpdate() {
        if(Input.GetKeyDown(KeyCode.D)) webSocket.Close(500, "shutdown");
    }
    #endif

    private void Update() {
        DequeueSocket();
    }

    private void Start() {
        callback = () => dequeueing = false;
    }
    
    private void DequeueSocket() {
        if(dequeueing || queue.Count == 0) return;
        dequeueing = true;
        ReceiveFormat result = queue.Dequeue();
        if(result.gameState != null) gameState = result.gameState;
        if(result.error != null) {
            Logger.LogError("WebSocket play wrong Error : " + result.error);
            dequeueing = false;
        }
        
        if(result.method == null) {dequeueing = false; return;}
        MethodInfo theMethod = thisType.GetMethod(result.method);
        if(theMethod == null) {dequeueing = false; return;}
        
        object[] args = new object[]{result.args, result.id, callback};
        showMessage(result);
        try {
            theMethod.Invoke(this, args);
        }
        catch(Exception e) {
            Debug.LogError("Message Method : " + result.method + "Error : " + e);
            callback();
        }
    }

    AccountManager.LeagueInfo orcLeagueInfo, humanLeagueInfo;
    public void begin_ready(object args, int? id, DequeueCallback callback) {
        string battleType = PlayerPrefs.GetString("SelectedBattleType");
        if (battleType == "league" || battleType == "leagueTest") {
            JObject json = (JObject)args;
            orcLeagueInfo = dataModules.JsonReader.Read<AccountManager.LeagueInfo>(json["orc"].ToString());
            humanLeagueInfo = dataModules.JsonReader.Read<AccountManager.LeagueInfo>(json["human"].ToString());

            CustomVibrate.Vibrate(1000);
        }

        this.message.text = "대전 상대를 찾았습니다!";
        textBlur.SetActive(true);
        FindObjectOfType<BattleConnectSceneAnimController>().PlayStartBattleAnim();

        StopCoroutine(timeCheck);
        if (canPlaySound) {
            SoundManager.Instance.PlayIngameSfx(IngameSfxSound.GAMEMATCH);
        }
        
        SetUserInfoText();
        SetSaveGameId();
        callback();
    }

    public void SetSaveGameId() {
        string gameId = gameState.gameId;
        string camp = PlayerPrefs.GetString("SelectedRace").ToLower();
        string battleType = PlayerPrefs.GetString("SelectedBattleType");
        NetworkManager.ReconnectData data = new NetworkManager.ReconnectData(gameId, camp, battleType);
        PlayerPrefs.SetString("ReconnectData", JsonConvert.SerializeObject(data));
    }

    /// <summary>
    /// 매칭 화면에서 유저 정보 표기
    /// </summary>
    private void SetUserInfoText() {
        string race = PlayerPrefs.GetString("SelectedRace").ToLower();
        string mode = PlayerPrefs.GetString("SelectedBattleType");

        var orcPlayerData = gameState.players.orc;
        var orcUserData = orcPlayerData.user;
        var humanPlayerData = gameState.players.human;
        var humanUserData = humanPlayerData.user;

        string orcPlayerNickName = orcUserData.nickName;
        if (string.IsNullOrEmpty(orcPlayerNickName)) orcPlayerNickName = "Bot";

        string orcHeroName = orcPlayerData.hero.name;
        string humanPlayerNickName = humanUserData.nickName;
        if (string.IsNullOrEmpty(humanPlayerNickName)) humanPlayerNickName = "Bot";
        string humanHeroName = humanPlayerData.hero.name;

        //Logger.Log(orcPlayerNickName);
        //Logger.Log(humanPlayerNickName);

        TextMeshProUGUI enemyNickNameTxt = machine.transform.Find("EnemyName/PlayerName").GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI enemyHeroNameTxt = machine.transform.Find("EnemyHero/HeroName").GetComponent<TextMeshProUGUI>();

        TextMeshProUGUI playerNickNameTxt = machine.transform.Find("PlayerName/PlayerName").GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI playerHeroNameTxt = machine.transform.Find("PlayerHero/HeroName").GetComponent<TextMeshProUGUI>();

        //Logger.Log(race);
        Transform enemyName = machine.transform.Find("EnemyName");
        Transform playerName = machine.transform.Find("PlayerName");

        Transform playerHero = machine.transform.Find("PlayerHero");
        Transform enemyHero = machine.transform.Find("EnemyHero");

        var PlayerTierParent = playerHero.Find("Tier");
        var EnemyTierParent = enemyHero.Find("Tier");
        int humanTier = gameState.players.human.hero.tier;
        int orcTier = gameState.players.orc.hero.tier;
        

        if (race == "human") {
            playerHeroNameTxt.text = "<color=#BED6FF>" + humanHeroName + "</color>";
            playerNickNameTxt.text = humanPlayerNickName;

            enemyHeroNameTxt.text = (mode == "story") ? "<color=#FFCACA>" + "오크 부족장" + "</color>" : "<color=#FFCACA>" + orcHeroName + "</color>";
            enemyNickNameTxt.text = (mode == "story") ? "오크 부족장" : orcPlayerNickName;


            for (int i = 0; i < humanTier; i++) {
                PlayerTierParent.GetChild(i).Find("Active").gameObject.SetActive(true);
                PlayerTierParent.GetChild(i).Find("Deactive").gameObject.SetActive(false);
            }
            for (int i = 0; i < orcTier; i++) {
                EnemyTierParent.GetChild(i).Find("Active").gameObject.SetActive(true);
                EnemyTierParent.GetChild(i).Find("Deactive").gameObject.SetActive(false);
            }
            if (mode == "story")
                machine.transform.Find("EnemyCharacter/EnemyKracus").gameObject.GetComponent<Image>().sprite = AccountManager.Instance.resource.heroPortraite["qh10002"];
            else
                machine.transform.Find("EnemyCharacter/EnemyKracus").gameObject.GetComponent<Image>().sprite = AccountManager.Instance.resource.heroPortraite[gameState.players.orc.hero.id];
        }
        else if (race == "orc") {
            playerHeroNameTxt.text = "<color=#FFCACA>" + orcHeroName + "</color>";
            playerNickNameTxt.text = orcPlayerNickName;

            enemyHeroNameTxt.text = (mode == "story") ? "<color=#BED6FF>" + "레이 첸 민" + "</color>" : "<color=#BED6FF>" + humanHeroName + "</color>";
            enemyNickNameTxt.text = (mode == "story") ? "레이 첸 민" : humanPlayerNickName;

            for (int i = 0; i < orcTier; i++) {
                PlayerTierParent.GetChild(i).Find("Active").gameObject.SetActive(true);
                PlayerTierParent.GetChild(i).Find("Deactive").gameObject.SetActive(false);
            }
            for (int i = 0; i < humanTier; i++) {
                EnemyTierParent.GetChild(i).Find("Active").gameObject.SetActive(true);
                EnemyTierParent.GetChild(i).Find("Deactive").gameObject.SetActive(false);
            }
            
            if (mode == "story")
                machine.transform.Find("EnemyCharacter/EnemyZerod").gameObject.GetComponent<Image>().sprite = AccountManager.Instance.resource.heroPortraite["qh10001"];
            else
                machine.transform.Find("EnemyCharacter/EnemyZerod").gameObject.GetComponent<Image>().sprite = AccountManager.Instance.resource.heroPortraite[gameState.players.human.hero.id];

        }
        timer.text = null;
        returnButton.onClick.RemoveListener(BattleCancel);
        returnButton.gameObject.SetActive(false);

        if (mode == "league" || mode == "leagueTest") {
            if(race == "human") {
                playerName.Find("MMR/Value").GetComponent<TextMeshProUGUI>().text = humanLeagueInfo.ratingPoint.ToString();
                enemyName.Find("MMR/Value").GetComponent<TextMeshProUGUI>().text = orcLeagueInfo.ratingPoint.ToString();

                var icons = AccountManager.Instance.resource.rankIcons;
                if (icons.ContainsKey(humanLeagueInfo.rankDetail.minorRankName)) {
                    playerName.Find("TierIcon").GetComponent<Image>().sprite = icons[humanLeagueInfo.rankDetail.minorRankName];
                }
                else {
                    playerName.Find("TierIcon").GetComponent<Image>().sprite = icons["default"];
                }
                if (icons.ContainsKey(orcLeagueInfo.rankDetail.minorRankName)) {
                    enemyName.Find("TierIcon").GetComponent<Image>().sprite = icons[orcLeagueInfo.rankDetail.minorRankName];
                }
                else {
                    enemyName.Find("TierIcon").GetComponent<Image>().sprite = icons["default"];
                }
            }
            else {
                playerName.Find("MMR/Value").GetComponent<TextMeshProUGUI>().text = orcLeagueInfo.ratingPoint.ToString();
                enemyName.Find("MMR/Value").GetComponent<TextMeshProUGUI>().text = humanLeagueInfo.ratingPoint.ToString();

                var icons = AccountManager.Instance.resource.rankIcons;
                if (icons.ContainsKey(orcLeagueInfo.rankDetail.minorRankName)) {
                    playerName.Find("TierIcon").GetComponent<Image>().sprite = icons[orcLeagueInfo.rankDetail.minorRankName];
                }
                else {
                    playerName.Find("TierIcon").GetComponent<Image>().sprite = icons["default"];
                }
                if (icons.ContainsKey(humanLeagueInfo.rankDetail.minorRankName)) {
                    enemyName.Find("TierIcon").GetComponent<Image>().sprite = icons[humanLeagueInfo.rankDetail.minorRankName];
                }
                else {
                    enemyName.Find("TierIcon").GetComponent<Image>().sprite = icons["default"];
                }
            }
        }
        else {
            playerName.Find("MMR").gameObject.SetActive(false);
            enemyName.Find("MMR").gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// 로비 연결 끊어짐 (연결 성공 혹은 유저가 로비를 나갔을 때
    /// </summary>
    /// <param name="args"></param>
    /// <param name="id"></param>
    public void disconnected(object args, int? id, DequeueCallback callback) {

    }

    public void entrance_complete(object args, int? id, DequeueCallback callback) {
        
    }

    public void matched(object args, int? id, DequeueCallback callback) {
        var json = (JObject)args;
        matchKey = json["matchKey"].ToString();
        JoinGame();
        callback();
    }

    public void end_ready(object args, int? id, DequeueCallback callback) { 
        bool isTest = PlayerPrefs.GetString("SelectedBattleType").CompareTo("test") == 0;
        if(isTest) {
            object value = JsonUtility.FromJson(PlayerPrefs.GetString("Editor_startState"), typeof(StartState));
            SendStartState(value);
        }
        callback();
    }

    public void begin_mulligan(object args, int? id, DequeueCallback callback) {
        //TODO : 멀리건 시작인데 해주는게 없다? 카드를 뽑아줘야할 타이밍 파악 필요
        if(ScenarioGameManagment.scenarioInstance == null) {
            PlayMangement.instance.player.GetComponent<IngameTimer>().BeginTimer(30);
            StartCoroutine(PlayMangement.instance.GenerateCard(callback));
        }
        else {
            callback();
            MulliganEnd();
        }
    }

    public void hand_changed(object args, int? id, DequeueCallback callback) {
        if(PlayMangement.instance == null) return;
        var json = (JObject)args;
        bool isHuman = PlayMangement.instance.player.isHuman;
        raceName = isHuman ? "human" : "orc";
        
        if(json["camp"].ToString().CompareTo(raceName) != 0) {callback(); return;}

        Card newCard = gameState.players.myPlayer(isHuman).newCard;
        HandchangeCallback(newCard.id, newCard.itemId, false);
        HandchangeCallback = null;
        callback();
    }

    public void end_mulligan(object args, int? id, DequeueCallback callback) {
        CardHandManager cardHandManager = PlayMangement.instance.cardHandManager;
        if(!cardHandManager.socketDone)
            cardHandManager.FirstDrawCardChange();

        PlayMangement.instance.surrendButton.enabled = true;
        callback();
    }

    public void begin_turn_start(object args, int? id, DequeueCallback callback) {
        PlayMangement.instance.SyncPlayerHp();
        callback();
    }
    
    public void end_turn_start(object args, int? id, DequeueCallback callback) {
        DebugSocketData.StartCheckMonster(gameState);
        callback();
    }

    public void begin_orc_pre_turn(object args, int? id, DequeueCallback callback) {
        PlayerController player;
        player = PlayMangement.instance.player.isHuman ? PlayMangement.instance.enemyPlayer : PlayMangement.instance.player;
        if (ScenarioGameManagment.scenarioInstance == null) {
            player.GetComponent<IngameTimer>().RopeTimerOn();
        }
        callback();
    }

    public void end_orc_pre_turn(object args, int? id, DequeueCallback callback) {
        PlayerController player;
        player = PlayMangement.instance.player.isHuman ? PlayMangement.instance.enemyPlayer : PlayMangement.instance.player;
        if(ScenarioGameManagment.scenarioInstance == null) {
            player.GetComponent<IngameTimer>().RopeTimerOff();
        }
        PlayMangement.instance.EventHandler.PostNotification(IngameEventHandler.EVENT_TYPE.END_TURN_BTN_CLICKED, this, TurnType.ORC);
        callback();
    }

    public void begin_human_turn(object args, int? id, DequeueCallback callback) {
        PlayerController player;
        player = PlayMangement.instance.player.isHuman ? PlayMangement.instance.player : PlayMangement.instance.enemyPlayer;
        if(ScenarioGameManagment.scenarioInstance == null) {
            player.GetComponent<IngameTimer>().RopeTimerOn(30);
        }
        callback();
    }

    public void end_human_turn(object args, int? id, DequeueCallback callback) {
        PlayerController player;
        player = PlayMangement.instance.player.isHuman ? PlayMangement.instance.player : PlayMangement.instance.enemyPlayer;
        if(ScenarioGameManagment.scenarioInstance == null) {
            player.GetComponent<IngameTimer>().RopeTimerOff();
        }
        PlayMangement.instance.EventHandler.PostNotification(IngameEventHandler.EVENT_TYPE.END_TURN_BTN_CLICKED, this, TurnType.HUMAN);
        callback();
    }

    public void begin_orc_post_turn(object args, int? id, DequeueCallback callback) {
        PlayerController player;
        player = PlayMangement.instance.player.isHuman ? PlayMangement.instance.enemyPlayer : PlayMangement.instance.player;
        if(ScenarioGameManagment.scenarioInstance == null) {
            player.GetComponent<IngameTimer>().RopeTimerOn();
        }
        callback();
    }

    public void end_orc_post_turn(object args, int? id, DequeueCallback callback) {
        PlayerController player;
        player = PlayMangement.instance.player.isHuman ? PlayMangement.instance.enemyPlayer : PlayMangement.instance.player;
        if(ScenarioGameManagment.scenarioInstance == null) {
            player.GetComponent<IngameTimer>().RopeTimerOff();
        }
        PlayMangement.instance.EventHandler.PostNotification(IngameEventHandler.EVENT_TYPE.END_TURN_BTN_CLICKED, this, TurnType.SECRET);
        callback();
    }

    public Queue<int> unitSkillList = new Queue<int>();//일단 임시

    public void skill_activated(object args, int? id, DequeueCallback callback) {
        if(PlayMangement.instance.enemyPlayer.isHuman) return;
        var json = (JObject)args;
        int itemId = int.Parse(json["targets"][0]["args"][0].ToString());
        unitSkillList.Enqueue(itemId);
        callback();
    }

    public async void begin_battle_turn(object args, int? id, DequeueCallback callback) {
        await Task.Delay(1000);
        callback();
        
    }

    public void end_battle_turn(object args, int? id, DequeueCallback callback) {
        callback();
        if (PlayMangement.instance == null) return;
    }

    public void line_battle(object args, int? id, DequeueCallback callback) {
        var json = (JObject)args;
        string line = json["lineNumber"].ToString();
        string camp = json["camp"].ToString();
        int line_num = int.Parse(line);
        PlayMangement.instance.StartBattle(camp, line_num, battleStack.BattleCamp(camp), callback);
    }

    public void map_clear(object args, int? id, DequeueCallback callback) {
        var json = (JObject)args;
        string line = json["lineNumber"].ToString();
        int line_num = int.Parse(line);    
        
        PlayMangement.instance.CheckLine(line_num, battleStack.CheckEndSecond() ,callback);
        shieldStack.ResetShield();
    }

    IngameTimer ingameTimer;

    public void begin_shield_turn(object args, int? id, DequeueCallback callback) {
        var json = (JObject)args;
        string camp = json["camp"].ToString();
        bool isHuman = PlayMangement.instance.player.isHuman;
        //human 실드 발동
        if (camp == "human") {
            if (!isHuman) {
                ingameTimer = PlayMangement.instance.enemyPlayer.GetComponent<IngameTimer>();
                ingameTimer.PauseTimer(20);
            }
        }
        //orc 실드 발동
        else {
            if (isHuman) {
                ingameTimer = PlayMangement.instance.enemyPlayer.GetComponent<IngameTimer>();
                ingameTimer.PauseTimer(20);
            }
        }
    }

    public void end_shield_turn(object args, int? id, DequeueCallback callback) { 
        PlayMangement.instance.heroShieldDone.Add(true);
        if(ingameTimer != null) {
            ingameTimer.ResumeTimer();
            ingameTimer = null;
        }
    }

    bool isSurrender = false;

    public void surrender(object args, int? id, DequeueCallback callback) {
        var json = (JObject)args;
        string camp = json["camp"].ToString();
        //Logger.Log(camp + "측 항복");
        isSurrender = true;
        string result = "";
        bool isHuman = PlayMangement.instance.player.isHuman;

        if ((isHuman && camp == "human") || (!isHuman && camp == "orc")) {
            result = "lose";
        }
        else {
            result = "win";
        }
        StartCoroutine(SetResult(result, isHuman));
    }

    IEnumerator SetResult(string result, bool isHuman) {
        yield return new WaitForSeconds(0.5f);
        PlayMangement.instance.resultManager.SetResultWindow(result, isHuman);
    }

    public IEnumerator waitSkillDone(UnityAction callback, bool isShield = false) {
        if(isShield) { 
            yield return new WaitUntil(() => PlayMangement.instance.heroShieldActive);
            yield return new WaitForSeconds(2.0f);
        }
        MagicDragHandler[] list = Resources.FindObjectsOfTypeAll<MagicDragHandler>();
        foreach(MagicDragHandler magic in list) {
            if(magic.skillHandler == null) continue;
            
            if(!(magic.skillHandler.finallyDone && magic.skillHandler.socketDone)) {
                yield return new WaitUntil(() => magic.skillHandler.finallyDone && magic.skillHandler.socketDone);
                yield return new WaitForSeconds(1.0f);
            }
        }
        PlaceMonster[] list2 = FindObjectsOfType<PlaceMonster>();
        foreach(PlaceMonster unit in list2) {
            if(unit.skillHandler == null) continue;
            
            if(!(unit.skillHandler.finallyDone && unit.skillHandler.socketDone)) {
                yield return new WaitUntil(() => unit.skillHandler.finallyDone && unit.skillHandler.socketDone);
                yield return new WaitForSeconds(1.0f);
            }
        }
        callback();
    }

    public void shield_gauge(object args, int? id, DequeueCallback callback) {
        var json = (JObject)args;
        string camp = json["camp"].ToString();
        string gauge = json["shieldGet"].ToString();
        ShieldCharge charge = new ShieldCharge();
        charge.shieldCount = int.Parse(gauge);
        charge.camp = camp;
        if(charge.shieldCount == 0) return;
        shieldStack.SavingShieldGauge(camp, int.Parse(gauge));
        callback();
    }

    public void begin_end_turn(object args, int? id, DequeueCallback callback) {callback();}

    public void end_end_turn(object args, int? id, DequeueCallback callback) {
        PlayMangement.instance.EventHandler.PostNotification(IngameEventHandler.EVENT_TYPE.END_TURN_BTN_CLICKED, this, TurnType.BATTLE);
        callback();
    }

    public void opponent_connection_closed(object args, int? id, DequeueCallback callback) {
        PlayMangement.instance.resultManager.SocketErrorUIOpen(true);
        callback();
    }

    public LeagueData leagueData;
    public void begin_end_game(object args, int? id, DequeueCallback callback) {
        Time.timeScale = 1f;
        if(ScenarioGameManagment.scenarioInstance == null) {
            PlayMangement.instance.player.GetComponent<IngameTimer>().EndTimer();
            PlayMangement.instance.enemyPlayer.GetComponent<IngameTimer>().EndTimer();
        }
        JObject jobject = (JObject)args;
        result = JsonConvert.DeserializeObject<ResultFormat>(jobject.ToString());
        leagueData.leagueInfo = result.leagueInfo;

        if (reconnectModal != null) {
            Destroy(GetComponent<ReconnectController>());
            Destroy(reconnectModal);
        }

        battleGameFinish = true;
        AccountManager.Instance.RequestUserInfo();

        if (ScenarioGameManagment.scenarioInstance != null) {
            string _result = result.result;

            PlayMangement playMangement = PlayMangement.instance;
            GameResultManager resultManager = playMangement.resultManager;
            resultManager.gameObject.SetActive(true);
            if(isSurrender) return;
            if (_result == "win") {
                resultManager.SetResultWindow("win", playMangement.player.isHuman);
            }
            else {
                resultManager.SetResultWindow("lose", playMangement.player.isHuman);
            }
        }

        //상대방이 재접속에 최종 실패하여 게임이 종료된 경우
        if (isOpponentPlayerDisconnected) {
            string _result = result.result;

            PlayMangement playMangement = PlayMangement.instance;
            GameResultManager resultManager = playMangement.resultManager;
            resultManager.gameObject.SetActive(true);

            if (_result == "win") {
                resultManager.SetResultWindow("win", playMangement.player.isHuman);
            }
            else {
                resultManager.SetResultWindow("lose", playMangement.player.isHuman);
            }
        }

        callback();
    }

    public void end_end_game(object args, int? id, DequeueCallback callback) {
        PlayMangement playMangement = PlayMangement.instance;
        GameResultManager resultManager = playMangement.resultManager;

        string battleType = PlayerPrefs.GetString("SelectedBattleType");
        if (battleType == "solo") {
            FBL_SceneManager.Instance.LoadScene(FBL_SceneManager.Scene.MAIN_SCENE);
        }
        callback();
    }

    public void ping(object args, int? id, DequeueCallback callback) {
        SendMethod("pong");
        callback();
    }

    public void card_played(object args, int? id, DequeueCallback callback) {
        string enemyCamp = PlayMangement.instance.enemyPlayer.isHuman ? "human" : "orc";
        string cardCamp = gameState.lastUse.cardItem.camp;
        bool isEnemyCard = cardCamp.CompareTo(enemyCamp) == 0;
        if(isEnemyCard) StartCoroutine(PlayMangement.instance.EnemyUseCard(gameState.lastUse, callback));
        else callback();
    }

    public void hero_card_kept(object args, int? id, DequeueCallback callback) {callback();}

    //public void reconnect_game() { }

    public void begin_reconnect_ready(object args, int? id, DequeueCallback callback) {
        if(gameState != null) SendMethod("reconnect_ready");
        callback();
    }

    public void reconnect_fail(object args, int? id, DequeueCallback callback) {
        Time.timeScale = 1f;
        PlayerPrefs.DeleteKey("ReconnectData");
        if (reconnectModal != null) Destroy(reconnectModal);
        PlayMangement.instance.resultManager.SocketErrorUIOpen(false);
        callback();
     }

    public void reconnect_success(object args, int? id, DequeueCallback callback) {
        reconnectCount = 0;
        callback();
    }

    /// <summary>
    /// 양쪽 모두 reconnect가 되었을 때
    /// </summary>
    /// <param name="args"></param>
    public void end_reconnect_ready(object args, int? id, DequeueCallback callback) {
        Time.timeScale = 1f;

        if (reconnectModal != null) Destroy(reconnectModal);
        isOpponentPlayerDisconnected = false;
        callback();
     }

    /// <summary>
    /// 상대방의 재접속을 대기 (상대가 튕김)
    /// </summary>
    /// <param name="args"></param>
    public void wait_reconnect(object args, int? id, DequeueCallback callback) {
        Time.timeScale = 0f;

        reconnectModal = Instantiate(Modal.instantiateReconnectModal());
        isOpponentPlayerDisconnected = true;
        callback();
    }

    public void begin_resend_battle_message(object args, int? id, DequeueCallback callback) {callback();}

    public void end_resend_battle_message(object args, int? id, DequeueCallback callback) {callback();}

    public void x2_reward(object args, int? id, DequeueCallback callback) {
        var json = (JObject)args;
        PlayMangement playMangement = PlayMangement.instance;
        GameResultManager resultManager = playMangement.resultManager;
        resultManager.ExtraRewardReceived(json);
        callback();
    }
}

public class BattleStack {    
    int humanCamp = 0;
    int orcCamp = 0;

    public bool BattleCamp(string camp) {
        if (camp == "human")
            humanCamp++;
        else
            orcCamp++;

        return SecondAttack(camp);
    }

    public bool CheckEndSecond() {
        if (humanCamp > 1 && orcCamp > 1) {
            Reset();
            return true;
        }
        else
            return false;
    }


    private bool SecondAttack(string camp) {
        if (camp == "human" && humanCamp > 1)
            return true;

        if (camp == "orc" && orcCamp > 1)
            return true;

        return false;
    }

    private void Reset() {
        humanCamp = 0;
        orcCamp = 0;
    }
}

public class ShieldStack {
    Queue<int> human = new Queue<int>();
    Queue<int> orc = new Queue<int>();

    public void ResetShield() {
        human.Clear();
        orc.Clear();
    }

    public int GetShieldAmount(bool isHuman) {
        if (isHuman == true)
            return human.Dequeue();
        else
            return orc.Dequeue();
    }
    
    public void SavingShieldGauge(string camp, int amount) {
        if (camp == "human")
            human.Enqueue(amount);
        else
            orc.Enqueue(amount);
    }

    public Queue<int> HitPerShield(string camp) {
        if (camp == "human") {
            return human;
        }
        else
            return orc;
    }
}