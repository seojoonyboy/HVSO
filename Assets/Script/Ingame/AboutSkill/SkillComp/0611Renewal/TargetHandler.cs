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

        public bool isDone = false;

        public SkillHandler skillHandler;

        public enum TargetEnum {
            NONE,
            UNIT,
            FIELD,
            ALL
        }


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

        protected Transform GetDropAreaCharacter() {
            var highlightedSlot = GetComponent<CardHandler>().highlightedSlot;
            if (highlightedSlot.GetComponentInParent<PlaceMonster>() != null) {
                return GetDropAreaUnit();
            }
            else {
                return highlightedSlot.GetComponentInParent<PlayerController>().transform;
            }
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
        public override void SelectTarget(SelectTargetFinished successCallback, SelectTargetFailed failedCallback, Filtering filter) {
            base.SelectTarget(successCallback, failedCallback, filter);

            //직접 지정하는게 없음
            SetTarget(null);
            successCallback(null);
        }

        public override void SetTarget(object parms) {
            string place = args[0];
            var observer = PlayMangement.instance.UnitsObserver;
            bool isHuman = PlayMangement.instance.player.isHuman;
            isHuman = skillHandler.isPlayer ? isHuman : !isHuman;
            switch (place) {
                case "rear":
                    var pos = observer.GetMyPos(gameObject);
                    if(pos.row == 0) break;
                    var list = observer.GetAllFieldUnits(pos.col, isHuman);
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
            
            GameObject playerObject = skillHandler.isPlayer ? PlayMangement.instance.player.gameObject : PlayMangement.instance.enemyPlayer.gameObject;
            GameObject enemyPlayerObject = skillHandler.isPlayer ? PlayMangement.instance.enemyPlayer.gameObject: PlayMangement.instance.player.gameObject;

            switch (args[0]) {
                case "my":
                    SetTarget(playerObject);
                    successCallback(enemyPlayerObject);
                    break;
                case "enemy":
                    SetTarget(enemyPlayerObject);
                    successCallback(playerObject);
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
            var observer = PlayMangement.instance.UnitsObserver;
            bool isHuman = PlayMangement.instance.player.isHuman;
            isHuman = isPlayer ? isHuman : !isHuman;

            List<GameObject> result = new List<GameObject>();
            if (args.Length != 2) {
                Logger.LogError("Args가 잘못 전달되었습니다.");
                return result;
            }

            switch (args[0]) {
                case "my":
                    if (args[1].Contains("unit")) {
                        if (args[1].Contains("hero")) {
                            result.Add(GetDropAreaCharacter().gameObject);
                        }
                        else {
                            result.Add(GetDropAreaUnit().gameObject);
                        }
                    }

                    else if (args[1].Contains("line")) {
                        int col = GetDropLine();
                        var targets = observer.GetAllFieldUnits(col, isHuman);
                        foreach (GameObject target in targets) {
                            result.Add(target);
                        }
                    }

                    else if (args[1].Contains("all")) {
                        var targets = observer.GetAllFieldUnits(isHuman);
                        foreach (GameObject target in targets) {
                            result.Add(target);
                        }
                    }
                    break;
                case "enemy":
                    if (args[1].Contains("unit")) {
                        if (args[1].Contains("hero")) {
                            result.Add(GetDropAreaCharacter().gameObject);
                        }
                        else {
                            result.Add(GetDropAreaUnit().gameObject);
                        }
                    }
                    
                    else if (args[1].Contains("line")) {
                        int col = GetDropLine();
                        var targets = observer.GetAllFieldUnits(col, !isHuman);
                        foreach (GameObject target in targets) {
                            result.Add(target);
                        }
                    }

                    else if (args[1].Contains("all")) {
                        var targets = observer.GetAllFieldUnits(!isHuman);
                        foreach (GameObject target in targets) {
                            result.Add(target);
                        }
                    }
                    break;

                case "all":
                    if(args[1] == "unit") {
                        result.Add(GetDropAreaUnit().gameObject);
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
            var playManagement = PlayMangement.instance;

            var observer = playManagement.UnitsObserver;
            bool isHuman = playManagement.player.isHuman;

            List<GameObject> result = new List<GameObject>();
            if (args.Length != 2) {
                Logger.LogError("Args가 잘못 전달되었습니다.");
                return result;
            }

            switch (args[0]) {
                case "my":
                    if (args[1] == "all") {
                        var targets = observer.GetAllFieldUnits(isHuman);
                        foreach (GameObject target in targets) {
                            if (target != GetDropAreaUnit().gameObject) {
                                result.Add(target);
                            }
                        }
                    }
                    break;
                case "enemy":
                    if (args[1] == "all") {
                        var targets = observer.GetAllFieldUnits(!isHuman);
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

    public class pending_select : select { }

    public class select : TargetHandler {
        protected SelectTargetFinished callback;
        protected SelectTargetFailed failed;
        protected string currentState;

        protected void Start() {
            if(!skillHandler.isPlayer) this.enabled = false;
        }

        protected void Update() {
            if(failed != null && PlayMangement.instance.socketHandler.gameState.state.CompareTo(currentState) != 0) {
                    failed("Time Over");
                    removeSelectUI();
            }
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
                    if (args[1] == "place") {
                        int col = selectedTarget.parent.GetSiblingIndex();
                        int row = 0;

                        var observer = PlayMangement.instance.UnitsObserver;
                        selectedTarget = observer.GetSlot(new FieldUnitsObserver.Pos(col, row), true);
                    }

                    if(args[1] == "unit") {
                        PlaceMonster targetMonster = selectedTarget.gameObject.GetComponentInParent<PlaceMonster>();
                        if(!targetMonster.transform.Find("ClickableUI").gameObject.activeSelf) return;
                        selectedTarget = targetMonster.transform;
                    }

                    SetTarget(selectedTarget.gameObject);
                    callback(selectedTarget);
                    skillHandler.SendingMessage(true);
                    removeSelectUI();
                    TintUnit(false);
                }
            }
        }

        private void removeSelectUI() {
            PlayMangement.instance.OffBlockPanel();
            CardDropManager.Instance.HideDropableSlot();
            callback = null;
            failed = null;
            PlayMangement.instance.infoOn = false;
            PlayMangement.dragable = true;
            PlayMangement.instance.UnlockTurnOver();

            bool isHuman = PlayMangement.instance.player.isHuman;
            
            var units = PlayMangement.instance.UnitsObserver.GetAllFieldUnits(!isHuman);
            foreach (GameObject unit in units) {
                unit
                    .transform
                    .Find("ClickableUI")
                    .gameObject
                    .SetActive(false);

                unit.transform.Find("MagicTargetTrigger").gameObject.SetActive(false);
            }
            units = PlayMangement.instance.UnitsObserver.GetAllFieldUnits(isHuman);
            foreach (GameObject unit in units) {
                unit
                    .transform
                    .Find("ClickableUI")
                    .gameObject
                    .SetActive(false);

                unit.transform.Find("MagicTargetTrigger").gameObject.SetActive(false);
            }
        }

        protected IEnumerator enemyTurnSelect(SelectTargetFinished successCallback, SelectTargetFailed failedCallback) {
            BattleConnector server = PlayMangement.instance.socketHandler;
            TintUnit(true);
            //턴 넘김으로 인한 경우 (현재 오크 잠복꾼) 위치 이동만 존재합니다.
            if(skillHandler.targetData == null) {
                //1. 메시지 올 때까지 기다리기
                PlayMangement.instance.OnBlockPanel("상대가 위치를 지정중입니다.");
                var list = PlayMangement.instance.socketHandler.unitSkillList;
                yield return new WaitUntil(PlayMangement.instance.passOrc);
                yield return list.WaitNext();
                if(list.Count == 0) {
                    failedCallback("상대가 위치 지정에 실패했습니다.");
                    yield break;
                }
                int position = list.Dequeue();
                //2. 밝혀줘야할 select 부분 찾기
                FieldUnitsObserver.Pos movePos = new FieldUnitsObserver.Pos(position, 0);
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
                terrainSlot.GetComponent<SpriteRenderer>().color = new Color(1, 0, 0, 0.6f);

                //4.  1초뒤에 끄기
                yield return new WaitForSeconds(1f);
                terrainSlot.gameObject.SetActive(false);
                terrainSlot.GetChild(0).gameObject.SetActive(false);
                terrainSlot.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 0.6f);
                
                //5. 실제 effect 실행하러 보내기
                Transform selectedTarget = PlayMangement.instance.UnitsObserver
                    .GetSlot(movePos, !PlayMangement.instance.player.isHuman)
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
                    PlayMangement.instance.UnlockTurnOver();
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
                    terrainSlot.GetComponent<SpriteRenderer>().color = new Color(1, 0, 0, 0.6f);

                    //4.  1초뒤에 끄기
                    yield return new WaitForSeconds(1f);
                    terrainSlot.gameObject.SetActive(false);
                    terrainSlot.GetChild(0).gameObject.SetActive(false);
                    terrainSlot.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 0.6f);
                    
                    FieldUnitsObserver observer = PlayMangement.instance.UnitsObserver;
                        selectedTarget = observer
                            .GetSlot(new FieldUnitsObserver.Pos(line, 0), targetCampHuman)
                            .transform;
                break;
                case "unit" :
                    //검색
                    int targetItemId = int.Parse(played_card.targets[1].args[0]);
                    var camp = played_card.targets[1].args[1];
                    var list = PlayMangement.instance.UnitsObserver
                            .GetAllFieldUnits(camp.CompareTo("human") == 0);
                    //list.AddRange(PlayMangement.instance.EnemyUnitsObserver.GetAllFieldUnits());
                    GameObject target = list.Find(x => x.GetComponent<PlaceMonster>().itemId == targetItemId);
                    GameObject highlightUI = target.transform.Find("ClickableUI").gameObject;
                    //3. unitSlot에서 특정 부분 밝혀주기
                    highlightUI.SetActive(true);
                    if(highlightUI.GetComponent<SpriteRenderer>() != null)
                        highlightUI.GetComponent<SpriteRenderer>().color = new Color(1, 0, 0, 0.6f);
                    //4.  1.5초뒤에 끄기
                    yield return new WaitForSeconds(1.5f);
                    if(highlightUI.GetComponent<SpriteRenderer>() != null)
                        highlightUI.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 0.6f);
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
            PlayMangement.instance.UnlockTurnOver();
            TintUnit(false);
        }

        public override void SelectTarget(SelectTargetFinished successCallback, SelectTargetFailed failedCallback, Filtering filter) {
            base.SelectTarget(successCallback, failedCallback, filter);
            if(GetComponent<MagicDragHandler>()) {
                transform.localScale = Vector3.zero;
            }
            PlayMangement.instance.LockTurnOver();



            //TODO : 적일 경우 해당 소켓이 도달 할 때까지 기다리기 card_played, skill_activated
            if(!skillHandler.isPlayer) { 
                StartCoroutine(enemyTurnSelect(successCallback, failedCallback));
                return; 
            }
            
            foreach(string arg in args) {
                Logger.Log(arg);
            }
            
            failed = failedCallback;
            currentState = PlayMangement.instance.socketHandler.gameState.state;

            GameObject targetObject = skillHandler.myObject;
            if (targetObject != null && targetObject.GetComponent<PlaceMonster>() != null)
                EffectSystem.Instance.CheckEveryLineMask(targetObject.GetComponent<PlaceMonster>());


            switch (args[0]) {
                case "my":
                    if(args.Length == 2 && args[1] == "place") {
                        if (CanSelect(args[1])) {
                            PlayMangement.instance.OnBlockPanel("대상을 지정해 주세요.");
                            callback = successCallback;
                            PlaceMonster myMonster = skillHandler.myObject.GetComponent<PlaceMonster>();
                            EffectSystem.Instance.CheckEveryLineMask(myMonster);
                            string[] attributes; 
                            if(myMonster != null)
                                attributes = myMonster.unit.attributes;
                            else
                                attributes = GetDropAreaUnit().GetComponent<PlaceMonster>().unit.attributes;

                            if (ScenarioGameManagment.scenarioInstance != null && ScenarioGameManagment.scenarioInstance.forcedSummonAt != -1)
                                CardDropManager.Instance.ShowScopeSlot();
                            else
                                CardDropManager.Instance.ShowDropableSlot(attributes, true);
                            TintUnit(true);
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
                            var units = PlayMangement.instance.UnitsObserver
                                .GetAllFieldUnits(PlayMangement.instance.player.isHuman);
                            filter(ref units);
                            
                            PlaceMonster myUnit = skillHandler.myObject.GetComponent<PlaceMonster>(); //현재 사용한 스킬이 유닛인 경우
                            if(myUnit) units.RemoveAll(unit => unit.GetComponent<PlaceMonster>().itemId == myUnit.itemId); //자기 자신이 포함되어있다면 제외

                            foreach (GameObject unit in units) {
                                var ui = unit.transform.Find("ClickableUI").gameObject;
                                if (ui != null) {
                                    ui.SetActive(true);
                                    PlayMangement.instance.infoOn = true;
                                }
                                unit.transform.Find("MagicTargetTrigger").gameObject.SetActive(true);
                            }
                            TintUnit(true);
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
                            var units = PlayMangement.instance.UnitsObserver.GetAllFieldUnits(!PlayMangement.instance.player.isHuman);

                            //잠복중인 유닛은 타겟에서 제외
                            for (int i = 0; i < units.Count; i++) {
                                var placeMonster = units[i].GetComponent<PlaceMonster>();
                                if (placeMonster.GetComponent<ambush>() != null) {
                                    units.Remove(units[i]);
                                    Logger.Log("잠복 유닛 감지됨");
                                }
                            }

                            if(units.Count == 0) {
                                failedCallback("타겟이 없습니다.");
                                break;
                            }

                            foreach (GameObject unit in units) {
                                var ui = unit.transform.Find("ClickableUI").gameObject;
                                if (ScenarioGameManagment.scenarioInstance != null && ScenarioGameManagment.scenarioInstance.forcedTargetAt != -1) {
                                    if (unit.GetComponent<PlaceMonster>().x == ScenarioGameManagment.scenarioInstance.forcedTargetAt) {
                                        if (ui != null) {
                                            ui.SetActive(true);
                                            PlayMangement.instance.infoOn = true;
                                        }
                                        unit.transform.Find("MagicTargetTrigger").gameObject.SetActive(true);
                                    }
                                }
                                else {
                                    if (ui != null) {
                                        ui.SetActive(true);
                                        PlayMangement.instance.infoOn = true;
                                    }
                                    unit.transform.Find("MagicTargetTrigger").gameObject.SetActive(true);
                                }
                            }

                            callback = successCallback;
                            TintUnit(true);
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

        protected bool CanSelect(string arg) {
            bool result = false;
            var observer = PlayMangement.instance.UnitsObserver;
            bool isHuman = PlayMangement.instance.player.isHuman;

            Transform slotParent = null;
            GameObject[,] slots = null;
            if (isHuman) {
                slots = observer.humanUnits;
                slotParent = PlayMangement.instance.player.transform;
            }
            else {
                slots = observer.orcUnits;
                slotParent = PlayMangement.instance.enemyPlayer.transform;
            }

            switch (arg) {
                case "place":
                    for (int i=0; i<5; i++) {
                        //빈 공간인 경우
                        if(slots[i, 0] == null) {
                            var placeMonster = skillHandler.myObject.GetComponent<PlaceMonster>();
                            //유닛카드인 경우
                            if (placeMonster != null) {
                                //숲 지형인 경우
                                //if (slotParent.transform.GetChild(0).GetChild(i).GetComponent<Terrain>().terrain == PlayMangement.LineState.forest) {
                                //    //유닛이 숲 지형에 갈 수 있는 경우
                                //    if (placeMonster.unit.attributes.ToList().Contains("footslog")) {
                                //        result = true;
                                //    }
                                //}
                                ////숲 지형이 아닌 경우
                                //else {
                                //    result = true;
                                //}
                                result = true;
                            }
                            //마법카드인 경우
                            else {
                                var skillTarget = ((List<GameObject>)skillHandler.skillTarget)[0];
                                placeMonster = skillTarget.GetComponent<PlaceMonster>();
                                //if (PlayMangement.instance.player.transform.GetChild(0).GetChild(i).GetComponent<Terrain>().terrain == PlayMangement.LineState.forest) {
                                //    //유닛이 숲 지형에 갈 수 있는 경우
                                //    if (placeMonster.unit.attributes.ToList().Contains("footslog")) {
                                //        result = true;
                                //    }
                                //}
                                ////숲 지형이 아닌 경우
                                //else {
                                //    result = true;
                                //}
                                result = true;
                            }
                        }
                    }
                    break;
                case "unit":
                    if(args[0] == "enemy") {
                        observer = PlayMangement.instance.UnitsObserver;
                    }

                    var units = observer.GetAllFieldUnits(isHuman);

                    //잠복중인 유닛은 타겟에서 제외
                    for(int i=0; i<units.Count; i++) {
                        var placeMonster = units[i].GetComponent<PlaceMonster>();
                        if (placeMonster.GetComponent<ambush>() != null) {
                            units.Remove(units[i]);
                        }
                    }

                    if (units.Count != 0) {
                        Logger.Log(observer.GetAllFieldUnits(isHuman).Count + "개의 적이 감지됨");
                        result = true;
                    }
                    break;
            }
            return result;
        }

        private void TintUnit(bool onOff) {
            PlaceMonster caster;
            caster = skillHandler.myObject.GetComponent<PlaceMonster>();
            if(caster == null) {
                object skillTarget = skillHandler.skillTarget;
                if(skillTarget.GetType() ==  typeof(GameObject))
                    caster = ((GameObject)skillTarget).GetComponent<PlaceMonster>();
                else if(skillTarget.GetType() == typeof(List<GameObject>))
                    caster = ((List<GameObject>)skillTarget)[0].GetComponent<PlaceMonster>();
            }
            caster.TintAnimation(onOff);
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
                    var observer = PlayMangement.instance.UnitsObserver;
                    targets = observer.GetAllFieldUnits(col, !PlayMangement.instance.player.isHuman);

                    SetTarget(targets);
                    successCallback(targets);
                }
                else failedCallback(null);
            }

            else if(args[0] == "player") {
                if(args[1] == "unit") {
                    var observer = PlayMangement.instance.UnitsObserver;
                    targets = observer.GetAllFieldUnits(col, PlayMangement.instance.player.isHuman);

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
    /// <summary>
    /// 빈 지역 가져오기인데, 서버에서 유닛을 배치해줘서 어떻게 가져와야할지 가늠이 잘 안오는중
    /// </summary>
    public class field : TargetHandler {
        public override void SelectTarget(SelectTargetFinished successCallback, SelectTargetFailed failedCallback, Filtering filter) {
            base.SelectTarget(successCallback, failedCallback, filter);

            FieldUnitsObserver observer = PlayMangement.instance.UnitsObserver;
            bool isHuman = PlayMangement.instance.player.isHuman;
            isHuman = skillHandler.isPlayer && isHuman;
            isHuman = (args[0].CompareTo("my") == 0) == isHuman;

            if(args[1].CompareTo("all") == 0) {
                for(int i = 0; i < 5; i++) {
                    List<GameObject> list = observer.GetAllFieldUnits(i, isHuman);
                    if(list.Count == 0) {
                        Transform unitPos = observer.GetSlot(new FieldUnitsObserver.Pos(i, 0), skillHandler.isPlayer);
                        SetTarget(unitPos.gameObject);
                    }
                }
            }
            else {
                Logger.LogError("only made all, " + args[1] + "need to be code");
            }
            successCallback(GetTarget());
        }

        public override void SetTarget(object target) {
            targets.Add((GameObject)target);
        }
    }
}
