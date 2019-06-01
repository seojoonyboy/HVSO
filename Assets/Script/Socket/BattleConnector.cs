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

public partial class BattleConnector : MonoBehaviour {
    private string url = "ws://ccdevclient.fbl.kr/game";
    WebSocket webSocket;
    [SerializeField] Text message;
    [SerializeField] GameObject machine;

    public UnityEvent OnReceiveSocketMessage;
    public UnityEvent OnSocketClose;
    public UnityAction<string, int, bool> HandchangeCallback;
    public Queue<UnityAction> skillCallbacks = new Queue<UnityAction>();
    private Coroutine pingpong;

    void Awake() {
        thisType = this.GetType();
        DontDestroyOnLoad(gameObject);
    }

    public void OpenSocket() {
        string url = string.Format("{0}", this.url);
        webSocket = new WebSocket(new Uri(url));
        webSocket.OnOpen += OnOpen;
        webSocket.OnMessage += ReceiveMessage;
        webSocket.Open();

        message.text = "대전상대를 찾는중...";
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
        WaitForSeconds beatTime = new WaitForSeconds(25f);
        while(true) {
            yield return beatTime;
            SendMethod("ping");
        }
    }

    //Receive Socket Message
    private void ReceiveMessage(WebSocket webSocket, string message) {
        //Debug.Log(message);
        OnReceiveSocketMessage.Invoke();
        ReceiveFormat result = JsonReader.Read<ReceiveFormat>(message);
        queue.Enqueue(result);
    }

    public void ClientReady() {
        SendMethod("client_ready");
    }

    public void ChangeCard(int itemId) {
        string[] args = new string[]{itemId.ToString()};
        SendMethod("hand_change", args);
    }

    public void MulliganEnd() {
        SendMethod("end_mulligan");
    }

    public void TurnOver() {
        SendMethod("turn_over");
    }

    public void UseCard(string[] args, UnityAction callback = null) {
        SendMethod("play_card", args);
        if(callback != null) skillCallbacks.Enqueue(callback);
    }

    void Error(WebSocket webSocket, Exception ex) {
        Debug.LogWarning(ex);
    }

    void OnDisable() {
        StopCoroutine(pingpong);
        webSocket.Close();
    }

    public void StartBattle() {
        SceneManager.Instance.LoadScene(SceneManager.Scene.MISSION_INGAME);
    }

    private void SendMethod(string method, string[] args = null) {
        if(args == null) args = new string[]{};
        SendFormat format = new SendFormat(method, args);
        webSocket.Send(JsonUtility.ToJson(format));
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
            Debug.LogError("WebSocket play wrong Error : " +result.error);
        if(result.method == null) return;
        MethodInfo theMethod = thisType.GetMethod(result.method);
        if(theMethod == null) return;
        theMethod.Invoke(this, result.args);
    }

    public void begin_ready() {
        this.message.text = "대전 상대를 찾았습니다.";
        CustomEvent.Trigger(machine, "PlayStartBattleAnim");
    }

    public void end_ready() {
        Debug.Log("WebSocket State : end_ready");
    }

    public void begin_mulligan() {
        Debug.Log("WebSocket State : begin_mulligan");
    }

    public void hand_changed(string arg) {
        if(PlayMangement.instance == null) return; //임시

        bool isHuman = PlayMangement.instance.player.isHuman;
        raceName = isHuman ? "human" : "orc";

        if(arg.CompareTo(raceName) != 0) return;

        Card newCard = gameState.players.myPlayer(isHuman).newCard;
        Debug.Log("Card id : "+ newCard.id + "  Card itemId : " + newCard.itemId);
        HandchangeCallback(newCard.id, newCard.itemId, false);
        HandchangeCallback = null;
    }

    public void end_mulligan() {
        Debug.Log("WebSocket State : end_mulligan");
        dequeueing = false;
        getNewCard = true;
    }

    public void begin_turn_start() {
        Debug.Log("WebSocket State : begin_turn_start");
    }
    
    public void end_turn_start() {
        Debug.Log("WebSocket State : end_turn_start");
    }

    public void begin_orc_pre_turn() {
        Debug.Log("WebSocket State : begin_orc_pre_turn");
        checkMyTurn(false);
    }

    public void end_orc_pre_turn() {
        Debug.Log("WebSocket State : end_orc_pre_turn");
        useCardList.isDone = true;
    }

