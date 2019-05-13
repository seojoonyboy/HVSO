using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class NetworkManager : Singleton<NetworkManager> {
#if DEVELOPMENT_BUILD
    //개발용
    //public string baseUrl = "http://ihak2devclient.fbl.kr/";
#else
    //릴리즈용
    //public string baseUrl = "http://ihak2test.fbl.kr/";
#endif
    protected NetworkManager() { }
    public delegate void Callback(HttpResponse response);

    public void request(string method, string url, WWWForm data, Callback callback, bool neeAuthor = true) {
        StartCoroutine(_request(method, url, data, callback, neeAuthor));
    }

    public void request(string method, string url, string data, Callback callback, bool neeAuthor = true) {
        StartCoroutine(_request(method, url, data, callback, neeAuthor));
    }

    public void request(string method, string url, Callback callback, bool neeAuthor = true) {
        WWWForm data = null;
        request(method, url, data, callback, neeAuthor);
    }

    IEnumerator _request(string method, string url, WWWForm data, Callback callback, bool needAuthor = true){
        UnityWebRequest _www;
        switch(method){
            case "POST":
                _www = UnityWebRequest.Post(url, data);
                break;
            case "PUT":
                _www = UnityWebRequest.Put(url,data.data);
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

        using(UnityWebRequest www = _www){
            yield return www.SendWebRequest();
            callback(new HttpResponse(www));
        }
    }

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