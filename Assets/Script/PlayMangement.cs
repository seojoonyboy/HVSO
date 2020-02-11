using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Spine;
using Spine.Unity;
using System.IO;
using Tutorial;

public partial class PlayMangement : MonoBehaviour {
    
    public PlayerController player, enemyPlayer;

    public GameObject cardDB;
    public GameObject uiSlot;
    public GameObject playerCanvas, enemyPlayerCanvas;

    public Transform cardInfoCanvas;
    protected Transform battleLineEffect;
    bool firstTurn = true;
    public bool isGame = true;
    public bool isMulligan = true;
    public bool infoOn = false;
    public static PlayMangement instance { get; protected set; }
    public GameObject backGround;
    public GameObject onCanvasPosGroup;
    public GameObject lineMaskObject;
    public GameObject backGroundTillObject;
    //public CardCircleManager cardCircleManager;
    public CardHandManager cardHandManager;
    public GameResultManager resultManager;
    
    public GameObject optionIcon;

    public GameObject baseUnit;
    protected int turn = 0;
    public GameObject blockPanel;
    public int unitNum = 0;
    public bool heroShieldActive = false;
    public List<bool> heroShieldDone = new List<bool>();
    public GameObject humanShield, orcShield;
    public static GameObject movingCard;
    public static bool dragable = true;
    public TurnType currentTurn;

    public bool skillAction = false;
    public victoryModule.VictoryCondition matchRule;
    public bool stopBattle = false;
    public bool stopTurn = false;
    public bool beginStopTurn = false;
    public bool afterStopTurn = false;
    public bool waitDraw = false;
    public bool stopFirstCard = false;

    public float cameraSize;

    public bool waitShowResult = false;

    public ShowCardsHandler showCardsHandler;
    public Button surrendButton;

    public GameObject levelCanvas;

    DequeueCallback messageCallBack;
    public Dictionary<string, string> uiLocalizeData;
    public string ui_FileName;
    public string ui_Key;

    // 시나리오용 추가 데이터들. 상속해서 쓰시면 됩니다.
    public static ChapterData chapterData;
    public static List<ChallengerHandler.Challenge> challengeDatas;
    protected Queue<ScriptData> chapterQueue;
    protected ScriptData currentChapterData;
    public ScenarioExecute currentExecute;
    public bool canNextChapter = true;
    Method currentMethod;
    public Dictionary<string, string> gameScriptData;
    public string fileName;
    public string key;

    public Transform exampleShow;

    public GameObject textCanvas;


    private void Awake() {
        socketHandler = FindObjectOfType<BattleConnector>();
        SetWorldScale();
        ReadUICsvFile();
        instance = this;
        socketHandler.ClientReady();
        SetCamera();
    }
    private void OnDestroy() {
        SoundManager.Instance.bgmController.SoundTrackLoopOn();
        PlayerPrefs.SetString("BattleMode", "");
        instance = null;
        if (socketHandler != null)
            Destroy(socketHandler.gameObject);
    }

    private void Start() {
        SetBackGround();
        BgmController.BgmEnum soundTrack =  BgmController.BgmEnum.CITY;
        SoundManager.Instance.bgmController.PlaySoundTrack(soundTrack);
    }

    protected void ReadCsvFile() {
        string pathToCsv = string.Empty;
        string language = AccountManager.Instance.GetLanguageSetting();

        if (gameScriptData == null)
            gameScriptData = new Dictionary<string, string>();

        if (Application.platform == RuntimePlatform.Android) {
            pathToCsv = Application.persistentDataPath + "/" + fileName;
        }
        else if (Application.platform == RuntimePlatform.IPhonePlayer) {
            pathToCsv = Application.persistentDataPath + "/" + fileName;
        }
        else {
            pathToCsv = Application.streamingAssetsPath + "/" + fileName;
        }

        var lines = File.ReadLines(pathToCsv);

        foreach (string line in lines) {
            if (line == null) continue;

            var _line = line;
            _line = line.Replace("\"", "");
            int splitPos = _line.IndexOf(',');
            string[] datas = new string[2];

            datas[0] = _line.Substring(0, splitPos);
            datas[1] = _line.Substring(splitPos + 1);
            gameScriptData.Add(datas[0], datas[1]);
        }
    }

    protected void ReadUICsvFile() {
        string pathToCsv = string.Empty;

        if (uiLocalizeData == null)
            uiLocalizeData = new Dictionary<string, string>();

        if (Application.platform == RuntimePlatform.Android) {
            pathToCsv = Application.persistentDataPath + "/" + ui_FileName;
        }
        else if (Application.platform == RuntimePlatform.IPhonePlayer) {
            pathToCsv = Application.persistentDataPath + "/" + ui_FileName;
        }
        else {
            pathToCsv = Application.streamingAssetsPath + "/" + ui_FileName;
        }

        var lines = File.ReadLines(pathToCsv);

        foreach (string line in lines) {
            if (line == null) continue;

            var datas = line.Split(',');
            uiLocalizeData.Add(datas[0], datas[1]);
        }
    }

