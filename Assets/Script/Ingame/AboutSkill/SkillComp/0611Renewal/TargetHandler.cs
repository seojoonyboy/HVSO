using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

namespace SkillModules {
    public class TargetHandler : MonoBehaviour {
        public delegate void SelectTargetFinished(object parms);
        public delegate void SelectTargetFailed(string msg);

        public string[] args;

        public List<GameObject> targets;
        protected FieldUnitsObserver playerUnitsObserver, enemyUnitsObserver;

        public TargetHandler(string[] args) {
            this.args = args;
        }

        /// <summary>
        /// 타겟을 지정하는 단계를 시작
        /// </summary>
        public virtual void SelectTarget(SelectTargetFinished successCallback, SelectTargetFailed failedCallback) {
            targets = new List<GameObject>();
            Logger.Log("타겟을 지정합니다.");
        }

        /// <summary>
        /// 최종적으로 지정된 타겟을 저장
        /// </summary>
        /// <param name="parms">지정된 타겟에 대한 정보</param>
        public virtual void SetTarget(object parms) { }

        protected List<GameObject> GetTarget() {
            if(targets == null || targets.Count == 0) {
                Debug.LogError("Target이 제대로 지정되지 않았습니다.");
            }
            return targets;
        }

        /// <summary>
        /// 카드를 Drop한 곳의 유닛 Transform을 반환
        /// </summary>
        /// <returns></returns>
        protected Transform GetDropAreaUnit() {
            return GetComponent<CardHandler>()
                .highlightedSlot
                .GetComponentInParent<PlaceMonster>()
                .transform;
        }

        protected Transform GetClickedAreaUnit() {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            LayerMask mask1 = (1 << LayerMask.NameToLayer("PlayerUnit"));
            LayerMask mask2 = (1 << LayerMask.NameToLayer("EnemyUnit"));
            LayerMask mask = mask1 | mask2;

            RaycastHit2D[] hits = Physics2D.RaycastAll(
                new Vector2(mousePos.x, mousePos.y),
                Vector2.zero,
                Mathf.Infinity,
                mask
            );

            if (hits != null) {
                foreach (RaycastHit2D hit in hits) {
                    return hit.transform;
                }
            }
            return null;
        }
    }

    public class skill_target : TargetHandler {
        public skill_target(string[] args) : base(args) { }

        public override void SelectTarget(SelectTargetFinished successCallback, SelectTargetFailed failedCallback) {
            base.SelectTarget(successCallback, failedCallback);

            var target = GetDropAreaUnit();
            if (target == null) {
                failedCallback("타겟을 찾을 수 없습니다.");
                return;
            }
            successCallback(target);
        }

        public override void SetTarget(object target) {
            targets.Add((GameObject)target);
        }
    }

    public class place : TargetHandler {
        public place(string[] args) : base(args) { }

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

        public override void SelectTarget(SelectTargetFinished successCallback, SelectTargetFailed failedCallback) {
            base.SelectTarget(successCallback, failedCallback);

            //직접 지정하는게 없음
            successCallback(null);
        }

        public override void SetTarget(object parms) {
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

    public class attack_target : TargetHandler {
        public attack_target(string[] args) : base(args) { }

        public override void SelectTarget(SelectTargetFinished successCallback, SelectTargetFailed failedCallback) {
            base.SelectTarget(successCallback, failedCallback);

            if(GetComponent<PlaceMonster>().myTarget == null) {
                failedCallback("대상을 찾을 수 없습니다.");
                return;
            }
            successCallback(GetComponent<PlaceMonster>().myTarget);
        }

        /// <summary></summary>
        /// <param name="target">내가 공격하려는 대상</param>
        public override void SetTarget(object target) {
            targets.Add((GameObject)target);
        }
    }

    public class self : TargetHandler {
        public self(string[] args) : base(args) { }

        public override void SelectTarget(SelectTargetFinished successCallback, SelectTargetFailed failedCallback) {
            base.SelectTarget(successCallback, failedCallback);

            successCallback(gameObject);
        }

        public override void SetTarget(object parms) {
            targets.Add(gameObject);
        }
    }
    
    public class played_target : TargetHandler {
        public played_target(string[] args) : base(args) { }

        public override void SelectTarget(SelectTargetFinished successCallback, SelectTargetFailed failedCallback) {
            base.SelectTarget(successCallback, failedCallback);

            var target = GetDropAreaUnit();
            if (target == null) {
                failedCallback("타겟을 찾을 수 없습니다.");
                return;
            }
            successCallback(target);
        }

        /// <summary></summary>
        /// <param name="target">내가 카드를 드롭하면서 지목한 대상</param>
        public override void SetTarget(object target) {
            targets.Add((GameObject)target);
        }
    }

    public class select : TargetHandler {
        SelectTargetFinished callback;
        public select(string[] args) : base(args) { }

        private void Update() {
            if (Input.GetMouseButtonDown(0)) {
                var selectedTarget = GetClickedAreaUnit();

                if (selectedTarget != null) callback(selectedTarget);
            }
        }

        public override void SelectTarget(SelectTargetFinished successCallback, SelectTargetFailed failedCallback) {
            base.SelectTarget(successCallback, failedCallback);

            PlayMangement.instance.OnBlockPanel("대상을 지정해 주세요.");
            callback = successCallback;
        }

        /// <summary></summary>
        /// <param name="parms">사용자가 직접 지목한 위치?</param>
        public override void SetTarget(object target) {
            targets.Add((GameObject)target);
        }
    }

    public class played : TargetHandler {
        public played(string[] args) : base(args) { }

        public override void SelectTarget(SelectTargetFinished successCallback, SelectTargetFailed failedCallback) {
            base.SelectTarget(successCallback, failedCallback);

            successCallback(null);
        }

        /// <summary></summary>
        /// <param name="parms">생성된 유닛</param>
        public override void SetTarget(object target) {
            targets.Add((GameObject)target);
        }
    }
}
