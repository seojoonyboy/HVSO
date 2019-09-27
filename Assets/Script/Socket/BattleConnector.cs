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

    private string url { get { 
        return NetworkManager.Instance.baseUrl + "game"; } }
    
    WebSocket webSocket;
    [SerializeField] Text message;
    [SerializeField] Text timer;
    [SerializeField] GameObject machine;
    [SerializeField] Button returnButton;

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

    private void Destroy() {
        Application.wantsToQuit -= Quitting;
    }

    public void OpenSocket() {
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
        Logger.LogWarning("Socket has been closed : " + code + "  message : " + msg);
        if(battleGameFinish) return;
        reconnectModal = Instantiate(Modal.instantiateReconnectModal());
        TryReconnect();
    }

    public void OnError(WebSocket webSocket, Exception ex) {
        Time.timeScale = 0;
        reconnectModal = Instantiate(Modal.instantiateReconnectModal());
        Logger.LogError("Socket Error message : " + ex);
        TryReconnect();
    }

    private bool Quitting() {
        Logger.Log("quitting");
        isQuit = true;
        webSocket.Close();
        webSocket.OnOpen -= OnOpen;
        webSocket.OnMessage -= ReceiveMessage;
        webSocket.OnClosed -= OnClosed;
        webSocket.OnError -= OnError;

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
                args = new string[]{data.gameId, data.camp};
                SendMethod("reconnect_game", args);
                return;
            }
            PlayerPrefs.DeleteKey("ReconnectData");
        }
        args = SetJoinGameData();
        SendMethod("join_game", args);
        OnOpenSocket.Invoke();
    }

    private string[] SetJoinGameData() {
        string deckId = PlayerPrefs.GetString("SelectedDeckId");
        string battleType = PlayerPrefs.GetString("SelectedBattleType");
        string race = PlayerPrefs.GetString("SelectedRace").ToLower();

        if(battleType.CompareTo("test") == 0)
            return new string[] { battleType, race };
        
        //Logger.Log("stageNum : " + PlayerPrefs.GetString("StageNum"));
        if (battleType == "story" && PlayerPrefs.GetString("StageNum") != "1") {
            PlayerPrefs.SetString("SelectedBattleType", "solo");
            battleType = "solo";
        }  //TODO : 튜토리얼 이외의 스토리 세팅이 되면 변경할 필요가 있음
        if (battleType == "story") {
            //========================================================
            //deckId 및 race 관련 처리 수정 예정
            string stageNum = PlayerPrefs.GetString("StageNum");
            race = "human";
            deckId = string.Empty;
            return new string[] { battleType, deckId, race, stageNum };
            //========================================================
        }
        //return new string[] { "story", "", "orc", "10"};
        return new string[] { battleType, deckId, race };

    }

    public void ClientReady() {
        SendMethod("client_ready");
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
        Logger.LogWarning(ex);
    }

    void OnDisable() {
        if(webSocket != null) webSocket.Close();
    }

    public void StartBattle() {
        if (PlayerPrefs.GetString("SelectedBattleType") != "story")
            UnityEngine.SceneManagement.SceneManager.LoadScene("IngameScene", UnityEngine.SceneManagement.LoadSceneMode.Single);
        else
            UnityEngine.SceneManagement.SceneManager.LoadScene("TutorialScene", UnityEngine.SceneManagement.LoadSceneMode.Single);
    }

    private void SendMethod(string method, object args = null) {
        if(args == null) args = new string[]{};
        SendFormat format = new SendFormat(method, args);
        string json = JsonConvert.SerializeObject(format);
        webSocket.Send(json);
        Logger.Log(string.Format("보내는 메시지 : {0}", json));
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

    public IEnumerator WaitGetCard() {
        if(!getNewCard) IngameNotice.instance.SetNotice("서버로부터 카드를 받고 있습니다");
        while(!getNewCard) {
            yield return new WaitForFixedUpdate();
        }
        getNewCard = false;
        IngameNotice.instance.CloseNotice();
    }

    public IEnumerator WaitMulliganFinish() {
        WaitForFixedUpdate fixedUpdate = new WaitForFixedUpdate();
        if(gameState.state.CompareTo("mulligan") == 0) IngameNotice.instance.SetNotice("상대방이 카드 교체중입니다.");
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