using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using SkillModules;

namespace TargetModules {
    public class TargetHandler : MonoBehaviour {
        public delegate void SelectTargetFinished(object parms);
        public delegate void SelectTargetFailed(string msg);
        public delegate void Filtering(ref List<GameObject> list);

        public string[] args;

        public List<GameObject> targets;
        protected FieldUnitsObserver playerUnitsObserver, enemyUnitsObserver;

        public bool isDone = false;

        public SkillHandler skillHandler;
        /// <summary>
        /// 타겟을 지정하는 단계를 시작
        /// </summary>
        public virtual void SelectTarget(SelectTargetFinished successCallback, SelectTargetFailed failedCallback, Filtering filter) {
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
            layer = "MagicTarget";

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
        public override void SelectTarget(SelectTargetFinished successCallback, SelectTargetFailed failedCallback, Filtering filter) {
            base.SelectTarget(successCallback, failedCallback, filter);

            var target = skillHandler.skillTarget;
            if (target == null) {
                failedCallback("타겟을 찾을 수 없습니다.");
                return;
            }
            SetTarget(target);
            successCallback(target);
        }

        public override void SetTarget(object target) {
            if(target.GetType() == typeof(List<GameObject>)) {
                targets.AddRange((List<GameObject>)target);
            }
            else targets.Add((GameObject)target);
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

        public override void SelectTarget(SelectTargetFinished successCallback, SelectTargetFailed failedCallback, Filtering filter) {
            base.SelectTarget(successCallback, failedCallback, filter);

            //직접 지정하는게 없음
            SetTarget(null);
            successCallback(null);
        }

        public override void SetTarget(object parms) {
            string place = args[0];
            switch (place) {
                case "rear":
                    var pos = playerUnitsObserver.GetMyPos(gameObject);
                    if(pos.row == 1) break;
                    var list = playerUnitsObserver.GetAllFieldUnits(pos.col);
                    list.Remove(gameObject);
                    targets.AddRange(list);
                    break;
            }
        }
    }

    public class attack_target : TargetHandler {
        public override void SelectTarget(SelectTargetFinished successCallback, SelectTargetFailed failedCallback, Filtering filter) {
            base.SelectTarget(successCallback, failedCallback, filter);

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
        public override void SelectTarget(SelectTargetFinished successCallback, SelectTargetFailed failedCallback, Filtering filter) {
            base.SelectTarget(successCallback, failedCallback, filter);

            SetTarget(gameObject);
            successCallback(gameObject);
        }

        public override void SetTarget(object parms) {
            targets.Add(gameObject);
        }
    }

    public class hero : TargetHandler {
        public override void SelectTarget(SelectTargetFinished successCallback, SelectTargetFailed failedCallback, Filtering filter) {
            base.SelectTarget(successCallback, failedCallback, filter);

            switch (args[0]) {
                case "my":
                    SetTarget(PlayMangement.instance.player.gameObject);
                    successCallback(PlayMangement.instance.player.gameObject);
                    break;
                case "enemy":
                    SetTarget(PlayMangement.instance.enemyPlayer.gameObject);
                    successCallback(PlayMangement.instance.player.gameObject);
                    break;
                default:
                    failedCallback(null);
                    break;
            }
        }

        public override void SetTarget(object parms) {
            targets.Add((GameObject)parms);
        }
    }
    
    public class played_target : TargetHandler {
        public override void SelectTarget(SelectTargetFinished successCallback, SelectTargetFailed failedCallback, Filtering filter) {
            base.SelectTarget(successCallback, failedCallback, filter);
            
            if (args == null) failedCallback("Args 가 존재가지 않습니다.");

            var targets = GetTarget(skillHandler.isPlayer, args);

            filter(ref targets);

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

    public class exclusive_played_target : TargetHandler {
        public override void SelectTarget(SelectTargetFinished successCallback, SelectTargetFailed failedCallback, Filtering filter) {
            base.SelectTarget(successCallback, failedCallback, filter);


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
                    if (args[1] == "all") {
                        var targets = playerObserver.GetAllFieldUnits();
                        foreach (GameObject target in targets) {
                            if (target != GetDropAreaUnit().gameObject) {
                                result.Add(target);
                            }
                        }
                    }
                    break;
                case "enemy":
                    if (args[1] == "all") {
                        var targets = enemyObserver.GetAllFieldUnits();
                        foreach (GameObject target in targets) {
                            if(target != GetDropAreaUnit().gameObject) {
                                result.Add(target);
                            }
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
                PlayMangement.dragable = false;

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
                    PlayMangement.dragable = true;

                    var units = PlayMangement.instance.EnemyUnitsObserver.GetAllFieldUnits();
                    foreach (GameObject unit in units) {
                        unit
                            .transform
                            .Find("ClickableUI")
                            .gameObject
                            .SetActive(false);

                        unit.transform.Find("MagicTargetTrigger").gameObject.SetActive(false);
                    }
                    units = PlayMangement.instance.PlayerUnitsObserver.GetAllFieldUnits();
                    foreach (GameObject unit in units) {
                        unit
                            .transform
                            .Find("ClickableUI")
                            .gameObject
                            .SetActive(false);

                        unit.transform.Find("MagicTargetTrigger").gameObject.SetActive(false);
                    }
                }
            }
        }

        private IEnumerator enemyTurnSelect(SelectTargetFinished successCallback, SelectTargetFailed failedCallback) {
            BattleConnector server = PlayMangement.instance.socketHandler;
            //턴 넘김으로 인한 경우 (현재 오크 잠복꾼) 위치 이동만 존재합니다.
            if(skillHandler.targetData == null) {
                //1. 메시지 올 때까지 기다리기
                PlayMangement.instance.OnBlockPanel("상대가 위치를 지정중입니다.");
                var list = PlayMangement.instance.socketHandler.unitSkillList;
                yield return list.WaitNext();
                if(list.Count == 0) {
                    failedCallback("상대가 위치 지정에 실패했습니다.");
                    yield break;
                }
                int itemId = list.Dequeue();
                var monList = server.gameState.map.allMonster;
                SocketFormat.Unit unit = monList.Find(x => x.itemId == itemId);
                //2. 밝혀줘야할 select 부분 찾기
                Pos movePos = unit.pos;
                Terrain[] terrains = GameObject.Find("BackGround").GetComponentsInChildren<Terrain>();
                Transform terrainSlot = null;
                
                foreach(Terrain x in terrains) {
                    if(movePos.col == x.transform.GetSiblingIndex()) {
                        terrainSlot = x.transform.Find("EnemyBackSlot");
                        break;
                    }
                }

                //3. unitSlot에서 특정 부분 밝혀주기 아마 baseSlot 맞겠지
                terrainSlot.gameObject.SetActive(true);
                terrainSlot.GetChild(0).gameObject.SetActive(true);
                terrainSlot.GetComponent<SpriteRenderer>().color = new Color(1, 0, 0, 155.0f / 255.0f);

                //4.  1.5초뒤에 끄기
                yield return new WaitForSeconds(1.5f);
                terrainSlot.gameObject.SetActive(false);
                terrainSlot.GetChild(0).gameObject.SetActive(false);
                terrainSlot.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 155.0f / 255.0f);
                
                //5. 실제 effect 실행하러 보내기
                Transform selectedTarget = PlayMangement.instance
                            .EnemyUnitsObserver
                            .transform
                            .GetChild(movePos.row)
                            .GetChild(movePos.col)
                            .transform;
                SetTarget(selectedTarget.gameObject);
                successCallback(selectedTarget);
            }
            //타겟이 있는 경우, 카드 사용으로 경우에 따라 나눠야함
            else {
                Transform selectedTarget = null;
                //1. 사용한 카드 찾기
                int itemId = skillHandler.myObject.GetComponent<PlaceMonster>() != null ? 
                    skillHandler.myObject.GetComponent<PlaceMonster>().itemId : 
                    skillHandler.myObject.GetComponent<MagicDragHandler>().itemID;
                var play_list = server.gameState.playHistory.ToList();
                SocketFormat.PlayHistory played_card = play_list.Find(x => x.cardItem.itemId == itemId);
                
                //왠만하면은 2번째 targets가 select인 경우들인것 같다. (유닛 소환 직후 또는 마법카드 사용)
                if(played_card.targets.Length != 2) {
                    failedCallback("상대가 위치 지정에 실패했습니다.");
                    yield break;
                }
                //2. 카드의 정보 확인 하기
                switch(played_card.targets[1].method) {
                case "place" :
                    int line = int.Parse(played_card.targets[1].args[0]);
                    bool targetCampHuman = played_card.targets[1].args[1].CompareTo("human")==0;
                    //지정된 타겟이 아군인지 적군인지 판단용
                    bool isTargetPlayer = PlayMangement.instance.player.isHuman == targetCampHuman;
                    Terrain[] terrains = GameObject.Find("BackGround").GetComponentsInChildren<Terrain>();
                    Transform terrainSlot = null;
                
                    foreach(Terrain x in terrains) {
                        if(line == x.transform.GetSiblingIndex()) {
                            if(isTargetPlayer) terrainSlot = x.transform.GetChild(0);
                            else terrainSlot = x.transform.Find("EnemyBackSlot");
                            break;
                        }
                    }

                    //3. unitSlot에서 특정 부분 밝혀주기 아마 baseSlot 맞겠지
                    terrainSlot.gameObject.SetActive(true);
                    terrainSlot.GetChild(0).gameObject.SetActive(true);
                    terrainSlot.GetComponent<SpriteRenderer>().color = new Color(1, 0, 0, 155.0f / 255.0f);

                    //4.  1.5초뒤에 끄기
                    yield return new WaitForSeconds(1.5f);
                    terrainSlot.gameObject.SetActive(false);
                    terrainSlot.GetChild(0).gameObject.SetActive(false);
                    terrainSlot.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 155.0f / 255.0f);

                    FieldUnitsObserver observer = isTargetPlayer ? PlayMangement.instance.PlayerUnitsObserver : PlayMangement.instance.EnemyUnitsObserver;
                    selectedTarget = observer
                            .transform
                            .GetChild(0) //TODO : 앞뒤 구분 해야함 
                            .GetChild(line)
                            .transform;
                break;
                case "unit" :
                    //검색
                    int targetItemId = int.Parse(played_card.targets[1].args[0]);
                    var list = PlayMangement.instance.PlayerUnitsObserver.GetAllFieldUnits();
                    list.AddRange(PlayMangement.instance.EnemyUnitsObserver.GetAllFieldUnits());
                    GameObject target = list.Find(x => x.GetComponent<PlaceMonster>().itemId == targetItemId);
                    GameObject highlightUI = target.transform.Find("ClickableUI").gameObject;
                    //3. unitSlot에서 특정 부분 밝혀주기
                    highlightUI.SetActive(true);
                    highlightUI.GetComponent<SpriteRenderer>().color = new Color(1, 0, 0, 155.0f / 255.0f);
                    //4.  1.5초뒤에 끄기
                    yield return new WaitForSeconds(1.5f);
                    highlightUI.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 155.0f / 255.0f);
                    highlightUI.SetActive(false);
                    selectedTarget = target.transform;
                break;
                default :
                failedCallback("전혀 다른 select가 나왔습니다.");
                yield break;
                }
                if(selectedTarget == null) {
                    failedCallback("selectedTarget이 누락 됐습니다.");
                    yield break;
                }
                SetTarget(selectedTarget.gameObject);
                successCallback(selectedTarget);
            }
            
            PlayMangement.instance.OffBlockPanel();
        }

        public override void SelectTarget(SelectTargetFinished successCallback, SelectTargetFailed failedCallback, Filtering filter) {
            base.SelectTarget(successCallback, failedCallback, filter);
            //TODO : 적일 경우 해당 소켓이 도달 할 때까지 기다리기 card_played, skill_activated
            if(!skillHandler.isPlayer) { 
                StartCoroutine(enemyTurnSelect(successCallback, failedCallback));
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
                            filter(ref units);
                            foreach (GameObject unit in units) {
                                var ui = unit.transform.Find("ClickableUI").gameObject;
                                if (ui != null) {
                                    ui.SetActive(true);
                                    PlayMangement.instance.infoOn = true;
                                }
                                unit.transform.Find("MagicTargetTrigger").gameObject.SetActive(true);
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
                            for (int i = 0; i < units.Count; i++) {
                                var placeMonster = units[i].GetComponent<PlaceMonster>();
                                if (placeMonster.GetComponent<ambush>() != null) {
                                    units.Remove(units[i]);
                                    Logger.Log("잠복 유닛 감지됨");
                                }
                            }

                            foreach (GameObject unit in units) {
                                var ui = unit.transform.Find("ClickableUI").gameObject;
                                if (ui != null) {
                                    ui.SetActive(true);
                                    PlayMangement.instance.infoOn = true;
                                }
                                unit.transform.Find("MagicTargetTrigger").gameObject.SetActive(true);
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
                            //유닛카드인 경우
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
                            //마법카드인 경우
                            else {
                                var skillTarget = (GameObject)skillHandler.skillTarget;
                                placeMonster = skillTarget.GetComponent<PlaceMonster>();
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

                    var units = observer.GetAllFieldUnits();

                    //잠복중인 유닛은 타겟에서 제외
                    for(int i=0; i<units.Count; i++) {
                        var placeMonster = units[i].GetComponent<PlaceMonster>();
                        if (placeMonster.GetComponent<ambush>() != null) {
                            units.Remove(units[i]);
                        }
                    }

                    if (units.Count != 0) {
                        Logger.Log(observer.GetAllFieldUnits().Count + "개의 적이 감지됨");
                        result = true;
                    }
                    break;
            }
            return result;
        }
    }

    public class same_line : TargetHandler {
        public override void SelectTarget(SelectTargetFinished successCallback, SelectTargetFailed failedCallback, Filtering filter) {
            base.SelectTarget(successCallback, failedCallback, filter);

            var targets = new List<GameObject>();
            int col = skillHandler.myObject.GetComponent<PlaceMonster>().x;

            if (args[0] == "enemy") {
                if (args[1] == "unit") {
                    //같은 라인에 있는 적 유닛들이 Target임.
                    var observer = PlayMangement.instance.EnemyUnitsObserver;
                    targets = observer.GetAllFieldUnits(col);

                    SetTarget(targets);
                    successCallback(targets);
                }
                else failedCallback(null);
            }

            else if(args[0] == "player") {
                if(args[1] == "unit") {
                    var observer = PlayMangement.instance.PlayerUnitsObserver;
                    targets = observer.GetAllFieldUnits(col);

                    SetTarget(targets);
                    successCallback(targets);
                }
                else failedCallback(null);
            }
        }

        public override void SetTarget(object target) {
            targets.AddRange((List<GameObject>)(target));
        }
    }

    public class played : TargetHandler {
        public override void SelectTarget(SelectTargetFinished successCallback, SelectTargetFailed failedCallback, Filtering filter) {
            base.SelectTarget(successCallback, failedCallback, filter);

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
