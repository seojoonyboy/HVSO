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
    public Sprite humanBtn, orcBtn;

    public GameObject cardDB;
    Camera cam;
    public GameObject uiSlot;

    public bool isGame = true;
    public static PlayMangement instance { get; private set; }
    public GameObject backGround;
    public GameObject onCanvasPosGroup;


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
    }
    private void OnDestroy()
    {
        instance = null;
    }

    private void Start()
    {
        cam = Camera.main;
        RequestStartData();
        DistributeResource();

        //StartCoroutine(DisconnectTest());
    }

    private void SetWorldScale() {
        SpriteRenderer backSprite = backGround.GetComponent<SpriteRenderer>();
        float height = Camera.main.orthographicSize * 2, width = height / Screen.height * Screen.width;

        backGround.transform.localScale = new Vector3(width / backSprite.sprite.bounds.size.x, width / backSprite.sprite.bounds.size.x, 1);

    }


    public void SetPlayerCard() {
        if (player.race == true) {
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
        player.resource.Value  += 2;
        enemyPlayer.resource.Value  += 2;
    }


    IEnumerator EnemySummonMonster() {
        int i = 0;
        CardData cardData;
        CardDataPackage cardDataPackage = AccountManager.Instance.cardPackage;
        string cardID;
        int enemyCardCount = enemyPlayer.playerUI.transform.Find("CardSlot").childCount;
        GameObject skeleton;


        if (enemyPlayer.race == false)
            cardID = "ac10012";
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
            skeleton = Resources.Load<GameObject>("Sprite/" + cardID + "/Skeleton_" + cardID);

            if (enemyPlayer.resource.Value < cardData.cost) break;

            GameObject monster = Instantiate(enemyPlayer.card.GetComponent<CardHandler>().unit);

            monster.transform.SetParent(enemyPlayer.transform.GetChild(0).GetChild(i));
            monster.transform.position = enemyPlayer.transform.GetChild(0).GetChild(i).position;
            GameObject monsterSkeleton = Instantiate(skeleton, monster.transform);
            monsterSkeleton.name = "skeleton";

            monster.GetComponent<PlaceMonster>().unit.HP = (int)cardData.hp;
            monster.GetComponent<PlaceMonster>().unit.power = (int)cardData.attack;
            monster.GetComponent<PlaceMonster>().unit.name = cardData.name;
            monster.GetComponent<PlaceMonster>().unit.type = cardData.type;

            monster.GetComponent<PlaceMonster>().Init();
            monster.GetComponent<PlaceMonster>().SpawnUnit();

            enemyPlayer.resource.Value -= cardData.cost;
            Destroy(enemyPlayer.playerUI.transform.Find("CardSlot").GetChild(0).gameObject);
            i++;
        }

        yield return new WaitForSeconds(1f);
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
                if(player.race == false) {
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
                if(player.race == true) {
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
                if (player.race == false) {
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
        while (line < 5) {
            if (player.race == false) {
                if (player.transform.Find("Line_1").GetChild(line).childCount != 0) {
                    player.transform.Find("Line_1").GetChild(line).GetChild(0).GetComponent<PlaceMonster>().GetTarget();

                    if (player.transform.Find("Line_1").GetChild(line).GetChild(0).GetComponent<PlaceMonster>().unit.power > 0)
                        yield return new WaitForSeconds(1.1f + player.transform.Find("Line_1").GetChild(line).GetChild(0).GetComponent<PlaceMonster>().atkTime);
                }

                if (player.transform.Find("Line_2").GetChild(line).childCount != 0) {
                    player.transform.Find("Line_2").GetChild(line).GetChild(0).GetComponent<PlaceMonster>().GetTarget();

                    if (player.transform.Find("Line_2").GetChild(line).GetChild(0).GetComponent<PlaceMonster>().unit.power > 0)
                        yield return new WaitForSeconds(1.1f + player.transform.Find("Line_2").GetChild(line).GetChild(0).GetComponent<PlaceMonster>().atkTime);
                }

                if (enemyPlayer.transform.Find("Line_1").GetChild(line).childCount != 0) {
                    enemyPlayer.transform.Find("Line_1").GetChild(line).GetChild(0).GetComponent<PlaceMonster>().GetTarget();

                    if (enemyPlayer.transform.Find("Line_1").GetChild(line).GetChild(0).GetComponent<PlaceMonster>().unit.power > 0)
                        yield return new WaitForSeconds(1.1f + enemyPlayer.transform.Find("Line_1").GetChild(line).GetChild(0).GetComponent<PlaceMonster>().atkTime);
                }

                if (enemyPlayer.transform.Find("Line_2").GetChild(line).childCount != 0) {
                    enemyPlayer.transform.Find("Line_2").GetChild(line).GetChild(0).GetComponent<PlaceMonster>().GetTarget();

                    if (enemyPlayer.transform.Find("Line_2").GetChild(line).GetChild(0).GetComponent<PlaceMonster>().unit.power > 0)
                        yield return new WaitForSeconds(1.1f + enemyPlayer.transform.Find("Line_2").GetChild(line).GetChild(0).GetComponent<PlaceMonster>().atkTime);
                }
            }

            else {
                if (enemyPlayer.transform.Find("Line_1").GetChild(line).childCount != 0) {
                    enemyPlayer.transform.Find("Line_1").GetChild(line).GetChild(0).GetComponent<PlaceMonster>().GetTarget();

                    if (enemyPlayer.transform.Find("Line_1").GetChild(line).GetChild(0).GetComponent<PlaceMonster>().unit.power > 0)
                        yield return new WaitForSeconds(1.1f + enemyPlayer.transform.Find("Line_1").GetChild(line).GetChild(0).GetComponent<PlaceMonster>().atkTime);
                }

                if (enemyPlayer.transform.Find("Line_2").GetChild(line).childCount != 0) {
                    enemyPlayer.transform.Find("Line_2").GetChild(line).GetChild(0).GetComponent<PlaceMonster>().GetTarget();

                    if (enemyPlayer.transform.Find("Line_2").GetChild(line).GetChild(0).GetComponent<PlaceMonster>().unit.power > 0)
                        yield return new WaitForSeconds(1.1f + enemyPlayer.transform.Find("Line_2").GetChild(line).GetChild(0).GetComponent<PlaceMonster>().atkTime);
                }
                if (player.transform.Find("Line_1").GetChild(line).childCount != 0) {
                    player.transform.Find("Line_1").GetChild(line).GetChild(0).GetComponent<PlaceMonster>().GetTarget();

                    if (player.transform.Find("Line_1").GetChild(line).GetChild(0).GetComponent<PlaceMonster>().unit.power > 0)
                        yield return new WaitForSeconds(1.1f + player.transform.Find("Line_1").GetChild(line).GetChild(0).GetComponent<PlaceMonster>().atkTime);
                }

                if (player.transform.Find("Line_2").GetChild(line).childCount != 0) {
                    player.transform.Find("Line_2").GetChild(line).GetChild(0).GetComponent<PlaceMonster>().GetTarget();

                    if (player.transform.Find("Line_2").GetChild(line).GetChild(0).GetComponent<PlaceMonster>().unit.power > 0)
                        yield return new WaitForSeconds(1.1f + player.transform.Find("Line_2").GetChild(line).GetChild(0).GetComponent<PlaceMonster>().atkTime);
                }
            }

            if (player.transform.Find("Line_1").GetChild(line).childCount != 0)
                player.transform.Find("Line_1").GetChild(line).GetChild(0).GetComponent<PlaceMonster>().CheckHP();
            if (player.transform.Find("Line_2").GetChild(line).childCount != 0)
                player.transform.Find("Line_2").GetChild(line).GetChild(0).GetComponent<PlaceMonster>().CheckHP();
            if (enemyPlayer.transform.Find("Line_1").GetChild(line).childCount != 0)
                enemyPlayer.transform.Find("Line_1").GetChild(line).GetChild(0).GetComponent<PlaceMonster>().CheckHP();
            if (enemyPlayer.transform.Find("Line_2").GetChild(line).childCount != 0)
                enemyPlayer.transform.Find("Line_2").GetChild(line).GetChild(0).GetComponent<PlaceMonster>().CheckHP();


            if (isGame == false) break;
            line++;
        }
        yield return new WaitForSeconds(1f);
        DistributeResource();
        CustomEvent.Trigger(gameObject, "EndTurn");
        player.EndTurnDraw();
        StopCoroutine("battleCoroutine");
    }
}

/// <summary>
/// Socket 관련 처리
/// </summary>
public partial class PlayMangement {
    public IngameEventHandler eventHandler;
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
        SocketFormat.SendFormat format = new SocketFormat.SendFormat("client_ready", new string[] { });
        SocketHandler.SendToSocket(format);
    }
}
