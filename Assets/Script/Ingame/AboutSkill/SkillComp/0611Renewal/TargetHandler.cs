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
                Logger.Log("타겟이 없습니다.");
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

        protected int GetDropLine() {
            Transform highlightedSlot = GetComponent<CardHandler>().highlightedSlot;
            int col = highlightedSlot.parent.GetSiblingIndex();

            Logger.Log(col);

            return col;
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

        protected Transform GetClickedAreaUnit(string layer) {
            layer = "ClickableUnit";

            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            LayerMask mask = (1 << LayerMask.NameToLayer(layer));

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
            string place = args[0];
            switch (place) {
                case "rear":
                    var pos = playerUnitsObserver.GetMyPos(gameObject);
                    var list = playerUnitsObserver.GetAllFieldUnits(pos.col);
                    list.Remove(gameObject);
                    targets.AddRange(list);
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

            var targets = GetTarget(skillHandler.isPlayer, args);
            SetTarget(targets);
            successCallback(targets);
        }

        /// <summary></summary>
        /// <param name="target">내가 카드를 드롭하면서 지목한 대상</param>
        public override void SetTarget(object target) {
            targets.AddRange((List<GameObject>)target);
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
                        int col = GetDropLine();
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
                        int col = GetDropLine();
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

        private void Start() {
            if(!skillHandler.isPlayer) this.enabled = false;
        }

        private void Update() {
            if (callback != null && Input.GetMouseButtonDown(0)) {
                Transform selectedTarget = null;
                if (args[1] == "place") {
                    selectedTarget = GetClickedAreaSlot();
                }
                else if(args[1] == "unit") {
                    string layer = "PlayerUnit";
                    if(args[0] == "enemy") {
                        layer = "EnemyUnit";
                    }
                    selectedTarget = GetClickedAreaUnit(layer);
                }

                if (selectedTarget != null) {
                    PlayMangement.instance.OffBlockPanel();

                    CardDropManager.Instance.HideDropableSlot();
                    if(args[1] == "place") {
                        int col = selectedTarget.parent.GetSiblingIndex();
                        int row = 0;
                        selectedTarget = PlayMangement.instance
                            .PlayerUnitsObserver
                            .transform
                            .GetChild(row)
                            .GetChild(col)
                            .transform;
                    }

                    if(args[1] == "unit") {
                        selectedTarget = selectedTarget.gameObject.GetComponentInParent<PlaceMonster>().transform;
                        
                    }

                    SetTarget(selectedTarget.gameObject);
                    callback(selectedTarget);

                    callback = null;
                    PlayMangement.instance.infoOn = false;

                    var units = PlayMangement.instance.EnemyUnitsObserver.GetAllFieldUnits();
                    foreach (GameObject unit in units) {
                        unit
                            .transform
                            .Find("ClickableUI")
                            .gameObject
                            .SetActive(false);
                    }
                    units = PlayMangement.instance.PlayerUnitsObserver.GetAllFieldUnits();
                    foreach (GameObject unit in units) {
                        unit
                            .transform
                            .Find("ClickableUI")
                            .gameObject
                            .SetActive(false);
                    }
                }
            }
        }

        public override void SelectTarget(SelectTargetFinished successCallback, SelectTargetFailed failedCallback) {
            base.SelectTarget(successCallback, failedCallback);
            //TODO : 적일 경우 해당 소켓이 도달 할 때까지 기다리기 card_played, skill_activated
            if(!skillHandler.isPlayer) { 
                skillHandler.isDone = true;
                return; 
            }
            
            foreach(string arg in args) {
                Logger.Log(arg);
            }

            switch (args[0]) {
                case "my":
                    if(args.Length == 2 && args[1] == "place") {
                        if (CanSelect(args[1])) {
                            PlayMangement.instance.OnBlockPanel("대상을 지정해 주세요.");
                            callback = successCallback;
                            PlaceMonster myMonster = skillHandler.myObject.GetComponent<PlaceMonster>();
                            string[] attributes; 
                            if(myMonster != null)
                                attributes = myMonster.unit.attributes;
                            else
                                attributes = GetDropAreaUnit().GetComponent<PlaceMonster>().unit.attributes;
                            CardDropManager.Instance.ShowDropableSlot(attributes, true);
                        }
                        else {
                            failedCallback("자리가 없습니다.");
                        }
                    }
                    if (args.Length == 2 && args[1] == "unit") {
                        if (CanSelect(args[1])) {
                            PlayMangement.instance.OnBlockPanel("대상을 지정해 주세요.");
                            callback = successCallback;

                            //잠복중인 유닛은 타겟에서 제외
                            var units = PlayMangement.instance.PlayerUnitsObserver.GetAllFieldUnits();
                            foreach(GameObject unit in units) {
                                var placeMonster = unit.GetComponent<PlaceMonster>();
                                if (placeMonster.GetComponent<ambush>() != null) {
                                    units.Remove(unit);
                                }
                            }

                            foreach (GameObject unit in units) {
                                var ui = unit.transform.Find("ClickableUI").gameObject;
                                if (ui != null) {
                                    ui.SetActive(true);
                                    PlayMangement.instance.infoOn = true;
                                }
                            }
                        }
                        else {
                            failedCallback("타겟이 없습니다.");
                        }
                    }
                    break;
                case "enemy":
                    if (args.Length == 2 && args[1] == "unit") {
                        if (CanSelect(args[1])) {
                            PlayMangement.instance.OnBlockPanel("대상을 지정해 주세요.");
                            var units = PlayMangement.instance.EnemyUnitsObserver.GetAllFieldUnits();

                            //잠복중인 유닛은 타겟에서 제외
                            foreach (GameObject unit in units) {
                                var placeMonster = unit.GetComponent<PlaceMonster>();
                                if (placeMonster.GetComponent<ambush>() != null) {
                                    units.Remove(unit);
                                }
                            }

                            foreach (GameObject unit in units) {
                                var ui = unit.transform.Find("ClickableUI").gameObject;
                                if (ui != null) {
                                    ui.SetActive(true);
                                    PlayMangement.instance.infoOn = true;
                                }
                            }

                            callback = successCallback;
                        }
                        else {
                            failedCallback("타겟이 없습니다.");
                        }
                    }
                    break;
            }
        }

        /// <summary></summary>
        /// <param name="parms">사용자가 직접 지목한 위치?</param>
        public override void SetTarget(object target) {
            targets = new List<GameObject>();
            targets.Add((GameObject)target);
        }

        private bool CanSelect(string arg) {
            bool result = false;
            var observer = PlayMangement.instance.PlayerUnitsObserver;

            switch (arg) {
                case "place":
                    for(int i=0; i<5; i++) {
                        //빈 공간인 경우
                        if(observer.units[i, 0] == null) {
                            var placeMonster = skillHandler.myObject.GetComponent<PlaceMonster>();
                            if (placeMonster != null) {
                                //숲 지형인 경우
                                if (observer.transform.GetChild(0).GetChild(i).GetComponent<Terrain>().terrain == PlayMangement.LineState.forest) {
                                    //유닛이 숲 지형에 갈 수 있는 경우
                                    if (placeMonster.unit.attributes.ToList().Contains("footslog")) {
                                        result = true;
                                    }
                                }
                                //숲 지형이 아닌 경우
                                else {
                                    result = true;
                                }
                            }
                        }
                    }
                    break;
                case "unit":
                    if(args[0] == "enemy") {
                        observer = PlayMangement.instance.EnemyUnitsObserver;
                    }
                    if ((observer.GetAllFieldUnits()).Count != 0) {
                        Logger.Log(observer.GetAllFieldUnits().Count + "개의 적이 감지됨");
                        result = true;
                    }
                    break;
            }
            return result;
        }
    }

    public class played : TargetHandler {
        public override void SelectTarget(SelectTargetFinished successCallback, SelectTargetFailed failedCallback) {
            base.SelectTarget(successCallback, failedCallback);

            if(args[0] == "my") {
                object[] tmp = (object[])skillHandler.targetData;
                bool isPlayer = (bool)tmp[0];
                GameObject summonedObject = (GameObject)tmp[1];

                if(summonedObject.GetComponent<PlaceMonster>() != null) {
                    if (skillHandler.myObject.GetComponent<PlaceMonster>() != null) {
                        if (skillHandler.myObject.GetComponent<PlaceMonster>().isPlayer == isPlayer) {
                            SetTarget(summonedObject);
                            successCallback(null);
                            return;
                        }
                    }
                }
            }
            failedCallback("타겟을 찾을 수 없습니다");
            Logger.Log("타겟을 찾을 수 없습니다.");
        }

        /// <summary></summary>
        /// <param name="parms">생성된 유닛</param>
        public override void SetTarget(object target) {
            targets.Add((GameObject)target);
        }
    }
}
