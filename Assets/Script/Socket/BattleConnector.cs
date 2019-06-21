using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BestHTTP.WebSocket;
using BestHTTP;
using System;
using UnityEngine.UI;
using Bolt;
using UnityEngine.Events;
using dataModules;
using SocketFormat;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Linq;

public partial class BattleConnector : MonoBehaviour {

    #if UNITY_EDITOR
    private string url = "wss://ccdevclient.fbl.kr/game";
    #else
    private string url = "wss://cctest.fbl.kr/game";
    #endif
    WebSocket webSocket;
    [SerializeField] Text message;
    [SerializeField] GameObject machine;

    public UnityEvent OnReceiveSocketMessage;
    public UnityEvent OnSocketClose;
    public UnityAction<string, int, bool> HandchangeCallback;
    private Coroutine pingpong;
    private bool battleGameFinish = false;

    void Awake() {
        thisType = this.GetType();
        DontDestroyOnLoad(gameObject);
    }

    public void OpenSocket() {
        string url = string.Format("{0}", this.url);
        webSocket = new WebSocket(new Uri(url));
        webSocket.OnOpen += OnOpen;
        webSocket.OnMessage += ReceiveMessage;
        webSocket.OnClosed += OnClosed;
        webSocket.Open();

        message.text = "대전상대를 찾는중...";
    }

    public void OnClosed(WebSocket webSocket, ushort code, string msg) {
        Logger.LogWarning("Socket has been closed : " + code + "  message : " + msg);
    }

    //Connected
    private void OnOpen(WebSocket webSocket) {
        string playerId = AccountManager.Instance.DEVICEID;
        string deckId = Variables.Saved.Get("SelectedRace").ToString().CompareTo("HUMAN") == 0 ? "deck1001" : "deck1002";
        string[] args = new string[] {"solo", playerId, "basic", deckId };
        SendMethod("join_game", args);
        pingpong = StartCoroutine("Heartbeat");
    }

    IEnumerator Heartbeat() {
        WaitForSeconds beatTime = new WaitForSeconds(10f);
        while(webSocket.IsOpen) {
            yield return beatTime;
        }
        if(!battleGameFinish)
            PlayMangement.instance.SocketErrorUIOpen();
    }

    //Receive Socket Message
    private void ReceiveMessage(WebSocket webSocket, string message) {
        Logger.Log(message);
        OnReceiveSocketMessage.Invoke();
        ReceiveFormat result = dataModules.JsonReader.Read<ReceiveFormat>(message);
        queue.Enqueue(result);
    }

    public void ClientReady() {
        SendMethod("client_ready");
    }

    private class ItemIdClass {
        public ItemIdClass(int itemId) { this.itemId = itemId.ToString(); }
        public string itemId;
    }

    public void ChangeCard(int itemId) {
        ItemIdClass argClass = new ItemIdClass(itemId);
        //argClass = null; //카드 무제한 변경 코드
        SendMethod("hand_change", argClass);
    }

    public void MulliganEnd() {
        SendMethod("end_mulligan");
    }

    public void TurnOver() {
        SendMethod("turn_over");
    }

    public void UseCard(object args, UnityAction callback = null) {
        SendMethod("play_card", args);
    }

    public void UnitSkillActivate(object args) {
        SendMethod("unit_skill_activate", args);
    }

    void Error(WebSocket webSocket, Exception ex) {
        Logger.LogWarning(ex);
    }

    void OnDisable() {
        if(pingpong != null) StopCoroutine(pingpong);
        if(webSocket != null) webSocket.Close();
    }

    public void StartBattle() {
        SceneManager.Instance.LoadScene(SceneManager.Scene.MISSION_INGAME);
    }

    private void SendMethod(string method, object args = null) {
        if(args == null) args = new string[]{};
        SendFormat format = new SendFormat(method, args);
        string json = JsonConvert.SerializeObject(format);
        webSocket.Send(json);
    }

    private string[] ConvertObjectArrayToStringArray(object[] objs) {
        string[] result = Array.ConvertAll<object, string>(objs, ConvertObjectToString);
        return result;
    }

    private string ConvertObjectToString(object obj) {
        return obj?.ToString() ?? string.Empty;
    }
}

