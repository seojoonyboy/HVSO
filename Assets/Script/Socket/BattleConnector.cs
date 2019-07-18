using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine;
using BestHTTP.WebSocket;
using Bolt;
using Newtonsoft.Json;
using SocketFormat;

public partial class BattleConnector : MonoBehaviour {

    #if UNITY_EDITOR
    private string url = "wss://ccdevclient.fbl.kr/game";
    #else
    private string url = "wss://cctest.fbl.kr/game";
    #endif
    WebSocket webSocket;
    [SerializeField] Text message;
    [SerializeField] Text timer;
    [SerializeField] GameObject machine;
    [SerializeField] Button returnButton;

    public UnityAction<string, int, bool> HandchangeCallback;
    private Coroutine pingpong;
    private Coroutine timeCheck;
    private bool battleGameFinish = false;

    void Awake() {
        thisType = this.GetType();
        DontDestroyOnLoad(gameObject);
    }

    public void OpenSocket() {
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
        SceneManager.Instance.LoadScene(SceneManager.Scene.MAIN_SCENE);
        Destroy(gameObject);
    }

    private void BattleCancel() {
        StopCoroutine(timeCheck);
        webSocket.Close();
        returnButton.onClick.RemoveListener(BattleCancel);
        SceneManager.Instance.LoadScene(SceneManager.Scene.MAIN_SCENE);
        Destroy(gameObject);
    }

    public void OnClosed(WebSocket webSocket, ushort code, string msg) {
        Logger.LogWarning("Socket has been closed : " + code + "  message : " + msg);
        battleGameFinish = true;
    }

    public void OnError(WebSocket webSocket, Exception ex) {
        Logger.LogError("Socket Error message : " + ex);
    }

    //Connected
    private void OnOpen(WebSocket webSocket) {
        //string playerId = AccountManager.Instance.DEVICEID;
        string deckId = Variables.Saved.Get("SelectedDeckId").ToString();
        string battleType = Variables.Saved.Get("SelectedBattleType").ToString();
        string race = Variables.Saved.Get("SelectedRace").ToString().ToLower();

        string[] args = new string[] { battleType, deckId, race };
        SendMethod("join_game", args);
        pingpong = StartCoroutine(Heartbeat());
    }

    IEnumerator Heartbeat() {
        WaitForSeconds beatTime = new WaitForSeconds(10f);
        while(webSocket.IsOpen) {
            yield return beatTime;
        }
        if(!battleGameFinish)
            PlayMangement.instance.SocketErrorUIOpen(false);
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
        SendMethod("end_mulligan");
    }

    public void TurnOver() {
        StartCoroutine(waitSkillDone(() => {SendMethod("turn_over");}));
    }

    public void UseCard(object args, UnityAction callback = null) {
        SendMethod("play_card", args);
    }

    public void UnitSkillActivate(object args) {
        SendMethod("unit_skill_activate", args);
    }

    void Error(WebSocket webSocket, Exception ex) {
        Logger.LogWarning(ex);
    }

    void OnDisable() {
        if(pingpong != null) StopCoroutine(pingpong);
        if(webSocket != null) webSocket.Close();
    }

    public void StartBattle() {
        SceneManager.Instance.LoadScene(SceneManager.Scene.MISSION_INGAME);
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
    public Queue <ShieldCharge> shieldChargeQueue = new Queue<ShieldCharge>();
    public QueueSocketList<int> unitSkillList = new QueueSocketList<int>();

    public IEnumerator WaitGetCard() {
        if(!getNewCard) IngameNotice.instance.SetNotice("서버로부터 카드를 받고 있습니다");
        while(!getNewCard) {
            yield return new WaitForFixedUpdate();
        }
        getNewCard = false;
        dequeueing = true;
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

    public GameState getStateList(bool isBattleEnd) {
        return isBattleEnd ? mapClearList.Dequeue() : lineBattleList.Dequeue();
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
        private int totalCount;

        public QueueSocketList() {
            isDone = true;
            queue = new Queue<T>();
            totalCount = 0;
        }

        public void Enqueue(T value) {
            totalCount++;
            queue.Enqueue(value);
        }

        public T Dequeue() {
            if(queue.Count == 0) return default(T);
            return queue.Dequeue();
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