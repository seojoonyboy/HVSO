using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine;
using BestHTTP.WebSocket;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SocketFormat;
using System.Threading.Tasks;
using System.Linq;

public partial class BattleConnector : MonoBehaviour {

    private string url {
        get {
            return NetworkManager.Instance.baseUrl + "game";
        }
    }

    private string lobbyUrl {
        get {
            return NetworkManager.Instance.baseUrl + "lobby/socket";
        }
    }
    
    WebSocket webSocket;
    [SerializeField] protected Text message;
    [SerializeField] protected Text timer;
    [SerializeField] protected GameObject machine;
    [SerializeField] protected Button returnButton, aiBattleButton;
    [SerializeField] protected GameObject textBlur;

    public UnityAction<string, int, bool> HandchangeCallback;
    protected Coroutine timeCheck;
    protected bool battleGameFinish = false;
    protected bool isQuit = false;
    protected int reconnectCount = 0;

    public static UnityEvent OnOpenSocket = new UnityEvent();

    private void Awake() {
        thisType = this.GetType();
        DontDestroyOnLoad(gameObject);
        Application.wantsToQuit += Quitting;
    }

    private void OnDestroy() {
        Application.wantsToQuit -= Quitting;
    }

    /// <summary>
    /// open lobby socket
    /// </summary>
    public virtual void OpenLobby() {
        reconnectCount = 0;

        Debug.Assert(!PlayerPrefs.GetString("SelectedRace").Any(char.IsUpper), "Race 정보는 소문자로 입력해야 합니다!");
        string race = PlayerPrefs.GetString("SelectedRace").ToLower();

        string url = string.Format("{0}", this.lobbyUrl);
        webSocket = new WebSocket(new Uri(string.Format("{0}?token={1}&camp={2}", url, AccountManager.Instance.TokenId, race)));
        webSocket.OnOpen += OnLobbyOpen;
        webSocket.OnMessage += ReceiveMessage;
        //webSocket.OnClosed += OnLobbyClosed;
        webSocket.OnError += Error;
        webSocket.Open();

        message.text = "대전상대를 찾는중...";
        timeCheck = StartCoroutine(TimerOn());
        returnButton.onClick.AddListener(BattleCancel);
        returnButton.gameObject.SetActive(true);

        aiBattleButton.gameObject.SetActive(true);
    }


    void Error(WebSocket webSocket, Exception ex) {
        Logger.LogError(ex);
    }

    public void OnAiBattleButtonClicked() {
        if(webSocket != null) webSocket.Close();
        PlayerPrefs.SetString("SelectedBattleType", "leagueTest");
        aiBattleButton.gameObject.SetActive(false);

        OpenSocket();
    }

    /// <summary>
    /// open game socket (after lobby socket connected)
    /// </summary>
    public virtual void OpenSocket() {
        reconnectCount = 0;
        string url = string.Format("{0}", this.url);
        webSocket = new WebSocket(new Uri(string.Format("{0}?token={1}", url, AccountManager.Instance.TokenId)));
        webSocket.OnOpen += OnOpen;
        webSocket.OnMessage += ReceiveMessage;
        webSocket.OnClosed += OnClosed;
        webSocket.OnError += OnError;
        webSocket.Open();

        message.text = "대전상대를 찾는중...";
        timeCheck = StartCoroutine(TimerOn());
        returnButton.onClick.AddListener(BattleCancel);
        returnButton.gameObject.SetActive(true);
    }

    private void OnLobbyOpen(WebSocket webSocket) {
        //Logger.Log("OnLobbyOpen");
    }

    private void OnLobbyClosed(WebSocket webSocket, ushort code, string message) {
        OpenSocket();
    }

    private IEnumerator TimerOn() {
        int time = 0;
        while(time < 60) {
            yield return new WaitForSeconds(1f);
            if(timer != null) timer.text = string.Format("{0}초 대기 중...", time);
            time++;
        }
        message.text = "대전상대를 찾지 못했습니다.";
        timer.text = "이전 메뉴로 돌아갑니다.";
        returnButton.gameObject.SetActive(false);
        webSocket.Close();
        yield return new WaitForSeconds(3f);
        FBL_SceneManager.Instance.LoadScene(FBL_SceneManager.Scene.MAIN_SCENE);
        Destroy(gameObject);
    }