    public void RefreshScript() {
        ReadCsvFile();
    }


    public void SyncPlayerHp() {
        if (socketHandler.gameState != null) {
            SocketFormat.Players socketStat = socketHandler.gameState.players;
            PlayerSetHPData((player.isHuman == true) ? socketStat.human.hero.hp : socketStat.orc.hero.hp,
                         (enemyPlayer.isHuman == true) ? socketStat.human.hero.hp : socketStat.orc.hero.hp);
        }
        else
            PlayerSetHPData(20, 20);
    }

    public void SetGameData() {
        string match = PlayerPrefs.GetString("BattleMode");
        InitGameData(match);
    }

    //최초에 데이터를 불러드릴 함수. missionData를 임시로 놓고, 그 후에 게임의 정보들 등록
    //승리목표 설정 -> 자원분배 -> 턴
    protected void InitGameData(string match = null) {
        ObservePlayerData();
        SetVictoryCondition(match);
        DistributeResource();
        InitTurnTable();
    }

    //승리 조건을 설정할 함수. victoryModule이라는 namespace로 전략패턴으로 구현 계획
    private void SetVictoryCondition(string data = null) {
        string condition = data;

        switch (condition) {
            case "Protect":
                matchRule = gameObject.AddComponent<victoryModule.ProtectObject>();
                matchRule.player = player;
                matchRule.enemyPlayer = enemyPlayer;
                if (ScenarioGameManagment.scenarioInstance != null && ScenarioGameManagment.scenarioInstance.isTutorial)
                    break;
                else
                    matchRule.SetCondition();
                break;
            default:
                matchRule = gameObject.AddComponent<victoryModule.Annihilation_Match>();
                matchRule.player = player;
                matchRule.enemyPlayer = enemyPlayer;

                if (ScenarioGameManagment.scenarioInstance != null && ScenarioGameManagment.scenarioInstance.isTutorial)
                    break;
                else
                    matchRule.SetCondition();
                break;
        }
    }

    public void ObservePlayerData() {
        player.SetPlayerStat();
        enemyPlayer.SetPlayerStat();
    }

    public void PlayerSetHPData(int playerHP, int enemyHP) {
        player.SetHP(playerHP);
        enemyPlayer.SetHP(enemyHP);
    }



    private void Update() {
        if(heroShieldActive) return;
        if (!infoOn && Input.GetMouseButtonDown(0)) {
            cardInfoCanvas.GetChild(0).GetComponent<CardListManager>().OpenUnitInfoWindow(Input.mousePosition);
        }
    }

    protected void SetWorldScale() {
        float ratio = (float)Screen.width / Screen.height;

        if (ratio < (float)1080 / 1920)
            ingameCamera.orthographicSize = ingameCamera.orthographicSize * (((float)1080 / 1920) / ratio);

        player.transform.position = backGround.transform.Find("PlayerPosition").Find("Player_1Pos").position;
        player.wallPosition = backGround.transform.Find("PlayerPosition").Find("Player_1Wall").position;
        player.unitClosePosition = backGround.transform.Find("PlayerPosition").Find("Player_1Close").position;

        enemyPlayer.transform.position = backGround.transform.Find("PlayerPosition").Find("Player_2Pos").position;
        enemyPlayer.wallPosition = backGround.transform.Find("PlayerPosition").Find("Player_2Wall").position;
        enemyPlayer.unitClosePosition = backGround.transform.Find("PlayerPosition").Find("Player_2Close").position;

        player.backLine.transform.position = backGround.transform.Find("Line_3").Find("BackSlot").position;
        player.frontLine.transform.position = backGround.transform.Find("Line_3").Find("FrontSlot").position;
        enemyPlayer.backLine.transform.position = backGround.transform.Find("Line_3").Find("EnemyBackSlot").position;
        enemyPlayer.frontLine.transform.position = backGround.transform.Find("Line_3").Find("EnemyFrontSlot").position;
    }


    protected virtual void SetBackGround() {
        GameObject raceSprite;
        if (player.isHuman == true) {
            raceSprite = Instantiate(AccountManager.Instance.resource.raceUiPrefabs["HUMAN_BACKGROUND"][0], backGround.transform);
            raceSprite.transform.SetAsLastSibling();
        }
        else {
            raceSprite = Instantiate(AccountManager.Instance.resource.raceUiPrefabs["ORC_BACKGROUND"][0], backGround.transform);
            raceSprite.transform.SetAsLastSibling();
        }
        lineMaskObject = backGround.transform.Find("field_mask").gameObject;
        backGroundTillObject = backGround.transform.Find("till").gameObject;

        backGroundTillObject.transform.Find("upkeep").gameObject.GetComponent<SpriteRenderer>().sprite
            = raceSprite.transform.Find("upkeep").gameObject.GetComponent<SpriteRenderer>().sprite;
        backGroundTillObject.transform.Find("upBackGround").gameObject.GetComponent<SpriteRenderer>().sprite
            = raceSprite.transform.Find("upBackGround").gameObject.GetComponent<SpriteRenderer>().sprite;
    }


