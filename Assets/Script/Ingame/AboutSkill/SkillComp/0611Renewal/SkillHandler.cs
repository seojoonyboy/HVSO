using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using dataModules;

namespace SkillModules {
    public class SkillHandler {
        private Skill[] skills;
        public GameObject myObject;
        public GameObject skillTarget;
        public bool isPlayer;
        public object targetData;

        //TODO : 데이터 세팅
        public void Initialize(dataModules.Skill[] _skills, GameObject myObject, bool isPlayer) {
            this.myObject = myObject;
            this.isPlayer = isPlayer;
            //스킬 갯수만큼 한줄씩 넣기
            skills = new Skill[skills.Length];
            for(int i = 0; i < skills.Length; i++) {
                skills[i].Initialize(_skills[i], this);
            }
        }

        //TODO : Trigger 관리
        private void Trigger() {
            //이벤트 처리
        }

        IEnumerator SkillTrigger() {
            bool isDone;
            foreach(Skill skill in skills) {
                isDone = false;
                bool active = skill.Trigger("field_change");
                //TODO : 해당 스킬이 완료할 때까지 대기타기 //문제는 다 됐다는걸 어떻게 알려주느냐
                if(active) yield return new WaitUntil(() => isDone);
            }
        }
        
    }
}
