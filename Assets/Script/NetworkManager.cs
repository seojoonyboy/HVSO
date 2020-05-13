using System;
using BestHTTP;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Collections;
using Haegin;
using HaeginGame;
using UnityEngine.SceneManagement;

/// <summary>
/// BestHTTP Pro
/// </summary>
public partial class NetworkManager : Singleton<NetworkManager> {
    public string baseUrl { get; private set; }
    public void SetUrl(string url) {
        this.baseUrl = url;
    }

    protected NetworkManager() { }

    private WebClient webClient;

    public void Auth() {
        DontDestroyOnLoad(gameObject);
        MAX_REDIRECTCOUNT = 10;
        webClient = WebClient.GetInstance();

        webClient.ErrorOccurred += OnErrorOccurred;
        webClient.Processing += OnProcessing;
        webClient.RetryOccurred += RetryOccurred;
        webClient.RetryFailed += RetryFailed;
        //webClient.MaintenanceStarted += OnMaintenanceStarted;
        webClient.Logged += (string log) => {
#if MDEBUG
            Debug.Log("Unity   " + log);
#endif
        };

        Debug.Log("Requesting IssueJWT");
        webClient.Request(new IssueJWTReq());
        //ThreadSafeDispatcher.Instance.PushSystemBackKeyListener(OnSystemBackKey);
    }

    void RetryOccurred(Protocol protocol, int retryCount)
    {
#if MDEBUG
        Debug.Log(protocol.ProtocolId + " : Retry Occurred  " + retryCount);
#endif
    }

    void RetryFailed(Protocol protocol)
    {
#if MDEBUG
        Debug.Log(protocol.ProtocolId + " : Retry Failed ");
#endif
    }

    void OnProcessing(ReqAndRes rar)
    {
#if MDEBUG
        Debug.Log("Processed Protocol : " + rar.Req.Protocol.ProtocolId);
        Debug.Log("Processed Result :" + rar.Res.ProtocolId);
#endif
        if(ProtocolId.IssueJWT == rar.Res.ProtocolId) {
            IssueJWTRes result = (IssueJWTRes)rar.Res;
            //Logger.Log("Token : " + result.Token);
            AccountManager.Instance.TokenId = result.Token;
            if(GameObject.Find("FBL_Login_Canvas") == null) return;
            GameObject.Find("FBL_Login_Canvas").transform.Find("Panel").GetComponent<Button>().enabled = true;
            //FindObjectOfType<SceneOBBCheckController>().gameObject.SetActive(false);
            //FindObjectOfType<LoginController>().Login();
            FindObjectOfType<LoginController>().sceneStartCanvas.gameObject.SetActive(true);
        }

        if(GameObject.Find("connectErrorModal")) return;
        if(rar.Res.Result == Result.AuthenticationExpired) {
            Debug.Log("중복접속확인");
            GameObject connectModal = Modal.instantiate("중복 접속이 확인되어 타이틀 화면으로 이동합니다.", Modal.Type.CHECK, ()=> {
                GameObject[] allObject = GameObject.FindObjectsOfType<GameObject>();
                foreach(GameObject gameobject in allObject) {
                    Destroy(gameobject);
                }
                SceneManager.LoadScene(0, LoadSceneMode.Single);
                
            });
            connectModal.name = "connectErrorModal";
        }
    }

    public void OnErrorOccurred(int error)
    {
        #if MDEBUG
        Debug.Log("OnErrorOccurred " + error);
        #endif
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
        if(CheckInternetState()) return;
        requests.Enqueue(new RequestFormat(request, callback, msg));
    }

    private bool CheckInternetState() {
        if(Application.internetReachability != NetworkReachability.NotReachable) return false;
        //Modal.instantiate("Internet Problem\nPlease check your network status\nGame is going to shut down", Modal.Type.CHECK, ()=>Application.Quit());
        return true;
    }

    private void DequeueRequest() {
        dequeueing = true;
        loadingModal = LoadingModal.instantiate();
        Text loadingMsg = loadingModal.transform.Find("Panel/AdditionalMessage").GetComponent<Text>();

        RequestFormat selectedRequestFormat = requests.Dequeue();
        HTTPRequest request = selectedRequestFormat.request;
        loadingMsg.text = selectedRequestFormat.loadingMessage;
        loadingModal.SetActive(false);
        request.LoadingMessage = selectedRequestFormat.loadingMessage;

        request.AddHeader("Content-Type", "application/json");
        if(request.RedirectCount != 0) request.Callback = null;
        request.Callback += CheckCondition;
        request.Callback += selectedRequestFormat.callback;
        request.Callback += FinishRequest;
        request.Timeout = timeout;
        request.Send();
        
        Logger.Log("<color=blue>"  + request.Uri + "</color>");
        StartCoroutine(ActivateLoadingModal(loadingModal));
    }

    IEnumerator ActivateLoadingModal(GameObject modal) {
        yield return new WaitForSeconds(2.0f);
        if(modal != null) modal.SetActive(true);
    }

    private void CheckCondition(HTTPRequest request, HTTPResponse response) {
        //timeout에 따른 재요청
        if(response == null) {
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
        public string bannerImage;
        public DeckEditController.DeckItem[] items;
    }

    public class ChangeProgressReqFormat {
        public int qid;
        public int progress;
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

    public class ReconnectData {
        public string gameId;
        public string camp;
        public string battleType;

        public ReconnectData(string gameId, string camp, string battleType) {
            this.gameId = gameId;
            this.camp = camp;
            this.battleType = battleType;
        }
    }

    public class ClearedStageFormat {
        public int id;
        public int userId;
        public int? chapterNumber;
        public string camp;
        public int stageNumber;
    }

    /// <summary>
    /// 3승 보상 요청에 대한 응답 format
    /// </summary>
    public class ThreeWinResFormat {
        public bool claimComplete;
        public List<SocketFormat.leagueWinReward> reward;
    }
}