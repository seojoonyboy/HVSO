using System;
using BestHTTP;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

/// <summary>
/// BestHTTP Pro
/// </summary>
public partial class NetworkManager : Singleton<NetworkManager> {
#if UNITY_EDITOR
    private string url = "https://ccdevclient.fbl.kr/";
#else
    private string url = "https://cctest.fbl.kr/";
#endif
    public string baseUrl { get { return url; } }
    protected NetworkManager() { }

    private void Awake() {
        DontDestroyOnLoad(gameObject);
        MAX_REDIRECTCOUNT = 10;
    }

    Queue<RequestFormat> requests = new Queue<RequestFormat>();
    private bool dequeueing = false;
    TimeSpan timeout = new TimeSpan(0, 0, 30);  //timeout 30초 지정
    private GameObject loadingModal;
    public int MAX_REDIRECTCOUNT { get; private set; }

    private void FixedUpdate() {
        if (dequeueing) return;
        if (requests.Count == 0) return;
        DequeueRequest();
    }

    /// <summary>
    /// HTTP 요청
    /// </summary>
    /// <param name="request">HTTPRequest에 맞는 Format 작성</param>
    /// <param name="callback">요청 완료시 받을 Callback</param>
    public void Request(HTTPRequest request, OnRequestFinishedDelegate callback, string msg = null) {
        requests.Enqueue(new RequestFormat(request, callback, msg));
    }

    private void DequeueRequest() {
        dequeueing = true;
        loadingModal = LoadingModal.instantiate();
        Text loadingMsg = loadingModal.transform.Find("Panel/AdditionalMessage").GetComponent<Text>();

        RequestFormat selectedRequestFormat = requests.Dequeue();
        HTTPRequest request = selectedRequestFormat.request;
        loadingMsg.text = selectedRequestFormat.loadingMessage;
        request.LoadingMessage = selectedRequestFormat.loadingMessage;

        request.AddHeader("Content-Type", "application/json");
        if(request.RedirectCount != 0) request.Callback = null;
        request.Callback += CheckCondition;
        request.Callback += selectedRequestFormat.callback;
        request.Callback += FinishRequest;
        request.Timeout = timeout;
        request.Send();
    }

    private void CheckCondition(HTTPRequest request, HTTPResponse response) {
        //timeout에 따른 재요청
        if(response == null) {
            FinishRequest(request, response);
            if (request.RedirectCount == MAX_REDIRECTCOUNT) {
                Modal.instantiate("Server가 불안정합니다. 잠시후 다시 접속해주세요.", Modal.Type.CHECK, () => {
                    SceneManager.Instance.LoadScene(SceneManager.Scene.MAIN_SCENE);
                });
                dequeueing = false;
                throw new ArgumentOutOfRangeException("Max Redirect", "Redirect Count 초과");
            }

            HTTPRequest re_request = new HTTPRequest(request.Uri);
            re_request.MethodType = request.MethodType;
            re_request.RedirectCount = ++request.RedirectCount;
            re_request.AddHeader("authorization", AccountManager.Instance.TokenFormat);

            request.Callback -= CheckCondition;
            request.Callback -= FinishRequest;

            Request(re_request, request.Callback, re_request.LoadingMessage);

            dequeueing = false;
            throw new ArgumentOutOfRangeException("TimeOut Request", "요청대기시간이 초과되었습니다.");
        }
        //token이 존재하지 않아 token 갱신 이후 재요청
        else if(response.DataAsText.Contains("invalid_token")) {
            FinishRequest(request, response);
            AccountManager.Instance.AuthUser((a, b) => {
                AccountManager.Instance.AuthUserCallback(a, b);
                HTTPRequest re_request = new HTTPRequest(request.Uri);
                re_request.MethodType = request.MethodType;
                re_request.RedirectCount = ++request.RedirectCount;
                re_request.AddHeader("authorization", AccountManager.Instance.TokenFormat);
                request.Callback -= CheckCondition;
                request.Callback -= FinishRequest;
                Request(re_request, request.Callback, re_request.LoadingMessage); 
            });
            dequeueing = false;
            throw new ArgumentOutOfRangeException("Token Invalid", "토큰이 존재하지 않습니다.");
        }
    }

    private void FinishRequest(HTTPRequest request, HTTPResponse response) {
        dequeueing = false;
        Destroy(loadingModal);
        loadingModal = null;
    }
}

/// <summary>
/// 요청 전용 양식 클래스
/// </summary>
public partial class NetworkManager {
    public class RequestFormat {
        public HTTPRequest request;
        public OnRequestFinishedDelegate callback;
        public string loadingMessage;
        public RequestFormat(HTTPRequest request, OnRequestFinishedDelegate callback, string msg = null) {
            this.request = request;
            this.callback = callback;
            loadingMessage = msg;
        }
    }

    public class AddCustomDeckReqFormat {
        public string name;
        public string heroId;
        public string camp;
        public DeckEditController.DeckItem[] items;
    }

    public class ModifyDeckReqFormat {
        public List<ModifyDeckReqArgs> parms;

        public ModifyDeckReqFormat() {
            parms = new List<ModifyDeckReqArgs>();
        }
    }

    public struct ModifyDeckReqArgs {
        public ModifyDeckReqField fieldName;
        public object value;
    }

    public enum ModifyDeckReqField {
        NAME,
        ITEMS
    }
}