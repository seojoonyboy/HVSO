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

        public ConditionChecker(SkillHandler mySkillHandler, string[] args = null) {
            this.mySkillHandler = mySkillHandler;
            this.args = args;
            if(this.args == null) args = new string[]{};
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
                Logger.LogError("args가 필요한 조건에 args가 존재하지 않습니다.");
                return false;
            }
            return true;
        }
    }
    
    public class skill_target_ctg_chk : ConditionChecker {
        public skill_target_ctg_chk(SkillHandler mySkillHandler, string[] args = null) : base(mySkillHandler, args) { }
        public override bool IsConditionSatisfied() {
            GameObject target = mySkillHandler.skillTarget;
            if(target == null) return false;
            IngameClass.Unit unit = target.GetComponent<PlaceMonster>().unit;
            bool exist = unit.cardCategories.ToList().Exists(x => x.CompareTo(args[0]) == 0);
            return exist;
        }
    }

    public class dmg_chk : ConditionChecker {
        PlayedObject playedObject;
        public dmg_chk(SkillHandler mySkillHandler, string[] args = null) : base(mySkillHandler, args) { 
            playedObject = new PlayedObject();
        }
        public override bool IsConditionSatisfied() {
            if(!playedObject.IsValidateData(mySkillHandler.targetData)) return false;
            
            Pos myPos = playerObserver.GetMyPos(mySkillHandler.myObject);
            Pos enemyPos = enemyObserver.GetMyPos(playedObject.targetObject);

            bool isSameLine = myPos.row == enemyPos.row;
            
            return isSameLine;
        }
    }

    public class terrain_chk : ConditionChecker {

        public terrain_chk(SkillHandler mySkillHandler, string[] args = null) : base(mySkillHandler, args) { }

        public override bool IsConditionSatisfied() {
            string conditionTerrain = args[0];
            FieldUnitsObserver observer = mySkillHandler.isPlayer ? playerObserver : enemyObserver;
            string myTerrain = mySkillHandler.myObject.GetComponentInParent<Terrain>().terrain.ToString();//.CompareTo(terrain) == 0
            bool isConditionTerrain = myTerrain.CompareTo(conditionTerrain) == 0;
            return isConditionTerrain;
        }
    }

    public class same_line : ConditionChecker {
        FieldUnitsObserver targetObserver;
        string subjectObserve;
        bool argSecondExist;
        bool value;

        public same_line(SkillHandler mySkillHandler, string[] args = null) : base(mySkillHandler, args) { }

        public override bool IsConditionSatisfied() {
            if(!ArgsExist()) return false;
            subjectObserve = args[0];
            if(args.Length > 1) argSecondExist = bool.TryParse(args[1], out value);
            Pos myPos = mySkillHandler.isPlayer ? playerObserver.GetMyPos(mySkillHandler.myObject) : enemyObserver.GetMyPos(mySkillHandler.myObject);             
            if(subjectObserve.CompareTo("enemy") == 0) {
                targetObserver = mySkillHandler.isPlayer ? enemyObserver : playerObserver;
                int unitCount = targetObserver.GetAllFieldUnits(myPos.row).Count;
                return checkSecondArg(unitCount);
            }
            return false;
        }

        private bool checkSecondArg(int unitCount) {
            if(argSecondExist && !value) {
                return unitCount == 0;    
            }
            return unitCount != 0;
        }
    }

    public class played_camp_chk : ConditionChecker {
        PlayedObject playedObject;
        public played_camp_chk(SkillHandler mySkillHandler, string[] args = null) : base(mySkillHandler, args) {
            playedObject = new PlayedObject();
        }
        public override bool IsConditionSatisfied() {
            if(!ArgsExist()) return false;
            playedObject.IsValidateData(mySkillHandler.targetData);
            if(args[0].CompareTo("my")==0) 
                return mySkillHandler.isPlayer == playedObject.isTargetPlayer;
            else if(args[0].CompareTo("enemy")==0) {
                return mySkillHandler.isPlayer != playedObject.isTargetPlayer;
            }
            //다른 args가 있는지
            return false;
        }
    }

    public class played_ctg_chk : ConditionChecker {
        PlayedObject playedObject;

        public played_ctg_chk(SkillHandler mySkillHandler, string[] args = null) : base(mySkillHandler, args) { 
            playedObject = new PlayedObject();
        }

        public override bool IsConditionSatisfied() {
            if(!ArgsExist()) return false;
            playedObject.IsValidateData(mySkillHandler.targetData);
            PlaceMonster playedMonster = playedObject.targetObject.GetComponent<PlaceMonster>();
            if(playedMonster == null) return false;
            bool isExist = playedMonster.unit.cardCategories.ToList().Exists(x => x.CompareTo(args[0]) == 0);
            return isExist;
        }
    }

    public class played_type_chk : ConditionChecker {
        PlayedObject playedObject;
        
        public played_type_chk(SkillHandler mySkillHandler, string[] args = null) : base(mySkillHandler, args) {
            playedObject = new PlayedObject();
        }

        public override bool IsConditionSatisfied() {
            if(!ArgsExist()) return false;
            playedObject.IsValidateData(mySkillHandler.targetData);
            PlaceMonster playedMonster = playedObject.targetObject.GetComponent<PlaceMonster>();
            MagicDragHandler playedMagic = playedObject.targetObject.GetComponent<MagicDragHandler>();
            bool isExist = args[0].CompareTo("magic") == 0 ? playedMagic != null : playedMonster != null;
            return isExist;
        }
    }

    public class has_empty_space : ConditionChecker {

        public has_empty_space(SkillHandler mySkillHandler, string[] args = null) : base(mySkillHandler, args) { }

        public override bool IsConditionSatisfied() {
            bool isEnemyField = args[0].CompareTo("my") != 0;
            FieldUnitsObserver fieldObserver = (mySkillHandler.isPlayer != isEnemyField )? playerObserver : enemyObserver;
            for(int i = 0; i < 5; i++) {    //나중에 줄 갯수 바뀔 때 대응을 준비해야함
                if(fieldObserver.GetAllFieldUnits(i).Count == 0) {
                    return true;
                }
            }
            return false;
        }
    }

    public class target_dmg_gte : ConditionChecker {
        
        public target_dmg_gte(SkillHandler mySkillHandler, string[] args = null) : base(mySkillHandler, args) { }

        /*public override bool IsConditionSatisfied() {
            if(!ArgsExist()) return false;
            playedObject.IsValidateData(mySkillHandler.targetData);
            PlaceMonster playedMonster = playedObject.targetObject.GetComponent<PlaceMonster>();
            return playedMonster.unit.attack >= int.Parse(args[0]);
        }*/
    }

    public class PlayedObject {
        public bool isTargetPlayer;
        public GameObject targetObject;

        public bool IsValidateData(object target) {
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


}