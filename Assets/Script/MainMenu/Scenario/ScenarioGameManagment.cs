using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using Tutorial;
using System.Reflection;
using System;
using System.IO;
using TMPro;
using UnityEngine.Events;
using Spine;
using Spine.Unity;
using UnityEngine.UI;

public class ScenarioGameManagment : PlayMangement {
    public static ChapterData chapterData;
    public static List<ChallengerHandler.Challenge> challengeDatas;
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
    public bool stopNextTurn = false;
    public bool infoWindow = true;

    public int forcedSummonAt = -1;
    public int forcedLine = -1;
    public int forcedTargetAt = -1;
    public int[] multipleforceLine = { -1, -1 };
    public string targetArgs = "";

    bool canBattleProceed = true;
    int battleStopAt = 0;

    public Transform showCardPos;
    public ScenarioExecute currentExecute;
    public GameObject settingModal;

    public GameObject challengeUI;
    public Sprite[] textShadowImages;
    public GameObject shieldTargetLine;
    public GameObject skipButton;
    public GameObject textCanvas;
    public Dictionary<string, string> gameScriptData;

    public string fileName;
    public string key;
    


    public bool blockInfoModal = false;

    private void Awake() {
        socketHandler = FindObjectOfType<BattleConnector>();
        instance = this;
        scenarioInstance = this;
        isTutorial = true;
        SetWorldScale();
        //SetPlayerCard();
        //GetComponent<TurnMachine>().onPrepareTurn.AddListener(DistributeCard);
        socketHandler.ClientReady();
        SetCamera();
        ReadCsvFile();
        //if (chapterData.stage_number > 1) 
        //    skipButton.SetActive(false);


        //if (chapterData.chapter == 0 && chapterData.stage_number == 1)
        optionIcon.SetActive(false);

        thisType = GetType();
        if (!InitQueue()) Logger.LogError("chapterData가 제대로 세팅되어있지 않습니다!");
    }

    private void ReadCsvFile() {
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
            datas[1] = _line.Substring(splitPos+1);
            gameScriptData.Add(datas[0], datas[1]);
        }

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

        bool isHuman = PlayMangement.instance.player.isHuman;
        ChallengerHandler challengerHandler = gameObject.AddComponent<ChallengerHandler>();
        challengerHandler.Init(
            challengeDatas, 
            challengeUI, 
            textShadowImages,
            shieldTargetLine
        );
        //BgmController.BgmEnum soundTrack = (player.isHuman == true) ? BgmController.BgmEnum.FOREST : BgmController.BgmEnum.CITY;
        BgmController.BgmEnum soundTrack = BgmController.BgmEnum.CITY;
        SoundManager.Instance.bgmController.PlaySoundTrack(soundTrack);
    }

    void OnDestroy() {
        instance = null;
        scenarioInstance = null;

        if (socketHandler != null)
            Destroy(socketHandler.gameObject);

        ChallengerHandler handler = GetComponent<ChallengerHandler>();
        if(handler != null) Destroy(handler);
    }

    void FixedUpdate() {
        if (!canNextChapter) return;
        DequeueChapter();
    }

    private void DequeueChapter() {
        canNextChapter = false;
        if(chapterQueue.Count == 0) {
            return;
        }
        currentChapterData = chapterQueue.Dequeue();
        GetComponent<ScenarioExecuteHandler>().Initialize(currentChapterData);
    }

    public void SkipTutorial() {
        Modal.instantiate("정말 튜토리얼을 스킵하시겠습니까?", Modal.Type.YESNO, () => {
            if (GetComponent<ScenarioExecuteHandler>().sets.Count > 0) {
                foreach (var exec in GetComponent<ScenarioExecuteHandler>().sets) { Destroy(exec); }
            }
            chapterQueue.Clear();
            canBattleProceed = true;
            stopEnemySummon = false;
            forcedSummonAt = -1;
            beginStopTurn = false;
            afterStopTurn = false;
            ScenarioMask.Instance.DisableMask();
            textCanvas.SetActive(false);
            challengeUI.SetActive(false);
            SocketHandler.TutorialEnd();

            //FBL_SceneManager.Instance.LoadScene(FBL_SceneManager.Scene.MAIN_SCENE);
        });
    }


    public override IEnumerator EnemyUseCard(SocketFormat.PlayHistory history, DequeueCallback callback) {
        #region socket use Card
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
        enemyPlayer.playerUI.transform.Find("CardCount").GetChild(0).gameObject.GetComponent<Text>().text = (count).ToString();
        //SocketFormat.DebugSocketData.CheckMapPosition(state);
        yield return new WaitForSeconds(0.5f);
        #endregion
        SocketFormat.DebugSocketData.ShowHandCard(socketHandler.gameState.players.enemyPlayer(enemyPlayer.isHuman).deck.handCards);
        callback();
    }

    //public override IEnumerator battleCoroutine() {
    //    dragable = false;
    //    yield return new WaitForSeconds(1.1f);
    //    yield return socketHandler.waitSkillDone(() => { });
    //    for (int line = 0; line < 5; line++) {
            
    //        #region 튜토리얼 추가 제어
    //        if (!canBattleProceed && (line == battleStopAt - 1)) yield return new WaitUntil(() => canBattleProceed == true);
    //        #endregion
    //        yield return StopBattleLine();
    //        EventHandler.PostNotification(IngameEventHandler.EVENT_TYPE.LINE_BATTLE_START, this, line);
    //        if (isGame == false) yield break;
    //    }
    //    yield return new WaitForSeconds(1f);        
    //    socketHandler.TurnOver();
    //    turn++;   
    //    DistributeResource();
    //    eventHandler.PostNotification(IngameEventHandler.EVENT_TYPE.END_BATTLE_TURN, this, null);
    //    yield return new WaitUntil(() => stopNextTurn == false);
    //    EndTurnDraw();
    //    yield return new WaitForSeconds(2.0f);
    //    yield return new WaitUntil(() => !SkillModules.SkillHandler.running);
    //    EventHandler.PostNotification(IngameEventHandler.EVENT_TYPE.END_TURN_BTN_CLICKED, this, TurnType.BATTLE);
    //    //CustomEvent.Trigger(gameObject, "EndTurn");
    //    StopCoroutine("battleCoroutine");
    //    dragable = true;
    //}


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

    public IEnumerator OpponentRanAway() {
        List<SkeletonAnimation> enemySpineList = new List<SkeletonAnimation>();
        enemyPlayer.GetComponentsInChildren<SkeletonAnimation>(enemySpineList);
        for(float alpha = 1f; alpha >= 0f; alpha -= 0.08f) {
            Color color = new Color(1f,1f,1f, alpha);
            for(int i = 0; i < enemySpineList.Count; i++) {
                enemySpineList[i].skeleton.SetColor(color);
            }
            yield return new WaitForFixedUpdate();
        }
        enemyPlayer.gameObject.SetActive(false);
    }


}


public class MissionRequire  {
    public string args;
    public bool isCheck;
    public GameObject targetObject;   
}
