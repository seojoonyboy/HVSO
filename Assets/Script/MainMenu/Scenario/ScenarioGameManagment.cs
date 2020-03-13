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
    public static ScenarioGameManagment scenarioInstance;
    public bool isTutorial;

    Type thisType;
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
    public GameObject settingModal;

    public GameObject challengeUI;
    public Sprite[] textShadowImages;
    public GameObject shieldTargetLine;
    public GameObject skipButton;  

    public bool blockInfoModal = false;

    private void Awake() {
        socketHandler = FindObjectOfType<BattleConnector>();
        instance = this;
        scenarioInstance = this;
        isTutorial = true;
        SetWorldScale();
        socketHandler.ClientReady();
        SetCamera();
        ReadUICsvFile();
        ReadCsvFile();


        //if (chapterData.chapter == 0 && chapterData.stage_number == 1)
        optionIcon.SetActive(false);

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

        eventHandler.AddListener(IngameEventHandler.EVENT_TYPE.BEGIN_ORC_PRE_TURN, ActiveSkip);
    }

    void OnDestroy() {
        instance = null;
        scenarioInstance = null;
        chapterData = null;

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
        string message = AccountManager.Instance.GetComponent<Fbl_Translator>().GetLocalizedText("UIPopup", "ui_popup_tuto_skipq");

        Modal.instantiate(message, Modal.Type.YESNO, () => {
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
        }
        );
    }


    public override IEnumerator UseCard(bool isPlayer, SocketFormat.PlayHistory history, DequeueCallback callback, object args = null) {
        if (isPlayer == false) {
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
                    SocketFormat.MagicArgs magicArgs = dataModules.JsonReader.Read<SocketFormat.MagicArgs>(args.ToString());
                    /*
                    if (summonedMagic.GetComponent<MagicDragHandler>().cardData.hero_chk == true)
                        yield return EffectSystem.Instance.HeroCutScene(enemyPlayer.isHuman);
                        */
                    yield return MagicActivate(summonedMagic, magicArgs);
                }
                SocketFormat.DebugSocketData.SummonCardData(history);
            }
            enemyPlayer.UpdateCardCount();
            //SocketFormat.DebugSocketData.CheckMapPosition(state);
            yield return new WaitForSeconds(0.5f);
            #endregion
            SocketFormat.DebugSocketData.ShowHandCard(socketHandler.gameState.players.enemyPlayer(enemyPlayer.isHuman).deck.handCards);
        }
        else {

        }
        callback();    
    }


    public void BattleResume() {
        canBattleProceed = true;
    }

    public void StopBattle(int line) {
        battleStopAt = line;
        canBattleProceed = false;
    }

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

    private void ActiveSkip(Enum event_type, Component Sender, object Param) {
        skipButton.GetComponent<Button>().enabled = true;
    }


}


public class MissionRequire  {
    public string args;
    public bool isCheck;
    public GameObject targetObject;   
}