    public void SetPlayerCard() {
        GameObject enemyCard;

        if (player.isHuman == true) {
            enemyCard = Resources.Load("Prefabs/OrcBackCard") as GameObject;
            player.back = cardDB.transform.Find("HumanBackCard").gameObject;
            enemyPlayer.back = cardDB.transform.Find("OrcBackCard").gameObject;
        }
        else {
            enemyCard = Resources.Load("Prefabs/HumanBackCard") as GameObject;
            player.back = cardDB.transform.Find("OrcBackCard").gameObject;
            enemyPlayer.back = cardDB.transform.Find("HumanBackCard").gameObject;
        }
    }

    public void DistributeResource() {
        SocketFormat.Player socketPlayer = (player.isHuman) ? socketHandler.gameState.players.human : socketHandler.gameState.players.orc;
        SocketFormat.Player socketEnemyPlayer = (enemyPlayer.isHuman) ? socketHandler.gameState.players.human : socketHandler.gameState.players.orc;

        SoundManager.Instance.PlayIngameSfx(IngameSfxSound.GETMANA);
        player.resource.Value = socketPlayer.resource;
        enemyPlayer.resource.Value = socketEnemyPlayer.resource;
    }

    public void OnBlockPanel(string msg) {
        blockPanel.SetActive(true);
        blockPanel
            .transform
            .GetChild(0)
            .GetComponent<TMPro.TextMeshProUGUI>()
            .text = msg;
    }

    public void OffBlockPanel() {
        blockPanel.SetActive(false);
    }

    public virtual IEnumerator EnemyUseCard(SocketFormat.PlayHistory history, DequeueCallback callback) {
        #region socket use Card
        if (history != null) {
            if (history.cardItem.type.CompareTo("unit") == 0) {
                //카드 정보 만들기
                GameObject summonUnit = MakeUnitCardObj(history);
                //카드 정보 보여주기
                yield return UnitActivate(history);
            }
            else {
                GameObject summonedMagic = MakeMagicCardObj(history);
                summonedMagic.GetComponent<MagicDragHandler>().isPlayer = false;
                yield return MagicActivate(summonedMagic, history);
            }
            SocketFormat.DebugSocketData.SummonCardData(history);
        }
        enemyPlayer.UpdateCardCount();
        yield return new WaitForSeconds(0.5f);
        #endregion
        SocketFormat.DebugSocketData.ShowHandCard(socketHandler.gameState.players.enemyPlayer(enemyPlayer.isHuman).deck.handCards);
        yield return waitSkillDone();
        callback();
    }

    public IEnumerator waitSkillDone() {
        MagicDragHandler[] list = Resources.FindObjectsOfTypeAll<MagicDragHandler>();
        foreach(MagicDragHandler magic in list) {
            if(magic.skillHandler == null) continue;
            
            if(!(magic.skillHandler.finallyDone && magic.skillHandler.socketDone)) {
                yield return new WaitUntil(() => magic.skillHandler.finallyDone && magic.skillHandler.socketDone);
            }
        }
        PlaceMonster[] list2 = FindObjectsOfType<PlaceMonster>();
        foreach(PlaceMonster unit in list2) {
            if(unit.skillHandler == null) continue;
            
            if(!(unit.skillHandler.finallyDone && unit.skillHandler.socketDone)) {
                yield return new WaitUntil(() => unit.skillHandler.finallyDone && unit.skillHandler.socketDone);
            }
        }
    }

    /// <summary>
    /// 마법 카드생성(비활성화 상태로 생성)
    /// </summary>
    /// <param name="history"></param>
    /// <returns></returns>
    protected GameObject MakeMagicCardObj(SocketFormat.PlayHistory history) {
        dataModules.CollectionCard cardData = AccountManager.Instance.allCardsDic[history.cardItem.id];

        GameObject magicCard = player.cdpm.InstantiateMagicCard(cardData, history.cardItem.itemId);
        magicCard.GetComponent<MagicDragHandler>().itemID = history.cardItem.itemId;

        Logger.Log("use Magic Card" + history.cardItem.name);
        if (!heroShieldActive)
            enemyPlayer.resource.Value -= cardData.cost;

        Destroy(enemyPlayer.latestCardSlot.GetChild(0).gameObject);

        return magicCard;
    }

    /// <summary>
    /// 유닛 카드생성(비활성화 상태로 생성)
    /// </summary>
    /// <param name="history"></param>
    /// <returns></returns>
    protected GameObject MakeUnitCardObj(SocketFormat.PlayHistory history) {
        dataModules.CollectionCard cardData = AccountManager.Instance.allCardsDic[history.cardItem.id];

        GameObject unitCard = player.cdpm.InstantiateUnitCard(cardData, history.cardItem.itemId);
        unitCard.GetComponent<UnitDragHandler>().itemID = history.cardItem.itemId;

        Destroy(enemyPlayer.latestCardSlot.GetChild(0).gameObject);
        return unitCard;
    }

