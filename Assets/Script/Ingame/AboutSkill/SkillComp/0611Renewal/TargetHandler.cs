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

        public bool isDone = false;

        public SkillHandler skillHandler;
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

        public List<GameObject> GetTarget() {
            if(targets == null || targets.Count == 0) {
                Logger.LogError("Target이 제대로 지정되지 않았습니다.");
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

        protected Transform GetClickedAreaSlot() {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            LayerMask mask = (1 << LayerMask.NameToLayer("UnitSlot"));

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
        public override void SelectTarget(SelectTargetFinished successCallback, SelectTargetFailed failedCallback) {
            base.SelectTarget(successCallback, failedCallback);

            var target = skillHandler.skillTarget;
            if (target == null) {
                failedCallback("타겟을 찾을 수 없습니다.");
                return;
            }
            SetTarget(target);
            successCallback(target);
        }

        public override void SetTarget(object target) {
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

        public override void SelectTarget(SelectTargetFinished successCallback, SelectTargetFailed failedCallback) {
            base.SelectTarget(successCallback, failedCallback);

            //직접 지정하는게 없음
            SetTarget(null);
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
        public override void SelectTarget(SelectTargetFinished successCallback, SelectTargetFailed failedCallback) {
            base.SelectTarget(successCallback, failedCallback);

            if(GetComponent<PlaceMonster>().myTarget == null) {
                failedCallback("대상을 찾을 수 없습니다.");
                return;
            }
            SetTarget(GetComponent<PlaceMonster>().myTarget);
            successCallback(GetComponent<PlaceMonster>().myTarget);
        }

        /// <summary></summary>
        /// <param name="target">내가 공격하려는 대상</param>
        public override void SetTarget(object target) {
            targets.Add((GameObject)target);
        }
    }

    public class self : TargetHandler {
        public override void SelectTarget(SelectTargetFinished successCallback, SelectTargetFailed failedCallback) {
            base.SelectTarget(successCallback, failedCallback);

            SetTarget(gameObject);
            successCallback(gameObject);
        }

        public override void SetTarget(object parms) {
            targets.Add(gameObject);
        }
    }
    
    public class played_target : TargetHandler {
        public override void SelectTarget(SelectTargetFinished successCallback, SelectTargetFailed failedCallback) {
            base.SelectTarget(successCallback, failedCallback);

            
            if (args == null) failedCallback("Args 가 존재가지 않습니다.");

            var targets = GetTarget(GetComponent<SkillHandler>().isPlayer, args);
            SetTarget(targets);
            successCallback(targets);
        }

        /// <summary></summary>
        /// <param name="target">내가 카드를 드롭하면서 지목한 대상</param>
        public override void SetTarget(object target) {
            targets.Add((GameObject)target);
        }

        private List<GameObject> GetTarget(bool isPlayer, string[] args) {
            FieldUnitsObserver playerObserver, enemyObserver;
            var playManagement = PlayMangement.instance;
            if (isPlayer) {
                playerObserver = playManagement.PlayerUnitsObserver;
                enemyObserver = playManagement.EnemyUnitsObserver;
            }
            else {
                playerObserver = playManagement.EnemyUnitsObserver;
                enemyObserver = playManagement.PlayerUnitsObserver;
            }

            List<GameObject> result = new List<GameObject>();
            if (args.Length != 2) {
                Logger.LogError("Args가 잘못 전달되었습니다.");
                return result;
            }

            switch (args[0]) {
                case "my":
                    if (args[1] == "unit") {
                        result.Add(GetDropAreaUnit().gameObject);
                    }
                    else if (args[1] == "line") {
                        int col = transform.parent.GetSiblingIndex();
                        var targets = playerObserver.GetAllFieldUnits(col);
                        foreach (GameObject target in targets) {
                            result.Add(target);
                        }
                    }
                    else if(args[1] == "all") {
                        var targets = playerObserver.GetAllFieldUnits();
                        foreach (GameObject target in targets) {
                            result.Add(target);
                        }
                    }
                    break;
                case "enemy":
                    if (args[1] == "unit") {
                        result.Add(GetDropAreaUnit().gameObject);
                    }
                    else if (args[1] == "line") {
                        int col = transform.parent.GetSiblingIndex();
                        var targets = enemyObserver.GetAllFieldUnits(col);
                        foreach(GameObject target in targets) {
                            result.Add(target);
                        }
                    }
                    else if (args[1] == "all") {
                        var targets = enemyObserver.GetAllFieldUnits();
                        foreach (GameObject target in targets) {
                            result.Add(target);
                        }
                    }
                    break;
            }
            return result;
        }
    }

    public class select : TargetHandler {
        SelectTargetFinished callback;
        string arg;

        private void Update() {
            if (Input.GetMouseButtonDown(0)) {
                var selectedTarget = GetClickedAreaSlot();

                if (selectedTarget != null) {
                    PlayMangement.instance.OffBlockPanel();

                    CardDropManager.Instance.HideDropableSlot();
                    SetTarget(selectedTarget.gameObject);
                    callback(selectedTarget);
                }
            }
        }

        public override void SelectTarget(SelectTargetFinished successCallback, SelectTargetFailed failedCallback) {
            base.SelectTarget(successCallback, failedCallback);
            arg = args[1];

            switch (args[0]) {
                case "my":
                    if(args.Length == 2 && args[1] == "place") {
                        PlayMangement.instance.OnBlockPanel("대상을 지정해 주세요.");
                        callback = successCallback;
                        CardDropManager.Instance.ShowDropableSlot(GetComponent<CardHandler>().cardData);
                    }
                    break;
                //case "enemy"
            }
        }

        /// <summary></summary>
        /// <param name="parms">사용자가 직접 지목한 위치?</param>
        public override void SetTarget(object target) {
            targets.Add((GameObject)target);
        }
    }

    public class played : TargetHandler {
        public override void SelectTarget(SelectTargetFinished successCallback, SelectTargetFailed failedCallback) {
            base.SelectTarget(successCallback, failedCallback);

            SetTarget(null);
            successCallback(null);
        }

        /// <summary></summary>
        /// <param name="parms">생성된 유닛</param>
        public override void SetTarget(object target) {
            targets.Add((GameObject)target);
        }
    }
}
