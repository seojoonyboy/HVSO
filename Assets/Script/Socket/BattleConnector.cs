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
        string deckId = "deck1001";
        string[] args = new string[] {"solo", playerId, "basic", deckId };
        SendMethod("join_game", args);
    }

    //Receive Socket Message
    private void ReceiveMessage(WebSocket webSocket, string message) {
        OnReceiveSocketMessage.Invoke();
        ReceiveFormat result = JsonReader.Read<ReceiveFormat>(message);
        queue.Enqueue(result);
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

    void Error(WebSocket webSocket, Exception ex) {
        Debug.LogWarning(ex);
    }

    void OnDisable() {
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

/// <summary>
/// 서버로부터 데이터를 받아올 때 reflection으로 string을 함수로 바로 발동하게 하는 부분
/// </summary>
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
            Debug.Log("WebSocket gameState changed");
        }
        if(result.method == null) return;
        MethodInfo theMethod = thisType.GetMethod(result.method);
        if(theMethod == null) return;
        theMethod.Invoke(this, result.args);
    }

    public void begin_ready() {
        this.message.text = "대전 상대를 찾았습니다.";
        CustomEvent.Trigger(machine, "PlayStartBattleAnim");
        SendMethod("client_ready");
    }

    public void begin_mulligan() {
        Debug.Log("WebSocket State : begin_mulligan");
    }

    public void hand_changed(string arg) {
        if(PlayMangement.instance == null) return; //임시

        bool isHuman = PlayMangement.instance.player.race;
        raceName = isHuman ? "human" : "orc";

        if(arg.CompareTo(raceName) != 0) return;

        Card newCard = gameState.players.human.newCard;
        Debug.Log("Card id : "+ newCard.id + "  Card itemId : " + newCard.itemId);
        HandchangeCallback(newCard.id, newCard.itemId, false);
        HandchangeCallback = null;
    }

    public void end_mulligan() {
        Debug.Log("WebSocket State : end_mulligan");
        //TODO : 이 때 새로운 카드가 들어옵니다.
        dequeueing = false;
        getNewCard = true;
        Card humanHeroNewCard = gameState.players.human.newCard;
        Card orcHeroNewCard = gameState.players.orc.newCard;
    }

    public void begin_turn_start() {
        Debug.Log("WebSocket State : begin_turn_start");
    }
    
    public void end_turn_start() {
        Debug.Log("WebSocket State : end_turn_start");
    }

    public void begin_orc_pre_turn() {
        Debug.Log("WebSocket State : begin_orc_pre_turn");
    }

    public void begin_human_turn() {
        Debug.Log("WebSocket State : begin_human_turn");
    }

    public void begin_orc_post_turn() {
        Debug.Log("WebSocket State : begin_orc_post_turn");
    }

    public void begin_battle_turn() {
        Debug.Log("WebSocket State : begin_battle_turn");
    }

    public void end_battle_turn() {
        Debug.Log("WebSocket State : end_battle_turn");
    }

    public void line_battle() {
        Debug.Log("WebSocket State : line_battle");
    }

    public void map_clear() {
        Debug.Log("WebSocket State : map_clear");
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

}

/// <summary>
/// 클라이언트로부터 데이터를 가져올 때
/// </summary>
public partial class BattleConnector : MonoBehaviour {
    private string status;
    private bool getNewCard = false;

    public IEnumerator WaitGetCard() {
        while(!getNewCard) {
            yield return new WaitForFixedUpdate();
        }
        getNewCard = false;
        dequeueing = true;
    }

}