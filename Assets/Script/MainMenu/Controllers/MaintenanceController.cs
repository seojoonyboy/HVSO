using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Haegin;
using HaeginGame;
using System;

public class MaintenanceController : MonoBehaviour {

    private WebClient webClient;

    [SerializeField] private GameObject systemDialog;
    [SerializeField] private GameObject eulaText;
    [SerializeField] private Canvas canvas;

    private void Start() {
        WebClientInit();
        AccountManager.Instance.tokenSetFinished += delegate{};
    }

    public void OnDestroy() {
        DestoryWebClient();
        AccountManager.Instance.tokenSetFinished = null;
    }

    private void WebClientInit() {
        webClient = WebClient.GetInstance();

        webClient.ErrorOccurred += OnErrorOccurred;
        webClient.Processing += OnProcessing;
        webClient.RetryOccurred += RetryOccurred;
        webClient.RetryFailed += RetryFailed;
        webClient.MaintenanceStarted += OnMaintenanceStarted;
        webClient.Logged += (string log) => {
#if MDEBUG
            Debug.Log("Unity   " + log);
#endif
        };
    }

    private void DestoryWebClient() {
        if (webClient != null) {
            webClient.ErrorOccurred -= OnErrorOccurred;
            webClient.Processing -= OnProcessing;
            webClient.RetryOccurred -= RetryOccurred;
            webClient.RetryFailed -= RetryFailed;
            webClient.MaintenanceStarted -= OnMaintenanceStarted;
        }
    }

    void OnProcessing(ReqAndRes rar){
        if(ProtocolId.IssueJWT == rar.Res.ProtocolId) {
            IssueJWTRes result = (IssueJWTRes)rar.Res;
            AccountManager.Instance.TokenId = result.Token;
            Modal.instantiate("계정 연동이 완료 되었습니다.", Modal.Type.CHECK, () => FBL_SceneManager.Instance.LoadScene(FBL_SceneManager.Scene.MAIN_SCENE));
        }
    }

    public void OnErrorOccurred(int error) {
        OnNetworkError();
    }

    void OnNetworkError() {
        UGUICommon.ShowMessageDialog(systemDialog, eulaText, canvas, TextManager.GetString(TextManager.StringTag.NetworkError), TextManager.GetString(TextManager.StringTag.NetworkErrorMessage), (UGUICommon.ButtonType buttonType) => {
            if (buttonType == UGUICommon.ButtonType.Ok) {
                ThreadSafeDispatcher.ApplicationQuit();
            }
        });
    }

    void OnMaintenanceStarted() {
        // 메인터넌스가 시작되었다.
        UGUICommon.ShowMessageDialog(systemDialog, eulaText, canvas, TextManager.GetString(TextManager.StringTag.ServerMaintenanceTitle), TextManager.GetString(TextManager.StringTag.ServerMaintenance), (UGUICommon.ButtonType buttonType) =>
        {
            if (buttonType == UGUICommon.ButtonType.Ok)
            {
                ThreadSafeDispatcher.ApplicationQuit();
            }
        });
    }
    
    void RetryFailed(Protocol protocol) {
        OnNetworkError();
    }

    void RetryOccurred(Protocol protocol, int retryCount) {
#if MDEBUG
        Debug.Log("Retry Occurred  " + retryCount);
#endif
    }

}
