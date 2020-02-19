using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

namespace SkillModules {
    public class ConditionChecker {
        public string[] args;
        protected SkillHandler mySkillHandler;

        public ConditionChecker(SkillHandler mySkillHandler, string[] args = null) {
            this.mySkillHandler = mySkillHandler;
            this.args = args;
            if(this.args == null) args = new string[]{};
        }

        public virtual bool IsConditionSatisfied() {
            return true;
        }

        public virtual void filtering(ref List<GameObject> list) {
            return;
        }

        public virtual bool filtering(GameObject testObject) {
            return true;
        }

        protected bool ArgsExist() {
            if(args.Length == 0) {
                //Logger.LogError("args가 필요한 조건에 args가 존재하지 않습니다.");
                return false;
            }
            return true;
        }
    }
    
    public class skill_target_ctg_chk : ConditionChecker {
        public skill_target_ctg_chk(SkillHandler mySkillHandler, string[] args = null) : base(mySkillHandler, args) { }
        public override bool IsConditionSatisfied() {
            List<GameObject> targets = (List<GameObject>)mySkillHandler.skillTarget;
            if(targets == null) return false;
            if (targets[0] == null) return false;
            GameObject target = targets[0];
            IngameClass.Unit unit = target.GetComponent<PlaceMonster>().unit;
            bool exist = unit.cardCategories.ToList().Exists(x => !string.IsNullOrEmpty(x) && x.CompareTo(args[0]) == 0);
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

            var observer = PlayMangement.instance.UnitsObserver;
            FieldUnitsObserver.Pos myPos = observer.GetMyPos(mySkillHandler.myObject);
            FieldUnitsObserver.Pos enemyPos = observer.GetMyPos(playedObject.targetObject);

            bool isSameLine = myPos.row == enemyPos.row;
            
            return isSameLine;
        }
    }

    public class terrain_chk : ConditionChecker {

        public terrain_chk(SkillHandler mySkillHandler, string[] args = null) : base(mySkillHandler, args) { }

        public override bool IsConditionSatisfied() {
            string conditionTerrain = args[0];
            FieldUnitsObserver observer = PlayMangement.instance.UnitsObserver;
            string myTerrain = mySkillHandler.myObject.GetComponentInParent<Terrain>().terrain.ToString();//.CompareTo(terrain) == 0
            bool isConditionTerrain = myTerrain.CompareTo(conditionTerrain) == 0;
            return isConditionTerrain;
        }
    }

    public class same_line : ConditionChecker {
        string subjectObserve;
        bool argSecondExist;
        bool value;

        public same_line(SkillHandler mySkillHandler, string[] args = null) : base(mySkillHandler, args) { }

        public override bool IsConditionSatisfied() {
            var observer = PlayMangement.instance.UnitsObserver;
            PlayerController opponent = mySkillHandler.isPlayer ? PlayMangement.instance.enemyPlayer : PlayMangement.instance.player;
            if(!ArgsExist()) return false;
            subjectObserve = args[0];
            if(args.Length > 1) argSecondExist = bool.TryParse(args[1], out value);
            FieldUnitsObserver.Pos myPos = observer.GetMyPos(mySkillHandler.myObject);
            if(subjectObserve.CompareTo("enemy") == 0) {
                int unitCount = observer
                    .GetAllFieldUnits(
                        myPos.col, 
                        opponent.isHuman
                    ).Count;
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
            bool isExist = playedMonster.unit.cardCategories.ToList().Exists(x => !string.IsNullOrEmpty(x) && x.CompareTo(args[0]) == 0);
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
            var observer = PlayMangement.instance.UnitsObserver;
            bool isHuman = PlayMangement.instance.player.isHuman;

            bool isEnemyField = args[0].CompareTo("my") != 0;
            for(int i = 0; i < 5; i++) {    //나중에 줄 갯수 바뀔 때 대응을 준비해야함
                if(observer.GetAllFieldUnits(i, isHuman).Count == 0) {
                    return true;
                }
            }
            return false;
        }
    }

    public class field_exist : ConditionChecker {
        PlayedObject playedObject;

        public field_exist(SkillHandler skillHandler, string[] args = null) : base(skillHandler, args) {
            playedObject = new PlayedObject();
        }

        public override bool IsConditionSatisfied() {
            if (!ArgsExist()) return false;
            playedObject.IsValidateData(mySkillHandler.targetData);

            string camp = args[0];
            var playManagement = PlayMangement.instance;
            List<GameObject> targetPool = null;

            var observer = PlayMangement.instance.UnitsObserver;
            if (camp == "player") {
                bool isHuman = playManagement.player.isHuman;
                targetPool = observer.GetAllFieldUnits(isHuman);
            }
            else if (camp == "enemy") {
                PlayerController opponent = mySkillHandler.isPlayer ? PlayMangement.instance.enemyPlayer : PlayMangement.instance.player;

                bool isHuman = playManagement.enemyPlayer.isHuman;
                targetPool = observer.GetAllFieldUnits(opponent.isHuman);
            }

            if (targetPool == null) return false;

            switch (args[1]) {
                case "dmg_gte":
                    int dmg = 0;
                    int.TryParse(args[2], out dmg);
                    if (targetPool.Exists(x => x.GetComponent<PlaceMonster>().unit.attack >= dmg)) {
                        return true;
                    }
                    else return false;
                case "without_attr":
                    if(targetPool.Count == 0) return false; 
                    string attr = args[2];
                    targetPool.RemoveAll(x => x.GetComponent<PlaceMonster>().unit.attributes.ToList().Exists(y => y.CompareTo(attr) == 0));
                    if(targetPool.Count == 0) return false;
                    return true;
            }
            return false;
        }