/// 서버로부터 데이터를 받아올 때 reflection으로 string을 함수로 바로 발동하게 하는 부분
public partial class BattleConnector : MonoBehaviour {
    public GameState gameState;
    private string raceName;
    private bool dequeueing = true;
    public Queue<ReceiveFormat> queue = new Queue<ReceiveFormat>();
    private Type thisType;

    private void FixedUpdate() {
        if(!dequeueing) return;
        if(queue.Count == 0) return;
        DequeueSocket();
    }
    
    private void DequeueSocket() {
        ReceiveFormat result = queue.Dequeue();
        status = result.method;
        if(result.gameState != null) {
            gameState = result.gameState;
        }
        if(result.error != null) 
            Logger.LogError("WebSocket play wrong Error : " +result.error);
        if(result.method == null) return;
        MethodInfo theMethod = thisType.GetMethod(result.method);
        if(theMethod == null) return;
        object[] args = new object[]{result.args};
        //var args = ((IEnumerable) result.args).Cast<object>().Select(x => x == null ? x : x.ToString()).ToArray();
        theMethod.Invoke(this, args);
    }

    public void begin_ready(object args) {
        this.message.text = "대전 상대를 찾았습니다.";
        CustomEvent.Trigger(machine, "PlayStartBattleAnim");
    }

    public void end_ready(object args) {
        //Logger.Log("WebSocket State : end_ready");
    }

    public void begin_mulligan(object args) {
        //Logger.Log("WebSocket State : begin_mulligan");
    }

    public void hand_changed(object args) {
        if(PlayMangement.instance == null) return; //임시
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
        //Logger.Log("WebSocket State : end_mulligan");
        dequeueing = false;
        getNewCard = true;
    }

    public void begin_turn_start(object args) {
        //Logger.Log("WebSocket State : begin_turn_start");
    }
    
    public void end_turn_start(object args) {
        //Logger.Log("WebSocket State : end_turn_start");
    }

    public void begin_orc_pre_turn(object args) {
        //Logger.Log("WebSocket State : begin_orc_pre_turn");
        checkMyTurn(false);
    }

    public void end_orc_pre_turn(object args) {
        //Logger.Log("WebSocket State : end_orc_pre_turn");
        useCardList.isDone = true;
    }

    public void begin_human_turn(object args) {
        //Logger.Log("WebSocket State : begin_human_turn");
        checkMyTurn(true);
    }

    public void end_human_turn(object args) {
        //Logger.Log("WebSocket State : end_human_turn");
        useCardList.isDone = true;
    }

    public void begin_orc_post_turn(object args) {
        //Logger.Log("WebSocket State : begin_orc_post_turn");
        checkMyTurn(false);
        DebugSocketData.CheckMapPosition(gameState);
    }

    public void checkMapPos(object args) {
        if(PlayMangement.instance.player.isHuman) return;
    }

    public void end_orc_post_turn(object args) {
        //Logger.Log("WebSocket State : end_orc_post_turn");
        useCardList.isDone = true;
    }

    public void skill_activate(object args) {
        //item Id가 args에 있음
        //Logger.Log("WebSocket State : unit_skill_activate");
        //적이 skill_activate 할 경우?
    }

    public void begin_battle_turn(object args) {
        //Logger.Log("WebSocket State : begin_battle_turn");
        lineBattleList.isDone = false;
        mapClearList.isDone = false;
    }

    public void end_battle_turn(object args) {
        //Logger.Log("WebSocket State : end_battle_turn");
    }

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

    public void begin_shild_turn(object args) {
        //Logger.Log("WebSocket State : begin_shild_turn");
        dequeueing = false;
        getNewCard = true;
    }

    public void end_shild_turn(object args) {
        //Logger.Log("WebSocket State : end_shild_turn");
        PlayMangement.instance.heroShieldActive = false;
    }

    public void shild_guage(object args) {
        var json = (JObject)args;
        string camp = json["camp"].ToString();
        string gauge = json["shildGet"].ToString();
        ShieldCharge charge = new ShieldCharge();
        charge.shieldCount = int.Parse(gauge);
        charge.camp = camp;
        shieldChargeQueue.Enqueue(charge);
    }

    public void begin_end_turn(object args) {
        //Logger.Log("WebSocket State : begin_end_turn");
        dequeueing = false;
        getNewCard = true;
    }

