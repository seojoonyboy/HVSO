using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Spine;
using Spine.Unity;
using TMPro;


public partial class PlayMangement : MonoBehaviour {
    public PlayerController player, enemyPlayer;

    public GameObject cardDB;
    public GameObject uiSlot;
    public GameObject canvas;

    public Transform cardInfoCanvas;
    public Transform battleLineEffect;
    bool firstTurn = true;
    public bool isGame = true;
    public bool isMulligan = true;
    public bool infoOn = false;
    public static PlayMangement instance { get; protected set; }
    public GameObject backGround;
    public GameObject onCanvasPosGroup;
    //public CardCircleManager cardCircleManager;
    public CardHandManager cardHandManager;
    public GameResultManager resultManager;
    public SkeletonGraphic playerMana, enemyMana;

    public GameObject baseUnit;
    private int turn = 0;
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

    //public string magicHistroy;

    private void Awake() {
        socketHandler = FindObjectOfType<BattleConnector>();
        bool isTest = PlayerPrefs.GetString("SelectedBattleType").CompareTo("test") == 0;
        SetWorldScale();
        instance = this;
        SetPlayerCard();
        GetComponent<TurnMachine>().onTurnChanged.AddListener(ChangeTurn);
        if (!isTest) GetComponent<TurnMachine>().onPrepareTurn.AddListener(DistributeCard);
        //GameObject backGroundEffect = Instantiate(EffectSystem.Instance.backgroundEffect);
        //backGroundEffect.transform.position = backGround.transform.Find("ParticlePosition").position;
        SetCamera();
    }
    private void OnDestroy() {
        instance = null;
        if (socketHandler != null)
            Destroy(socketHandler.gameObject);
    }

    private void Start() {
        SetBackGround();
        InitGameData();



        //StartCoroutine(DisconnectTest());
    }


    //최초에 데이터를 불러드릴 함수. missionData를 임시로 놓고, 그 후에 게임의 정보들 등록
    //체력설정 -> 승리목표 설정 -> 자원분배 -> 턴
    private void InitGameData() {
        object missionData = null;

        RequestStartData(20, 20);
        SetVictoryCondition();
        DistributeResource();
        InitTurnTable();
    }