    protected IEnumerator UnitActivate(SocketFormat.PlayHistory history) {
        GameObject summonedMonster = SummonMonster(history);
        summonedMonster.GetComponent<PlaceMonster>().isPlayer = false;

        object[] parms = new object[] { false, summonedMonster };
        EventHandler.PostNotification(IngameEventHandler.EVENT_TYPE.END_CARD_PLAY, this, parms);
        EventHandler.PostNotification(IngameEventHandler.EVENT_TYPE.FIELD_CHANGED, null, null);
        yield return 0;
    }

    protected IEnumerator MagicActivate(GameObject card, SocketFormat.PlayHistory history) {
        MagicDragHandler magicCard = card.GetComponent<MagicDragHandler>();
        magicCard.skillHandler.socketDone = false;
        dragable = false;
        //카드 등장 애니메이션
        card.transform.rotation = new Quaternion(0, 0, 540, card.transform.rotation.w);
        card.transform.SetParent(enemyPlayer.playerUI.transform);
        card.SetActive(true);
        Logger.Log(enemyPlayer.playerUI.transform.position);
        yield return new WaitForSeconds(1.0f);
        //타겟 지정 애니메이션
        yield return cardHandManager.ShowUsedCard(100, card);
        yield return EnemySettingTarget(history.targets[0], magicCard);
        SoundManager.Instance.PlayMagicSound(magicCard.cardData.id);
        //실제 카드 사용
        object[] parms = new object[] { false, card };
        if (magicCard.cardData.isHeroCard == true) {
            card.transform.Find("GlowEffect").gameObject.SetActive(false);
            card.transform.Find("Portrait").gameObject.SetActive(false);
            card.transform.Find("BackGround").gameObject.SetActive(false);
            card.transform.Find("Cost").gameObject.SetActive(false);
            enemyPlayer.UpdateCardCount();
            yield return EffectSystem.Instance.HeroCutScene(enemyPlayer.heroID);
        }
        EventHandler.PostNotification(IngameEventHandler.EVENT_TYPE.END_CARD_PLAY, this, parms);
        yield return new WaitForSeconds(1f);
        
        //카드 파괴
        card.transform.localScale = new Vector3(1, 1, 1);
        cardHandManager.DestroyCard(card);
        card.GetComponent<MagicDragHandler>().skillHandler.RemoveTriggerEvent();
    }

