using Bolt;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public partial class PlayMangement : MonoBehaviour {
    public PlayerController player, enemyPlayer;
    public Sprite humanResourceIcon, orcResourceIcon;

    public GameObject cardDB;
    public GameObject uiSlot;
    public GameObject canvas;

    public Transform cardInfoCanvas;
    public Transform battleLineEffect;
    public bool isGame = true;
    public bool isMulligan = true;
    public bool infoOn = false;
    public static PlayMangement instance { get; protected set; }
    public GameObject backGround;
    public GameObject onCanvasPosGroup;
    public EffectManager effectManager;
    public SpineEffectManager spineEffectManager;
    public CardCircleManager cardCircleManager;

    public GameObject baseUnit;
    private int turn = 0;
    public GameObject blockPanel;
    public int unitNum = 0;
    public bool heroShieldActive = false;
    public GameObject humanShield, orcShield;
    public static GameObject movingCard;
    public static bool dragable = true;

    private void Awake() {
        socketHandler = FindObjectOfType<BattleConnector>();

        SetWorldScale();
        instance = this;
        SetPlayerCard();
        gameObject.GetComponent<TurnChanger>().onTurnChanged.AddListener(() => ChangeTurn());
        gameObject.GetComponent<TurnChanger>().onPrepareTurn.AddListener(() => DistributeCard());
        GameObject backGroundEffect = Instantiate(effectManager.backGroundEffect);
        backGroundEffect.transform.position = backGround.transform.Find("ParticlePosition").position;
        SetCamera();
    }
    private void OnDestroy() {
        instance = null;
        if (socketHandler != null)
            Destroy(socketHandler.gameObject);
    }

    private void Start() {
        SetBackGround();
        RequestStartData();
        DistributeResource();
        InitTurnTable();
        //StartCoroutine(DisconnectTest());
    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.Escape)) {
            SceneManager.Instance.LoadScene(SceneManager.Scene.MAIN_SCENE);
        }
        if (!infoOn && Input.GetMouseButtonDown(0)) {
            cardInfoCanvas.GetChild(0).GetComponent<CardListManager>().OpenUnitInfoWindow(Input.mousePosition);
        }
    }

    private void SetWorldScale() {

        SpriteRenderer backSprite = backGround.GetComponent<SpriteRenderer>();
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


    private void SetBackGround() {
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
        if (player.isHuman == true) {
            //player.card = cardDB.transform.Find("Card").gameObject;
            player.back = cardDB.transform.Find("HumanBackCard").gameObject;
            enemyPlayer.back = cardDB.transform.Find("OrcBackCard").gameObject;
        }
        else {
            //player.card = cardDB.transform.Find("Card").gameObject;
            player.back = cardDB.transform.Find("OrcBackCard").gameObject;
            enemyPlayer.back = cardDB.transform.Find("HumanBackCard").gameObject;
        }
    }

    public void RequestStartData() {
        player.SetPlayerStat(20);
        enemyPlayer.SetPlayerStat(20);

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
                    GameObject summonedMonster = SummonMonster(history);
                    summonedMonster.GetComponent<PlaceMonster>().isPlayer = false;

                    object[] parms = new object[] { false, summonedMonster };
                    EventHandler.PostNotification(IngameEventHandler.EVENT_TYPE.END_CARD_PLAY, this, parms);
                    EventHandler.PostNotification(IngameEventHandler.EVENT_TYPE.FIELD_CHANGED, null, null);
                }
                else {
                    GameObject summonedMagic = SummonMagic(history);
                    summonedMagic.GetComponent<MagicDragHandler>().isPlayer = false;
                    yield return MagicActivate(summonedMagic, history);
                }
                SocketFormat.DebugSocketData.SummonCardData(history);
            }
            //SocketFormat.DebugSocketData.CheckMapPosition(state);
            yield return new WaitForSeconds(0.5f);
        }
        #endregion
        SocketFormat.DebugSocketData.ShowHandCard(socketHandler.gameState.players.enemyPlayer(enemyPlayer.isHuman).deck.handCards);
        if (isBefore)
            enemyPlayer.ReleaseTurn();
    }

    private GameObject SummonMagic(SocketFormat.PlayHistory history) {
        CardData cardData;
        CardDataPackage cardDataPackage = AccountManager.Instance.cardPackage;

        cardData = cardDataPackage.data[history.cardItem.id];
        GameObject magicCard = player.cdpm.InstantiateMagicCard(cardData, history.cardItem.itemId);
        magicCard.GetComponent<MagicDragHandler>().itemID = history.cardItem.itemId;

        Logger.Log("use Magic Card" + history.cardItem.name);
        enemyPlayer.resource.Value -= cardData.cost;

        Destroy(enemyPlayer.playerUI.transform.Find("CardSlot").GetChild(CountEnemyCard() - 1).GetChild(0).gameObject);
        return magicCard;
    }

    private IEnumerator MagicActivate(GameObject card, SocketFormat.PlayHistory history) {
        MagicDragHandler magicCard = card.GetComponent<MagicDragHandler>();
        magicCard.skillHandler.isDone = false;
        dragable = false;
        //카드 등장 애니메이션
        card.transform.rotation = new Quaternion(0, 0, 540, card.transform.rotation.w);
        card.transform.SetParent(enemyPlayer.playerUI.transform);
        card.SetActive(true);
        //iTween.RotateTo(card, Vector3.zero, 0.5f);
        //iTween.MoveTo(card, iTween.Hash(
        //    "x", card.transform.parent.position.x,
        //    "y", card.transform.parent.position.y,
        //    "time", 0.5f,
        //    "easetype", iTween.EaseType.easeWeakOutBack));
        Logger.Log(enemyPlayer.playerUI.transform.position);
        yield return new WaitForSeconds(1.0f);
        //타겟 지정 애니메이션
        yield return EnemySettingTarget(history.targets[0], magicCard);
        //실제 카드 사용
        object[] parms = new object[] { false, card };
        yield return StartCoroutine(cardCircleManager.ShowUsedMagicCard(100, card));
        EventHandler.PostNotification(IngameEventHandler.EVENT_TYPE.END_CARD_PLAY, this, parms);
        yield return new WaitForSeconds(2f);
        //카드 파괴
        card.transform.localScale = new Vector3(1, 1, 1);
        cardCircleManager.DestroyCard(card);
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
                    list = enemyUnitsObserver.GetAllFieldUnits();
                else //적의 적 (나)일 경우
                    list = playerUnitsObserver.GetAllFieldUnits();
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
        highlightUI.GetComponent<SpriteRenderer>().color = new Color(1, 0, 0, 155.0f / 255.0f);
        yield return new WaitForSeconds(1.5f);
        highlightUI.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 155.0f / 255.0f);
        highlightUI.SetActive(false);
    }

    private GameObject SummonMonster(SocketFormat.PlayHistory history) {
        int i = int.Parse(history.targets[0].args[0]);
        string id = history.cardItem.id;
        bool isFront = history.targets[0].args[2].CompareTo("front")==0;
        bool unitExist = enemyUnitsObserver.CheckUnitPosition(i, 0);
        int j = isFront && unitExist ? 1 : 0;
        if(unitExist && !isFront) {
            Transform line_rear = enemyPlayer.transform.GetChild(0);
            Transform line_front = enemyPlayer.transform.GetChild(1);
            Transform existUnit;
            existUnit = line_rear.GetChild(i).GetChild(0);
            existUnit.GetComponent<PlaceMonster>().unitLocation = line_front.GetChild(i).position;
            enemyUnitsObserver.UnitChangePosition(existUnit.gameObject, i, 1);
        }
        GameObject monster = SummonUnit(false, id, i, j, history.cardItem.itemId);
        return monster;
    }

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
        if (isPlayer) {
            player.isPicking.Value = false;
            if (player.isHuman)
                player.ActivePlayer();
            else
                player.ActiveOrcTurn();
            if (args != null)
                playerUnitsObserver.RefreshFields(args);
            else
                playerUnitsObserver.UnitAdded(unit, col, row);

            player.cdpm.DestroyCard(cardIndex);
        }
        else {
            int enemyCardCount = CountEnemyCard();
            Destroy(enemyPlayer.playerUI.transform.Find("CardSlot").GetChild(enemyCardCount - 1).GetChild(0).gameObject);

            SkillModules.SkillHandler skillHandler = new SkillModules.SkillHandler();
            skillHandler.Initialize(cardData.skills, unit, false);
            unit.GetComponent<PlaceMonster>().skillHandler = skillHandler;
            cardInfoCanvas.GetChild(0).GetComponent<CardListManager>().AddFeildUnitInfo(0, placeMonster.myUnitNum, cardData);
            EnemyUnitsObserver.UnitAdded(unit, col, 0);
            unit.layer = 14;
        }
        targetPlayer.PlayerUseCard();
        return unit;
    }

    public void ChangeTurn() {
        if (isGame == false) return;
        string currentTurn = Variables.Scene(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene()
            ).Get("CurrentTurn").ToString();
        Logger.Log(currentTurn);
        switch (currentTurn) {
            case "ORC":
                if (player.isHuman == false) {
                    player.ActiveOrcTurn();
                    enemyPlayer.DisablePlayer();
                }
                else {
                    player.DisablePlayer();
                    enemyPlayer.ActivePlayer();
                    StartCoroutine(EnemyUseCard(true));
                }
                EventHandler.PostNotification(IngameEventHandler.EVENT_TYPE.BEGIN_ORC_PRE_TURN, this, null);
                break;

            case "HUMAN":
                if (player.isHuman == true) {
                    player.ActivePlayer();
                    enemyPlayer.DisablePlayer();
                }
                else {
                    player.DisablePlayer();
                    enemyPlayer.ActivePlayer();
                    StartCoroutine(EnemyUseCard(true));
                }
                EventHandler.PostNotification(IngameEventHandler.EVENT_TYPE.BEGIN_HUMAN_TURN, this, null);
                break;

            case "SECRET":
                if (player.isHuman == false) {
                    //player.ActiveOrcSpecTurn();
                    player.ActiveOrcTurn();
                    enemyPlayer.DisablePlayer();
                }
                else {
                    player.DisablePlayer();
                    StartCoroutine(EnemeyOrcMagicSummon());
                }
                EventHandler.PostNotification(IngameEventHandler.EVENT_TYPE.BEGIN_ORC_POST_TURN, this, null);
                break;
            case "BATTLE":
                dragable = false;
                player.DisablePlayer();
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

    public IEnumerator EnemeyOrcMagicSummon() {
        yield return new WaitForSeconds(1f);
        //잠복 스킬 발동 중이면 해결 될 때까지 대기 상태
        if (SkillModules.SkillHandler.running)
            yield return new WaitUntil(() => !SkillModules.SkillHandler.running);

        //그다음 카드 사용한게 있으면 카드 사용으로 패스
        yield return socketHandler.useCardList.WaitNext();
        if (!socketHandler.cardPlayFinish())
            yield return EnemyUseCard(false);
        //서버에서 턴 넘김이 완료 될 때까지 대기
        yield return socketHandler.WaitBattle();
        CustomEvent.Trigger(gameObject, "EndTurn");
    }

    public void GetPlayerTurnRelease() {
        CustomEvent.Trigger(gameObject, "EndTurn");
    }

    IEnumerator battleCoroutine() {
        dragable = false;
        yield return new WaitForSeconds(1.1f);
        yield return socketHandler.WaitBattle();
        for (int line = 0; line < 5; line++) {
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
        CustomEvent.Trigger(gameObject, "EndTurn");
        StopCoroutine("battleCoroutine");
        dragable = true;
    }

    IEnumerator battleLine(int line) {
        battleLineEffect = backGround.transform.GetChild(line).Find("BattleLineEffect");
        battleLineEffect.gameObject.SetActive(true);
        battleLineEffect.GetComponent<SpriteRenderer>().color = new Color(1, 0.384f, 0.121f, 0.608f);
        var list = playerUnitsObserver.GetAllFieldUnits(line);
        list.AddRange(enemyUnitsObserver.GetAllFieldUnits(line));
        if (list.Count != 0) {
            if (player.isHuman == false) yield return whoFirstBattle(player, enemyPlayer, playerUnitsObserver, line);
            else yield return whoFirstBattle(enemyPlayer, player, enemyUnitsObserver, line);
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
        ResetCount(line);
        battleLineEffect.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 0.608f);
        battleLineEffect.gameObject.SetActive(false);
        EventHandler.PostNotification(IngameEventHandler.EVENT_TYPE.LINE_BATTLE_FINISHED, this);
    }

    void ResetCount(int line) {
        var list = playerUnitsObserver.GetAllFieldUnits(line);
        list.AddRange(enemyUnitsObserver.GetAllFieldUnits(line));
        list.ForEach(x => x.GetComponent<PlaceMonster>().atkCount = 0);
    }

    IEnumerator whoFirstBattle(PlayerController first, PlayerController second, FieldUnitsObserver firstObserver, int line) {
        var list = firstObserver.GetAllFieldUnits(line);
        if(list.Count == 0) {
            yield return WaitSocketData(socketHandler.lineBattleList, line, true);
            shieldDequeue();
        }
        else {
            yield return GetBattle(first, line);
        }
        yield return GetBattle(second, line);
        CheckUnitStatus(line);
        yield return WaitSocketData(socketHandler.mapClearList, line, false);
        yield return GetBattle(first, line);
        yield return GetBattle(second, line);
        CheckUnitStatus(line);
        yield return WaitSocketData(socketHandler.mapClearList, line, false);
    }

    IEnumerator GetBattle(PlayerController player, int line) {
        yield return WaitSocketData(socketHandler.lineBattleList, line, true);
        yield return battleUnit(player.backLine, line);
        yield return battleUnit(player.frontLine, line);
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
        monster.CheckDebuff();
    }

    IEnumerator battleUnit(GameObject lineObject, int line) {
        if (!isGame) yield break;
        if (lineObject.transform.GetChild(line).childCount == 0) yield break;
        PlaceMonster placeMonster = lineObject.transform.GetChild(line).GetChild(0).GetComponent<PlaceMonster>();
        if (placeMonster.atkCount >= placeMonster.maxAtkCount) yield break;
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
        Queue<SocketFormat.Player> data;
        data = player.isHuman ? socketHandler.orcData : socketHandler.humanData;
        if (data.Peek().shieldActivate) yield break;
        yield return new WaitForSeconds(0.1f);
        do {
            yield return new WaitForFixedUpdate();
        } while (heroShieldActive);
    }

    public IEnumerator DrawSpecialCard(bool isHuman) {
        yield return socketHandler.WaitGetCard();
        //Logger.Log("쉴드 발동!");
        bool isPlayer = (isHuman == player.isHuman);
        if (isPlayer) {
            CardCircleManager cdpm = FindObjectOfType<CardCircleManager>();
            bool race = player.isHuman;
            SocketFormat.Card cardData = socketHandler.gameState.players.myPlayer(race).newCard;
            battleLineEffect.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 0.608f);
            battleLineEffect.gameObject.SetActive(false);
            yield return StartCoroutine(cdpm.DrawHeroCard(cardData));
            battleLineEffect.gameObject.SetActive(true);
            battleLineEffect.GetComponent<SpriteRenderer>().color = new Color(1, 0.384f, 0.121f, 0.608f);
        }
        else {
            GameObject enemyCard;
            if (enemyPlayer.isHuman)
                enemyCard = Instantiate(Resources.Load("Prefabs/HumanBackCard") as GameObject, enemyPlayer.playerUI.transform.Find("CardSlot").GetChild(CountEnemyCard()));
            else
                enemyCard = Instantiate(Resources.Load("Prefabs/OrcBackCard") as GameObject, enemyPlayer.playerUI.transform.Find("CardSlot").GetChild(CountEnemyCard()));

            enemyCard.transform.SetParent(PlayMangement.instance.enemyPlayer.playerUI.transform.Find("CardSlot").GetChild(PlayMangement.instance.CountEnemyCard()));
            enemyCard.transform.localScale = new Vector3(1, 1, 1);
            enemyCard.transform.localPosition = new Vector3(0, 0, 0);
            enemyCard.SetActive(true);
        }
        yield return new WaitForSeconds(1f);
        if (isPlayer) socketHandler.TurnOver();
        yield return WaitShieldDone();

    }

    public IEnumerator WaitShieldDone() {
        do {
            yield return new WaitForFixedUpdate();
            //스킬 사용
            if(socketHandler.useCardList.Count != 0) {
                SocketFormat.GameState state = socketHandler.getHistory();
                SocketFormat.PlayHistory history = state.lastUse;
                if (history != null) {
                    GameObject summonedMagic = SummonMagic(history);
                    summonedMagic.GetComponent<MagicDragHandler>().isPlayer = false;
                    yield return MagicActivate(summonedMagic, history);
                    SocketFormat.DebugSocketData.SummonCardData(history);
                    yield return new WaitForSeconds(1f);
                }
            }
        } while (heroShieldActive);
    }
}

/// <summary>
/// 승패 적용
/// </summary>
public partial class PlayMangement {

    public GameObject resultUI;
    public GameObject SocketDisconnectedUI;

    public void GetBattleResult() {
        isGame = false;
        resultUI.SetActive(true);

        if (player.HP.Value <= 0) {
            if (player.isHuman)
                SetResultWindow("lose", "human");
            else
                SetResultWindow("lose", "orc");
        }
        else if (enemyPlayer.HP.Value <= 0) {
            if (player.isHuman)
                SetResultWindow("win", "human");
            else
                SetResultWindow("win", "orc");
        }
    }

    public void OnReturnBtn() {
        if (resultUI.transform.GetChild(0).gameObject.activeSelf) {
            resultUI.transform.GetChild(0).gameObject.SetActive(false);
            resultUI.transform.GetChild(1).gameObject.SetActive(true);
        }
        else if (resultUI.transform.GetChild(1).gameObject.activeSelf) {
            SceneManager.Instance.LoadScene(SceneManager.Scene.MAIN_SCENE);
        }
    }

    public void SocketErrorUIOpen(bool friendOut) {
        SocketDisconnectedUI.SetActive(true);
        if(friendOut)
            SocketDisconnectedUI.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = "상대방이 게임을 \n 종료했습니다.";
    }

    public void OnMoveSceneBtn() {
        SceneManager.Instance.LoadScene(SceneManager.Scene.MAIN_SCENE);
    }

    private void SetResultWindow(string result, string race) {
        Transform baseWindow = resultUI.transform.GetChild(0);
        Transform resourceWindow = resultUI.transform.GetChild(1);
        baseWindow.gameObject.SetActive(true);
        switch (result) {
            case "win":
                if (race == "human") {
                    baseWindow.Find("ResultCharacter/ResultHuman").gameObject.SetActive(true);
                    resourceWindow.Find("ResourceResultRibon/HumanRibon").gameObject.SetActive(true);
                }
                else {
                    baseWindow.Find("ResultCharacter/ResultOrc").gameObject.SetActive(true);
                    resourceWindow.Find("ResourceResultRibon/OrcRibon").gameObject.SetActive(true);
                }
                baseWindow.Find("ShineEffect/WinShineEffect").gameObject.SetActive(true);
                resourceWindow.Find("ShineEffect/WinShineEffect").gameObject.SetActive(true);
                baseWindow.Find("ResultCharacter/ResultText/WinText").gameObject.SetActive(true);
                resourceWindow.Find("ResultText/WinText").gameObject.SetActive(true);
                break;
            case "lose":
                if (race == "human") {
                    baseWindow.Find("ResultCharacter/ResultHuman").gameObject.SetActive(true);
                }
                else {
                    baseWindow.Find("ResultCharacter/ResultOrc").gameObject.SetActive(true);
                }
                baseWindow.Find("ShineEffect/LoseShineEffect").gameObject.SetActive(true);
                resourceWindow.Find("ShineEffect/LoseShineEffect").gameObject.SetActive(true);
                baseWindow.Find("ResultCharacter/LoseRibon").gameObject.SetActive(true);
                resourceWindow.Find("LoseRibon").gameObject.SetActive(true);
                baseWindow.Find("ResultCharacter/ResultText/LoseText").gameObject.SetActive(true);
                resourceWindow.Find("ResultText/LoseText").gameObject.SetActive(true);
                break;
            default:
                break;
        }
        resourceWindow.gameObject.SetActive(false);
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
        while (i < 5) {
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
            enemyCard.SetActive(true);
            i++;
        }
    }

    public void EndTurnDraw() {
        if (isGame == false) return;
        bool race = player.isHuman;
        SocketFormat.Card cardData = socketHandler.gameState.players.myPlayer(race).newCard;
        player.cdpm.AddCard(null, cardData);

        GameObject enemyCard;
        if (enemyPlayer.isHuman)
            enemyCard = Instantiate(Resources.Load("Prefabs/HumanBackCard") as GameObject, enemyPlayer.playerUI.transform.Find("CardSlot").GetChild(CountEnemyCard()));
        else
            enemyCard = Instantiate(Resources.Load("Prefabs/OrcBackCard") as GameObject, enemyPlayer.playerUI.transform.Find("CardSlot").GetChild(CountEnemyCard()));
        enemyCard.transform.position = player.cdpm.cardSpawnPos.position;
        enemyCard.transform.localScale = new Vector3(1, 1, 1);
        iTween.MoveTo(enemyCard, enemyCard.transform.parent.position, 0.3f);
        enemyCard.SetActive(true);
    }

    public IEnumerator EnemyMagicCardDraw(int drawNum) {
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

    [SerializeField] FieldUnitsObserver playerUnitsObserver;
    public FieldUnitsObserver PlayerUnitsObserver {
        get {
            return playerUnitsObserver;
        }
    }

    [SerializeField] FieldUnitsObserver enemyUnitsObserver;
    public FieldUnitsObserver EnemyUnitsObserver {
        get {
            return enemyUnitsObserver;
        }
    }
}

public partial class PlayMangement {
    [SerializeField] Transform turnTable;
    private GameObject releaseTurnBtn;
    private GameObject nonplayableTurnArrow;
    private GameObject playableTurnArrow;
    private Transform turnIcon;

    public void InitTurnTable() {
        string race = Variables.Saved.Get("SelectedRace").ToString();
        bool isHuman;
        if (race == "HUMAN") isHuman = true;
        else isHuman = false;
        if (isHuman) {
            releaseTurnBtn = turnTable.Find("HumanButton").gameObject;
            //turnTable.GetChild(1).GetChild(0).gameObject.SetActive(true);
            //turnTable.Find("ReleaseTurnButton/HumanTurnButtonImage").gameObject.SetActive(true);
        }
        else {
            releaseTurnBtn = turnTable.Find("OrcButton").gameObject;
            //turnIcon = turnTable.GetChild(5);
            //turnTable.GetChild(1).GetChild(1).gameObject.SetActive(true);
            //turnTable.Find("ReleaseTurnButton/OrcTurnButtonImage").gameObject.SetActive(true);
        }
        for (int i = 0; i < 4; i++) {
            turnTable.Find("TurnBoard").position = canvas.transform.GetChild(2).GetChild(2).position;
        }
        //turnIcon.gameObject.SetActive(true);
        //turnIcon.GetChild(0).gameObject.SetActive(true);
        //nonplayableTurnArrow.SetActive(true);
    }

    private IEnumerator SetHumanTurnTable(string currentTurn) {
        yield return new WaitForSeconds(0.4f);
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

    private IEnumerator SetOrcTurnTable(string currentTurn) {
        yield return new WaitForSeconds(0.4f);
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
        releaseTurnBtn.transform.Find("TurnOverFeedback").gameObject.SetActive(turnOn);
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