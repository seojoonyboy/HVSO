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
            PlayMangement.instance.StartCoroutine (SkillTrigger (triggerType, Param));
        }

        private void SummonNonEndCardTriggerMonster() {
            if(triggerList.Exists(x => IngameEventHandler.EVENT_TYPE.END_CARD_PLAY == x)) return;
            if(!isPlayer) return;
            if(myObject.GetComponent<PlaceMonster>() == null) return;
            BattleConnector connector = PlayMangement.instance.socketHandler;
            MessageFormat format = MessageForm();
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
        }

        private bool isPlayingCard() {
            if (!isPlayer) return false;
            if (targetData == null) return false;
            if (!targetData.GetType().IsArray) return false;
            if (myObject != (GameObject)(((object[])targetData)[1])) return false;
            if (!triggerList.Exists(x => IngameEventHandler.EVENT_TYPE.END_CARD_PLAY == x)) return false;
            return true;
        }

        private void SendSocket() {
            BattleConnector connector = PlayMangement.instance.socketHandler;
            MessageFormat format = MessageForm();
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
            }
        }

        private MessageFormat MessageForm() {
            MessageFormat format = new MessageFormat();
            List<Arguments> targets = new List<Arguments>();

            //마법 사용
            if(myObject.GetComponent<MagicDragHandler>() != null) {
                format.itemId = myObject.GetComponent<MagicDragHandler>().itemID;
                targets.Add(ArgumentForm(skills[0], false));
            }
            //유닛 소환
            else {
                format.itemId = myObject.GetComponent<PlaceMonster>().itemId;
                targets.Add(UnitArgument());
            }
            //Select 스킬 있을 시
            Skill select = skills.ToList().Find(x => x.TargetSelectExist());
            //Playing scope 있는 Select일 때
            if(select != null && select.isPlayingSelect()) {
                List<GameObject> selectList = select.GetTargetFromSelect();
                if(selectList.Count > 0)
                    targets.Add(ArgumentForm(select, true));
            }
                
            
            format.targets = targets.ToArray();
            return format;
        }

        private Arguments UnitArgument() {
            PlayMangement manage = PlayMangement.instance;
            
            string camp = isPlayer != manage.player.isHuman ? "orc" : "human";
            FieldUnitsObserver targetObserver = isPlayer ? manage.PlayerUnitsObserver : manage.EnemyUnitsObserver;
            int line = targetObserver.GetMyPos(myObject).row;
            var posObject = targetObserver.GetAllFieldUnits(line);
            string placed = posObject.Count == 1 ? "front" : posObject[0] == myObject ? "back" : "front";
            return new Arguments("place", new string[]{line.ToString(), camp, placed});
        }

        private Arguments ArgumentForm(Skill skill, bool isSelect) {
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
                if(isSelect) {
                    //TODO : select시 유닛 선택
                }
                else 
                    args.Add(GetDropAreaLine().ToString());
                break;
                case "unit":
                if(isSelect) {
                    //TODO : select시 유닛 선택
                }
                else
                    args.Add(GetDropAreaUnit().itemId.ToString());
                break;
                case "place":
                    int line = selectList[0].transform.parent.GetSiblingIndex();
                    args.Add(line.ToString());
                    isOrc = skillTarget.GetComponent<PlaceMonster>().isPlayer != isPlayerHuman;
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
            return myObject.GetComponent<CardHandler>()
                .highlightedSlot
                .GetComponentInParent<PlaceMonster>();
        }
        
        private int GetDropAreaLine() {
            return myObject.GetComponent<CardHandler>()
                .highlightedSlot
                .GetComponentInParent<Terrain>()
                .transform.GetSiblingIndex();
        }
    }
}