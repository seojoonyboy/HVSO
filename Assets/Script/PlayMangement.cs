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
    public Transform battleLineEffect;
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
    public SkeletonGraphic playerMana, enemyMana;
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
    public string currentTurn;

    

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
    //public string magicHistroy;

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


    public GameObject textCanvas;


    private void Awake() {
        socketHandler = FindObjectOfType<BattleConnector>();
        bool isTest = PlayerPrefs.GetString("SelectedBattleType").CompareTo("test") == 0;
        SetWorldScale();
        ReadUICsvFile();
        instance = this;
        
        GetComponent<TurnMachine>().onTurnChanged.AddListener(ChangeTurn);
        if (!isTest) GetComponent<TurnMachine>().onPrepareTurn.AddListener(DistributeCard);
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
        //SetPlayerCard();
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
            PlayerSetHPData((player.isHuman == true) ? socketStat.human.hero.currentHp : socketStat.orc.hero.currentHp,
                         (enemyPlayer.isHuman == true) ? socketStat.human.hero.currentHp : socketStat.orc.hero.currentHp);
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
        //if (Input.GetKeyDown(KeyCode.Escape)) {
        //    FBL_SceneManager.Instance.LoadScene(FBL_SceneManager.Scene.MAIN_SCENE);
        //}
        if(heroShieldActive) return;
        if (!infoOn && Input.GetMouseButtonDown(0)) {
            cardInfoCanvas.GetChild(0).GetComponent<CardListManager>().OpenUnitInfoWindow(Input.mousePosition);
        }
    }

    protected void SetWorldScale() {

        //SpriteRenderer backSprite = backGround.GetComponent<SpriteRenderer>();
        float ratio = (float)Screen.width / Screen.height;

        if (ratio < (float)1080 / 1920)
            ingameCamera.orthographicSize = ingameCamera.orthographicSize * (((float)1080 / 1920) / ratio);

        //canvas.transform.Find("FirstDrawWindow").GetComponent<RectTransform>().sizeDelta = new Vector2(Screen.width, Screen.height);
        //cardInfoCanvas.transform.Find("CardInfoList").GetComponent<RectTransform>().sizeDelta = new Vector2(Screen.width, Screen.height);

        //Rect temp = TargetCameraPos(Camera.main.orthographicSize);
        //tempCube.transform.TransformPoint(new Vector3(temp.x, temp.y, 0));
        //tempCube.transform.localScale = new Vector3(temp.width, temp.height, 1);
        //Vector3 canvasScale = canvas.transform.localScale;
        //canvas.GetComponent<RectTransform>().sizeDelta = new Vector2(Camera.main.pixelWidth, Camera.main.pixelHeight);
        //canvas.transform.localScale = new Vector3(width /Camera.main.pixelWidth, height/Camera.main.pixelHeight, 1);


        if (ratio > 1.77f) {
            //backgroundScale = width / backSprite.sprite.bounds.size.x;
            //backGround.transform.localScale = new Vector3(backgroundScale, backgroundScale, 1);
            //backGround.transform.localPosition = Vector3.zero;


        }
        else {
            //backgroundScale = height / backSprite.sprite.bounds.size.y;
            //backGround.transform.localScale = new Vector3(backgroundScale, backgroundScale, 1); ;
            //backGround.transform.localPosition = new Vector3(0, -0.5f, 0f);

        }
        //backGround.transform.localPosition = new Vector3(0, -0.45f, 0f);
        player.transform.position = backGround.transform.Find("PlayerPosition").Find("Player_1Pos").position;
        player.wallPosition = backGround.transform.Find("PlayerPosition").Find("Player_1Wall").position;
        player.unitClosePosition = backGround.transform.Find("PlayerPosition").Find("Player_1Close").position;

        enemyPlayer.transform.position = backGround.transform.Find("PlayerPosition").Find("Player_2Pos").position;
        enemyPlayer.wallPosition = backGround.transform.Find("PlayerPosition").Find("Player_2Wall").position;
        enemyPlayer.unitClosePosition = backGround.transform.Find("PlayerPosition").Find("Player_2Close").position;

        //for (int i = 0; i < player.frontLine.transform.childCount; i++) {
        //    player.backLine.transform.GetChild(i).position = new Vector3(backGround.transform.GetChild(i).position.x, player.backLine.transform.position.y, 0);
        //    player.frontLine.transform.GetChild(i).position = new Vector3(backGround.transform.GetChild(i).position.x, player.frontLine.transform.position.y, 0);
        //    enemyPlayer.backLine.transform.GetChild(i).position = new Vector3(backGround.transform.GetChild(i).position.x, enemyPlayer.backLine.transform.position.y, 0);
        //    enemyPlayer.frontLine.transform.GetChild(i).position = new Vector3(backGround.transform.GetChild(i).position.x, enemyPlayer.frontLine.transform.position.y, 0);
        //}

        player.backLine.transform.position = backGround.transform.Find("Line_3").Find("BackSlot").position;
        player.frontLine.transform.position = backGround.transform.Find("Line_3").Find("FrontSlot").position;
        enemyPlayer.backLine.transform.position = backGround.transform.Find("Line_3").Find("EnemyBackSlot").position;
        enemyPlayer.frontLine.transform.position = backGround.transform.Find("Line_3").Find("EnemyFrontSlot").position;

        //for (int i = 0; i < player.frontLine.transform.childCount; i++) {
        //    Vector3 pos = backGround.transform.GetChild(i).position;
        //    backGround.transform.GetChild(i).position = new Vector3(pos.x, player.backLine.transform.position.y, 0);
        //}
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
        //enemyPlayer.playerUI.transform.Find("CardCount").gameObject.GetComponent<Image>().sprite = enemyCard.GetComponent<Image>().sprite;
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

    public virtual IEnumerator EnemyUseCard(bool isBefore) {
        if (isBefore)
            yield return new WaitForSeconds(1.0f);

        yield return BeginStopTurn();
        #region socket use Card
        while (!socketHandler.cardPlayFinish()) {
            yield return socketHandler.useCardList.WaitNext();
            if (socketHandler.useCardList.allDone) break;
            SocketFormat.GameState state = socketHandler.getHistory();
            SocketFormat.PlayHistory history = state.lastUse;
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
                    /*
                    if (summonedMagic.GetComponent<MagicDragHandler>().cardData.hero_chk == true)
                        yield return EffectSystem.Instance.HeroCutScene(enemyPlayer.isHuman);
                        */
                    yield return MagicActivate(summonedMagic, history);
                }
                SocketFormat.DebugSocketData.SummonCardData(history);
            }
            int count = CountEnemyCard();
            enemyPlayer.playerUI.transform.Find("CardCount").GetChild(0).gameObject.GetComponent<Text>().text = (count).ToString();
            //SocketFormat.DebugSocketData.CheckMapPosition(state);
            yield return new WaitForSeconds(0.5f);
        }
        #endregion
        SocketFormat.DebugSocketData.ShowHandCard(socketHandler.gameState.players.enemyPlayer(enemyPlayer.isHuman).deck.handCards);

        yield return AfterStopTurn();
        if (isBefore)
            EventHandler.PostNotification(IngameEventHandler.EVENT_TYPE.END_TURN_BTN_CLICKED, this, GetComponent<TurnMachine>().CurrentTurn());
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

        Destroy(enemyPlayer.playerUI.transform.Find("CardSlot").GetChild(CountEnemyCard() - 1).GetChild(0).gameObject);

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

        Destroy(enemyPlayer.playerUI.transform.Find("CardSlot").GetChild(CountEnemyCard() - 1).GetChild(0).gameObject);
        return unitCard;
    }

    protected IEnumerator UnitActivate(SocketFormat.PlayHistory history) {
        //UnitDragHandler unitDragHandler = card.GetComponent<UnitDragHandler>();
        //dragable = false;

        //card.transform.rotation = new Quaternion(0, 0, 540, card.transform.rotation.w);
        //card.transform.SetParent(enemyPlayer.playerUI.transform);
        //card.SetActive(true);

        ////카드 보여주기
        //yield return cardHandManager.ShowUsedCard(100, card);
        ////카드 파괴
        //card.transform.localScale = new Vector3(1, 1, 1);
        //cardHandManager.DestroyCard(card);

        //실제 유닛 소환
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
            int count = CountEnemyCard();
            enemyPlayer.playerUI.transform.Find("CardCount").GetChild(0).gameObject.GetComponent<Text>().text = (count).ToString();


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
                int itemId = int.Parse(target.args[0]);
                List<GameObject> list = UnitsObserver.GetAllFieldUnits();
                GameObject unit = list.Find(x => x.GetComponent<PlaceMonster>().itemId == itemId);
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

    public void ChangeTurn() {
        if (isGame == false) return;
        player.buttonParticle.SetActive(false);
        currentTurn = GetComponent<TurnMachine>().CurrentTurn().ToString();
        Logger.Log(currentTurn);
        switch (currentTurn) {
            case "ORC":
                if (firstTurn) {
                    firstTurn = false;
                }
                else
                    turnSpine.AnimationState.SetAnimation(0, "1.orc_attack", false);
                playerMana.AnimationState.SetAnimation(0, "animation", false);
                enemyMana.AnimationState.SetAnimation(0, "animation", false);
                if (player.isHuman == false) {
                    player.ActiveOrcTurn();
                    enemyPlayer.DisablePlayer();
                }
                else {
                    player.DisablePlayer();
                    enemyPlayer.ActivePlayer();
                    enemyPlayer.PlayerThinking();
                    StartCoroutine(EnemyUseCard(true));
                }
                EventHandler.PostNotification(IngameEventHandler.EVENT_TYPE.BEGIN_ORC_PRE_TURN, this, null);
                break;

            case "HUMAN":
                turnSpine.AnimationState.SetAnimation(0, "2.human_attack", false);
                if (player.isHuman == true) {
                    player.ActivePlayer();
                    enemyPlayer.DisablePlayer();
                    enemyPlayer.PlayerThinkFinish();
                }
                else {
                    player.DisablePlayer();
                    enemyPlayer.ActivePlayer();
                    enemyPlayer.PlayerThinking();
                    StartCoroutine(EnemyUseCard(true));
                }
                EventHandler.PostNotification(IngameEventHandler.EVENT_TYPE.BEGIN_HUMAN_TURN, this, null);
                break;

            case "SECRET":
                turnSpine.AnimationState.SetAnimation(0, "3.orc_trick", false);
                if (player.isHuman == false) {
                    //player.ActiveOrcSpecTurn();
                    player.ActiveOrcTurn();
                    enemyPlayer.DisablePlayer();
                    enemyPlayer.PlayerThinkFinish();
                }
                else {
                    player.DisablePlayer();
                    enemyPlayer.PlayerThinking();
                    StartCoroutine(EnemeyOrcMagicSummon());
                }
                EventHandler.PostNotification(IngameEventHandler.EVENT_TYPE.BEGIN_ORC_POST_TURN, this, null);
                break;
            case "BATTLE":
                turnSpine.AnimationState.SetAnimation(0, "4.battle", false);
                dragable = false;
                player.DisablePlayer();
                enemyPlayer.PlayerThinkFinish();
                StartBattle();
                EventHandler.PostNotification(IngameEventHandler.EVENT_TYPE.BEGIN_BATTLE_TURN, this, null);
                break;
        }
        if (player.isHuman)
            StartCoroutine(SetHumanTurnTable(currentTurn));
        else
            StartCoroutine(SetOrcTurnTable(currentTurn));
    }

    public void StartBattle() {
        StartCoroutine("battleCoroutine");
    }

    public bool passOrc() {
        string turnName = socketHandler.gameState.state;
        if (turnName.CompareTo("orcPostTurn") == 0) return true;
        if (turnName.CompareTo("battleTurn") == 0) return true;
        if (turnName.CompareTo("shieldTurn") == 0) return true;
        if (turnName.CompareTo("endGame") == 0) return true;
        return false;
    }

    public IEnumerator EnemeyOrcMagicSummon() {
        yield return BeginStopTurn();
        yield return new WaitForSeconds(1f);
        //서버에서 오크 마법 턴 올 때까지 대기
        yield return new WaitUntil(passOrc);
        //잠복 스킬 발동 중이면 해결 될 때까지 대기 상태
        if (SkillModules.SkillHandler.running)
            yield return new WaitUntil(() => !SkillModules.SkillHandler.running);

        //그다음 카드 사용한게 있으면 카드 사용으로 패스
        yield return socketHandler.useCardList.WaitNext();
        if (!socketHandler.cardPlayFinish())
            yield return EnemyUseCard(false);
        //서버에서 턴 넘김이 완료 될 때까지 대기
        yield return socketHandler.WaitBattle();
        yield return AfterStopTurn();
        EventHandler.PostNotification(IngameEventHandler.EVENT_TYPE.END_TURN_BTN_CLICKED, this, TurnType.SECRET);
        //CustomEvent.Trigger(gameObject, "EndTurn");
    }

    public virtual IEnumerator battleCoroutine() {
        dragable = false;
        yield return new WaitForSeconds(0.8f);
        yield return socketHandler.waitSkillDone(() => { });
        yield return socketHandler.WaitBattle();
        for (int line = 0; line < 5; line++) {
            EventHandler.PostNotification(IngameEventHandler.EVENT_TYPE.LINE_BATTLE_START, this, line);
            yield return StopBattleLine();
            yield return battleLine(line);
            if (isGame == false) yield break;
        }
        yield return new WaitForSeconds(1f);
        socketHandler.TurnOver();
        if(socketHandler.humanData.Count > 0 || socketHandler.orcData.Count > 0) {
            Logger.LogWarning("전투 데이터가 아직 남았습니다. 이 메시지를 보시면 이종욱에게 알려주세요");
            socketHandler.humanData.Clear();
            socketHandler.orcData.Clear();
        }
        turn++;
        yield return socketHandler.WaitGetCard();
        DistributeResource();
        eventHandler.PostNotification(IngameEventHandler.EVENT_TYPE.END_BATTLE_TURN, this, null);
        EndTurnDraw();
        yield return new WaitForSeconds(2.0f);
        yield return new WaitUntil(() => !SkillModules.SkillHandler.running);
        EventHandler.PostNotification(IngameEventHandler.EVENT_TYPE.END_TURN_BTN_CLICKED, this, TurnType.BATTLE);
        //CustomEvent.Trigger(gameObject, "EndTurn");
        StopCoroutine("battleCoroutine");
        dragable = true;
    }

    protected IEnumerator battleLine(int line) {
        battleLineEffect = backGround.transform.GetChild(line).Find("BattleLineEffect");
        battleLineEffect.gameObject.SetActive(true);
        battleLineEffect.GetComponent<SpriteRenderer>().color = new Color(1, 0.545f, 0.427f, 0.6f);

        var observer = GetComponent<FieldUnitsObserver>();
        var list = observer.GetAllFieldUnits(line);
        if (list.Count != 0) {
            if (player.isHuman == false) yield return whoFirstBattle(player, enemyPlayer, line);
            else yield return whoFirstBattle(enemyPlayer, player, line);
        }
        else {
            // socketHandler.lineBattleList.Dequeue();
            // socketHandler.lineBattleList.Dequeue();
            // socketHandler.mapClearList.Dequeue();
            // socketHandler.lineBattleList.Dequeue();
            // socketHandler.lineBattleList.Dequeue();
            // socketHandler.mapClearList.Dequeue();
            yield return WaitSocketData(socketHandler.lineBattleList, line, true);
            shieldDequeue();
            yield return WaitSocketData(socketHandler.lineBattleList, line, true);
            shieldDequeue();
            yield return WaitSocketData(socketHandler.mapClearList, line, false);
            yield return WaitSocketData(socketHandler.lineBattleList, line, true);
            shieldDequeue();
            yield return WaitSocketData(socketHandler.lineBattleList, line, true);
            shieldDequeue();
            yield return WaitSocketData(socketHandler.mapClearList, line, false);
            yield return new WaitForSeconds(0.1f);
        }
        battleLineEffect.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 0.6f);
        battleLineEffect.gameObject.SetActive(false);
        EventHandler.PostNotification(IngameEventHandler.EVENT_TYPE.LINE_BATTLE_FINISHED, this, line);
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




    IEnumerator whoFirstBattle(PlayerController first, PlayerController second, int line) {
        var observer = GetComponent<FieldUnitsObserver>();
        var list = observer.GetAllFieldUnits(line, false);

        if(list.Count == 0) {
            yield return WaitSocketData(socketHandler.lineBattleList, line, true);
            shieldDequeue();
        }
        else {
            yield return GetBattle(first, line, false);
        }
        yield return GetBattle(second, line, false);
        CheckUnitStatus(line);
        yield return WaitSocketData(socketHandler.mapClearList, line, false);
        yield return GetBattle(first, line, true);
        yield return GetBattle(second, line, true);
        CheckUnitStatus(line);
        yield return WaitSocketData(socketHandler.mapClearList, line, false);
    }

    IEnumerator GetBattle(PlayerController player, int line, bool secondAttack) {
        yield return WaitSocketData(socketHandler.lineBattleList, line, true);
        yield return battleUnit(player.backLine, line, secondAttack);
        yield return battleUnit(player.frontLine, line, secondAttack);
        yield return HeroSpecialWait();
        shieldDequeue();
        yield return null;
    }

    private void CheckUnitStatus(int line) {
        CheckMonsterStatus(player.backLine.transform.GetChild(line));
        CheckMonsterStatus(player.frontLine.transform.GetChild(line));
        CheckMonsterStatus(enemyPlayer.backLine.transform.GetChild(line));
        CheckMonsterStatus(enemyPlayer.frontLine.transform.GetChild(line));
    }

    private void CheckMonsterStatus(Transform monsterTransform) {
        if (monsterTransform.childCount == 0) return;

        PlaceMonster monster = monsterTransform.GetChild(0).GetComponent<PlaceMonster>();
        monster.CheckHP();
    }

    IEnumerator battleUnit(GameObject lineObject, int line, bool secondAttack) {
        if (!isGame) yield break;
        if (lineObject.transform.GetChild(line).childCount == 0) yield break;
        PlaceMonster placeMonster = lineObject.transform.GetChild(line).GetChild(0).GetComponent<PlaceMonster>();
        if (placeMonster.maxAtkCount == 1 && secondAttack) yield break;
        if (placeMonster.unit.attack <= 0) yield break;
        placeMonster.GetTarget();
        yield return new WaitForSeconds(0.8f + placeMonster.atkTime);
    }
    IEnumerator WaitSocketData(SocketFormat.QueueSocketList<SocketFormat.GameState> queueList, int line, bool isBattle) {
        if (!isGame) yield break;
        yield return queueList.WaitNext();
        SocketFormat.GameState state = queueList.Dequeue();
        //Logger.Log("쌓인 데이터 리스트 : " + queueList.Count);
        if (state == null) {
            Logger.LogError("데이터가 없는 문제가 발생했습니다. 우선은 클라이언트에서 배틀 진행합니다.");
            yield break;
        }
        SocketFormat.DebugSocketData.ShowBattleData(state, line, isBattle);
        //데이터 체크 및 데이터 동기화
        if (!isBattle) SocketFormat.DebugSocketData.CheckBattleSynchronization(state);

    }

    private void shieldDequeue() {
        if (!isGame) return;
        if (socketHandler.humanData.Count == 0) return;
        socketHandler.humanData.Dequeue();
        socketHandler.orcData.Dequeue();
    }

    public IEnumerator HeroSpecialWait() {
        while(heroShieldActive) {
            yield return new WaitForFixedUpdate();
        }
    }

    public IEnumerator WaitDrawHeroCard() {
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

            battleLineEffect.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 0.6f);
            battleLineEffect.gameObject.SetActive(false);
            yield return cdpm.DrawHeroCard(heroCards);
            battleLineEffect.gameObject.SetActive(true);
            battleLineEffect.GetComponent<SpriteRenderer>().color = new Color(1, 0.545f, 0.427f, 0.6f);
        }
        else {
            GameObject enemyCard;
            if (enemyPlayer.isHuman)
                enemyCard = Instantiate(Resources.Load("Prefabs/HumanBackCard") as GameObject, enemyPlayer.playerUI.transform.Find("CardSlot").GetChild(CountEnemyCard()));
            else
                enemyCard = Instantiate(Resources.Load("Prefabs/OrcBackCard") as GameObject, enemyPlayer.playerUI.transform.Find("CardSlot").GetChild(CountEnemyCard()));
            enemyCard.transform.localScale = new Vector3(1, 1, 1);
            enemyCard.transform.localPosition = new Vector3(0, 0, 0);
            int count = CountEnemyCard();
            enemyCard.SetActive(false);
            enemyPlayer.playerUI.transform.Find("CardCount").GetChild(0).gameObject.GetComponent<Text>().text = (count).ToString();
            IngameNotice.instance.SelectNotice();
            //IngameNotice.instance.SetNotice("상대방이 영웅카드 사용 여부를 결정 중입니다");
        }
        yield return new WaitForSeconds(1f);
        if (isPlayer) socketHandler.TurnOver();
        yield return WaitShieldDone();
        StartCoroutine(socketHandler.waitSkillDone(() => {
            heroShieldActive = false;
            UnlockTurnOver();
        }, true));
        if (!isPlayer) enemyPlayer.ConsumeShieldStack();

    }

    public IEnumerator WaitShieldDone() {
        do {
            yield return new WaitForFixedUpdate();
            //스킬 사용
            if(socketHandler.useCardList.Count != 0) {
                IngameNotice.instance.CloseNotice();
                SocketFormat.GameState state = socketHandler.getHistory();
                SocketFormat.PlayHistory history = state.lastUse;
                if (history != null) {
                    GameObject summonedMagic = MakeMagicCardObj(history);
                    summonedMagic.GetComponent<MagicDragHandler>().isPlayer = false;
                    yield return MagicActivate(summonedMagic, history);
                    SocketFormat.DebugSocketData.SummonCardData(history);
                    int count = CountEnemyCard();
                    enemyPlayer.playerUI.transform.Find("CardCount").GetChild(0).gameObject.GetComponent<Text>().text = (count).ToString();
                    yield return new WaitForSeconds(1f);
                }
            }
        } while (heroShieldDone.Count == 0);
        heroShieldDone.RemoveAt(0);
        IngameNotice.instance.CloseNotice();
    }

    public void OnMoveSceneBtn() {
        FBL_SceneManager.Instance.LoadScene(FBL_SceneManager.Scene.MAIN_SCENE);
    }
}

