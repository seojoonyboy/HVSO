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

public class BattleConnector : MonoBehaviour {
    private string url = "ws://ccdevclient.fbl.kr/game";
    WebSocket webSocket;
    [SerializeField] Text message;
    [SerializeField] GameObject machine;

    public UnityEvent OnReceiveSocketMessage;
    public UnityEvent OnSocketClose;
    public UnityAction<string, int, bool> HandchangeCallback;
    public GameState gameState;
    private string raceName;
    private bool dequeueing = true;
    public Queue<ReceiveFormat> queue = new Queue<ReceiveFormat>();

    void Awake() {
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

    private void FixedUpdate() {
        if(!dequeueing) return;
        if(queue.Count == 0) return;
        DequeueSocket();
    }
    
    private void DequeueSocket() {
        ReceiveFormat result = queue.Dequeue();
        if(result.gameState != null) gameState = result.gameState;
        
        switch(result.method) {
            case "begin_ready" : BeginReady(); break;
            case "end_ready" : break;
            case "begin_mulligan" : break;
            case "hand_changed" : HandChanged(result.args[0]); break;
            case "end_mulligan" : break;
            case "begin_turn_start" : break;
            case "end_turn_start" : break;
            case "begin_orc_pre_turn" : break;
            case "begin_human_turn" : break;
            case "begin_orc_post_turn" : break;
            case "begin_battle_turn" : break;
            case "end_battle_turn" : break;
            case "line_battle" : break;
            case "map_clear" : break;
            case "opponent_connection_closed" : break;
            case "begin_end_game" : break;
            case "end_end_game" : break;
            default :
            Debug.Log(result.gameState + " method is not set");
            break;
        }
    }
    private void BeginReady() {
        this.message.text = "대전 상대를 찾았습니다.";
        CustomEvent.Trigger(machine, "PlayStartBattleAnim");
        SendMethod("client_ready");
    }

    public void ChangeCard(int itemId) {
        string[] args = new string[]{itemId.ToString()};
        SendMethod("hand_change", args);
    }

    public void HandChanged(string argsName) {
        if(PlayMangement.instance == null) return; //임시
        bool isOrc = PlayMangement.instance.player.race;
        raceName = isOrc ? "orc" : "human";
        if(argsName.CompareTo(raceName) == 0) return;
        Card newCard = gameState.players.human.newCard;
        Debug.Log("Card id : "+ newCard.id + "  Card itemId : " + newCard.itemId);
        HandchangeCallback(newCard.id, newCard.itemId, false);
        HandchangeCallback = null;
    }

    public void MulliganEnd() {
        SendMethod("end_mulligan");
    }

    public void TurnOver() {
        SendMethod("turn_over");
    }

    void Error(WebSocket webSocket, Exception ex) {
        Debug.Log(ex);
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
}
