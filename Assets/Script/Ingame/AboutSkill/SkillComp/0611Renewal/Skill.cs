using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using dataModules;

namespace SkillModules {
    public class Skill {
        private ConditionChecker[] conditionCheckers;
        private TargetHandler targetHandler;
        private Ability ability;
        private SkillHandler mySkillHandler;
        

        //TODO : 한줄마다의 스킬들을 초기화
        public void Initialize(dataModules.Skill dataSkill, SkillHandler mySkillHandler) {
            this.mySkillHandler = mySkillHandler;
            //TODO : method가 아닌 dataSkill의 각각의 method로 가져와야함
            string method = "something";
            InitCondition(dataSkill.conditions);
            targetHandler = MethodToClass<TargetHandler>(method, new TargetHandler());
            string abilityClass = string.Format("SkillModules.{0}", method);
            Component component = mySkillHandler.myObject.AddComponent(System.Type.GetType(abilityClass));
            ability = component.GetComponent<Ability>();
            ability.skillHandler = mySkillHandler;
        }

        public void InitCondition(dataModules.Condition[] conditions) {
            conditionCheckers = new ConditionChecker[conditions.Length];
            for(int i = 0; i < conditions.Length; i++) {
                conditionCheckers[i] = MethodToClass<ConditionChecker>(conditions[i].method, new ConditionChecker());
            }
        }

        public T MethodToClass<T>(string method, T t) {
            if(string.IsNullOrEmpty(method)) return (T)System.Activator.CreateInstance(t.GetType());
            else return (T)System.Activator.CreateInstance(System.Type.GetType(method));
        }

        public bool Trigger(string trigger) {
            //컨디션 args 내용도 보내기
            bool condition = true;
            foreach(ConditionChecker checker in conditionCheckers) 
                condition = condition && checker.IsConditionSatisfied();
            if(!condition) return false;
            //Target 가져오기
            //ability 발동하기
            return true;
        }
    }
}
