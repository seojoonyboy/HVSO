using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IngameMainContorller : MonoBehaviour
{
    public static IngameMainContorller Instance { get; protected set; }
    public PlayerController player, enemyPlayer;
    public GameObject playerCanvas, enemyCanvas;
    public GameObject optionIcon;
    public GameObject backGround;

    public BattleConnector socketHandler;
    public EffectSystem effectSystem;

    public victoryModule.VictoryCondition matchRule;

    public bool skillAction = false;
    public bool stopBattle = false;
    public bool stopTurn = false;
    public bool beginStopTurn = false;
    public bool afterStopTurn = false;
    public bool waitDraw = false;




    private void Awake() {
        Instance = this;
        socketHandler = FindObjectOfType<BattleConnector>();
    }

    private void OnDestroy() {
        if (Instance != null) Instance = null;
    }


    void Start()
    {
        AccountManager.Instance.prevSceneName = "Ingame";
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SyncPlayerHP() {
        if (socketHandler.gameState != null) {
            SocketFormat.Players socketStat = socketHandler.gameState.players;
            PlayerSetHPData((player.isHuman == true) ? socketStat.human.hero.currentHp : socketStat.orc.hero.currentHp,
                         (enemyPlayer.isHuman == true) ? socketStat.human.hero.currentHp : socketStat.orc.hero.currentHp);
        }
    }
    public void PlayerSetHPData(int playerHP, int enemyHP) {
        player.SetHP(playerHP);
        enemyPlayer.SetHP(enemyHP);
    }

    public void SetBattleGround() {
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

}
