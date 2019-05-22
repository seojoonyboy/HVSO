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
    private string url = "ws://192.168.1.23/game";
    WebSocket webSocket;
    [SerializeField] Text message;
    [SerializeField] GameObject machine;

    public UnityEvent OnReceiveSocketMessage;
    public UnityEvent OnSocketClose;

    void Awake() {
        DontDestroyOnLoad(gameObject);
    }

    //void GetRoom() {
    //    HTTPRequest request = new HTTPRequest(new Uri(url), OnRequestFinished);
    //    request.Send();
    //}

    //void OnRequestFinished(HTTPRequest request, HTTPResponse response) {
    //    if (response.IsSuccess) OpenSocket();
    //}

    public void OpenSocket() {
        string url = string.Format("{0}", this.url);
        webSocket = new WebSocket(new Uri(url));
        webSocket.OnOpen += OnOpen;
        webSocket.OnMessage += ReceiveMessage;
        webSocket.Open();

        message.text = "대전상대를 찾는중...";
    }

    //Connected
    void OnOpen(WebSocket webSocket) {
        SendFormat format = new SendFormat();
        format.method = "join_game";
        string playerId = AccountManager.Instance.DEVICEID;
        string deckId = "deck1001";
        format.args = new string[] {"solo", playerId, "basic", deckId };

        string json = JsonUtility.ToJson(format);
        this.webSocket.Send(json);
    }

    public void SendToSocket(SendFormat data) {
        Debug.Log("Sending...");
        string json = JsonUtility.ToJson(data);
        webSocket.Send(json);
    }

    //Receive Socket Message
    void ReceiveMessage(WebSocket webSocket, string message) {
        OnReceiveSocketMessage.Invoke();

        ReceiveFormat result = JsonReader.Read<ReceiveFormat>(message);
        if(result.method == "begin_ready") {
            this.message.text = "대전 상대를 찾았습니다.";
            CustomEvent.Trigger(machine, "PlayStartBattleAnim");
            return;
        }
        Debug.Log(message);
        //if (message.CompareTo("closing") == 0) {
        //    OnSocketClose.Invoke();
        //    webSocket.Close();
        //}
    }

    void Error(WebSocket webSocket, Exception ex) {
        Debug.Log(ex);
    }

    void OnDisable() {
        //webSocket.Close();
    }

    public void StartBattle() {
        SceneManager.Instance.LoadScene(SceneManager.Scene.MISSION_INGAME);
    }
}
