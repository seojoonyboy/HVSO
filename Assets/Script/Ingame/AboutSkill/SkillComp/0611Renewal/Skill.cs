using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using dataModules;
using System;

namespace SkillModules {
    public class Skill {
        private IngameEventHandler.EVENT_TYPE myTrigger;
        private ConditionChecker[] conditionCheckers;
        private ScopeChecker scopeChecker;
        private TargetHandler targetHandler;
        private Ability ability;
        private SkillHandler mySkillHandler;
        

        //TODO : 한줄마다의 스킬들을 초기화
        public void Initialize(dataModules.Skill dataSkill, SkillHandler mySkillHandler) {
            this.mySkillHandler = mySkillHandler;

            Type enumType = typeof(IngameEventHandler.EVENT_TYPE);
            IngameEventHandler.EVENT_TYPE state = (IngameEventHandler.EVENT_TYPE)Enum.Parse(enumType, dataSkill.trigger.ToUpper());
            myTrigger = state;
            mySkillHandler.RegisterTriggerEvent(state);

            scopeChecker = MethodToClass<ScopeChecker>(dataSkill.scope, new ScopeChecker(mySkillHandler), mySkillHandler);
            InitCondition(dataSkill.conditions);

            //targetHandler = MethodToClass<TargetHandler>(dataSkill.target.method, new TargetHandler(dataSkill.target.args), mySkillHandler);
            string targetClass = string.Format("SkillModules.{0}", dataSkill.target.method);
            Component targetComponent = mySkillHandler.myObject.AddComponent(System.Type.GetType(targetClass));
            targetHandler = targetComponent.GetComponent<TargetHandler>();
            targetHandler.args = dataSkill.target.args;

            string abilityClass = string.Format("SkillModules.{0}", dataSkill.effect.method);
            Component component = mySkillHandler.myObject.AddComponent(System.Type.GetType(abilityClass));
            ability = component.GetComponent<Ability>();
            ability.skillHandler = mySkillHandler;
        }

        public void InitCondition(Condition[] conditions) {
            conditionCheckers = new ConditionChecker[conditions.Length];
            for(int i = 0; i < conditions.Length; i++) {
                conditionCheckers[i] = MethodToClass<ConditionChecker>(conditions[i].method, new ConditionChecker(), mySkillHandler);
            }
        }

        public T MethodToClass<T>(string method, T t, SkillHandler handler) {
            object result;
            string methodAdd = string.Format("SkillModules.{0}", method);
            if(string.IsNullOrEmpty(method)) {
                result = Activator.CreateInstance(t.GetType());
            }
            else {
                System.Type type = System.Type.GetType(methodAdd);
                result = Activator.CreateInstance(type, handler);
            }
            T resultChange = (T)result;
            return resultChange;
        }

        public bool Trigger(IngameEventHandler.EVENT_TYPE triggerType) {
            //trigger 검사
            if(myTrigger != triggerType) return false;
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