    private void BattleCancel() {
        StopCoroutine(timeCheck);
        webSocket.Close();
        returnButton.onClick.RemoveListener(BattleCancel);
        FBL_SceneManager.Instance.LoadScene(FBL_SceneManager.Scene.MAIN_SCENE);
        Destroy(gameObject);
    }

    public void OnClosed(WebSocket webSocket, ushort code, string msg) {
        //Logger.LogWarning("Socket has been closed : " + code + "  message : " + msg);
        if(battleGameFinish) return;
        reconnectModal = Instantiate(Modal.instantiateReconnectModal());
        TryReconnect();
    }

    public void OnError(WebSocket webSocket, Exception ex) {
        Time.timeScale = 0;
        reconnectModal = Instantiate(Modal.instantiateReconnectModal());
        //Logger.LogError("Socket Error message : " + ex);
        TryReconnect();
    }

    private bool Quitting() {
        //Logger.Log("quitting");
        isQuit = true;
        if(webSocket != null) {
            webSocket.OnOpen -= OnOpen;
            webSocket.OnMessage -= ReceiveMessage;
            webSocket.OnClosed -= OnClosed;
            webSocket.OnError -= OnError;
            webSocket.Close();
        }
        PlayerPrefs.DeleteKey("ReconnectData");
        return true;
    }

    public async void TryReconnect() {
        await Task.Delay(2000);
        if(isQuit) return;
        if(reconnectCount >= 5) {
            PlayMangement playMangement = PlayMangement.instance;
            PlayerPrefs.DeleteKey("ReconnectData");

            ReconnectController controller = FindObjectOfType<ReconnectController>();
            if(controller != null) {
                Destroy(controller);
                Destroy(controller.gameObject);
            }

            if (playMangement) playMangement.resultManager.SocketErrorUIOpen(false);
            else FBL_SceneManager.Instance.LoadScene(FBL_SceneManager.Scene.MAIN_SCENE);
            return;
        }
        reconnectCount++;
        webSocket = new WebSocket(new Uri(string.Format("{0}?token={1}", url, AccountManager.Instance.TokenId)));
        webSocket.OnOpen += OnOpen;
        webSocket.OnMessage += ReceiveMessage;
        webSocket.OnClosed += OnClosed;
        webSocket.OnError += OnError;
        webSocket.Open();
    }

    //Connected
    private void OnOpen(WebSocket webSocket) {
        string[] args;
        string reconnect = PlayerPrefs.GetString("ReconnectData", null);
        if(!string.IsNullOrEmpty(reconnect)) {
            NetworkManager.ReconnectData data = JsonConvert.DeserializeObject<NetworkManager.ReconnectData>(reconnect);
            bool isSameType = data.battleType.CompareTo(PlayerPrefs.GetString("SelectedBattleType")) == 0;
            //bool isMulti = data.battleType.CompareTo("multi") == 0;
            if(isSameType) {
                args = new string[]{data.gameId, data.camp, data.battleType.CompareTo("leagueTest") == 0 ? "league" : data.battleType};
                SendMethod("reconnect_game", args);
                return;
            }
            PlayerPrefs.DeleteKey("ReconnectData");
        }
        args = SetJoinGameData();
        SendMethod("join_game", args);
        OnOpenSocket.Invoke();
    }

    private void JoinGame() {
        webSocket.Close();
        OpenSocket();

        string[] args;
        PlayerPrefs.SetString("SelectedBattleType", "league");

        args = SetJoinGameData();
        SendMethod("join_game", args);
    }

