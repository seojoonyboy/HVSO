using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace SkillModules {
    public class TargetHandler : MonoBehaviour {
        public List<GameObject> targets;
        protected FieldUnitsObserver playerUnitsObserver, enemyUnitsObserver;

        public virtual void SetTarget(object parms) {
            targets = new List<GameObject>();
        }

        protected List<GameObject> GetTarget() {
            if(targets == null || targets.Count == 0) {
                Debug.LogError("Target이 제대로 지정되지 않았습니다.");
            }
            return targets;
        }
    }

    public class skill_target : TargetHandler {
        public override void SetTarget(object target) {
            base.SetTarget(target);

            targets.Add((GameObject)target);
        }
    }

    public class place : TargetHandler {
        void Awake() {
            if (GetComponent<PlaceMonster>().isPlayer) {
                playerUnitsObserver = PlayMangement.instance.PlayerUnitsObserver;
                enemyUnitsObserver = PlayMangement.instance.EnemyUnitsObserver;
            }
            else {
                playerUnitsObserver = PlayMangement.instance.EnemyUnitsObserver;
                enemyUnitsObserver = PlayMangement.instance.PlayerUnitsObserver;
            }
        }

        public override void SetTarget(object parms) {
            base.SetTarget(parms);

            string place = (string)parms;
            switch (place) {
                case "rear":
                    var pos = playerUnitsObserver.GetMyPos(gameObject);

                    targets.AddRange(
                        playerUnitsObserver
                        .GetAllFieldUnits(pos.row)
                    );
                    break;
            }
        }
    }
}