    //승리 조건을 설정할 함수. victoryModule이라는 namespace로 전략패턴으로 구현 계획
    private void SetVictoryCondition(object data = null) {
        string condition = (string)data;

        switch (condition) {
            default:
                matchRule = new victoryModule.Annihilation_Match(player, enemyPlayer);
                matchRule.SetCondition();
                break;
        }
    }
    // 시작전 체력부여, default 20
    public void RequestStartData(int playerData = 20, int enemyData = 20) {
        int playerHP = playerData;
        int enemyHP = enemyData;

        player.SetPlayerStat(playerHP);
        enemyPlayer.SetPlayerStat(enemyHP);
    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.Escape)) {
            FBL_SceneManager.Instance.LoadScene(FBL_SceneManager.Scene.MAIN_SCENE);
        }
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

        for (int i = 0; i < player.frontLine.transform.childCount; i++) {
            player.backLine.transform.GetChild(i).position = new Vector3(backGround.transform.GetChild(i).position.x, player.backLine.transform.position.y, 0);
            player.frontLine.transform.GetChild(i).position = new Vector3(backGround.transform.GetChild(i).position.x, player.frontLine.transform.position.y, 0);
            enemyPlayer.backLine.transform.GetChild(i).position = new Vector3(backGround.transform.GetChild(i).position.x, enemyPlayer.backLine.transform.position.y, 0);
            enemyPlayer.frontLine.transform.GetChild(i).position = new Vector3(backGround.transform.GetChild(i).position.x, enemyPlayer.frontLine.transform.position.y, 0);
        }

        player.backLine.transform.position = backGround.transform.Find("Line_Y_Position").Find("Player1_BackLine").position;
        player.frontLine.transform.position = backGround.transform.Find("Line_Y_Position").Find("Player1_FrontLine").position;
        enemyPlayer.backLine.transform.position = backGround.transform.Find("Line_Y_Position").Find("Player2_BackLine").position;
        enemyPlayer.frontLine.transform.position = backGround.transform.Find("Line_Y_Position").Find("Player2_FrontLine").position;

        for (int i = 0; i < player.frontLine.transform.childCount; i++) {
            Vector3 pos = backGround.transform.GetChild(i).position;
            backGround.transform.GetChild(i).position = new Vector3(pos.x, player.backLine.transform.position.y, 0);
        }
    }


    protected virtual void SetBackGround() {
        if (player.isHuman == true) {
            GameObject raceSprite = Instantiate(AccountManager.Instance.resource.raceUiPrefabs["HUMAN_BACKGROUND"][0], backGround.transform);
            raceSprite.transform.SetAsLastSibling();
        }
        else {
            GameObject raceSprite = Instantiate(AccountManager.Instance.resource.raceUiPrefabs["ORC_BACKGROUND"][0], backGround.transform);
            raceSprite.transform.SetAsLastSibling();
        }
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
        enemyPlayer.playerUI.transform.Find("CardCount").gameObject.GetComponent<Image>().sprite = enemyCard.GetComponent<Image>().sprite;
    }



    public void DistributeResource() {
        player.resource.Value = turn + 1;
        enemyPlayer.resource.Value = turn + 1;
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

    IEnumerator EnemyUseCard(bool isBefore) {
        if (isBefore)
            yield return new WaitForSeconds(1.0f);
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
            enemyPlayer.playerUI.transform.Find("CardCount").GetChild(0).gameObject.GetComponent<TextMeshProUGUI>().text = "X" + " " + (count).ToString();
            //SocketFormat.DebugSocketData.CheckMapPosition(state);
            yield return new WaitForSeconds(0.5f);
        }
        #endregion
        SocketFormat.DebugSocketData.ShowHandCard(socketHandler.gameState.players.enemyPlayer(enemyPlayer.isHuman).deck.handCards);
        if (isBefore)
            enemyPlayer.ReleaseTurn();
    }

    /// <summary>
    /// 마법 카드생성(비활성화 상태로 생성)
    /// </summary>
    /// <param name="history"></param>
    /// <returns></returns>
    private GameObject MakeMagicCardObj(SocketFormat.PlayHistory history) {
        CardData cardData;
        CardDataPackage cardDataPackage = AccountManager.Instance.cardPackage;

        cardData = cardDataPackage.data[history.cardItem.id];
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
    private GameObject MakeUnitCardObj(SocketFormat.PlayHistory history) {
        CardData cardData;
        CardDataPackage cardDataPackage = AccountManager.Instance.cardPackage;

        cardData = cardDataPackage.data[history.cardItem.id];
        GameObject unitCard = player.cdpm.InstantiateUnitCard(cardData, history.cardItem.itemId);
        unitCard.GetComponent<UnitDragHandler>().itemID = history.cardItem.itemId;

        Destroy(enemyPlayer.playerUI.transform.Find("CardSlot").GetChild(CountEnemyCard() - 1).GetChild(0).gameObject);
        return unitCard;
    }

    private IEnumerator UnitActivate(SocketFormat.PlayHistory history) {
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

    private IEnumerator MagicActivate(GameObject card, SocketFormat.PlayHistory history) {
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
        //실제 카드 사용
        object[] parms = new object[] { false, card };
        if (magicCard.cardData.hero_chk == true) {
            card.transform.Find("GlowEffect").gameObject.SetActive(false);
            card.transform.Find("Portrait").gameObject.SetActive(false);
            card.transform.Find("BackGround").gameObject.SetActive(false);
            card.transform.Find("Cost").gameObject.SetActive(false);
            int count = CountEnemyCard();
            enemyPlayer.playerUI.transform.Find("CardCount").GetChild(0).gameObject.GetComponent<TextMeshProUGUI>().text = "X" + " " + (count).ToString();


            yield return EffectSystem.Instance.HeroCutScene(enemyPlayer.isHuman);
        }
        EventHandler.PostNotification(IngameEventHandler.EVENT_TYPE.END_CARD_PLAY, this, parms);
        yield return new WaitForSeconds(2f);
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
                List<GameObject> list;
                if (args[0].CompareTo("my") == 0) //적 자신일 경우
                    list = UnitsObserver.GetAllFieldUnits(enemyPlayer.isHuman);
                else //적의 적 (나)일 경우
                    list = UnitsObserver.GetAllFieldUnits(!enemyPlayer.isHuman);
                GameObject unit = list.Find(x => x.GetComponent<PlaceMonster>().itemId == itemId);
                highlightUI = unit.transform.Find("ClickableUI").gameObject;
                highlightUI.SetActive(true);
                break;
            case "line":
                int line = int.Parse(target.args[0]);
                Terrain[] lineList = FindObjectsOfType<Terrain>();
                int terrainLine;
                for (int i = 0; i < lineList.Length; i++) {
                    terrainLine = lineList[i].transform.GetSiblingIndex();
                    if (terrainLine != line) continue;
                    highlightUI = lineList[i].transform.Find("BattleLineEffect").gameObject;
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
        return monster;
    }

    public void ChangeTurn() {
        if (isGame == false) return;
        player.buttonParticle.SetActive(false);
        currentTurn = GetComponent<TurnMachine>().CurrentTurn();
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
        EventHandler.PostNotification(IngameEventHandler.EVENT_TYPE.END_TURN_BTN_CLICKED, this);
        //CustomEvent.Trigger(gameObject, "EndTurn");
    }

    public void GetPlayerTurnRelease() {
        EventHandler.PostNotification(IngameEventHandler.EVENT_TYPE.END_TURN_BTN_CLICKED, this);
        //CustomEvent.Trigger(gameObject, "EndTurn");
    }

    IEnumerator battleCoroutine() {
        dragable = false;
        yield return new WaitForSeconds(1.1f);
        yield return socketHandler.waitSkillDone(() => { });
        yield return socketHandler.WaitBattle();
        for (int line = 0; line < 5; line++) {
            EventHandler.PostNotification(IngameEventHandler.EVENT_TYPE.LINE_BATTLE_START, this, line);
            yield return StopBattleLine();
            yield return battleLine(line);
            if (isGame == false) break;
        }
        yield return new WaitForSeconds(1f);
        socketHandler.TurnOver();
        turn++;
        yield return socketHandler.WaitGetCard();
        DistributeResource();
        eventHandler.PostNotification(IngameEventHandler.EVENT_TYPE.END_BATTLE_TURN, this, null);
        EndTurnDraw();
        yield return new WaitForSeconds(2.0f);
        yield return new WaitUntil(() => !SkillModules.SkillHandler.running);
        EventHandler.PostNotification(IngameEventHandler.EVENT_TYPE.END_TURN_BTN_CLICKED, this);
        //CustomEvent.Trigger(gameObject, "EndTurn");
        StopCoroutine("battleCoroutine");
        dragable = true;
    }

    IEnumerator battleLine(int line) {
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
            yield return new WaitForSeconds(0.2f);
        }
        battleLineEffect.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 0.6f);
        battleLineEffect.gameObject.SetActive(false);
        EventHandler.PostNotification(IngameEventHandler.EVENT_TYPE.LINE_BATTLE_FINISHED, this, line);
    }

    IEnumerator StopBattleLine() {
        yield return new WaitUntil(() => stopBattle == false);
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
        yield return new WaitForSeconds(1.1f + placeMonster.atkTime);
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

    public IEnumerator DrawSpecialCard(bool isHuman) {
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
            enemyCard.SetActive(true);
            int count = CountEnemyCard();
            enemyPlayer.playerUI.transform.Find("CardCount").GetChild(0).gameObject.GetComponent<TextMeshProUGUI>().text = "X" + " " + (count).ToString();
            IngameNotice.instance.SetNotice("상대방이 영웅카드 사용 여부를 결정 중입니다");
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
                    enemyPlayer.playerUI.transform.Find("CardCount").GetChild(0).gameObject.GetComponent<TextMeshProUGUI>().text = "X" + " " + (count).ToString();
                    yield return new WaitForSeconds(1f);
                }
            }
        } while (heroShieldDone.Count == 0);
        heroShieldDone.RemoveAt(0);
        IngameNotice.instance.CloseNotice();
    }

    //public void GetBattleResult() {
    //    isGame = false;
    //    resultManager.gameObject.SetActive(true);

    //    if (player.HP.Value <= 0) {
    //        if (player.isHuman)
    //            resultManager.SetResultWindow("lose", "human");
    //        else
    //            resultManager.SetResultWindow("lose", "orc");
    //    }
    //    else if (enemyPlayer.HP.Value <= 0) {
    //        if (player.isHuman)
    //            resultManager.SetResultWindow("win", "human");
    //        else
    //            resultManager.SetResultWindow("win", "orc");
    //    }
    //}
}

