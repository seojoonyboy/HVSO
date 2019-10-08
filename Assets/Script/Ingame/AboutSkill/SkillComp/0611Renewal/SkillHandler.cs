using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using dataModules;
using UnityEngine;
using SocketFormat;

namespace SkillModules {
    public class SkillHandler {
        public Skill[] skills;
        public GameObject myObject;
        public object skillTarget;
        private object additionalArgs;

        public bool isPlayer;
        public object targetData;
        private List<IngameEventHandler.EVENT_TYPE> triggerList;
        public GameObject finalTarget;
        public bool isDone = true;
        public delegate bool DragFilter(GameObject TestObject);
        public DragFilter dragFiltering;
        public bool socketDone = true;
        public bool finallyDone = true;
        public Transform highlight;
        private int end_Card_Count = 0;
        private int coroutineCount = 0;

        string targetType;

        //TODO : 데이터 세팅
        public void Initialize (dataModules.Skill[] _skills, GameObject myObject, bool isPlayer) {
            triggerList = new List<IngameEventHandler.EVENT_TYPE> ();
            this.myObject = myObject;
            this.isPlayer = isPlayer;
            //스킬 갯수만큼 한줄씩 넣기
            skills = new Skill[_skills.Length];
            for (int i = 0; i < skills.Length; i++) {
                skills[i] = new Skill ();
                skills[i].Initialize (_skills[i], this);
            }
            SummonNonEndCardTriggerMonster();
        }

        public void RegisterTriggerEvent (IngameEventHandler.EVENT_TYPE triggerType) {
            IngameEventHandler handler = PlayMangement.instance.EventHandler;

            bool triggerExist = triggerList.Exists (x => x == triggerType);
            if (triggerExist) return;

            if (triggerType == IngameEventHandler.EVENT_TYPE.END_CARD_PLAY)
                end_Card_Count++;

            triggerList.Add (triggerType);
            handler.AddListener (triggerType, Trigger);
        }

        public void RemoveTriggerEvent () {
            IngameEventHandler handler = PlayMangement.instance.EventHandler;
            triggerList.ForEach (x => handler.RemoveListener (x, Trigger));
        }

        //TODO : Trigger 관리
        private void Trigger (Enum Event_Type, Component Sender, object Param = null) {
            targetData = Param;
            //if(myObject.GetComponent<CardHandler>() != null) highlight = myObject.GetComponent<CardHandler>().highlightedSlot;

            IngameEventHandler.EVENT_TYPE triggerType = (IngameEventHandler.EVENT_TYPE) Event_Type;
            if(triggerType == IngameEventHandler.EVENT_TYPE.BEGIN_ORC_POST_TURN || triggerType == IngameEventHandler.EVENT_TYPE.END_BATTLE_TURN) {
                AddTurnTriggerUnit(triggerType, Param);
                return;
            }
            PlayMangement.instance.StartCoroutine (SkillTrigger (triggerType, Param));

        }

        static public bool running = false;
        static List<SkillHandler> turnUnitList;

        private void AddTurnTriggerUnit(IngameEventHandler.EVENT_TYPE triggerType, object Param) {
            if(turnUnitList == null) {
                turnUnitList = new List<SkillHandler>();
                PlayMangement.instance.OnBlockPanel(null);
            }
            turnUnitList.Add(this);
            PlayMangement.instance.StartCoroutine(TurnTrigger(triggerType, Param));
        }

        private IEnumerator TurnTrigger(IngameEventHandler.EVENT_TYPE triggerType, object Param) {
            if(running) yield break;
            running = true;
            yield return new WaitForSeconds(1f);
            turnUnitList.Sort(compare);
            foreach(SkillHandler x in turnUnitList)
                yield return x.SkillTrigger(triggerType, Param);
            running = false;
            turnUnitList = null; 
        }

        private int compare(SkillHandler x, SkillHandler y) {
            int X = x.myObject.GetComponent<PlaceMonster>().x;
            int Y = y.myObject.GetComponent<PlaceMonster>().x;
            return X.CompareTo(Y);
        }

        private void SummonNonEndCardTriggerMonster() {
            if(triggerList.Exists(x => IngameEventHandler.EVENT_TYPE.END_CARD_PLAY == x)) return;
            if(!isPlayer) return;
            if(myObject.GetComponent<PlaceMonster>() == null) return;
            BattleConnector connector = PlayMangement.instance.socketHandler;
            MessageFormat format = MessageForm(true);
            connector.UseCard(format);
            if(ScenarioGameManagment.scenarioInstance == null) PlayMangement.instance.UnlockTurnOver();
        }

