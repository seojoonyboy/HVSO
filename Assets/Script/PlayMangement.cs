using Bolt;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public partial class PlayMangement : MonoBehaviour {
    public PlayerController player, enemyPlayer;
    public Sprite plantResourceIcon, zombieResourceIcon;

    public GameObject cardDB;
    Camera cam;
    public GameObject uiSlot;
    public GameObject canvas;
    public bool isGame = true;
    public bool isMulligan = true;
    public bool infoOn = false;
    public static PlayMangement instance { get; private set; }
    public GameObject backGround;
    public GameObject onCanvasPosGroup;
    public EffectManager effectManager;

    private int turn = 0;
    public GameObject blockPanel;
    public int unitNum = 0;
    public bool heroShieldActive = false;
    public GameObject humanShield, orcShield;
    public static GameObject movingCard;
    private void Awake()
    {
        socketHandler = FindObjectOfType<BattleConnector>();
        socketHandler.ClientReady();

        SetWorldScale();
        instance = this;
        SetPlayerCard();
        gameObject.GetComponent<TurnChanger>().onTurnChanged.AddListener(() => ChangeTurn());
        gameObject.GetComponent<TurnChanger>().onPrepareTurn.AddListener(() => DistributeCard());
        GameObject backGroundEffect = Instantiate(effectManager.backGroundEffect);
        backGroundEffect.transform.position = backGround.transform.Find("ParticlePosition").position;
        SetCamera();
        InitTurnTable();
    }
    private void OnDestroy()
    {
        instance = null;
    }

    private void Start()
    {
        cam = Camera.main;
        SetBackGround();
        RequestStartData();
        DistributeResource();
        //StartCoroutine(DisconnectTest());
    }

    private void Update() {
        if (!infoOn && Input.GetMouseButtonDown(0)) {
            canvas.transform.GetChild(3).GetComponent<CardListManager>().OpenUnitInfoWindow(Input.mousePosition);
        }
    }

    private void SetWorldScale() {
        SpriteRenderer backSprite = backGround.GetComponent<SpriteRenderer>();
        float ratio = (float)Screen.height / Screen.width;
        Debug.Log(ratio);

        float height = Camera.main.orthographicSize * 2, width = height / Screen.height * Screen.width;
        float backgroundScale;

        if (ratio > 1.77f) {
            backgroundScale = width / backSprite.sprite.bounds.size.x;
            backGround.transform.localScale = new Vector3(backgroundScale, backgroundScale, 1);
        }
        else {
            backgroundScale = height / backSprite.sprite.bounds.size.y;
            backGround.transform.localScale = new Vector3(backgroundScale, backgroundScale, 1); ;
        }
        Debug.Log(Mathf.RoundToInt((float) 1f / backgroundScale));

        canvas = GameObject.Find("Canvas");
        Vector3 canvasScale = canvas.transform.localScale;
        canvas.transform.localScale = new Vector3(canvasScale.x * backGround.transform.localScale.x, canvasScale.y * backGround.transform.localScale.y, 1);

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

        for(int i = 0; i < player.frontLine.transform.childCount; i++) {
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
            enemyPlayer.card = cardDB.transform.Find("Card").gameObject; ;
            enemyPlayer.back = cardDB.transform.Find("OrcBackCard").gameObject;
        }
        else {
            //player.card = cardDB.transform.Find("Card").gameObject;
            player.back = cardDB.transform.Find("OrcBackCard").gameObject;
            enemyPlayer.card = cardDB.transform.Find("Card").gameObject;
            enemyPlayer.back = cardDB.transform.Find("HumanBackCard").gameObject;
        }
    }

    public void RequestStartData() {
        player.SetPlayerStat(20);
        enemyPlayer.SetPlayerStat(20);

    }

    public void DistributeResource() {
        player.resource.Value  = turn + 1;
        enemyPlayer.resource.Value  = turn + 1;
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

    IEnumerator EnemySummonMonster() {
        yield return new WaitForSeconds(1.0f);
        #region socket use Card
        while(!socketHandler.cardPlayFinish()) {
            yield return socketHandler.useCardList.WaitNext();
            SocketFormat.PlayHistory history = socketHandler.getHistory();
            if(history != null) {
                if (history.cardItem.type.CompareTo("unit") == 0) {
                    GameObject summonedMonster = SummonMonster(history);

                    object[] parms = new object[] { false, summonedMonster };
                    EventHandler.PostNotification(IngameEventHandler.EVENT_TYPE.END_CARD_PLAY, this, parms);
                }

                else SummonMagic(history);
                SocketFormat.DebugSocketData.SummonCardData(history);
            }
            yield return new WaitForSeconds(0.5f);
        }
        #endregion

        yield return new WaitForSeconds(1.0f);
        SocketFormat.DebugSocketData.ShowHandCard(socketHandler.gameState.players.enemyPlayer(enemyPlayer.isHuman).deck.handCards);
        enemyPlayer.ReleaseTurn();
        StopCoroutine("EnemySummonMonster");
    }

    private GameObject SummonMagic(SocketFormat.PlayHistory history) {
        int i = int.Parse(history.target.args[0]);
        CardData cardData;
        CardDataPackage cardDataPackage = AccountManager.Instance.cardPackage;
        int enemyCardCount = CountEnemyCard();

        string id = history.cardItem.id;

        cardData = cardDataPackage.data[id];
        Debug.Log("use Magic Card" + history.cardItem.name);
        enemyPlayer.resource.Value -= cardData.cost;

        //TODO : EVENT : END_CARD_PLAY 호출
        Destroy(enemyPlayer.playerUI.transform.Find("CardSlot").GetChild(enemyCardCount - 1).GetChild(0).gameObject);
        return null;
    }

    private GameObject SummonMonster(SocketFormat.PlayHistory history) {
        int i = int.Parse(history.target.args[0]);
        CardData cardData;
        CardDataPackage cardDataPackage = AccountManager.Instance.cardPackage;
        int enemyCardCount = CountEnemyCard();
        GameObject skeleton;

        string id = history.cardItem.id;

        cardData = cardDataPackage.data[id];
        skeleton = AccountManager.Instance.resource.cardSkeleton[id];

        GameObject monster = Instantiate(enemyPlayer.card.GetComponent<CardHandler>().unit);

        monster.transform.SetParent(enemyPlayer.transform.GetChild(0).GetChild(i));
        monster.transform.position = enemyPlayer.transform.GetChild(0).GetChild(i).position;
        GameObject monsterSkeleton = Instantiate(skeleton, monster.transform);
        monsterSkeleton.name = "skeleton";

        monster.GetComponent<PlaceMonster>().itemId = history.cardItem.itemId;
        monster.GetComponent<PlaceMonster>().unit.HP = (int)cardData.hp;
        monster.GetComponent<PlaceMonster>().unit.currentHP = (int)cardData.hp;
        monster.GetComponent<PlaceMonster>().unit.attack = (int)cardData.attack;
        monster.GetComponent<PlaceMonster>().unit.name = cardData.name;
        monster.GetComponent<PlaceMonster>().unit.type = cardData.type;
        monster.GetComponent<PlaceMonster>().unit.attackRange = cardData.attackRange;
        monster.GetComponent<PlaceMonster>().unit.cost = cardData.cost;
        monster.GetComponent<PlaceMonster>().unit.rarelity = cardData.rarelity;
        monster.GetComponent<PlaceMonster>().unit.id = cardData.cardId;

        if (cardData.category_2 != "") {
            monster.GetComponent<PlaceMonster>().unit.cardCategories = new string[2];
            monster.GetComponent<PlaceMonster>().unit.cardCategories[0] = cardData.category_1;
            monster.GetComponent<PlaceMonster>().unit.cardCategories[1] = cardData.category_2;
        }
        else {
            monster.GetComponent<PlaceMonster>().unit.cardCategories = new string[1];
            monster.GetComponent<PlaceMonster>().unit.cardCategories[0] = cardData.category_1;
        }

        if (cardData.attackTypes.Length > 0) {
            monster.GetComponent<PlaceMonster>().unit.attackType = new string[cardData.attackTypes.Length];
            monster.GetComponent<PlaceMonster>().unit.attackType = cardData.attackTypes;
            
        }

        foreach (dataModules.Skill skill in cardData.skills) {
            foreach (var effect in skill.effects) {
                var newComp = monster.AddComponent(System.Type.GetType("SkillModules.Ability_" + effect.method));
                if (newComp == null) {
                    Debug.LogError(effect.method + "에 해당하는 컴포넌트를 찾을 수 없습니다.");
                }
                else {
                    ((Ability)newComp).InitData(skill, true);
                }
            }
        }


        monster.GetComponent<PlaceMonster>().Init(cardData);
        monster.GetComponent<PlaceMonster>().SpawnUnit();

        EnemyUnitsObserver.UnitAdded(monster, i, 0);

        enemyPlayer.resource.Value -= cardData.cost;
        Destroy(enemyPlayer.playerUI.transform.Find("CardSlot").GetChild(enemyCardCount - 1).GetChild(0).gameObject);

        if(monster.GetComponent<PlaceMonster>().unit.name == "방패병") {
            monster.AddComponent<TmpBuff>();
        }

        return monster;
    }

    public void ChangeTurn() {
        if (isGame == false) return;

        string currentTurn = Variables.Scene(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene()
            ).Get("CurrentTurn").ToString();
        Debug.Log(currentTurn);
        switch (currentTurn) {
            case "ZOMBIE":
                if(player.isHuman == false) {
                    player.ActiveOrcTurn();
                    enemyPlayer.DisablePlayer();
                }
                else {
                    player.DisablePlayer();
                    enemyPlayer.ActivePlayer();
                    StartCoroutine("EnemySummonMonster");
                }
                EventHandler.PostNotification(IngameEventHandler.EVENT_TYPE.BEGIN_ORC_PRE_TURN, this, null);
                break;

            case "PLANT":
                if(player.isHuman == true) {
                    player.ActivePlayer();
                    enemyPlayer.DisablePlayer();
                }
                else {
                    player.DisablePlayer();
                    enemyPlayer.ActivePlayer();
                    StartCoroutine("EnemySummonMonster");
                }
                EventHandler.PostNotification(IngameEventHandler.EVENT_TYPE.BEGIN_HUMAN_TURN, this, null);
                break;

            case "SECRET":
                if (player.isHuman == false) {
                    player.ActiveOrcSpecTurn();
                    enemyPlayer.DisablePlayer();
                }
                else {
                    player.DisablePlayer();
                    StartCoroutine("WaitSecond");
                }
                EventHandler.PostNotification(IngameEventHandler.EVENT_TYPE.BEGIN_ORC_POST_TURN, this, null);
                break;
            case "BATTLE":
                player.DisablePlayer();
                StartBattle();
                EventHandler.PostNotification(IngameEventHandler.EVENT_TYPE.BEGIN_BATTLE_TURN, this, null);
                break;
        }
        if (player.isHuman)
            SetHumanTurnTable(currentTurn);
        else
            SetOrcTurnTable(currentTurn);
    }

    public void StartBattle() {
        StartCoroutine("battleCoroutine");
    }

    public IEnumerator WaitSecond() {
        yield return new WaitForSeconds(5f);
        //Debug.Log("Triggering EndTurn");
        CustomEvent.Trigger(gameObject, "EndTurn");
    }

    public void GetPlayerTurnRelease() {
        CustomEvent.Trigger(gameObject, "EndTurn");
    }

    IEnumerator battleCoroutine() {
        int line = 0;
        yield return new WaitForSeconds(1.1f);
        while (line < 5) {
            yield return battleLine(line);
            yield return WaitSocketData(socketHandler.mapClearList, line, false);
            if (isGame == false) break;
            line++;
        }
        yield return new WaitForSeconds(1f);
        socketHandler.TurnOver();
        turn++;
        yield return socketHandler.WaitGetCard();
        DistributeResource();
        EndTurnDraw();
        yield return new WaitForSeconds(2.0f);
        CustomEvent.Trigger(gameObject, "EndTurn");
        StopCoroutine("battleCoroutine");
    }

    IEnumerator battleLine(int line) {
        backGround.transform.GetChild(line).Find("BattleLineEffect").gameObject.SetActive(true);
        backGround.transform.GetChild(line).Find("BattleLineEffect").GetComponent<SpriteRenderer>().color = new Color(1, 98.0f / 255.0f, 31.0f / 255.0f, 155.0f / 255.0f);
        if (player.isHuman == false) {
            yield return WaitSocketData(socketHandler.lineBattleList, line, true);
            yield return battleUnit(player.backLine, line);
            yield return battleUnit(player.frontLine, line);
            yield return HeroSpecialWait();
            shildDequeue();
            yield return WaitSocketData(socketHandler.lineBattleList, line, true);
            yield return battleUnit(enemyPlayer.backLine, line);
            yield return battleUnit(enemyPlayer.frontLine, line);
            yield return HeroSpecialWait();
            shildDequeue();
        }
        else {
            yield return WaitSocketData(socketHandler.lineBattleList, line, true);
            yield return battleUnit(enemyPlayer.backLine, line);
            yield return battleUnit(enemyPlayer.frontLine, line);
            yield return HeroSpecialWait();
            shildDequeue();
            yield return WaitSocketData(socketHandler.lineBattleList, line, true);
            yield return battleUnit(player.backLine, line);
            yield return battleUnit(player.frontLine, line);
            yield return HeroSpecialWait();
            shildDequeue();
            
        }

        if (player.backLine.transform.GetChild(line).childCount != 0) {
            player.backLine.transform.GetChild(line).GetChild(0).GetComponent<PlaceMonster>().CheckHP();
            player.backLine.transform.GetChild(line).GetChild(0).GetComponent<PlaceMonster>().CheckDebuff();
        }
        if (player.frontLine.transform.GetChild(line).childCount != 0) {
            player.frontLine.transform.GetChild(line).GetChild(0).GetComponent<PlaceMonster>().CheckHP();
            player.frontLine.transform.GetChild(line).GetChild(0).GetComponent<PlaceMonster>().CheckDebuff();
        }
        if (enemyPlayer.backLine.transform.GetChild(line).childCount != 0) {
            enemyPlayer.backLine.transform.GetChild(line).GetChild(0).GetComponent<PlaceMonster>().CheckHP();
            enemyPlayer.backLine.transform.GetChild(line).GetChild(0).GetComponent<PlaceMonster>().CheckDebuff();
        }
        if (enemyPlayer.frontLine.transform.GetChild(line).childCount != 0) {
            enemyPlayer.frontLine.transform.GetChild(line).GetChild(0).GetComponent<PlaceMonster>().CheckHP();
            enemyPlayer.backLine.transform.GetChild(line).GetChild(0).GetComponent<PlaceMonster>().CheckDebuff();
        }
        backGround.transform.GetChild(line).Find("BattleLineEffect").GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 155.0f / 255.0f);
        backGround.transform.GetChild(line).Find("BattleLineEffect").gameObject.SetActive(false);
        EventHandler.PostNotification(IngameEventHandler.EVENT_TYPE.LINE_BATTLE_FINISHED, this);
    }

    IEnumerator battleUnit(GameObject lineObject, int line) {
        if(lineObject.transform.GetChild(line).childCount != 0) {
            PlaceMonster placeMonster = lineObject.transform.GetChild(line).GetChild(0).GetComponent<PlaceMonster>();
                while(placeMonster.atkCount < placeMonster.maxAtkCount) {
                    if (placeMonster.unit.attack <= 0)
                        break;
                    placeMonster.GetTarget();
                yield return new WaitForSeconds(1.1f + placeMonster.atkTime);
                }
            placeMonster.atkCount = 0;
        }
        yield return null;
    }

    IEnumerator WaitSocketData(SocketFormat.QueueSocketList<SocketFormat.GameState> queueList, int line, bool isBattle) {
        yield return queueList.WaitNext();
        SocketFormat.GameState state = queueList.Dequeue();
        Debug.Log("쌓인 데이터 리스트 : " + queueList.Count);
        SocketFormat.DebugSocketData.ShowBattleData(state, line, isBattle);
        //데이터 체크 및 데이터 동기화
        if(!isBattle) SocketFormat.DebugSocketData.CheckBattleSynchronization(state);
        
    }

    private void shildDequeue() {
        socketHandler.humanData.Dequeue();
        socketHandler.orcData.Dequeue();
    }

    public IEnumerator HeroSpecialWait() {
        yield return new WaitForSeconds(0.1f);
        do {
            yield return new WaitForFixedUpdate();
        } while(heroShieldActive);
    }

    public IEnumerator DrawSpecialCard(bool isHuman) {
        yield return socketHandler.WaitGetCard();
        Debug.Log("쉴드 발동!");
        bool isPlayer = (isHuman == player.isHuman);
        if(isPlayer) {
            CardHandDeckManager cdpm = FindObjectOfType<CardHandDeckManager>();
            bool race = player.isHuman;
            SocketFormat.Card cardData = socketHandler.gameState.players.myPlayer(race).newCard;
            cdpm.AddCard(null, cardData);
        }
        else {
            GameObject enemyCard = Instantiate(enemyPlayer.back);
            enemyCard.transform.SetParent(enemyPlayer.playerUI.transform.Find("CardSlot"));
            enemyCard.transform.localScale = new Vector3(1, 1, 1);
            enemyCard.SetActive(true);
        }
        if(isPlayer) socketHandler.TurnOver();
        yield return new WaitForSeconds(1.0f);
        heroShieldActive = false;
    }
}

