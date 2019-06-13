using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using dataModules;

namespace SkillModules {
    public class Skill {
        private ConditionChecker[] conditionCheckers;
        private ScopeChecker scopeChecker;
        private TargetHandler targetHandler;
        private Ability ability;
        private SkillHandler mySkillHandler;
        

        //TODO : 한줄마다의 스킬들을 초기화
        public void Initialize(dataModules.Skill dataSkill, SkillHandler mySkillHandler) {
            this.mySkillHandler = mySkillHandler;

            scopeChecker = MethodToClass<ScopeChecker>(dataSkill.scope, new ScopeChecker(mySkillHandler));
            InitCondition(dataSkill.conditions);

            targetHandler = MethodToClass<TargetHandler>(dataSkill.target.method, new TargetHandler(dataSkill.target.args));

            string abilityClass = string.Format("SkillModules.{0}", dataSkill.effect.method);
            Component component = mySkillHandler.myObject.AddComponent(System.Type.GetType(abilityClass));
            ability = component.GetComponent<Ability>();
            ability.skillHandler = mySkillHandler;
        }

        public void InitCondition(Condition[] conditions) {
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
            //scope 유효성 검사
            bool scopeCondition = scopeChecker.IsConditionSatisfied(mySkillHandler.myObject);
            if (!scopeCondition) return false;

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
