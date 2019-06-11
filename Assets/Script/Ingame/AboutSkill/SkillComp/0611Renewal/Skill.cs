using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using dataModules;

namespace SkillModules {
    public class Skill {
        ConditionChecker conditionChecker;
        //Target 뭐시기
        Ability ability;
        

        //TODO : 한줄마다의 스킬들을 초기화
        public void Initialize(dataModules.Skill dataSkill) {
            //method들을 클래스로 가져오기
        }

        public bool Trigger(string trigger) {
            //컨디션 args 내용도 보내기
            bool condition = conditionChecker.IsConditionSatisfied();
            if(!condition) return false;
            //Target 가져오기
            //ability 발동하기
            return true;
        }
    }
}
