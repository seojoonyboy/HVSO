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

/// 서버로부터 데이터를 받아올 때 reflection으로 string을 함수로 바로 발동하게 하는 부분
public partial class BattleConnector : MonoBehaviour {
    public GameState gameState;
    private string raceName;
    public Queue<ReceiveFormat> queue = new Queue<ReceiveFormat>();
    private Type thisType;
    public ResultFormat result = null;
    public bool isOpponentPlayerDisconnected = false;

    private void ReceiveMessage(WebSocket webSocket, string message) {
        ReceiveFormat result = dataModules.JsonReader.Read<ReceiveFormat>(message);
        queue.Enqueue(result);
        DequeueSocket();
        StartCoroutine("showMessage",result);
    }

    private IEnumerator showMessage(ReceiveFormat result) {
        yield return null;
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
    
    private void DequeueSocket() {
        ReceiveFormat result = queue.Dequeue();
        if(result.gameState != null) gameState = result.gameState;
        if(result.error != null) Logger.LogError("WebSocket play wrong Error : " +result.error);
        
        if(result.method == null) return;
        MethodInfo theMethod = thisType.GetMethod(result.method);
        if(theMethod == null) return;
        
        object[] args = new object[]{result.args};
        theMethod.Invoke(this, args);
    }

    public void begin_ready(object args) {
        this.message.text = "대전 상대를 찾았습니다.";
        FindObjectOfType<BattleConnectSceneAnimController>().PlayStartBattleAnim();

        StopCoroutine(timeCheck);
        SetUserInfoText();
        SetSaveGameId();
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

        Logger.Log(orcPlayerNickName);
        Logger.Log(humanPlayerNickName);

        Text enemyNickNameTxt = machine.transform.Find("EnemyName/NickName").GetComponent<Text>();
        Text enemyHeroNameTxt = machine.transform.Find("EnemyName/HeroName").GetComponent<Text>();

        Text playerNickNameTxt = machine.transform.Find("PlayerName/NickName").GetComponent<Text>();
        Text playerHeroNameTxt = machine.transform.Find("PlayerName/HeroName").GetComponent<Text>();

        Logger.Log(race);

        if (race == "human") {
            playerHeroNameTxt.text = humanHeroName;
            playerNickNameTxt.text = humanPlayerNickName;

            enemyHeroNameTxt.text = orcHeroName;
            enemyNickNameTxt.text = orcPlayerNickName;
        }
        else if (race == "orc") {
            playerHeroNameTxt.text = orcHeroName;
            playerNickNameTxt.text = orcPlayerNickName;

            enemyHeroNameTxt.text = humanHeroName;
            enemyNickNameTxt.text = humanPlayerNickName;
        }
        timer.text = null;
        returnButton.onClick.RemoveListener(BattleCancel);
        returnButton.gameObject.SetActive(false);
    }

    public void end_ready(object args) { 
        bool isTest = PlayerPrefs.GetString("SelectedBattleType").CompareTo("test") == 0;
        if(isTest) {
            object value = JsonUtility.FromJson(PlayerPrefs.GetString("Editor_startState"), typeof(StartState));
            SendStartState(value);
        }
        PlayMangement.instance.GetComponent<TurnMachine>().onPrepareTurn.Invoke();
        Logger.Log("준비 턴");
    }

    public void start_state(object args) {
        PlayMangement.instance.EditorTestInit(gameState);
    }

    public void begin_mulligan(object args) {
        PlayMangement.instance.player.GetComponent<IngameTimer>().BeginTimer(30);
    }

    public void hand_changed(object args) {
        if(PlayMangement.instance == null) return;
        var json = (JObject)args;
        bool isHuman = PlayMangement.instance.player.isHuman;
        raceName = isHuman ? "human" : "orc";
        
        if(json["camp"].ToString().CompareTo(raceName) != 0) return;

        Card newCard = gameState.players.myPlayer(isHuman).newCard;
        //Logger.Log("Card id : "+ newCard.id + "  Card itemId : " + newCard.itemId);
        HandchangeCallback(newCard.id, newCard.itemId, false);
        HandchangeCallback = null;
    }

    public void end_mulligan(object args) {
        CardHandManager cardHandManager= PlayMangement.instance.cardHandManager;
        if(!cardHandManager.socketDone)
            cardHandManager.FirstDrawCardChange();
        //dequeueing = false;
        //getNewCard = true;
    }

    public void begin_turn_start(object args) {
        PlayMangement.instance.SyncPlayerHp();
    }
    
    public void end_turn_start(object args) { }

    public void begin_orc_pre_turn(object args) {
        PlayerController player;
        player = PlayMangement.instance.player.isHuman ? PlayMangement.instance.enemyPlayer : PlayMangement.instance.player;
        player.GetComponent<IngameTimer>().BeginTimer();
        checkMyTurn(false);
    }

    public void end_orc_pre_turn(object args) {
        PlayerController player;
        player = PlayMangement.instance.player.isHuman ? PlayMangement.instance.enemyPlayer : PlayMangement.instance.player;
        player.GetComponent<IngameTimer>().EndTimer();
        if(!PlayMangement.instance.player.isHuman)
            PlayMangement.instance.EventHandler.PostNotification(IngameEventHandler.EVENT_TYPE.END_TURN_BTN_CLICKED, this, TurnType.ORC);
        useCardList.isDone = true;
    }

    public void begin_human_turn(object args) {
        PlayerController player;
        player = PlayMangement.instance.player.isHuman ? PlayMangement.instance.player : PlayMangement.instance.enemyPlayer;
        player.GetComponent<IngameTimer>().BeginTimer();
        checkMyTurn(true);
    }

    public void end_human_turn(object args) {
        PlayerController player;
        player = PlayMangement.instance.player.isHuman ? PlayMangement.instance.player : PlayMangement.instance.enemyPlayer;
        player.GetComponent<IngameTimer>().EndTimer();
        if(PlayMangement.instance.player.isHuman)
            PlayMangement.instance.EventHandler.PostNotification(IngameEventHandler.EVENT_TYPE.END_TURN_BTN_CLICKED, this, TurnType.HUMAN);
        useCardList.isDone = true;
    }

    public void begin_orc_post_turn(object args) {
        PlayerController player;
        player = PlayMangement.instance.player.isHuman ? PlayMangement.instance.enemyPlayer : PlayMangement.instance.player;
        player.GetComponent<IngameTimer>().BeginTimer();
        checkMyTurn(false);
        unitSkillList.isDone = false;
    }

    public void end_orc_post_turn(object args) {
        PlayerController player;
        player = PlayMangement.instance.player.isHuman ? PlayMangement.instance.enemyPlayer : PlayMangement.instance.player;
        player.GetComponent<IngameTimer>().EndTimer();
        if(!PlayMangement.instance.player.isHuman)
            PlayMangement.instance.EventHandler.PostNotification(IngameEventHandler.EVENT_TYPE.END_TURN_BTN_CLICKED, this, TurnType.SECRET);
        useCardList.isDone = true;
        unitSkillList.isDone = true;
    }

    public void skill_activated(object args) {
        if(PlayMangement.instance.enemyPlayer.isHuman) return;
        var json = (JObject)args;
        int itemId = int.Parse(json["itemId"].ToString());
        unitSkillList.Enqueue(itemId);
    }

    public void begin_battle_turn(object args) {
        lineBattleList.isDone = false;
        mapClearList.isDone = false;
    }

    public void end_battle_turn(object args) { }

    public void line_battle(object args) {
        var json = (JObject)args;
        string line = json["lineNumber"].ToString();
        string camp = json["camp"].ToString();
        int line_num = int.Parse(line);
        lineBattleList.Enqueue(gameState);
        lineBattleList.checkCount();
        humanData.Enqueue(gameState.players.human);
        orcData.Enqueue(gameState.players.orc);
    }

    public void map_clear(object args) {
        var json = (JObject)args;
        string line = json["lineNumber"].ToString();
        int line_num = int.Parse(line);
        mapClearList.Enqueue(gameState);
        mapClearList.checkCount();
    }

    public void begin_shield_turn(object args) { }

    public void end_shield_turn(object args) { }

    public void surrender(object args) {
        var json = (JObject)args;
        string camp = json["camp"].ToString();
        Logger.Log(camp + "측 항복");

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

    public void shield_gauge(object args) {
        var json = (JObject)args;
        string camp = json["camp"].ToString();
        string gauge = json["shieldGet"].ToString();
        ShieldCharge charge = new ShieldCharge();
        charge.shieldCount = int.Parse(gauge);
        charge.camp = camp;
        if(charge.shieldCount == 0) return;
        shieldChargeQueue.Enqueue(charge);
    }

    public void begin_end_turn(object args) {
        getNewCard = true;
    }

    public void end_end_turn(object args) { }

    public void opponent_connection_closed(object args) {
        PlayMangement.instance.resultManager.SocketErrorUIOpen(true);
    }

    public void begin_end_game(object args) {
        Time.timeScale = 1f;
        JObject jobject = (JObject)args;
        result = JsonConvert.DeserializeObject<ResultFormat>(jobject.ToString());

        if (reconnectModal != null) {
            Destroy(GetComponent<ReconnectController>());
            Destroy(reconnectModal);
        }
     }

    public void end_end_game(object args) {
        battleGameFinish = true;
        AccountManager.Instance.RequestUserInfo((req, res) => {
            if (res != null) {
                if (res.IsSuccess) {
                    AccountManager.Instance.SetSignInData(res);
                }
            }
        });

        //상대방이 재접속에 최종 실패하여 게임이 종료된 경우
        if (isOpponentPlayerDisconnected) {
            string _result = result.result;

            PlayMangement playMangement = PlayMangement.instance;
            GameResultManager resultManager = playMangement.resultManager;
            resultManager.gameObject.SetActive(true);

            if(_result == "win") {
                resultManager.SetResultWindow("win", playMangement.player.isHuman);
            }
            else {
                resultManager.SetResultWindow("lose", playMangement.player.isHuman);
            }
        }
    }

    public void ping(object args) {
        SendMethod("pong");
    }

    public void card_played(object args) {
        string enemyCamp = PlayMangement.instance.enemyPlayer.isHuman ? "human" : "orc";
        string cardCamp = gameState.lastUse.cardItem.camp;
        bool isEnemyCard = cardCamp.CompareTo(enemyCamp) == 0;
        if(isEnemyCard) useCardList.Enqueue(gameState);
    }

    public void hero_card_kept(object args) { }

    //public void reconnect_game() { }

    public void begin_reconnect_ready(object args) {
        if(gameState != null) SendMethod("reconnect_ready");
    }

    public void reconnect_fail(object args) {
        Time.timeScale = 1f;
        PlayerPrefs.DeleteKey("ReconnectData");
        if (reconnectModal != null) Destroy(reconnectModal);
        PlayMangement.instance.resultManager.SocketErrorUIOpen(false);
     }

    public void reconnect_success(object args) {
        reconnectCount = 0;
    }

    /// <summary>
    /// 양쪽 모두 reconnect가 되었을 때
    /// </summary>
    /// <param name="args"></param>
    public void end_reconnect_ready(object args) {
        Time.timeScale = 1f;

        if (reconnectModal != null) Destroy(reconnectModal);
        isOpponentPlayerDisconnected = false;
     }

    /// <summary>
    /// 상대방의 재접속을 대기 (상대가 튕김)
    /// </summary>
    /// <param name="args"></param>
    public void wait_reconnect(object args) {
        Time.timeScale = 0f;

        reconnectModal = Instantiate(Modal.instantiateReconnectModal());
        isOpponentPlayerDisconnected = true;
    }
}

public partial class BattleConnector : MonoBehaviour {
    GameObject reconnectModal;
}