    private string[] SetJoinGameData() {
        string deckId = PlayerPrefs.GetString("SelectedDeckId");
        string battleType = PlayerPrefs.GetString("SelectedBattleType");

        Debug.Assert(!PlayerPrefs.GetString("SelectedRace").Any(char.IsUpper), "Race 정보는 소문자로 입력해야 합니다!");
        string race = PlayerPrefs.GetString("SelectedRace").ToLower();

        if(battleType.CompareTo("test") == 0)
            return new string[] { battleType, race };

        if (battleType == "story") {
            //========================================================
            //deckId 및 race 관련 처리 수정 예정
            string stageNum = PlayerPrefs.GetString("StageNum");
            //race = "human";
            race = (race == "human") ? "human" : "orc";
            deckId = string.Empty;
            return new string[] { battleType, deckId, race, stageNum };
            //========================================================
        }
        else if(battleType == "league") {
            //========================================================
            //matchkey 필요
            return new string[] { battleType, deckId, race, matchKey };
            //========================================================
        }
        //return new string[] { "story", "", "orc", "10"};
        return new string[] { battleType, deckId, race };

    }

    public void ClientReady() {
        SendMethod("client_ready");
    }

    public void TutorialEnd() {
        SendMethod("end_story_game");

        var isHuman = PlayMangement.instance.player.isHuman;
        if (isHuman) {
            PlayerPrefs.SetString("PrevTutorial", "Human_Tutorial");
        }
        else {
            PlayerPrefs.SetString("PrevTutorial", "Orc_Tutorial");
        }
    }

    private class ItemIdClass {
        public ItemIdClass(int itemId) { this.itemId = itemId.ToString(); }
        public string itemId;
    }

    public void ChangeCard(int itemId) {
        JObject item = new JObject();
        item["itemId"] = itemId.ToString();
        //argClass = null; //카드 무제한 변경 코드
        SendMethod("hand_change", item);
    }

    public void MulliganEnd() {
        if(ScenarioGameManagment.scenarioInstance == null) {
            PlayMangement.instance.player.GetComponent<IngameTimer>().EndTimer();
        }
        SendMethod("end_mulligan");
    }

    public void TurnOver() {
        StartCoroutine(waitSkillDone(() => {SendMethod("turn_over");}));
    }

    public void UseCard(object args) {
        SendMethod("play_card", args);
    }

    public void SendStartState(object args) {
        SendMethod("start_state", args);
    }

    public void UnitSkillActivate(object args) {
        SendMethod("unit_skill_activate", args);
    }

    public void KeepHeroCard(int itemId) {
        JObject args = new JObject();
        args["itemId"] = itemId;
        Debug.Log(args);
        SendMethod("keep_hero_card", args);
    }

    public void Surrend(object args) {
        SendMethod("player_surrender", args);
    }

    void OnDisable() {
        if(webSocket != null) Quitting();
    }

    public void StartBattle() {
        if (PlayerPrefs.GetString("SelectedBattleType") != "story")
            UnityEngine.SceneManagement.SceneManager.LoadScene("IngameScene", UnityEngine.SceneManagement.LoadSceneMode.Single);
        else
            UnityEngine.SceneManagement.SceneManager.LoadScene("TutorialScene", UnityEngine.SceneManagement.LoadSceneMode.Single);
    }

    public void SendMethod(string method, object args = null) {
        if(args == null) args = new string[]{};
        SendFormat format = new SendFormat(method, args);
        string json = JsonConvert.SerializeObject(format);
        webSocket.Send(json);
    }

    public void DrawNewCards(int drawNum, int itemId) {
        PlayMangement playMangement = PlayMangement.instance;
        bool isHuman = playMangement.player.isHuman;
        int cardNum = gameState.players.myPlayer(isHuman).deck.handCards.Length - 1;
        StartCoroutine(DrawCardIEnumerator(itemId));
    }

    public IEnumerator DrawCardIEnumerator(int itemId) {
        PlayMangement playMangement = PlayMangement.instance;
        bool isHuman = playMangement.player.isHuman;
        yield return new WaitUntil(() =>
            gameState.lastUse != null && gameState.lastUse.cardItem.itemId == itemId && 
            ((gameState.lastUse.cardItem.camp.CompareTo("human")==0) == playMangement.player.isHuman));

        StartCoroutine(PlayMangement.instance.player.cdpm.AddMultipleCard(gameState.players.myPlayer(isHuman).deck.handCards));
    }
}