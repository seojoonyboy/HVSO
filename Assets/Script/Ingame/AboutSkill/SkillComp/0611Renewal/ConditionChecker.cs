using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

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
            if(mySkillHandler.skillTarget == null) return false;

            return false;
        }
    }

    public class dmg_chk : ConditionChecker {

    }

    public class terrain_chk : ConditionChecker {

    }

    public class same_line : ConditionChecker {

    }

    public class played_camp_chk : ConditionChecker {

    }

    public class played_ctg_chk : ConditionChecker {

    }

    public class played_type_chk : ConditionChecker {

    }

    public class has_empty_space : ConditionChecker {

    }

    public class has_attr : ConditionChecker {

    }

    public class target_dmg_gte : ConditionChecker {

    }


}