    private IEnumerator EnemySettingTarget(SocketFormat.Target target, MagicDragHandler magicHandler) {
        GameObject highlightUI = null;
        string[] args = magicHandler.skillHandler.targetArgument();
        switch (target.method) {
            case "place":
                //target.args[0] line, args[1] camp, args[2] front or rear
                //근데 마법으로 place는 안나올 듯 패스!
                break;
            case "unit":
                string itemId = target.args[0];
                List<GameObject> list = UnitsObserver.GetAllFieldUnits();
                GameObject unit = list.Find(x => x.GetComponent<PlaceMonster>().itemId.CompareTo(itemId) == 0);
                highlightUI = unit.transform.Find("ClickableUI").gameObject;
                highlightUI.SetActive(true);
                break;
            case "line":
                int line = int.Parse(target.args[0]);
                for (int i = 0; i < 5; i++) {
                    if (i != line) continue;
                    highlightUI = PlayMangement.instance.backGround.transform.GetChild(i).Find("BattleLineEffect").gameObject;
                    break;
                }
                break;
            case "all":
                //보여줄게 없음
                break;
            case "camp":
                //보여줄게 없음
                break;
        }
        if (highlightUI == null) yield break;
        highlightUI.SetActive(true);
        magicHandler.highlightedSlot = highlightUI.transform;

        if (highlightUI.GetComponent<SpriteRenderer>() != null)
            highlightUI.GetComponent<SpriteRenderer>().color = new Color(1, 0, 0, 155.0f / 255.0f);
        yield return CardInfoOnDrag.instance.MoveCrossHair(magicHandler.gameObject, highlightUI.transform);
        if (highlightUI.GetComponent<SpriteRenderer>() != null)
            highlightUI.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 155.0f / 255.0f);
        highlightUI.SetActive(false);
    }

    /// <summary>
    /// 적의 소환
    /// </summary>
    /// <param name="history"></param>
    /// <returns></returns>
    private GameObject SummonMonster(SocketFormat.PlayHistory history) {
        int i = int.Parse(history.targets[0].args[0]);
        string id = history.cardItem.id;
        bool isFront = history.targets[0].args[2].CompareTo("front") == 0;

        bool unitExist = UnitsObserver.IsUnitExist(new FieldUnitsObserver.Pos(i, 0), !player.isHuman);
        int j = isFront && unitExist ? 1 : 0;
        if (unitExist && !isFront) {
            Transform line_rear = enemyPlayer.transform.GetChild(0);
            Transform line_front = enemyPlayer.transform.GetChild(1);
            Transform existUnit;
            existUnit = line_rear.GetChild(i).GetChild(0);
            existUnit.GetComponent<PlaceMonster>().unitLocation = line_front.GetChild(i).position;
            UnitsObserver.UnitChangePosition(existUnit.gameObject, new FieldUnitsObserver.Pos(i, 1), player.isHuman);
        }
        GameObject monster = SummonUnit(false, id, i, j, history.cardItem.itemId);
        eventHandler.PostNotification(IngameEventHandler.EVENT_TYPE.ENEMY_SUMMON_UNIT, this, id);
        return monster;
    }

    public void StartBattle(string attacker ,string[] affectedList, DequeueCallback battleEndCall) {
        //
        StartCoroutine(ExecuteBattle(attacker, affectedList, battleEndCall));
    }

    public bool passOrc() {
        string turnName = socketHandler.gameState.gameState;
        if (turnName.CompareTo("orcPostTurn") == 0) return true;
        if (turnName.CompareTo("battleTurn") == 0) return true;
        if (turnName.CompareTo("shieldTurn") == 0) return true;
        if (turnName.CompareTo("endGame") == 0) return true;
        return false;
    }

    protected IEnumerator ExecuteBattle(string attackerPos, string[] affectedList, DequeueCallback battleEndCall) {
        yield return StopBattleLine();        
        FieldUnitsObserver observer = GetComponent<FieldUnitsObserver>();       

        GameObject attackUnitObject = observer.GetAttacker(attackerPos);
        PlaceMonster attacker = attackUnitObject.GetComponent<PlaceMonster>();

        List<GameObject> AffectedList = observer.GetAfftecdList(attacker.unit.ishuman, affectedList);


        attacker.GetTarget(AffectedList);
        yield return new WaitForSeconds(0.8f + attacker.atkTime);


        battleEndCall();
    }

    IEnumerator battleUnit(List<GameObject> unitList, bool secondAttack) {
        PlaceMonster placeMonster;
        for(int i = 0; i < unitList.Count; i++) {
            placeMonster = unitList[i].GetComponent<PlaceMonster>();

            if (secondAttack == true && placeMonster.CanMultipleAttack == false)
                continue;

            if (placeMonster.unit.attack > 0) {
                //placeMonster.GetTarget();
                yield return new WaitForSeconds(0.8f + placeMonster.atkTime);
            }
            else
                yield return new WaitForSeconds(0.01f);
        }
    }

    protected IEnumerator StopBattleLine() {
        yield return new WaitUntil(() => stopBattle == false);
    }

    protected IEnumerator StopTurn() {
        yield return new WaitUntil(() => stopTurn == false);
    }

    protected IEnumerator BeginStopTurn() {
        yield return new WaitUntil(() => beginStopTurn == false);
    }

    protected IEnumerator AfterStopTurn() {
        yield return new WaitUntil(() => afterStopTurn == false);
    }

    public void SetBattleLineColor(bool isBattle, int line = -1) {
        battleLineEffect = (line != -1) ? backGround.transform.GetChild(line).Find("BattleLineEffect") : battleLineEffect;
        if (isBattle == true) {
            battleLineEffect.gameObject.SetActive(true);
            battleLineEffect.GetComponent<SpriteRenderer>().color = new Color(1, 0.545f, 0.427f, 0.6f);
        }
        else {
            battleLineEffect.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 0.6f);
            battleLineEffect.gameObject.SetActive(false);
        }
    }

    public void CheckLine(int line, bool isSecond , DequeueCallback clearCall) {
        CheckMonsterStatus(player.backLine.transform.GetChild(line));
        CheckMonsterStatus(player.frontLine.transform.GetChild(line));
        CheckMonsterStatus(enemyPlayer.backLine.transform.GetChild(line));
        CheckMonsterStatus(enemyPlayer.frontLine.transform.GetChild(line));

        if (isSecond == true)
            SetBattleLineColor(false, line);

        clearCall();

        if (isSecond == true && line >= 4)
            socketHandler.TurnOver();
    }

    private void CheckMonsterStatus(Transform monsterTransform) {
        if (monsterTransform.childCount == 0) return;

        PlaceMonster monster = monsterTransform.GetChild(0).GetComponent<PlaceMonster>();
        monster.CheckHP();
    }

    public IEnumerator WaitDrawHeroCard() {
        yield return new WaitUntil(() => messageCallBack != null);
        yield return new WaitUntil(() => waitDraw == false);
    }


    public IEnumerator DrawSpecialCard(bool isHuman) {
        yield return WaitDrawHeroCard();
        Logger.Log("쉴드 발동!");
        bool isPlayer = (isHuman == player.isHuman);
        if (isPlayer) {
            CardHandManager cdpm = FindObjectOfType<CardHandManager>();
            bool race = player.isHuman;
            SocketFormat.Card[] heroCards = socketHandler.gameState.players.myPlayer(race).deck.heroCards;
            SetBattleLineColor(false);
            yield return cdpm.DrawHeroCard(heroCards);
            SetBattleLineColor(true);
        }
        else {
            GameObject enemyCard;
            if (enemyPlayer.isHuman)
                enemyCard = Instantiate(Resources.Load("Prefabs/HumanBackCard") as GameObject, enemyPlayer.EmptyCardSlot);
            else
                enemyCard = Instantiate(Resources.Load("Prefabs/OrcBackCard") as GameObject, enemyPlayer.EmptyCardSlot);
            enemyCard.transform.localScale = new Vector3(1, 1, 1);
            enemyCard.transform.localPosition = new Vector3(0, 0, 0);
            enemyCard.SetActive(false);
            enemyPlayer.UpdateCardCount();
            IngameNotice.instance.SelectNotice();
        }
        yield return new WaitForSeconds(1f);
        if (isPlayer) socketHandler.TurnOver();
        heroShieldActive = false;
        UnlockTurnOver();
        if (!isPlayer) enemyPlayer.ConsumeShieldStack();
        ExecuteAfterMessage();
    }

    public IEnumerator WaitShieldDone() {
        do {
            yield return new WaitForFixedUpdate();
            //스킬 사용
            if(true) {
                IngameNotice.instance.CloseNotice();
                SocketFormat.PlayHistory history =  socketHandler.gameState.lastUse;
                if (history != null) {
                    GameObject summonedMagic = MakeMagicCardObj(history);
                    summonedMagic.GetComponent<MagicDragHandler>().isPlayer = false;
                    yield return MagicActivate(summonedMagic, history);
                    SocketFormat.DebugSocketData.SummonCardData(history);
                    //int count = CountEnemyCard();
                    //enemyPlayer.playerUI.transform.Find("CardCount").GetChild(0).gameObject.GetComponent<Text>().text = (count).ToString();
                    yield return new WaitForSeconds(1f);
                }
            }
        } while (heroShieldDone.Count == 0);
        heroShieldDone.RemoveAt(0);
        IngameNotice.instance.CloseNotice();
    }

    public void SocketAfterMessage(DequeueCallback call) {
        messageCallBack = call;
    }

    public void ExecuteAfterMessage() {
        messageCallBack();
        messageCallBack -= messageCallBack;
    }



    public void OnMoveSceneBtn() {
        FBL_SceneManager.Instance.LoadScene(FBL_SceneManager.Scene.MAIN_SCENE);
    }
}