        IEnumerator SkillTrigger (IngameEventHandler.EVENT_TYPE triggerType, object parms) {
            if(myObject.GetComponent<PlaceMonster>() != null) SendingMessage(false);
            foreach (Skill skill in skills) {
                isDone = false;                
                bool active = skill.Trigger (triggerType, parms);
                if(!active && skill.TargetSelectExist()) SendingMessage(true);
                if (active && !isDone) yield return new WaitUntil (() => isDone);
                PlayMangement.instance.OffBlockPanel();                
            }            
            //유닛 소환이나 마법 카드 사용 했을 때
            isDone = true;
            socketDone = true;
            if(!isPlayer) yield break;
            if(isPlayingCard()) {
                if(ScenarioGameManagment.scenarioInstance == null) PlayMangement.instance.UnlockTurnOver();
                DestroyMyCard();         
            }
        }

        public void SendingMessage(bool after) {
            if(TargetSelectExist() != after) return;
            if(isPlayingCard()) SendSocket();
            if(isFieldCard()) SkillActivate();
        }

        private void DestroyMyCard() {
            MagicDragHandler magic = myObject.GetComponent<MagicDragHandler>();
            if(magic != null) {
                if(!magic.heroCardActivate) {
                    int cardIndex = myObject.transform.parent.GetSiblingIndex();
                    PlayMangement.instance.player.cdpm.DestroyCard(cardIndex);
                    if(PlayMangement.instance.player.isHuman)
                        PlayMangement.instance.player.ActivePlayer();
                    else
                        PlayMangement.instance.player.ActiveOrcTurn();
                }
                else {
                    PlayMangement.instance.player.cdpm.DestroyUsedHeroCard(myObject.transform);
                }
                magic.CARDUSED = true;
                magic.heroCardActivate = false;
            }
        }

        private bool isPlayingCard() {
            if (!isPlayer) return false;
            if (targetData == null) return false;
            if (!targetData.GetType().IsArray) return false;
            if (myObject != (GameObject)(((object[])targetData)[1])) return false;
            if (!triggerList.Exists(x => IngameEventHandler.EVENT_TYPE.END_CARD_PLAY == x)) return false;
            return true;
        }

        private bool isFieldCard() {
            if (!isPlayer) return false;
            if (!TargetSelectExist()) return false;
            if (!triggerList.Exists(x => IngameEventHandler.EVENT_TYPE.BEGIN_ORC_POST_TURN == x)) return false;
            return true;
        }

        private void SkillActivate() {
            BattleConnector connector = PlayMangement.instance.socketHandler;
            MessageFormat format = MessageForm(false);
            if(format.targets.Count() == 0) return;
            connector.UnitSkillActivate(format);
        }


        public void SendSocket() {
            BattleConnector connector = PlayMangement.instance.socketHandler;
            MessageFormat format = MessageForm(true);
            connector.UseCard(format);
        }

        private MessageFormat MessageForm(bool isEndCardPlay) {
            MessageFormat format = new MessageFormat();
            List<Arguments> targets = new List<Arguments>();

            //마법 사용
            if(myObject.GetComponent<MagicDragHandler>() != null) {
                format.itemId = myObject.GetComponent<MagicDragHandler>().itemID;
                targets.Add(ArgumentForm(skills[0], false, isEndCardPlay));
            }
            //유닛 소환
            else if(isEndCardPlay) {
                format.itemId = myObject.GetComponent<PlaceMonster>().itemId;
                targets.Add(UnitArgument());
            }
            else {
                format.itemId = myObject.GetComponent<PlaceMonster>().itemId;
            }
            //Select 스킬 있을 시
            Skill select = skills.ToList().Find(x => x.TargetSelectExist());
            //Playing scope 있는 Select일 때
            if(select != null && select.isPlayingSelect()) {
                List<GameObject> selectList = select.GetTargetFromSelect();
                if(selectList != null && selectList.Count > 0)
                    targets.Add(ArgumentForm(select, true, isEndCardPlay));
            }
            else if(!isEndCardPlay) {
                List<GameObject> selectList = select.GetTargetFromSelect();
                if(selectList != null && selectList.Count > 0)
                    targets.Add(ArgumentForm(select, true, isEndCardPlay));
            }
            
            format.targets = targets.ToArray();
            return format;
        }

        private Arguments UnitArgument() {
            PlayMangement manage = PlayMangement.instance;
            
            string camp = isPlayer != manage.player.isHuman ? "orc" : "human";
            var observer = manage.UnitsObserver;
            int line = observer.GetMyPos(myObject).col;
            var posObject = observer.GetAllFieldUnits(line, manage.player.isHuman);
            string placed = posObject.Count == 1 ? "front" : posObject[0] == myObject ? "rear" : "front";
            return new Arguments("place", new string[]{line.ToString(), camp, placed});
        }

