using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SkillModules {
    public class ScopeChecker {
        protected SkillHandler mySkillHandler;

        public ScopeChecker(SkillHandler mySkillHandler) {
            this.mySkillHandler = mySkillHandler;
        }

        public virtual bool IsConditionSatisfied(GameObject summonedObject) {
            return true;
        }
    }

    public class playing : ScopeChecker {
        public playing(SkillHandler mySkillHandler) : base(mySkillHandler) { }

        public override bool IsConditionSatisfied(GameObject summonedObject) {
            return mySkillHandler.myObject == summonedObject;
        }
    }

    public class field : ScopeChecker {
        public field(SkillHandler mySkillHandler) : base(mySkillHandler) { }

        public override bool IsConditionSatisfied(GameObject summonedObject) {
            PlayedObject playedObject = new PlayedObject();
            if(playedObject.IsValidateData(mySkillHandler.targetData))
                if(playedObject.targetObject == mySkillHandler.myObject)
                    return false;
            return mySkillHandler.myObject.GetComponent<PlaceMonster>() != null;
        }
    }

    public class die : ScopeChecker {
        public die(SkillHandler mySkillHandler) : base(mySkillHandler) { }

        public override bool IsConditionSatisfied(GameObject summonedObject) {
            return mySkillHandler.myObject == summonedObject;
        }
    }
}