/// <summary>
/// 유닛 소환관련 처리
/// </summary>
public partial class PlayMangement {
    public GameObject SummonUnit(bool isPlayer, string unitID, int col, int row, string itemID = null, int cardIndex = -1, Transform[][] args = null, bool isFree = false) {
        PlayerController targetPlayer = (isPlayer == true) ? player : enemyPlayer;
        if (unitsObserver.IsUnitExist(new FieldUnitsObserver.Pos(col, row), targetPlayer.isHuman) == true)
            return null;

        CardDataPackage cardDataPackage = AccountManager.Instance.cardPackage;

        GameObject skeleton;
        dataModules.CollectionCard cardData;
        cardData = AccountManager.Instance.allCardsDic[unitID];

        Logger.Log(col);
        Logger.Log(row);

        GameObject unit = Instantiate(baseUnit, targetPlayer.transform.GetChild(row).GetChild(col));
        unit.transform.position = targetPlayer.transform.GetChild(row).GetChild(col).position;
        PlaceMonster placeMonster = unit.GetComponent<PlaceMonster>();
        EventHandler.PostNotification(IngameEventHandler.EVENT_TYPE.UNIT_SUMMONED, this, unitID);


        placeMonster.isPlayer = isPlayer;
        placeMonster.itemId = itemID;
        placeMonster.unit.name = cardData.name;
        placeMonster.unit.hp = (int)cardData.hp;
        placeMonster.unit.currentHp = (int)cardData.hp;
        placeMonster.unit.originalAttack = (int)cardData.attack;
        placeMonster.unit.attack = (int)cardData.attack;
        placeMonster.unit.type = cardData.type;
        placeMonster.unit.attackRange = cardData.attackRange;
        placeMonster.unit.cost = cardData.cost;
        placeMonster.unit.rarelity = cardData.rarelity;
        placeMonster.unit.cardId = cardData.id;
        placeMonster.unit.attributes = cardData.attributes;


        if (cardData.cardCategories.Length > 1) {
            placeMonster.unit.cardCategories = new string[2];
            placeMonster.unit.cardCategories[0] = cardData.cardCategories[0];
            placeMonster.unit.cardCategories[1] = cardData.cardCategories[1];
        }
        else if(cardData.cardCategories.Length > 0) {
            placeMonster.unit.cardCategories = new string[1];
            placeMonster.unit.cardCategories[0] = cardData.cardCategories[0];
        }

        if (cardData.attackTypes.Length != 0) {
            placeMonster.unit.attackTypes = new string[cardData.attackTypes.Length];
            placeMonster.unit.attackTypes = cardData.attackTypes;
        }

        skeleton = Instantiate(AccountManager.Instance.resource.cardSkeleton[unitID], placeMonster.transform);
        skeleton.name = "skeleton";
        skeleton.transform.localScale = (isPlayer == true) ? new Vector3(-1, 1, 1) : new Vector3(1, 1, 1);
        placeMonster.name = cardData.name;

        placeMonster.Init(cardData);
        placeMonster.SpawnUnit();
        if(!isFree) targetPlayer.resource.Value -= cardData.cost;
        var observer = UnitsObserver;

        if (isPlayer) {
            player.isPicking.Value = false;
            if (player.isHuman)
                player.ActivePlayer();
            else
                player.ActiveOrcTurn();
            if (args != null)
                observer.RefreshFields(args, player.isHuman);
            else
                observer.UnitAdded(unit, new FieldUnitsObserver.Pos(col, row), player.isHuman);

            if(cardIndex != -1)
                player.cdpm.DestroyCard(cardIndex);
            if(isFree) cardInfoCanvas.GetChild(0).GetComponent<CardListManager>().AddFeildUnitInfo(0, placeMonster.myUnitNum, cardData);
            //if(placeMonster.unit.id.Contains("qc")) cardInfoCanvas.GetChild(0).GetComponent<CardListManager>().AddFeildUnitInfo(0, placeMonster.myUnitNum, cardData);
        }
        else {
            Destroy(enemyPlayer.latestCardSlot.GetChild(0).gameObject);
            enemyPlayer.UpdateCardCount();

            SkillModules.SkillHandler skillHandler = new SkillModules.SkillHandler();
            skillHandler.Initialize(cardData.skills, unit, false);
            unit.GetComponent<PlaceMonster>().skillHandler = skillHandler;
            cardInfoCanvas.GetChild(0).GetComponent<CardListManager>().AddFeildUnitInfo(0, placeMonster.myUnitNum, cardData);
            observer.UnitAdded(unit, new FieldUnitsObserver.Pos(col, row), enemyPlayer.isHuman);
            //observer.RefreshFields(args, enemyPlayer.isHuman);
            unit.layer = 14;
        }
        
        targetPlayer.PlayerUseCard();
        return unit;
    }
}


