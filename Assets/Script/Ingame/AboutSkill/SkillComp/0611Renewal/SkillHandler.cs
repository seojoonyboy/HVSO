using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using dataModules;

namespace SkillModules {
    public class SkillHandler {
        Skill[] skills;

        //TODO : 데이터 세팅
        public void Initialize(CardInventory inventory) {
            //스킬 갯수만큼 한줄씩 넣기
            skills = new Skill[inventory.skills.Length];
            for(int i = 0; i < skills.Length; i++) {
                skills[i].Initialize(inventory.skills[i]);
            }
        }

        //TODO : Trigger 관리
        private void Trigger() {
            
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
