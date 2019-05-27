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

public class BattleConnector : MonoBehaviour {
    private string url = "ws://ccdevclient.fbl.kr/game";
    WebSocket webSocket;
    [SerializeField] Text message;
    [SerializeField] GameObject machine;

    public UnityEvent OnReceiveSocketMessage;
    public UnityEvent OnSocketClose;
    public GameState gameState;

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
        Debug.Log("Client : " + message);
        ReceiveFormat result = JsonReader.Read<ReceiveFormat>(message);
        if(result.gameState != null) gameState = result.gameState;
        if(result.method == "begin_ready") {
            this.message.text = "대전 상대를 찾았습니다.";
            CustomEvent.Trigger(machine, "PlayStartBattleAnim");
            SendMethod("client_ready");
        }
        else if(result.method == "begin_mulligan") {
            ChangeCard(gameState.players.human.CardsId[0]);
            //TODO : 카드 리스트 보여줌 (데이터 전송?)
        }
        else if(result.method == "hand_changed") {
            string newCardId = gameState.players.human.newCardId;
            //TODO : 교체된 카드를 알려줌
        }
    }

    public void ChangeCard(string itemId) {
        string[] args = new string[]{itemId};
        SendMethod("hand_change", args);
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