    public void begin_human_turn() {
        Debug.Log("WebSocket State : begin_human_turn");
        checkMyTurn(true);
    }

    public void end_human_turn() {
        Debug.Log("WebSocket State : end_human_turn");
        useCardList.isDone = true;
    }

    public void begin_orc_post_turn() {
        Debug.Log("WebSocket State : begin_orc_post_turn");
        checkMyTurn(false);
    }

    public void end_orc_post_turn() {
        Debug.Log("WebSocket State : end_orc_post_turn");
        useCardList.isDone = true;
    }

    public void begin_battle_turn() {
        Debug.Log("WebSocket State : begin_battle_turn");
        lineBattleList.isDone = false;
        mapClearList.isDone = false;
    }

    public void end_battle_turn() {
        Debug.Log("WebSocket State : end_battle_turn");
    }

    public void line_battle(string line, string camp) {
        int line_num = int.Parse(line);
        lineBattleList.Enqueue(gameState);
        lineBattleList.checkCount();
        humanData.Enqueue(gameState.players.human);
        orcData.Enqueue(gameState.players.orc);
    }

    public void map_clear(string line) {
        int line_num = int.Parse(line);
        mapClearList.Enqueue(gameState);
        mapClearList.checkCount();
    }

    public void begin_shild_turn(string camp, string itemId) {
        Debug.Log("WebSocket State : begin_shild_turn");
        dequeueing = false;
        getNewCard = true;
    }

    public void end_shild_turn() {
        Debug.Log("WebSocket State : end_shild_turn");
    }

    public void begin_end_turn() {
        Debug.Log("WebSocket State : begin_end_turn");
        dequeueing = false;
        getNewCard = true;
    }

    public void end_end_turn() {
        Debug.Log("WebSocket State : end_end_turn");
    }

    public void opponent_connection_closed() {
        Debug.Log("WebSocket State : opponent_connection_closed");
    }

    public void begin_end_game() {
        Debug.Log("WebSocket State : begin_end_game");
        
    }

    public void end_end_game() {
        Debug.Log("WebSocket State : end_end_game");
    }

    public void card_played() {
        Debug.Log("WebSocket State : card_played");
        string enemyCamp = PlayMangement.instance.enemyPlayer.isHuman ? "human" : "orc";
        string cardCamp = gameState.lastUse.cardItem.camp;
        bool isEnemyCard = cardCamp.CompareTo(enemyCamp) == 0;
        if(isEnemyCard) useCardList.Enqueue(gameState.lastUse);
        else if(skillCallbacks.Count != 0) skillCallbacks.Dequeue()();
    }
}

/// 클라이언트로부터 데이터를 가져올 때
public partial class BattleConnector : MonoBehaviour {
    private string status;
    private bool getNewCard = false;
    public QueueSocketList<PlayHistory> useCardList = new QueueSocketList<PlayHistory>();
    public QueueSocketList<GameState> lineBattleList = new QueueSocketList<GameState>();
    public QueueSocketList<GameState> mapClearList = new QueueSocketList<GameState>();
    public Queue<SocketFormat.Player> humanData = new Queue<SocketFormat.Player>();
    public Queue<SocketFormat.Player> orcData = new Queue<SocketFormat.Player>();

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

    public PlayHistory getHistory() {
        return useCardList.Dequeue();
    }

    private void checkMyTurn(bool isHuman) {
        if(PlayMangement.instance.player.isHuman != isHuman)
            useCardList.isDone = false;
    }

    public GameState getStateList(bool isBattleEnd) {
        return isBattleEnd ? mapClearList.Dequeue() : lineBattleList.Dequeue();
    }

    public void DrawNewCards(int drawNum) {
        PlayMangement playMangement = PlayMangement.instance;
        bool isHuman = playMangement.player.isHuman;
        SocketFormat.Card[] cards = playMangement.socketHandler.gameState.players.myPlayer(isHuman).deck.handCards;
        StartCoroutine(DrawCardIEnumerator(cards, drawNum));
    }

    IEnumerator DrawCardIEnumerator(SocketFormat.Card[] cards, int count) {
        for(int i = cards.Length - count; i < cards.Length; i++) {
            PlayMangement.instance.player.cdpm.AddCard(null, cards[i]);
            yield return new WaitForSeconds(0.6f);
        }
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