/// <summary>
/// 유닛 소환관련 처리
/// </summary>
public partial class PlayMangement {
    public GameObject SummonUnit(bool isPlayer, string unitID, int col, int row, int itemID = -1, int cardIndex = -1, Transform[][] args = null) {
        PlayerController targetPlayer = (isPlayer == true) ? player : enemyPlayer;
        CardDataPackage cardDataPackage = AccountManager.Instance.cardPackage;

        GameObject skeleton;
        CardData cardData;
        cardData = cardDataPackage.data[unitID];

        Logger.Log(col);
        Logger.Log(row);

        GameObject unit = Instantiate(baseUnit, targetPlayer.transform.GetChild(row).GetChild(col));
        unit.transform.position = targetPlayer.transform.GetChild(row).GetChild(col).position;
        PlaceMonster placeMonster = unit.GetComponent<PlaceMonster>();

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
        placeMonster.unit.id = cardData.cardId;
        placeMonster.unit.attributes = cardData.attributes;


        if (cardData.category_2 != "") {
            placeMonster.unit.cardCategories = new string[2];
            placeMonster.unit.cardCategories[0] = cardData.category_1;
            placeMonster.unit.cardCategories[1] = cardData.category_2;
        }
        else {
            placeMonster.unit.cardCategories = new string[1];
            placeMonster.unit.cardCategories[0] = cardData.category_1;
        }

        if (cardData.attackTypes.Length > 0) {
            placeMonster.unit.attackType = new string[cardData.attackTypes.Length];
            placeMonster.unit.attackType = cardData.attackTypes;
        }

        skeleton = Instantiate(AccountManager.Instance.resource.cardSkeleton[unitID], placeMonster.transform);
        skeleton.name = "skeleton";
        skeleton.transform.localScale = (isPlayer == true) ? new Vector3(-1, 1, 1) : new Vector3(1, 1, 1);
        placeMonster.name = cardData.name;

        placeMonster.Init(cardData);
        placeMonster.SpawnUnit();
        targetPlayer.resource.Value -= cardData.cost;
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
            enemyCard.SetActive(false);
            enemyPlayer.playerUI.transform.Find("CardCount").GetChild(0).gameObject.GetComponent<TextMeshProUGUI>().text = "X" + " " + (i + 1).ToString();
            i++;
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
        enemyCard.SetActive(true);
        int count = CountEnemyCard();
        enemyPlayer.playerUI.transform.Find("CardCount").GetChild(0).gameObject.GetComponent<TextMeshProUGUI>().text = "X" + " " + (count).ToString();
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
            iTween.MoveTo(enemyCard, enemyCard.transform.parent.position, 0.3f);
            enemyCard.SetActive(true);
            int count = CountEnemyCard();
            enemyPlayer.playerUI.transform.Find("CardCount").GetChild(0).gameObject.GetComponent<TextMeshProUGUI>().text = "X" + " " + (count).ToString();
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
    [SerializeField] IngameEventHandler eventHandler;
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
        string race = PlayerPrefs.GetString("SelectedRace");
        bool isHuman;

        turnSpine = turnTable.Find("TurnSpine").GetComponent<SkeletonGraphic>();
        if (race == "HUMAN") isHuman = true;
        else isHuman = false;
        if (isHuman) {
            releaseTurnBtn = turnTable.Find("HumanButton").gameObject;
        }
        else {
            releaseTurnBtn = turnTable.Find("OrcButton").gameObject;
        }
        for (int i = 0; i < 4; i++) {
            turnTable.Find("TurnBoard").position = canvas.transform.GetChild(2).GetChild(2).position;
        }
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