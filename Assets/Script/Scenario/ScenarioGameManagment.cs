using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using Tutorial;
using System.Reflection;
using System;
using TMPro;
using UnityEngine.Events;

public class ScenarioGameManagment : PlayMangement {
    public static ChapterData chapterData;

    Queue<ScriptData> chapterQueue;
    ScriptData currentChapterData;
    Method currentMethod;
    public static ScenarioGameManagment scenarioInstance;
    public bool isTutorial;

    Type thisType;
    public bool canNextChapter = true;
    public bool canHeroCardToHand = true;
    public bool stopEnemySummon = false;
    public bool stopEnemySpell = false;

    public int forcedSummonAt = -1;

    bool canBattleProceed = true;
    int battleStopAt = 0;

    public Transform showCardPos;
    public ScenarioExecute currentExecute;
    private void Awake() {
        socketHandler = FindObjectOfType<BattleConnector>();
        instance = this;
        scenarioInstance = this;
        isTutorial = true;
        SetWorldScale();
        SetPlayerCard();
        GetComponent<TurnMachine>().onTurnChanged.AddListener(ChangeTurn);
        GetComponent<TurnMachine>().onPrepareTurn.AddListener(DistributeCard);
        SetCamera();

        thisType = GetType();
        if (!InitQueue()) Logger.LogError("chapterData가 제대로 세팅되어있지 않습니다!");
    }

    private bool InitQueue() {
        if (chapterData == null) return false;

        chapterQueue = new Queue<ScriptData>();
        foreach (ScriptData scriptData in chapterData.scripts) {
            chapterQueue.Enqueue(scriptData);
        }
        ScenarioMask.Instance.gameObject.SetActive(true);
        return true;
    }

    void Start() {
        SetBackGround();
        InitGameData(20,10);
    }

    void OnDestroy() {
        instance = null;
        scenarioInstance = null;

        if (socketHandler != null)
            Destroy(socketHandler.gameObject);
    }

    void FixedUpdate() {
        if (!canNextChapter) return;
        DequeueChapter();
    }

    private void DequeueChapter() {
        canNextChapter = false;
        if(chapterQueue.Count == 0) {
            //EndTutorial();
            return;
        }
        currentChapterData = chapterQueue.Dequeue();
        GetComponent<ScenarioExecuteHandler>().Initialize(currentChapterData);
    }

    public override IEnumerator EnemyUseCard(bool isBefore) {
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
                    #region tutorial 추가 제어
                    yield return new WaitUntil(() => !stopEnemySummon);
                    #endregion

                    //카드 정보 만들기
                    GameObject summonUnit = MakeUnitCardObj(history);
                    //카드 정보 보여주기
                    yield return UnitActivate(history);
                }
                else {
                    #region tutorial 추가 제어
                    yield return new WaitUntil(() => !stopEnemySpell);
                    #endregion
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

    public override IEnumerator battleCoroutine() {
        dragable = false;
        yield return new WaitForSeconds(1.1f);
        yield return socketHandler.waitSkillDone(() => { });
        yield return socketHandler.WaitBattle();
        for (int line = 0; line < 5; line++) {
            #region 튜토리얼 추가 제어
            if (!canBattleProceed && (line == battleStopAt - 1)) yield return new WaitUntil(() => canBattleProceed == true);
            #endregion
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


    public void BattleResume() {
        canBattleProceed = true;
    }

    public void StopBattle(int line) {
        battleStopAt = line;
        canBattleProceed = false;
    }

    //IEnumerator ChapterScript() {
    //    while(chapterQueue.Count > 0) {
    //        while(chapterQueue.Peek().isExecute == false) {
    //            DequeueChapter();                
    //        }
    //    }
    //    yield return null;
    //}

    //IEnumerator ExecuteMethod(int methodNum) {
    //    ScenarioExecute dataExecute = (ScenarioExecute)Activator.CreateInstance(Type.GetType(chapterQueue.Peek().methods[methodNum].name));
    //    dataExecute.args = chapterQueue.Peek().methods[methodNum].args;
    //    dataExecute.Execute();     
    //    yield return new WaitUntil(() => dataExecute.handler.isDone == true);
    //}


}


public class MissionRequire  {
    public string args;
    public bool isCheck;
    public GameObject targetObject;   
}
