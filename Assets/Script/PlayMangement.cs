using Bolt;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public partial class PlayMangement : MonoBehaviour
{
    public PlayerController player, enemyPlayer;
    public GameObject card, back;
    public Sprite plantResourceIcon, zombieResourceIcon;

    public GameObject cardDB;
    Camera cam;
    public GameObject uiSlot;
    public bool isGame = true;
    public bool isFirst = true;
    public static PlayMangement instance { get; private set; }
    public GameObject backGround;
    public GameObject onCanvasPosGroup;
    public EffectManager effectManager;

    private int turn = 0;
    public GameObject blockPanel;
    

    private void Awake()
    {
        socketHandler = FindObjectOfType<BattleConnector>();
        StartCoroutine(SendReadyToSocket());
        //string selectedRace = Variables.Saved.Get("SelectedRace").ToString();

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

    private void SetWorldScale() {
        SpriteRenderer backSprite = backGround.GetComponent<SpriteRenderer>();
        float height = Camera.main.orthographicSize * 2, width = height / Screen.height * Screen.width;       

        backGround.transform.localScale = new Vector3(width / backSprite.sprite.bounds.size.x, width / backSprite.sprite.bounds.size.x, 1);
        GameObject canvas = GameObject.Find("Canvas");
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
            player.card = cardDB.transform.Find("Card").gameObject;
            player.back = cardDB.transform.Find("HumanBackCard").gameObject;
            enemyPlayer.card = cardDB.transform.Find("Card").gameObject; ;
            enemyPlayer.back = cardDB.transform.Find("OrcBackCard").gameObject;
        }
        else {
            player.card = cardDB.transform.Find("Card").gameObject;
            player.back = cardDB.transform.Find("OrcBackCard").gameObject;
            enemyPlayer.card = cardDB.transform.Find("Card").gameObject;
            enemyPlayer.back = cardDB.transform.Find("HumanBackCard").gameObject;
        }
    }

    public void DistributeCard() {
        StartCoroutine(player.GenerateCard());
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
        int i = 0;
        CardData cardData;
        CardDataPackage cardDataPackage = AccountManager.Instance.cardPackage;
        string cardID;
        int enemyCardCount = enemyPlayer.playerUI.transform.Find("CardSlot").childCount;
        GameObject skeleton;


        if (enemyPlayer.isHuman == false)
            cardID = "ac10018";
        else
            cardID = "ac10001";


        while (i < enemyCardCount) {
            if (enemyCardCount < 0) break;
            if (i >= 3) break;
            if (isGame == false) break;
            if (enemyPlayer.transform.GetChild(0).GetChild(i).childCount != 0) { i++; continue; }
            if (cardDataPackage.data.ContainsKey(cardID) == false) { i++; continue; }


            yield return new WaitForSeconds(0.5f);
            cardData = cardDataPackage.data[cardID];
            skeleton = AccountManager.Instance.resource.cardSkeleton[cardID];

            if (enemyPlayer.resource.Value < cardData.cost) break;

            GameObject monster = Instantiate(enemyPlayer.card.GetComponent<CardHandler>().unit);

            monster.transform.SetParent(enemyPlayer.transform.GetChild(0).GetChild(i));
            monster.transform.position = enemyPlayer.transform.GetChild(0).GetChild(i).position;
            GameObject monsterSkeleton = Instantiate(skeleton, monster.transform);
            monsterSkeleton.name = "skeleton";

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


            monster.GetComponent<PlaceMonster>().Init();
            monster.GetComponent<PlaceMonster>().SpawnUnit();

            EnemyUnitsObserver.UnitAdded(monster, i, 0);

            enemyPlayer.resource.Value -= cardData.cost;
            Destroy(enemyPlayer.playerUI.transform.Find("CardSlot").GetChild(0).gameObject);
            i++;
        }

        #region socket use Card
        /*while(true) {
            yield return socketHandler.WaitUseCard();
            //TODO : 카드 소환
            Debug.Log("card summon!");
            SocketFormat.PlayHistory history = socketHandler.gameState.lastUse;
            Debug.Log("summon pos : " + history.target.args[1]);
            Debug.Log("summon id : " + history.cardItem.id);
        }*/
        #endregion

        yield return new WaitForSeconds(1.0f);

        enemyPlayer.ReleaseTurn();
        StopCoroutine("EnemySummonMonster");
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
                    player.ActivePlayer();
                    enemyPlayer.DisablePlayer();
                }
                else {
                    player.DisablePlayer();
                    enemyPlayer.ActivePlayer();
                    StartCoroutine("EnemySummonMonster");
                }
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
                break;

            case "SECRET":
                if (player.isHuman == false) {
                    player.ActivePlayer();
                    enemyPlayer.DisablePlayer();
                }
                else {
                    player.DisablePlayer();
                    StartCoroutine("WaitSecond");
                }
                break;
            case "BATTLE":
                StartBattle();
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
            if (player.isHuman == false) {
                if (player.backLine.transform.GetChild(line).childCount != 0) {
                    PlaceMonster placeMonster = player.backLine.transform.GetChild(line).GetChild(0).GetComponent<PlaceMonster>();
                    
                    while(placeMonster.atkCount < placeMonster.maxAtkCount) {
                        if (placeMonster.unit.attack <= 0)
                            break;

                        placeMonster.GetTarget();
                        yield return new WaitForSeconds(1.1f + placeMonster.atkTime);

                    }
                    placeMonster.atkCount = 0;
                }

                if (player.frontLine.transform.GetChild(line).childCount != 0) {
                    PlaceMonster placeMonster = player.backLine.transform.GetChild(line).GetChild(0).GetComponent<PlaceMonster>();

                    while (placeMonster.atkCount < placeMonster.maxAtkCount) {
                        if (placeMonster.unit.attack <= 0)
                            break;

                        placeMonster.GetTarget();
                        yield return new WaitForSeconds(1.1f + placeMonster.atkTime);

                    }
                    placeMonster.atkCount = 0;
                }

                if (enemyPlayer.backLine.transform.GetChild(line).childCount != 0) {
                    PlaceMonster placeMonster = enemyPlayer.backLine.transform.GetChild(line).GetChild(0).GetComponent<PlaceMonster>();

                    while (placeMonster.atkCount < placeMonster.maxAtkCount) {
                        if (placeMonster.unit.attack <= 0)
                            break;

                        placeMonster.GetTarget();
                        yield return new WaitForSeconds(1.1f + placeMonster.atkTime);

                    }
                    placeMonster.atkCount = 0;
                }

                if (enemyPlayer.frontLine.transform.GetChild(line).childCount != 0) {
                    PlaceMonster placeMonster = enemyPlayer.frontLine.transform.GetChild(line).GetChild(0).GetComponent<PlaceMonster>();

                    while (placeMonster.atkCount < placeMonster.maxAtkCount) {
                        if (placeMonster.unit.attack <= 0)
                            break;

                        placeMonster.GetTarget();
                        yield return new WaitForSeconds(1.1f + placeMonster.atkTime);

                    }
                    placeMonster.atkCount = 0;
                }
            }

            else {
                if (enemyPlayer.backLine.transform.GetChild(line).childCount != 0) {
                    PlaceMonster placeMonster = enemyPlayer.backLine.transform.GetChild(line).GetChild(0).GetComponent<PlaceMonster>();

                    while (placeMonster.atkCount < placeMonster.maxAtkCount) {
                        if (placeMonster.unit.attack <= 0)
                            break;

                        placeMonster.GetTarget();
                        yield return new WaitForSeconds(1.1f + placeMonster.atkTime);

                    }
                    placeMonster.atkCount = 0;
                }

                if (enemyPlayer.frontLine.transform.GetChild(line).childCount != 0) {
                    PlaceMonster placeMonster = enemyPlayer.frontLine.transform.GetChild(line).GetChild(0).GetComponent<PlaceMonster>();

                    while (placeMonster.atkCount < placeMonster.maxAtkCount) {
                        if (placeMonster.unit.attack <= 0)
                            break;

                        placeMonster.GetTarget();
                        yield return new WaitForSeconds(1.1f + placeMonster.atkTime);

                    }
                    placeMonster.atkCount = 0;
                }
                if (player.backLine.transform.GetChild(line).childCount != 0) {
                    PlaceMonster placeMonster = player.backLine.transform.GetChild(line).GetChild(0).GetComponent<PlaceMonster>();

                    while (placeMonster.atkCount < placeMonster.maxAtkCount) {
                        if (placeMonster.unit.attack <= 0)
                            break;

                        placeMonster.GetTarget();
                        yield return new WaitForSeconds(1.1f + placeMonster.atkTime);

                    }
                    placeMonster.atkCount = 0;
                }

                if (player.frontLine.transform.GetChild(line).childCount != 0) {
                    PlaceMonster placeMonster = player.backLine.transform.GetChild(line).GetChild(0).GetComponent<PlaceMonster>();

                    while (placeMonster.atkCount < placeMonster.maxAtkCount) {
                        if (placeMonster.unit.attack <= 0)
                            break;

                        placeMonster.GetTarget();
                        yield return new WaitForSeconds(1.1f + placeMonster.atkTime);

                    }
                    placeMonster.atkCount = 0;
                }
            }

            if (player.backLine.transform.GetChild(line).childCount != 0)
                player.backLine.transform.GetChild(line).GetChild(0).GetComponent<PlaceMonster>().CheckHP();
            if (player.frontLine.transform.GetChild(line).childCount != 0)
                player.frontLine.transform.GetChild(line).GetChild(0).GetComponent<PlaceMonster>().CheckHP();
            if (enemyPlayer.backLine.transform.GetChild(line).childCount != 0)
                enemyPlayer.backLine.transform.GetChild(line).GetChild(0).GetComponent<PlaceMonster>().CheckHP();
            if (enemyPlayer.frontLine.transform.GetChild(line).childCount != 0)
                enemyPlayer.frontLine.transform.GetChild(line).GetChild(0).GetComponent<PlaceMonster>().CheckHP();


            if (isGame == false) break;
            line++;
        }
        yield return new WaitForSeconds(1f);
        PlayMangement.instance.socketHandler.TurnOver();
        turn++;
        yield return PlayMangement.instance.socketHandler.WaitGetCard();
        DistributeResource();
        player.EndTurnDraw();
        yield return new WaitForSeconds(2.0f);
        CustomEvent.Trigger(gameObject, "EndTurn");
        StopCoroutine("battleCoroutine");
    }
}

/// <summary>
/// 승패 적용
/// </summary>
public partial class PlayMangement {

    public GameObject resultUI;


    public void GetBattleResult() {
        isGame = false;

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

    IEnumerator SendReadyToSocket() {
        yield return new WaitForSeconds(1.0f);

        Debug.Log("Client_ready 전송");
        socketHandler.ClientReady();
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