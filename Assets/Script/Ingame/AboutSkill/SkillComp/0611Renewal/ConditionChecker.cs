using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

namespace SkillModules {
    public class ConditionChecker {
        protected static FieldUnitsObserver playerObserver;
        protected static FieldUnitsObserver enemyObserver;
        protected string[] args;
        protected SkillHandler mySkillHandler;

        public ConditionChecker(SkillHandler mySkillHandler) {
            this.mySkillHandler = mySkillHandler;
            args = new string[]{};
            GetObserver();
        }

        public ConditionChecker(string[] args, SkillHandler mySkillHandler) {
            this.mySkillHandler = mySkillHandler;
            this.args = args;
            GetObserver();
        }

        public ConditionChecker() {
            args = new string[]{};
            GetObserver();
        }

        private void GetObserver() {
            if(playerObserver != null) return;
            playerObserver = PlayMangement.instance.PlayerUnitsObserver;
            enemyObserver = PlayMangement.instance.EnemyUnitsObserver;
        }

        public virtual bool IsConditionSatisfied() {
            return true;
        }

        protected bool ArgsExist() {
            if(args.Length == 0) {
                Debug.LogError("args가 필요한 조건에 args가 존재하지 않습니다.");
                return false;
            }
            return true;
        }
    }

    public class skill_target_ctg_chk : ConditionChecker {
        public override bool IsConditionSatisfied() {
            if(ArgsExist()) return false;
            GameObject target = mySkillHandler.skillTarget;
            if(target == null) return false;
            IngameClass.Unit unit = target.GetComponent<PlaceMonster>().unit;
            bool exist = unit.cardCategories.ToList().Exists(x => x.CompareTo(args[0]) == 0);
            return exist;
        }
    }

    public class dmg_chk : ConditionChecker {
        bool isTargetPlayer;
        GameObject targetObject;
        public override bool IsConditionSatisfied() {
            if(!IsValidateData(mySkillHandler.targetData)) return false;
            
            Pos myPos = playerObserver.GetMyPos(mySkillHandler.myObject);
            Pos enemyPos = enemyObserver.GetMyPos(targetObject);

            bool isSameLine = myPos.row == enemyPos.row;
            
            return isSameLine;
        }

        private bool IsValidateData(object target) {
            if(target == null) return false;
            if(!target.GetType().IsArray) return false;
            if(SetTargetData(target)) return false;
            return true;
        }

        private bool SetTargetData(object target) {
            object[] targets = (object[])target;
            if(targets[0] is bool && targets[1] is GameObject) {
                isTargetPlayer = (bool)targets[0];
                targetObject = (GameObject)targets[1];
                return false;
            }
            else
                return true;
        }
    }

    public class terrain_chk : ConditionChecker {
        public override bool IsConditionSatisfied() {
            string conditionTerrain = args[0];
            FieldUnitsObserver observer = mySkillHandler.isPlayer ? playerObserver : enemyObserver;
            observer.GetMyPos(mySkillHandler.myObject);
            string myTerrain = mySkillHandler.myObject.GetComponentInParent<Terrain>().terrain.ToString();//.CompareTo(terrain) == 0
            bool isConditionTerrain = myTerrain.CompareTo(conditionTerrain) == 0;
            return isConditionTerrain;
        }
    }

    public class same_line : ConditionChecker {
        public override bool IsConditionSatisfied() {
            return false;
        }
    }

    public class played_camp_chk : ConditionChecker {
        public override bool IsConditionSatisfied() {
            return false;
        }
    }

    public class played_ctg_chk : ConditionChecker {
        public override bool IsConditionSatisfied() {
            return false;
        }
    }

    public class played_type_chk : ConditionChecker {
        public override bool IsConditionSatisfied() {
            return false;
        }
    }

    public class has_empty_space : ConditionChecker {
        public override bool IsConditionSatisfied() {
            return false;
        }
    }

    public class has_attr : ConditionChecker {
        public override bool IsConditionSatisfied() {
            return false;
        }
    }

    public class target_dmg_gte : ConditionChecker {
        public override bool IsConditionSatisfied() {
            return false;
        }
    }


}