    public void end_end_turn(object args) {
        //Logger.Log("WebSocket State : end_end_turn");
    }

    public void opponent_connection_closed(object args) {
        //Logger.Log("WebSocket State : opponent_connection_closed");
    }

    public void begin_end_game(object args) {
        //Logger.Log("WebSocket State : begin_end_game");
        
    }

    public void end_end_game(object args) {
        //Logger.Log("WebSocket State : end_end_game");
        battleGameFinish = true;
    }

    public void ping(object args) {
        //Logger.Log("ping");
        SendMethod("pong");
    }

    public void card_played(object args) {
        Logger.Log("WebSocket State : card_played");
        string enemyCamp = PlayMangement.instance.enemyPlayer.isHuman ? "human" : "orc";
        string cardCamp = gameState.lastUse.cardItem.camp;
        bool isEnemyCard = cardCamp.CompareTo(enemyCamp) == 0;
        if(isEnemyCard) useCardList.Enqueue(gameState);
        //DebugSocketData.CheckMapPosition(gameState);
    }
}

/// 클라이언트로부터 데이터를 가져올 때
public partial class BattleConnector : MonoBehaviour {
    private string status;
    private bool getNewCard = false;
    public QueueSocketList<GameState> useCardList = new QueueSocketList<GameState>();
    public QueueSocketList<GameState> lineBattleList = new QueueSocketList<GameState>();
    public QueueSocketList<GameState> mapClearList = new QueueSocketList<GameState>();
    public Queue<SocketFormat.Player> humanData = new Queue<SocketFormat.Player>();
    public Queue<SocketFormat.Player> orcData = new Queue<SocketFormat.Player>();
    public Queue <ShieldCharge> shieldChargeQueue = new Queue<ShieldCharge>();

    public IEnumerator WaitGetCard() {
        while(!getNewCard) {
            yield return new WaitForFixedUpdate();
        }
        getNewCard = false;
        dequeueing = true;
    }

    public bool cardPlayFinish() {
        return useCardList.allDone;
    }

    public GameState getHistory() {
        return useCardList.Dequeue();
    }

    private void checkMyTurn(bool isHuman) {
        if(PlayMangement.instance.player.isHuman != isHuman)
            useCardList.isDone = false;
    }

    public GameState getStateList(bool isBattleEnd) {
        return isBattleEnd ? mapClearList.Dequeue() : lineBattleList.Dequeue();
    }

    public void DrawNewCards(int drawNum, int itemId) {
        PlayMangement playMangement = PlayMangement.instance;
        bool isHuman = playMangement.player.isHuman;
        int cardNum = gameState.players.myPlayer(isHuman).deck.handCards.Length - 1;
        StartCoroutine(DrawCardIEnumerator(itemId));
    }

    public IEnumerator DrawCardIEnumerator(int itemId) {
        PlayMangement playMangement = PlayMangement.instance;
        bool isHuman = playMangement.player.isHuman;
        yield return new WaitUntil(() =>
            gameState.lastUse != null && gameState.lastUse.cardItem.itemId == itemId && 
            ((gameState.lastUse.cardItem.camp.CompareTo("human")==0) == playMangement.player.isHuman));

        StartCoroutine(PlayMangement.instance.player.cdpm.AddMultipleCard(gameState.players.myPlayer(isHuman).deck.handCards));
    }
}
namespace SocketFormat {
    public class QueueSocketList<T> {
        public bool isDone;
        private Queue<T> queue;
        private int totalCount;

        public QueueSocketList() {
            isDone = true;
            queue = new Queue<T>();
            totalCount = 0;
        }

        public void Enqueue(T value) {
            totalCount++;
            queue.Enqueue(value);
        }

        public T Dequeue() {
            if(queue.Count == 0) return default(T);
            return queue.Dequeue();
        }

        public void checkCount() {
            if(totalCount == 5) isDone = true;
        }
        public int Count { get { return queue.Count; } }

        public bool allDone { get { return isDone && queue.Count == 0; } }

        public IEnumerator WaitNext() {
            while(allDone) {
                yield return new WaitForFixedUpdate();
                if(Count != 0) break;
            }
        }
    }
}