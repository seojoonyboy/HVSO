using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using dataModules;
using System;

namespace SkillModules {
    public class SkillHandler {
        private Skill[] skills;
        public GameObject myObject;
        public GameObject skillTarget;
        public bool isPlayer;
        public object targetData;
        private List<IngameEventHandler.EVENT_TYPE> triggerList;

        //TODO : 데이터 세팅
        public void Initialize(dataModules.Skill[] _skills, GameObject myObject, bool isPlayer) {
            triggerList = new List<IngameEventHandler.EVENT_TYPE>();
            this.myObject = myObject;
            this.isPlayer = isPlayer;
            //스킬 갯수만큼 한줄씩 넣기
            skills = new Skill[skills.Length];
            for(int i = 0; i < skills.Length; i++) {
                skills[i].Initialize(_skills[i], this);
            }
        }

        public void RegisterTriggerEvent(IngameEventHandler.EVENT_TYPE triggerType) {
            IngameEventHandler handler = PlayMangement.instance.EventHandler;
            
            bool triggerExist = triggerList.Exists(x => x == triggerType);
            if(triggerExist) return;
            
            triggerList.Add(triggerType);
            handler.AddListener(triggerType, Trigger);
        }

        public void RemoveTriggerEvent() {
            IngameEventHandler handler = PlayMangement.instance.EventHandler;
            triggerList.ForEach(x => handler.RemoveListener(x, Trigger));
        }

        //TODO : Trigger 관리
        private void Trigger(Enum Event_Type, Component Sender, object Param = null) {
            targetData = Param;
            IngameEventHandler.EVENT_TYPE triggerType = (IngameEventHandler.EVENT_TYPE)Event_Type;
            PlayMangement.instance.StartCoroutine(SkillTrigger(triggerType)); //이 부분 초큼...
        }

        IEnumerator SkillTrigger(IngameEventHandler.EVENT_TYPE triggerType) {
            bool isDone;
            foreach(Skill skill in skills) {
                isDone = false;
                bool active = skill.Trigger(triggerType);
                //TODO : 해당 스킬이 완료할 때까지 대기타기 //문제는 다 됐다는걸 어떻게 알려주느냐
                if(active) yield return new WaitUntil(() => isDone);
            }
        }
        
    }
}
