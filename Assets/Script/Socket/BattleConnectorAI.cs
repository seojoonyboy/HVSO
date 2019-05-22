using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BestHTTP.WebSocket;
using System;
using dataModules;
using SocketFormat;

public class BattleConnectorAI : MonoBehaviour {
    private string url = "ws://192.168.1.23/game";
    public string gameUuidId;
    WebSocket webSocket;

    public void OpenSocket() {
        string url = string.Format("{0}", this.url);
        webSocket = new WebSocket(new Uri(url));
        webSocket.OnOpen += OnOpen;
        webSocket.OnMessage += ReceiveMessage;
        webSocket.Open();
    }

    //Connected
    void OnOpen(WebSocket webSocket) {
        SendFormat format = new SendFormat();
        format.method = "join_game";
        string playerId = AccountManager.Instance.DEVICEID;
        string deckId = "deck1002";
        format.args = new string[] {"solo", playerId, "basic", deckId, gameUuidId };

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
        ReceiveFormat result = JsonReader.Read<ReceiveFormat>(message);
        Debug.Log("AI : " + message);
        if(result.method == "begin_ready") {
            SendMethod("client_ready");
        }
        else if(result.method == "mulligan") {
            SendMethod("end_mulligan");
        }
        else if(result.method == "begin_orc_pre_turn") {
            SendMethod("turn_over");
        }
        else if(result.method == "begin_orc_post_turn") {
            SendMethod("turn_over");
        }
        else if(result.method == "begin_battle_turn") {
            SendMethod("endBattleTurn");
        }
    }

    void Error(WebSocket webSocket, Exception ex) {
        Debug.Log(ex);
    }

    void OnDisable() {
        webSocket.Close();
    }

    private void SendMethod(string method) {
        SendFormat format = new SendFormat(method, new string[]{});
        StartCoroutine(timeOutWebSocket(2f, JsonUtility.ToJson(format)));
    }

    IEnumerator timeOutWebSocket(float time, string json) {
        WaitForSeconds wait = new WaitForSeconds(time);
        yield return wait;
        webSocket.Send(json);
    }
}
