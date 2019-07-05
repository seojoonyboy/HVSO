using System;
using BestHTTP;
using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System.Collections.Generic;
using UnityEngine.UI;

public partial class NetworkManager : Singleton<NetworkManager> {
    #if UNITY_EDITOR
    private string url = "https://ccdevclient.fbl.kr/";
    #else
    private string url = "https://cctest.fbl.kr/";
    #endif
    public string baseUrl {get {return url;}}
    protected NetworkManager() { }
    public delegate void Callback(HttpResponse response);
    public delegate void CallbackRetryOccured(string msg);

    private void Awake() {
        DontDestroyOnLoad(gameObject);
    }

    public void request(string method, string url, string data, Callback callback, bool neeAuthor = true) {
        StartCoroutine(_request(method, url, data, callback, neeAuthor));
    }

    //overload
    IEnumerator _request(string method, string url, string data, Callback callback, bool needAuthor = true) {
        UnityWebRequest _www;
        switch (method) {
            case "POST":
                _www = UnityWebRequest.Post(url, data);
                break;
            case "PUT":
                _www = UnityWebRequest.Put(url, data);
                _www.method = "POST";
                _www.SetRequestHeader("Content-Type", "application/json");
                break;
            case "DELETE":
                _www = UnityWebRequest.Delete(url);
                break;
            case "GET":
            default:
                _www = UnityWebRequest.Get(url);
                break;
        }
        _www.timeout = 10;

        if (needAuthor) {
            //_www.SetRequestHeader("Authorization", "Token " + GameManager.Instance.userStore.userTokenId);
            //_www.downloadHandler = new DownloadHandlerBuffer();
        }

        using (UnityWebRequest www = _www) {
            yield return www.SendWebRequest();
            callback(new HttpResponse(www));
        }
    }
}

public partial class NetworkManager {
    public GameObject subReqPrefab;
    //TODO : Queue(FIFO)의 형태로 Network 요청을 쌓아 처리하도록 수정
    Queue networkQueue = new Queue();

    //overload
    /// <summary>
    /// 다중 요청 시도
    /// </summary>
    /// <param name="method">POST/GET/PUT/DELETE</param>
    /// <param name="url">URL</param>
    /// <param name="data"></param>
    /// <param name="callback"></param>
    /// <param name="needAuthor"></param>
    /// <param name="tryCount"></param>
    /// <returns></returns>
    public void request(string method, string url, Callback callback, CallbackRetryOccured retryMessageCallback, bool needAuthor = true, int tryCount = 3) {
        WWWForm data = null;

        GameObject subReqObj = Instantiate(subReqPrefab, transform);
        NetworkModules.Request subReq = subReqObj.GetComponent<NetworkModules.Request>();

        subReq.StartRequest(
            method: method,
            url: url,
            data: data,
            tryCount: tryCount,
            callback: callback,
            retryMessageCallback: retryMessageCallback);
    }
}

public class HttpResponse {
    public bool isError;
    public string errorMessage;
    public string data;
    public long responseCode;
    public UnityWebRequest request;
    public string header;
    public HttpResponse(UnityWebRequest _request){
        request = _request;
        responseCode = _request.responseCode;
        isError = _request.isNetworkError;
        errorMessage = _request.error;
        if(_request.downloadHandler == null) {
            data = null;
        }
        else {
            data = _request.downloadHandler.text;
        }
        header = _request.GetResponseHeader("Link");
    }
}

/// <summary>
/// BestHTTP Pro
/// 점진적으로 기존 HTTP 요청을 모두 BestHTTP로 통합/변경 예정
/// </summary>
public partial class NetworkManager {
    Queue<RequestFormat> requests = new Queue<RequestFormat>();
    private bool dequeueing = false;
    TimeSpan timeout = new TimeSpan(0, 0, 30);  //timeout 30초 지정 

    private void FixedUpdate() {
        if (dequeueing) return;
        if (requests.Count == 0) return;
        DequeueRequest();
    }

    private void DequeueRequest() {
        dequeueing = true;
        GameObject loadingModal = LoadingModal.instantiate();
        Text loadingMsg = loadingModal.transform.Find("Panel/AdditionalMessage").GetComponent<Text>();

        RequestFormat selectedRequestFormat = requests.Dequeue();
        HTTPRequest request = selectedRequestFormat.request;
        loadingMsg.text = selectedRequestFormat.loadingMessage;

        request.SetHeader("Content-Type", "application/json");
        request.Callback += selectedRequestFormat.callback;
        request.Callback += (x, y) => {
            dequeueing = false;
            Destroy(loadingModal);
        };
        request.ConnectTimeout = timeout;
        request.Send();
    }

    /// <summary>
    /// HTTP 요청
    /// </summary>
    /// <param name="request">HTTPRequest에 맞는 Format 작성</param>
    /// <param name="callback">요청 완료시 받을 Callback</param>
    public void Request(HTTPRequest request, OnRequestFinishedDelegate callback, string msg = null) {
        requests.Enqueue(new RequestFormat(request, callback, msg));
    }

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
        public DeckItem[] items;
    }

    public class DeckItem {
        public string cardId;
        public int cardCount;
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