        public override void filtering(ref List<GameObject> list) {
            switch (args[1]) {
                case "dmg_gte":
                    int power = int.Parse(args[2]);
                    list.RemoveAll(x => x.GetComponent<PlaceMonster>().unit.attack < power);
                    break;
                case "without_attr":
                    list.RemoveAll(x => x.GetComponent<PlaceMonster>().unit.attributes.ToList().Exists(y => y.CompareTo(args[2]) == 0));
                    break;
            }
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

        public override void filtering(ref List<GameObject> list) {
            int power = int.Parse(args[0]);
            list.RemoveAll(x => x.GetComponent<PlaceMonster>().unit.attack < power);
            return;
        }

        public override bool filtering(GameObject testObject) {
            return testObject.GetComponent<PlaceMonster>().unit.attack >= int.Parse(args[0]);
        }
    }

    public class my_field_ctg_chk : ConditionChecker {
        PlayedObject playedObject;

        public my_field_ctg_chk(SkillHandler skillHandler, string[] args = null) : base(skillHandler, args) {
            playedObject = new PlayedObject();
        }

        public override bool IsConditionSatisfied() {
            if (!ArgsExist()) return false;
            playedObject.IsValidateData(mySkillHandler.targetData);

            var observer = PlayMangement.instance.UnitsObserver;
            bool isHuman = PlayMangement.instance.player.isHuman;

            var units = observer.GetAllFieldUnits(isHuman);

            //자신은 제외
            var me = units.Find(x => x == mySkillHandler.myObject);
            units.Remove(me);

            if (units.Count == 0) return false;

            foreach(GameObject unit in units) {
                PlaceMonster placeMonster = unit.GetComponent<PlaceMonster>();
                if (placeMonster.unit.cardCategories.ToList().Contains(args[0])) return true;
            }
            return false;
        }

        public override void filtering(ref List<GameObject> list) {
            string category = args[0];
            list.RemoveAll(x => x.GetComponent<PlaceMonster>().unit.cardCategories.ToList().Exists(ctg => ctg.CompareTo(category)!=0));
            return;
        }
    }

    public class select_ctg_chk : ConditionChecker {
        PlayedObject playedObject;

        public select_ctg_chk(SkillHandler skillHandler, string[] args = null) : base(skillHandler) {
            playedObject = new PlayedObject();
        }

        public override bool IsConditionSatisfied() {
            if (!ArgsExist()) return false;
            playedObject.IsValidateData(mySkillHandler.targetData);

            PlaceMonster playedMonster = playedObject.targetObject.GetComponent<PlaceMonster>();
            return playedMonster.unit.cardCategories.ToList().Contains(args[0]);
        }

        public override void filtering(ref List<GameObject> list) {
            string category = args[0];
            list.RemoveAll(x => (!x.GetComponent<PlaceMonster>().unit.cardCategories.ToList().Exists(y => y.CompareTo(category)==0)));
            return;
        }
    }

    public class has_attribute : ConditionChecker {
        PlayedObject playedObject;

        public has_attribute(SkillHandler skillHandler, string[] args = null) : base(skillHandler) {
            playedObject = new PlayedObject();
        }

        public override bool IsConditionSatisfied() {
            if (!ArgsExist()) return false;
            playedObject.IsValidateData(mySkillHandler.targetData);

            PlaceMonster playedMonster = playedObject.targetObject.GetComponent<PlaceMonster>();
            return playedMonster.unit.attributes.ToList().Contains(args[0]);
        }

        public override void filtering(ref List<GameObject> list) {
            string category = (string)args[0];
            list.RemoveAll(x => (!x.GetComponent<PlaceMonster>().unit.attributes.ToList().Exists(y => y.CompareTo(category) == 0)));
            return;
        }
    }

    public class ambushing : ConditionChecker {
        PlayedObject playedObject;

        public ambushing(SkillHandler skillHandler, string[] args = null) : base(skillHandler) {
            playedObject = new PlayedObject();
        }

        public override bool IsConditionSatisfied() {
            playedObject.IsValidateData(mySkillHandler.targetData);

            PlaceMonster playedMonster = mySkillHandler.highlight.gameObject.GetComponentInParent<PlaceMonster>();
            return playedMonster.gameObject.GetComponent<ambush>() != null;
        }

        public override bool filtering(GameObject testObject) {
            return testObject.GetComponent<PlaceMonster>().unit.attributes.ToList().Contains("ambush");
        }
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