/// <summary>
/// 카드 드로우 작동
/// </summary>
public partial class PlayMangement {

    public IEnumerator GenerateCard(DequeueCallback callback) {
        int i = 0;
        while (i < 4) {
            yield return new WaitForSeconds(0.3f);
            if (i < 4)
                StartCoroutine(player.cdpm.FirstDraw());

            GameObject enemyCard;
            if (enemyPlayer.isHuman)
                enemyCard = Instantiate(Resources.Load("Prefabs/HumanBackCard") as GameObject, enemyPlayer.EmptyCardSlot);
            else
                enemyCard = Instantiate(Resources.Load("Prefabs/OrcBackCard") as GameObject, enemyPlayer.EmptyCardSlot);
            enemyCard.transform.position = player.cdpm.cardSpawnPos.position;
            enemyCard.transform.localScale = new Vector3(1, 1, 1);
            iTween.MoveTo(enemyCard, enemyCard.transform.parent.position, 0.3f);
            yield return new WaitForSeconds(0.3f);
            SoundManager.Instance.PlayIngameSfx(IngameSfxSound.CARDDRAW);
            enemyCard.SetActive(false);
            enemyPlayer.UpdateCardCount();
            i++;
        }
        callback();
    }

    public IEnumerator StoryDrawEnemyCard() {
        int length = socketHandler.gameState.players.enemyPlayer(enemyPlayer.isHuman).deck.handCards.Length;
        for(int i = 0; i < length; i++) {
            GameObject enemyCard;
            if (enemyPlayer.isHuman)
                enemyCard = Instantiate(Resources.Load("Prefabs/HumanBackCard") as GameObject, enemyPlayer.EmptyCardSlot);
            else
                enemyCard = Instantiate(Resources.Load("Prefabs/OrcBackCard") as GameObject, enemyPlayer.EmptyCardSlot);
            enemyCard.transform.position = player.cdpm.cardSpawnPos.position;
            enemyCard.transform.localScale = new Vector3(1, 1, 1);
            iTween.MoveTo(enemyCard, enemyCard.transform.parent.position, 0.3f);
            yield return new WaitForSeconds(0.15f);
            enemyCard.SetActive(false);
            SoundManager.Instance.PlayIngameSfx(IngameSfxSound.CARDDRAW);
            enemyPlayer.playerUI.transform.Find("CardNum").GetChild(0).gameObject.GetComponent<Text>().text = (i + 1).ToString();
        }
    }

