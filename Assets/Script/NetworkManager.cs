using System;
using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System.Collections.Generic;

public partial class NetworkManager : Singleton<NetworkManager> {
    //개발용
    public string baseUrl = "http://ccdevclient.fbl.kr/";
    //public string baseUrl = "http://192.168.1.23/";
    protected NetworkManager() { }
    public delegate void Callback(HttpResponse response);
    public delegate void CallbackRetryOccured(string msg);

    private void Awake() {
        DontDestroyOnLoad(gameObject);
    }

    public void request(string method, string url, string data, Callback callback, bool neeAuthor = true) {
        StartCoroutine(_request(method, url, data, callback, neeAuthor));
    }

    //IEnumerator _request(string method, string url, WWWForm data, Callback callback, bool needAuthor = true){
    //    UnityWebRequest _www;
    //    switch(method){
    //        case "POST":
    //            _www = UnityWebRequest.Post(url, data);
    //            break;
    //        case "PUT":
    //            _www = UnityWebRequest.Put(url,data.data);
    //            _www.SetRequestHeader("Content-Type", "application/json");
    //            break;
    //        case "DELETE":
    //            _www = UnityWebRequest.Delete(url);
    //            break;
    //        case "GET":
    //        default:
    //            _www = UnityWebRequest.Get(url);
    //            break;
    //    }
    //    _www.timeout = 10;

    //    if (needAuthor) {
    //        //_www.SetRequestHeader("Authorization", "Token " + GameManager.Instance.userStore.userTokenId);
    //        //_www.downloadHandler = new DownloadHandlerBuffer();
    //    }

    //    using(UnityWebRequest www = _www){
    //        yield return www.SendWebRequest();
    //        callback(new HttpResponse(www));
    //    }
    //}

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