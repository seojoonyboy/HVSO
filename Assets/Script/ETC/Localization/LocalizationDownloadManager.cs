using BestHTTP;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocalizationDownloadManager : MonoBehaviour {
    Queue<NetworkManager.RequestFormat> requests = new Queue<NetworkManager.RequestFormat>();
    private bool dequeueing = false;
    TimeSpan timeout = new TimeSpan(0, 0, 30);
    public int MAX_REDIRECTCOUNT { get; private set; }
    [SerializeField] LocalizationProgress localizationProgress;
    void Awake() {
        MAX_REDIRECTCOUNT = 10;
    }

    void FixedUpdate() {
        if (dequeueing) return;
        if (requests.Count == 0) {
            localizationProgress.OnFinished();
            return;
        }
        DequeueRequest();
    }

    /// <summary>
    /// HTTP 요청
    /// </summary>
    /// <param name="request">HTTPRequest에 맞는 Format 작성</param>
    /// <param name="callback">요청 완료시 받을 Callback</param>
    public void Request(HTTPRequest request, OnRequestFinishedDelegate callback, string msg = null) {
        requests.Enqueue(new NetworkManager.RequestFormat(request, callback, msg));
    }

    private void DequeueRequest() {
        dequeueing = true;
        localizationProgress.StartProgress();

        NetworkManager.RequestFormat selectedRequestFormat = requests.Dequeue();
        HTTPRequest request = selectedRequestFormat.request;
        localizationProgress.label.text = selectedRequestFormat.loadingMessage;

        if (request.RedirectCount != 0) request.Callback = null;
        request.Callback += CheckCondition;
        request.Callback += selectedRequestFormat.callback;
        request.Callback += FinishRequest;
        request.OnProgress += OnProgress;
        request.Timeout = timeout;
        request.Send();
    }

    private void OnProgress(HTTPRequest originalRequest, long downloaded, long downloadLength) {
        localizationProgress.OnProgress(downloaded, downloadLength);
    }

    private void CheckCondition(HTTPRequest request, HTTPResponse response) {
        //timeout에 따른 재요청
        if (response == null) {
            FinishRequest(request, response);
            if (request.RedirectCount == MAX_REDIRECTCOUNT) {
                Modal.instantiate("Server가 불안정합니다. 잠시후 다시 접속해주세요.", Modal.Type.CHECK, () => {
                    //Application.Quit();
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
    }

    private void FinishRequest(HTTPRequest request, HTTPResponse response) {
        dequeueing = false;
    }
}
