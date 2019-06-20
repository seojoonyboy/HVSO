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
            PlayMangement.instance.StartCoroutine (SkillTrigger (triggerType, Param)); //이 부분 초큼...
        }

        IEnumerator SkillTrigger (IngameEventHandler.EVENT_TYPE triggerType, object parms) {
            foreach (Skill skill in skills) {
                isDone = false;
                bool active = skill.Trigger (triggerType, parms);
                //TODO : 해당 스킬이 완료할 때까지 대기타기 //문제는 다 됐다는걸 어떻게 알려주느냐

                if (active && !isDone) yield return new WaitUntil (() => isDone);
            }
            if(isMagicCard()) MagicSendSocket();
        }

        private bool isMagicCard() {
            if (targetData == null) return false;
            if (!targetData.GetType().IsArray) return false;
            if (myObject != (GameObject)(((object[])targetData)[1])) return false;
            if (myObject.GetComponent<MagicDragHandler>() == null) return false;
            return true;
        }

        private void MagicSendSocket () {
            BattleConnector connector = PlayMangement.instance.socketHandler;
            MessageFormat format = MessageForm();
            //string args = JsonUtility.ToJson(format);
            //connector.UseCard(args);
            MonoBehaviour.Destroy(myObject);
        }

        private MessageFormat MessageForm() {
            MessageFormat format = new MessageFormat();
            format.itemId = myObject.GetComponent<MagicDragHandler>().itemID;
            List<Arguments> target = new List<Arguments>();
            target.Add(ArgumentForm(skills[0]));
            Skill select = skills.ToList().Find(x => x.TargetSelectExist());
            if(select != null) target.Add(ArgumentForm(select));
            format.targets = target.ToArray();
            return format;
        }

        private Arguments ArgumentForm(Skill skill) {
            Arguments arguments = new Arguments();
            arguments.method = skill.firstTargetArgs();
            PlayerController player = isPlayer ? PlayMangement.instance.player : PlayMangement.instance.enemyPlayer;
            bool isPlayerHuman = player.isHuman;
            bool isOrc;
            List<string> args = new List<string>();
            switch(arguments.method) {
                case "all":
                isOrc = (skill.targetCamp().CompareTo("my") == 0) != isPlayerHuman;
                args.Add(isOrc ? "orc" : "human");
                break;
                case "line":
                args.Add(GetDropAreaLine().ToString());
                break;
                case "unit":
                args.Add(GetDropAreaUnit().itemId.ToString());
                break;
                case "place":
                //TODO : 나중에 선택 되는 놈은 GetDropAreaUnit()이나 GetDropAreaLine()으로 가져와지질 않는다는 문제점이 있다
                //args.Add(GetDropAreaLine().ToString());
                //isOrc = GetDropAreaUnit().isPlayer != isPlayerHuman;
                //TODO : front or rear 찾기
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