    public virtual void EndTurnDraw() {
        if (isGame == false) return;
        bool race = player.isHuman;
        SocketFormat.Card cardData = socketHandler.gameState.players.myPlayer(race).newCard;
        player.cdpm.AddCard(null, cardData);
        if(enemyPlayer.CurrentCardCount >= 10) return;
        GameObject enemyCard;
        if (enemyPlayer.isHuman)
            enemyCard = Instantiate(Resources.Load("Prefabs/HumanBackCard") as GameObject, enemyPlayer.EmptyCardSlot);
        else
            enemyCard = Instantiate(Resources.Load("Prefabs/OrcBackCard") as GameObject, enemyPlayer.EmptyCardSlot);
        enemyCard.transform.position = player.cdpm.cardSpawnPos.position;
        enemyCard.transform.localScale = new Vector3(1, 1, 1);
        iTween.MoveTo(enemyCard, enemyCard.transform.parent.position, 0.3f);
        SoundManager.Instance.PlayIngameSfx(IngameSfxSound.CARDDRAW);
        enemyCard.SetActive(false);
        enemyPlayer.UpdateCardCount();
    }

    public IEnumerator EnemyMagicCardDraw(int drawNum) {
        int total = enemyPlayer.CurrentCardCount + drawNum;
        if(total > 10) drawNum = drawNum - (total - 10);
        for(int i = 0 ; i < drawNum; i++) {
            GameObject enemyCard;
            if (enemyPlayer.isHuman)
                enemyCard = Instantiate(Resources.Load("Prefabs/HumanBackCard") as GameObject, enemyPlayer.EmptyCardSlot);
            else
                enemyCard = Instantiate(Resources.Load("Prefabs/OrcBackCard") as GameObject, enemyPlayer.EmptyCardSlot);
            enemyCard.transform.position = player.cdpm.cardSpawnPos.position;
            enemyCard.transform.localScale = new Vector3(1, 1, 1);
            SoundManager.Instance.PlayIngameSfx(IngameSfxSound.CARDDRAW);
            iTween.MoveTo(enemyCard, enemyCard.transform.parent.position, 0.3f);
            enemyCard.SetActive(false);
            enemyPlayer.UpdateCardCount();
            yield return new WaitForSeconds(0.3f);
        }
    }
}

/// <summary>
/// 카메라 처리
/// </summary>

public partial class PlayMangement {
    public Camera ingameCamera;
    public Vector3 cameraPos;

    public void SetCamera() {
        cameraPos = Camera.main.transform.position;
        cameraSize = Camera.main.orthographicSize;
    }

    public IEnumerator cameraShake(float time, int power) {
        float timer = 0;
        float cameraSize = ingameCamera.orthographicSize;
        while (timer <= time) {


            ingameCamera.transform.position = (Vector3)Random.insideUnitCircle * 0.1f * power + cameraPos;

            timer += Time.deltaTime;
            yield return null;
        }
        ingameCamera.orthographicSize = cameraSize;
        ingameCamera.transform.position = cameraPos;
    }


}




/// <summary>
/// Socket 관련 처리
/// </summary>
public partial class PlayMangement {
    [SerializeField] protected IngameEventHandler eventHandler;
    public IngameEventHandler EventHandler {
        get {
            return eventHandler;
        }
    }

    public BattleConnector socketHandler;
    public BattleConnector SocketHandler {
        get {
            return socketHandler;
        }
        private set {
            socketHandler = value;
        }
    }

    [SerializeField] FieldUnitsObserver unitsObserver;
    public FieldUnitsObserver UnitsObserver {
        get {
            return unitsObserver;
        }
    }
}

public partial class PlayMangement {
    [SerializeField] Transform turnTable;
    public GameObject releaseTurnBtn;
    private GameObject nonplayableTurnArrow;
    private GameObject playableTurnArrow;
    private Transform turnIcon;

    public void InitTurnTable() {
        string race = PlayerPrefs.GetString("SelectedRace").ToLower();
        bool isHuman;

        if (race == "human") isHuman = true;
        else isHuman = false;
        if (isHuman) {
            releaseTurnBtn = turnTable.Find("HumanButton").gameObject;
        }
        else {
            releaseTurnBtn = turnTable.Find("OrcButton").gameObject;
        }

        Debug.Log("isHuman" + isHuman);
    }

    public void LockTurnOver() {
        releaseTurnBtn.GetComponent<Button>().enabled = false;
    }

    public void UnlockTurnOver() {
        releaseTurnBtn.GetComponent<Button>().enabled = true;
    }

    public void OnNoCostEffect(bool turnOn) {
        releaseTurnBtn.transform.Find("ResourceOut").gameObject.SetActive(turnOn);
    }
}

/// <summary>
/// 지형처리
/// </summary>
public partial class PlayMangement {
    public enum LineState {
        hill,
        flat,
        forest,
        water
    }
}