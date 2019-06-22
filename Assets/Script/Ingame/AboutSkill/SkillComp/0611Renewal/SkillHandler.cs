using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using dataModules;
using UnityEngine;
using SocketFormat;

namespace SkillModules {
    public class SkillHandler {
        private Skill[] skills;
        public GameObject myObject;
        public GameObject skillTarget;
        public bool isPlayer;
        public object targetData;
        private List<IngameEventHandler.EVENT_TYPE> triggerList;
        public GameObject finalTarget;
        public bool isDone;

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
            IngameEventHandler.EVENT_TYPE triggerType = (IngameEventHandler.EVENT_TYPE) Event_Type;
            if(triggerType == IngameEventHandler.EVENT_TYPE.BEGIN_ORC_POST_TURN) {
                AddOrcPostTurnUnit(triggerType, Param);
                return;
            }
            PlayMangement.instance.StartCoroutine (SkillTrigger (triggerType, Param));
        }

        static bool running = false;
        static List<SkillHandler> orcList;

        private void AddOrcPostTurnUnit(IngameEventHandler.EVENT_TYPE triggerType, object Param) {
            if(orcList == null) orcList = new List<SkillHandler>();
            orcList.Add(this);
            PlayMangement.instance.StartCoroutine(OrcPostTurnTrigger(triggerType, Param));
        }

        private IEnumerator OrcPostTurnTrigger(IngameEventHandler.EVENT_TYPE triggerType, object Param) {
            if(running) yield break;
            running = true;
            yield return new WaitForSeconds(1f);
            orcList.Sort(compare);
            foreach(SkillHandler x in orcList)
                yield return x.SkillTrigger(triggerType, Param);
            running = false;
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
        }

        IEnumerator SkillTrigger (IngameEventHandler.EVENT_TYPE triggerType, object parms) {
            foreach (Skill skill in skills) {
                isDone = false;
                bool active = skill.Trigger (triggerType, parms);
                if (active && !isDone) yield return new WaitUntil (() => isDone);
            }
            //유닛 소환이나 마법 카드 사용 했을 때
            if(isPlayingCard()) SendSocket();
            //TODO : field에서 select 발동 했을 때
            else if(isFieldCard()) SkillActivate();
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
            if (!triggerList.Exists(x => IngameEventHandler.EVENT_TYPE.BEGIN_ORC_POST_TURN == x)) return false;
            return true;
        }

        private void SkillActivate() {
            BattleConnector connector = PlayMangement.instance.socketHandler;
            MessageFormat format = MessageForm(false);
            connector.UnitSkillActivate(format);
        }


        private void SendSocket() {
            BattleConnector connector = PlayMangement.instance.socketHandler;
            MessageFormat format = MessageForm(true);
            connector.UseCard(format);
            if(myObject.GetComponent<MagicDragHandler>() != null) {
                int cardIndex = 0;
                if (myObject.transform.parent.parent.name == "CardSlot_1")
                    cardIndex = myObject.transform.parent.GetSiblingIndex();
                else {
                    Transform slot1 = myObject.transform.parent.parent.parent.GetChild(0);
                    for (int i = 0; i < 5; i++) {
                        if (slot1.GetChild(i).gameObject.activeSelf)
                            cardIndex++;
                    }
                    cardIndex += myObject.transform.parent.GetSiblingIndex();
                }
                PlayMangement.instance.player.cdpm.DestroyCard(cardIndex);
                if(PlayMangement.instance.player.isHuman)
                    PlayMangement.instance.player.ActivePlayer();
                else
                    PlayMangement.instance.player.ActiveOrcTurn();
            }
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
                if(selectList.Count > 0)
                    targets.Add(ArgumentForm(select, true, isEndCardPlay));
            }
            
            format.targets = targets.ToArray();
            return format;
        }

        private Arguments UnitArgument() {
            PlayMangement manage = PlayMangement.instance;
            
            string camp = isPlayer != manage.player.isHuman ? "orc" : "human";
            FieldUnitsObserver targetObserver = isPlayer ? manage.PlayerUnitsObserver : manage.EnemyUnitsObserver;
            int line = targetObserver.GetMyPos(myObject).col;
            var posObject = targetObserver.GetAllFieldUnits(line);
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
            switch(arguments.method) {
                case "all":
                    isOrc = (skill.targetCamp().CompareTo("my") == 0) != isPlayerHuman;
                    args.Add(isOrc ? "orc" : "human");
                break;
                case "line":
                    if(isSelect) args.Add(selectList[0].GetComponent<PlaceMonster>().x.ToString());
                    else args.Add(GetDropAreaLine().ToString());
                break;
                case "unit":
                    int unitItemId;
                    PlaceMonster monster;
                    if(isSelect) monster = selectList[0].GetComponent<PlaceMonster>();
                    else monster = GetDropAreaUnit();
                    unitItemId = monster.itemId;
                    args.Add(unitItemId.ToString());
                    isOrc = monster.isPlayer != isPlayerHuman;
                    args.Add(isOrc ? "orc" : "human");
                break;
                case "place":
                    int line = selectList[0].transform.GetSiblingIndex();
                    args.Add(line.ToString());
                    if(isEndCardPlay)
                        isOrc = skillTarget.GetComponent<PlaceMonster>().isPlayer != isPlayerHuman;
                    else
                        isOrc = myObject.GetComponent<PlaceMonster>().isPlayer != isPlayerHuman;
                    args.Add(isOrc ? "orc" : "human");
                    //TODO : 협력 몬스터랑 같이 있을 시 앞 뒤 위치 제대로 파악해야함
                    args.Add("front");
                break;
                case "camp":
                    isOrc = (skill.targetCamp().CompareTo("my") == 0) != isPlayerHuman;
                    args.Add(isOrc ? "orc" : "human");
                break;
            }
            arguments.args = args.ToArray();
            return arguments;
        }

        private PlaceMonster GetDropAreaUnit() {
            PlaceMonster unit;
            Transform highlight = myObject.GetComponent<CardHandler>().highlightedSlot;
            if(highlight != null)
                unit = highlight.GetComponentInParent<PlaceMonster>();
            else
                unit = skillTarget.GetComponent<PlaceMonster>();
            return unit;
        }
        
        private int GetDropAreaLine() {
            return myObject.GetComponent<CardHandler>()
                .highlightedSlot
                .GetComponentInParent<Terrain>()
                .transform.GetSiblingIndex();
        }
    }
}