/// <summary>
/// 승패 적용
/// </summary>
public partial class PlayMangement {

    public GameObject resultUI;


    public void GetBattleResult() {
        isGame = false;
        Destroy(socketHandler);
        resultUI.SetActive(true);

        if (player.HP.Value <= 0) {
            if (player.isHuman)
                SetResultWindow("lose", "human");
            else
                SetResultWindow("lose", "orc");
        }
        else if(enemyPlayer.HP.Value <= 0) {
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

            GameObject enemyCard = new GameObject();
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

        GameObject enemyCard = new GameObject();
        if (enemyPlayer.isHuman)
            enemyCard = Instantiate(Resources.Load("Prefabs/HumanBackCard") as GameObject, enemyPlayer.playerUI.transform.Find("CardSlot").GetChild(CountEnemyCard()));
        else
            enemyCard = Instantiate(Resources.Load("Prefabs/OrcBackCard") as GameObject, enemyPlayer.playerUI.transform.Find("CardSlot").GetChild(CountEnemyCard()));
        enemyCard.transform.position = player.cdpm.cardSpawnPos.position;
        enemyCard.transform.localScale = new Vector3(1, 1, 1);
        iTween.MoveTo(enemyCard, enemyCard.transform.parent.position, 0.3f);
        enemyCard.SetActive(true);
    }

    public int CountEnemyCard() {
        int enemyNum = 0;
        for(int i = 0; i < 10; i++){
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
        ingameCamera = Camera.main;
        cameraPos = Camera.main.transform.position;
    }
    

    public IEnumerator cameraShake(float time) {
        float timer = 0;
        float cameraSize = ingameCamera.orthographicSize;
        while (timer <= time) {
            

            ingameCamera.transform.position = (Vector3)Random.insideUnitCircle * 0.1f + cameraPos;

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

    public void DisconnectSocket() {
        //Destroy(FindObjectOfType<BattleConnector>().gameObject);
        EventHandler.PostNotification(IngameEventHandler.EVENT_TYPE.REMOVE_SOCKET_CONNECTOR, this);

        //SocketManager.OnReceiveSocketMessage.AddListener(() => Debug.Log("On Socket Message Received"));
    }

    IEnumerator DisconnectTest() {
        yield return new WaitForSeconds(8.0f);
        //DisconnectSocket();

        Debug.Log("소켓 커넥터 파괴됨");
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
        releaseTurnBtn = turnTable.GetChild(2).gameObject;
        nonplayableTurnArrow = turnTable.GetChild(3).GetChild(0).gameObject;
        playableTurnArrow = turnTable.GetChild(3).GetChild(1).gameObject;
        if (isHuman) {
            turnIcon = turnTable.GetChild(4);
            turnTable.GetChild(1).GetChild(0).gameObject.SetActive(true);
            turnTable.GetChild(2).GetChild(0).gameObject.SetActive(true);
        }
        else {
            turnIcon = turnTable.GetChild(5);
            turnTable.GetChild(1).GetChild(1).gameObject.SetActive(true);
            turnTable.GetChild(2).GetChild(1).gameObject.SetActive(true);
        }
        turnIcon.gameObject.SetActive(true);
        turnIcon.GetChild(0).gameObject.SetActive(true);
        nonplayableTurnArrow.SetActive(true);
    }

    private void SetHumanTurnTable(string currentTurn) {
        switch (currentTurn) {
            case "ZOMBIE":
                turnIcon.GetChild(3).gameObject.SetActive(false);
                turnIcon.GetChild(0).gameObject.SetActive(true);
                break;
            case "PLANT":
                turnIcon.GetChild(0).gameObject.SetActive(false);
                turnIcon.GetChild(1).gameObject.SetActive(true);
                releaseTurnBtn.SetActive(true);
                nonplayableTurnArrow.SetActive(false);
                playableTurnArrow.SetActive(true);
                break;
            case "SECRET":
                turnIcon.GetChild(1).gameObject.SetActive(false);
                turnIcon.GetChild(2).gameObject.SetActive(true);
                releaseTurnBtn.SetActive(false);
                playableTurnArrow.SetActive(false);
                nonplayableTurnArrow.SetActive(true);
                break;
            case "BATTLE":
                turnIcon.GetChild(2).gameObject.SetActive(false);
                turnIcon.GetChild(3).gameObject.SetActive(true);
                break;
        }
    }

    private void SetOrcTurnTable(string currentTurn) {
        switch (currentTurn) {
            case "ZOMBIE":
                turnIcon.GetChild(3).gameObject.SetActive(false);
                turnIcon.GetChild(0).gameObject.SetActive(true);
                releaseTurnBtn.SetActive(true);
                nonplayableTurnArrow.SetActive(true);
                playableTurnArrow.SetActive(false);
                break;
            case "PLANT":
                turnIcon.GetChild(0).gameObject.SetActive(false);
                turnIcon.GetChild(1).gameObject.SetActive(true);
                releaseTurnBtn.SetActive(false);
                playableTurnArrow.SetActive(true);
                nonplayableTurnArrow.SetActive(false);
                break;
            case "SECRET":
                turnIcon.GetChild(1).gameObject.SetActive(false);
                turnIcon.GetChild(2).gameObject.SetActive(true);
                releaseTurnBtn.SetActive(true);
                nonplayableTurnArrow.SetActive(true);
                playableTurnArrow.SetActive(false);
                break;
            case "BATTLE":
                turnIcon.GetChild(2).gameObject.SetActive(false);
                turnIcon.GetChild(3).gameObject.SetActive(true);
                releaseTurnBtn.SetActive(false);
                playableTurnArrow.SetActive(true);
                nonplayableTurnArrow.SetActive(false);
                break;
        }
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