/// <summary>
/// 유닛 소환관련 처리
/// </summary>
public partial class PlayMangement {
    public GameObject SummonUnit(bool isPlayer, string unitID, int col, int row, int itemID = -1, int cardIndex = -1, Transform[][] args = null, bool isFree = false) {
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
        placeMonster.unit.HP = (int)cardData.hp;
        placeMonster.unit.currentHP = (int)cardData.hp;
        placeMonster.unit.originalAttack = (int)cardData.attack;
        placeMonster.unit.attack = (int)cardData.attack;
        placeMonster.unit.type = cardData.type;
        placeMonster.unit.attackRange = cardData.attackRange;
        placeMonster.unit.cost = cardData.cost;
        placeMonster.unit.rarelity = cardData.rarelity;
        placeMonster.unit.id = cardData.id;
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
            placeMonster.unit.attackType = new string[cardData.attackTypes.Length];
            placeMonster.unit.attackType = cardData.attackTypes;
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
            int enemyCardCount = CountEnemyCard();
            //Destroy(enemyPlayer.playerUI.transform.Find("CardSlot").GetChild(enemyCardCount - 1).GetChild(0).gameObject);

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

    public void DistributeCard() {
        StartCoroutine(GenerateCard());
    }

    public IEnumerator GenerateCard() {
        int i = 0;
        while (i < 4) {
            yield return new WaitForSeconds(0.3f);
            if (i < 4)
                StartCoroutine(player.cdpm.FirstDraw());

            GameObject enemyCard;
            if (enemyPlayer.isHuman)
                enemyCard = Instantiate(Resources.Load("Prefabs/HumanBackCard") as GameObject, enemyPlayer.playerUI.transform.Find("CardSlot").GetChild(CountEnemyCard()));
            else
                enemyCard = Instantiate(Resources.Load("Prefabs/OrcBackCard") as GameObject, enemyPlayer.playerUI.transform.Find("CardSlot").GetChild(CountEnemyCard()));
            enemyCard.transform.position = player.cdpm.cardSpawnPos.position;
            enemyCard.transform.localScale = new Vector3(1, 1, 1);
            iTween.MoveTo(enemyCard, enemyCard.transform.parent.position, 0.3f);
            yield return new WaitForSeconds(0.3f);
            SoundManager.Instance.PlayIngameSfx(IngameSfxSound.CARDDRAW);
            enemyCard.SetActive(false);
            enemyPlayer.playerUI.transform.Find("CardCount").GetChild(0).gameObject.GetComponent<Text>().text = (i + 1).ToString();
            i++;
        }
    }

    public IEnumerator StoryDrawEnemyCard() {
        int length = socketHandler.gameState.players.enemyPlayer(enemyPlayer.isHuman).deck.handCards.Length;
        for(int i = 0; i < length; i++) {
            GameObject enemyCard;
            if (enemyPlayer.isHuman)
                enemyCard = Instantiate(Resources.Load("Prefabs/HumanBackCard") as GameObject, enemyPlayer.playerUI.transform.Find("CardSlot").GetChild(CountEnemyCard()));
            else
                enemyCard = Instantiate(Resources.Load("Prefabs/OrcBackCard") as GameObject, enemyPlayer.playerUI.transform.Find("CardSlot").GetChild(CountEnemyCard()));
            enemyCard.transform.position = player.cdpm.cardSpawnPos.position;
            enemyCard.transform.localScale = new Vector3(1, 1, 1);
            iTween.MoveTo(enemyCard, enemyCard.transform.parent.position, 0.3f);
            yield return new WaitForSeconds(0.15f);
            enemyCard.SetActive(false);
            SoundManager.Instance.PlayIngameSfx(IngameSfxSound.CARDDRAW);
            enemyPlayer.playerUI.transform.Find("CardCount").GetChild(0).gameObject.GetComponent<Text>().text = (i + 1).ToString();
        }
    }

    public virtual void EndTurnDraw() {
        if (isGame == false) return;
        bool race = player.isHuman;
        SocketFormat.Card cardData = socketHandler.gameState.players.myPlayer(race).newCard;
        player.cdpm.AddCard(null, cardData);
        if(CountEnemyCard() >= 10) return;
        GameObject enemyCard;
        if (enemyPlayer.isHuman)
            enemyCard = Instantiate(Resources.Load("Prefabs/HumanBackCard") as GameObject, enemyPlayer.playerUI.transform.Find("CardSlot").GetChild(CountEnemyCard()));
        else
            enemyCard = Instantiate(Resources.Load("Prefabs/OrcBackCard") as GameObject, enemyPlayer.playerUI.transform.Find("CardSlot").GetChild(CountEnemyCard()));
        enemyCard.transform.position = player.cdpm.cardSpawnPos.position;
        enemyCard.transform.localScale = new Vector3(1, 1, 1);
        iTween.MoveTo(enemyCard, enemyCard.transform.parent.position, 0.3f);
        SoundManager.Instance.PlayIngameSfx(IngameSfxSound.CARDDRAW);
        enemyCard.SetActive(false);
        int count = CountEnemyCard();
        enemyPlayer.playerUI.transform.Find("CardCount").GetChild(0).gameObject.GetComponent<Text>().text = (count).ToString();
    }

    public IEnumerator EnemyMagicCardDraw(int drawNum) {
        int total = CountEnemyCard() + drawNum;
        if(total > 10) drawNum = drawNum - (total - 10);
        for(int i = 0 ; i < drawNum; i++) {
            GameObject enemyCard;
            if (enemyPlayer.isHuman)
                enemyCard = Instantiate(Resources.Load("Prefabs/HumanBackCard") as GameObject, enemyPlayer.playerUI.transform.Find("CardSlot").GetChild(CountEnemyCard()));
            else
                enemyCard = Instantiate(Resources.Load("Prefabs/OrcBackCard") as GameObject, enemyPlayer.playerUI.transform.Find("CardSlot").GetChild(CountEnemyCard()));
            enemyCard.transform.position = player.cdpm.cardSpawnPos.position;
            enemyCard.transform.localScale = new Vector3(1, 1, 1);
            SoundManager.Instance.PlayIngameSfx(IngameSfxSound.CARDDRAW);
            iTween.MoveTo(enemyCard, enemyCard.transform.parent.position, 0.3f);
            enemyCard.SetActive(false);
            int count = CountEnemyCard();
            enemyPlayer.playerUI.transform.Find("CardCount").GetChild(0).gameObject.GetComponent<Text>().text = (count).ToString();
            yield return new WaitForSeconds(0.3f);
        }
    }

    public int CountEnemyCard() {
        int enemyNum = 0;
        for (int i = 0; i < 10; i++) {
            if (enemyPlayer.playerUI.transform.Find("CardSlot").GetChild(i).childCount > 0)
                enemyNum++;
            else
                return enemyNum;
        }
        return enemyNum;
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
    SkeletonGraphic turnSpine;
    public GameObject releaseTurnBtn;
    private GameObject nonplayableTurnArrow;
    private GameObject playableTurnArrow;
    private Transform turnIcon;

    public void InitTurnTable() {
        string race = PlayerPrefs.GetString("SelectedRace").ToLower();
        bool isHuman;

        turnSpine = turnTable.Find("TurnSpine").GetComponent<SkeletonGraphic>();
        if (race == "human") isHuman = true;
        else isHuman = false;
        if (isHuman) {
            releaseTurnBtn = turnTable.Find("HumanButton").gameObject;
        }
        else {
            releaseTurnBtn = turnTable.Find("OrcButton").gameObject;
        }
        //for (int i = 0; i < 4; i++) {
        //    turnTable.Find("TurnBoard").position = playerCanvas.transform.GetChild(1).GetChild(2).position;
        //}

        Debug.Log("isHuman" + isHuman);
    }

    public void SetTurnButton() {
        switch (currentTurn) {
            case "ORC":
                if (player.isHuman)
                    releaseTurnBtn.SetActive(false);
                else
                    releaseTurnBtn.SetActive(true);
                break;
            case "HUMAN":
                if (player.isHuman)
                    releaseTurnBtn.SetActive(true);
                else
                    releaseTurnBtn.SetActive(false);
                break;
            case "SECRET":
                if (player.isHuman)
                    releaseTurnBtn.SetActive(false);
                else
                    releaseTurnBtn.SetActive(true);
                break;
            case "BATTLE":
                releaseTurnBtn.SetActive(false);
                break;
        }
    }
    
    private IEnumerator SetHumanTurnTable(string currentTurn) {
        yield return new WaitForSeconds(0.3f);
        switch (currentTurn) {
            case "HUMAN":
                releaseTurnBtn.SetActive(true);
                break;
            case "ORC":
            case "SECRET":
            case "BATTLE":
                releaseTurnBtn.SetActive(false);
                break;
        }
    }

    public void LockTurnOver() {
        releaseTurnBtn.GetComponent<Button>().enabled = false;
    }

    public void UnlockTurnOver() {
        releaseTurnBtn.GetComponent<Button>().enabled = true;
    }

    private IEnumerator SetOrcTurnTable(string currentTurn) {
        yield return new WaitForSeconds(0.3f);
        switch (currentTurn) {
            case "ORC":
            case "SECRET":
                releaseTurnBtn.SetActive(true);
                break;
            case "HUMAN":
            case "BATTLE":
                releaseTurnBtn.SetActive(false);
                break;
        }
    }

    public void OnNoCostEffect(bool turnOn) {
        releaseTurnBtn.transform.Find("ResourceOut").gameObject.SetActive(turnOn);
    }

    public void EditorTestInit(SocketFormat.GameState state) {
        EditorMapInit(state);
        EditorCardInit(state);
        EditorPlayerInit(state);
        EventHandler.PostNotification(IngameEventHandler.EVENT_TYPE.FIELD_CHANGED, null, null);
        StartCoroutine(player.cdpm.EditorSkipMulligan());
    }

    private void EditorMapInit(SocketFormat.GameState state) {
        for(int i = 0; i < state.map.lines.Length; i++) {
            EditorSummonUnit(i, state.map.lines[i].human, true);
            EditorSummonUnit(i, state.map.lines[i].orc, false);
        }
    }

    private void EditorSummonUnit(int line, SocketFormat.Unit[] units, bool isHuman) {
        if(units.Length == 0) return;
        Transform race = player.isHuman == isHuman ? player.transform : enemyPlayer.transform;
        Transform line_rear = race.GetChild(0);
        Transform line_front = race.GetChild(1);
        GameObject unit1 = SummonUnit(player.isHuman == isHuman, units[0].id, line, 0, units[0].itemId);
        if(units.Length == 2) return;
        GameObject unit2 = SummonUnit(player.isHuman == isHuman, units[1].id, line, 1, units[1].itemId);
    }

    private void EditorCardInit(SocketFormat.GameState state) {
        StartCoroutine(player.cdpm.AddMultipleCard(state.players.myPlayer(player.isHuman).deck.handCards));
        StartCoroutine(EnemyMagicCardDraw(state.players.enemyPlayer(enemyPlayer.isHuman).deck.handCards.Length));
    }

    private void EditorPlayerInit(SocketFormat.GameState state) {
        PlayerDataInit(player, state.players.myPlayer(player.isHuman));
        PlayerDataInit(enemyPlayer, state.players.enemyPlayer(enemyPlayer.isHuman));
    }

    private void PlayerDataInit(PlayerController player, SocketFormat.Player data) {
        player.HP.Value = data.hero.currentHp;
        player.shieldStack.Value = data.hero.shieldGauge;
        player.resource.Value = data.resource;
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