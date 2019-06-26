using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using dataModules;
using System;
using TargetModules;

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
            InitCondition(dataSkill.conditions, mySkillHandler);

            if(mySkillHandler.myObject.GetComponent<PlaceMonster>() != null) {
                var name = mySkillHandler.myObject.GetComponent<PlaceMonster>().unit.name;
            }

            //targetHandler = MethodToClass<TargetHandler>(dataSkill.target.method, new TargetHandler(dataSkill.target.args), mySkillHandler);
            string targetClass = string.Format("TargetModules.{0}", dataSkill.target.method);
            Component targetComponent = mySkillHandler.myObject.AddComponent(System.Type.GetType(targetClass));
            if(targetComponent != null) {
                var lists = targetComponent.GetComponents<TargetHandler>();
                targetHandler = lists.Last();
                targetHandler.args = dataSkill.target.args;
                targetHandler.skillHandler = mySkillHandler;
            }

            string abilityClass = string.Format("SkillModules.{0}", dataSkill.effect.method);
            ability = MethodToClass<Ability>(dataSkill.effect.method, new Ability());
            ability.skillHandler = mySkillHandler;
            ability.args = dataSkill.effect.args;

            //Component component = mySkillHandler.myObject.AddComponent(System.Type.GetType(abilityClass));
            //ability = component.GetComponent<Ability>();
            //ability.skillHandler = mySkillHandler;
        }

        public void InitCondition(Condition[] conditions, SkillHandler mySkillHandler) {
            conditionCheckers = new ConditionChecker[conditions.Length];
            for(int i = 0; i < conditions.Length; i++) {
                conditionCheckers[i] = MethodToClass<ConditionChecker>(conditions[i].method, 
                    new ConditionChecker(mySkillHandler, conditions[i].args), 
                    mySkillHandler, conditions[i].args);
            }
            if(conditionCheckers.Length == 0) return;
            mySkillHandler.dragFiltering = conditionCheckers[0].filtering;
        }

        public T MethodToClass<T>(string method, T t, SkillHandler handler = null, string[] args = null) {
            object result;
            string methodAdd = string.Format("SkillModules.{0}", method);
            if(string.IsNullOrEmpty(method)) {
                result = Activator.CreateInstance(t.GetType());
            }
            else {
                System.Type type = System.Type.GetType(methodAdd);
                if(handler == null) {
                    return (T)Activator.CreateInstance(type);
                }
                if (args == null)
                    result = Activator.CreateInstance(type, handler);
                else
                    result = Activator.CreateInstance(type, handler, args);
            }
            T resultChange = (T)result;
            return resultChange;
        }

        public bool Trigger(IngameEventHandler.EVENT_TYPE triggerType, object parms) {
            Logger.Log(mySkillHandler.myObject);
            GameObject obj = null;
            PlayedObject parmsObject = new PlayedObject();
            if(parmsObject.IsValidateData(parms)) 
                obj = parmsObject.targetObject;

            //trigger 검사
            if(myTrigger != triggerType) return false;
            //scope 유효성 검사
            bool scopeCondition = scopeChecker.IsConditionSatisfied(obj);
            if (!scopeCondition) return false;

            //컨디션 args 내용도 보내기
            bool condition = true;
            foreach(ConditionChecker checker in conditionCheckers) 
                condition = condition && checker.IsConditionSatisfied();
            if(!condition) return false;

            if(targetHandler == null) {
                ability.Execute(SetExecuteData(null, ability.args));
            }
            else {
                targetHandler.SelectTarget(
                    delegate {
                        ability.Execute(SetExecuteData(targetHandler.GetTarget(), ability.args));
                    },
                    delegate {
                        Logger.Log("타겟이 없습니다.");
                        mySkillHandler.isDone = true;
                    },
                    delegate(ref List<GameObject> list) {
                        if(conditionCheckers.Length == 0) return;
                        conditionCheckers[0].filtering(ref list);
                    } 
                );
            }
            
            return true;
        }

        private object SetExecuteData(List<GameObject> targets, object[] targetArgs) {
            object result = null;

            if (ability.GetType() == typeof(gain)) {
                GainArgs args = new GainArgs();
                int.TryParse((string)targetArgs[0], out args.atk);
                int.TryParse((string)targetArgs[1], out args.hp);
                result = new object[] { targets, args };
            }
            else if(ability.GetType() == typeof(give_attribute)) {
                string attrName = (string)targetArgs[0];
                result = new object[] { targets, attrName };
            }
            else if(ability.GetType() == typeof(set_skill_target)) {
                if (targets.Count != 0) result = targets[0];
            }
            else if(ability.GetType() == typeof(supply)) {
                int num = 0;
                result = int.TryParse((string)ability.args[0], out num);
            }
            else if(ability.GetType() == typeof(hook)) {
                bool isPlayer = mySkillHandler.isPlayer;
                HookArgs args = new HookArgs();

                FieldUnitsObserver observer = null;
                if (isPlayer) observer = PlayMangement.instance.PlayerUnitsObserver;
                else observer = PlayMangement.instance.EnemyUnitsObserver;

                var pos = observer.GetMyPos(mySkillHandler.myObject);

                Logger.Log(pos.col);
                Logger.Log(pos.row);

                args.col = pos.col;
                args.row = 0;

                result = new object[] { targets[0], args, isPlayer };
            }
            else if(ability.GetType() == typeof(quick)) {
                result = targets[0];
            }
            else if(ability.GetType() == typeof(clear_skill_target)) {
                result = mySkillHandler.skillTarget;
            }
            else if(ability.GetType() == typeof(skill_target_move)) {
                GameObject slotToMove = targets[0];
                bool isPlayer = mySkillHandler.isPlayer;

                SkillTargetArgs args = new SkillTargetArgs();
                args.col = targets[0].transform.GetSiblingIndex();
                args.row = 0;

                result = new object[] { slotToMove, args, isPlayer };
            }
            else if(ability.GetType() == typeof(self_move)) {
                SelfMoveArgs args = new SelfMoveArgs();

                //타겟이 unit인 경우
                if (targets[0].GetComponent<PlaceMonster>() != null) {
                    args.col = targets[0].transform.parent.GetSiblingIndex();
                    args.row = 0;
                }
                //타겟이 slot인 경우
                else {
                    args.col = targets[0].transform.GetSiblingIndex();
                    args.row = 0;
                }
                bool isPlayer = mySkillHandler.isPlayer;

                result = new object[] { args, isPlayer };
            }
            else if(ability.GetType() == typeof(blast_enemy)) {
                bool isPlayer = mySkillHandler.isPlayer;
                int amount = 0;
                int.TryParse((string)ability.args[0], out amount);

                result = new object[] { isPlayer, targets, amount };
            }
            else if(ability.GetType() == typeof(random_blast_enemy)) {
                bool isPlayer = mySkillHandler.isPlayer;
                int amount = 0;
                int num = 0;
                int.TryParse((string)ability.args[0], out amount);
                int.TryParse((string)ability.args[1], out num);

                result = new object[] { isPlayer, targets, num, amount };
            }
            else if(ability.GetType() == typeof(r_return)) {
                bool isPlayer = mySkillHandler.isPlayer;
                result = new object[] { isPlayer };
            }
            else if(ability.GetType() == typeof(kill)) {
                bool isPlayer = mySkillHandler.isPlayer;
                result = new object[] { targets[0], isPlayer };
            }
            else if(ability.GetType() == typeof(give_attack_type)) {
                string attrName = (string)ability.args[0];
                result = new object[] { targets, attrName };
            }
            else if(ability.GetType() == typeof(summon_random)) {
                //result = new object[] { null };
            }
            else if(ability.GetType() == typeof(heal)) {
                int amount = 0;
                bool isPlayer = mySkillHandler.isPlayer;
                int.TryParse((string)ability.args[0], out amount);
                result = new object[] { isPlayer, amount };
            }
            else if(ability.GetType() == typeof(st_filter_terrain)) {
                bool isPlayer = mySkillHandler.isPlayer;
                result = new object[] { isPlayer, targets };
            }
            else if(ability.GetType() == typeof(st_filter_ctg)) {
                bool isPlayer = mySkillHandler.isPlayer;
                result = new object[] { isPlayer, targets };
            }
            else if(ability.GetType() == typeof(gain_resource)) {
                //혹시 무언가 필요하면...
            }
            return result;
        }

        public string firstTargetArgs() {
            if(targetHandler.args.Length < 2) return null;
            return targetHandler.args[1];
        }

        public string targetCamp() {
            return targetHandler.args[0];
        }

        public bool TargetSelectExist() {
            if(targetHandler == null) return false;
            return targetHandler.GetType().Name.Contains("select");
        }

        public bool isPlayingSelect() {
             if(scopeChecker == null) return false;
            return scopeChecker.GetType().Name.Contains("playing");
        }

        public List<GameObject> GetTargetFromSelect() {
            if(targetHandler == null) return null;
            return targetHandler.GetTarget(); 
        }
    }
}
