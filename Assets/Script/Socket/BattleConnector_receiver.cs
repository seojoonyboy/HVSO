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
    private bool dequeueing = true;
    public Queue<ReceiveFormat> queue = new Queue<ReceiveFormat>();
    private Type thisType;
    public ResultFormat result = null;

    private void ReceiveMessage(WebSocket webSocket, string message) {
        ReceiveFormat result = dataModules.JsonReader.Read<ReceiveFormat>(message);
        queue.Enqueue(result);
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

    private void FixedUpdate() {
        if(!dequeueing) return;
        if(queue.Count == 0) return;
        DequeueSocket();
    }
    
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
        ClientReady();
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
    }

    public void start_state(object args) {
        PlayMangement.instance.EditorTestInit(gameState);
    }

    public void begin_mulligan(object args) { }

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
        //dequeueing = false;
        //getNewCard = true;
    }

    public void begin_turn_start(object args) { }
    
    public void end_turn_start(object args) { }

    public void begin_orc_pre_turn(object args) {
        checkMyTurn(false);
    }

    public void end_orc_pre_turn(object args) {
        useCardList.isDone = true;
    }

    public void begin_human_turn(object args) {
        checkMyTurn(true);
    }

    public void end_human_turn(object args) {
        useCardList.isDone = true;
    }

    public void begin_orc_post_turn(object args) {
        checkMyTurn(false);
        unitSkillList.isDone = false;
    }

    public void checkMapPos(object args) {
        if(PlayMangement.instance.player.isHuman) return;
    }

    public void end_orc_post_turn(object args) {
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

    public void begin_shield_turn(object args) {
        PlayMangement.instance.LockTurnOver();
    }

    public void end_shield_turn(object args) {
        PlayMangement.instance.heroShieldDone.Add(true);
    }

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
        shieldChargeQueue.Enqueue(charge);
    }

    public void begin_end_turn(object args) {
        dequeueing = false;
        getNewCard = true;
    }

    public void end_end_turn(object args) { }

    public void opponent_connection_closed(object args) {
        PlayMangement.instance.resultManager.SocketErrorUIOpen(true);
    }

    public void begin_end_game(object args) {
        JObject jobject = (JObject)args;
        result = JsonConvert.DeserializeObject<ResultFormat>(jobject.ToString());
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
}