        private Arguments ArgumentForm(Skill skill, bool isSelect, bool isEndCardPlay) {
            Arguments arguments = new Arguments();
            arguments.method = skill.firstTargetArgs();
            PlayerController player = isPlayer ? PlayMangement.instance.player : PlayMangement.instance.enemyPlayer;
            bool isPlayerHuman = player.isHuman;
            bool isOrc;
            List<GameObject> selectList = skill.GetTargetFromSelect();
            List<string> args = new List<string>();

            //타겟이 unit, hero인 경우
            if (arguments.method.Contains("unit")){
                if (arguments.method.Contains("hero")) {
                    //unit인지 hero인지 구분
                    int unitItemId;
                    PlaceMonster monster;
                    //select 스킬인 경우
                    if (isSelect) {
                        monster = selectList[0].GetComponent<PlaceMonster>();
                        //타겟이 영웅?
                        if(monster == null) {
                            if (selectList[0].GetComponentInParent<PlayerController>() != null) {
                                isOrc = (skill.targetCamp().CompareTo("my") == 0) != isPlayerHuman;
                                args.Add(isOrc ? "orc" : "human");

                                args.Add("hero");
                            }
                        }
                        //타겟이 유닛
                        else {
                            monster = GetDropAreaUnit();
                            unitItemId = monster.itemId;
                            args.Add(unitItemId.ToString());
                            isOrc = monster.isPlayer != isPlayerHuman;
                            args.Add(isOrc ? "orc" : "human");
                        }
                    }
                    //select 스킬이 아닌경우
                    else {
                        monster = highlight.GetComponentInParent<PlaceMonster>();
                        //타겟이 영웅?
                        if (monster == null) {
                            if (highlight.GetComponentInParent<PlayerController>() != null) {
                                isOrc = (skill.targetCamp().CompareTo("my") == 0) != isPlayerHuman;
                                args.Add(isOrc ? "orc" : "human");

                                args.Add("hero");
                            }
                        }
                        //타겟이 유닛
                        else {
                            monster = GetDropAreaUnit();
                            unitItemId = monster.itemId;
                            args.Add(unitItemId.ToString());
                            isOrc = monster.isPlayer != isPlayerHuman;
                            args.Add(isOrc ? "orc" : "human");
                        }
                    }
                }
                else {
                    int unitItemId;
                    PlaceMonster monster;
                    if (isSelect) monster = selectList[0].GetComponent<PlaceMonster>();
                    else monster = GetDropAreaUnit();
                    unitItemId = monster.itemId;
                    args.Add(unitItemId.ToString());
                    isOrc = monster.isPlayer != isPlayerHuman;
                    args.Add(isOrc ? "orc" : "human");
                }
            }
            else {
                if (arguments.method.Contains("all")) {
                    isOrc = (skill.targetCamp().CompareTo("my") == 0) != isPlayerHuman;
                    args.Add(isOrc ? "orc" : "human");
                }

                else if (arguments.method.Contains("line")) {
                    if (isSelect) args.Add(selectList[0].GetComponent<PlaceMonster>().x.ToString());
                    else args.Add(GetDropAreaLine().ToString());
                }

                else if (arguments.method.Contains("place")) {
                    int line = selectList[0].transform.GetSiblingIndex();
                    args.Add(line.ToString());
                    if (isEndCardPlay) {
                        isOrc = (((List<GameObject>)skillTarget))[0].GetComponent<PlaceMonster>().isPlayer != isPlayerHuman;
                    }
                    else
                        isOrc = myObject.GetComponent<PlaceMonster>().isPlayer != isPlayerHuman;
                    args.Add(isOrc ? "orc" : "human");
                }
            }
            arguments.args = args.ToArray();
            return arguments;
        }

        private PlaceMonster GetDropAreaUnit() {
            PlaceMonster unit;
            if(highlight != null)
                unit = highlight.GetComponentInParent<PlaceMonster>();
            else
                unit = ((List<GameObject>)skillTarget)[0].GetComponent<PlaceMonster>();
            return unit;
        }
        
        private int GetDropAreaLine() {
            return highlight
                .GetComponentInParent<Terrain>()
                .transform.GetSiblingIndex();
        }

        public string[] targetArgument() {
            return skills[0].TargetArgs();
        }

        public bool TargetSelectExist() {
            return skills.ToList().Exists(x => x.TargetSelectExist());
        }

        public void AddAdditionalArgs(object args) {
            additionalArgs = args;
        }

        public object GetAdditionalArgs() {
            return additionalArgs;
        }
    }
}