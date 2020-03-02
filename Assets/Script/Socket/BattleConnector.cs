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
    [SerializeField] Text message;
    [SerializeField] Text timer;
    [SerializeField] GameObject machine;
    [SerializeField] Button returnButton, aiBattleButton;
    [SerializeField] GameObject textBlur;

    public UnityAction<string, int, bool> HandchangeCallback;
    private Coroutine timeCheck;
    private bool battleGameFinish = false;
    private bool isQuit = false;
    private int reconnectCount = 0;

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
    public void OpenLobby() {
        reconnectCount = 0;

        Debug.Assert(!PlayerPrefs.GetString("SelectedRace").Any(char.IsUpper), "Race 정보는 소문자로 입력해야 합니다!");
        string race = PlayerPrefs.GetString("SelectedRace").ToLower();

        string url = string.Format("{0}", this.lobbyUrl);
        webSocket = new WebSocket(new Uri(string.Format("{0}?token={1}&camp={2}", url, AccountManager.Instance.TokenId, race)));
        webSocket.OnOpen += OnLobbyOpen;
        webSocket.OnMessage += ReceiveMessage;
        //webSocket.OnClosed += OnLobbyClosed;
        webSocket.OnError += OnError;
        webSocket.Open();

        string findMessage = AccountManager.Instance.GetComponent<Fbl_Translator>().GetLocalizedText("MainUI", "ui_page_league_findopponent");

        message.text = findMessage;
        timeCheck = StartCoroutine(TimerOn());

        string battleType = PlayerPrefs.GetString("SelectedBattleType");

        if (battleType == "league" || battleType == "leagueTest") {
            returnButton.onClick.AddListener(BattleCancel);
            returnButton.gameObject.SetActive(true);
            aiBattleButton.gameObject.SetActive(true);
        }
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
    public void OpenSocket() {
        reconnectCount = 0;
        string url = string.Format("{0}", this.url);
        webSocket = new WebSocket(new Uri(string.Format("{0}?token={1}", url, AccountManager.Instance.TokenId)));
        webSocket.OnOpen += OnOpen;
        webSocket.OnMessage += ReceiveMessage;
        webSocket.OnClosed += OnClosed;
        webSocket.OnError += OnError;
        webSocket.Open();

        string findMessage = AccountManager.Instance.GetComponent<Fbl_Translator>().GetLocalizedText("MainUI", "ui_page_league_findopponent");

        message.text = findMessage;
        timeCheck = StartCoroutine(TimerOn());

        string battleType = PlayerPrefs.GetString("SelectedBattleType");

        if (battleType == "league" || battleType == "leagueTest") {
            returnButton.onClick.AddListener(BattleCancel);
            returnButton.gameObject.SetActive(true);
        }
    }

    private void OnLobbyOpen(WebSocket webSocket) {
        //Logger.Log("OnLobbyOpen");
    }

    private void OnLobbyClosed(WebSocket webSocket, ushort code, string message) {
        OpenSocket();
    }

    private IEnumerator TimerOn() {
        int time = 0;
        string countFormat = AccountManager.Instance.GetComponent<Fbl_Translator>().GetLocalizedText("MainUI", "ui_page_league_waitingsec");
        while(time < 60) {
            yield return new WaitForSeconds(1f);
            if(timer != null) timer.text = countFormat.Replace("{n}", time.ToString());
            ++time;
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
        else if(battleType == "league" || battleType == "leagueTest") {
            //========================================================
            //첫 리그 데이터 구별
            if(leagueData.leagueInfo.winningStreak == 0 && leagueData.leagueInfo.losingStreak == 0)
                PlayerPrefs.SetInt("isLeagueFirst", 1);
            else PlayerPrefs.SetInt("isLeagueFirst", 0);
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
        ItemIdClass argClass = new ItemIdClass(itemId);
        //argClass = null; //카드 무제한 변경 코드
        SendMethod("hand_change", argClass);
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

    void Error(WebSocket webSocket, Exception ex) {
        //Logger.LogWarning(ex);
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
        //Logger.Log(string.Format("보내는 메시지 : {0}", json));
    }

    private string[] ConvertObjectArrayToStringArray(object[] objs) {
        string[] result = Array.ConvertAll<object, string>(objs, ConvertObjectToString);
        return result;
    }

    private string ConvertObjectToString(object obj) {
        return obj?.ToString() ?? string.Empty;
    }
}



/// 클라이언트로부터 데이터를 가져올 때
public partial class BattleConnector : MonoBehaviour {
    private bool getNewCard = false;
    public QueueSocketList<GameState> useCardList = new QueueSocketList<GameState>();
    public QueueSocketList<GameState> lineBattleList = new QueueSocketList<GameState>();
    public QueueSocketList<GameState> mapClearList = new QueueSocketList<GameState>();
    public Queue<SocketFormat.Player> humanData = new Queue<SocketFormat.Player>();
    public Queue<SocketFormat.Player> orcData = new Queue<SocketFormat.Player>();
    public QueueSocketList <ShieldCharge> shieldChargeQueue = new QueueSocketList<ShieldCharge>();
    public QueueSocketList<int> unitSkillList = new QueueSocketList<int>();
    public Queue<SocketFormat.Player> shieldActivateQueue = new Queue<SocketFormat.Player>();

    public IEnumerator WaitGetCard() {
        if(!getNewCard) IngameNotice.instance.SetNotice();
        while(!getNewCard) {
            yield return new WaitForFixedUpdate();
        }
        getNewCard = false;
        IngameNotice.instance.CloseNotice();
    }

    public IEnumerator WaitMulliganFinish() {
        WaitForFixedUpdate fixedUpdate = new WaitForFixedUpdate();
        if(gameState.state.CompareTo("mulligan") == 0) IngameNotice.instance.SetNotice("Waiting...");
        while(gameState.state.CompareTo("mulligan") == 0)
            yield return fixedUpdate;
        IngameNotice.instance.CloseNotice();
    }

    public bool cardPlayFinish() {
        return useCardList.allDone;
    }

    public GameState getHistory() {
        return useCardList.Dequeue();
    }

    private void checkMyTurn(bool isHuman) {
        if(PlayMangement.instance.player.isHuman != isHuman)
            useCardList.isDone = false;
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

    public IEnumerator WaitBattle() {
        bool needWaitMore = !NeedtoWaitBattle();
        yield return new WaitUntil(() => (NeedtoWaitBattle()));
        if(!PlayMangement.instance.player.isHuman && needWaitMore)
            yield return new WaitForSeconds(3f);
    }

    private bool NeedtoWaitBattle() {
        return gameState.state.CompareTo("battleTurn") == 0 || 
                gameState.state.CompareTo("shieldTurn") == 0 || 
                gameState.state.CompareTo("endGame") == 0;
    }
}
namespace SocketFormat {
    public class QueueSocketList<T> {
        public bool isDone;
        private Queue<T> queue;
        private Queue<int> id;
        private int totalCount;

        public QueueSocketList() {
            isDone = true;
            queue = new Queue<T>();
            id = new Queue<int>();
            totalCount = 0;
        }

        public void Enqueue(T value, int id = -1) {
            if(id != -1) {
                if(this.id.Contains(id)) {
                    Debug.Log(id+"id 거르기");
                    return;
                }
                this.id.Enqueue(id);
            }
            totalCount++;
            queue.Enqueue(value);
        }

        public T Dequeue() {
            if(queue.Count == 0) return default(T);
            
            return queue.Dequeue();
        }

        public void RemoveAllId() {
            id.Clear();
        }

        public void checkCount() {
            if(totalCount == 5) isDone = true;
        }
        public int Count { get { return queue.Count; } }

        public bool allDone { get { return isDone && queue.Count == 0; } }

        public IEnumerator WaitNext() {
            while(!isDone) {
                yield return new WaitForFixedUpdate();
                if(Count != 0) break;
            }
        }
    }
}