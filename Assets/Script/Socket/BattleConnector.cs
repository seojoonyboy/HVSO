using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BestHTTP.WebSocket;
using BestHTTP;
using System;
using UnityEngine.UI;
using Bolt;
using UnityEngine.Events;

public class BattleConnector : MonoBehaviour {
    public string url = "";
    WebSocket webSocket;
    [SerializeField] Text message;
    [SerializeField] GameObject machine;

    public UnityEvent OnReceiveSocketMessage;
    public UnityEvent OnSocketClose;

    void Awake() {
        DontDestroyOnLoad(gameObject);
    }

    void GetRoom() {
        HTTPRequest request = new HTTPRequest(new Uri(url), OnRequestFinished);
        request.Send();
    }

    void OnRequestFinished(HTTPRequest request, HTTPResponse response) {
        if (response.IsSuccess) OpenSocket(response.DataAsText);
    }

    void OpenSocket(string room) {
        string url = string.Format("{0}", this.url);
        webSocket = new WebSocket(new Uri(url));
        webSocket.OnOpen += OnOpen;
        webSocket.OnMessage += ReceiveMessage;
        webSocket.Open();
    }

    //Connected
    void OnOpen(WebSocket webSocket) {
        this.webSocket.Send("{\"join_game\"}");
    }

    //Receive Socket Message
    void ReceiveMessage(WebSocket webSocket, string message) {
        OnReceiveSocketMessage.Invoke();

        Debug.Log(message);
        if (message.CompareTo("closing") == 0) {
            OnSocketClose.Invoke();
            webSocket.Close();
        }
    }

    void Error(WebSocket webSocket, Exception ex) {
        Debug.Log(ex);
    }

    void OnDisable() {
        //webSocket.Close();
    }

    /// <summary>
    /// 테스트 코드
    /// </summary>
    /// <returns></returns>
    private IEnumerator Start() {
        yield return StartCoroutine(DummyOpenSocket());
    }

    public IEnumerator DummyOpenSocket() {
        yield return new WaitForSeconds(3.0f);
        message.text = "대전상대를 찾는중...";
        yield return new WaitForSeconds(3.0f);
        StartCoroutine(DummyOnOpen());
    }

    IEnumerator DummyOnOpen() {
        message.text = "대전 상대를 찾았습니다.";
        yield return new WaitForSeconds(3.0f);
        CustomEvent.Trigger(machine, "PlayStartBattleAnim");
    }

    public void StartBattle() {
        SceneManager.Instance.LoadScene(SceneManager.Scene.MISSION_INGAME);
    }
}
